using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class LoginCredentialsDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}

