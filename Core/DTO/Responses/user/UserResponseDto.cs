using CollageMangmentSystem.Core.Entities;

namespace CollageMangmentSystem.Core.DTO.Responses
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Fname { get; set; } = string.Empty;
        public string? Lname { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}