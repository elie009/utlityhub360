using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Represents a journal entry for double-entry accounting
    /// Each entry must have balanced debits and credits
    /// </summary>
    public class JournalEntry
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? LoanId { get; set; } // Associated loan if applicable

        [StringLength(450)]
        public string? BillId { get; set; } // Associated bill if applicable

        [StringLength(450)]
        public string? SavingsAccountId { get; set; } // Associated savings account if applicable

        [Required]
        [StringLength(50)]
        public string EntryType { get; set; } = string.Empty; // LOAN_DISBURSEMENT, LOAN_PAYMENT, BILL_PAYMENT, SAVINGS_DEPOSIT, SAVINGS_WITHDRAWAL, EXPENSE, BANK_TRANSFER, PROCESSING_FEE, DOWN_PAYMENT, INTEREST_ACCRUAL

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Reference { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebit { get; set; } // Sum of all debit amounts

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; } // Sum of all credit amounts

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("LoanId")]
        public virtual Loan? Loan { get; set; }

        [ForeignKey("BillId")]
        public virtual Bill? Bill { get; set; }

        [ForeignKey("SavingsAccountId")]
        public virtual SavingsAccount? SavingsAccount { get; set; }

        public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
    }

    /// <summary>
    /// Represents a single line in a journal entry (debit or credit)
    /// </summary>
    public class JournalEntryLine
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string JournalEntryId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AccountName { get; set; } = string.Empty; // Cash, Loan Payable, Interest Expense, etc.

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; } = string.Empty; // ASSET, LIABILITY, EXPENSE, REVENUE, EQUITY

        [Required]
        [StringLength(10)]
        public string EntrySide { get; set; } = string.Empty; // DEBIT, CREDIT

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("JournalEntryId")]
        public virtual JournalEntry JournalEntry { get; set; } = null!;
    }
}



