using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class UserOnboarding
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Onboarding steps completion tracking
        public bool WelcomeCompleted { get; set; } = false;
        public bool IncomeSetupCompleted { get; set; } = false;
        public bool BillsSetupCompleted { get; set; } = false;
        public bool LoansSetupCompleted { get; set; } = false;
        public bool VariableExpensesSetupCompleted { get; set; } = false;
        public bool DashboardTourCompleted { get; set; } = false;
        public bool OnboardingCompleted { get; set; } = false;

        // Progress tracking
        public int CurrentStep { get; set; } = 1; // 1-6 steps
        public int TotalSteps { get; set; } = 6;
        public double CompletionPercentage => (double)GetCompletedSteps() / TotalSteps * 100;

        // Timestamps
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // User preferences from onboarding
        public string? PreferredCurrency { get; set; } = "PHP";
        public string? FinancialGoal { get; set; } // SAVINGS, DEBT_FREEDOM, EMERGENCY_FUND, etc.
        public decimal? MonthlyIncomeTarget { get; set; }
        public decimal? MonthlyExpenseTarget { get; set; }
        public decimal? SavingsGoalAmount { get; set; }
        public DateTime? SavingsGoalDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Helper methods
        public int GetCompletedSteps()
        {
            int completed = 0;
            if (WelcomeCompleted) completed++;
            if (IncomeSetupCompleted) completed++;
            if (BillsSetupCompleted) completed++;
            if (LoansSetupCompleted) completed++;
            if (VariableExpensesSetupCompleted) completed++;
            if (DashboardTourCompleted) completed++;
            return completed;
        }

        public bool IsStepCompleted(int stepNumber)
        {
            return stepNumber switch
            {
                1 => WelcomeCompleted,
                2 => IncomeSetupCompleted,
                3 => BillsSetupCompleted,
                4 => LoansSetupCompleted,
                5 => VariableExpensesSetupCompleted,
                6 => DashboardTourCompleted,
                _ => false
            };
        }

        public void MarkStepCompleted(int stepNumber)
        {
            switch (stepNumber)
            {
                case 1:
                    WelcomeCompleted = true;
                    break;
                case 2:
                    IncomeSetupCompleted = true;
                    break;
                case 3:
                    BillsSetupCompleted = true;
                    break;
                case 4:
                    LoansSetupCompleted = true;
                    break;
                case 5:
                    VariableExpensesSetupCompleted = true;
                    break;
                case 6:
                    DashboardTourCompleted = true;
                    break;
            }
            
            LastUpdatedAt = DateTime.UtcNow;
            
            // Check if onboarding is complete
            if (GetCompletedSteps() == TotalSteps)
            {
                OnboardingCompleted = true;
                CompletedAt = DateTime.UtcNow;
            }
        }

        public List<OnboardingStep> GetStepDetails()
        {
            return new List<OnboardingStep>
            {
                new OnboardingStep
                {
                    StepNumber = 1,
                    Title = "Welcome & Setup",
                    Description = "Get to know UtilityHub360 and set your preferences",
                    IsCompleted = WelcomeCompleted,
                    IsCurrent = CurrentStep == 1
                },
                new OnboardingStep
                {
                    StepNumber = 2,
                    Title = "Add Income Sources",
                    Description = "Tell us about your monthly income sources",
                    IsCompleted = IncomeSetupCompleted,
                    IsCurrent = CurrentStep == 2
                },
                new OnboardingStep
                {
                    StepNumber = 3,
                    Title = "Add Fixed Bills",
                    Description = "Add your recurring monthly bills and expenses",
                    IsCompleted = BillsSetupCompleted,
                    IsCurrent = CurrentStep == 3
                },
                new OnboardingStep
                {
                    StepNumber = 4,
                    Title = "Add Loans (Optional)",
                    Description = "Add any loans or debt payments you have",
                    IsCompleted = LoansSetupCompleted,
                    IsCurrent = CurrentStep == 4
                },
                new OnboardingStep
                {
                    StepNumber = 5,
                    Title = "Track Variable Expenses",
                    Description = "Start logging your daily spending",
                    IsCompleted = VariableExpensesSetupCompleted,
                    IsCurrent = CurrentStep == 5
                },
                new OnboardingStep
                {
                    StepNumber = 6,
                    Title = "Dashboard Tour",
                    Description = "Learn how to use your financial dashboard",
                    IsCompleted = DashboardTourCompleted,
                    IsCurrent = CurrentStep == 6
                }
            };
        }
    }

    public class OnboardingStep
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}
