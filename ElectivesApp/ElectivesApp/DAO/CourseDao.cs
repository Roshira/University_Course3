using ElectivesApp.Infrastructure;
using ElectivesApp.Models;
using Npgsql;

namespace ElectivesApp.DAO;

public class CourseDao : ICourseDao
{
    private readonly IDbConnectionFactory _factory;

    public CourseDao(IDbConnectionFactory factory) => _factory = factory;

    private const string SelectBase =
        "SELECT c.id, c.title, c.description, c.teacher_id, u.full_name AS teacher_name, " +
        "c.max_students, c.is_active, c.created_at FROM courses c " +
        "JOIN users u ON u.id = c.teacher_id";

    public Course? GetById(int id)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand($"{SelectBase} WHERE c.id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IEnumerable<Course> GetAll()
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand($"{SelectBase} ORDER BY c.title", conn);
        using var reader = cmd.ExecuteReader();
        var list = new List<Course>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public IEnumerable<Course> GetByTeacherId(int teacherId)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand($"{SelectBase} WHERE c.teacher_id = @tid ORDER BY c.title", conn);
        cmd.Parameters.AddWithValue("tid", teacherId);
        using var reader = cmd.ExecuteReader();
        var list = new List<Course>();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public int Create(Course course)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO courses (title, description, teacher_id, max_students, is_active, created_at) " +
            "VALUES (@t, @d, @tid, @ms, @ia, NOW()) RETURNING id", conn);
        cmd.Parameters.AddWithValue("t", course.Title);
        cmd.Parameters.AddWithValue("d", course.Description);
        cmd.Parameters.AddWithValue("tid", course.TeacherId);
        cmd.Parameters.AddWithValue("ms", course.MaxStudents);
        cmd.Parameters.AddWithValue("ia", course.IsActive);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Update(Course course)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "UPDATE courses SET title=@t, description=@d, max_students=@ms, is_active=@ia WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("t", course.Title);
        cmd.Parameters.AddWithValue("d", course.Description);
        cmd.Parameters.AddWithValue("ms", course.MaxStudents);
        cmd.Parameters.AddWithValue("ia", course.IsActive);
        cmd.Parameters.AddWithValue("id", course.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM courses WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    private static Course Map(NpgsqlDataReader r) => new Course
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        Title = r.GetString(r.GetOrdinal("title")),
        Description = r.GetString(r.GetOrdinal("description")),
        TeacherId = r.GetInt32(r.GetOrdinal("teacher_id")),
        TeacherName = r.GetString(r.GetOrdinal("teacher_name")),
        MaxStudents = r.GetInt32(r.GetOrdinal("max_students")),
        IsActive = r.GetBoolean(r.GetOrdinal("is_active")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("created_at"))
    };
}