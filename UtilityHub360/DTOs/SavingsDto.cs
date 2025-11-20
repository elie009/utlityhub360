using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class CreateSavingsAccountDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SavingsType { get; set; } = string.Empty; // EMERGENCY, VACATION, INVESTMENT, etc.

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public decimal TargetAmount { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Goal cannot exceed 100 characters")]
        public string? Goal { get; set; }

        [Required]
        public DateTime TargetDate { get; set; }

        public DateTime? StartDate { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";
    }

    public class SavingsAccountDto
    {
        public string Id { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string SavingsType { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Goal { get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal ProgressPercentage { get; set; }
        public decimal RemainingAmount { get; set; }
        public int DaysRemaining { get; set; }
        public decimal MonthlyTarget { get; set; }
    }

    public class CreateSavingsTransactionDto
    {
        [Required]
        [StringLength(450)]
        public string SavingsAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string SourceBankAccountId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = string.Empty; // DEPOSIT, WITHDRAWAL, TRANSFER

        [Required]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string? RecurringFrequency { get; set; }

        [StringLength(50)]
        public string? Method { get; set; } = "CASH"; // BANK_TRANSFER, CASH, etc.
    }

    public class SavingsTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string SavingsAccountId { get; set; } = string.Empty;
        public string SourceBankAccountId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SavingsSummaryDto
    {
        public int TotalSavingsAccounts { get; set; }
        public decimal TotalSavingsBalance { get; set; }
        public decimal TotalTargetAmount { get; set; }
        public decimal OverallProgressPercentage { get; set; }
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public decimal MonthlySavingsTarget { get; set; }
        public decimal ThisMonthSaved { get; set; }
        public List<SavingsAccountDto> RecentAccounts { get; set; } = new List<SavingsAccountDto>();
    }

    public class SavingsAnalyticsDto
    {
        public decimal TotalSaved { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public decimal NetSavings { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public Dictionary<string, decimal> SavingsByType { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> SavingsByCategory { get; set; } = new Dictionary<string, decimal>();
        public List<SavingsTransactionDto> RecentTransactions { get; set; } = new List<SavingsTransactionDto>();
        public decimal AverageMonthlySavings { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    public class AutoSaveSettingsDto
    {
        [Required]
        [StringLength(450)]
        public string SavingsAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string SourceBankAccountId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Frequency { get; set; } = string.Empty; // MONTHLY, WEEKLY, DAILY

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
