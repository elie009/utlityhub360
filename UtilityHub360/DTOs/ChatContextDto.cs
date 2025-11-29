namespace UtilityHub360.DTOs
{
    public class ChatContextDto
    {
        public ChatFinancialSummaryDto FinancialSummary { get; set; } = new ChatFinancialSummaryDto();
        public List<BillDto> UpcomingBills { get; set; } = new List<BillDto>();
        public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
        public List<LoanDto> ActiveLoans { get; set; } = new List<LoanDto>();
        public List<SavingsAccountDto> SavingsAccounts { get; set; } = new List<SavingsAccountDto>();
        public List<VariableExpenseDto> RecentExpenses { get; set; } = new List<VariableExpenseDto>();
        public DateTime ContextDate { get; set; } = DateTime.UtcNow;
        public string ContextPeriod { get; set; } = "Last 30 days";
    }

    public class ChatFinancialSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal DisposableAmount { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal NetWorth { get; set; }
        public string Currency { get; set; } = "USD";
    }
}
