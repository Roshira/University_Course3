using System.Net;
using ElectivesApp.Core.Controllers;
using ElectivesApp.DAO;
using ElectivesApp.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ElectivesApp.Core;

// ── Front Controller ──────────────────────────────────────────────────────────
// Single entry point for all HTTP requests. Resolves the route, checks auth,
// and dispatches to the appropriate controller action.
public class FrontController
{
    private readonly AppConfig _config;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<FrontController> _logger;
    private readonly Router _router;
    private readonly SessionStore _sessionStore;

    // DAOs
    private readonly IUserDao _userDao;
    private readonly ICourseDao _courseDao;
    private readonly IEnrollmentDao _enrollmentDao;

    // Controllers
    private readonly AuthController _authController;
    private readonly StudentController _studentController;
    private readonly TeacherController _teacherController;

    public FrontController(AppConfig config, ILoggerFactory loggerFactory)
    {
        _config = config;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<FrontController>();
        _router = new Router();
        _sessionStore = new SessionStore(config.SessionTimeoutMinutes);

        // Wiring dependencies manually (no DI container — keeps it explicit)
        var dbFactory = new PostgresConnectionFactory(config.ConnectionString);
        _userDao = new UserDao(dbFactory);
        _courseDao = new CourseDao(dbFactory);
        _enrollmentDao = new EnrollmentDao(dbFactory);

        _authController = new AuthController(_userDao, _sessionStore, config, loggerFactory);
        _studentController = new StudentController(_courseDao, _enrollmentDao, loggerFactory);
        _teacherController = new TeacherController(_courseDao, _enrollmentDao, _userDao, loggerFactory);

        RegisterRoutes();
    }

    // ── Route registration ────────────────────────────────────────────────────
    private void RegisterRoutes()
    {
        // Auth (no auth required)
        _router.Get("/login", _authController.GetLogin, requiresAuth: false);
        _router.Post("/login", _authController.PostLogin, requiresAuth: false);
        _router.Get("/logout", _authController.GetLogout, requiresAuth: true);

        _router.Get("/register", _authController.GetRegister, requiresAuth: false);
        _router.Post("/register", _authController.PostRegister, requiresAuth: false);

        // Student routes
        _router.Get("/student/courses", _studentController.GetCourses, role: "student");
        _router.Post("/student/enroll", _studentController.PostEnroll, role: "student");
        _router.Get("/student/my-courses", _studentController.GetMyCourses, role: "student");
        _router.Post("/student/unenroll", _studentController.PostUnenroll, role: "student");

        // Teacher routes
        _router.Get("/teacher/courses", _teacherController.GetCourses, role: "teacher");
        _router.Get("/teacher/courses/new", _teacherController.GetCreateCourse, role: "teacher");
        _router.Post("/teacher/courses/new", _teacherController.PostCreateCourse, role: "teacher");
        _router.Get("/teacher/courses/{id}/edit", _teacherController.GetEditCourse, role: "teacher");
        _router.Post("/teacher/courses/{id}/edit", _teacherController.PostEditCourse, role: "teacher");
        _router.Post("/teacher/courses/{id}/delete", _teacherController.PostDeleteCourse, role: "teacher");
        _router.Get("/teacher/courses/{id}/students", _teacherController.GetStudents, role: "teacher");
        _router.Post("/teacher/grade", _teacherController.PostGrade, role: "teacher");

        // Root redirect
        _router.Get("/", async ctx => Redirect(ctx, "/login"), requiresAuth: false);
    }

    // ── HTTP listener loop ────────────────────────────────────────────────────
    public async Task StartAsync()
    {
        var prefix = $"http://{_config.Host}:{_config.Port}/";
        var listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();
        _logger.LogInformation("Server listening on {Prefix}", prefix);

        // Background purge of expired sessions
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(5));
                _sessionStore.PurgeExpired();
            }
        });

        while (true)
        {
            var listenerCtx = await listener.GetContextAsync();
            // Handle each request on its own task (non-blocking)
            _ = Task.Run(() => HandleRequestAsync(listenerCtx));
        }
    }

    // ── Request dispatching ───────────────────────────────────────────────────
    private async Task HandleRequestAsync(HttpListenerContext listenerCtx)
    {
        var req = listenerCtx.Request;
        var res = listenerCtx.Response;
        var path = req.Url?.AbsolutePath ?? "/";
        var method = req.HttpMethod;

        _logger.LogInformation("{Method} {Path}", method, path);

        // Serve static files (css / js)
        if (path.StartsWith("/static/"))
        {
            await ServeStaticAsync(path, res);
            return;
        }

        var ctx = new HttpContext(req, res);

        try
        {
            var (route, routeParams) = _router.Match(method, path);

            if (route == null)
            {
                await Send404Async(res);
                return;
            }

            // Populate route params
            foreach (var (k, v) in routeParams)
                ctx.RouteParams[k] = v;

            // Auth check
            if (route.RequiresAuth)
            {
                var session = SessionHelper.GetSession(req, _sessionStore, _config.SessionCookieName);
                if (session == null)
                {
                    _logger.LogWarning("Unauthenticated access to {Path} — redirecting to /login", path);
                    Redirect(ctx, "/login");
                    return;
                }

                // Role check
                if (route.RequiredRole != null &&
                    !string.Equals(session.Role, route.RequiredRole, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Role mismatch for {Username}: expected {Role}", session.Username, route.RequiredRole);
                    await Send403Async(res);
                    return;
                }

                ctx.Session = session;
            }

            await route.Handler(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", method, path);
            await Send500Async(res, ex.Message);
        }
    }

    // ── Static file serving ───────────────────────────────────────────────────
    private static async Task ServeStaticAsync(string urlPath, HttpListenerResponse res)
    {
        var filePath = Path.Combine("wwwroot", urlPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(filePath))
        {
            res.StatusCode = 404;
            res.OutputStream.Close();
            return;
        }

        var ext = Path.GetExtension(filePath).ToLower();
        res.ContentType = ext switch
        {
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };

        var bytes = await File.ReadAllBytesAsync(filePath);
        res.ContentLength64 = bytes.Length;
        await res.OutputStream.WriteAsync(bytes);
        res.OutputStream.Close();
    }

    // ── Error responses ───────────────────────────────────────────────────────
    private static void Redirect(HttpContext ctx, string url)
    {
        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = url;
        ctx.Response.OutputStream.Close();
    }

    private static async Task Send404Async(HttpListenerResponse res)
        => await SendErrorAsync(res, 404, "404 — Сторінку не знайдено");

    private static async Task Send403Async(HttpListenerResponse res)
        => await SendErrorAsync(res, 403, "403 — Доступ заборонено");

    private static async Task Send500Async(HttpListenerResponse res, string detail)
        => await SendErrorAsync(res, 500, $"500 — Внутрішня помилка сервера: {System.Net.WebUtility.HtmlEncode(detail)}");

    private static async Task SendErrorAsync(HttpListenerResponse res, int code, string msg)
    {
        res.StatusCode = code;
        res.ContentType = "text/html; charset=utf-8";
        var html = System.Text.Encoding.UTF8.GetBytes(
            $"<!DOCTYPE html><html><body style='font-family:sans-serif;padding:2rem'>" +
            $"<h1>{msg}</h1><a href='/'>← На головну</a></body></html>");
        res.ContentLength64 = html.Length;
        await res.OutputStream.WriteAsync(html);
        res.OutputStream.Close();
    }
}