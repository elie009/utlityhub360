namespace UtilityHub360.DTOs
{
    public class SimpleFinancialSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        
        // Income
        public decimal TotalIncome { get; set; }
        public int IncomeSourcesCount { get; set; }
        
        // Expenses
        public decimal TotalBills { get; set; }
        public int BillsCount { get; set; }
        public decimal TotalLoans { get; set; }
        public int ActiveLoansCount { get; set; }
        public decimal TotalExpenses { get; set; } // Bills + Loans
        
        // Savings
        public decimal TotalSavings { get; set; }
        public int SavingsAccountsCount { get; set; }
        
        // Remaining Amount
        public decimal RemainingAmount { get; set; } // Income - Expenses - Savings
        public decimal RemainingPercentage { get; set; }
        
        // Status
        public string FinancialStatus { get; set; } = string.Empty; // HEALTHY, WARNING, CRITICAL
    }
}

