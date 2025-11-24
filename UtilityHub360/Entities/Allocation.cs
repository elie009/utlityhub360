using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Entity for storing allocation templates (50/30/20 rule, etc.)
    /// </summary>
    public class AllocationTemplate
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "50/30/20 Rule", "Zero-Based Budget", etc.

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsSystemTemplate { get; set; } = true; // System templates vs user-created

        [StringLength(450)]
        public string? UserId { get; set; } // Null for system templates, set for user-created

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Navigation property for categories
        public virtual ICollection<AllocationTemplateCategory> Categories { get; set; } = new List<AllocationTemplateCategory>();
    }

    /// <summary>
    /// Categories within an allocation template (e.g., Needs, Wants, Savings for 50/30/20)
    /// </summary>
    public class AllocationTemplateCategory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string TemplateId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty; // "Needs", "Wants", "Savings"

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; } // 50.00, 30.00, 20.00

        [Required]
        public int DisplayOrder { get; set; }

        [StringLength(50)]
        public string? Color { get; set; } // Hex color for UI

        // Navigation property
        [ForeignKey("TemplateId")]
        public virtual AllocationTemplate Template { get; set; } = null!;
    }

    /// <summary>
    /// User's active allocation plan
    /// </summary>
    public class AllocationPlan
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? TemplateId { get; set; } // Reference to template if based on one

        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyIncome { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TemplateId")]
        public virtual AllocationTemplate? Template { get; set; }

        public virtual ICollection<AllocationCategory> Categories { get; set; } = new List<AllocationCategory>();
    }

    /// <summary>
    /// Categories within a user's allocation plan
    /// </summary>
    public class AllocationCategory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string PlanId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; } // Budgeted amount

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; } // Percentage of income

        [Required]
        public int DisplayOrder { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("PlanId")]
        public virtual AllocationPlan Plan { get; set; } = null!;
    }

    /// <summary>
    /// Historical tracking of allocation performance over time
    /// </summary>
    public class AllocationHistory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string PlanId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? CategoryId { get; set; } // Null for overall plan, set for specific category

        [Required]
        public DateTime PeriodDate { get; set; } // Month/Year this record represents

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; } // Budgeted

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualAmount { get; set; } // Actual spending

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Variance { get; set; } // Allocated - Actual

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal VariancePercentage { get; set; } // (Variance / Allocated) * 100

        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // "on_track", "over_budget", "under_budget"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PlanId")]
        public virtual AllocationPlan Plan { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual AllocationCategory? Category { get; set; }
    }

    /// <summary>
    /// Recommendations for allocation adjustments
    /// </summary>
    public class AllocationRecommendation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string PlanId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RecommendationType { get; set; } = string.Empty; // "increase_allocation", "decrease_allocation", "rebalance", "emergency_fund"

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(450)]
        public string? CategoryId { get; set; } // Category this recommendation applies to

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SuggestedAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SuggestedPercentage { get; set; }

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "medium"; // "low", "medium", "high", "urgent"

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        public bool IsApplied { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AppliedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PlanId")]
        public virtual AllocationPlan Plan { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual AllocationCategory? Category { get; set; }
    }
}

