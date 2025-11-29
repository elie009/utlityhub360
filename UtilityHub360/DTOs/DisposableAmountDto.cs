namespace UtilityHub360.DTOs
{
    public class DisposableAmountDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty; // "MONTHLY", "YEARLY", "CUSTOM"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Income
        public decimal TotalIncome { get; set; }
        public List<IncomeBreakdown> IncomeBreakdown { get; set; } = new();
        
        // Fixed Expenses
        public decimal TotalFixedExpenses { get; set; }
        public decimal TotalBills { get; set; }
        public decimal TotalLoans { get; set; }
        public List<ExpenseDetail> BillsBreakdown { get; set; } = new();
        public List<ExpenseDetail> LoansBreakdown { get; set; } = new();
        
        // Variable Expenses
        public decimal TotalVariableExpenses { get; set; }
        public List<VariableExpenseBreakdown> VariableExpensesBreakdown { get; set; } = new();
        
        // Disposable Amount
        public decimal DisposableAmount { get; set; }
        public decimal DisposablePercentage { get; set; }
        
        // Optional: Savings Goals
        public decimal? TargetSavings { get; set; }
        public decimal? InvestmentAllocation { get; set; }
        public decimal? NetDisposableAmount { get; set; }
        
        // Spending Tracking
        public decimal ActualSpending { get; set; } // Actual spending from transactions
        public decimal SpendingPercentage { get; set; } // Percentage of disposable amount spent
        public decimal RemainingPercentage { get; set; } // Percentage of disposable amount remaining
        public decimal? ExceededPercentage { get; set; } // Percentage exceeded (if spending > disposable)
        public bool IsExceeded { get; set; } // Whether spending exceeds disposable amount
        public decimal? RemainingDisposableAmount { get; set; } // Actual remaining disposable amount
        
        // Insights
        public List<string> Insights { get; set; } = new();
        public ComparisonData? Comparison { get; set; }
    }

    public class IncomeBreakdown
    {
        public string SourceName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal MonthlyAmount { get; set; }
        public string Frequency { get; set; } = string.Empty;
    }

    public class ExpenseDetail
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }

    public class VariableExpenseBreakdown
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ComparisonData
    {
        public decimal? PreviousPeriodDisposableAmount { get; set; }
        public decimal? ChangeAmount { get; set; }
        public decimal? ChangePercentage { get; set; }
        public string Trend { get; set; } = string.Empty; // "UP", "DOWN", "STABLE"
    }
}

