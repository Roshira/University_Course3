using ElectivesApp.DAO;
using ElectivesApp.Models;
using Microsoft.Extensions.Logging;

namespace ElectivesApp.Core.Controllers;

public class TeacherController : BaseController
{
    private readonly ICourseDao _courseDao;
    private readonly IEnrollmentDao _enrollmentDao;
    private readonly IUserDao _userDao;
    private readonly ILogger<TeacherController> _logger;

    public TeacherController(ICourseDao courseDao, IEnrollmentDao enrollmentDao,
        IUserDao userDao, ILoggerFactory loggerFactory)
    {
        _courseDao = courseDao;
        _enrollmentDao = enrollmentDao;
        _userDao = userDao;
        _logger = loggerFactory.CreateLogger<TeacherController>();
    }

    // GET /teacher/courses
    public async Task GetCourses(HttpContext ctx)
    {
        var teacherId = ctx.Session!.UserId;
        var courses = _courseDao.GetByTeacherId(teacherId).ToList();

        var rows = courses.Select(c => new Dictionary<string, object?>
        {
            ["Id"] = c.Id.ToString(),
            ["Title"] = c.Title,
            ["Description"] = c.Description,
            ["MaxStudents"] = c.MaxStudents.ToString(),
            ["IsActive"] = c.IsActive ? "Активний" : "Неактивний",
            ["IsActiveBool"] = c.IsActive ? "true" : "",
            ["CreatedAt"] = c.CreatedAt.ToString("dd.MM.yyyy")
        }).Cast<object?>().ToList();

        await View(ctx, "Teacher/Courses.html", new Dictionary<string, object?>
        {
            ["title"] = "Мої курси",
            ["username"] = ctx.Session.Username,
            ["courses"] = rows
        });
    }

    // GET /teacher/courses/new
    public async Task GetCreateCourse(HttpContext ctx)
    {
        await View(ctx, "Teacher/CourseForm.html", new Dictionary<string, object?>
        {
            ["title"] = "Новий курс",
            ["username"] = ctx.Session!.Username,
            ["action"] = "/teacher/courses/new",
            ["btnLabel"] = "Створити",
            ["Id"] = "",
            ["Title"] = "",
            ["Description"] = "",
            ["MaxStudents"] = "30",
            ["IsActive"] = "true",
            ["error"] = ""
        });
    }

    // POST /teacher/courses/new
    public async Task PostCreateCourse(HttpContext ctx)
    {
        var form = await ReadFormAsync(ctx.Request);
        var (course, err) = ParseCourseForm(form, ctx.Session!.UserId);

        if (err != null)
        {
            await View(ctx, "Teacher/CourseForm.html", FormModel(ctx, "/teacher/courses/new", "Створити", form, err));
            return;
        }

        var id = _courseDao.Create(course!);
        _logger.LogInformation("Teacher {Id} created course {CourseId}: {Title}",
            ctx.Session.UserId, id, course!.Title);
        Redirect(ctx, "/teacher/courses");
    }

    // GET /teacher/courses/{id}/edit
    public async Task GetEditCourse(HttpContext ctx)
    {
        var course = _courseDao.GetById(ctx.GetRouteInt("id"));
        if (course == null || course.TeacherId != ctx.Session!.UserId)
        {
            Redirect(ctx, "/teacher/courses");
            return;
        }

        await View(ctx, "Teacher/CourseForm.html", new Dictionary<string, object?>
        {
            ["title"] = "Редагування курсу",
            ["username"] = ctx.Session.Username,
            ["action"] = $"/teacher/courses/{course.Id}/edit",
            ["btnLabel"] = "Зберегти",
            ["Id"] = course.Id.ToString(),
            ["Title"] = course.Title,
            ["Description"] = course.Description,
            ["MaxStudents"] = course.MaxStudents.ToString(),
            ["IsActive"] = course.IsActive ? "true" : "",
            ["error"] = ""
        });
    }

    // POST /teacher/courses/{id}/edit
    public async Task PostEditCourse(HttpContext ctx)
    {
        var id = ctx.GetRouteInt("id");
        var existing = _courseDao.GetById(id);
        if (existing == null || existing.TeacherId != ctx.Session!.UserId)
        {
            Redirect(ctx, "/teacher/courses");
            return;
        }

        var form = await ReadFormAsync(ctx.Request);
        var (course, err) = ParseCourseForm(form, ctx.Session.UserId);

        if (err != null)
        {
            await View(ctx, "Teacher/CourseForm.html",
                FormModel(ctx, $"/teacher/courses/{id}/edit", "Зберегти", form, err));
            return;
        }

        course!.Id = id;
        _courseDao.Update(course);
        _logger.LogInformation("Teacher {TId} updated course {CId}", ctx.Session.UserId, id);
        Redirect(ctx, "/teacher/courses");
    }

