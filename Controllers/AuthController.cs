using CollageManagementSystem.DTOs.Requests.Auth;
using CollageManagementSystem.DTOs.Responses.Auth;
using CollageManagementSystem.Models;
using CollageManagementSystem.Services;
using CollageManagementSystem.Services.Auth;
using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Entities.department;
using CollageMangmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CollageMangmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        private readonly IDepRepostaory<Department> _depRepostaory;

        public AuthController(
            ITokenService tokenService,
            IUserService userService,
            IConfiguration configuration,
            IDepRepostaory<Department> depRepostaory
            )
        {
            _tokenService = tokenService;
            _userService = userService;
            _configuration = configuration;
            _depRepostaory = depRepostaory;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Check if user already exists
            if (await _userService.UserExists(registerDto.Email))
            {
                return BadRequest("User already exists");
            }

            // Create password hash
            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Create new user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = registerDto.FullName ?? string.Empty,
                DepartmentId = registerDto.DepartmentId,
            };

            // Save user
            await _userService.CreateUser(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, user.GetRoleByIndex((int)user.Role));
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Refresh token valid for 7 days
            await _userService.UpdateUser(user);
            var depNamee = _depRepostaory.GetByIdAsync(registerDto.DepartmentId ?? Guid.Empty);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                DepartmentName = depNamee?.Result?.Name ?? string.Empty,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = user.GetRoleByIndex((int)user.Role),
                AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                DepartmentId = user.DepartmentId,
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.GetUserByEmail(loginDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, user.GetRoleByIndex((int)user.Role));
            var refreshToken = _tokenService.GenerateRefreshToken();

            SetTokenCookies(accessToken, refreshToken, user.Id.ToString());
            // set refresh token expiry in header
            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
                Role = user.GetRoleByIndex((int)user.Role),
                FullName = user.FullName
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Invalid refresh token");
            }

            var user = await _userService.GetUserByRefreshToken(refreshToken);
            if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return BadRequest("Invalid refresh token");
            }

            // Get user role again

            var newAccessToken = _tokenService.GenerateAccessToken(user, user.GetRoleByIndex((int)user.Role));
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update cookies
            SetTokenCookies(newAccessToken, newRefreshToken, user.Id.ToString());

            // Update user with new refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userService.UpdateUser(user);

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.GetRoleByIndex((int)user.Role),
            });
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            var user = await _userService.GetUserByRefreshToken(refreshToken);
            if (user == null)
            {
                return BadRequest("Invalid refresh token");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userService.UpdateUser(user);

            return NoContent();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var user = await _userService.GetUserByRefreshToken(refreshToken);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;
                    await _userService.UpdateUser(user);
                }
            }

            // Clear cookies
            ClearTokenCookies();

            return Ok(new { Message = "Logged out successfully" });
        }



        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        private void SetTokenCookies(string accessToken, string refreshToken, string userId)
        {
            // Access token cookie
            Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = _configuration.GetValue<bool>("CookieSettings:UseHttps"),
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Domain = _configuration["CookieSettings:CookieDomain"],
                Path = "/",
                IsEssential = true
            });

            // Refresh token cookie (more restrictive)
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = _configuration.GetValue<bool>("CookieSettings:UseHttps"),
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/auth/refresh",
                IsEssential = true
            });

            // User ID cookie (non-sensitive)
            Response.Cookies.Append("userId", userId, new CookieOptions
            {
                Secure = _configuration.GetValue<bool>("CookieSettings:UseHttps"),
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        private void ClearTokenCookies()
        {
            var cookieOptions = new CookieOptions
            {
                Secure = _configuration.GetValue<bool>("CookieSettings:UseHttps"),
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            Response.Cookies.Delete("accessToken", cookieOptions);
            Response.Cookies.Delete("refreshToken", cookieOptions);
            Response.Cookies.Delete("userId", cookieOptions);
        }
        private string GetUserIdFromCookies()
        {
            var userId = Request.Cookies["userId"];
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in cookies");
            }
            return userId;
        }





    }
}