using ElectivesApp.Infrastructure;
using ElectivesApp.Models;
using Npgsql;

namespace ElectivesApp.DAO;

public class UserDao : IUserDao
{
    private readonly IDbConnectionFactory _factory;

    public UserDao(IDbConnectionFactory factory) => _factory = factory;

    public User? GetByUsername(string username)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT id, username, password_hash, full_name, email, role, department, group_name " +
            "FROM users WHERE username = @u", conn);
        cmd.Parameters.AddWithValue("u", username);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public User? GetById(int id)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT id, username, password_hash, full_name, email, role, department, group_name " +
            "FROM users WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public int Create(User user)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO users (username, password_hash, full_name, email, role, department, group_name) " +
            "VALUES (@u, @ph, @fn, @em, @r, @dept, @grp) RETURNING id", conn);
        cmd.Parameters.AddWithValue("u", user.Username);
        cmd.Parameters.AddWithValue("ph", user.PasswordHash);
        cmd.Parameters.AddWithValue("fn", user.FullName);
        cmd.Parameters.AddWithValue("em", user.Email);
        cmd.Parameters.AddWithValue("r", user.Role);
        cmd.Parameters.AddWithValue("dept", user is Teacher t ? (object)t.Department : DBNull.Value);
        cmd.Parameters.AddWithValue("grp", user is Student s ? (object)s.GroupName : DBNull.Value);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Update(User user)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "UPDATE users SET full_name=@fn, email=@em, department=@dept, group_name=@grp " +
            "WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("fn", user.FullName);
        cmd.Parameters.AddWithValue("em", user.Email);
        cmd.Parameters.AddWithValue("dept", user is Teacher t ? (object)t.Department : DBNull.Value);
        cmd.Parameters.AddWithValue("grp", user is Student s ? (object)s.GroupName : DBNull.Value);
        cmd.Parameters.AddWithValue("id", user.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM users WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    public IEnumerable<Student> GetAllStudents()
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT id, username, password_hash, full_name, email, role, department, group_name " +
            "FROM users WHERE role='student' ORDER BY full_name", conn);
        using var reader = cmd.ExecuteReader();
        var list = new List<Student>();
        while (reader.Read())
            list.Add((Student)MapUser(reader));
        return list;
    }

    public IEnumerable<Teacher> GetAllTeachers()
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT id, username, password_hash, full_name, email, role, department, group_name " +
            "FROM users WHERE role='teacher' ORDER BY full_name", conn);
        using var reader = cmd.ExecuteReader();
        var list = new List<Teacher>();
        while (reader.Read())
            list.Add((Teacher)MapUser(reader));
        return list;
    }

    private static User MapUser(NpgsqlDataReader r)
    {
        var role = r.GetString(r.GetOrdinal("role"));
        User user = role == "teacher" ? new Teacher() : new Student();
        user.Id = r.GetInt32(r.GetOrdinal("id"));
        user.Username = r.GetString(r.GetOrdinal("username"));
        user.PasswordHash = r.GetString(r.GetOrdinal("password_hash"));
        user.FullName = r.GetString(r.GetOrdinal("full_name"));
        user.Email = r.GetString(r.GetOrdinal("email"));
        user.Role = role;

        if (user is Teacher t)
            t.Department = r.IsDBNull(r.GetOrdinal("department")) ? "" : r.GetString(r.GetOrdinal("department"));
        if (user is Student s)
            s.GroupName = r.IsDBNull(r.GetOrdinal("group_name")) ? "" : r.GetString(r.GetOrdinal("group_name"));

        return user;
    }
}