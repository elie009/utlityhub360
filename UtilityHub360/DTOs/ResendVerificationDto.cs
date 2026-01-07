using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ResendVerificationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}


