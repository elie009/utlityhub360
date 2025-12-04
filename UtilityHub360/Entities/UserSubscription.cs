using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class UserSubscription
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string SubscriptionPlanId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, CANCELLED, EXPIRED, SUSPENDED, TRIAL

        [Required]
        [StringLength(20)]
        public string BillingCycle { get; set; } = "MONTHLY"; // MONTHLY, YEARLY

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrice { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? TrialEndDate { get; set; }

        [StringLength(255)]
        public string? StripeSubscriptionId { get; set; } // For Stripe integration
        [StringLength(255)]
        public string? StripeCustomerId { get; set; }
        [StringLength(255)]
        public string? PaymentMethodId { get; set; }

        // Usage tracking
        public int TransactionsThisMonth { get; set; } = 0;
        public int BillsThisMonth { get; set; } = 0;
        public int ReceiptOcrThisMonth { get; set; } = 0;
        public int AiQueriesThisMonth { get; set; } = 0;
        public int ApiCallsThisMonth { get; set; } = 0;
        public DateTime? LastUsageResetDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("SubscriptionPlanId")]
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    }
}

