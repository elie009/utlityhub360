using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class Payment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Loan-related fields (nullable for bank transactions)
        [StringLength(450)]
        public string? LoanId { get; set; }

        // Bank Account-related fields (nullable for loan payments)
        [StringLength(450)]
        public string? BankAccountId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = string.Empty; // BANK_TRANSFER, CARD, WALLET, CASH

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, COMPLETED, FAILED

        // Bank Transaction specific fields
        public bool IsBankTransaction { get; set; } = false; // Flag to identify bank transactions

        [StringLength(20)]
        public string? TransactionType { get; set; } // CREDIT, DEBIT (for bank transactions)

        [StringLength(255)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } // FOOD, TRANSPORTATION, ENTERTAINMENT, etc.

        [StringLength(100)]
        public string? ExternalTransactionId { get; set; } // From bank/integration

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
        public decimal? BalanceAfterTransaction { get; set; } // For bank transactions

        [Required]
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        public DateTime? TransactionDate { get; set; } // For bank transactions

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan? Loan { get; set; }

        [ForeignKey("BankAccountId")]
        public virtual BankAccount? BankAccount { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

