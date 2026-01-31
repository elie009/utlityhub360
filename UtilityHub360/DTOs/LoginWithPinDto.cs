using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    /// <summary>DTO for logging in with PIN (mobile-only).</summary>
    public class LoginWithPinDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN must be exactly 6 digits.")]
        public string Pin { get; set; } = string.Empty;
    }
}
