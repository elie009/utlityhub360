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
        // SAVINGS REPORT (STUB)
        // ==========================================

        public async Task<ApiResponse<SavingsReportDto>> GetSavingsReportAsync(string userId, ReportQueryDto query)
        {
            return await Task.FromResult(ApiResponse<SavingsReportDto>.ErrorResult("Not implemented yet"));
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
            var incomeSources = await _context.IncomeSources
                .Where(i => i.UserId == userId && !i.IsDeleted && i.IsActive)
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
    }
}
