using CollageManagementSystem.Core.Entities.userEnrollments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CollageMangmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IUserEnrollments<UserEnrollments> _userEnrollmentsService;

        public EnrollmentsController(IUserEnrollments<UserEnrollments> userEnrollmentsService)
        {
            _userEnrollmentsService = userEnrollmentsService;
        }

        [HttpGet("{id}/enrollments")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetUserEnrollments(Guid id)
        {
            var enrollments = await _userEnrollmentsService.GetUserEnrollmentById(id);
            if (enrollments == null || !enrollments.Any())
            {
                return NotFound(new { Message = "No enrollments found for this user." });
            }
            return Ok(enrollments);
        }

        [HttpPost("{id}/enrollments")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> AddUserEnrollment(Guid id, [FromBody] CreateUserEnrollmentDto userEnrollment)
        {
            if (userEnrollment == null)
            {
                return BadRequest(new { Message = "Invalid enrollment data." });
            }

            userEnrollment.UserId = id; // Set the user ID for the enrollment
            // var existingEnrollment = await _userEnrollmentsService.GetUserEnrollmentById(id);
            // if (existingEnrollment != null)
            // {
            //     return Conflict(new { Message = "User is already enrolled." });
            // }
            var newEnrollment = new UserEnrollments
            {
                UserId = id,
                CourseId = userEnrollment.CourseId,
                EnrollDate = DateTime.UtcNow,
            };
            await _userEnrollmentsService.AddUserEnrollment(newEnrollment);
            return CreatedAtAction(nameof(GetUserEnrollments), new { id }, userEnrollment);
        }

    }
}