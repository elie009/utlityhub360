using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Budget tracking for expenses by category and period
    /// </summary>
    public class ExpenseBudget
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string CategoryId { get; set; } = string.Empty; // Foreign key to ExpenseCategory

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string PeriodType { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, YEARLY

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Alert thresholds
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AlertThreshold { get; set; } // Percentage (e.g., 80 = alert at 80% of budget)

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual ExpenseCategory Category { get; set; } = null!;

        // One-to-many relationships
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}

