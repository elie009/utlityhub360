using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class BankTransaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty; // CREDIT, DEBIT

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; } // Income, Expense, Transfer, etc.

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(100)]
        public string? ExternalTransactionId { get; set; } // From bank/integration

        public DateTime TransactionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Merchant { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string? RecurringFrequency { get; set; } // monthly, weekly, etc.

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfterTransaction { get; set; }

        // Navigation properties
        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
