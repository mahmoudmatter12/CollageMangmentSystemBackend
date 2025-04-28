using System.ComponentModel.DataAnnotations;

namespace CollageMangmentSystem.Core.DTO.Requests
{
    public class UpdateUserDto
    {
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters")]
        public string? Fname { get; set; }

        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Last name can only contain letters")]
        public string? Lname { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

    }
}