using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Represents a match between a system transaction and a bank statement item
    /// </summary>
    public class ReconciliationMatch
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string ReconciliationId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string SystemTransactionId { get; set; } = string.Empty; // Payment or BankTransaction ID

        [Required]
        [StringLength(50)]
        public string SystemTransactionType { get; set; } = string.Empty; // Payment, BankTransaction

        [StringLength(450)]
        public string? StatementItemId { get; set; } // BankStatementItem ID if matching against statement

        [Required]
        [StringLength(50)]
        public string MatchType { get; set; } = string.Empty; // AUTO, MANUAL, SUGGESTED

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string MatchStatus { get; set; } = "MATCHED"; // MATCHED, UNMATCHED, PENDING, DISPUTED

        [StringLength(1000)]
        public string? MatchNotes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountDifference { get; set; } // If amounts don't match exactly

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? MatchedBy { get; set; } // UserId who created the match

        // Navigation properties
        [ForeignKey("ReconciliationId")]
        public virtual Reconciliation Reconciliation { get; set; } = null!;

        [ForeignKey("StatementItemId")]
        public virtual BankStatementItem? StatementItem { get; set; }
    }
}

