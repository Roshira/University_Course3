using ElectivesApp.DAO;
using ElectivesApp.Models;
using Microsoft.Extensions.Logging;

namespace ElectivesApp.Core.Controllers;

public class StudentController : BaseController
{
    private readonly ICourseDao _courseDao;
    private readonly IEnrollmentDao _enrollmentDao;
    private readonly ILogger<StudentController> _logger;

    public StudentController(ICourseDao courseDao, IEnrollmentDao enrollmentDao,
        ILoggerFactory loggerFactory)
    {
        _courseDao = courseDao;
        _enrollmentDao = enrollmentDao;
        _logger = loggerFactory.CreateLogger<StudentController>();
    }

    // GET /student/courses – list all available courses
    public async Task GetCourses(HttpContext ctx)
    {
        var courses = _courseDao.GetAll().Where(c => c.IsActive).ToList();
        var studentId = ctx.Session!.UserId;
        var enrolledIds = _enrollmentDao.GetByStudentId(studentId)
            .Select(e => e.CourseId).ToHashSet();

        var rows = courses.Select(c => new Dictionary<string, object?>
        {
            ["Id"] = c.Id.ToString(),
            ["Title"] = c.Title,
            ["Description"] = c.Description,
            ["TeacherName"] = c.TeacherName,
            ["MaxStudents"] = c.MaxStudents.ToString(),
            ["IsEnrolled"] = enrolledIds.Contains(c.Id) ? "true" : ""
        }).Cast<object?>().ToList();

        await View(ctx, "Student/Courses.html", new Dictionary<string, object?>
        {
            ["title"] = "Доступні факультативи",
            ["username"] = ctx.Session.Username,
            ["courses"] = rows
        });
    }

    // POST /student/enroll – enroll student in a course
    public async Task PostEnroll(HttpContext ctx)
    {
        var form = await ReadFormAsync(ctx.Request);
        if (!int.TryParse(form.GetValueOrDefault("courseId"), out var courseId))
        {
            Redirect(ctx, "/student/courses");
            return;
        }

        var studentId = ctx.Session!.UserId;
        if (_enrollmentDao.IsEnrolled(studentId, courseId))
        {
            _logger.LogWarning("Student {Id} already enrolled in course {CourseId}", studentId, courseId);
            Redirect(ctx, "/student/courses");
            return;
        }

        _enrollmentDao.Create(new Enrollment { StudentId = studentId, CourseId = courseId });
        _logger.LogInformation("Student {Id} enrolled in course {CourseId}", studentId, courseId);
        Redirect(ctx, "/student/my-courses");
    }

    // GET /student/my-courses – my enrollments
    public async Task GetMyCourses(HttpContext ctx)
    {
        var enrollments = _enrollmentDao.GetByStudentId(ctx.Session!.UserId).ToList();
        var rows = enrollments.Select(e => new Dictionary<string, object?>
        {
            ["Id"] = e.Id.ToString(),
            ["CourseName"] = e.CourseName,
            ["TeacherName"] = e.TeacherName,
            ["EnrolledAt"] = e.EnrolledAt.ToString("dd.MM.yyyy"),
            ["Status"] = e.Status.ToString(),
            ["Grade"] = e.Grade?.ToString() ?? "—",
            ["Feedback"] = e.Feedback ?? "—",
            ["HasGrade"] = e.Grade.HasValue ? "true" : ""
        }).Cast<object?>().ToList();

        await View(ctx, "Student/MyCourses.html", new Dictionary<string, object?>
        {
            ["title"] = "Мої факультативи",
            ["username"] = ctx.Session.Username,
            ["enrollments"] = rows
        });
    }

    // POST /student/unenroll – withdraw from course
    public async Task PostUnenroll(HttpContext ctx)
    {
        var form = await ReadFormAsync(ctx.Request);
        if (!int.TryParse(form.GetValueOrDefault("enrollmentId"), out var enrollmentId))
        {
            Redirect(ctx, "/student/my-courses");
            return;
        }

        var enrollment = _enrollmentDao.GetById(enrollmentId);
        if (enrollment != null && enrollment.StudentId == ctx.Session!.UserId
            && enrollment.Status == EnrollmentStatus.Active)
        {
            _enrollmentDao.Delete(enrollmentId);
            _logger.LogInformation("Student {Id} unenrolled from enrollment {EnId}",
                ctx.Session.UserId, enrollmentId);
        }
        Redirect(ctx, "/student/my-courses");
    }
}