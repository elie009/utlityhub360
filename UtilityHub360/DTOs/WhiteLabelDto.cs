using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class WhiteLabelSettingsDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string PrimaryColor { get; set; } = "#1976d2";
        public string SecondaryColor { get; set; } = "#424242";
        public string? CustomDomain { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateWhiteLabelSettingsDto
    {
        [StringLength(100)]
        public string? CompanyName { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(7)]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code")]
        public string? PrimaryColor { get; set; }

        [StringLength(7)]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code")]
        public string? SecondaryColor { get; set; }

        [StringLength(255)]
        public string? CustomDomain { get; set; }

        public bool? IsActive { get; set; }
    }
}

