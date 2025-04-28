using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Entities.user;

namespace CollageMangmentSystem.Core.DTO.Responses
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Fname { get; set; } = string.Empty;
        public string? Lname { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}