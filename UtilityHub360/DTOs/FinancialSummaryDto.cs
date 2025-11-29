namespace UtilityHub360.DTOs
{
    public class FinancialSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        // Current Month Summary
        public MonthlySnapshot CurrentMonth { get; set; } = new();
        
        // Previous Month Summary
        public MonthlySnapshot? PreviousMonth { get; set; }
        
        // Year-to-Date Summary
        public YearlySnapshot YearToDate { get; set; } = new();
        
        // Quick Stats
        public QuickStats Stats { get; set; } = new();
    }

    public class MonthlySnapshot
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal FixedExpenses { get; set; }
        public decimal VariableExpenses { get; set; }
        public decimal DisposableAmount { get; set; }
        public decimal SavingsAmount { get; set; }
        public decimal SavingsRate { get; set; } // Percentage
    }

    public class YearlySnapshot
    {
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalDisposable { get; set; }
        public decimal AverageMonthlyDisposable { get; set; }
        public decimal TotalSavings { get; set; }
        public List<MonthlyDataPoint> MonthlyBreakdown { get; set; } = new();
    }

    public class MonthlyDataPoint
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Disposable { get; set; }
    }

    public class QuickStats
    {
        public decimal AverageMonthlyIncome { get; set; }
        public decimal AverageMonthlyExpenses { get; set; }
        public decimal AverageDisposable { get; set; }
        public string TopExpenseCategory { get; set; } = string.Empty;
        public decimal TopExpenseCategoryAmount { get; set; }
        public int ActiveLoans { get; set; }
        public decimal TotalLoanBalance { get; set; }
        public int PendingBills { get; set; }
        public decimal PendingBillsAmount { get; set; }
    }
}

