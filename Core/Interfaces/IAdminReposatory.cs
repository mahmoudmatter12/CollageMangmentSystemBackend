using CollageManagementSystem.Core.Entities.userEnrollments;
using CollageMangmentSystem.Core.DTO.Responses.CombiendDtos;
using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Entities.course;
using CollageMangmentSystem.Core.Entities.department;
using CollageMangmentSystem.Core.Entities.user;

namespace CollageMangmentSystem.Core.Interfaces;

public interface IAdminReposatory
{
    // Users
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByNameAsync(string name);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetUsersByCourseAsync(Guid courseId);
    Task<IEnumerable<User>> GetUsersWithRolesAsync();  // Ensure returns `IEnumerable<T>`
    Task ToggleUserRoleAsync(Guid userId ,UserRole role);

    // Enrollments
    Task<IEnumerable<UserEnrollments>> GetEnrollmentsByUserIdAsync(Guid userId);
    Task<IEnumerable<UserEnrollments>> GetAllEnrollmentsAsync();
    Task<UserEnrollments?> GetEnrollmentByIdAsync(Guid id);

    // Courses
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetCoursesByNameAsync(string name);
    Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(Guid departmentId);
    Task<Course?> ToggleCourseStatusAsync(Guid courseId);

    // Departments
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(Guid id);
    Task<IEnumerable<Department>> GetDepartmentsByNameAsync(string name);

    // Combined Queries
    Task<IEnumerable<User>> GetUsersByDepartmentAsync(Guid departmentId, Guid? courseId = null, Guid? enrollmentId = null);
    Task<IEnumerable<CourseWithUsers>> GetCoursesWithEnrolledUsersAsync(Guid courseId);  // Optional
}