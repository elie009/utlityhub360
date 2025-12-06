using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class SubscriptionPlan
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // "Starter", "Professional", "Enterprise"

        [Required]
        [StringLength(50)]
        public string DisplayName { get; set; } = string.Empty; // "Free Plan - Starter", "Premium Plan - Professional"

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? YearlyPrice { get; set; }

        // Feature limits
        public int? MaxBankAccounts { get; set; } // null = unlimited
        public int? MaxTransactionsPerMonth { get; set; } // null = unlimited
        public int? MaxBillsPerMonth { get; set; } // null = unlimited
        public int? MaxLoans { get; set; } // null = unlimited
        public int? MaxSavingsGoals { get; set; } // null = unlimited
        public int? MaxReceiptOcrPerMonth { get; set; } // null = unlimited
        public int? MaxAiQueriesPerMonth { get; set; } // null = unlimited
        public int? MaxApiCallsPerMonth { get; set; } // null = unlimited
        public int? MaxUsers { get; set; } // null = unlimited (for family/team plans)
        public int? TransactionHistoryMonths { get; set; } // null = unlimited

        // Feature flags
        public bool HasAiAssistant { get; set; }
        public bool HasBankFeedIntegration { get; set; }
        public bool HasReceiptOcr { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasInvestmentTracking { get; set; }
        public bool HasTaxOptimization { get; set; }
        public bool HasMultiUserSupport { get; set; }
        public bool HasWhiteLabelOptions { get; set; }
        public bool HasCustomIntegrations { get; set; }
        public bool HasDedicatedSupport { get; set; }
        public bool HasAccountManager { get; set; }
        public bool HasCustomReporting { get; set; }
        public bool HasAdvancedSecurity { get; set; }
        public bool HasComplianceReports { get; set; }
        public bool HasFinancialHealthScore { get; set; }
        public bool HasBillForecasting { get; set; }
        public bool HasDebtOptimizer { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0; // For ordering in UI

        // Stripe Integration Fields
        [StringLength(255)]
        public string? StripeMonthlyPriceId { get; set; }

        [StringLength(255)]
        public string? StripeYearlyPriceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}

