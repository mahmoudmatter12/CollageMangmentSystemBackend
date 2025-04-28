using CollageMangmentSystem.Core.Entities;

namespace CollageMangmentSystem.Core.DTO.Responses
{
    public class GetUserIdResponseDto
    {
        public Guid Id { get; set; }
        public string? Fullname { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public required string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}