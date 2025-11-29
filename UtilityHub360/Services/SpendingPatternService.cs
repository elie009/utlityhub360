using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for analyzing spending patterns and generating insights
    /// </summary>
    public class SpendingPatternService : ISpendingPatternService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SpendingPatternService> _logger;

        public SpendingPatternService(ApplicationDbContext context, ILogger<SpendingPatternService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SpendingPatternAnalysis> AnalyzeSpendingPatternsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var analysis = new SpendingPatternAnalysis
            {
                UserId = userId,
                AnalysisPeriodStart = startDate ?? DateTime.UtcNow.AddMonths(-3),
                AnalysisPeriodEnd = endDate ?? DateTime.UtcNow
            };

            try
            {
                var transactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId
                        && t.TransactionType == "DEBIT"
                        && t.TransactionDate >= analysis.AnalysisPeriodStart
                        && t.TransactionDate <= analysis.AnalysisPeriodEnd
                        && !t.IsDeleted)
                    .ToListAsync();

                analysis.TotalSpending = transactions.Sum(t => t.Amount);
                
                var daysInPeriod = (analysis.AnalysisPeriodEnd - analysis.AnalysisPeriodStart).TotalDays;
                analysis.AverageDailySpending = daysInPeriod > 0 
                    ? analysis.TotalSpending / (decimal)daysInPeriod 
                    : 0;
                
                var monthsInPeriod = daysInPeriod / 30.0;
                analysis.AverageMonthlySpending = monthsInPeriod > 0 
                    ? analysis.TotalSpending / (decimal)monthsInPeriod 
                    : 0;

                // Top categories
                analysis.TopCategories = transactions
                    .Where(t => !string.IsNullOrEmpty(t.Category))
                    .GroupBy(t => t.Category!)
                    .Select(g => new CategorySpending
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(t => t.Amount),
                        TransactionCount = g.Count(),
                        PercentageOfTotal = analysis.TotalSpending > 0 
                            ? (double)(g.Sum(t => t.Amount) / analysis.TotalSpending * 100) 
                            : 0,
                        AverageAmount = g.Average(t => t.Amount)
                    })
                    .OrderByDescending(c => c.TotalAmount)
                    .Take(10)
                    .ToList();

                // Top merchants
                analysis.TopMerchants = transactions
                    .Where(t => !string.IsNullOrEmpty(t.Merchant))
                    .GroupBy(t => t.Merchant!)
                    .Select(g => new MerchantSpending
                    {
                        MerchantName = g.Key,
                        TotalAmount = g.Sum(t => t.Amount),
                        TransactionCount = g.Count(),
                        LastTransactionDate = g.Max(t => t.TransactionDate)
                    })
                    .OrderByDescending(m => m.TotalAmount)
                    .Take(10)
                    .ToList();

                // Determine trend
                analysis.Trend = CalculateTrend(transactions);

                // Generate insights
                analysis.Insights = GenerateInsights(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing spending patterns for user {UserId}", userId);
            }

            return analysis;
        }

        public async Task<List<CategorySpendingTrend>> GetCategorySpendingTrendsAsync(string userId, int months = 6)
        {
            var trends = new List<CategorySpendingTrend>();
            var startDate = DateTime.UtcNow.AddMonths(-months);

            try
            {
                var transactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId
                        && t.TransactionType == "DEBIT"
                        && t.TransactionDate >= startDate
                        && !string.IsNullOrEmpty(t.Category)
                        && !t.IsDeleted)
                    .ToListAsync();

                var categories = transactions
                    .Select(t => t.Category!)
                    .Distinct()
                    .ToList();

                foreach (var category in categories)
                {
                    var categoryTransactions = transactions
                        .Where(t => t.Category == category)
                        .ToList();

                    var monthlyData = categoryTransactions
                        .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                        .Select(g => new MonthlySpending
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Amount = g.Sum(t => t.Amount),
                            TransactionCount = g.Count()
                        })
                        .OrderBy(m => m.Year)
                        .ThenBy(m => m.Month)
                        .ToList();

                    var trend = new CategorySpendingTrend
                    {
                        CategoryName = category,
                        MonthlyData = monthlyData,
                        Trend = CalculateCategoryTrend(monthlyData)
                    };

                    trends.Add(trend);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category spending trends for user {UserId}", userId);
            }

            return trends.OrderByDescending(t => t.MonthlyData.Sum(m => m.Amount)).ToList();
        }

        public async Task<List<UnusualSpendingAlert>> DetectUnusualSpendingAsync(string userId)
        {
            var alerts = new List<UnusualSpendingAlert>();

            try
            {
                var currentMonth = DateTime.UtcNow;
                var currentMonthStart = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);
                var lastMonthEnd = currentMonthStart.AddDays(-1);

                // Get current month spending by category
                var currentMonthTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId
                        && t.TransactionType == "DEBIT"
                        && t.TransactionDate >= currentMonthStart
                        && !string.IsNullOrEmpty(t.Category)
                        && !t.IsDeleted)
                    .ToListAsync();

                // Get historical average (last 3 months)
                var historicalStart = currentMonthStart.AddMonths(-3);
                var historicalTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId
                        && t.TransactionType == "DEBIT"
                        && t.TransactionDate >= historicalStart
                        && t.TransactionDate < currentMonthStart
                        && !string.IsNullOrEmpty(t.Category)
                        && !t.IsDeleted)
                    .ToListAsync();

                var categoryGroups = currentMonthTransactions
                    .GroupBy(t => t.Category!)
                    .ToList();

                foreach (var group in categoryGroups)
                {
                    var categoryName = group.Key;
                    var currentAmount = group.Sum(t => t.Amount);

                    var historicalAmounts = historicalTransactions
                        .Where(t => t.Category == categoryName)
                        .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                        .Select(g => g.Sum(t => t.Amount))
                        .ToList();

                    if (historicalAmounts.Any())
                    {
                        var averageAmount = historicalAmounts.Average();
                        var deviation = (double)((currentAmount - averageAmount) / averageAmount * 100);

                        if (Math.Abs(deviation) > 50) // More than 50% deviation
                        {
                            alerts.Add(new UnusualSpendingAlert
                            {
                                CategoryName = categoryName,
                                CurrentAmount = currentAmount,
                                AverageAmount = averageAmount,
                                DeviationPercentage = deviation,
                                AlertType = deviation > 0 ? "SPIKE" : "DROP",
                                Message = deviation > 0
                                    ? $"Spending in {categoryName} is {deviation:F1}% higher than average"
                                    : $"Spending in {categoryName} is {Math.Abs(deviation):F1}% lower than average"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting unusual spending for user {UserId}", userId);
            }

            return alerts.OrderByDescending(a => Math.Abs(a.DeviationPercentage)).ToList();
        }

        public async Task<SpendingPrediction> PredictSpendingAsync(string userId, int monthsAhead = 1)
        {
            var prediction = new SpendingPrediction
            {
                PredictionDate = DateTime.UtcNow.AddMonths(monthsAhead)
            };

            try
            {
                var historicalStart = DateTime.UtcNow.AddMonths(-6);
                var historicalTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId
                        && t.TransactionType == "DEBIT"
                        && t.TransactionDate >= historicalStart
                        && !t.IsDeleted)
                    .ToListAsync();

                if (historicalTransactions.Any())
                {
                    var monthlyTotals = historicalTransactions
                        .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                        .Select(g => g.Sum(t => t.Amount))
                        .ToList();

                    if (monthlyTotals.Count >= 2)
                    {
                        var average = monthlyTotals.Average();
                        var trend = CalculateLinearTrend(monthlyTotals);
                        
                        prediction.PredictedAmount = average + (trend * monthsAhead);
                        prediction.ConfidenceLowerBound = prediction.PredictedAmount * 0.8m;
                        prediction.ConfidenceUpperBound = prediction.PredictedAmount * 1.2m;

                        // Category predictions
                        var categoryGroups = historicalTransactions
                            .Where(t => !string.IsNullOrEmpty(t.Category))
                            .GroupBy(t => t.Category!)
                            .ToList();

                        foreach (var group in categoryGroups)
                        {
                            var categoryMonthly = historicalTransactions
                                .Where(t => t.Category == group.Key)
                                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                                .Select(g => g.Sum(t => t.Amount))
                                .ToList();

                            if (categoryMonthly.Count >= 2)
                            {
                                var categoryAvg = categoryMonthly.Average();
                                var categoryTrend = CalculateLinearTrend(categoryMonthly);
                                
                                prediction.CategoryPredictions.Add(new CategoryPrediction
                                {
                                    CategoryName = group.Key,
                                    PredictedAmount = categoryAvg + (categoryTrend * monthsAhead),
                                    Confidence = Math.Min(0.9, 0.5 + (categoryMonthly.Count * 0.1))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting spending for user {UserId}", userId);
            }

            return prediction;
        }

        private SpendingTrend CalculateTrend(List<Entities.BankTransaction> transactions)
        {
            if (transactions.Count < 2)
                return SpendingTrend.Stable;

            var monthlyTotals = transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => g.Sum(t => t.Amount))
                .OrderBy(a => a)
                .ToList();

            if (monthlyTotals.Count < 2)
                return SpendingTrend.Stable;

            var firstHalf = monthlyTotals.Take(monthlyTotals.Count / 2).Average();
            var secondHalf = monthlyTotals.Skip(monthlyTotals.Count / 2).Average();

            var change = (double)((secondHalf - firstHalf) / firstHalf * 100);

            return change switch
            {
                > 10 => SpendingTrend.Increasing,
                < -10 => SpendingTrend.Decreasing,
                _ => SpendingTrend.Stable
            };
        }

        private SpendingTrend CalculateCategoryTrend(List<MonthlySpending> monthlyData)
        {
            if (monthlyData.Count < 2)
                return SpendingTrend.Stable;

            var amounts = monthlyData.Select(m => m.Amount).ToList();
            var firstHalf = amounts.Take(amounts.Count / 2).Average();
            var secondHalf = amounts.Skip(amounts.Count / 2).Average();

            var change = (double)((secondHalf - firstHalf) / firstHalf * 100);

            return change switch
            {
                > 10 => SpendingTrend.Increasing,
                < -10 => SpendingTrend.Decreasing,
                _ => SpendingTrend.Stable
            };
        }

        private decimal CalculateLinearTrend(List<decimal> values)
        {
            if (values.Count < 2)
                return 0;

            var n = values.Count;
            var sumX = n * (n + 1) / 2;
            var sumY = values.Sum();
            var sumXY = values.Select((v, i) => (i + 1) * v).Sum();
            var sumX2 = n * (n + 1) * (2 * n + 1) / 6;

            var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            return slope;
        }

        private List<string> GenerateInsights(SpendingPatternAnalysis analysis)
        {
            var insights = new List<string>();

            if (analysis.TopCategories.Any())
            {
                var topCategory = analysis.TopCategories.First();
                insights.Add($"Your top spending category is {topCategory.CategoryName} ({topCategory.PercentageOfTotal:F1}% of total spending)");
            }

            if (analysis.Trend == SpendingTrend.Increasing)
            {
                insights.Add("Your spending has been increasing over the analyzed period");
            }
            else if (analysis.Trend == SpendingTrend.Decreasing)
            {
                insights.Add("Your spending has been decreasing over the analyzed period");
            }

            if (analysis.AverageDailySpending > 0)
            {
                insights.Add($"You spend an average of ${analysis.AverageDailySpending:F2} per day");
            }

            return insights;
        }
    }
}

