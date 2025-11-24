using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Represents an imported bank statement
    /// </summary>
    public class BankStatement
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string StatementName { get; set; } = string.Empty; // e.g., "January 2024 Statement"

        [Required]
        public DateTime StatementStartDate { get; set; }

        [Required]
        public DateTime StatementEndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ClosingBalance { get; set; }

        [StringLength(50)]
        public string? ImportFormat { get; set; } // CSV, OFX, QIF

        [StringLength(500)]
        public string? ImportSource { get; set; } // File name or source

        [Required]
        public int TotalTransactions { get; set; } // Number of statement items

        [Required]
        public int MatchedTransactions { get; set; } = 0; // Number of matched items

        [Required]
        public int UnmatchedTransactions { get; set; } = 0; // Number of unmatched items

        [Required]
        public bool IsReconciled { get; set; } = false; // Whether statement is fully reconciled

        public DateTime? ReconciledAt { get; set; }

        [StringLength(450)]
        public string? ReconciledBy { get; set; } // UserId who reconciled

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; } = null!;

        public virtual ICollection<BankStatementItem> StatementItems { get; set; } = new List<BankStatementItem>();
        public virtual ICollection<Reconciliation> Reconciliations { get; set; } = new List<Reconciliation>();
    }
}

