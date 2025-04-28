using CollageManagementSystem.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CollageManagementSystem.Middleware
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration? _configuration;
        private readonly ITokenService? _tokenService;

        public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ITokenService tokenService)
        {
            _next = next;
            _configuration = configuration;
            _tokenService = tokenService;
        }
        
        public async Task Invoke(HttpContext context)
        {
            // Skip authentication for auth endpoints and swagger
            if (context.Request.Path.StartsWithSegments("/api/auth") ||
                context.Request.Path.StartsWithSegments("/swagger")
                )
            {
                await _next(context);
                return;
            }

            // Try to get token from cookie first
            var token = context.Request.Cookies["accessToken"];

            // Fallback to Authorization header
            if (string.IsNullOrEmpty(token))
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader?.StartsWith("Bearer ") == true)
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("No token provided login first");
                return;
            }

            try
            {
                if (_tokenService == null)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Token service is not available");
                    return;
                }

                var principal = _tokenService.ValidateToken(token);
                context.User = principal;

                // Attach user info to context
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    context.Items["UserId"] = userId;
                }

                await _next(context);
            }
            catch (SecurityTokenExpiredException)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token has expired");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token");
            }
        }

    }
}