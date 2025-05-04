using CollageManagementSystem.Core.Entities.userEnrollments;
using CollageManagementSystem.Services;
using CollageMangmentSystem.Core.DTO.Responses.CombiendDtos;
using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Entities.course;
using CollageMangmentSystem.Core.Entities.department;
using CollageMangmentSystem.Core.Entities.user;
using CollageMangmentSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollageMangmentSystem.Infrastructure.Data.Repositories;

public class AdminReposatory : IAdminReposatory
{
    private readonly ApplicationDbContext _context;
    private readonly ICourseReposatory<Course> _courseReposatory;
    private readonly IDepRepostaory<Department> _departmentRepository;
    private readonly IUserEnrollments<UserEnrollments> _userEnrollmentsService;
    private readonly IUserService _userService;

    public AdminReposatory(
        ICourseReposatory<Course> courseReposatory,
        IDepRepostaory<Department> departmentRepository,
        IUserService userService,
        ApplicationDbContext context,
        IUserEnrollments<UserEnrollments> userEnrollmentsService)
    {
        _context = context;
        _courseReposatory = courseReposatory;
        _departmentRepository = departmentRepository;
        _userService = userService;
        _userEnrollmentsService = userEnrollmentsService;
    }

    // Users
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userService.GetAllUsers();
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _userService.GetUserById(id)
            ?? throw new KeyNotFoundException($"User with id {id} not found.");
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _userService.GetUserByEmail(email);
        if (user == null)
        {
            return null;
        }
        return user;
    }

    public async Task<IEnumerable<User>> GetUsersByNameAsync(string name)
    {
        var lowerCaseName = name.ToLower();
        return await _context.Users
            .Where(u => u.FullName.ToLower().Contains(lowerCaseName))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByCourseAsync(Guid courseId)
    {
        return await _context.UserEnrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.User)
            .Where(u => u != null)
            .OfType<User>()
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersWithRolesAsync()
    {
        return await _context.Users
            .Select(u => new User
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();
    }

    public async Task ToggleUserRoleAsync(Guid userId, UserRole role)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {userId} not found.");
        }
        user.Role = role;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    // Enrollments
    public async Task<IEnumerable<UserEnrollments>> GetEnrollmentsByUserIdAsync(Guid userId)
    {
        return await _userEnrollmentsService.GetUserEnrollmentsById(userId);
    }

    public async Task<IEnumerable<User>> GetUsersByEnrollmentIdAsync(Guid enrollmentId)
    {
        var enrollment = await _userEnrollmentsService.GetByIdAsync(enrollmentId);
        if (enrollment == null) return Enumerable.Empty<User>();

        var user = await _userService.GetUserById(enrollment.UserId);
        return user != null ? new List<User> { user } : Enumerable.Empty<User>();
    }

    public async Task<IEnumerable<UserEnrollments>> GetAllEnrollmentsAsync()
    {
        return await _context.UserEnrollments.ToListAsync();
    }

    public async Task<UserEnrollments?> GetEnrollmentByIdAsync(Guid id)
    {
        return await _userEnrollmentsService.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Enrollment with id {id} not found.");
    }

    // Courses
    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _courseReposatory.GetAllAsync();
    }

    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        return await _courseReposatory.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Course with id {id} not found.");
    }

    public async Task<IEnumerable<Course>> GetCoursesByNameAsync(string name)
    {
        return await _context.Courses
            .Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(Guid departmentId)
    {
        return await _context.Courses
            .Where(c => c.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<Course?> ToggleCourseStatusAsync(Guid courseId)
    {
        var course = await _courseReposatory.GetByIdAsync(courseId)
            ?? throw new KeyNotFoundException($"Course with id {courseId} not found.");

        course.IsOpen = !course.IsOpen;
        await _courseReposatory.UpdateAsync(course);
        await _courseReposatory.SaveChangesAsync();
        return course;
    }

    // Departments
    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _departmentRepository.GetAllAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(Guid id)
    {
        return await _departmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Department with id {id} not found.");
    }

    public async Task<IEnumerable<Department>> GetDepartmentsByNameAsync(string name)
    {
        return await _context.Departments
            .Where(d => d.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
    }

    // Combined Queries
    public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(Guid departmentId, Guid? courseId = null, Guid? enrollmentId = null)
    {
        var query = _context.Users.Where(u => u.DepartmentId == departmentId);

        if (courseId.HasValue)
        {
            query = query.Where(u => _context.UserEnrollments
                .Any(e => e.UserId == u.Id && e.CourseId == courseId.Value));
        }

        if (enrollmentId.HasValue)
        {
            query = query.Where(u => _context.UserEnrollments
                .Any(e => e.UserId == u.Id && e.Id == enrollmentId.Value));
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<CourseWithUsers>> GetCoursesWithEnrolledUsersAsync(Guid courseId)
    {
        // get the course
        var course = await _courseReposatory.GetByIdAsync(courseId);
        if (course == null) throw new KeyNotFoundException($"Course with id {courseId} not found.");

        // we have the courseId the its users from the enrollments
        var usersIds = await _context.UserEnrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.UserId)
            .ToListAsync();

        // get the users
        var users = await _context.Users
            .Where(u => usersIds.Contains(u.Id))
            .ToListAsync();

        // return the course with its users
        return new List<CourseWithUsers>
        {
            new CourseWithUsers
            {
            CourseId = course.Id,
            CourseName = course.Name,
            Users = users.Select(u => new User
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role
            }).ToList()
            }
        };
    }
}
