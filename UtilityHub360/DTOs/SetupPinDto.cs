using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    /// <summary>DTO for setting up PIN login (mobile-only).</summary>
    public class SetupPinDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN must be exactly 6 digits.")]
        public string Pin { get; set; } = string.Empty;
    }
}
