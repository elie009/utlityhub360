using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class CreateUserProfileDto
    {
        // Employment Information
        [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string? JobTitle { get; set; }

        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string? Company { get; set; }

        [StringLength(50, ErrorMessage = "Employment type cannot exceed 50 characters")]
        public string? EmploymentType { get; set; } // FULL_TIME, PART_TIME, CONTRACT, FREELANCE, SELF_EMPLOYED

        // Financial Goals
        [Range(0, double.MaxValue, ErrorMessage = "Monthly savings goal must be a positive number")]
        public decimal? MonthlySavingsGoal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly investment goal must be a positive number")]
        public decimal? MonthlyInvestmentGoal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly emergency fund goal must be a positive number")]
        public decimal? MonthlyEmergencyFundGoal { get; set; }

        // Tax Information
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal? TaxRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly tax deductions must be a positive number")]
        public decimal? MonthlyTaxDeductions { get; set; }

        // Additional Information
        [StringLength(100, ErrorMessage = "Industry cannot exceed 100 characters")]
        public string? Industry { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        // Currency Preference
        [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters")]
        public string? PreferredCurrency { get; set; } = "USD";

        // Generalized Income Sources
        public List<CreateIncomeSourceDto>? IncomeSources { get; set; }
    }

    public class UpdateUserProfileDto
    {
        // Employment Information
        [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string? JobTitle { get; set; }

        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string? Company { get; set; }

        [StringLength(50, ErrorMessage = "Employment type cannot exceed 50 characters")]
        public string? EmploymentType { get; set; }

        // Financial Goals
        [Range(0, double.MaxValue, ErrorMessage = "Monthly savings goal must be a positive number")]
        public decimal? MonthlySavingsGoal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly investment goal must be a positive number")]
        public decimal? MonthlyInvestmentGoal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly emergency fund goal must be a positive number")]
        public decimal? MonthlyEmergencyFundGoal { get; set; }

        // Tax Information
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal? TaxRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly tax deductions must be a positive number")]
        public decimal? MonthlyTaxDeductions { get; set; }

        // Additional Information
        [StringLength(100, ErrorMessage = "Industry cannot exceed 100 characters")]
        public string? Industry { get; set; }

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        // Currency Preference
        [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters")]
        public string? PreferredCurrency { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        // Employment Information
        public string? JobTitle { get; set; }
        public string? Company { get; set; }
        public string? EmploymentType { get; set; }

        // Financial Goals
        public decimal? MonthlySavingsGoal { get; set; }
        public decimal? MonthlyInvestmentGoal { get; set; }
        public decimal? MonthlyEmergencyFundGoal { get; set; }

        // Tax Information
        public decimal? TaxRate { get; set; }
        public decimal? MonthlyTaxDeductions { get; set; }

        // Additional Information
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string? Country { get; set; }
        public string? Notes { get; set; }

        // Currency Preference
        public string PreferredCurrency { get; set; } = "USD";

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed Properties
        public decimal TotalMonthlyIncome { get; set; }
        public decimal NetMonthlyIncome { get; set; }
        public decimal TotalMonthlyGoals { get; set; }
        public decimal DisposableIncome { get; set; }

        // Generalized Income Sources
        public List<IncomeSourceDto> IncomeSources { get; set; } = new List<IncomeSourceDto>();
    }

}
