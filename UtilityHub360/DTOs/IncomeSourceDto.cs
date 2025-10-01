using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class CreateIncomeSourceDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Income source name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Frequency cannot exceed 50 characters")]
        public string Frequency { get; set; } = "MONTHLY"; // WEEKLY, BI_WEEKLY, MONTHLY, QUARTERLY, ANNUALLY

        [Required]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = "PRIMARY"; // PRIMARY, PASSIVE, BUSINESS, SIDE_HUSTLE, INVESTMENT, RENTAL, OTHER

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string Currency { get; set; } = "USD";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string? Company { get; set; }
    }

    public class UpdateIncomeSourceDto
    {
        [StringLength(100, ErrorMessage = "Income source name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }

        [StringLength(50, ErrorMessage = "Frequency cannot exceed 50 characters")]
        public string? Frequency { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string? Category { get; set; }

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string? Currency { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string? Company { get; set; }

        public bool? IsActive { get; set; }
    }

    public class IncomeSourceDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Frequency { get; set; } = "MONTHLY";
        public string Category { get; set; } = "PRIMARY";
        public string Currency { get; set; } = "USD";
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? Company { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal MonthlyAmount { get; set; }
    }

    public class IncomeSummaryDto
    {
        public decimal TotalMonthlyIncome { get; set; }
        public decimal NetMonthlyIncome { get; set; }
        public decimal TotalMonthlyGoals { get; set; }
        public decimal DisposableIncome { get; set; }
        public decimal SavingsRate { get; set; }
        public Dictionary<string, decimal> IncomeByCategory { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> IncomeByFrequency { get; set; } = new Dictionary<string, decimal>();
        public List<IncomeSourceDto> IncomeSources { get; set; } = new List<IncomeSourceDto>();
        public Dictionary<string, decimal> GoalsBreakdown { get; set; } = new Dictionary<string, decimal>();
    }

    public class IncomeAnalyticsDto
    {
        public decimal AverageMonthlyIncome { get; set; }
        public decimal HighestMonthlyIncome { get; set; }
        public decimal LowestMonthlyIncome { get; set; }
        public Dictionary<string, decimal> IncomeTrend { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> CategoryDistribution { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> FrequencyDistribution { get; set; } = new Dictionary<string, decimal>();
        public List<string> TopIncomeSources { get; set; } = new List<string>();
    }

    // DTOs for bulk operations
    public class CreateMultipleIncomeSourcesDto
    {
        [Required]
        public List<CreateIncomeSourceDto> IncomeSources { get; set; } = new List<CreateIncomeSourceDto>();
    }

    public class UpdateMultipleIncomeSourcesDto
    {
        [Required]
        public List<UpdateIncomeSourceDto> IncomeSources { get; set; } = new List<UpdateIncomeSourceDto>();
    }

    public class BulkCreateIncomeSourceDto
    {
        [Required(ErrorMessage = "At least one income source is required.")]
        public List<CreateIncomeSourceDto> IncomeSources { get; set; } = new List<CreateIncomeSourceDto>();
    }

    public class IncomeSourceListResponseDto
    {
        public List<IncomeSourceDto> IncomeSources { get; set; } = new List<IncomeSourceDto>();
        public int TotalActiveSources { get; set; }
        public int TotalPrimarySources { get; set; }
        public int TotalSources { get; set; }
        public decimal TotalMonthlyIncome { get; set; }
    }

    public class ToggleStatusResponseDto
    {
        public int TotalActiveSources { get; set; }
        public int TotalPrimarySources { get; set; }
        public int TotalSources { get; set; }
    }
}
