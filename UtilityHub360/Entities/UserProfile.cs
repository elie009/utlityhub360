using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class UserProfile
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        // Employment Information (kept for backward compatibility and general info)
        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(200)]
        public string? Company { get; set; }

        [StringLength(50)]
        public string? EmploymentType { get; set; } // FULL_TIME, PART_TIME, CONTRACT, FREELANCE, SELF_EMPLOYED

        // Tax Information
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; } // Percentage

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyTaxDeductions { get; set; }

        // Financial Goals
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlySavingsGoal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyInvestmentGoal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyEmergencyFundGoal { get; set; }

        // Additional Information
        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        // Currency Preference
        [StringLength(10)]
        public string PreferredCurrency { get; set; } = "USD";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Navigation property for income sources
        public virtual ICollection<IncomeSource> IncomeSources { get; set; } = new List<IncomeSource>();

        // Computed Properties (will be calculated in service layer using IncomeSources)
        [NotMapped]
        public decimal TotalMonthlyIncome { get; set; }

        [NotMapped]
        public decimal NetMonthlyIncome => TotalMonthlyIncome - (MonthlyTaxDeductions ?? 0);

        [NotMapped]
        public decimal TotalMonthlyGoals => 
            (MonthlySavingsGoal ?? 0) + 
            (MonthlyInvestmentGoal ?? 0) + 
            (MonthlyEmergencyFundGoal ?? 0);
    }
}
