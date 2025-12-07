using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class WhiteLabelSettings
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [Required]
        [StringLength(7)]
        public string PrimaryColor { get; set; } = "#1976d2";

        [Required]
        [StringLength(7)]
        public string SecondaryColor { get; set; } = "#424242";

        [StringLength(255)]
        public string? CustomDomain { get; set; }

        [Required]
        public bool IsActive { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

