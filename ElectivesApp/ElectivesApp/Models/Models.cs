namespace ElectivesApp.Models;

// ── User (base for Teacher and Student) ──────────────────────────────────────
public abstract class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;  // "teacher" | "student"
}

public class Teacher : User
{
    public Teacher() { Role = "teacher"; }
    public string Department { get; set; } = string.Empty;
    public List<Course> Courses { get; set; } = new();
}

public class Student : User
{
    public Student() { Role = "student"; }
    public string GroupName { get; set; } = string.Empty;
    public List<Enrollment> Enrollments { get; set; } = new();
}

// ── Course ────────────────────────────────────────────────────────────────────
public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;  // denormalized for display
    public int MaxStudents { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

// ── Enrollment ────────────────────────────────────────────────────────────────
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;   // denormalized
    public string TeacherName { get; set; } = string.Empty;  // denormalized
    public DateTime EnrolledAt { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    // Grade info (filled by teacher after completion)
    public int? Grade { get; set; }          // 1-100
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
}

public enum EnrollmentStatus { Active, Completed, Withdrawn }