    // POST /teacher/courses/{id}/delete
    public async Task PostDeleteCourse(HttpContext ctx)
    {
        var id = ctx.GetRouteInt("id");
        var course = _courseDao.GetById(id);
        if (course != null && course.TeacherId == ctx.Session!.UserId)
        {
            _courseDao.Delete(id);
            _logger.LogInformation("Teacher {TId} deleted course {CId}", ctx.Session.UserId, id);
        }
        Redirect(ctx, "/teacher/courses");
    }

    // GET /teacher/courses/{id}/students
    public async Task GetStudents(HttpContext ctx)
    {
        var courseId = ctx.GetRouteInt("id");
        var course = _courseDao.GetById(courseId);
        if (course == null || course.TeacherId != ctx.Session!.UserId)
        {
            Redirect(ctx, "/teacher/courses");
            return;
        }

        var enrollments = _enrollmentDao.GetByCourseId(courseId).ToList();

        // For each enrollment fetch the student name
        var rows = enrollments.Select(e =>
        {
            var student = _userDao.GetById(e.StudentId);
            return new Dictionary<string, object?>
            {
                ["EnrollmentId"] = e.Id.ToString(),
                ["StudentName"] = student?.FullName ?? "—",
                ["EnrolledAt"] = e.EnrolledAt.ToString("dd.MM.yyyy"),
                ["Status"] = e.Status.ToString(),
                ["Grade"] = e.Grade?.ToString() ?? "",
                ["Feedback"] = e.Feedback ?? "",
                ["HasGrade"] = e.Grade.HasValue ? "true" : "",
                ["IsActive"] = e.Status == EnrollmentStatus.Active ? "true" : ""
            };
        }).Cast<object?>().ToList();

        await View(ctx, "Teacher/Students.html", new Dictionary<string, object?>
        {
            ["title"] = $"Студенти — {course.Title}",
            ["username"] = ctx.Session.Username,
            ["courseName"] = course.Title,
            ["courseId"] = course.Id.ToString(),
            ["enrollments"] = rows
        });
    }

    // POST /teacher/grade  (form: enrollmentId, grade, feedback)
    public async Task PostGrade(HttpContext ctx)
    {
        var form = await ReadFormAsync(ctx.Request);
        var enrollmentId = int.Parse(form.GetValueOrDefault("enrollmentId", "0"));
        var grade = int.Parse(form.GetValueOrDefault("grade", "0"));
        var feedback = form.GetValueOrDefault("feedback", "");
        var courseId = int.Parse(form.GetValueOrDefault("courseId", "0"));

        if (grade < 1 || grade > 100)
        {
            _logger.LogWarning("Invalid grade {Grade} submitted by teacher {TId}", grade, ctx.Session!.UserId);
            Redirect(ctx, $"/teacher/courses/{courseId}/students");
            return;
        }

        var enrollment = _enrollmentDao.GetById(enrollmentId);
        if (enrollment != null)
        {
            // Verify the teacher owns this course
            var course = _courseDao.GetById(enrollment.CourseId);
            if (course?.TeacherId == ctx.Session!.UserId)
            {
                _enrollmentDao.UpdateGrade(enrollmentId, grade, feedback);
                _logger.LogInformation("Teacher {TId} graded enrollment {EnId}: {Grade}",
                    ctx.Session.UserId, enrollmentId, grade);
            }
        }
        Redirect(ctx, $"/teacher/courses/{courseId}/students");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static (Course? course, string? error) ParseCourseForm(
        Dictionary<string, string> form, int teacherId)
    {
        var title = form.GetValueOrDefault("title", "").Trim();
        var desc = form.GetValueOrDefault("description", "").Trim();
        var maxStr = form.GetValueOrDefault("maxStudents", "30");
        var isActive = form.ContainsKey("isActive");

        if (string.IsNullOrEmpty(title)) return (null, "Назва курсу є обов'язковою");
        if (string.IsNullOrEmpty(desc)) return (null, "Опис курсу є обов'язковим");
        if (!int.TryParse(maxStr, out var max) || max < 1 || max > 500)
            return (null, "Максимальна кількість студентів має бути від 1 до 500");

        return (new Course
        {
            Title = title,
            Description = desc,
            TeacherId = teacherId,
            MaxStudents = max,
            IsActive = isActive
        }, null);
    }

    private static Dictionary<string, object?> FormModel(HttpContext ctx,
        string action, string btnLabel, Dictionary<string, string> form, string? error)
        => new()
        {
            ["title"] = "Курс",
            ["username"] = ctx.Session!.Username,
            ["action"] = action,
            ["btnLabel"] = btnLabel,
            ["Id"] = form.GetValueOrDefault("id", ""),
            ["Title"] = form.GetValueOrDefault("title", ""),
            ["Description"] = form.GetValueOrDefault("description", ""),
            ["MaxStudents"] = form.GetValueOrDefault("maxStudents", "30"),
            ["IsActive"] = form.ContainsKey("isActive") ? "true" : "",
            ["error"] = error ?? ""
        };
}