using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UtilityHub360.Data;
using UtilityHub360.DTOs;

namespace UtilityHub360.Services
{
    public class DisposableAmountService : IDisposableAmountService
    {
        private readonly ApplicationDbContext _context;

        public DisposableAmountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DisposableAmountDto> GetCurrentMonthDisposableAmountAsync(string userId, decimal? targetSavings = null, decimal? investmentAllocation = null)
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            return await GetDisposableAmountAsync(userId, startDate, endDate, targetSavings, investmentAllocation);
        }

        public async Task<DisposableAmountDto> GetMonthlyDisposableAmountAsync(string userId, int year, int month, decimal? targetSavings = null, decimal? investmentAllocation = null)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            return await GetDisposableAmountAsync(userId, startDate, endDate, targetSavings, investmentAllocation);
        }

        public async Task<DisposableAmountDto> GetDisposableAmountAsync(
            string userId, 
            DateTime startDate, 
            DateTime endDate, 
            decimal? targetSavings = null, 
            decimal? investmentAllocation = null)
        {
            var dto = new DisposableAmountDto
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Period = DeterminePeriod(startDate, endDate),
                TargetSavings = targetSavings,
                InvestmentAllocation = investmentAllocation
            };

            // Calculate Total Income
            var incomes = await _context.IncomeSources
                .Where(i => i.UserId == userId && i.IsActive)
                .ToListAsync();

            dto.TotalIncome = CalculatePeriodIncome(incomes, startDate, endDate);
            dto.IncomeBreakdown = incomes.Select(i => new IncomeBreakdown
            {
                SourceName = i.Name,
                Category = i.Category,
                Amount = i.Amount,
                MonthlyAmount = i.MonthlyAmount,
                Frequency = i.Frequency
            }).ToList();

            // Calculate Fixed Expenses - Bills
            var bills = await _context.Bills
                .Where(b => b.UserId == userId && 
                           b.DueDate >= startDate && 
                           b.DueDate <= endDate)
                .ToListAsync();

            dto.TotalBills = bills.Sum(b => b.Amount);
            dto.BillsBreakdown = bills.Select(b => new ExpenseDetail
            {
                Id = b.Id,
                Name = b.BillName,
                Type = b.BillType,
                Amount = b.Amount,
                Status = b.Status,
                DueDate = b.DueDate
            }).ToList();

            // Calculate Fixed Expenses - Loans
            var loans = await _context.Loans
                .Where(l => l.UserId == userId && 
                           (l.Status == "ACTIVE" || l.Status == "APPROVED"))
                .ToListAsync();

            // Calculate loan payments for the period
            var monthsInPeriod = CalculateMonthsInPeriod(startDate, endDate);
            dto.TotalLoans = loans.Sum(l => l.MonthlyPayment * monthsInPeriod);
            dto.LoansBreakdown = loans.Select(l => new ExpenseDetail
            {
                Id = l.Id,
                Name = l.Purpose,
                Type = "LOAN",
                Amount = l.MonthlyPayment * monthsInPeriod,
                Status = l.Status
            }).ToList();

            dto.TotalFixedExpenses = dto.TotalBills + dto.TotalLoans;

            // Calculate Variable Expenses
            var variableExpenses = await _context.VariableExpenses
                .Where(v => v.UserId == userId && 
                           v.ExpenseDate >= startDate && 
                           v.ExpenseDate <= endDate)
                .ToListAsync();

            dto.TotalVariableExpenses = variableExpenses.Sum(v => v.Amount);
            
            var expensesByCategory = variableExpenses
                .GroupBy(v => v.Category)
                .Select(g => new VariableExpenseBreakdown
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(v => v.Amount),
                    Count = g.Count(),
                    Percentage = dto.TotalVariableExpenses > 0 
                        ? (g.Sum(v => v.Amount) / dto.TotalVariableExpenses) * 100 
                        : 0
                })
                .OrderByDescending(v => v.TotalAmount)
                .ToList();

            dto.VariableExpensesBreakdown = expensesByCategory;

            // Calculate Disposable Amount
            dto.DisposableAmount = dto.TotalIncome - dto.TotalFixedExpenses - dto.TotalVariableExpenses;
            dto.DisposablePercentage = dto.TotalIncome > 0 
                ? (dto.DisposableAmount / dto.TotalIncome) * 100 
                : 0;

            // Calculate Net Disposable Amount if savings/investment goals provided
            if (targetSavings.HasValue || investmentAllocation.HasValue)
            {
                var totalAllocations = (targetSavings ?? 0) + (investmentAllocation ?? 0);
                dto.NetDisposableAmount = dto.DisposableAmount - totalAllocations;
            }

            // Calculate Actual Spending from Transactions
            var actualSpending = await CalculateActualSpendingAsync(userId, startDate, endDate);
            dto.ActualSpending = actualSpending;
            
            // Calculate Spending Percentages
            if (dto.DisposableAmount > 0)
            {
                if (actualSpending > dto.DisposableAmount)
                {
                    // Spending exceeded disposable amount
                    dto.IsExceeded = true;
                    dto.SpendingPercentage = 100;
                    dto.RemainingPercentage = 0;
                    dto.ExceededPercentage = ((actualSpending - dto.DisposableAmount) / dto.DisposableAmount) * 100;
                    dto.RemainingDisposableAmount = 0;
                }
                else
                {
                    // Spending is within disposable amount
                    dto.IsExceeded = false;
                    dto.SpendingPercentage = (actualSpending / dto.DisposableAmount) * 100;
                    dto.RemainingPercentage = ((dto.DisposableAmount - actualSpending) / dto.DisposableAmount) * 100;
                    dto.ExceededPercentage = null;
                    dto.RemainingDisposableAmount = dto.DisposableAmount - actualSpending;
                }
            }
            else
            {
                // No disposable amount available
                dto.IsExceeded = actualSpending > 0;
                dto.SpendingPercentage = 0;
                dto.RemainingPercentage = 0;
                dto.ExceededPercentage = actualSpending > 0 ? 100 : null;
                dto.RemainingDisposableAmount = 0;
            }

            // Get comparison data (previous period) - only if not already calculating comparison
            var previousStartDate = startDate.AddMonths(-1);
            var previousEndDate = endDate.AddMonths(-1);
            
            // Calculate previous period disposable amount without recursion
            var previousDisposableAmount = await CalculateDisposableAmountOnly(userId, previousStartDate, previousEndDate);
            
            dto.Comparison = new ComparisonData
            {
                PreviousPeriodDisposableAmount = previousDisposableAmount,
                ChangeAmount = dto.DisposableAmount - previousDisposableAmount,
                ChangePercentage = previousDisposableAmount != 0 
                    ? ((dto.DisposableAmount - previousDisposableAmount) / previousDisposableAmount) * 100 
                    : 0,
                Trend = dto.DisposableAmount > previousDisposableAmount ? "UP" 
                      : dto.DisposableAmount < previousDisposableAmount ? "DOWN" 
                      : "STABLE"
            };

            // Generate Insights
            dto.Insights = GenerateInsights(dto, dto.Comparison);

            return dto;
        }

        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(string userId)
        {
            var summary = new FinancialSummaryDto
            {
                UserId = userId,
                GeneratedAt = DateTime.UtcNow
            };

            var now = DateTime.UtcNow;
            var currentYear = now.Year;

            // Current Month
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            var currentMonthData = await GetDisposableAmountAsync(userId, currentMonthStart, currentMonthEnd);

            summary.CurrentMonth = new MonthlySnapshot
            {
                Month = now.Month,
                Year = now.Year,
                TotalIncome = currentMonthData.TotalIncome,
                TotalExpenses = currentMonthData.TotalFixedExpenses + currentMonthData.TotalVariableExpenses,
                FixedExpenses = currentMonthData.TotalFixedExpenses,
                VariableExpenses = currentMonthData.TotalVariableExpenses,
                DisposableAmount = currentMonthData.DisposableAmount,
                SavingsAmount = currentMonthData.DisposableAmount > 0 ? currentMonthData.DisposableAmount : 0,
                SavingsRate = currentMonthData.TotalIncome > 0 
                    ? (currentMonthData.DisposableAmount / currentMonthData.TotalIncome) * 100 
                    : 0
            };

            // Previous Month
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
            var previousMonthData = await GetDisposableAmountAsync(userId, previousMonthStart, previousMonthEnd);

            summary.PreviousMonth = new MonthlySnapshot
            {
                Month = previousMonthStart.Month,
                Year = previousMonthStart.Year,
                TotalIncome = previousMonthData.TotalIncome,
                TotalExpenses = previousMonthData.TotalFixedExpenses + previousMonthData.TotalVariableExpenses,
                FixedExpenses = previousMonthData.TotalFixedExpenses,
                VariableExpenses = previousMonthData.TotalVariableExpenses,
                DisposableAmount = previousMonthData.DisposableAmount,
                SavingsAmount = previousMonthData.DisposableAmount > 0 ? previousMonthData.DisposableAmount : 0,
                SavingsRate = previousMonthData.TotalIncome > 0 
                    ? (previousMonthData.DisposableAmount / previousMonthData.TotalIncome) * 100 
                    : 0
            };

            // Year-to-Date
            var yearStart = new DateTime(currentYear, 1, 1);
            var yearEnd = new DateTime(currentYear, 12, 31);

            var monthlyBreakdown = new List<MonthlyDataPoint>();
            decimal totalYearIncome = 0;
            decimal totalYearExpenses = 0;
            decimal totalYearDisposable = 0;

            for (int month = 1; month <= now.Month; month++)
            {
                var monthStart = new DateTime(currentYear, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthData = await GetDisposableAmountAsync(userId, monthStart, monthEnd);

                totalYearIncome += monthData.TotalIncome;
                totalYearExpenses += monthData.TotalFixedExpenses + monthData.TotalVariableExpenses;
                totalYearDisposable += monthData.DisposableAmount;

                monthlyBreakdown.Add(new MonthlyDataPoint
                {
                    Month = month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    Income = monthData.TotalIncome,
                    Expenses = monthData.TotalFixedExpenses + monthData.TotalVariableExpenses,
                    Disposable = monthData.DisposableAmount
                });
            }

            summary.YearToDate = new YearlySnapshot
            {
                Year = currentYear,
                TotalIncome = totalYearIncome,
                TotalExpenses = totalYearExpenses,
                TotalDisposable = totalYearDisposable,
                AverageMonthlyDisposable = now.Month > 0 ? totalYearDisposable / now.Month : 0,
                TotalSavings = totalYearDisposable > 0 ? totalYearDisposable : 0,
                MonthlyBreakdown = monthlyBreakdown
            };

            // Quick Stats
            var activeLoans = await _context.Loans
                .Where(l => l.UserId == userId && l.Status == "ACTIVE")
                .ToListAsync();

            var pendingBills = await _context.Bills
                .Where(b => b.UserId == userId && 
                           b.Status == "PENDING" && 
                           b.DueDate >= now)
                .ToListAsync();

            var topExpenseCategory = currentMonthData.VariableExpensesBreakdown
                .OrderByDescending(v => v.TotalAmount)
                .FirstOrDefault();

            summary.Stats = new QuickStats
            {
                AverageMonthlyIncome = now.Month > 0 ? totalYearIncome / now.Month : 0,
                AverageMonthlyExpenses = now.Month > 0 ? totalYearExpenses / now.Month : 0,
                AverageDisposable = now.Month > 0 ? totalYearDisposable / now.Month : 0,
                TopExpenseCategory = topExpenseCategory?.Category ?? "N/A",
                TopExpenseCategoryAmount = topExpenseCategory?.TotalAmount ?? 0,
                ActiveLoans = activeLoans.Count,
                TotalLoanBalance = activeLoans.Sum(l => l.RemainingBalance),
                PendingBills = pendingBills.Count,
                PendingBillsAmount = pendingBills.Sum(b => b.Amount)
            };

            return summary;
        }

        public async Task<SimpleFinancialSummaryDto> GetSimpleFinancialSummaryAsync(string userId, int? year = null, int? month = null)
        {
            var now = DateTime.UtcNow;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;
            
            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var summary = new SimpleFinancialSummaryDto
            {
                UserId = userId,
                Year = targetYear,
                Month = targetMonth
            };

            // Get Total Income (monthly)
            var incomeSources = await _context.IncomeSources
                .Where(i => i.UserId == userId && i.IsActive)
                .ToListAsync();

            summary.TotalIncome = incomeSources.Sum(i => i.MonthlyAmount);
            summary.IncomeSourcesCount = incomeSources.Count;

            // Get Total Bills for this month
            var bills = await _context.Bills
                .Where(b => b.UserId == userId && 
                           b.DueDate >= startDate && 
                           b.DueDate <= endDate &&
                           (b.Status == "PENDING" || b.Status == "PAID"))
                .ToListAsync();

            summary.TotalBills = bills.Sum(b => b.Amount);
            summary.BillsCount = bills.Count;

            // Get Total Loan Payments (monthly)
            var activeLoans = await _context.Loans
                .Where(l => l.UserId == userId && 
                           (l.Status == "ACTIVE" || l.Status == "APPROVED"))
                .ToListAsync();

            summary.TotalLoans = activeLoans.Sum(l => l.MonthlyPayment);
            summary.ActiveLoansCount = activeLoans.Count;

            // Total Expenses
            summary.TotalExpenses = summary.TotalBills + summary.TotalLoans;

            // Get Total Savings (monthly deposits/goals)
            var savingsAccounts = await _context.SavingsAccounts
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            // Get savings transactions for this month
            var savingsTransactions = await _context.SavingsTransactions
                .Where(st => st.SavingsAccount.UserId == userId &&
                            st.TransactionDate >= startDate &&
                            st.TransactionDate <= endDate &&
                            st.TransactionType == "DEPOSIT")
                .ToListAsync();

            summary.TotalSavings = savingsTransactions.Sum(st => st.Amount);
            summary.SavingsAccountsCount = savingsAccounts.Count;

            // Calculate Remaining Amount
            summary.RemainingAmount = summary.TotalIncome - summary.TotalExpenses - summary.TotalSavings;
            
            // Calculate percentage
            if (summary.TotalIncome > 0)
            {
                summary.RemainingPercentage = (summary.RemainingAmount / summary.TotalIncome) * 100;
            }

            // Determine financial status
            if (summary.RemainingPercentage >= 20)
            {
                summary.FinancialStatus = "HEALTHY";
            }
            else if (summary.RemainingPercentage >= 10)
            {
                summary.FinancialStatus = "WARNING";
            }
            else if (summary.RemainingAmount >= 0)
            {
                summary.FinancialStatus = "CRITICAL";
            }
            else
            {
                summary.FinancialStatus = "DEFICIT";
            }

            return summary;
        }

        // Helper Methods

        private async Task<decimal> CalculateDisposableAmountOnly(string userId, DateTime startDate, DateTime endDate)
        {
            // Calculate income
            var incomes = await _context.IncomeSources
                .Where(i => i.UserId == userId && i.IsActive)
                .ToListAsync();
            var totalIncome = CalculatePeriodIncome(incomes, startDate, endDate);

            // Calculate bills
            var bills = await _context.Bills
                .Where(b => b.UserId == userId && 
                           b.DueDate >= startDate && 
                           b.DueDate <= endDate)
                .ToListAsync();
            var totalBills = bills.Sum(b => b.Amount);

            // Calculate loans
            var loans = await _context.Loans
                .Where(l => l.UserId == userId && 
                           (l.Status == "ACTIVE" || l.Status == "APPROVED"))
                .ToListAsync();
            var monthsInPeriod = CalculateMonthsInPeriod(startDate, endDate);
            var totalLoans = loans.Sum(l => l.MonthlyPayment * monthsInPeriod);

            // Calculate variable expenses
            var variableExpenses = await _context.VariableExpenses
                .Where(v => v.UserId == userId && 
                           v.ExpenseDate >= startDate && 
                           v.ExpenseDate <= endDate)
                .ToListAsync();
            var totalVariableExpenses = variableExpenses.Sum(v => v.Amount);

            return totalIncome - totalBills - totalLoans - totalVariableExpenses;
        }

        private decimal CalculatePeriodIncome(List<Entities.IncomeSource> incomes, DateTime startDate, DateTime endDate)
        {
            // Check if this is a full calendar month (e.g., Oct 1 - Oct 31)
            var isFullCalendarMonth = IsFullCalendarMonth(startDate, endDate);
            
            if (isFullCalendarMonth)
            {
                // For a full calendar month, monthly income is fixed regardless of days in the month
                // Use MonthlyAmount directly without any multiplier
                return incomes.Sum(i => i.MonthlyAmount);
            }
            else
            {
                // For partial months or periods spanning multiple months, use proportional calculation
                var monthsInPeriod = CalculateMonthsInPeriod(startDate, endDate);
                return incomes.Sum(i => i.MonthlyAmount * monthsInPeriod);
            }
        }

        private bool IsFullCalendarMonth(DateTime startDate, DateTime endDate)
        {
            // Check if startDate is the first day of a month and endDate is the last day of the same month
            return startDate.Day == 1 && 
                   endDate == startDate.AddMonths(1).AddDays(-1);
        }

        private decimal CalculateMonthsInPeriod(DateTime startDate, DateTime endDate)
        {
            var days = (endDate - startDate).Days + 1;
            return days / 30.0m; // Approximate months
        }

        private async Task<decimal> CalculateActualSpendingAsync(string userId, DateTime startDate, DateTime endDate)
        {
            // Get all DEBIT transactions (actual spending) from Payments table for the period
            var debitTransactions = await _context.Payments
                .Where(p => p.UserId == userId && 
                           p.IsBankTransaction &&
                           p.TransactionType == "DEBIT" &&
                           p.TransactionDate >= startDate && 
                           p.TransactionDate <= endDate)
                .SumAsync(p => p.Amount);

            return debitTransactions;
        }

        private string DeterminePeriod(DateTime startDate, DateTime endDate)
        {
            var days = (endDate - startDate).Days + 1;
            
            if (days <= 31 && startDate.Month == endDate.Month)
                return "MONTHLY";
            else if (days >= 365)
                return "YEARLY";
            else
                return "CUSTOM";
        }

        private List<string> GenerateInsights(DisposableAmountDto current, ComparisonData? comparison)
        {
            var insights = new List<string>();

            // Disposable amount change insight
            if (comparison != null && comparison.ChangePercentage != 0)
            {
                var changeDirection = comparison.ChangePercentage > 0 ? "increased" : "decreased";
                var changePercent = Math.Abs(comparison.ChangePercentage ?? 0);
                insights.Add($"Your disposable income {changeDirection} by {changePercent:F1}% compared to the previous period.");
            }

            // High variable expenses insight
            if (current.TotalIncome > 0)
            {
                var variableExpensePercentage = (current.TotalVariableExpenses / current.TotalIncome) * 100;
                if (variableExpensePercentage > 40)
                {
                    insights.Add($"Your variable expenses are {variableExpensePercentage:F1}% of your income. Consider reviewing discretionary spending.");
                }
            }

            // Savings potential insight
            if (current.TotalVariableExpenses > 0)
            {
                var potentialSavings = current.TotalVariableExpenses * 0.15m; // 15% reduction
                var savingsIncrease = current.DisposableAmount > 0 
                    ? (potentialSavings / current.DisposableAmount) * 100 
                    : 0;
                
                if (savingsIncrease > 0)
                {
                    insights.Add($"Reducing your variable expenses by 15% (${potentialSavings:N2}) can increase your savings by {savingsIncrease:F1}%.");
                }
            }

            // Top spending category insight
            var topCategory = current.VariableExpensesBreakdown.OrderByDescending(v => v.TotalAmount).FirstOrDefault();
            if (topCategory != null && current.TotalVariableExpenses > 0)
            {
                insights.Add($"Your highest spending category is {topCategory.Category} at ${topCategory.TotalAmount:N2} ({topCategory.Percentage:F1}% of variable expenses).");
            }

            // Savings goal recommendation
            if (current.DisposableAmount > 0)
            {
                var recommendedSavings = current.DisposableAmount * 0.20m; // 20% of disposable
                insights.Add($"Consider saving at least ${recommendedSavings:N2} per month (20% of your disposable income) to build your financial cushion.");
            }

            // Loan payment insight
            if (current.TotalLoans > 0 && current.TotalIncome > 0)
            {
                var loanPercentage = (current.TotalLoans / current.TotalIncome) * 100;
                if (loanPercentage > 30)
                {
                    insights.Add($"Your loan payments represent {loanPercentage:F1}% of your income. This is quite high - consider strategies to reduce debt.");
                }
            }

            // Spending status insight
            if (current.DisposableAmount > 0)
            {
                if (current.IsExceeded && current.ExceededPercentage.HasValue)
                {
                    insights.Add($"⚠️ You've exceeded your disposable amount by {current.ExceededPercentage.Value:F1}%. Current spending: ${current.ActualSpending:N2} vs Available: ${current.DisposableAmount:N2}.");
                }
                else if (current.RemainingPercentage > 0)
                {
                    insights.Add($"✅ You have {current.RemainingPercentage:F1}% of your disposable amount remaining ({current.RemainingDisposableAmount:C} out of {current.DisposableAmount:C}).");
                }
            }

            return insights;
        }

        public async Task<RecentActivityDto> GetRecentActivityAsync(string userId)
        {
            var activity = new RecentActivityDto
            {
                UserId = userId,
                GeneratedAt = DateTime.UtcNow
            };

            // Get income sources count and total monthly income
            var incomeSources = await _context.IncomeSources
                .Where(i => i.UserId == userId && i.IsActive)
                .ToListAsync();

            activity.IncomeSourcesCount = incomeSources.Count;
            activity.TotalMonthlyIncome = incomeSources.Sum(i => i.MonthlyAmount);

            // Get user profile for monthly goals
            var userProfile = await _context.UserProfiles
                .Where(p => p.UserId == userId && p.IsActive)
                .FirstOrDefaultAsync();

            if (userProfile != null)
            {
                activity.HasProfile = true;
                activity.ProfileStatus = $"Profile completed with {activity.IncomeSourcesCount} income source{(activity.IncomeSourcesCount != 1 ? "s" : "")}";
                activity.TotalMonthlyGoals = (userProfile.MonthlySavingsGoal ?? 0) +
                                           (userProfile.MonthlyInvestmentGoal ?? 0) +
                                           (userProfile.MonthlyEmergencyFundGoal ?? 0);
            }
            else
            {
                activity.HasProfile = false;
                activity.ProfileStatus = "Profile setup required";
                activity.TotalMonthlyGoals = 0;
            }

            // Get disposable amount for current month
            try
            {
                var disposableAmountDto = await GetCurrentMonthDisposableAmountAsync(userId);
                activity.DisposableAmount = disposableAmountDto.DisposableAmount;
            }
            catch
            {
                // If calculation fails, use 0
                activity.DisposableAmount = 0;
            }

            return activity;
        }
    }
}

