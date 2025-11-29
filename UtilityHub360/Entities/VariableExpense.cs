using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UtilityHub360.Models;

namespace UtilityHub360.Entities
{
    public class VariableExpense
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "OTHER"; // FOOD, GROCERIES, TRANSPORTATION, ENTERTAINMENT, SHOPPING, HEALTHCARE, EDUCATION, TRAVEL, OTHER

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? Merchant { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; } // CASH, CARD, BANK_TRANSFER, WALLET

        public bool IsRecurring { get; set; } = false; // For tracking if this is a recurring expense

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        [StringLength(450)]
        public string? DeletedBy { get; set; }
        
        [StringLength(500)]
        public string? DeleteReason { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

