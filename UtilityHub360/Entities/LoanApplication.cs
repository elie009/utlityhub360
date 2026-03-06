using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class LoanApplication
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Principal { get; set; }

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        public int Term { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyIncome { get; set; }

        [StringLength(20)]
        public string? EmploymentStatus { get; set; } // employed, self-employed, unemployed, retired, student

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED

        [Required]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        [StringLength(450)]
        public string? ReviewedBy { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

