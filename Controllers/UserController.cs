// Controllers/UsersController.cs
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CollageManagementSystem.Services;
using CollageMangmentSystem.Core.DTO.Requests;
using CollageMangmentSystem.Core.DTO.Responses;
using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;

    public UsersController(IRepository<User> userRepository, ILogger<UsersController> logger,
        IPasswordHasher passwordHasher, IUserService userService)
    {
        _userRepository = userRepository;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _userService = userService;
    }

    [HttpGet]
    [EnableRateLimiting("FixedWindowPolicy")]
    public async Task<IActionResult> GetAllUsersPaged(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var users = await _userRepository.GetAllAsyncPaged(pageNumber, pageSize);
            var totalUsers = await _userRepository.GetAllAsync();

            var response = new PagedResponse<User>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalUsers.Count(),
                TotalPages = (int)Math.Ceiling((double)totalUsers.Count() / pageSize),
                Users = users.Select(user => new GetUserIdResponseDto
                {
                    Id = user.Id,
                    Fullname = user.FullName,
                    Email = user.Email,
                    Role = user.GetRoleByIndex((int)user.Role),
                    CreatedAt = user.CreatedAt
                }).ToList()
            };

            return Ok(response);
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


    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] AddUserDto userDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            var existingUser = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Email == userDto.Email);

            if (existingUser != null)
                return Conflict("Email already in use");

            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = userDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = userDto.FullName ?? string.Empty,

            };

            await _userRepository.AddAsync(user);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                new
                {
                    user.Id,
                    user.Email,
                    Role = user.GetRoleByIndex((int)user.Role),
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id:guid}")]
    [EnableRateLimiting("FixedWindowPolicy")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto userDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            // Update only the fields that are provided
            user.Email = userDto.Email ?? user.Email;

            await _userRepository.UpdateAsync(user);

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
            _logger.LogError(ex, $"Error updating user with ID {id}");
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


    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
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
