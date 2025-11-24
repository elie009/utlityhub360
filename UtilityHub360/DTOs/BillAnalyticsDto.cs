using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // ============================================
    // Variable Monthly Billing - DTOs
    // ============================================

    /// <summary>
    /// DTO for bill history with analytics data
    /// </summary>
    public class BillHistoryWithAnalyticsDto
    {
        public List<BillDto> Bills { get; set; } = new();
        public BillAnalyticsCalculationsDto Analytics { get; set; } = new();
        public BillForecastDto Forecast { get; set; } = new();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// DTO for analytics calculations (averages, totals, trends)
    /// </summary>
    public class BillAnalyticsCalculationsDto
    {
        public decimal AverageSimple { get; set; }
        public decimal AverageWeighted { get; set; }
        public decimal AverageSeasonal { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal HighestBill { get; set; }
        public decimal LowestBill { get; set; }
        public string Trend { get; set; } = string.Empty; // "increasing", "decreasing", "stable"
        public int BillCount { get; set; }
        public DateTime? FirstBillDate { get; set; }
        public DateTime? LastBillDate { get; set; }
    }

    /// <summary>
    /// DTO for bill forecast information
    /// </summary>
    public class BillForecastDto
    {
        public decimal EstimatedAmount { get; set; }
        public string CalculationMethod { get; set; } = string.Empty; // "simple", "weighted", "seasonal"
        public string Confidence { get; set; } = string.Empty; // "high", "medium", "low"
        public DateTime EstimatedForMonth { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for variance analysis between actual and estimated bills
    /// </summary>
    public class BillVarianceDto
    {
        public string BillId { get; set; } = string.Empty;
        public decimal ActualAmount { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Status { get; set; } = string.Empty; // "over_budget", "slightly_over", "on_target", "under_budget"
        public string Message { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for budget settings
    /// </summary>
    public class BudgetSettingDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string BillType { get; set; } = string.Empty;
        public decimal MonthlyBudget { get; set; }
        public bool EnableAlerts { get; set; }
        public int AlertThreshold { get; set; } // Percentage (e.g., 90 = alert at 90% of budget)
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating or updating budget settings
    /// </summary>
    public class CreateBudgetSettingDto
    {
        [Required]
        [StringLength(100)]
        public string Provider { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BillType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly budget must be greater than 0")]
        public decimal MonthlyBudget { get; set; }

        public bool EnableAlerts { get; set; } = true;

        [Range(1, 100, ErrorMessage = "Alert threshold must be between 1 and 100")]
        public int AlertThreshold { get; set; } = 90;
    }

    /// <summary>
    /// DTO for budget status
    /// </summary>
    public class BudgetStatusDto
    {
        public string BudgetId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string BillType { get; set; } = string.Empty;
        public decimal MonthlyBudget { get; set; }
        public decimal CurrentBill { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentageUsed { get; set; }
        public string Status { get; set; } = string.Empty; // "on_track", "approaching_limit", "over_budget"
        public bool Alert { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for bill alerts
    /// </summary>
    public class BillAlertDto
    {
        public string Id { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "budget_exceeded", "trend_increase", "unusual_spike", "due_date", "overdue", "savings"
        public string Severity { get; set; } = string.Empty; // "info", "warning", "error", "success"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? BillId { get; set; }
        public string? Provider { get; set; }
        public decimal? Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? ActionLink { get; set; }
    }

    /// <summary>
    /// DTO for monthly bill summary (for trend graphs)
    /// </summary>
    public class MonthlyBillSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int BillCount { get; set; }
        public decimal AverageAmount { get; set; }
        public string Status { get; set; } = string.Empty; // "paid", "pending", "overdue"
    }

    /// <summary>
    /// DTO for provider-specific analytics
    /// </summary>
    public class ProviderAnalyticsDto
    {
        public string Provider { get; set; } = string.Empty;
        public string BillType { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public decimal AverageMonthly { get; set; }
        public int BillCount { get; set; }
        public decimal HighestBill { get; set; }
        public decimal LowestBill { get; set; }
        public DateTime? LastBillDate { get; set; }
        public decimal? CurrentBudget { get; set; }
        public List<MonthlyBillSummaryDto> MonthlySummary { get; set; } = new();
    }

    /// <summary>
    /// DTO for requesting bill analytics
    /// </summary>
    public class BillAnalyticsRequestDto
    {
        public string? Provider { get; set; }
        public string? BillType { get; set; }
        public int Months { get; set; } = 6; // Default to last 6 months
        public string CalculationMethod { get; set; } = "weighted"; // "simple", "weighted", "seasonal"
    }

    /// <summary>
    /// DTO for comprehensive bill dashboard data
    /// </summary>
    public class BillDashboardDto
    {
        public List<BillDto> CurrentBills { get; set; } = new();
        public List<BillDto> UpcomingBills { get; set; } = new();
        public List<BillDto> OverdueBills { get; set; } = new();
        public List<ProviderAnalyticsDto> ProviderAnalytics { get; set; } = new();
        public List<BudgetStatusDto> BudgetStatuses { get; set; } = new();
        public List<BillAlertDto> Alerts { get; set; } = new();
        public BillAnalyticsDto Summary { get; set; } = new();
    }

    /// <summary>
    /// DTO for variance dashboard with aggregated variance data
    /// </summary>
    public class VarianceDashboardDto
    {
        // Summary Statistics
        public decimal TotalActualAmount { get; set; }
        public decimal TotalEstimatedAmount { get; set; }
        public decimal TotalVariance { get; set; }
        public int TotalBillsAnalyzed { get; set; }
        
        // Status Breakdown
        public int OverBudgetCount { get; set; }
        public int SlightlyOverCount { get; set; }
        public int OnTargetCount { get; set; }
        public int UnderBudgetCount { get; set; }
        public int NoDataCount { get; set; }
        
        // Individual Variances
        public List<BillVarianceDto> Variances { get; set; } = new();
        
        // Generated at timestamp
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}

