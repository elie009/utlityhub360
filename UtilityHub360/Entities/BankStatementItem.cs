using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Represents a single transaction line item from a bank statement
    /// </summary>
    public class BankStatementItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string BankStatementId { get; set; } = string.Empty;

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string TransactionType { get; set; } = string.Empty; // DEBIT, CREDIT

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ReferenceNumber { get; set; }

        [StringLength(255)]
        public string? Merchant { get; set; }

        [StringLength(255)]
        public string? Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfterTransaction { get; set; }

        [Required]
        public bool IsMatched { get; set; } = false; // Whether matched to a system transaction

        [StringLength(450)]
        public string? MatchedTransactionId { get; set; } // ID of matched Payment/BankTransaction

        [StringLength(50)]
        public string? MatchedTransactionType { get; set; } // Payment, BankTransaction

        public DateTime? MatchedAt { get; set; }

        [StringLength(450)]
        public string? MatchedBy { get; set; } // UserId who matched

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BankStatementId")]
        public virtual BankStatement BankStatement { get; set; } = null!;
    }
}

