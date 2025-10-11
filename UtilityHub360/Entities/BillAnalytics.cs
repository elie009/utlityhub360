using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Entity for storing user budget settings for bills
    /// </summary>
    public class BudgetSetting
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Provider { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BillType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyBudget { get; set; }

        [Required]
        public bool EnableAlerts { get; set; } = true;

        [Required]
        public int AlertThreshold { get; set; } = 90; // Percentage

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Entity for caching bill analytics calculations
    /// </summary>
    public class BillAnalyticsCache
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Provider { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BillType { get; set; } = string.Empty;

        [Required]
        public DateTime CalculationMonth { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SimpleAverage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightedAverage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SeasonalAverage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ForecastAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HighestBill { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LowestBill { get; set; }

        [StringLength(20)]
        public string Trend { get; set; } = string.Empty; // "increasing", "decreasing", "stable"

        public int BillCount { get; set; }

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Entity for storing bill alerts/notifications
    /// </summary>
    public class BillAlert
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AlertType { get; set; } = string.Empty; // "budget_exceeded", "trend_increase", "unusual_spike", "due_date", "overdue", "savings"

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = string.Empty; // "info", "warning", "error", "success"

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(450)]
        public string? BillId { get; set; }

        [StringLength(100)]
        public string? Provider { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [StringLength(500)]
        public string? ActionLink { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("BillId")]
        public virtual Bill? Bill { get; set; }
    }
}

