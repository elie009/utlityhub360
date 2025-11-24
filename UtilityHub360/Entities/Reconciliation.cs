using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Represents a reconciliation session for matching transactions
    /// </summary>
    public class Reconciliation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? BankStatementId { get; set; } // Optional: if reconciling against a statement

        [Required]
        [StringLength(255)]
        public string ReconciliationName { get; set; } = string.Empty; // e.g., "January 2024 Reconciliation"

        [Required]
        public DateTime ReconciliationDate { get; set; } // Date being reconciled

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BookBalance { get; set; } // System balance

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StatementBalance { get; set; } // Bank statement balance

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Difference { get; set; } // StatementBalance - BookBalance

        [Required]
        public int TotalTransactions { get; set; } // Total system transactions in period

        [Required]
        public int MatchedTransactions { get; set; } = 0; // Number of matched transactions

        [Required]
        public int UnmatchedTransactions { get; set; } = 0; // Number of unmatched transactions

        [Required]
        public int PendingTransactions { get; set; } = 0; // Transactions pending review

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "PENDING"; // PENDING, IN_PROGRESS, COMPLETED, DISCREPANCY

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(450)]
        public string? CompletedBy { get; set; } // UserId who completed reconciliation

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; } = null!;

        [ForeignKey("BankStatementId")]
        public virtual BankStatement? BankStatement { get; set; }

        public virtual ICollection<ReconciliationMatch> Matches { get; set; } = new List<ReconciliationMatch>();
    }
}

