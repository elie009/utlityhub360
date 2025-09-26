using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class IncomeSource
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Salary", "Side Hustle", "Freelance", etc.

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Frequency { get; set; } = "MONTHLY"; // WEEKLY, BI_WEEKLY, MONTHLY, QUARTERLY, ANNUALLY

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "PRIMARY"; // PRIMARY, PASSIVE, BUSINESS, SIDE_HUSTLE, INVESTMENT, RENTAL, OTHER

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Company { get; set; } // For salary or business income

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Computed property for monthly equivalent
        [NotMapped]
        public decimal MonthlyAmount => ConvertToMonthly(Amount, Frequency);

        // Helper method to convert any frequency to monthly
        private decimal ConvertToMonthly(decimal amount, string frequency) => frequency.ToUpper() switch
        {
            "WEEKLY" => amount * 4.33m, // Average weeks per month (52 weeks / 12 months)
            "BI_WEEKLY" => amount * 2.17m, // Average bi-weeks per month
            "MONTHLY" => amount,
            "QUARTERLY" => amount / 3m,
            "ANNUALLY" => amount / 12m,
            _ => amount // Default to monthly if unknown frequency
        };
    }
}
