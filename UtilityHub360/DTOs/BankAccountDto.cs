using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class BankAccountDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? FinancialInstitution { get; set; }
        public string? AccountNumber { get; set; }
        public string? RoutingNumber { get; set; }
        public string SyncFrequency { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public string? ConnectionId { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? Iban { get; set; }
        public string? SwiftCode { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
    }

    public class CreateBankAccountDto
    {
        [Required]
        [StringLength(255, ErrorMessage = "Account name cannot exceed 255 characters")]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Initial balance must be 0 or greater")]
        public decimal InitialBalance { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Financial institution name cannot exceed 100 characters")]
        public string? FinancialInstitution { get; set; }

        [StringLength(255, ErrorMessage = "Account number cannot exceed 255 characters")]
        public string? AccountNumber { get; set; }

        [StringLength(100, ErrorMessage = "Routing number cannot exceed 100 characters")]
        public string? RoutingNumber { get; set; }

        [StringLength(50)]
        public string SyncFrequency { get; set; } = "MANUAL";

        [StringLength(100, ErrorMessage = "IBAN cannot exceed 100 characters")]
        public string? Iban { get; set; }

        [StringLength(100, ErrorMessage = "SWIFT code cannot exceed 100 characters")]
        public string? SwiftCode { get; set; }
    }

    public class UpdateBankAccountDto
    {
        [StringLength(255, ErrorMessage = "Account name cannot exceed 255 characters")]
        public string? AccountName { get; set; }

        [StringLength(50)]
        public string? AccountType { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Current balance must be 0 or greater")]
        public decimal? CurrentBalance { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Financial institution name cannot exceed 100 characters")]
        public string? FinancialInstitution { get; set; }

        [StringLength(255, ErrorMessage = "Account number cannot exceed 255 characters")]
        public string? AccountNumber { get; set; }

        [StringLength(100, ErrorMessage = "Routing number cannot exceed 100 characters")]
        public string? RoutingNumber { get; set; }

        [StringLength(50)]
        public string? SyncFrequency { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(100, ErrorMessage = "IBAN cannot exceed 100 characters")]
        public string? Iban { get; set; }

        [StringLength(100, ErrorMessage = "SWIFT code cannot exceed 100 characters")]
        public string? SwiftCode { get; set; }
    }

    public class BankTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string BankAccountId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? ExternalTransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public string? Merchant { get; set; }
        public string? Location { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal BalanceAfterTransaction { get; set; }
    }

    public class CreateBankTransactionDto
    {
        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(100)]
        public string? ExternalTransactionId { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Merchant { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string? RecurringFrequency { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        // Optional references for bills, savings, or other purposes
        [StringLength(450)]
        public string? BillId { get; set; } // Reference to bill if category is bill-related

        [StringLength(450)]
        public string? SavingsAccountId { get; set; } // Reference to savings account if category is savings-related

        [StringLength(450)]
        public string? LoanId { get; set; } // Reference to loan if category is loan-related
    }

    public class BankAccountSummaryDto
    {
        public decimal TotalBalance { get; set; }
        public decimal? TotalRemainingCreditLimit { get; set; }
        public int TotalAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int ConnectedAccounts { get; set; }
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public decimal CurrentMonthIncoming { get; set; }
        public decimal CurrentMonthOutgoing { get; set; }
        public decimal CurrentMonthNet { get; set; }
        public string Frequency { get; set; } = "monthly";
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TransactionCount { get; set; }
        public List<BankAccountDto> Accounts { get; set; } = new List<BankAccountDto>();
        public Dictionary<string, decimal> SpendingByCategory { get; set; } = new Dictionary<string, decimal>();
    }

    public class BankIntegrationDto
    {
        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FinancialInstitution { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string AuthToken { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ConnectionId { get; set; }
    }

    public class SyncBankAccountDto
    {
        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        public bool ForceSync { get; set; } = false;
    }

    public class BankAccountAnalyticsDto
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<BankAccountDto> TopAccounts { get; set; } = new List<BankAccountDto>();
        public Dictionary<string, decimal> SpendingByCategory { get; set; } = new Dictionary<string, decimal>();
    }

    public class CreateExpenseDto
    {
        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // FOOD, TRANSPORTATION, etc.

        [Required]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Merchant { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string? RecurringFrequency { get; set; }
    }

    public class ExpenseAnalyticsDto
    {
        public decimal TotalExpenses { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public Dictionary<string, decimal> SpendingByCategory { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> SpendingByMerchant { get; set; } = new Dictionary<string, decimal>();
        public List<BankTransactionDto> RecentExpenses { get; set; } = new List<BankTransactionDto>();
        public decimal AverageDailySpending { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    public class ExpenseSummaryDto
    {
        public decimal TodayExpenses { get; set; }
        public decimal ThisWeekExpenses { get; set; }
        public decimal ThisMonthExpenses { get; set; }
        public decimal LastMonthExpenses { get; set; }
        public Dictionary<string, decimal> TopCategories { get; set; } = new Dictionary<string, decimal>();
        public List<BankTransactionDto> RecentExpenses { get; set; } = new List<BankTransactionDto>();
    }
}
