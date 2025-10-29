using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // ==========================================
    // MAIN FINANCIAL REPORT DTO
    // ==========================================
    
    public class FinancialReportDto
    {
        public DateTime ReportDate { get; set; }
        public string Period { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, YEARLY
        
        // Summary Cards
        public ReportFinancialSummaryDto Summary { get; set; } = new();
        
        // Detailed Reports
        public IncomeReportDto IncomeReport { get; set; } = new();
        public ExpenseReportDto ExpenseReport { get; set; } = new();
        public DisposableIncomeReportDto DisposableIncomeReport { get; set; } = new();
        public BillsReportDto BillsReport { get; set; } = new();
        public LoanReportDto LoanReport { get; set; } = new();
        public SavingsReportDto SavingsReport { get; set; } = new();
        public NetWorthReportDto NetWorthReport { get; set; } = new();
        
        // Insights & Predictions
        public List<FinancialInsightDto> Insights { get; set; } = new();
        public List<FinancialPredictionDto> Predictions { get; set; } = new();
        
        // Recent Transactions
        public List<TransactionLogDto> RecentTransactions { get; set; } = new();
    }

    // ==========================================
    // FULL FINANCIAL REPORT DTO (with date range)
    // ==========================================
    
    public class FullFinancialReportDto
    {
        public DateTime ReportDate { get; set; }
        public string Period { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, YEARLY
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Summary Cards
        public ReportFinancialSummaryDto? Summary { get; set; }
        
        // Detailed Reports
        public IncomeReportDto? IncomeReport { get; set; }
        public ExpenseReportDto? ExpenseReport { get; set; }
        public DisposableIncomeReportDto? DisposableIncomeReport { get; set; }
        public BillsReportDto? BillsReport { get; set; }
        public LoanReportDto? LoanReport { get; set; }
        public SavingsReportDto? SavingsReport { get; set; }
        public NetWorthReportDto? NetWorthReport { get; set; }
        
        // Insights & Predictions
        public List<FinancialInsightDto>? Insights { get; set; }
        public List<FinancialPredictionDto>? Predictions { get; set; }
        
        // Recent Transactions
        public List<TransactionLogDto>? RecentTransactions { get; set; }
    }

    // ==========================================
    // SUMMARY CARDS DTO
    // ==========================================
    
    public class ReportFinancialSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal IncomeChange { get; set; } // Percentage change from last period
        
        public decimal TotalExpenses { get; set; }
        public decimal ExpenseChange { get; set; }
        
        public decimal DisposableIncome { get; set; }
        public decimal DisposableChange { get; set; }
        
        public decimal TotalSavings { get; set; }
        public decimal SavingsGoal { get; set; }
        public decimal SavingsProgress { get; set; } // Percentage
        
        public decimal NetWorth { get; set; }
        public decimal NetWorthChange { get; set; }
    }

    // ==========================================
    // INCOME REPORT DTO
    // ==========================================
    
    public class IncomeReportDto
    {
        public decimal TotalIncome { get; set; }
        public decimal MonthlyAverage { get; set; }
        public decimal GrowthRate { get; set; } // Percentage
        
        public Dictionary<string, decimal> IncomeBySource { get; set; } = new();
        public Dictionary<string, decimal> IncomeByCategory { get; set; } = new();
        
        public List<TrendDataPoint> IncomeTrend { get; set; } = new();
        
        public string TopIncomeSource { get; set; } = string.Empty;
        public decimal TopIncomeAmount { get; set; }
    }

    // ==========================================
    // EXPENSE REPORT DTO
    // ==========================================
    
    public class ExpenseReportDto
    {
        public decimal TotalExpenses { get; set; }
        public decimal FixedExpenses { get; set; }
        public decimal VariableExpenses { get; set; }
        
        public Dictionary<string, decimal> ExpenseByCategory { get; set; } = new();
        public Dictionary<string, decimal> ExpensePercentage { get; set; } = new();
        
        public List<TrendDataPoint> ExpenseTrend { get; set; } = new();
        
        public string HighestExpenseCategory { get; set; } = string.Empty;
        public decimal HighestExpenseAmount { get; set; }
        public decimal HighestExpensePercentage { get; set; }
        
        public decimal AverageMonthlyExpense { get; set; }
        public List<ExpenseComparisonDto> CategoryComparison { get; set; } = new();
    }

    // ==========================================
    // DISPOSABLE INCOME REPORT DTO
    // ==========================================
    
    public class DisposableIncomeReportDto
    {
        public decimal CurrentDisposableIncome { get; set; }
        public decimal AverageDisposableIncome { get; set; }
        public decimal DisposableIncomeChange { get; set; }
        
        public List<TrendDataPoint> DisposableIncomeTrend { get; set; } = new();
        
        public decimal RecommendedSavingsAmount { get; set; } // 30% of disposable
        public decimal RecommendedSavingsPercentage { get; set; } = 30;
    }

    // ==========================================
    // BILLS & UTILITIES REPORT DTO
    // ==========================================
    
    public class BillsReportDto
    {
        public decimal TotalMonthlyBills { get; set; }
        public decimal AverageMonthlyBills { get; set; }
        
        public Dictionary<string, decimal> BillsByType { get; set; } = new();
        public Dictionary<string, decimal> BillsByProvider { get; set; } = new();
        
        public List<BillComparisonDto> MonthlyComparison { get; set; } = new();
        public List<TrendDataPoint> BillsTrend { get; set; } = new();
        
        public decimal PredictedNextMonthTotal { get; set; }
        public int UnpaidBillsCount { get; set; }
        public int OverdueBillsCount { get; set; }
        public List<UpcomingBillDto> UpcomingBills { get; set; } = new();
    }

    // ==========================================
    // LOAN & DEBT REPORT DTO
    // ==========================================
    
    public class LoanReportDto
    {
        public int ActiveLoansCount { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalRemainingBalance { get; set; }
        public decimal TotalMonthlyPayment { get; set; }
        public decimal TotalInterestPaid { get; set; }
        
        public List<LoanDetailDto> ActiveLoans { get; set; } = new();
        public List<TrendDataPoint> RepaymentTrend { get; set; } = new();
        
        public DateTime? ProjectedDebtFreeDate { get; set; }
        public int MonthsUntilDebtFree { get; set; }
    }

    // ==========================================
    // SAVINGS & GOAL PROGRESS REPORT DTO
    // ==========================================
    
    public class SavingsReportDto
    {
        public decimal TotalSavings { get; set; }
        public decimal MonthlySavings { get; set; }
        public decimal SavingsGoal { get; set; }
        public decimal GoalProgress { get; set; } // Percentage
        
        public List<SavingsGoalDto> Goals { get; set; } = new();
        public List<TrendDataPoint> SavingsTrend { get; set; } = new();
        
        public decimal SavingsRate { get; set; } // Percentage of income saved
        public DateTime? ProjectedGoalDate { get; set; }
        public int MonthsToGoal { get; set; }
        
        public decimal SuggestionIncrease { get; set; }
        public int SuggestionMonthsSaved { get; set; }
    }

    // ==========================================
    // NET WORTH REPORT DTO
    // ==========================================
    
    public class NetWorthReportDto
    {
        public decimal CurrentNetWorth { get; set; }
        public decimal NetWorthChange { get; set; } // Amount
        public decimal NetWorthChangePercentage { get; set; }
        
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        
        public Dictionary<string, decimal> AssetBreakdown { get; set; } = new();
        public Dictionary<string, decimal> LiabilityBreakdown { get; set; } = new();
        
        public List<TrendDataPoint> NetWorthTrend { get; set; } = new();
        
        public string TrendDescription { get; set; } = string.Empty;
    }

    // ==========================================
    // SUPPORTING DTOs
    // ==========================================
    
    public class TrendDataPoint
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty; // "Jan 2025", "Q1 2025", etc.
        public decimal Value { get; set; }
        public decimal? ComparisonValue { get; set; } // For comparing two lines
    }

    // Alias for TrendDataPoint (for backwards compatibility)
    public class TrendDataDto : TrendDataPoint
    {
    }

    public class ExpenseComparisonDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal PreviousAmount { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercentage { get; set; }
    }

    public class BillComparisonDto
    {
        public string BillType { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal PreviousAmount { get; set; }
        public decimal Change { get; set; }
        public string ChangeDescription { get; set; } = string.Empty;
    }

    public class UpcomingBillDto
    {
        public string BillName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LoanDetailDto
    {
        public string LoanId { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public decimal Principal { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal InterestRate { get; set; }
        public int RepaymentProgress { get; set; } // Percentage
    }

    public class SavingsGoalDto
    {
        public string GoalName { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal Progress { get; set; } // Percentage
        public DateTime? TargetDate { get; set; }
    }

    public class TransactionLogDto
    {
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // CREDIT, DEBIT
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
    }

    // ==========================================
    // INSIGHTS & PREDICTIONS DTOs
    // ==========================================
    
    public class FinancialInsightDto
    {
        public string Type { get; set; } = string.Empty; // ALERT, TIP, FORECAST, INFO
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public decimal? Percentage { get; set; }
        public string Severity { get; set; } = "INFO"; // INFO, WARNING, CRITICAL
        public string Icon { get; set; } = "ℹ️";
    }

    public class FinancialPredictionDto
    {
        public string Type { get; set; } = string.Empty; // INCOME, EXPENSE, SAVINGS, BILL
        public string Description { get; set; } = string.Empty;
        public decimal PredictedAmount { get; set; }
        public DateTime PredictionDate { get; set; }
        public decimal Confidence { get; set; } // 0-100 percentage
    }

    // ==========================================
    // REPORT QUERY/REQUEST DTOs
    // ==========================================
    
    public class ReportQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        [Required]
        public string Period { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, YEARLY, CUSTOM
        
        public bool IncludeComparison { get; set; } = true;
        public bool IncludeInsights { get; set; } = true;
        public bool IncludePredictions { get; set; } = true;
        public bool IncludeTransactions { get; set; } = true;
    }

    public class ExportReportDto
    {
        [Required]
        public string Format { get; set; } = "PDF"; // PDF, CSV, EXCEL
        
        [Required]
        public string ReportType { get; set; } = "FULL"; // FULL, INCOME, EXPENSE, BILLS, LOANS, SAVINGS, NETWORTH
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

