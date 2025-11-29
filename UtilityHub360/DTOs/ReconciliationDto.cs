using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // Bank Statement DTOs
    public class BankStatementDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string BankAccountId { get; set; } = string.Empty;
        public string StatementName { get; set; } = string.Empty;
        public DateTime StatementStartDate { get; set; }
        public DateTime StatementEndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string? ImportFormat { get; set; }
        public string? ImportSource { get; set; }
        public int TotalTransactions { get; set; }
        public int MatchedTransactions { get; set; }
        public int UnmatchedTransactions { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime? ReconciledAt { get; set; }
        public string? ReconciledBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BankStatementItemDto>? StatementItems { get; set; }
    }

    public class BankStatementItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string BankStatementId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Merchant { get; set; }
        public string? Category { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public bool IsMatched { get; set; }
        public string? MatchedTransactionId { get; set; }
        public string? MatchedTransactionType { get; set; }
        public DateTime? MatchedAt { get; set; }
        public string? MatchedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ImportBankStatementDto
    {
        [Required]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string StatementName { get; set; } = string.Empty;

        [Required]
        public DateTime StatementStartDate { get; set; }

        [Required]
        public DateTime StatementEndDate { get; set; }

        [Required]
        public decimal OpeningBalance { get; set; }

        [Required]
        public decimal ClosingBalance { get; set; }

        [StringLength(50)]
        public string? ImportFormat { get; set; } // CSV, OFX, QIF

        [StringLength(500)]
        public string? ImportSource { get; set; } // File name

        [Required]
        public List<BankStatementItemImportDto> StatementItems { get; set; } = new List<BankStatementItemImportDto>();
    }

    public class BankStatementItemImportDto
    {
        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
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

        public decimal BalanceAfterTransaction { get; set; }
    }

    // Reconciliation DTOs
    public class ReconciliationDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string BankAccountId { get; set; } = string.Empty;
        public string? BankStatementId { get; set; }
        public string ReconciliationName { get; set; } = string.Empty;
        public DateTime ReconciliationDate { get; set; }
        public decimal BookBalance { get; set; }
        public decimal StatementBalance { get; set; }
        public decimal Difference { get; set; }
        public int TotalTransactions { get; set; }
        public int MatchedTransactions { get; set; }
        public int UnmatchedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ReconciliationMatchDto>? Matches { get; set; }
    }

    public class ReconciliationMatchDto
    {
        public string Id { get; set; } = string.Empty;
        public string ReconciliationId { get; set; } = string.Empty;
        public string SystemTransactionId { get; set; } = string.Empty;
        public string SystemTransactionType { get; set; } = string.Empty;
        public string? StatementItemId { get; set; }
        public string MatchType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public string MatchStatus { get; set; } = string.Empty;
        public string? MatchNotes { get; set; }
        public decimal? AmountDifference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? MatchedBy { get; set; }
    }

    public class CreateReconciliationDto
    {
        [Required]
        public string BankAccountId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? BankStatementId { get; set; }

        [Required]
        [StringLength(255)]
        public string ReconciliationName { get; set; } = string.Empty;

        [Required]
        public DateTime ReconciliationDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class MatchTransactionDto
    {
        [Required]
        public string ReconciliationId { get; set; } = string.Empty;

        [Required]
        public string SystemTransactionId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SystemTransactionType { get; set; } = string.Empty; // Payment, BankTransaction

        [StringLength(450)]
        public string? StatementItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string MatchType { get; set; } = "MANUAL"; // AUTO, MANUAL, SUGGESTED

        [StringLength(1000)]
        public string? MatchNotes { get; set; }
    }

    public class UnmatchTransactionDto
    {
        [Required]
        public string MatchId { get; set; } = string.Empty;
    }

    public class CompleteReconciliationDto
    {
        [Required]
        public string ReconciliationId { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class ReconciliationSummaryDto
    {
        public string ReconciliationId { get; set; } = string.Empty;
        public string BankAccountId { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public DateTime ReconciliationDate { get; set; }
        public decimal BookBalance { get; set; }
        public decimal StatementBalance { get; set; }
        public decimal Difference { get; set; }
        public int TotalTransactions { get; set; }
        public int MatchedTransactions { get; set; }
        public int UnmatchedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsBalanced { get; set; }
    }

    public class TransactionMatchSuggestionDto
    {
        public string SystemTransactionId { get; set; } = string.Empty;
        public string SystemTransactionType { get; set; } = string.Empty;
        public string? StatementItemId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public decimal MatchScore { get; set; } // 0-100, higher is better match
        public string MatchReason { get; set; } = string.Empty; // Why this is a good match
    }
}

