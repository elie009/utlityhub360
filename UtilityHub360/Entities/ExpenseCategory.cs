using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Expense categories for better organization and reporting
    /// </summary>
    public class ExpenseCategory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; } // Icon name or emoji

        [StringLength(20)]
        public string? Color { get; set; } // Hex color code

        // Budget settings
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyBudget { get; set; } // Optional monthly budget limit

        [Column(TypeName = "decimal(18,2)")]
        public decimal? YearlyBudget { get; set; } // Optional yearly budget limit

        // Parent category for subcategories
        [StringLength(450)]
        public string? ParentCategoryId { get; set; }

        // Tax settings
        public bool IsTaxDeductible { get; set; } = false;
        [StringLength(50)]
        public string? TaxCategory { get; set; } // BUSINESS, MEDICAL, CHARITABLE, etc.

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0; // For sorting in UI

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ParentCategoryId")]
        public virtual ExpenseCategory? ParentCategory { get; set; }

        // One-to-many relationships
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<ExpenseBudget> Budgets { get; set; } = new List<ExpenseBudget>();
    }
}

