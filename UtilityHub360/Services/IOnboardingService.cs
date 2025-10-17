using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IOnboardingService
    {
        // Progress tracking
        Task<ApiResponse<OnboardingProgressDto>> GetOnboardingProgressAsync(string userId);
        Task<ApiResponse<OnboardingProgressDto>> StartOnboardingAsync(string userId);
        Task<ApiResponse<OnboardingProgressDto>> UpdateCurrentStepAsync(string userId, int stepNumber);

        // Step completion
        Task<ApiResponse<QuickSetupResponseDto>> CompleteWelcomeStepAsync(string userId, WelcomeSetupDto welcomeSetup);
        Task<ApiResponse<QuickSetupResponseDto>> CompleteIncomeStepAsync(string userId, IncomeSetupDto incomeSetup);
        Task<ApiResponse<QuickSetupResponseDto>> CompleteBillsStepAsync(string userId, BillsSetupDto billsSetup);
        Task<ApiResponse<QuickSetupResponseDto>> CompleteLoansStepAsync(string userId, LoansSetupDto loansSetup);
        Task<ApiResponse<QuickSetupResponseDto>> CompleteVariableExpensesStepAsync(string userId, VariableExpensesSetupDto expensesSetup);
        Task<ApiResponse<OnboardingProgressDto>> CompleteDashboardTourAsync(string userId, DashboardTourDto dashboardTour);

        // Complete onboarding
        Task<ApiResponse<OnboardingProgressDto>> CompleteOnboardingAsync(string userId, CompleteOnboardingDto completeOnboarding);
        Task<ApiResponse<OnboardingProgressDto>> SkipOnboardingAsync(string userId);
        Task<ApiResponse<OnboardingProgressDto>> ResetOnboardingAsync(string userId);

        // Helper methods
        Task<ApiResponse<OnboardingProgressDto>> GetOrCreateOnboardingAsync(string userId);
    }
}
