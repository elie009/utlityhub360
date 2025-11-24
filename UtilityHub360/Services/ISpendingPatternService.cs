using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for recognizing spending patterns and generating insights
    /// </summary>
    public interface ISpendingPatternService
    {
        /// <summary>
        /// Analyze spending patterns for a user
        /// </summary>
        Task<SpendingPatternAnalysis> AnalyzeSpendingPatternsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get spending trends by category
        /// </summary>
        Task<List<CategorySpendingTrend>> GetCategorySpendingTrendsAsync(string userId, int months = 6);

        /// <summary>
        /// Detect unusual spending patterns
        /// </summary>
        Task<List<UnusualSpendingAlert>> DetectUnusualSpendingAsync(string userId);

        /// <summary>
        /// Get spending predictions for next period
        /// </summary>
        Task<SpendingPrediction> PredictSpendingAsync(string userId, int monthsAhead = 1);
    }

    /// <summary>
    /// Spending pattern analysis result
    /// </summary>
    public class SpendingPatternAnalysis
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime AnalysisPeriodStart { get; set; }
        public DateTime AnalysisPeriodEnd { get; set; }
        public decimal TotalSpending { get; set; }
        public decimal AverageDailySpending { get; set; }
        public decimal AverageMonthlySpending { get; set; }
        public List<CategorySpending> TopCategories { get; set; } = new();
        public List<MerchantSpending> TopMerchants { get; set; } = new();
        public SpendingTrend Trend { get; set; } = SpendingTrend.Stable;
        public List<string> Insights { get; set; } = new();
    }

    /// <summary>
    /// Category spending information
    /// </summary>
    public class CategorySpending
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public double PercentageOfTotal { get; set; }
        public decimal AverageAmount { get; set; }
    }

    /// <summary>
    /// Merchant spending information
    /// </summary>
    public class MerchantSpending
    {
        public string MerchantName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public DateTime LastTransactionDate { get; set; }
    }

    /// <summary>
    /// Spending trend
    /// </summary>
    public enum SpendingTrend
    {
        Increasing,
        Decreasing,
        Stable,
        Volatile
    }

    /// <summary>
    /// Category spending trend over time
    /// </summary>
    public class CategorySpendingTrend
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<MonthlySpending> MonthlyData { get; set; } = new();
        public SpendingTrend Trend { get; set; } = SpendingTrend.Stable;
    }

    /// <summary>
    /// Monthly spending data
    /// </summary>
    public class MonthlySpending
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Unusual spending alert
    /// </summary>
    public class UnusualSpendingAlert
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public double DeviationPercentage { get; set; }
        public string AlertType { get; set; } = string.Empty; // "SPIKE", "DROP", "UNUSUAL"
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Spending prediction
    /// </summary>
    public class SpendingPrediction
    {
        public DateTime PredictionDate { get; set; }
        public decimal PredictedAmount { get; set; }
        public decimal ConfidenceLowerBound { get; set; }
        public decimal ConfidenceUpperBound { get; set; }
        public List<CategoryPrediction> CategoryPredictions { get; set; } = new();
    }

    /// <summary>
    /// Category spending prediction
    /// </summary>
    public class CategoryPrediction
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal PredictedAmount { get; set; }
        public double Confidence { get; set; }
    }
}

