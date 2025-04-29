using CollageManagementSystem.Core.Entities.department;
using CollageManagementSystem.Services;
using CollageMangmentSystem.Core.DTO.Responses.course;
using CollageMangmentSystem.Core.Entities.course;
using CollageMangmentSystem.Core.Entities.department;
using CollageMangmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CollageMangmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CourseController : ControllerBase
    {
        private readonly IRepository<Course> _CourseRepo;
        private readonly IDepRepostaory<Department> _depRepo;

        private readonly IUserService _userService;
        public CourseController(IRepository<Course> courseRepo, IUserService userService, IDepRepostaory<Department> depRepo)
        {
            _depRepo = depRepo;
            _CourseRepo = courseRepo;
            _userService = userService;
        }

        [HttpGet("all")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _CourseRepo.GetAllAsync();
            var courseDtos = await Task.WhenAll(courses.Select(async course => new courseResponseDto
            {
                Id = course.Id,
                Name = course.Name,
                CreditHours = course.CreditHours,
                Semester = course.Semester,
                IsOpen = course.IsOpen,
                DepName = await _depRepo.GetDepartmentName(course.DepartmentId),
                PrerequisiteCourses = course.PrerequisiteCoursesNames()
            }));

            return Ok(courseDtos.ToList());
        }

        [HttpGet("{id:guid}")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            var course = await _CourseRepo.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound("Course not found");
            }
            var courseDtos = new courseResponseDto
            {
                Id = course.Id,
                Name = course.Name,
                CreditHours = course.CreditHours,
                Semester = course.Semester,
                IsOpen = course.IsOpen,
                DepName = await _depRepo.GetDepartmentName(course.DepartmentId),
                PrerequisiteCourses = course.PrerequisiteCoursesNames()
            };
            return Ok(courseDtos);
        }

        [HttpPost("add")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseReqDto course)
        {
            if (course == null)
            {
                return BadRequest("Course cannot be null");
            }

            var newCourse = new Course
            {
                Name = course.Name,
                CreditHours = course.CreditHours,
                Semester = course.Semester,
                IsOpen = course.IsOpen,
                DepartmentId = course.DepartmentId,
                PrerequisiteCourseIds = course.PrerequisiteCourseIds ?? new List<Guid>(),
            };

            await _CourseRepo.AddAsync(newCourse);
            return CreatedAtAction(nameof(GetCourseById), new { id = newCourse.Id }, newCourse);
        }

        [HttpPost("Toggel/{id:guid}")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> ToggleCourse(Guid id)
        {
            var course = await _CourseRepo.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound("Course not found");
            }

            course.IsOpen = !course.IsOpen;
            await _CourseRepo.UpdateAsync(course);
            var courseDtos = new courseResponseDto
            {
                Id = course.Id,
                Name = course.Name,
                CreditHours = course.CreditHours,
                Semester = course.Semester,
                IsOpen = course.IsOpen,
                DepName = await _depRepo.GetDepartmentName(course.DepartmentId),
                PrerequisiteCourses = course.PrerequisiteCoursesNames()
            };
            return Ok(courseDtos);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _CourseRepo.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound("Course not found");
            }

            await _CourseRepo.DeleteAsync(course);
            return Ok("Course deleted successfully");
        }


    }
}