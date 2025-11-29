using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // Main onboarding progress DTO
    public class OnboardingProgressDto
    {
        public string UserId { get; set; } = string.Empty;
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public double CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<OnboardingStepDto> Steps { get; set; } = new List<OnboardingStepDto>();
    }

    // Individual step DTO
    public class OnboardingStepDto
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }

    // Step 1: Welcome & Preferences
    public class WelcomeSetupDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string PreferredCurrency { get; set; } = "PHP";

        [Required]
        public string FinancialGoal { get; set; } = string.Empty; // SAVINGS, DEBT_FREEDOM, EMERGENCY_FUND, INVESTMENT

        [Range(0, double.MaxValue, ErrorMessage = "Monthly income target must be positive")]
        public decimal? MonthlyIncomeTarget { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly expense target must be positive")]
        public decimal? MonthlyExpenseTarget { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Savings goal amount must be positive")]
        public decimal? SavingsGoalAmount { get; set; }

        public DateTime? SavingsGoalDate { get; set; }
    }

    // Step 2: Income Sources Setup
    public class IncomeSetupDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one income source is required")]
        public List<IncomeSourceSetupDto> IncomeSources { get; set; } = new List<IncomeSourceSetupDto>();
    }

    public class IncomeSourceSetupDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Income source name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string Frequency { get; set; } = "MONTHLY"; // WEEKLY, BI_WEEKLY, MONTHLY, QUARTERLY, ANNUALLY

        [Required]
        public string Category { get; set; } = "PRIMARY"; // PRIMARY, SECONDARY, SIDE_HUSTLE, BONUS, INVESTMENT

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Step 3: Bills Setup
    public class BillsSetupDto
    {
        public List<BillSetupDto> Bills { get; set; } = new List<BillSetupDto>();
        public bool SkipBills { get; set; } = false; // Allow users to skip if they have no bills
    }

    public class BillSetupDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Bill name cannot exceed 100 characters")]
        public string BillName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Bill type cannot exceed 50 characters")]
        public string BillType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public string Frequency { get; set; } = "monthly"; // weekly, monthly, quarterly, annually

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Step 4: Loans Setup (Optional)
    public class LoansSetupDto
    {
        public List<LoanSetupDto> Loans { get; set; } = new List<LoanSetupDto>();
        public bool SkipLoans { get; set; } = false; // Allow users to skip if they have no loans
    }

    public class LoanSetupDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Loan name cannot exceed 100 characters")]
        public string LoanName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Loan type cannot exceed 50 characters")]
        public string LoanType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly payment must be greater than 0")]
        public decimal MonthlyPayment { get; set; }

        [Required]
        [Range(0.01, 100, ErrorMessage = "Interest rate must be between 0.01 and 100")]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Step 5: Variable Expenses Setup
    public class VariableExpensesSetupDto
    {
        public List<VariableExpenseSetupDto> Expenses { get; set; } = new List<VariableExpenseSetupDto>();
        public bool SkipExpenses { get; set; } = false; // Allow users to skip initial expense logging
    }

    public class VariableExpenseSetupDto
    {
        [Required]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty; // GROCERIES, TRANSPORTATION, FOOD, ENTERTAINMENT, SHOPPING, HEALTHCARE, etc.

        [Required]
        public DateTime ExpenseDate { get; set; }

        [StringLength(100)]
        public string? Merchant { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // Step 6: Dashboard Tour Completion
    public class DashboardTourDto
    {
        [Required]
        public bool TourCompleted { get; set; }

        public List<string>? ViewedSections { get; set; } // Track which sections user viewed
        public int TimeSpentSeconds { get; set; } = 0;
    }

    // Complete onboarding request
    public class CompleteOnboardingDto
    {
        [Required]
        public WelcomeSetupDto WelcomeSetup { get; set; } = new WelcomeSetupDto();

        [Required]
        public IncomeSetupDto IncomeSetup { get; set; } = new IncomeSetupDto();

        [Required]
        public BillsSetupDto BillsSetup { get; set; } = new BillsSetupDto();

        [Required]
        public LoansSetupDto LoansSetup { get; set; } = new LoansSetupDto();

        [Required]
        public VariableExpensesSetupDto VariableExpensesSetup { get; set; } = new VariableExpensesSetupDto();

        [Required]
        public DashboardTourDto DashboardTour { get; set; } = new DashboardTourDto();
    }


    // Quick setup response
    public class QuickSetupResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ItemsCreated { get; set; }
        public OnboardingProgressDto Progress { get; set; } = new OnboardingProgressDto();
        public List<string> CreatedItems { get; set; } = new List<string>();
    }
}
