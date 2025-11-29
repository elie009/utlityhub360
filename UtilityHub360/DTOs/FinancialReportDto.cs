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
    // BALANCE SHEET DTO
    // ==========================================
    
    public class BalanceSheetDto
    {
        public DateTime AsOfDate { get; set; }
        
        // ASSETS SECTION
        public AssetsSectionDto Assets { get; set; } = new();
        
        // LIABILITIES SECTION
        public LiabilitiesSectionDto Liabilities { get; set; } = new();
        
        // EQUITY SECTION
        public EquitySectionDto Equity { get; set; } = new();
        
        // Validation
        public decimal TotalAssets => Assets.TotalAssets;
        public decimal TotalLiabilitiesAndEquity => Liabilities.TotalLiabilities + Equity.TotalEquity;
        public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;
    }

    public class AssetsSectionDto
    {
        // Current Assets
        public List<BalanceSheetItemDto> CurrentAssets { get; set; } = new();
        public decimal TotalCurrentAssets { get; set; }
        
        // Fixed Assets (if applicable)
        public List<BalanceSheetItemDto> FixedAssets { get; set; } = new();
        public decimal TotalFixedAssets { get; set; }
        
        // Other Assets
        public List<BalanceSheetItemDto> OtherAssets { get; set; } = new();
        public decimal TotalOtherAssets { get; set; }
        
        // Total Assets
        public decimal TotalAssets => TotalCurrentAssets + TotalFixedAssets + TotalOtherAssets;
    }

    public class LiabilitiesSectionDto
    {
        // Current Liabilities
        public List<BalanceSheetItemDto> CurrentLiabilities { get; set; } = new();
        public decimal TotalCurrentLiabilities { get; set; }
        
        // Long-term Liabilities
        public List<BalanceSheetItemDto> LongTermLiabilities { get; set; } = new();
        public decimal TotalLongTermLiabilities { get; set; }
        
        // Total Liabilities
        public decimal TotalLiabilities => TotalCurrentLiabilities + TotalLongTermLiabilities;
    }

    public class EquitySectionDto
    {
        // Owner's Capital
        public decimal OwnersCapital { get; set; }
        
        // Retained Earnings (Net Income - Withdrawals)
        public decimal RetainedEarnings { get; set; }
        
        // Total Equity
        public decimal TotalEquity => OwnersCapital + RetainedEarnings;
    }

    public class BalanceSheetItemDto
    {
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // e.g., "Bank Account", "Savings Account", "Loan"
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? ReferenceId { get; set; } // ID of the related entity (bank account ID, loan ID, etc.)
    }

    // ==========================================
    // CASH FLOW STATEMENT DTO
    // ==========================================
    
    public class CashFlowStatementDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Period { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, YEARLY
        
        // OPERATING ACTIVITIES
        public OperatingActivitiesDto OperatingActivities { get; set; } = new();
        
        // INVESTING ACTIVITIES
        public InvestingActivitiesDto InvestingActivities { get; set; } = new();
        
        // FINANCING ACTIVITIES
        public FinancingActivitiesDto FinancingActivities { get; set; } = new();
        
        // NET CASH FLOW
        public decimal NetCashFlow => OperatingActivities.NetCashFromOperations + 
                                      InvestingActivities.NetCashFromInvesting + 
                                      FinancingActivities.NetCashFromFinancing;
        
        // BEGINNING AND ENDING CASH
        public decimal BeginningCash { get; set; }
        public decimal EndingCash { get; set; }
        
        // Validation
        public bool IsBalanced => Math.Abs(EndingCash - (BeginningCash + NetCashFlow)) < 0.01m;
    }

    public class OperatingActivitiesDto
    {
        // Cash Inflows
        public decimal IncomeReceived { get; set; }
        public decimal OtherOperatingInflows { get; set; }
        public decimal TotalOperatingInflows => IncomeReceived + OtherOperatingInflows;
        
        // Cash Outflows
        public decimal ExpensesPaid { get; set; }
        public decimal BillsPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal OtherOperatingOutflows { get; set; }
        public decimal TotalOperatingOutflows => ExpensesPaid + BillsPaid + InterestPaid + OtherOperatingOutflows;
        
        // Net Cash from Operating Activities
        public decimal NetCashFromOperations => TotalOperatingInflows - TotalOperatingOutflows;
        
        // Details
        public List<CashFlowItemDto> InflowItems { get; set; } = new();
        public List<CashFlowItemDto> OutflowItems { get; set; } = new();
    }

    public class InvestingActivitiesDto
    {
        // Cash Inflows
        public decimal SavingsWithdrawals { get; set; }
        public decimal InvestmentReturns { get; set; }
        public decimal OtherInvestingInflows { get; set; }
        public decimal TotalInvestingInflows => SavingsWithdrawals + InvestmentReturns + OtherInvestingInflows;
        
        // Cash Outflows
        public decimal SavingsDeposits { get; set; }
        public decimal InvestmentsMade { get; set; }
        public decimal OtherInvestingOutflows { get; set; }
        public decimal TotalInvestingOutflows => SavingsDeposits + InvestmentsMade + OtherInvestingOutflows;
        
        // Net Cash from Investing Activities
        public decimal NetCashFromInvesting => TotalInvestingInflows - TotalInvestingOutflows;
        
        // Details
        public List<CashFlowItemDto> InflowItems { get; set; } = new();
        public List<CashFlowItemDto> OutflowItems { get; set; } = new();
    }

    public class FinancingActivitiesDto
    {
        // Cash Inflows
        public decimal LoanDisbursements { get; set; }
        public decimal OtherFinancingInflows { get; set; }
        public decimal TotalFinancingInflows => LoanDisbursements + OtherFinancingInflows;
        
        // Cash Outflows
        public decimal LoanPayments { get; set; }
        public decimal PrincipalPayments { get; set; }
        public decimal OtherFinancingOutflows { get; set; }
        public decimal TotalFinancingOutflows => LoanPayments + OtherFinancingOutflows;
        
        // Net Cash from Financing Activities
        public decimal NetCashFromFinancing => TotalFinancingInflows - TotalFinancingOutflows;
        
        // Details
        public List<CashFlowItemDto> InflowItems { get; set; } = new();
        public List<CashFlowItemDto> OutflowItems { get; set; } = new();
    }

    public class CashFlowItemDto
    {
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; } // "BILL", "LOAN", "SAVINGS", "INCOME", etc.
    }

    // ==========================================
    // INCOME STATEMENT DTO
    // ==========================================
    
    public class IncomeStatementDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Period { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, YEARLY
        
        // REVENUE SECTION
        public RevenueSectionDto Revenue { get; set; } = new();
        
        // EXPENSES SECTION
        public ExpensesSectionDto Expenses { get; set; } = new();
        
        // NET INCOME
        public decimal NetIncome => Revenue.TotalRevenue - Expenses.TotalExpenses;
        
        // Comparison with previous period
        public IncomeStatementComparisonDto? Comparison { get; set; }
    }

    public class RevenueSectionDto
    {
        // Operating Revenue
        public decimal SalaryIncome { get; set; }
        public decimal BusinessIncome { get; set; }
        public decimal FreelanceIncome { get; set; }
        public decimal OtherOperatingRevenue { get; set; }
        public decimal TotalOperatingRevenue { get; set; }
        
        // Other Revenue
        public decimal InvestmentIncome { get; set; }
        public decimal InterestIncome { get; set; }
        public decimal RentalIncome { get; set; }
        public decimal DividendIncome { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal TotalOtherRevenue { get; set; }
        
        // Total Revenue
        public decimal TotalRevenue => TotalOperatingRevenue + TotalOtherRevenue;
        
        // Details
        public List<IncomeStatementItemDto> RevenueItems { get; set; } = new();
    }

    public class ExpensesSectionDto
    {
        // Operating Expenses
        public decimal UtilitiesExpense { get; set; }
        public decimal RentExpense { get; set; }
        public decimal InsuranceExpense { get; set; }
        public decimal SubscriptionExpense { get; set; }
        public decimal FoodExpense { get; set; }
        public decimal TransportationExpense { get; set; }
        public decimal HealthcareExpense { get; set; }
        public decimal EducationExpense { get; set; }
        public decimal EntertainmentExpense { get; set; }
        public decimal OtherOperatingExpenses { get; set; }
        public decimal TotalOperatingExpenses { get; set; }
        
        // Financial Expenses
        public decimal InterestExpense { get; set; }
        public decimal LoanFeesExpense { get; set; }
        public decimal TotalFinancialExpenses { get; set; }
        
        // Total Expenses
        public decimal TotalExpenses => TotalOperatingExpenses + TotalFinancialExpenses;
        
        // Details
        public List<IncomeStatementItemDto> ExpenseItems { get; set; } = new();
    }

    public class IncomeStatementItemDto
    {
        public string AccountName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
    }

    public class IncomeStatementComparisonDto
    {
        public decimal PreviousRevenue { get; set; }
        public decimal PreviousExpenses { get; set; }
        public decimal PreviousNetIncome { get; set; }
        public decimal RevenueChange { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public decimal ExpensesChange { get; set; }
        public decimal ExpensesChangePercentage { get; set; }
        public decimal NetIncomeChange { get; set; }
        public decimal NetIncomeChangePercentage { get; set; }
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
        public DateTime? StartDate { get; set; }
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

    public class CashFlowProjectionDto
    {
        public DateTime ProjectionDate { get; set; }
        public int MonthsAhead { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal ProjectedIncome { get; set; }
        public decimal ProjectedExpenses { get; set; }
        public decimal ProjectedBills { get; set; }
        public decimal ProjectedLoanPayments { get; set; }
        public decimal ProjectedSavings { get; set; }
        public decimal ProjectedEndingBalance { get; set; }
        public decimal NetCashFlow { get; set; }
        public List<MonthlyCashFlowProjectionDto> MonthlyBreakdown { get; set; } = new();
    }

    public class MonthlyCashFlowProjectionDto
    {
        public DateTime Month { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Bills { get; set; }
        public decimal LoanPayments { get; set; }
        public decimal Savings { get; set; }
        public decimal EndingBalance { get; set; }
        public decimal NetFlow { get; set; }
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

    // ==========================================
    // FINANCIAL RATIOS DTO
    // ==========================================
    
    public class FinancialRatiosDto
    {
        public DateTime AsOfDate { get; set; }
        
        // LIQUIDITY RATIOS
        public LiquidityRatiosDto Liquidity { get; set; } = new();
        
        // DEBT RATIOS
        public DebtRatiosDto Debt { get; set; } = new();
        
        // PROFITABILITY RATIOS
        public ProfitabilityRatiosDto Profitability { get; set; } = new();
        
        // EFFICIENCY RATIOS
        public EfficiencyRatiosDto Efficiency { get; set; } = new();
        
        // INTERPRETATION & RECOMMENDATIONS
        public List<RatioInsightDto> Insights { get; set; } = new();
    }

    public class LiquidityRatiosDto
    {
        // Current Ratio = Current Assets / Current Liabilities
        public decimal CurrentRatio { get; set; }
        public string CurrentRatioInterpretation { get; set; } = string.Empty; // "Good", "Fair", "Poor"
        
        // Quick Ratio = (Current Assets - Inventory) / Current Liabilities
        // For personal finance, we'll use: (Current Assets - Savings) / Current Liabilities
        public decimal QuickRatio { get; set; }
        public string QuickRatioInterpretation { get; set; } = string.Empty;
        
        // Cash Ratio = Cash / Current Liabilities
        public decimal CashRatio { get; set; }
        public string CashRatioInterpretation { get; set; } = string.Empty;
        
        // Supporting Data
        public decimal CurrentAssets { get; set; }
        public decimal CurrentLiabilities { get; set; }
        public decimal CashAndEquivalents { get; set; }
    }

    public class DebtRatiosDto
    {
        // Debt-to-Equity Ratio = Total Liabilities / Total Equity
        public decimal DebtToEquityRatio { get; set; }
        public string DebtToEquityInterpretation { get; set; } = string.Empty;
        
        // Debt-to-Assets Ratio = Total Liabilities / Total Assets
        public decimal DebtToAssetsRatio { get; set; }
        public string DebtToAssetsInterpretation { get; set; } = string.Empty;
        
        // Debt Service Coverage Ratio = Net Income / Total Debt Payments
        public decimal DebtServiceCoverageRatio { get; set; }
        public string DebtServiceCoverageInterpretation { get; set; } = string.Empty;
        
        // Supporting Data
        public decimal TotalLiabilities { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal NetIncome { get; set; }
        public decimal TotalDebtPayments { get; set; }
    }

    public class ProfitabilityRatiosDto
    {
        // Net Profit Margin = (Net Income / Total Revenue) × 100
        public decimal NetProfitMargin { get; set; } // Percentage
        public string NetProfitMarginInterpretation { get; set; } = string.Empty;
        
        // Return on Assets (ROA) = (Net Income / Total Assets) × 100
        public decimal ReturnOnAssets { get; set; } // Percentage
        public string ReturnOnAssetsInterpretation { get; set; } = string.Empty;
        
        // Return on Equity (ROE) = (Net Income / Total Equity) × 100
        public decimal ReturnOnEquity { get; set; } // Percentage
        public string ReturnOnEquityInterpretation { get; set; } = string.Empty;
        
        // Supporting Data
        public decimal NetIncome { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalEquity { get; set; }
    }

    public class EfficiencyRatiosDto
    {
        // Asset Turnover = Total Revenue / Total Assets
        public decimal AssetTurnover { get; set; }
        public string AssetTurnoverInterpretation { get; set; } = string.Empty;
        
        // Expense Ratio = Total Expenses / Total Revenue
        public decimal ExpenseRatio { get; set; } // Percentage
        public string ExpenseRatioInterpretation { get; set; } = string.Empty;
        
        // Savings Rate = (Savings / Total Income) × 100
        public decimal SavingsRate { get; set; } // Percentage
        public string SavingsRateInterpretation { get; set; } = string.Empty;
        
        // Supporting Data
        public decimal TotalRevenue { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalIncome { get; set; }
    }

    public class RatioInsightDto
    {
        public string RatioName { get; set; } = string.Empty;
        public decimal RatioValue { get; set; }
        public string Category { get; set; } = string.Empty; // "LIQUIDITY", "DEBT", "PROFITABILITY", "EFFICIENCY"
        public string Interpretation { get; set; } = string.Empty; // "Good", "Fair", "Poor", "Excellent"
        public string Recommendation { get; set; } = string.Empty;
        public string Severity { get; set; } = "INFO"; // "INFO", "WARNING", "CRITICAL", "SUCCESS"
    }

    // ==========================================
    // TAX REPORTING DTO
    // ==========================================
    
    public class TaxReportDto
    {
        public int TaxYear { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        
        // INCOME SUMMARY
        public TaxIncomeSummaryDto IncomeSummary { get; set; } = new();
        
        // DEDUCTIBLE EXPENSES
        public TaxDeductionsDto Deductions { get; set; } = new();
        
        // TAX CALCULATION
        public TaxCalculationDto TaxCalculation { get; set; } = new();
        
        // TAX CATEGORIES BREAKDOWN
        public List<TaxCategoryDto> TaxCategories { get; set; } = new();
        
        // QUARTERLY BREAKDOWN (if applicable)
        public List<TaxQuarterlySummaryDto> QuarterlyBreakdown { get; set; } = new();
    }

    public class TaxIncomeSummaryDto
    {
        // Total Income
        public decimal TotalIncome { get; set; }
        
        // Income by Type
        public decimal SalaryIncome { get; set; }
        public decimal BusinessIncome { get; set; }
        public decimal FreelanceIncome { get; set; }
        public decimal InvestmentIncome { get; set; }
        public decimal InterestIncome { get; set; }
        public decimal RentalIncome { get; set; }
        public decimal DividendIncome { get; set; }
        public decimal OtherIncome { get; set; }
        
        // Taxable Income (after adjustments)
        public decimal TaxableIncome { get; set; }
        
        // Details
        public List<TaxIncomeItemDto> IncomeItems { get; set; } = new();
    }

    public class TaxDeductionsDto
    {
        // Business Expenses
        public decimal BusinessExpenses { get; set; }
        public List<TaxDeductionItemDto> BusinessExpenseItems { get; set; } = new();
        
        // Personal Deductions
        public decimal PersonalDeductions { get; set; }
        public List<TaxDeductionItemDto> PersonalDeductionItems { get; set; } = new();
        
        // Standard Deductions (if applicable)
        public decimal StandardDeduction { get; set; }
        
        // Total Deductions
        public decimal TotalDeductions => BusinessExpenses + PersonalDeductions + StandardDeduction;
        
        // Deduction Categories
        public Dictionary<string, decimal> DeductionsByCategory { get; set; } = new();
    }

    public class TaxCalculationDto
    {
        // Adjusted Gross Income (AGI)
        public decimal AdjustedGrossIncome { get; set; }
        
        // Taxable Income
        public decimal TaxableIncome { get; set; }
        
        // Estimated Tax Liability (simplified calculation)
        public decimal EstimatedTaxLiability { get; set; }
        
        // Tax Brackets (if applicable)
        public List<TaxBracketDto> TaxBrackets { get; set; } = new();
        
        // Effective Tax Rate
        public decimal EffectiveTaxRate { get; set; } // Percentage
        
        // Marginal Tax Rate
        public decimal MarginalTaxRate { get; set; } // Percentage
        
        // Notes
        public string Notes { get; set; } = string.Empty;
    }

    public class TaxCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty; // "Business Expenses", "Personal Deductions", "Investment Income", etc.
        public string CategoryType { get; set; } = string.Empty; // "INCOME", "DEDUCTION", "EXPENSE"
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; } // Percentage of total
        public string Description { get; set; } = string.Empty;
        public List<TaxCategoryItemDto> Items { get; set; } = new();
    }

    public class TaxIncomeItemDto
    {
        public string SourceName { get; set; } = string.Empty;
        public string IncomeType { get; set; } = string.Empty; // "SALARY", "BUSINESS", "FREELANCE", "INVESTMENT", etc.
        public decimal Amount { get; set; }
        public DateTime IncomeDate { get; set; }
        public bool IsTaxable { get; set; } = true;
        public string? ReferenceId { get; set; }
    }

    public class TaxDeductionItemDto
    {
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "UTILITIES", "RENT", "INSURANCE", "INTEREST", etc.
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public bool IsDeductible { get; set; } = true;
        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; } // "BILL", "LOAN", "EXPENSE", etc.
    }

    public class TaxBracketDto
    {
        public decimal MinIncome { get; set; }
        public decimal MaxIncome { get; set; }
        public decimal TaxRate { get; set; } // Percentage
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class TaxCategoryItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? ReferenceId { get; set; }
    }

    public class TaxQuarterlySummaryDto
    {
        public int Quarter { get; set; } // 1, 2, 3, 4
        public DateTime QuarterStart { get; set; }
        public DateTime QuarterEnd { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal EstimatedTaxLiability { get; set; }
    }

    // ==========================================
    // BUDGET VS ACTUAL REPORT DTO
    // ==========================================
    
    public class BudgetVsActualReportDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Period { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, YEARLY
        
        // Summary
        public decimal TotalBudget { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal TotalVariancePercentage { get; set; }
        
        // Breakdown by Category
        public List<BudgetVsActualCategoryDto> Categories { get; set; } = new();
        
        // Breakdown by Bill Provider
        public List<BudgetVsActualBillDto> Bills { get; set; } = new();
        
        // Overall Status
        public string OverallStatus { get; set; } = string.Empty; // "ON_TRACK", "OVER_BUDGET", "UNDER_BUDGET"
        public List<string> Alerts { get; set; } = new();
    }

    public class BudgetVsActualCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty; // "BILL", "EXPENSE", "LOAN", etc.
        public decimal BudgetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Status { get; set; } = string.Empty; // "ON_TRACK", "OVER_BUDGET", "UNDER_BUDGET"
        public List<BudgetVsActualItemDto> Items { get; set; } = new();
    }

    public class BudgetVsActualBillDto
    {
        public string Provider { get; set; } = string.Empty;
        public string BillType { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public int BillCount { get; set; }
    }

    public class BudgetVsActualItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public DateTime? Date { get; set; }
    }

    // ==========================================
    // CUSTOM REPORT BUILDER DTOs
    // ==========================================
    
    public class CustomReportRequestDto
    {
        [Required]
        public string ReportName { get; set; } = string.Empty;
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Report Sections to Include
        public bool IncludeIncome { get; set; } = true;
        public bool IncludeExpenses { get; set; } = true;
        public bool IncludeBills { get; set; } = true;
        public bool IncludeLoans { get; set; } = true;
        public bool IncludeSavings { get; set; } = true;
        public bool IncludeNetWorth { get; set; } = true;
        public bool IncludeBalanceSheet { get; set; } = false;
        public bool IncludeIncomeStatement { get; set; } = false;
        public bool IncludeCashFlowStatement { get; set; } = false;
        public bool IncludeBudgetVsActual { get; set; } = false;
        public bool IncludeTaxReport { get; set; } = false;
        
        // Filters
        public List<string>? CategoryFilters { get; set; }
        public List<string>? AccountFilters { get; set; }
        public List<string>? BillTypeFilters { get; set; }
        
        // Grouping
        public string GroupBy { get; set; } = "NONE"; // "NONE", "CATEGORY", "ACCOUNT", "MONTH", "QUARTER", "YEAR"
        
        // Comparison
        public bool IncludeComparison { get; set; } = false;
        public DateTime? ComparisonStartDate { get; set; }
        public DateTime? ComparisonEndDate { get; set; }
        
        // Formatting
        public string CurrencyFormat { get; set; } = "USD";
        public string DateFormat { get; set; } = "MM/DD/YYYY";
    }

    public class CustomReportDto
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string ReportName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        
        // Report Data
        public Dictionary<string, object> ReportData { get; set; } = new();
        
        // Summary
        public CustomReportSummaryDto Summary { get; set; } = new();
        
        // Sections (only included sections will be populated)
        public IncomeReportDto? IncomeReport { get; set; }
        public ExpenseReportDto? ExpenseReport { get; set; }
        public BillsReportDto? BillsReport { get; set; }
        public LoanReportDto? LoanReport { get; set; }
        public SavingsReportDto? SavingsReport { get; set; }
        public NetWorthReportDto? NetWorthReport { get; set; }
        public BalanceSheetDto? BalanceSheet { get; set; }
        public IncomeStatementDto? IncomeStatement { get; set; }
        public CashFlowStatementDto? CashFlowStatement { get; set; }
        public BudgetVsActualReportDto? BudgetVsActual { get; set; }
        public TaxReportDto? TaxReport { get; set; }
        
        // Comparison Data
        public Dictionary<string, object>? ComparisonData { get; set; }
    }

    public class CustomReportSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal NetWorth { get; set; }
        public int TransactionCount { get; set; }
        public int BillCount { get; set; }
        public int LoanCount { get; set; }
    }

    public class SaveCustomReportTemplateDto
    {
        [Required]
        public string TemplateName { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public CustomReportRequestDto ReportConfig { get; set; } = new();
    }

    public class CustomReportTemplateDto
    {
        public string TemplateId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CustomReportRequestDto ReportConfig { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

