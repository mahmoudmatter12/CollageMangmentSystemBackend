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
        [MaxLength(100)]
        public required string Fname { get; set; }

        [MaxLength(100)]
        public string? Lname { get; set; }

        [NotMapped]
        public string Fullname => $"{Fname}{(string.IsNullOrEmpty(Lname) ? "" : " " + Lname)}";

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Required]
        [MaxLength(255)] // For hashed password
        public string PasswordHash { get; set; } = string.Empty;

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