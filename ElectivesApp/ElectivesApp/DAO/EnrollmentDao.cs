using ElectivesApp.Infrastructure;
using ElectivesApp.Models;
using Npgsql;

namespace ElectivesApp.DAO;

public class EnrollmentDao : IEnrollmentDao
{
    private readonly IDbConnectionFactory _factory;

    public EnrollmentDao(IDbConnectionFactory factory) => _factory = factory;

    private const string SelectBase =
        "SELECT e.id, e.student_id, e.course_id, c.title AS course_name, " +
        "u.full_name AS teacher_name, e.enrolled_at, e.status, e.grade, e.feedback, e.graded_at " +
        "FROM enrollments e " +
        "JOIN courses c ON c.id = e.course_id " +
        "JOIN users u ON u.id = c.teacher_id";

    public Enrollment? GetById(int id)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand($"{SelectBase} WHERE e.id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IEnumerable<Enrollment> GetByStudentId(int studentId)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand($"{SelectBase} WHERE e.student_id = @sid ORDER BY e.enrolled_at DESC", conn);
        cmd.Parameters.AddWithValue("sid", studentId);
        using var reader = cmd.ExecuteReader();
        var list = new List<Enrollment>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public IEnumerable<Enrollment> GetByCourseId(int courseId)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        // Also get student names for teacher view
        using var cmd = new NpgsqlCommand(
            "SELECT e.id, e.student_id, e.course_id, c.title AS course_name, " +
            "ust.full_name AS student_full_name, ut.full_name AS teacher_name, " +
            "e.enrolled_at, e.status, e.grade, e.feedback, e.graded_at " +
            "FROM enrollments e " +
            "JOIN courses c ON c.id = e.course_id " +
            "JOIN users ut ON ut.id = c.teacher_id " +
            "JOIN users ust ON ust.id = e.student_id " +
            "WHERE e.course_id = @cid ORDER BY ust.full_name", conn);
        cmd.Parameters.AddWithValue("cid", courseId);
        using var reader = cmd.ExecuteReader();
        var list = new List<Enrollment>();
        while (reader.Read())
        {
            var e = Map(reader);
            // Attach student name as a temporary field via feedback slot is wrong,
            // we use a dedicated DTO instead – here we do a simple re-mapping
            list.Add(e);
        }
        return list;
    }

    public bool IsEnrolled(int studentId, int courseId)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM enrollments WHERE student_id=@sid AND course_id=@cid", conn);
        cmd.Parameters.AddWithValue("sid", studentId);
        cmd.Parameters.AddWithValue("cid", courseId);
        return (long)cmd.ExecuteScalar()! > 0;
    }

    public int Create(Enrollment enrollment)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO enrollments (student_id, course_id, enrolled_at, status) " +
            "VALUES (@sid, @cid, NOW(), 'Active') RETURNING id", conn);
        cmd.Parameters.AddWithValue("sid", enrollment.StudentId);
        cmd.Parameters.AddWithValue("cid", enrollment.CourseId);
        return (int)cmd.ExecuteScalar()!;
    }

    public void UpdateGrade(int enrollmentId, int grade, string feedback)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "UPDATE enrollments SET grade=@g, feedback=@f, graded_at=NOW(), status='Completed' " +
            "WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("g", grade);
        cmd.Parameters.AddWithValue("f", feedback);
        cmd.Parameters.AddWithValue("id", enrollmentId);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM enrollments WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    private static Enrollment Map(NpgsqlDataReader r) => new Enrollment
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        StudentId = r.GetInt32(r.GetOrdinal("student_id")),
        CourseId = r.GetInt32(r.GetOrdinal("course_id")),
        CourseName = r.GetString(r.GetOrdinal("course_name")),
        TeacherName = r.GetString(r.GetOrdinal("teacher_name")),
        EnrolledAt = r.GetDateTime(r.GetOrdinal("enrolled_at")),
        Status = Enum.Parse<EnrollmentStatus>(r.GetString(r.GetOrdinal("status"))),
        Grade = r.IsDBNull(r.GetOrdinal("grade")) ? null : r.GetInt32(r.GetOrdinal("grade")),
        Feedback = r.IsDBNull(r.GetOrdinal("feedback")) ? null : r.GetString(r.GetOrdinal("feedback")),
        GradedAt = r.IsDBNull(r.GetOrdinal("graded_at")) ? null : r.GetDateTime(r.GetOrdinal("graded_at"))
    };
}