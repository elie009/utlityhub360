using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class SavingsTransaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string SavingsAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string SourceBankAccountId { get; set; } = string.Empty; // Where the money comes from

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = string.Empty; // DEPOSIT, WITHDRAWAL, TRANSFER

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; } // MONTHLY_SAVINGS, BONUS, TAX_REFUND, etc.

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string? RecurringFrequency { get; set; } // MONTHLY, WEEKLY, etc.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SavingsAccountId")]
        public virtual SavingsAccount SavingsAccount { get; set; } = null!;

        [ForeignKey("SourceBankAccountId")]
        public virtual BankAccount SourceBankAccount { get; set; } = null!;
    }
}
