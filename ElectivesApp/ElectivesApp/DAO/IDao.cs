using ElectivesApp.Models;

namespace ElectivesApp.DAO;

public interface IUserDao
{
    User? GetByUsername(string username);
    User? GetById(int id);
    int Create(User user);
    void Update(User user);
    void Delete(int id);
    IEnumerable<Student> GetAllStudents();
    IEnumerable<Teacher> GetAllTeachers();
}

public interface ICourseDao
{
    Course? GetById(int id);
    IEnumerable<Course> GetAll();
    IEnumerable<Course> GetByTeacherId(int teacherId);
    int Create(Course course);
    void Update(Course course);
    void Delete(int id);
}

public interface IEnrollmentDao
{
    Enrollment? GetById(int id);
    IEnumerable<Enrollment> GetByStudentId(int studentId);
    IEnumerable<Enrollment> GetByCourseId(int courseId);
    bool IsEnrolled(int studentId, int courseId);
    int Create(Enrollment enrollment);
    void UpdateGrade(int enrollmentId, int grade, string feedback);
    void Delete(int id);
}