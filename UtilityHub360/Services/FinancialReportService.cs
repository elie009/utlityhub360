using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using System.Text;

namespace UtilityHub360.Services
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly ApplicationDbContext _context;

        public FinancialReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // MAIN REPORT GENERATION
        // ==========================================

        public async Task<ApiResponse<FinancialReportDto>> GenerateFullReportAsync(string userId, ReportQueryDto query)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(query);

                var report = new FinancialReportDto
                {
                    ReportDate = DateTime.UtcNow,
                    Period = query.Period
                };

                // Generate all report sections
                var summaryResult = await GetFinancialSummaryAsync(userId, endDate);
                if (summaryResult.Success) report.Summary = summaryResult.Data!;

                var incomeResult = await GetIncomeReportAsync(userId, query);
                if (incomeResult.Success) report.IncomeReport = incomeResult.Data!;

                var expenseResult = await GetExpenseReportAsync(userId, query);
                if (expenseResult.Success) report.ExpenseReport = expenseResult.Data!;

                var disposableResult = await GetDisposableIncomeReportAsync(userId, query);
                if (disposableResult.Success) report.DisposableIncomeReport = disposableResult.Data!;

                var billsResult = await GetBillsReportAsync(userId, query);
                if (billsResult.Success) report.BillsReport = billsResult.Data!;

                var loanResult = await GetLoanReportAsync(userId, query);
                if (loanResult.Success) report.LoanReport = loanResult.Data!;

                var savingsResult = await GetSavingsReportAsync(userId, query);
                if (savingsResult.Success) report.SavingsReport = savingsResult.Data!;

                var netWorthResult = await GetNetWorthReportAsync(userId, query);
                if (netWorthResult.Success) report.NetWorthReport = netWorthResult.Data!;

                if (query.IncludeInsights)
                {
                    var insightsResult = await GetFinancialInsightsAsync(userId, endDate);
                    if (insightsResult.Success) report.Insights = insightsResult.Data!;
                }

                if (query.IncludePredictions)
                {
                    var predictionsResult = await GetFinancialPredictionsAsync(userId);
                    if (predictionsResult.Success) report.Predictions = predictionsResult.Data!;
                }

                if (query.IncludeTransactions)
                {
                    var transactionsResult = await GetTransactionLogsAsync(userId, 20);
                    if (transactionsResult.Success) report.RecentTransactions = transactionsResult.Data!;
                }

                return ApiResponse<FinancialReportDto>.SuccessResult(report, "Financial report generated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<FinancialReportDto>.ErrorResult($"Error generating financial report: {ex.Message}");
            }
        }

        // ==========================================
        // FINANCIAL SUMMARY
        // ==========================================

        public async Task<ApiResponse<ReportFinancialSummaryDto>> GetFinancialSummaryAsync(string userId, DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow;
                var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Get previous month for comparison
                var prevMonthStart = startOfMonth.AddMonths(-1);
                var prevMonthEnd = startOfMonth.AddDays(-1);

                // Calculate current month values
                var currentIncome = await CalculateTotalIncomeAsync(userId, startOfMonth, endOfMonth);
                var currentExpenses = await CalculateTotalExpensesAsync(userId, startOfMonth, endOfMonth);
                var currentSavings = await CalculateTotalSavingsAsync(userId);
                var savingsGoal = await GetSavingsGoalAsync(userId);
                var netWorth = await CalculateNetWorthAsync(userId);

                // Calculate previous month values for comparison
                var prevIncome = await CalculateTotalIncomeAsync(userId, prevMonthStart, prevMonthEnd);
                var prevExpenses = await CalculateTotalExpensesAsync(userId, prevMonthStart, prevMonthEnd);
                var prevNetWorth = await CalculateNetWorthAsync(userId, prevMonthEnd);

                var summary = new ReportFinancialSummaryDto
                {
                    TotalIncome = currentIncome,
                    IncomeChange = CalculatePercentageChange(prevIncome, currentIncome),
                    
                    TotalExpenses = currentExpenses,
                    ExpenseChange = CalculatePercentageChange(prevExpenses, currentExpenses),
                    
                    DisposableIncome = currentIncome - currentExpenses,
                    DisposableChange = CalculatePercentageChange(
                        prevIncome - prevExpenses,
                        currentIncome - currentExpenses
                    ),
                    
                    TotalSavings = currentSavings,
                    SavingsGoal = savingsGoal,
                    SavingsProgress = savingsGoal > 0 ? (currentSavings / savingsGoal * 100) : 0,
                    
                    NetWorth = netWorth,
                    NetWorthChange = CalculatePercentageChange(prevNetWorth, netWorth)
                };

                return ApiResponse<ReportFinancialSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReportFinancialSummaryDto>.ErrorResult($"Error getting financial summary: {ex.Message}");
            }
        }

        // ==========================================
        // INCOME REPORT
        // ==========================================

        public async Task<ApiResponse<IncomeReportDto>> GetIncomeReportAsync(string userId, ReportQueryDto query)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(query);

                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && !i.IsDeleted && i.IsActive)
                    .ToListAsync();

                var totalIncome = incomeSources.Sum(i => i.MonthlyAmount);

                // Get historical data for trend
                var trendData = await GetIncomeTrendDataAsync(userId, startDate, endDate, query.Period);

                // Group by source and category
                var incomeBySource = incomeSources
                    .GroupBy(i => i.Name)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                var incomeByCategory = incomeSources
                    .GroupBy(i => i.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                var topSource = incomeBySource.OrderByDescending(x => x.Value).FirstOrDefault();

                var report = new IncomeReportDto
                {
                    TotalIncome = totalIncome,
                    MonthlyAverage = trendData.Any() ? trendData.Average(t => t.Value) : totalIncome,
                    GrowthRate = CalculateGrowthRate(trendData),
                    IncomeBySource = incomeBySource,
                    IncomeByCategory = incomeByCategory,
                    IncomeTrend = trendData.Cast<TrendDataPoint>().ToList(),
                    TopIncomeSource = topSource.Key ?? string.Empty,
                    TopIncomeAmount = topSource.Value
                };

                return ApiResponse<IncomeReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                return ApiResponse<IncomeReportDto>.ErrorResult($"Error generating income report: {ex.Message}");
            }
        }

        // ==========================================
        // EXPENSE REPORT
        // ==========================================

        public async Task<ApiResponse<ExpenseReportDto>> GetExpenseReportAsync(string userId, ReportQueryDto query)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(query);
                var (prevStartDate, prevEndDate) = GetPreviousPeriod(startDate, endDate);

                // Get bills (fixed expenses)
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId && !b.IsDeleted &&
                                b.DueDate >= startDate && b.DueDate <= endDate)
                    .ToListAsync();

                var fixedExpenses = bills.Sum(b => b.Amount);

                // Get variable expenses
                var variableExpenses = await _context.VariableExpenses
                    .Where(v => v.UserId == userId && !v.IsDeleted &&
                                v.ExpenseDate >= startDate && v.ExpenseDate <= endDate)
                    .ToListAsync();

                var variableExpenseAmount = variableExpenses.Sum(v => v.Amount);

                var totalExpenses = fixedExpenses + variableExpenseAmount;

                // Group expenses by category
                var expenseByCategory = new Dictionary<string, decimal>
                {
                    ["Bills & Utilities"] = fixedExpenses
                };

                foreach (var group in variableExpenses.GroupBy(v => v.Category))
                {
                    expenseByCategory[group.Key] = group.Sum(v => v.Amount);
                }

                // Calculate percentages
                var expensePercentage = expenseByCategory.ToDictionary(
                    kvp => kvp.Key,
                    kvp => totalExpenses > 0 ? (kvp.Value / totalExpenses * 100) : 0
                );

                // Get trend data
                var trendData = await GetExpenseTrendDataAsync(userId, startDate, endDate, query.Period);

                // Get previous period for comparison
                var prevExpenses = await CalculateTotalExpensesAsync(userId, prevStartDate, prevEndDate);
                var prevVarExpenses = await _context.VariableExpenses
                    .Where(v => v.UserId == userId && !v.IsDeleted &&
                                v.ExpenseDate >= prevStartDate && v.ExpenseDate <= prevEndDate)
                    .GroupBy(v => v.Category)
                    .Select(g => new { Category = g.Key, Amount = g.Sum(v => v.Amount) })
                    .ToListAsync();

                var categoryComparison = new List<ExpenseComparisonDto>();
                foreach (var current in expenseByCategory)
                {
                    var prevAmount = current.Key == "Bills & Utilities"
                        ? await CalculateBillsAsync(userId, prevStartDate, prevEndDate)
                        : prevVarExpenses.FirstOrDefault(p => p.Category == current.Key)?.Amount ?? 0;

                    categoryComparison.Add(new ExpenseComparisonDto
                    {
                        Category = current.Key,
                        CurrentAmount = current.Value,
                        PreviousAmount = prevAmount,
                        Change = current.Value - prevAmount,
                        ChangePercentage = CalculatePercentageChange(prevAmount, current.Value)
                    });
                }

                var highest = expenseByCategory.OrderByDescending(x => x.Value).FirstOrDefault();

                var report = new ExpenseReportDto
                {
                    TotalExpenses = totalExpenses,
                    FixedExpenses = fixedExpenses,
                    VariableExpenses = variableExpenseAmount,
                    ExpenseByCategory = expenseByCategory,
                    ExpensePercentage = expensePercentage,
                    ExpenseTrend = trendData.Cast<TrendDataPoint>().ToList(),
                    HighestExpenseCategory = highest.Key ?? string.Empty,
                    HighestExpenseAmount = highest.Value,
                    HighestExpensePercentage = expensePercentage.ContainsKey(highest.Key) ? expensePercentage[highest.Key] : 0,
                    AverageMonthlyExpense = trendData.Any() ? trendData.Average(t => t.Value) : totalExpenses,
                    CategoryComparison = categoryComparison
                };

                return ApiResponse<ExpenseReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseReportDto>.ErrorResult($"Error generating expense report: {ex.Message}");
            }
        }

        // ==========================================
        // DISPOSABLE INCOME REPORT (STUB)
        // ==========================================

        public async Task<ApiResponse<DisposableIncomeReportDto>> GetDisposableIncomeReportAsync(string userId, ReportQueryDto query)
        {
            return await Task.FromResult(ApiResponse<DisposableIncomeReportDto>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // BILLS REPORT (STUB)
        // ==========================================

        public async Task<ApiResponse<BillsReportDto>> GetBillsReportAsync(string userId, ReportQueryDto query)
        {
            return await Task.FromResult(ApiResponse<BillsReportDto>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // LOAN REPORT (STUB)
        // ==========================================

        public async Task<ApiResponse<LoanReportDto>> GetLoanReportAsync(string userId, ReportQueryDto query)
        {
            return await Task.FromResult(ApiResponse<LoanReportDto>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // SAVINGS REPORT (STUB)
        // ==========================================

        public async Task<ApiResponse<SavingsReportDto>> GetSavingsReportAsync(string userId, ReportQueryDto query)
        {
            return await Task.FromResult(ApiResponse<SavingsReportDto>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // NET WORTH REPORT (STUB)
        // ==========================================

        public async Task<ApiResponse<NetWorthReportDto>> GetNetWorthReportAsync(string userId, ReportQueryDto query)
        {
            return await Task.FromResult(ApiResponse<NetWorthReportDto>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // INSIGHTS (STUB)
        // ==========================================

        public async Task<ApiResponse<List<FinancialInsightDto>>> GetFinancialInsightsAsync(string userId, DateTime? date = null)
        {
            return await Task.FromResult(ApiResponse<List<FinancialInsightDto>>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // PREDICTIONS (STUB)
        // ==========================================

        public async Task<ApiResponse<List<FinancialPredictionDto>>> GetFinancialPredictionsAsync(string userId)
        {
            return await Task.FromResult(ApiResponse<List<FinancialPredictionDto>>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // COMPARISON (STUB)
        // ==========================================

        public async Task<ApiResponse<Dictionary<string, object>>> ComparePeriodsAsync(string userId, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End)
        {
            return await Task.FromResult(ApiResponse<Dictionary<string, object>>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // EXPORT FUNCTIONALITY (STUB)
        // ==========================================

        public async Task<byte[]> ExportReportToPdfAsync(string userId, ExportReportDto exportDto)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("PDF export not implemented yet");
        }

        public async Task<byte[]> ExportReportToCsvAsync(string userId, ExportReportDto exportDto)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("CSV export not implemented yet");
        }

        // ==========================================
        // TRANSACTION LOGS (STUB)
        // ==========================================

        public async Task<ApiResponse<List<TransactionLogDto>>> GetTransactionLogsAsync(string userId, int limit = 20)
        {
            return await Task.FromResult(ApiResponse<List<TransactionLogDto>>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // FULL FINANCIAL REPORT
        // ==========================================

        public async Task<ApiResponse<FullFinancialReportDto>> GetFullFinancialReportAsync(
            string userId, 
            string period = "MONTHLY", 
            bool includeComparison = false, 
            bool includeInsights = false, 
            bool includePredictions = false, 
            bool includeTransactions = false)
        {
            try
            {
                var query = new ReportQueryDto
                {
                    Period = period,
                    IncludeComparison = includeComparison,
                    IncludeInsights = includeInsights,
                    IncludePredictions = includePredictions,
                    IncludeTransactions = includeTransactions
                };

                var (startDate, endDate) = GetDateRange(query);

                var report = new FullFinancialReportDto
                {
                    ReportDate = DateTime.UtcNow,
                    Period = period,
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Generate all report sections
                var summaryResult = await GetFinancialSummaryAsync(userId, endDate);
                if (summaryResult.Success) report.Summary = summaryResult.Data!;

                var incomeResult = await GetIncomeReportAsync(userId, query);
                if (incomeResult.Success) report.IncomeReport = incomeResult.Data!;

                var expenseResult = await GetExpenseReportAsync(userId, query);
                if (expenseResult.Success) report.ExpenseReport = expenseResult.Data!;

                var disposableResult = await GetDisposableIncomeReportAsync(userId, query);
                if (disposableResult.Success) report.DisposableIncomeReport = disposableResult.Data!;

                var billsResult = await GetBillsReportAsync(userId, query);
                if (billsResult.Success) report.BillsReport = billsResult.Data!;

                var loanResult = await GetLoanReportAsync(userId, query);
                if (loanResult.Success) report.LoanReport = loanResult.Data!;

                var savingsResult = await GetSavingsReportAsync(userId, query);
                if (savingsResult.Success) report.SavingsReport = savingsResult.Data!;

                var netWorthResult = await GetNetWorthReportAsync(userId, query);
                if (netWorthResult.Success) report.NetWorthReport = netWorthResult.Data!;

                if (query.IncludeInsights)
                {
                    var insightsResult = await GetFinancialInsightsAsync(userId, endDate);
                    if (insightsResult.Success) report.Insights = insightsResult.Data!;
                }

                if (query.IncludePredictions)
                {
                    var predictionsResult = await GetFinancialPredictionsAsync(userId);
                    if (predictionsResult.Success) report.Predictions = predictionsResult.Data!;
                }

                if (query.IncludeTransactions)
                {
                    var transactionsResult = await GetTransactionLogsAsync(userId, 20);
                    if (transactionsResult.Success) report.RecentTransactions = transactionsResult.Data!;
                }

                return ApiResponse<FullFinancialReportDto>.SuccessResult(report, "Full financial report generated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<FullFinancialReportDto>.ErrorResult($"Error generating full financial report: {ex.Message}");
            }
        }

        // ==========================================
        // HELPER METHODS
        // ==========================================

        private (DateTime startDate, DateTime endDate) GetDateRange(ReportQueryDto query)
        {
            var now = DateTime.UtcNow;
            
            return query.Period?.ToUpper() switch
            {
                "DAILY" => (now.Date, now.Date.AddDays(1).AddSeconds(-1)),
                "WEEKLY" => (now.AddDays(-7), now),
                "MONTHLY" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1)),
                "QUARTERLY" => (now.AddMonths(-3), now),
                "YEARLY" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31)),
                "CUSTOM" => (query.StartDate ?? now.AddMonths(-1), query.EndDate ?? now),
                _ => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1))
            };
        }

        private (DateTime startDate, DateTime endDate) GetPreviousPeriod(DateTime startDate, DateTime endDate)
        {
            var duration = endDate - startDate;
            return (startDate.Subtract(duration), startDate.AddDays(-1));
        }

        private async Task<decimal> CalculateTotalIncomeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var incomeSources = await _context.IncomeSources
                .Where(i => i.UserId == userId && !i.IsDeleted && i.IsActive)
                .ToListAsync();

            return incomeSources.Sum(i => i.MonthlyAmount);
        }

        private async Task<decimal> CalculateTotalExpensesAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var bills = await _context.Bills
                .Where(b => b.UserId == userId && !b.IsDeleted &&
                            b.DueDate >= startDate && b.DueDate <= endDate)
                .SumAsync(b => b.Amount);

            var variableExpenses = await _context.VariableExpenses
                .Where(v => v.UserId == userId && !v.IsDeleted &&
                            v.ExpenseDate >= startDate && v.ExpenseDate <= endDate)
                .SumAsync(v => v.Amount);

            return bills + variableExpenses;
        }

        private async Task<decimal> CalculateBillsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Bills
                .Where(b => b.UserId == userId && !b.IsDeleted &&
                            b.DueDate >= startDate && b.DueDate <= endDate)
                .SumAsync(b => b.Amount);
        }

        private async Task<decimal> CalculateTotalSavingsAsync(string userId)
        {
            var savings = await _context.SavingsAccounts
                .Where(sa => sa.UserId == userId)
                .SelectMany(sa => _context.SavingsTransactions
                    .Where(st => st.SavingsAccountId == sa.Id && !st.IsDeleted))
                .ToListAsync();

            return savings.Sum(s => s.TransactionType == "DEPOSIT" ? s.Amount : -s.Amount);
        }

        private async Task<decimal> GetSavingsGoalAsync(string userId)
        {
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return profile?.MonthlySavingsGoal ?? 0;
        }

        private async Task<decimal> CalculateNetWorthAsync(string userId, DateTime? asOfDate = null)
        {
            var savings = await CalculateTotalSavingsAsync(userId);
            
            // For now, net worth is just savings. Could be extended to include assets and liabilities
            return savings;
        }

        private decimal CalculatePercentageChange(decimal oldValue, decimal newValue)
        {
            if (oldValue == 0) return newValue > 0 ? 100 : 0;
            return ((newValue - oldValue) / oldValue) * 100;
        }

        private decimal CalculateGrowthRate(List<TrendDataDto> trendData)
        {
            if (trendData.Count < 2) return 0;

            var oldest = trendData.First().Value;
            var newest = trendData.Last().Value;

            return CalculatePercentageChange(oldest, newest);
        }

        private async Task<List<TrendDataDto>> GetIncomeTrendDataAsync(string userId, DateTime startDate, DateTime endDate, string period)
        {
            // Stub implementation - would need to calculate income over time periods
            return new List<TrendDataDto>();
        }

        private async Task<List<TrendDataDto>> GetExpenseTrendDataAsync(string userId, DateTime startDate, DateTime endDate, string period)
        {
            // Stub implementation - would need to calculate expenses over time periods
            return new List<TrendDataDto>();
        }
    }
}
