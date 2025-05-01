// Controllers/UsersController.cs
using System.Security.Claims;
using CollageManagementSystem.Services;
using CollageMangmentSystem.Core.DTO.Responses;
using CollageMangmentSystem.Core.DTO.Responses.user;
using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CollageMangmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<User> _userRepository;
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;

        public UsersController(IRepository<User> userRepository, ILogger<UsersController> logger, IUserService userService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("all")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetAllUsersPaged(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var users = await _userRepository.GetAllAsyncPaged(pageNumber, pageSize);
                var totalUsers = await _userRepository.GetAllAsync();
                var usersDto = users.Select(user => user.ToGetStudentIdResponseDto()).ToList();
                return Ok(new PagedResponse<GetUserIdResponseDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalUsers.Count(),
                    TotalPages = (int)Math.Ceiling((double)totalUsers.Count() / pageSize),
                    Data = usersDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return NotFound();

                return Ok(new GetUserIdResponseDto
                {
                    Id = user.Id,
                    Fullname = user.FullName,
                    Email = user.Email,
                    Role = user.GetRoleByIndex((int)user.Role),
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                await _userRepository.DeleteAsync(user);

                return Ok("User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet("protected")]
        public async Task<IActionResult> Protected()
        {
            var userId = GetUserIdFromClaims();
            var UserRole = await _userService.GetRoleByUserId(userId);
            return Ok(new { message = "This is a protected route", userId, UserRole });
        }


        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            return userId;
        }
    }
}