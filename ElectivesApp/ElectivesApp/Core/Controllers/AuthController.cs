using BCrypt.Net;
using ElectivesApp.DAO;
using ElectivesApp.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ElectivesApp.Core.Controllers;

public class AuthController : BaseController
{
    private readonly IUserDao _userDao;
    private readonly SessionStore _sessions;
    private readonly AppConfig _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserDao userDao, SessionStore sessions,
        AppConfig config, ILoggerFactory loggerFactory)
        : base()
    {
        _userDao = userDao;
        _sessions = sessions;
        _config = config;
        _logger = loggerFactory.CreateLogger<AuthController>();
    }

    public async Task GetLogin(HttpContext ctx)
    {
        await View(ctx, "Auth/Login.html", new Dictionary<string, object?>
        {
            ["error"] = "",
            ["title"] = "Вхід"
        });
    }

    public async Task PostLogin(HttpContext ctx)
    {
        var form = await ReadFormAsync(ctx.Request);

        // Додаємо .Trim(), щоб прибрати зайві пробіли або символи перенесення рядка
        var username = form.GetValueOrDefault("username", "").Trim();
        var password = form.GetValueOrDefault("password", "").Trim();
        _logger.LogInformation("Спроба входу: {Username}", username);
        _logger.LogInformation($"[DEBUG] ГЕНЕРУЮ НОВИЙ ХЕШ ДЛЯ '12345': {BCrypt.Net.BCrypt.HashPassword("12345")}");
        var user = _userDao.GetByUsername(username);

        if (user != null)
        {
            // Перевірка пароля
            bool isOk = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            // Виведемо довжину пароля, щоб перевірити чи немає прихованих символів
            Console.WriteLine($"[DEBUG] Введений пароль: '{password}' (довжина: {password.Length})");
            Console.WriteLine($"[DEBUG] Результат BCrypt.Verify: {isOk}");

            if (isOk)
            {
                var session = _sessions.Create(user.Id, user.Role, user.Username);
                SessionHelper.SetSessionCookie(ctx.Response, session, _config.SessionCookieName);
                _logger.LogInformation("Користувач {Username} увійшов успішно", user.Username);
                Redirect(ctx, user.Role == "teacher" ? "/teacher/courses" : "/student/courses");
                return;
            }
        }

        // Якщо ми тут — значить вхід не вдався
        _logger.LogWarning("Невдалий вхід для: {Username}", username);
        await View(ctx, "Auth/Login.html", new Dictionary<string, object?>
        {
            ["error"] = "Невірний логін або пароль",
            ["title"] = "Вхід"
        });
    }

    public Task GetLogout(HttpContext ctx)
    {
        if (ctx.Session != null)
        {
            _sessions.Remove(ctx.Session.Id);
            _logger.LogInformation("User {Username} logged out", ctx.Session.Username);
        }
        SessionHelper.ClearSessionCookie(ctx.Response, _config.SessionCookieName);
        Redirect(ctx, "/login");
        return Task.CompletedTask;
    }

    public async Task GetRegister(HttpContext ctx)
    {
        await View(ctx, "Auth/Register.html", new Dictionary<string, object?>
        {
            ["error"] = "",
            ["title"] = "Реєстрація"
        });
    }

    public async Task PostRegister(HttpContext ctx)
    {
        var form = await ReadFormAsync(ctx.Request);
        var username = form.GetValueOrDefault("username", "").Trim();
        var password = form.GetValueOrDefault("password", "").Trim();
        var fullName = form.GetValueOrDefault("fullName", "").Trim();
        var email = form.GetValueOrDefault("email", "").Trim();
        var role = form.GetValueOrDefault("role", "student");

        // Перевірка, чи існує користувач
        if (_userDao.GetByUsername(username) != null)
        {
            await View(ctx, "Auth/Register.html", new Dictionary<string, object?>
            {
                ["error"] = "Цей логін вже зайнятий",
                ["title"] = "Реєстрація"
            });
            return;
        }

        // Хешування пароля
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Створення об'єкта (використовуємо моделі Student або Teacher)
        ElectivesApp.Models.User newUser = role == "teacher"
            ? new ElectivesApp.Models.Teacher()
            : new ElectivesApp.Models.Student();

        newUser.Username = username;
        newUser.PasswordHash = passwordHash;
        newUser.FullName = fullName;
        newUser.Email = email;
        newUser.Role = role;

        _userDao.Create(newUser);

        _logger.LogInformation("Новий користувач зареєстрований: {Username}", username);

        // Перенаправлення на вхід після успішної реєстрації
        Redirect(ctx, "/login");
    }
}