using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CollageMangmentSystem.Core.Entities.user;

namespace CollageMangmentSystem.Core.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // We'll generate UUID in code
        public Guid Id { get; set; } = Guid.NewGuid(); // Auto-generated UUID

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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


    }


}