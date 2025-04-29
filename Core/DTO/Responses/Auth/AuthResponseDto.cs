using CollageMangmentSystem.Core.Entities.user;

namespace CollageManagementSystem.DTOs.Responses.Auth
{
    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }

        public string? Role { get; set; }
    }
}