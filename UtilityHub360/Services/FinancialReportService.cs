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
                if (loanResult.Success)
                {
                    report.LoanReport = loanResult.Data!;
                    Console.WriteLine($"[FULL REPORT] Loan report: {loanResult.Data!.ActiveLoansCount} loans");
                }
                else
                {
                    report.LoanReport = new LoanReportDto();
                    Console.WriteLine($"[FULL REPORT ERROR] Loan report failed: {loanResult.Message}");
                }

                var savingsResult = await GetSavingsReportAsync(userId, query);
                if (savingsResult.Success) report.SavingsReport = savingsResult.Data!;

                var netWorthResult = await GetNetWorthReportAsync(userId, query);
                if (netWorthResult.Success)
                {
                    report.NetWorthReport = netWorthResult.Data!;
                    Console.WriteLine($"[FULL REPORT] Net Worth report: {netWorthResult.Data!.CurrentNetWorth:C}");
                }
                else
                {
                    report.NetWorthReport = new NetWorthReportDto(); // Ensure it's not null even if fails
                    Console.WriteLine($"[FULL REPORT ERROR] Net Worth report failed: {netWorthResult.Message}");
                }

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

                // Note: IsDeleted is ignored in DbContext, so we don't filter by it
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
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
        // LOAN REPORT
        // ==========================================

        public async Task<ApiResponse<LoanReportDto>> GetLoanReportAsync(string userId, ReportQueryDto query)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(query);

                // Debug: Log the query
                Console.WriteLine($"[LOAN REPORT DEBUG] ========== STARTING LOAN REPORT ==========");
                Console.WriteLine($"[LOAN REPORT DEBUG] UserId: {userId}, Query Period: {query.Period}");
                Console.WriteLine($"[LOAN REPORT DEBUG] Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Fetch ALL loans for this user (for diagnostic and filtering)
                var allUserLoans = await _context.Loans
                    .Where(l => l.UserId == userId)
                    .ToListAsync();

                Console.WriteLine($"[LOAN REPORT DEBUG] Total loans for user {userId}: {allUserLoans.Count}");
                if (allUserLoans.Any())
                {
                    foreach (var loan in allUserLoans)
                    {
                        Console.WriteLine($"[LOAN REPORT DEBUG] ALL LOANS - Loan ID: {loan.Id}, Status: '{loan.Status}' (Length: {loan.Status?.Length ?? 0}), Principal: {loan.Principal}, RemainingBalance: {loan.RemainingBalance}");
                    }
                }

                // Filter in memory with case-insensitive comparison and trim whitespace
                // This handles any case sensitivity issues or whitespace in the Status field
                var activeLoans = allUserLoans
                    .Where(l => !string.IsNullOrWhiteSpace(l.Status) &&
                               l.Status.Trim().ToUpper() != "REJECTED" && 
                               l.Status.Trim().ToUpper() != "COMPLETED")
                    .ToList();

                Console.WriteLine($"[LOAN REPORT DEBUG] Found {activeLoans.Count} active loans (excluding REJECTED/COMPLETED) for user {userId}");
                if (activeLoans.Any())
                {
                    foreach (var loan in activeLoans)
                    {
                        Console.WriteLine($"[LOAN REPORT DEBUG] ACTIVE LOAN - Loan ID: {loan.Id}, Status: '{loan.Status}', Principal: {loan.Principal}, RemainingBalance: {loan.RemainingBalance}, MonthlyPayment: {loan.MonthlyPayment}");
                    }
                }

                if (!activeLoans.Any())
                {
                    Console.WriteLine($"[LOAN REPORT DEBUG] No active loans found for user {userId}. Returning empty report.");
                    return ApiResponse<LoanReportDto>.SuccessResult(new LoanReportDto
                    {
                        ActiveLoansCount = 0,
                        TotalPrincipal = 0,
                        TotalRemainingBalance = 0,
                        TotalMonthlyPayment = 0,
                        TotalInterestPaid = 0,
                        ActiveLoans = new List<LoanDetailDto>(),
                        RepaymentTrend = new List<TrendDataPoint>(),
                        ProjectedDebtFreeDate = null,
                        MonthsUntilDebtFree = 0
                    });
                }

                var loanIds = activeLoans.Select(l => l.Id).ToList();

                // Calculate totals
                var totalPrincipal = activeLoans.Sum(l => l.Principal);
                var totalRemainingBalance = activeLoans.Sum(l => l.RemainingBalance);
                var totalMonthlyPayment = activeLoans.Sum(l => l.MonthlyPayment);

                // Calculate total interest paid from RepaymentSchedule (paid installments)
                // Note: Using string comparison for Status to avoid case sensitivity issues
                var paidSchedules = await _context.RepaymentSchedules
                    .Where(rs => loanIds.Contains(rs.LoanId) && 
                               rs.Status != null && 
                               rs.Status.ToUpper() == "PAID")
                    .ToListAsync();

                var totalInterestPaid = paidSchedules.Sum(rs => rs.InterestAmount);

                // Get loan details with repayment progress
                var loanDetails = new List<LoanDetailDto>();
                foreach (var loan in activeLoans)
                {
                    var paidAmount = loan.TotalAmount - loan.RemainingBalance;
                    var repaymentProgress = loan.TotalAmount > 0 
                        ? (int)((paidAmount / loan.TotalAmount) * 100) 
                        : 0;

                    loanDetails.Add(new LoanDetailDto
                    {
                        LoanId = loan.Id,
                        Purpose = loan.Purpose,
                        Principal = loan.Principal,
                        RemainingBalance = loan.RemainingBalance,
                        MonthlyPayment = loan.MonthlyPayment,
                        InterestRate = loan.InterestRate,
                        RepaymentProgress = repaymentProgress
                    });
                }

                // Calculate repayment trend (monthly payments made)
                var repaymentTrend = new List<TrendDataPoint>();
                // Query payments without loading navigation properties to avoid soft delete column issues
                var payments = await _context.Payments
                    .Where(p => loanIds.Contains(p.LoanId) && 
                               !p.IsBankTransaction &&
                               p.TransactionType == "PAYMENT" &&
                               p.TransactionDate >= startDate &&
                               p.TransactionDate <= endDate)
                    .Select(p => new { p.Id, p.Amount, p.TransactionDate, p.ProcessedAt })
                    .ToListAsync();

                var monthlyPayments = payments
                    .GroupBy(p => new { 
                        Year = p.TransactionDate?.Year ?? p.ProcessedAt.Year, 
                        Month = p.TransactionDate?.Month ?? p.ProcessedAt.Month 
                    })
                    .Select(g => new TrendDataPoint
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Label = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                        Value = g.Sum(p => p.Amount)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                repaymentTrend = monthlyPayments;

                // Calculate projected debt-free date
                DateTime? projectedDebtFreeDate = null;
                int monthsUntilDebtFree = 0;

                if (activeLoans.Any() && totalMonthlyPayment > 0)
                {
                    var averageMonthlyPayment = totalMonthlyPayment / activeLoans.Count;
                    if (averageMonthlyPayment > 0)
                    {
                        var monthsNeeded = (int)Math.Ceiling(totalRemainingBalance / averageMonthlyPayment);
                        monthsUntilDebtFree = monthsNeeded;
                        projectedDebtFreeDate = DateTime.UtcNow.AddMonths(monthsNeeded);
                    }
                }

                var report = new LoanReportDto
                {
                    ActiveLoansCount = activeLoans.Count,
                    TotalPrincipal = totalPrincipal,
                    TotalRemainingBalance = totalRemainingBalance,
                    TotalMonthlyPayment = totalMonthlyPayment,
                    TotalInterestPaid = totalInterestPaid,
                    ActiveLoans = loanDetails,
                    RepaymentTrend = repaymentTrend,
                    ProjectedDebtFreeDate = projectedDebtFreeDate,
                    MonthsUntilDebtFree = monthsUntilDebtFree
                };

                Console.WriteLine($"[LOAN REPORT DEBUG] ========== SUCCESS - Returning report with {activeLoans.Count} loans ==========");
                return ApiResponse<LoanReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOAN REPORT ERROR] ========== EXCEPTION CAUGHT ==========");
                Console.WriteLine($"[LOAN REPORT ERROR] Exception Message: {ex.Message}");
                Console.WriteLine($"[LOAN REPORT ERROR] Stack Trace: {ex.StackTrace}");
                return ApiResponse<LoanReportDto>.ErrorResult($"Error generating loan report: {ex.Message}");
            }
        }

        // ==========================================
        // SAVINGS REPORT
        // ==========================================

        public async Task<ApiResponse<SavingsReportDto>> GetSavingsReportAsync(string userId, ReportQueryDto query)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(query);

                // Get all active savings accounts for the user
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId && sa.IsActive)
                    .ToListAsync();

                // Calculate total savings (sum of current balances)
                var totalSavings = savingsAccounts.Sum(sa => sa.CurrentBalance);

                // Calculate total savings goal (sum of target amounts)
                var savingsGoal = savingsAccounts.Sum(sa => sa.TargetAmount);

                // Calculate goal progress percentage
                var goalProgress = savingsGoal > 0 
                    ? (totalSavings / savingsGoal) * 100 
                    : 0;

                // Calculate monthly savings from transactions in the period
                var monthlySavings = await _context.SavingsTransactions
                    .Where(st => st.SavingsAccount.UserId == userId && 
                                st.TransactionType == "DEPOSIT" &&
                                st.TransactionDate >= startDate && 
                                st.TransactionDate <= endDate)
                    .SumAsync(st => st.Amount);

                // Get savings goals list
                var goals = savingsAccounts.Select(sa => 
                {
                    // Use StartDate if available, otherwise use CreatedAt as fallback
                    var startDate = sa.StartDate ?? sa.CreatedAt;
                    
                    return new SavingsGoalDto
                    {
                        GoalName = sa.Goal ?? sa.AccountName,
                        TargetAmount = sa.TargetAmount,
                        CurrentAmount = sa.CurrentBalance,
                        Progress = sa.TargetAmount > 0 
                            ? (sa.CurrentBalance / sa.TargetAmount) * 100 
                            : 0,
                        TargetDate = sa.TargetDate,
                        StartDate = startDate
                    };
                }).ToList();

                // Calculate savings trend (monthly breakdown)
                var savingsTrend = new List<TrendDataPoint>();
                var savingsTransactions = await _context.SavingsTransactions
                    .Where(st => st.SavingsAccount.UserId == userId && 
                                st.TransactionType == "DEPOSIT" &&
                                st.TransactionDate >= startDate && 
                                st.TransactionDate <= endDate)
                    .ToListAsync();

                var monthlyGrouped = savingsTransactions
                    .GroupBy(st => new { 
                        Year = st.TransactionDate.Year, 
                        Month = st.TransactionDate.Month 
                    })
                    .Select(g => new TrendDataPoint
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Label = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                        Value = g.Sum(st => st.Amount)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                savingsTrend = monthlyGrouped;

                // Calculate savings rate (as percentage of income - you may need to get income from income sources)
                var savingsRate = 0m; // TODO: Calculate based on income sources

                // Calculate projected goal date and months to goal
                DateTime? projectedGoalDate = null;
                int monthsToGoal = 0;
                decimal suggestionIncrease = 0;
                int suggestionMonthsSaved = 0;

                if (savingsAccounts.Any() && monthlySavings > 0)
                {
                    var remainingAmount = savingsGoal - totalSavings;
                    if (remainingAmount > 0)
                    {
                        var monthsRemaining = (int)Math.Ceiling((double)(remainingAmount / monthlySavings));
                        projectedGoalDate = DateTime.UtcNow.AddMonths(monthsRemaining);
                        monthsToGoal = monthsRemaining;

                        // Calculate suggestion: if they increase savings by 20%, how many months saved?
                        var increasedSavings = monthlySavings * 1.2m;
                        var monthsWithIncrease = (int)Math.Ceiling((double)(remainingAmount / increasedSavings));
                        suggestionMonthsSaved = monthsRemaining - monthsWithIncrease;
                        suggestionIncrease = increasedSavings - monthlySavings;
                    }
                    else
                    {
                        // Goal already reached
                        projectedGoalDate = DateTime.UtcNow;
                        monthsToGoal = 0;
                    }
                }

                var report = new SavingsReportDto
                {
                    TotalSavings = totalSavings,
                    MonthlySavings = monthlySavings,
                    SavingsGoal = savingsGoal,
                    GoalProgress = goalProgress,
                    Goals = goals,
                    SavingsTrend = savingsTrend,
                    SavingsRate = savingsRate,
                    ProjectedGoalDate = projectedGoalDate,
                    MonthsToGoal = monthsToGoal,
                    SuggestionIncrease = suggestionIncrease,
                    SuggestionMonthsSaved = suggestionMonthsSaved
                };

                return ApiResponse<SavingsReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                return ApiResponse<SavingsReportDto>.ErrorResult($"Error generating savings report: {ex.Message}");
            }
        }

        // ==========================================
        // NET WORTH REPORT
        // ==========================================

        public async Task<ApiResponse<NetWorthReportDto>> GetNetWorthReportAsync(string userId, ReportQueryDto query)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(query);

                Console.WriteLine($"[NET WORTH REPORT DEBUG] ========== STARTING NET WORTH REPORT ==========");
                Console.WriteLine($"[NET WORTH REPORT DEBUG] UserId: {userId}, Query Period: {query.Period}");
                Console.WriteLine($"[NET WORTH REPORT DEBUG] Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Calculate Assets: Bank Accounts + Savings
                var bankAccounts = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                var totalBankBalance = bankAccounts.Sum(ba => ba.CurrentBalance);
                Console.WriteLine($"[NET WORTH REPORT DEBUG] Total Bank Accounts: {bankAccounts.Count}, Total Balance: {totalBankBalance}");

                // Calculate Savings (from SavingsAccounts)
                var totalSavings = await CalculateTotalSavingsAsync(userId);
                Console.WriteLine($"[NET WORTH REPORT DEBUG] Total Savings: {totalSavings}");

                var totalAssets = totalBankBalance + totalSavings;
                Console.WriteLine($"[NET WORTH REPORT DEBUG] Total Assets: {totalAssets}");

                // Calculate Liabilities: Active Loans (remaining balance)
                var allUserLoans = await _context.Loans
                    .Where(l => l.UserId == userId)
                    .ToListAsync();

                var activeLoans = allUserLoans
                    .Where(l => !string.IsNullOrWhiteSpace(l.Status) &&
                               l.Status.Trim().ToUpper() != "REJECTED" && 
                               l.Status.Trim().ToUpper() != "COMPLETED")
                    .ToList();

                var totalLiabilities = activeLoans.Sum(l => l.RemainingBalance);
                Console.WriteLine($"[NET WORTH REPORT DEBUG] Active Loans: {activeLoans.Count}, Total Liabilities: {totalLiabilities}");

                // Calculate Net Worth
                var currentNetWorth = totalAssets - totalLiabilities;
                Console.WriteLine($"[NET WORTH REPORT DEBUG] Current Net Worth: {currentNetWorth}");

                // Calculate previous period for comparison
                var (prevStartDate, prevEndDate) = GetPreviousPeriod(startDate, endDate);
                var previousNetWorth = await CalculateNetWorthAtDateAsync(userId, prevEndDate);
                var netWorthChange = currentNetWorth - previousNetWorth;
                var netWorthChangePercentage = CalculatePercentageChange(previousNetWorth, currentNetWorth);

                Console.WriteLine($"[NET WORTH REPORT DEBUG] Previous Net Worth ({prevEndDate:yyyy-MM-dd}): {previousNetWorth}");
                Console.WriteLine($"[NET WORTH REPORT DEBUG] Net Worth Change: {netWorthChange} ({netWorthChangePercentage:F2}%)");

                // Build Asset Breakdown
                var assetBreakdown = new Dictionary<string, decimal>();
                if (totalBankBalance > 0)
                {
                    assetBreakdown["Bank Accounts"] = totalBankBalance;
                }
                if (totalSavings > 0)
                {
                    assetBreakdown["Savings Accounts"] = totalSavings;
                }

                // Build Liability Breakdown
                var liabilityBreakdown = new Dictionary<string, decimal>();
                foreach (var loan in activeLoans)
                {
                    var loanLabel = string.IsNullOrWhiteSpace(loan.Purpose) 
                        ? $"Loan #{(loan.Id.Length >= 8 ? loan.Id.Substring(0, 8) : loan.Id)}" 
                        : loan.Purpose;
                    liabilityBreakdown[loanLabel] = loan.RemainingBalance;
                }

                // Generate Trend Data (monthly net worth over time)
                var netWorthTrend = await GenerateNetWorthTrendAsync(userId, startDate, endDate);

                // Generate trend description
                var trendDescription = GenerateTrendDescription(netWorthChange, netWorthChangePercentage, currentNetWorth);

                var report = new NetWorthReportDto
                {
                    CurrentNetWorth = currentNetWorth,
                    NetWorthChange = netWorthChange,
                    NetWorthChangePercentage = netWorthChangePercentage,
                    TotalAssets = totalAssets,
                    TotalLiabilities = totalLiabilities,
                    AssetBreakdown = assetBreakdown,
                    LiabilityBreakdown = liabilityBreakdown,
                    NetWorthTrend = netWorthTrend,
                    TrendDescription = trendDescription
                };

                Console.WriteLine($"[NET WORTH REPORT DEBUG] ========== SUCCESS - Returning report ==========");
                return ApiResponse<NetWorthReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NET WORTH REPORT ERROR] ========== EXCEPTION CAUGHT ==========");
                Console.WriteLine($"[NET WORTH REPORT ERROR] Exception Message: {ex.Message}");
                Console.WriteLine($"[NET WORTH REPORT ERROR] Stack Trace: {ex.StackTrace}");
                return ApiResponse<NetWorthReportDto>.ErrorResult($"Error generating net worth report: {ex.Message}");
            }
        }

        // ==========================================
        // BALANCE SHEET
        // ==========================================

        public async Task<ApiResponse<DTOs.BalanceSheetDto>> GetBalanceSheetAsync(string userId, DateTime? asOfDate = null)
        {
            try
            {
                var reportDate = asOfDate ?? DateTime.UtcNow;

                // ASSETS SECTION
                var assets = new DTOs.AssetsSectionDto();

                // Current Assets: Bank Accounts (EXCLUDE credit cards - they're liabilities)
                var bankAccounts = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                foreach (var account in bankAccounts)
                {
                    // Exclude credit cards from assets
                    var accountTypeLower = account.AccountType?.ToLower().Trim() ?? "";
                    var isCreditCard = accountTypeLower == "credit_card" || 
                                       accountTypeLower == "credit card" || 
                                       accountTypeLower == "creditcard";
                    
                    if (!isCreditCard && account.CurrentBalance > 0)
                    {
                        assets.CurrentAssets.Add(new DTOs.BalanceSheetItemDto
                        {
                            AccountName = account.AccountName ?? "Unnamed Account",
                            AccountType = account.AccountType ?? "Bank Account",
                            Amount = account.CurrentBalance,
                            Description = $"{account.AccountType} - {account.AccountName}",
                            ReferenceId = account.Id
                        });
                    }
                }

                // Current Assets: Savings Accounts
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId)
                    .ToListAsync();

                foreach (var savingsAccount in savingsAccounts)
                {
                    // Calculate savings balance from transactions
                    var savingsTransactions = await _context.SavingsTransactions
                        .Where(st => st.SavingsAccountId == savingsAccount.Id)
                        .ToListAsync();

                    var savingsBalance = savingsTransactions.Sum(st => 
                        st.TransactionType == "DEPOSIT" ? st.Amount : -st.Amount);

                    if (savingsBalance > 0)
                    {
                        assets.CurrentAssets.Add(new DTOs.BalanceSheetItemDto
                        {
                            AccountName = savingsAccount.AccountName ?? "Unnamed Savings",
                            AccountType = "Savings Account",
                            Amount = savingsBalance,
                            Description = $"Savings - {savingsAccount.AccountName}",
                            ReferenceId = savingsAccount.Id
                        });
                    }
                }

                assets.TotalCurrentAssets = assets.CurrentAssets.Sum(a => a.Amount);

                // Fixed Assets: None for personal finance (could add property, vehicles, etc. in future)
                assets.TotalFixedAssets = 0;

                // Other Assets: None for now
                assets.TotalOtherAssets = 0;

                // LIABILITIES SECTION
                var liabilities = new DTOs.LiabilitiesSectionDto();

                // Current Liabilities: Credit Card Balances
                foreach (var account in bankAccounts)
                {
                    var accountTypeLower = account.AccountType?.ToLower().Trim() ?? "";
                    var isCreditCard = accountTypeLower == "credit_card" || 
                                       accountTypeLower == "credit card" || 
                                       accountTypeLower == "creditcard";
                    
                    if (isCreditCard && account.CurrentBalance > 0)
                    {
                        liabilities.CurrentLiabilities.Add(new DTOs.BalanceSheetItemDto
                        {
                            AccountName = account.AccountName ?? "Unnamed Credit Card",
                            AccountType = "Credit Card",
                            Amount = account.CurrentBalance,
                            Description = $"Credit Card - {account.AccountName} (Outstanding Balance)",
                            ReferenceId = account.Id
                        });
                    }
                }

                // Current Liabilities: Unpaid Bills
                var unpaidBills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status != null && 
                               b.Status.ToUpper() != "PAID")
                    .ToListAsync();

                foreach (var bill in unpaidBills)
                {
                    liabilities.CurrentLiabilities.Add(new DTOs.BalanceSheetItemDto
                    {
                        AccountName = bill.Provider ?? "Unnamed Bill",
                        AccountType = bill.BillType ?? "Bill",
                        Amount = bill.Amount,
                        Description = $"{bill.BillType} - {bill.Provider}",
                        ReferenceId = bill.Id
                    });
                }

                liabilities.TotalCurrentLiabilities = liabilities.CurrentLiabilities.Sum(l => l.Amount);

                // Long-term Liabilities: Active Loans
                var activeLoans = await _context.Loans
                    .Where(l => l.UserId == userId &&
                               !string.IsNullOrWhiteSpace(l.Status) &&
                               l.Status.Trim().ToUpper() != "REJECTED" && 
                               l.Status.Trim().ToUpper() != "COMPLETED")
                    .ToListAsync();

                foreach (var loan in activeLoans)
                {
                    liabilities.LongTermLiabilities.Add(new DTOs.BalanceSheetItemDto
                    {
                        AccountName = loan.Purpose ?? "Loan",
                        AccountType = "Loan Payable",
                        Amount = loan.RemainingBalance,
                        Description = $"Loan - {loan.Purpose} (Remaining: {loan.RemainingBalance:C})",
                        ReferenceId = loan.Id
                    });
                }

                liabilities.TotalLongTermLiabilities = liabilities.LongTermLiabilities.Sum(l => l.Amount);

                // EQUITY SECTION
                var equity = new DTOs.EquitySectionDto();

                // Calculate total assets and liabilities for equity calculation
                var totalAssets = assets.TotalAssets;
                var totalLiabilities = liabilities.TotalLiabilities;

                // Owner's Capital: Initial capital (could be from user profile or first transaction)
                // For now, we'll calculate it as: Assets - Liabilities - Retained Earnings
                // Retained Earnings = Net Income (Income - Expenses) over time
                
                // Calculate net income (simplified - from income sources and expenses)
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();

                var totalIncome = incomeSources.Sum(i => i.Amount);

                // Calculate expenses from transactions
                var expenseTransactions = await _context.Payments
                    .Where(p => p.UserId == userId && 
                               p.TransactionType == "DEBIT" &&
                               p.TransactionDate <= reportDate)
                    .SumAsync(p => p.Amount);

                var netIncome = totalIncome - expenseTransactions;

                // Retained Earnings = Net Income (simplified)
                equity.RetainedEarnings = netIncome > 0 ? netIncome : 0;

                // Owner's Capital = Total Assets - Total Liabilities - Retained Earnings
                equity.OwnersCapital = totalAssets - totalLiabilities - equity.RetainedEarnings;
                if (equity.OwnersCapital < 0)
                {
                    // If negative, it means we have negative equity (debt exceeds assets)
                    equity.OwnersCapital = 0;
                    equity.RetainedEarnings = totalAssets - totalLiabilities;
                }

                // Build Balance Sheet
                var balanceSheet = new DTOs.BalanceSheetDto
                {
                    AsOfDate = reportDate,
                    Assets = assets,
                    Liabilities = liabilities,
                    Equity = equity
                };

                return ApiResponse<DTOs.BalanceSheetDto>.SuccessResult(balanceSheet);
            }
            catch (Exception ex)
            {
                return ApiResponse<DTOs.BalanceSheetDto>.ErrorResult($"Error generating balance sheet: {ex.Message}");
            }
        }

        // ==========================================
        // CASH FLOW STATEMENT
        // ==========================================

        public async Task<ApiResponse<DTOs.CashFlowStatementDto>> GetCashFlowStatementAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, string period = "MONTHLY")
        {
            try
            {
                // Determine date range
                DateTime periodStart, periodEnd;
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    periodStart = startDate.Value;
                    periodEnd = endDate.Value;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    periodStart = new DateTime(now.Year, now.Month, 1);
                    
                    periodEnd = period.ToUpper() switch
                    {
                        "QUARTERLY" => periodStart.AddMonths(3).AddDays(-1),
                        "YEARLY" => periodStart.AddYears(1).AddDays(-1),
                        _ => periodStart.AddMonths(1).AddDays(-1) // MONTHLY
                    };
                }

                // Get beginning cash balance (from bank accounts at start of period)
                var bankAccounts = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                // Calculate beginning balance by looking at transactions before period start
                var beginningBalance = 0m;
                foreach (var account in bankAccounts)
                {
                    // Get balance at period start by calculating from transactions
                    var transactionsBefore = await _context.Payments
                        .Where(p => p.BankAccountId == account.Id && 
                                   p.TransactionDate < periodStart)
                        .ToListAsync();
                    
                    var accountBalance = account.CurrentBalance;
                    // Adjust for transactions during period to get beginning balance
                    var transactionsDuring = await _context.Payments
                        .Where(p => p.BankAccountId == account.Id && 
                                   p.TransactionDate.HasValue &&
                                   p.TransactionDate >= periodStart && 
                                   p.TransactionDate <= periodEnd)
                        .ToListAsync();
                    
                    var netDuringPeriod = transactionsDuring.Sum(t => 
                        t.TransactionType == "CREDIT" ? t.Amount : -t.Amount);
                    
                    beginningBalance += accountBalance - netDuringPeriod;
                }

                // OPERATING ACTIVITIES
                var operating = new DTOs.OperatingActivitiesDto();

                // Income received (CREDIT transactions that are not linked to loans, bills, or savings)
                var incomeTransactions = await _context.Payments
                    .Where(p => p.UserId == userId &&
                               p.TransactionType == "CREDIT" &&
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= periodStart &&
                               p.TransactionDate <= periodEnd &&
                               p.LoanId == null &&
                               p.BillId == null &&
                               p.SavingsAccountId == null)
                    .ToListAsync();

                operating.IncomeReceived = incomeTransactions.Sum(t => t.Amount);
                operating.InflowItems.AddRange(incomeTransactions.Select(t => new DTOs.CashFlowItemDto
                {
                    Description = t.Description ?? "Income",
                    Category = t.Category ?? "Income",
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate ?? DateTime.UtcNow,
                    ReferenceId = t.Id,
                    ReferenceType = "INCOME"
                }));

                // Expenses paid (DEBIT transactions for expenses)
                var expenseTransactions = await _context.Payments
                    .Where(p => p.UserId == userId &&
                               p.TransactionType == "DEBIT" &&
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= periodStart &&
                               p.TransactionDate <= periodEnd &&
                               p.BillId == null &&
                               p.LoanId == null &&
                               p.SavingsAccountId == null)
                    .ToListAsync();

                operating.ExpensesPaid = expenseTransactions.Sum(t => t.Amount);
                operating.OutflowItems.AddRange(expenseTransactions.Select(t => new DTOs.CashFlowItemDto
                {
                    Description = t.Description ?? "Expense",
                    Category = t.Category ?? "Other",
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate ?? DateTime.UtcNow,
                    ReferenceId = t.Id,
                    ReferenceType = "EXPENSE"
                }));

                // Bills paid
                var billPayments = await _context.Payments
                    .Where(p => p.UserId == userId &&
                               p.TransactionType == "DEBIT" &&
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= periodStart &&
                               p.TransactionDate <= periodEnd &&
                               p.BillId != null)
                    .ToListAsync();

                operating.BillsPaid = billPayments.Sum(t => t.Amount);
                operating.OutflowItems.AddRange(billPayments.Select(t => new DTOs.CashFlowItemDto
                {
                    Description = t.Description ?? "Bill Payment",
                    Category = "Bills",
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate ?? DateTime.UtcNow,
                    ReferenceId = t.BillId,
                    ReferenceType = "BILL"
                }));

                // Interest paid (from loan payments)
                var loanPayments = await _context.Payments
                    .Where(p => p.UserId == userId &&
                               p.TransactionType == "DEBIT" &&
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= periodStart &&
                               p.TransactionDate <= periodEnd &&
                               p.LoanId != null)
                    .ToListAsync();

                // Calculate interest portion from journal entries
                var interestPaid = 0m;
                foreach (var payment in loanPayments)
                {
                    var journalEntries = await _context.JournalEntries
                        .Include(je => je.JournalEntryLines)
                        .Where(je => je.Reference == payment.Id.ToString() || 
                                    (je.LoanId != null && je.LoanId == payment.LoanId &&
                                     je.EntryDate >= periodStart && je.EntryDate <= periodEnd))
                        .ToListAsync();

                    foreach (var entry in journalEntries)
                    {
                        var interestLine = entry.JournalEntryLines
                            .FirstOrDefault(jel => jel.AccountName.Contains("Interest", StringComparison.OrdinalIgnoreCase));
                        if (interestLine != null && interestLine.EntrySide == "DEBIT")
                        {
                            interestPaid += interestLine.Amount;
                        }
                    }
                }

                operating.InterestPaid = interestPaid;

                // INVESTING ACTIVITIES
                var investing = new DTOs.InvestingActivitiesDto();

                // Savings deposits (outflow)
                var savingsDeposits = await _context.SavingsTransactions
                    .Include(st => st.SavingsAccount)
                    .Where(st => st.SavingsAccount != null &&
                                st.SavingsAccount.UserId == userId &&
                                st.TransactionType == "DEPOSIT" &&
                                st.TransactionDate >= periodStart &&
                                st.TransactionDate <= periodEnd)
                    .ToListAsync();
                
                // Filter out deleted transactions in memory (IsDeleted may not be mapped)
                savingsDeposits = savingsDeposits.Where(st => !st.IsDeleted).ToList();

                investing.SavingsDeposits = savingsDeposits.Sum(t => t.Amount);
                investing.OutflowItems.AddRange(savingsDeposits.Select(t => new DTOs.CashFlowItemDto
                {
                    Description = $"Savings Deposit - {t.SavingsAccount?.AccountName ?? "Savings"}",
                    Category = "Savings",
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    ReferenceId = t.SavingsAccountId,
                    ReferenceType = "SAVINGS"
                }));

                // Savings withdrawals (inflow)
                var savingsWithdrawals = await _context.SavingsTransactions
                    .Include(st => st.SavingsAccount)
                    .Where(st => st.SavingsAccount != null &&
                                st.SavingsAccount.UserId == userId &&
                                st.TransactionType == "WITHDRAWAL" &&
                                st.TransactionDate >= periodStart &&
                                st.TransactionDate <= periodEnd)
                    .ToListAsync();
                
                // Filter out deleted transactions in memory (IsDeleted may not be mapped)
                savingsWithdrawals = savingsWithdrawals.Where(st => !st.IsDeleted).ToList();

                investing.SavingsWithdrawals = savingsWithdrawals.Sum(t => t.Amount);
                investing.InflowItems.AddRange(savingsWithdrawals.Select(t => new DTOs.CashFlowItemDto
                {
                    Description = $"Savings Withdrawal - {t.SavingsAccount?.AccountName ?? "Savings"}",
                    Category = "Savings",
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    ReferenceId = t.SavingsAccountId,
                    ReferenceType = "SAVINGS"
                }));

                // Investment returns (if any investment income)
                var investmentIncome = incomeTransactions
                    .Where(t => t.Category != null && 
                               (t.Category.Contains("investment", StringComparison.OrdinalIgnoreCase) ||
                                t.Category.Contains("dividend", StringComparison.OrdinalIgnoreCase)))
                    .Sum(t => t.Amount);

                investing.InvestmentReturns = investmentIncome;

                // FINANCING ACTIVITIES
                var financing = new DTOs.FinancingActivitiesDto();

                // Loan disbursements (inflow)
                var loanDisbursements = await _context.Loans
                    .Where(l => l.UserId == userId &&
                               (l.Status == "APPROVED" || l.Status == "ACTIVE") &&
                               l.DisbursedAt.HasValue &&
                               l.DisbursedAt >= periodStart &&
                               l.DisbursedAt <= periodEnd)
                    .ToListAsync();

                financing.LoanDisbursements = loanDisbursements.Sum(l => l.Principal);
                financing.InflowItems.AddRange(loanDisbursements.Select(l => new DTOs.CashFlowItemDto
                {
                    Description = $"Loan Disbursement - {l.Purpose ?? "Loan"}",
                    Category = "Loan",
                    Amount = l.Principal,
                    TransactionDate = l.DisbursedAt ?? l.AppliedAt,
                    ReferenceId = l.Id,
                    ReferenceType = "LOAN"
                }));

                // Loan payments (outflow) - principal portion
                var principalPayments = 0m;
                foreach (var payment in loanPayments)
                {
                    var journalEntries = await _context.JournalEntries
                        .Include(je => je.JournalEntryLines)
                        .Where(je => je.Reference == payment.Id.ToString() || 
                                    (je.LoanId != null && je.LoanId == payment.LoanId &&
                                     je.EntryDate >= periodStart && je.EntryDate <= periodEnd))
                        .ToListAsync();

                    foreach (var entry in journalEntries)
                    {
                        var principalLine = entry.JournalEntryLines
                            .FirstOrDefault(jel => jel.AccountName.Contains("Loan Payable", StringComparison.OrdinalIgnoreCase) ||
                                                  jel.AccountName.Contains("Principal", StringComparison.OrdinalIgnoreCase));
                        if (principalLine != null && principalLine.EntrySide == "DEBIT")
                        {
                            principalPayments += principalLine.Amount;
                        }
                    }
                }

                financing.PrincipalPayments = principalPayments;
                financing.LoanPayments = loanPayments.Sum(t => t.Amount);
                financing.OutflowItems.AddRange(loanPayments.Select(t => new DTOs.CashFlowItemDto
                {
                    Description = $"Loan Payment - {t.Description ?? "Loan Payment"}",
                    Category = "Loan Payment",
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate ?? DateTime.UtcNow,
                    ReferenceId = t.LoanId,
                    ReferenceType = "LOAN"
                }));

                // Calculate ending cash balance
                var endingBalance = bankAccounts.Sum(ba => ba.CurrentBalance);

                // Build Cash Flow Statement
                var cashFlowStatement = new DTOs.CashFlowStatementDto
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Period = period,
                    OperatingActivities = operating,
                    InvestingActivities = investing,
                    FinancingActivities = financing,
                    BeginningCash = beginningBalance,
                    EndingCash = endingBalance
                };

                return ApiResponse<DTOs.CashFlowStatementDto>.SuccessResult(cashFlowStatement);
            }
            catch (Exception ex)
            {
                return ApiResponse<DTOs.CashFlowStatementDto>.ErrorResult($"Error generating cash flow statement: {ex.Message}");
            }
        }

        // ==========================================
        // INCOME STATEMENT
        // ==========================================

        public async Task<ApiResponse<DTOs.IncomeStatementDto>> GetIncomeStatementAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, string period = "MONTHLY", bool includeComparison = false)
        {
            try
            {
                // Determine date range
                DateTime periodStart, periodEnd;
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    periodStart = startDate.Value;
                    periodEnd = endDate.Value;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    periodStart = new DateTime(now.Year, now.Month, 1);
                    
                    periodEnd = period.ToUpper() switch
                    {
                        "QUARTERLY" => periodStart.AddMonths(3).AddDays(-1),
                        "YEARLY" => periodStart.AddYears(1).AddDays(-1),
                        _ => periodStart.AddMonths(1).AddDays(-1) // MONTHLY
                    };
                }

                // REVENUE SECTION
                var revenue = new DTOs.RevenueSectionDto();

                // Get income sources
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();
                
                // Filter out deleted income sources in memory (IsDeleted may not be mapped)
                incomeSources = incomeSources.Where(i => !i.IsDeleted).ToList();

                // Calculate period income based on frequency
                var monthsInPeriod = CalculateMonthsInPeriod(periodStart, periodEnd);
                
                foreach (var income in incomeSources)
                {
                    var periodAmount = CalculatePeriodIncomeAmount(income, periodStart, periodEnd, monthsInPeriod);
                    var category = income.Category?.ToUpper() ?? "OTHER";
                    var name = income.Name?.ToUpper() ?? "";

                    // Categorize income
                    if (category.Contains("PRIMARY") || name.Contains("SALARY") || name.Contains("WAGE"))
                    {
                        revenue.SalaryIncome += periodAmount;
                    }
                    else if (category.Contains("BUSINESS"))
                    {
                        revenue.BusinessIncome += periodAmount;
                    }
                    else if (category.Contains("SIDE_HUSTLE") || name.Contains("FREELANCE") || name.Contains("CONTRACT"))
                    {
                        revenue.FreelanceIncome += periodAmount;
                    }
                    else if (category.Contains("INVESTMENT") || category.Contains("DIVIDEND"))
                    {
                        revenue.InvestmentIncome += periodAmount;
                        revenue.DividendIncome += periodAmount;
                    }
                    else if (category.Contains("INTEREST"))
                    {
                        revenue.InterestIncome += periodAmount;
                    }
                    else if (category.Contains("RENTAL") || category.Contains("RENT"))
                    {
                        revenue.RentalIncome += periodAmount;
                    }
                    else
                    {
                        revenue.OtherOperatingRevenue += periodAmount;
                    }

                    revenue.RevenueItems.Add(new DTOs.IncomeStatementItemDto
                    {
                        AccountName = income.Name,
                        Category = income.Category ?? "OTHER",
                        Amount = periodAmount,
                        Description = income.Description,
                        ReferenceId = income.Id,
                        ReferenceType = "INCOME_SOURCE"
                    });
                }

                revenue.TotalOperatingRevenue = revenue.SalaryIncome + revenue.BusinessIncome + 
                                               revenue.FreelanceIncome + revenue.OtherOperatingRevenue;
                revenue.TotalOtherRevenue = revenue.InvestmentIncome + revenue.InterestIncome + 
                                          revenue.RentalIncome + revenue.DividendIncome + revenue.OtherIncome;

                // EXPENSES SECTION
                var expenses = new DTOs.ExpensesSectionDto();

                // Get bills
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId &&
                               b.DueDate >= periodStart && b.DueDate <= periodEnd)
                    .ToListAsync();
                
                // Filter out deleted bills in memory (IsDeleted may not be mapped)
                bills = bills.Where(b => !b.IsDeleted).ToList();

                foreach (var bill in bills)
                {
                    var billType = bill.BillType?.ToUpper() ?? "";
                    var amount = bill.Amount;

                    if (billType.Contains("UTILITY") || billType.Contains("ELECTRIC") || 
                        billType.Contains("WATER") || billType.Contains("GAS"))
                    {
                        expenses.UtilitiesExpense += amount;
                    }
                    else if (billType.Contains("INSURANCE"))
                    {
                        expenses.InsuranceExpense += amount;
                    }
                    else if (billType.Contains("SUBSCRIPTION"))
                    {
                        expenses.SubscriptionExpense += amount;
                    }
                    else
                    {
                        expenses.OtherOperatingExpenses += amount;
                    }

                    expenses.ExpenseItems.Add(new DTOs.IncomeStatementItemDto
                    {
                        AccountName = bill.Provider ?? "Bill",
                        Category = bill.BillType ?? "OTHER",
                        Amount = amount,
                        Description = $"{bill.BillType} - {bill.Provider}",
                        ReferenceId = bill.Id,
                        ReferenceType = "BILL"
                    });
                }

                // Get variable expenses
                var variableExpenses = await _context.VariableExpenses
                    .Where(v => v.UserId == userId &&
                               v.ExpenseDate >= periodStart && v.ExpenseDate <= periodEnd)
                    .ToListAsync();
                
                // Filter out deleted variable expenses in memory (IsDeleted may not be mapped)
                variableExpenses = variableExpenses.Where(v => !v.IsDeleted).ToList();

                foreach (var expense in variableExpenses)
                {
                    var category = expense.Category?.ToUpper() ?? "";
                    var amount = expense.Amount;

                    if (category.Contains("FOOD") || category.Contains("GROCERIES") || category.Contains("RESTAURANT"))
                    {
                        expenses.FoodExpense += amount;
                    }
                    else if (category.Contains("TRANSPORT") || category.Contains("GAS") || category.Contains("TAXI"))
                    {
                        expenses.TransportationExpense += amount;
                    }
                    else if (category.Contains("HEALTH") || category.Contains("MEDICAL") || category.Contains("DOCTOR"))
                    {
                        expenses.HealthcareExpense += amount;
                    }
                    else if (category.Contains("EDUCATION") || category.Contains("COURSE"))
                    {
                        expenses.EducationExpense += amount;
                    }
                    else if (category.Contains("ENTERTAINMENT") || category.Contains("MOVIE") || category.Contains("GAME"))
                    {
                        expenses.EntertainmentExpense += amount;
                    }
                    else
                    {
                        expenses.OtherOperatingExpenses += amount;
                    }

                    expenses.ExpenseItems.Add(new DTOs.IncomeStatementItemDto
                    {
                        AccountName = expense.Category ?? "Expense",
                        Category = expense.Category ?? "OTHER",
                        Amount = amount,
                        Description = expense.Description,
                        ReferenceId = expense.Id,
                        ReferenceType = "VARIABLE_EXPENSE"
                    });
                }

                // Get loan interest payments
                var loanPayments = await _context.Payments
                    .Where(p => p.UserId == userId &&
                               p.TransactionType == "DEBIT" &&
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= periodStart &&
                               p.TransactionDate <= periodEnd &&
                               p.LoanId != null)
                    .ToListAsync();

                foreach (var payment in loanPayments)
                {
                    // Get interest portion from journal entries
                    var journalEntries = await _context.JournalEntries
                        .Include(je => je.JournalEntryLines)
                        .Where(je => je.Reference == payment.Id.ToString() || 
                                    (je.LoanId != null && je.LoanId == payment.LoanId &&
                                     je.EntryDate >= periodStart && je.EntryDate <= periodEnd))
                        .ToListAsync();

                    foreach (var entry in journalEntries)
                    {
                        var interestLine = entry.JournalEntryLines
                            .FirstOrDefault(jel => jel.AccountName.Contains("Interest", StringComparison.OrdinalIgnoreCase));
                        if (interestLine != null && interestLine.EntrySide == "DEBIT")
                        {
                            expenses.InterestExpense += interestLine.Amount;
                        }
                    }
                }

                expenses.TotalOperatingExpenses = expenses.UtilitiesExpense + expenses.RentExpense +
                                                 expenses.InsuranceExpense + expenses.SubscriptionExpense +
                                                 expenses.FoodExpense + expenses.TransportationExpense +
                                                 expenses.HealthcareExpense + expenses.EducationExpense +
                                                 expenses.EntertainmentExpense + expenses.OtherOperatingExpenses;
                expenses.TotalFinancialExpenses = expenses.InterestExpense + expenses.LoanFeesExpense;

                // COMPARISON (if requested)
                DTOs.IncomeStatementComparisonDto? comparison = null;
                if (includeComparison)
                {
                    var (prevStart, prevEnd) = GetPreviousPeriod(periodStart, periodEnd);
                    var prevRevenue = await CalculateTotalIncomeAsync(userId, prevStart, prevEnd);
                    var prevExpenses = await CalculateTotalExpensesAsync(userId, prevStart, prevEnd);
                    var prevNetIncome = prevRevenue - prevExpenses;

                    comparison = new DTOs.IncomeStatementComparisonDto
                    {
                        PreviousRevenue = prevRevenue,
                        PreviousExpenses = prevExpenses,
                        PreviousNetIncome = prevNetIncome,
                        RevenueChange = revenue.TotalRevenue - prevRevenue,
                        RevenueChangePercentage = CalculatePercentageChange(prevRevenue, revenue.TotalRevenue),
                        ExpensesChange = expenses.TotalExpenses - prevExpenses,
                        ExpensesChangePercentage = CalculatePercentageChange(prevExpenses, expenses.TotalExpenses),
                        NetIncomeChange = (revenue.TotalRevenue - expenses.TotalExpenses) - prevNetIncome,
                        NetIncomeChangePercentage = CalculatePercentageChange(prevNetIncome, revenue.TotalRevenue - expenses.TotalExpenses)
                    };
                }

                // Build Income Statement
                var incomeStatement = new DTOs.IncomeStatementDto
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Period = period,
                    Revenue = revenue,
                    Expenses = expenses,
                    Comparison = comparison
                };

                return ApiResponse<DTOs.IncomeStatementDto>.SuccessResult(incomeStatement);
            }
            catch (Exception ex)
            {
                return ApiResponse<DTOs.IncomeStatementDto>.ErrorResult($"Error generating income statement: {ex.Message}");
            }
        }

        private decimal CalculatePeriodIncomeAmount(Entities.IncomeSource income, DateTime startDate, DateTime endDate, decimal monthsInPeriod)
        {
            var frequency = income.Frequency?.ToUpper() ?? "MONTHLY";
            
            return frequency switch
            {
                "WEEKLY" => income.Amount * (decimal)((endDate - startDate).TotalDays / 7.0),
                "BIWEEKLY" or "BI_WEEKLY" => income.Amount * (decimal)((endDate - startDate).TotalDays / 14.0),
                "MONTHLY" => income.Amount * monthsInPeriod,
                "QUARTERLY" => income.Amount * (monthsInPeriod / 3m),
                "YEARLY" or "ANNUALLY" => income.Amount * (monthsInPeriod / 12m),
                _ => income.Amount * monthsInPeriod
            };
        }

        private decimal CalculateMonthsInPeriod(DateTime startDate, DateTime endDate)
        {
            var days = (endDate - startDate).TotalDays;
            return (decimal)(days / 30.0); // Approximate months
        }

        private async Task<decimal> CalculateNetWorthAtDateAsync(string userId, DateTime asOfDate)
        {
            try
            {
                // Optimize: Use a single database query batch where possible
                // For simplicity, calculate current net worth
                // In a production system, you'd want to calculate historical net worth based on transaction history
                // For now, we'll just return the current calculation (same for all recent dates)
                
                // Fetch all data in parallel batches for better performance
                var bankAccountsTask = _context.BankAccounts
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                var activeLoansTask = _context.Loans
                    .Where(l => l.UserId == userId &&
                               !string.IsNullOrWhiteSpace(l.Status) &&
                               l.Status.Trim().ToUpper() != "REJECTED" && 
                               l.Status.Trim().ToUpper() != "COMPLETED")
                    .ToListAsync();

                var bankAccounts = await bankAccountsTask;
                var activeLoans = await activeLoansTask;

                var totalBankBalance = bankAccounts.Sum(ba => ba.CurrentBalance);
                var totalSavings = await CalculateTotalSavingsAsync(userId);
                var totalAssets = totalBankBalance + totalSavings;

                var totalLiabilities = activeLoans.Sum(l => l.RemainingBalance);

                return totalAssets - totalLiabilities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NET WORTH CALC ERROR] Error calculating net worth at {asOfDate:yyyy-MM-dd}: {ex.Message}");
                return 0;
            }
        }

        private async Task<List<TrendDataPoint>> GenerateNetWorthTrendAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var trend = new List<TrendDataPoint>();
            
            // Optimize: Only calculate trend if there's a meaningful date range
            var daysDifference = (endDate - startDate).TotalDays;
            
            // For very short periods (same month or less), just return current value
            if (daysDifference <= 31)
            {
                var currentNetWorth = await CalculateNetWorthAtDateAsync(userId, endDate);
                trend.Add(new TrendDataPoint
                {
                    Date = endDate,
                    Label = $"{endDate:MMM yyyy}",
                    Value = currentNetWorth
                });
                return trend;
            }

            // For longer periods, limit to max 12 data points (avoid too many queries)
            var maxDataPoints = 12;
            var monthsToProcess = (int)Math.Ceiling(daysDifference / 30.0);
            var monthsPerPoint = Math.Max(1, monthsToProcess / maxDataPoints);
            
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            var processedCount = 0;
            
            while (currentDate <= endDate && processedCount < maxDataPoints)
            {
                var monthEnd = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1).AddDays(-1);
                if (monthEnd > endDate) monthEnd = endDate;

                // Use current net worth calculation (faster, same value for recent dates)
                var netWorth = await CalculateNetWorthAtDateAsync(userId, monthEnd);
                
                trend.Add(new TrendDataPoint
                {
                    Date = monthEnd,
                    Label = $"{monthEnd:MMM yyyy}",
                    Value = netWorth
                });

                currentDate = currentDate.AddMonths(monthsPerPoint);
                processedCount++;
            }

            return trend;
        }

        private string GenerateTrendDescription(decimal change, decimal changePercentage, decimal currentNetWorth)
        {
            if (change == 0)
            {
                return currentNetWorth >= 0 
                    ? "Your net worth has remained stable." 
                    : "Your net worth remains negative. Focus on reducing debt and increasing savings.";
            }

            var isPositive = change > 0;
            var isNegativeNetWorth = currentNetWorth < 0;
            
            if (isPositive && !isNegativeNetWorth)
            {
                return $"Great progress! Your net worth increased by {changePercentage:F1}% ({change:C}). You're building wealth.";
            }
            else if (isPositive && isNegativeNetWorth)
            {
                return $"You're improving! Net worth increased by {changePercentage:F1}% ({change:C}), but you're still in debt. Keep paying down loans.";
            }
            else if (!isPositive && !isNegativeNetWorth)
            {
                return $"Net worth decreased by {changePercentage:F1}% ({change:C}). Review your spending and debt management.";
            }
            else
            {
                return $"Net worth decreased by {changePercentage:F1}% ({change:C}). Your debt increased. Prioritize debt reduction.";
            }
        }

        // ==========================================
        // INSIGHTS (STUB)
        // ==========================================

        public async Task<ApiResponse<List<FinancialInsightDto>>> GetFinancialInsightsAsync(string userId, DateTime? date = null)
        {
            return await Task.FromResult(ApiResponse<List<FinancialInsightDto>>.ErrorResult("Not implemented yet"));
        }

        // ==========================================
        // PREDICTIONS
        // ==========================================

        public async Task<ApiResponse<List<FinancialPredictionDto>>> GetFinancialPredictionsAsync(string userId)
        {
            try
            {
                var predictions = new List<FinancialPredictionDto>();
                var now = DateTime.UtcNow;
                var nextMonth = now.AddMonths(1);
                var next3Months = now.AddMonths(3);
                var next6Months = now.AddMonths(6);

                // 1. Predict Next Month Income
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();
                
                if (incomeSources.Any())
                {
                    var predictedIncome = incomeSources.Sum(i => i.MonthlyAmount);
                    predictions.Add(new FinancialPredictionDto
                    {
                        Type = "INCOME",
                        Description = "Projected monthly income for next month",
                        PredictedAmount = predictedIncome,
                        PredictionDate = nextMonth,
                        Confidence = 85 // High confidence for active income sources
                    });
                }

                // 2. Predict Next Month Expenses
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId && !b.IsDeleted &&
                                b.DueDate >= nextMonth && b.DueDate < nextMonth.AddMonths(1))
                    .ToListAsync();
                
                var predictedBills = bills.Sum(b => b.Amount);
                
                // Get average variable expenses from last 3 months
                var threeMonthsAgo = now.AddMonths(-3);
                var variableExpenses = await _context.VariableExpenses
                    .Where(v => v.UserId == userId && !v.IsDeleted &&
                                v.ExpenseDate >= threeMonthsAgo && v.ExpenseDate <= now)
                    .ToListAsync();
                
                var avgVariableExpenses = variableExpenses.Any()
                    ? variableExpenses.GroupBy(v => new { v.ExpenseDate.Year, v.ExpenseDate.Month })
                        .Select(g => g.Sum(v => v.Amount))
                        .Average()
                    : 0;
                
                var predictedExpenses = predictedBills + (decimal)avgVariableExpenses;
                
                predictions.Add(new FinancialPredictionDto
                {
                    Type = "EXPENSE",
                    Description = "Projected monthly expenses for next month",
                    PredictedAmount = predictedExpenses,
                    PredictionDate = nextMonth,
                    Confidence = predictedBills > 0 ? 75 : 60 // Higher confidence if bills are scheduled
                });

                // 3. Predict Savings Growth (6 months)
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(s => s.UserId == userId && !s.IsDeleted)
                    .ToListAsync();
                
                if (savingsAccounts.Any())
                {
                    var currentSavings = savingsAccounts.Sum(s => s.CurrentBalance);
                    // Calculate monthly savings target from target amount and date
                    var monthlySavings = 0m;
                    foreach (var savings in savingsAccounts)
                    {
                        if (savings.TargetDate > now)
                        {
                            var monthsRemaining = Math.Max(1, 
                                (savings.TargetDate.Year - now.Year) * 12 + 
                                (savings.TargetDate.Month - now.Month));
                            var remainingNeeded = savings.TargetAmount - savings.CurrentBalance;
                            if (remainingNeeded > 0)
                            {
                                monthlySavings += remainingNeeded / monthsRemaining;
                            }
                        }
                    }
                    var predictedSavings = currentSavings + (monthlySavings * 6);
                    
                    predictions.Add(new FinancialPredictionDto
                    {
                        Type = "SAVINGS",
                        Description = "Projected savings balance in 6 months",
                        PredictedAmount = predictedSavings,
                        PredictionDate = next6Months,
                        Confidence = monthlySavings > 0 ? 70 : 50
                    });
                }

                // 4. Predict Net Worth (3 months)
                var bankAccounts = await _context.BankAccounts
                    .Where(b => b.UserId == userId && !b.IsDeleted)
                    .ToListAsync();
                
                var activeLoans = await _context.Loans
                    .Where(l => l.UserId == userId &&
                               l.Status != "REJECTED" && l.Status != "COMPLETED")
                    .ToListAsync();
                
                var currentAssets = bankAccounts.Sum(b => b.CurrentBalance) + savingsAccounts.Sum(s => s.CurrentBalance);
                var currentLiabilities = activeLoans.Sum(l => l.RemainingBalance);
                var currentNetWorth = currentAssets - currentLiabilities;
                
                // Project future net worth
                var monthlyIncome = incomeSources.Sum(i => i.MonthlyAmount);
                var monthlyLoanPayments = activeLoans.Sum(l => l.MonthlyPayment);
                var monthlyNetChange = monthlyIncome - predictedExpenses - monthlyLoanPayments;
                var predictedNetWorth = currentNetWorth + (monthlyNetChange * 3);
                
                predictions.Add(new FinancialPredictionDto
                {
                    Type = "NETWORTH",
                    Description = "Projected net worth in 3 months",
                    PredictedAmount = predictedNetWorth,
                    PredictionDate = next3Months,
                    Confidence = 65
                });

                // 5. Predict Bill Trends (3 months average)
                var billsLast3Months = await _context.Bills
                    .Where(b => b.UserId == userId && !b.IsDeleted &&
                                b.DueDate >= threeMonthsAgo && b.DueDate <= now)
                    .ToListAsync();
                
                if (billsLast3Months.Any())
                {
                    var avgBills = billsLast3Months
                        .GroupBy(b => new { b.DueDate.Year, b.DueDate.Month })
                        .Select(g => g.Sum(b => b.Amount))
                        .Average();
                    
                    predictions.Add(new FinancialPredictionDto
                    {
                        Type = "BILL",
                        Description = "Average monthly bills based on 3-month trend",
                        PredictedAmount = (decimal)avgBills,
                        PredictionDate = nextMonth,
                        Confidence = 80
                    });
                }

                return ApiResponse<List<FinancialPredictionDto>>.SuccessResult(predictions);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FinancialPredictionDto>>.ErrorResult($"Error generating predictions: {ex.Message}");
            }
        }

        // ==========================================
        // CASH FLOW PROJECTIONS
        // ==========================================

        public async Task<ApiResponse<CashFlowProjectionDto>> GetCashFlowProjectionAsync(string userId, int monthsAhead = 6)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startingDate = new DateTime(now.Year, now.Month, 1);
                var endingDate = startingDate.AddMonths(monthsAhead);

                // Get starting balance (bank accounts)
                var bankAccounts = await _context.BankAccounts
                    .Where(b => b.UserId == userId && !b.IsDeleted)
                    .ToListAsync();
                
                var startingBalance = bankAccounts.Sum(b => b.CurrentBalance);

                // Get income sources
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();
                
                // Filter out deleted income sources in memory (IsDeleted may not be mapped)
                incomeSources = incomeSources.Where(i => !i.IsDeleted).ToList();
                
                // Calculate monthly income using the computed MonthlyAmount property
                var monthlyIncome = incomeSources.Sum(i => i.MonthlyAmount);

                // Get active loans
                var activeLoans = await _context.Loans
                    .Where(l => l.UserId == userId &&
                               l.Status != "REJECTED" && l.Status != "COMPLETED")
                    .ToListAsync();
                
                var monthlyLoanPayments = activeLoans.Sum(l => l.MonthlyPayment);

                // Get savings targets - calculate monthly target from target amount and date
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(s => s.UserId == userId && !s.IsDeleted && s.IsActive)
                    .ToListAsync();
                
                var monthlySavings = 0m;
                foreach (var savings in savingsAccounts)
                {
                    if (savings.TargetDate > now)
                    {
                        var monthsRemaining = Math.Max(1, 
                            (savings.TargetDate.Year - now.Year) * 12 + 
                            (savings.TargetDate.Month - now.Month));
                        var remainingNeeded = savings.TargetAmount - savings.CurrentBalance;
                        if (remainingNeeded > 0)
                        {
                            monthlySavings += remainingNeeded / monthsRemaining;
                        }
                    }
                }

                // Get average variable expenses from last 3 months
                var threeMonthsAgo = now.AddMonths(-3);
                var variableExpenses = await _context.VariableExpenses
                    .Where(v => v.UserId == userId && !v.IsDeleted &&
                                v.ExpenseDate >= threeMonthsAgo && v.ExpenseDate <= now)
                    .ToListAsync();
                
                var avgVariableExpenses = variableExpenses.Any()
                    ? variableExpenses.GroupBy(v => new { v.ExpenseDate.Year, v.ExpenseDate.Month })
                        .Select(g => g.Sum(v => v.Amount))
                        .Average()
                    : 0;

                // Build monthly breakdown
                var monthlyBreakdown = new List<MonthlyCashFlowProjectionDto>();
                var currentBalance = startingBalance;
                var totalProjectedIncome = 0m;
                var totalProjectedExpenses = 0m;
                var totalProjectedBills = 0m;
                var totalProjectedLoanPayments = 0m;
                var totalProjectedSavings = 0m;

                for (int monthOffset = 0; monthOffset < monthsAhead; monthOffset++)
                {
                    var monthStart = startingDate.AddMonths(monthOffset);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    // Get bills for this month
                    var billsForMonth = await _context.Bills
                        .Where(b => b.UserId == userId && !b.IsDeleted &&
                                    b.DueDate >= monthStart && b.DueDate <= monthEnd)
                        .ToListAsync();
                    
                    var billsAmount = billsForMonth.Sum(b => b.Amount);
                    var expensesAmount = (decimal)avgVariableExpenses;
                    var totalExpenses = billsAmount + expensesAmount;

                    // Calculate net flow for this month
                    var netFlow = monthlyIncome - totalExpenses - monthlyLoanPayments - monthlySavings;
                    var endingBalance = currentBalance + netFlow;

                    monthlyBreakdown.Add(new MonthlyCashFlowProjectionDto
                    {
                        Month = monthStart,
                        StartingBalance = currentBalance,
                        Income = monthlyIncome,
                        Expenses = expensesAmount,
                        Bills = billsAmount,
                        LoanPayments = monthlyLoanPayments,
                        Savings = monthlySavings,
                        EndingBalance = endingBalance,
                        NetFlow = netFlow
                    });

                    totalProjectedIncome += monthlyIncome;
                    totalProjectedExpenses += expensesAmount;
                    totalProjectedBills += billsAmount;
                    totalProjectedLoanPayments += monthlyLoanPayments;
                    totalProjectedSavings += monthlySavings;

                    currentBalance = endingBalance;
                }

                var projection = new CashFlowProjectionDto
                {
                    ProjectionDate = now,
                    MonthsAhead = monthsAhead,
                    StartingBalance = startingBalance,
                    ProjectedIncome = totalProjectedIncome,
                    ProjectedExpenses = totalProjectedExpenses,
                    ProjectedBills = totalProjectedBills,
                    ProjectedLoanPayments = totalProjectedLoanPayments,
                    ProjectedSavings = totalProjectedSavings,
                    ProjectedEndingBalance = currentBalance,
                    NetCashFlow = currentBalance - startingBalance,
                    MonthlyBreakdown = monthlyBreakdown
                };

                return ApiResponse<CashFlowProjectionDto>.SuccessResult(projection);
            }
            catch (Exception ex)
            {
                return ApiResponse<CashFlowProjectionDto>.ErrorResult($"Error generating cash flow projection: {ex.Message}");
            }
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
                if (loanResult.Success)
                {
                    report.LoanReport = loanResult.Data!;
                    Console.WriteLine($"[FULL REPORT] Loan report: {loanResult.Data!.ActiveLoansCount} loans");
                }
                else
                {
                    report.LoanReport = new LoanReportDto();
                    Console.WriteLine($"[FULL REPORT ERROR] Loan report failed: {loanResult.Message}");
                }

                var savingsResult = await GetSavingsReportAsync(userId, query);
                if (savingsResult.Success) report.SavingsReport = savingsResult.Data!;

                var netWorthResult = await GetNetWorthReportAsync(userId, query);
                if (netWorthResult.Success)
                {
                    report.NetWorthReport = netWorthResult.Data!;
                    Console.WriteLine($"[FULL REPORT] Net Worth report: {netWorthResult.Data!.CurrentNetWorth:C}");
                }
                else
                {
                    report.NetWorthReport = new NetWorthReportDto(); // Ensure it's not null even if fails
                    Console.WriteLine($"[FULL REPORT ERROR] Net Worth report failed: {netWorthResult.Message}");
                }

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
            // Note: IsDeleted is ignored in DbContext, so we don't filter by it
            var incomeSources = await _context.IncomeSources
                .Where(i => i.UserId == userId && i.IsActive)
                .ToListAsync();

            return incomeSources.Sum(i => i.MonthlyAmount);
        }

        private async Task<decimal> CalculateTotalExpensesAsync(string userId, DateTime startDate, DateTime endDate)
        {
            // Note: IsDeleted is ignored in DbContext, so we don't filter by it
            var bills = await _context.Bills
                .Where(b => b.UserId == userId &&
                            b.DueDate >= startDate && b.DueDate <= endDate)
                .SumAsync(b => b.Amount);

            // Note: IsDeleted is ignored in DbContext, so we don't filter by it
            var variableExpenses = await _context.VariableExpenses
                .Where(v => v.UserId == userId &&
                            v.ExpenseDate >= startDate && v.ExpenseDate <= endDate)
                .SumAsync(v => v.Amount);

            return bills + variableExpenses;
        }

        private async Task<decimal> CalculateBillsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            // Note: IsDeleted is ignored in DbContext, so we don't filter by it
            return await _context.Bills
                .Where(b => b.UserId == userId &&
                            b.DueDate >= startDate && b.DueDate <= endDate)
                .SumAsync(b => b.Amount);
        }

        private async Task<decimal> CalculateTotalSavingsAsync(string userId)
        {
            // Note: IsDeleted is ignored in DbContext, so we don't filter by it
            var savings = await _context.SavingsAccounts
                .Where(sa => sa.UserId == userId)
                .SelectMany(sa => _context.SavingsTransactions
                    .Where(st => st.SavingsAccountId == sa.Id))
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
            // Calculate total assets (Bank Accounts + Savings)
            var bankAccounts = await _context.BankAccounts
                .Where(ba => ba.UserId == userId && ba.IsActive)
                .ToListAsync();

            var totalBankBalance = bankAccounts.Sum(ba => ba.CurrentBalance);
            var totalSavings = await CalculateTotalSavingsAsync(userId);
            var totalAssets = totalBankBalance + totalSavings;

            // Calculate total liabilities (Active Loans)
            var activeLoans = await _context.Loans
                .Where(l => l.UserId == userId &&
                           !string.IsNullOrWhiteSpace(l.Status) &&
                           l.Status.Trim().ToUpper() != "REJECTED" && 
                           l.Status.Trim().ToUpper() != "COMPLETED")
                .ToListAsync();

            var totalLiabilities = activeLoans.Sum(l => l.RemainingBalance);

            // Net Worth = Assets - Liabilities
            return totalAssets - totalLiabilities;
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

        // ==========================================
        // FINANCIAL RATIOS
        // ==========================================

        public async Task<ApiResponse<FinancialRatiosDto>> GetFinancialRatiosAsync(string userId, DateTime? asOfDate = null)
        {
            try
            {
                var reportDate = asOfDate ?? DateTime.UtcNow;

                // Get Balance Sheet data
                var balanceSheetResult = await GetBalanceSheetAsync(userId, reportDate);
                if (!balanceSheetResult.Success)
                {
                    return ApiResponse<FinancialRatiosDto>.ErrorResult("Failed to generate balance sheet for ratios calculation");
                }
                var balanceSheet = balanceSheetResult.Data!;

                // Get Income Statement data (for the period ending at reportDate)
                var yearStart = new DateTime(reportDate.Year, 1, 1);
                var incomeStatementResult = await GetIncomeStatementAsync(userId, yearStart, reportDate, "YEARLY");
                if (!incomeStatementResult.Success)
                {
                    return ApiResponse<FinancialRatiosDto>.ErrorResult("Failed to generate income statement for ratios calculation");
                }
                var incomeStatement = incomeStatementResult.Data!;

                // Calculate liquidity ratios
                var liquidity = new LiquidityRatiosDto
                {
                    CurrentAssets = balanceSheet.Assets.TotalCurrentAssets,
                    CurrentLiabilities = balanceSheet.Liabilities.TotalCurrentLiabilities,
                    CashAndEquivalents = balanceSheet.Assets.CurrentAssets
                        .Where(a => a.AccountType.Contains("Bank", StringComparison.OrdinalIgnoreCase))
                        .Sum(a => a.Amount)
                };

                liquidity.CurrentRatio = liquidity.CurrentLiabilities > 0
                    ? liquidity.CurrentAssets / liquidity.CurrentLiabilities
                    : liquidity.CurrentAssets > 0 ? 999m : 0m;
                liquidity.CurrentRatioInterpretation = InterpretCurrentRatio(liquidity.CurrentRatio);

                liquidity.QuickRatio = liquidity.CurrentLiabilities > 0
                    ? (liquidity.CurrentAssets - balanceSheet.Assets.CurrentAssets
                        .Where(a => a.AccountType.Contains("Savings", StringComparison.OrdinalIgnoreCase))
                        .Sum(a => a.Amount)) / liquidity.CurrentLiabilities
                    : liquidity.CurrentAssets > 0 ? 999m : 0m;
                liquidity.QuickRatioInterpretation = InterpretQuickRatio(liquidity.QuickRatio);

                liquidity.CashRatio = liquidity.CurrentLiabilities > 0
                    ? liquidity.CashAndEquivalents / liquidity.CurrentLiabilities
                    : liquidity.CashAndEquivalents > 0 ? 999m : 0m;
                liquidity.CashRatioInterpretation = InterpretCashRatio(liquidity.CashRatio);

                // Calculate debt ratios
                var debt = new DebtRatiosDto
                {
                    TotalLiabilities = balanceSheet.Liabilities.TotalLiabilities,
                    TotalAssets = balanceSheet.Assets.TotalAssets,
                    TotalEquity = balanceSheet.Equity.TotalEquity,
                    NetIncome = incomeStatement.NetIncome
                };

                // Calculate total debt payments (monthly loan payments)
                var activeLoans = await _context.Loans
                    .Where(l => l.UserId == userId &&
                               !string.IsNullOrWhiteSpace(l.Status) &&
                               l.Status.Trim().ToUpper() != "REJECTED" &&
                               l.Status.Trim().ToUpper() != "COMPLETED")
                    .ToListAsync();
                debt.TotalDebtPayments = activeLoans.Sum(l => l.MonthlyPayment) * 12; // Annualized

                debt.DebtToEquityRatio = debt.TotalEquity > 0
                    ? debt.TotalLiabilities / debt.TotalEquity
                    : debt.TotalLiabilities > 0 ? 999m : 0m;
                debt.DebtToEquityInterpretation = InterpretDebtToEquity(debt.DebtToEquityRatio);

                debt.DebtToAssetsRatio = debt.TotalAssets > 0
                    ? debt.TotalLiabilities / debt.TotalAssets
                    : 0m;
                debt.DebtToAssetsInterpretation = InterpretDebtToAssets(debt.DebtToAssetsRatio);

                debt.DebtServiceCoverageRatio = debt.TotalDebtPayments > 0
                    ? debt.NetIncome / debt.TotalDebtPayments
                    : debt.NetIncome > 0 ? 999m : 0m;
                debt.DebtServiceCoverageInterpretation = InterpretDebtServiceCoverage(debt.DebtServiceCoverageRatio);

                // Calculate profitability ratios
                var profitability = new ProfitabilityRatiosDto
                {
                    NetIncome = incomeStatement.NetIncome,
                    TotalRevenue = incomeStatement.Revenue.TotalRevenue,
                    TotalAssets = balanceSheet.Assets.TotalAssets,
                    TotalEquity = balanceSheet.Equity.TotalEquity
                };

                profitability.NetProfitMargin = profitability.TotalRevenue > 0
                    ? (profitability.NetIncome / profitability.TotalRevenue) * 100
                    : 0m;
                profitability.NetProfitMarginInterpretation = InterpretNetProfitMargin(profitability.NetProfitMargin);

                profitability.ReturnOnAssets = profitability.TotalAssets > 0
                    ? (profitability.NetIncome / profitability.TotalAssets) * 100
                    : 0m;
                profitability.ReturnOnAssetsInterpretation = InterpretReturnOnAssets(profitability.ReturnOnAssets);

                profitability.ReturnOnEquity = profitability.TotalEquity > 0
                    ? (profitability.NetIncome / profitability.TotalEquity) * 100
                    : profitability.NetIncome > 0 ? 999m : 0m;
                profitability.ReturnOnEquityInterpretation = InterpretReturnOnEquity(profitability.ReturnOnEquity);

                // Calculate efficiency ratios
                var efficiency = new EfficiencyRatiosDto
                {
                    TotalRevenue = incomeStatement.Revenue.TotalRevenue,
                    TotalAssets = balanceSheet.Assets.TotalAssets,
                    TotalExpenses = incomeStatement.Expenses.TotalExpenses,
                    TotalIncome = incomeStatement.Revenue.TotalRevenue
                };

                // Calculate total savings
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId)
                    .ToListAsync();
                foreach (var savingsAccount in savingsAccounts)
                {
                    var savingsTransactions = await _context.SavingsTransactions
                        .Where(st => st.SavingsAccountId == savingsAccount.Id)
                        .ToListAsync();
                    efficiency.TotalSavings += savingsTransactions.Sum(st =>
                        st.TransactionType == "DEPOSIT" ? st.Amount : -st.Amount);
                }

                efficiency.AssetTurnover = efficiency.TotalAssets > 0
                    ? efficiency.TotalRevenue / efficiency.TotalAssets
                    : 0m;
                efficiency.AssetTurnoverInterpretation = InterpretAssetTurnover(efficiency.AssetTurnover);

                efficiency.ExpenseRatio = efficiency.TotalRevenue > 0
                    ? (efficiency.TotalExpenses / efficiency.TotalRevenue) * 100
                    : 0m;
                efficiency.ExpenseRatioInterpretation = InterpretExpenseRatio(efficiency.ExpenseRatio);

                efficiency.SavingsRate = efficiency.TotalIncome > 0
                    ? (efficiency.TotalSavings / efficiency.TotalIncome) * 100
                    : 0m;
                efficiency.SavingsRateInterpretation = InterpretSavingsRate(efficiency.SavingsRate);

                // Generate insights
                var insights = new List<RatioInsightDto>();
                insights.AddRange(GenerateRatioInsights(liquidity, debt, profitability, efficiency));

                var ratios = new FinancialRatiosDto
                {
                    AsOfDate = reportDate,
                    Liquidity = liquidity,
                    Debt = debt,
                    Profitability = profitability,
                    Efficiency = efficiency,
                    Insights = insights
                };

                return ApiResponse<FinancialRatiosDto>.SuccessResult(ratios);
            }
            catch (Exception ex)
            {
                return ApiResponse<FinancialRatiosDto>.ErrorResult($"Error calculating financial ratios: {ex.Message}");
            }
        }

        // ==========================================
        // TAX REPORTING
        // ==========================================

        public async Task<ApiResponse<TaxReportDto>> GetTaxReportAsync(string userId, int taxYear, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var periodStart = startDate ?? new DateTime(taxYear, 1, 1);
                var periodEnd = endDate ?? new DateTime(taxYear, 12, 31, 23, 59, 59);

                // Get Income Statement for the tax year
                var incomeStatementResult = await GetIncomeStatementAsync(userId, periodStart, periodEnd, "YEARLY");
                if (!incomeStatementResult.Success)
                {
                    return ApiResponse<TaxReportDto>.ErrorResult("Failed to generate income statement for tax report");
                }
                var incomeStatement = incomeStatementResult.Data!;

                // Build income summary
                var incomeSummary = new TaxIncomeSummaryDto
                {
                    TotalIncome = incomeStatement.Revenue.TotalRevenue,
                    SalaryIncome = incomeStatement.Revenue.SalaryIncome,
                    BusinessIncome = incomeStatement.Revenue.BusinessIncome,
                    FreelanceIncome = incomeStatement.Revenue.FreelanceIncome,
                    InvestmentIncome = incomeStatement.Revenue.InvestmentIncome,
                    InterestIncome = incomeStatement.Revenue.InterestIncome,
                    RentalIncome = incomeStatement.Revenue.RentalIncome,
                    DividendIncome = incomeStatement.Revenue.DividendIncome,
                    OtherIncome = incomeStatement.Revenue.OtherOperatingRevenue + incomeStatement.Revenue.OtherIncome
                };

                // Build income items from revenue items
                incomeSummary.IncomeItems = incomeStatement.Revenue.RevenueItems.Select(item => new TaxIncomeItemDto
                {
                    SourceName = item.AccountName,
                    IncomeType = MapIncomeType(item.Category),
                    Amount = item.Amount,
                    IncomeDate = periodStart, // Approximate - would need actual dates from source
                    IsTaxable = true,
                    ReferenceId = item.ReferenceId
                }).ToList();

                incomeSummary.TaxableIncome = incomeSummary.TotalIncome; // Simplified - would need adjustments

                // Build deductions
                var deductions = new TaxDeductionsDto();

                // Business expenses (from expense items)
                var businessExpenseItems = incomeStatement.Expenses.ExpenseItems
                    .Where(e => IsBusinessExpense(e.Category))
                    .ToList();
                deductions.BusinessExpenses = businessExpenseItems.Sum(e => e.Amount);
                deductions.BusinessExpenseItems = businessExpenseItems.Select(item => new TaxDeductionItemDto
                {
                    Description = item.Description ?? item.AccountName,
                    Category = item.Category,
                    Amount = item.Amount,
                    ExpenseDate = periodStart, // Approximate
                    IsDeductible = true,
                    ReferenceId = item.ReferenceId,
                    ReferenceType = item.ReferenceType
                }).ToList();

                // Personal deductions (utilities, rent, insurance, etc.)
                var personalExpenseItems = incomeStatement.Expenses.ExpenseItems
                    .Where(e => !IsBusinessExpense(e.Category))
                    .ToList();
                deductions.PersonalDeductions = personalExpenseItems.Sum(e => e.Amount);
                deductions.PersonalDeductionItems = personalExpenseItems.Select(item => new TaxDeductionItemDto
                {
                    Description = item.Description ?? item.AccountName,
                    Category = item.Category,
                    Amount = item.Amount,
                    ExpenseDate = periodStart, // Approximate
                    IsDeductible = IsDeductibleExpense(item.Category),
                    ReferenceId = item.ReferenceId,
                    ReferenceType = item.ReferenceType
                }).ToList();

                // Standard deduction (simplified - would need tax rules)
                deductions.StandardDeduction = 0; // Would calculate based on tax rules

                // Build deductions by category
                deductions.DeductionsByCategory = deductions.BusinessExpenseItems
                    .Concat(deductions.PersonalDeductionItems.Where(p => p.IsDeductible))
                    .GroupBy(d => d.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));

                // Calculate tax
                var taxCalculation = new TaxCalculationDto
                {
                    AdjustedGrossIncome = incomeSummary.TotalIncome - deductions.BusinessExpenses,
                    TaxableIncome = incomeSummary.TaxableIncome - deductions.TotalDeductions
                };

                // Simplified tax calculation (would need actual tax brackets)
                taxCalculation.EffectiveTaxRate = CalculateEffectiveTaxRate(taxCalculation.TaxableIncome);
                taxCalculation.MarginalTaxRate = CalculateMarginalTaxRate(taxCalculation.TaxableIncome);
                taxCalculation.EstimatedTaxLiability = taxCalculation.TaxableIncome * (taxCalculation.EffectiveTaxRate / 100);

                // Build tax categories
                var taxCategories = new List<TaxCategoryDto>();

                // Income categories
                taxCategories.Add(new TaxCategoryDto
                {
                    CategoryName = "Total Income",
                    CategoryType = "INCOME",
                    Amount = incomeSummary.TotalIncome,
                    Percentage = 100,
                    Description = "All income sources for the tax year",
                    Items = incomeSummary.IncomeItems.Select(i => new TaxCategoryItemDto
                    {
                        ItemName = i.SourceName,
                        Amount = i.Amount,
                        Date = i.IncomeDate,
                        ReferenceId = i.ReferenceId
                    }).ToList()
                });

                // Deduction categories
                foreach (var category in deductions.DeductionsByCategory)
                {
                    taxCategories.Add(new TaxCategoryDto
                    {
                        CategoryName = category.Key,
                        CategoryType = "DEDUCTION",
                        Amount = category.Value,
                        Percentage = incomeSummary.TotalIncome > 0
                            ? (category.Value / incomeSummary.TotalIncome) * 100
                            : 0,
                        Description = $"Deductible expenses in {category.Key}",
                        Items = deductions.BusinessExpenseItems
                            .Concat(deductions.PersonalDeductionItems)
                            .Where(d => d.Category == category.Key && d.IsDeductible)
                            .Select(d => new TaxCategoryItemDto
                            {
                                ItemName = d.Description,
                                Amount = d.Amount,
                                Date = d.ExpenseDate,
                                ReferenceId = d.ReferenceId
                            }).ToList()
                    });
                }

                // Quarterly breakdown
                var quarterlyBreakdown = new List<TaxQuarterlySummaryDto>();
                for (int quarter = 1; quarter <= 4; quarter++)
                {
                    var quarterStart = new DateTime(taxYear, (quarter - 1) * 3 + 1, 1);
                    var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);

                    var quarterIncomeResult = await GetIncomeStatementAsync(userId, quarterStart, quarterEnd, "QUARTERLY");
                    if (quarterIncomeResult.Success)
                    {
                        var quarterIncome = quarterIncomeResult.Data!;
                        quarterlyBreakdown.Add(new TaxQuarterlySummaryDto
                        {
                            Quarter = quarter,
                            QuarterStart = quarterStart,
                            QuarterEnd = quarterEnd,
                            TotalIncome = quarterIncome.Revenue.TotalRevenue,
                            TotalDeductions = quarterIncome.Expenses.TotalExpenses, // Simplified
                            TaxableIncome = quarterIncome.NetIncome,
                            EstimatedTaxLiability = quarterIncome.NetIncome * (taxCalculation.EffectiveTaxRate / 100)
                        });
                    }
                }

                var taxReport = new TaxReportDto
                {
                    TaxYear = taxYear,
                    ReportDate = DateTime.UtcNow,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    IncomeSummary = incomeSummary,
                    Deductions = deductions,
                    TaxCalculation = taxCalculation,
                    TaxCategories = taxCategories,
                    QuarterlyBreakdown = quarterlyBreakdown
                };

                return ApiResponse<TaxReportDto>.SuccessResult(taxReport);
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxReportDto>.ErrorResult($"Error generating tax report: {ex.Message}");
            }
        }

        // ==========================================
        // HELPER METHODS FOR RATIOS INTERPRETATION
        // ==========================================

        private string InterpretCurrentRatio(decimal ratio)
        {
            if (ratio >= 2.0m) return "Excellent";
            if (ratio >= 1.5m) return "Good";
            if (ratio >= 1.0m) return "Fair";
            return "Poor";
        }

        private string InterpretQuickRatio(decimal ratio)
        {
            if (ratio >= 1.5m) return "Excellent";
            if (ratio >= 1.0m) return "Good";
            if (ratio >= 0.5m) return "Fair";
            return "Poor";
        }

        private string InterpretCashRatio(decimal ratio)
        {
            if (ratio >= 1.0m) return "Excellent";
            if (ratio >= 0.5m) return "Good";
            if (ratio >= 0.2m) return "Fair";
            return "Poor";
        }

        private string InterpretDebtToEquity(decimal ratio)
        {
            if (ratio <= 0.5m) return "Excellent";
            if (ratio <= 1.0m) return "Good";
            if (ratio <= 2.0m) return "Fair";
            return "Poor";
        }

        private string InterpretDebtToAssets(decimal ratio)
        {
            if (ratio <= 0.3m) return "Excellent";
            if (ratio <= 0.5m) return "Good";
            if (ratio <= 0.7m) return "Fair";
            return "Poor";
        }

        private string InterpretDebtServiceCoverage(decimal ratio)
        {
            if (ratio >= 2.0m) return "Excellent";
            if (ratio >= 1.5m) return "Good";
            if (ratio >= 1.0m) return "Fair";
            return "Poor";
        }

        private string InterpretNetProfitMargin(decimal percentage)
        {
            if (percentage >= 20m) return "Excellent";
            if (percentage >= 10m) return "Good";
            if (percentage >= 5m) return "Fair";
            return "Poor";
        }

        private string InterpretReturnOnAssets(decimal percentage)
        {
            if (percentage >= 15m) return "Excellent";
            if (percentage >= 10m) return "Good";
            if (percentage >= 5m) return "Fair";
            return "Poor";
        }

        private string InterpretReturnOnEquity(decimal percentage)
        {
            if (percentage >= 20m) return "Excellent";
            if (percentage >= 15m) return "Good";
            if (percentage >= 10m) return "Fair";
            return "Poor";
        }

        private string InterpretAssetTurnover(decimal ratio)
        {
            if (ratio >= 2.0m) return "Excellent";
            if (ratio >= 1.0m) return "Good";
            if (ratio >= 0.5m) return "Fair";
            return "Poor";
        }

        private string InterpretExpenseRatio(decimal percentage)
        {
            if (percentage <= 50m) return "Excellent";
            if (percentage <= 70m) return "Good";
            if (percentage <= 85m) return "Fair";
            return "Poor";
        }

        private string InterpretSavingsRate(decimal percentage)
        {
            if (percentage >= 30m) return "Excellent";
            if (percentage >= 20m) return "Good";
            if (percentage >= 10m) return "Fair";
            return "Poor";
        }

        private List<RatioInsightDto> GenerateRatioInsights(
            LiquidityRatiosDto liquidity,
            DebtRatiosDto debt,
            ProfitabilityRatiosDto profitability,
            EfficiencyRatiosDto efficiency)
        {
            var insights = new List<RatioInsightDto>();

            // Liquidity insights
            if (liquidity.CurrentRatio < 1.0m)
            {
                insights.Add(new RatioInsightDto
                {
                    RatioName = "Current Ratio",
                    RatioValue = liquidity.CurrentRatio,
                    Category = "LIQUIDITY",
                    Interpretation = "Poor",
                    Recommendation = "Increase your current assets or reduce current liabilities to improve liquidity.",
                    Severity = "WARNING"
                });
            }

            // Debt insights
            if (debt.DebtToEquityRatio > 2.0m)
            {
                insights.Add(new RatioInsightDto
                {
                    RatioName = "Debt-to-Equity Ratio",
                    RatioValue = debt.DebtToEquityRatio,
                    Category = "DEBT",
                    Interpretation = "Poor",
                    Recommendation = "Consider paying down debt to improve your debt-to-equity ratio.",
                    Severity = "WARNING"
                });
            }

            // Profitability insights
            if (profitability.NetProfitMargin < 5m)
            {
                insights.Add(new RatioInsightDto
                {
                    RatioName = "Net Profit Margin",
                    RatioValue = profitability.NetProfitMargin,
                    Category = "PROFITABILITY",
                    Interpretation = "Poor",
                    Recommendation = "Focus on increasing income or reducing expenses to improve profitability.",
                    Severity = "INFO"
                });
            }

            // Efficiency insights
            if (efficiency.SavingsRate < 10m)
            {
                insights.Add(new RatioInsightDto
                {
                    RatioName = "Savings Rate",
                    RatioValue = efficiency.SavingsRate,
                    Category = "EFFICIENCY",
                    Interpretation = "Poor",
                    Recommendation = "Aim to save at least 20% of your income for better financial health.",
                    Severity = "INFO"
                });
            }

            return insights;
        }

        // ==========================================
        // HELPER METHODS FOR TAX REPORTING
        // ==========================================

        private string MapIncomeType(string category)
        {
            var categoryLower = category.ToLower();
            if (categoryLower.Contains("salary") || categoryLower.Contains("wage"))
                return "SALARY";
            if (categoryLower.Contains("business"))
                return "BUSINESS";
            if (categoryLower.Contains("freelance") || categoryLower.Contains("contract"))
                return "FREELANCE";
            if (categoryLower.Contains("investment") || categoryLower.Contains("dividend"))
                return "INVESTMENT";
            if (categoryLower.Contains("interest"))
                return "INTEREST";
            if (categoryLower.Contains("rental"))
                return "RENTAL";
            return "OTHER";
        }

        private bool IsBusinessExpense(string category)
        {
            var categoryLower = category.ToLower();
            return categoryLower.Contains("business") ||
                   categoryLower.Contains("office") ||
                   categoryLower.Contains("professional");
        }

        private bool IsDeductibleExpense(string category)
        {
            var categoryLower = category.ToLower();
            return categoryLower.Contains("interest") ||
                   categoryLower.Contains("charitable") ||
                   categoryLower.Contains("medical") ||
                   categoryLower.Contains("education");
        }

        private decimal CalculateEffectiveTaxRate(decimal taxableIncome)
        {
            // Simplified progressive tax calculation (Philippines example)
            if (taxableIncome <= 250000) return 0;
            if (taxableIncome <= 400000) return 20;
            if (taxableIncome <= 800000) return 25;
            if (taxableIncome <= 2000000) return 30;
            if (taxableIncome <= 8000000) return 32;
            return 35;
        }

        private decimal CalculateMarginalTaxRate(decimal taxableIncome)
        {
            // Simplified - returns the highest bracket rate
            if (taxableIncome <= 250000) return 0;
            if (taxableIncome <= 400000) return 20;
            if (taxableIncome <= 800000) return 25;
            if (taxableIncome <= 2000000) return 30;
            if (taxableIncome <= 8000000) return 32;
            return 35;
        }
    }
}
