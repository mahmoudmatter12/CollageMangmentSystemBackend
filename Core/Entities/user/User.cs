using System.ComponentModel.DataAnnotations;
using CollageMangmentSystem.Core.DTO.Responses;
using CollageMangmentSystem.Core.Entities.department;
using CollageMangmentSystem.Core.Entities.user;

namespace CollageMangmentSystem.Core.Entities
{
    public class User : BaseEntity
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [Required]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public UserRole Role { get; set; } = UserRole.Student; // Default to Student

        public string GetRoleByIndex(int roleIndex)
        {
            return roleIndex switch
            {
                0 => UserRole.Student.ToString(),
                1 => UserRole.Teacher.ToString(),
                2 => UserRole.Admin.ToString(),
                _ => "Unknown"
            };
        }

        // each user user can be in only one department
        public required Guid DepartmentId { get; set; }
        public Department? Department { get; set; }

        public GetUserIdResponseDto ToGetStudentIdResponseDto() => new()
        {
            Id = Id,
            Fullname = FullName,
            Email = Email,
            Role = Role.ToString(),
            CreatedAt = CreatedAt,
            DepId = DepartmentId,
            DepName = Department?.Name
        };


        public string getName()
        {
            return this.FullName;
        }

    }


}