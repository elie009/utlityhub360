using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IUserProfileService
    {
        // User Profile CRUD Operations
        Task<ApiResponse<UserProfileDto>> CreateUserProfileAsync(CreateUserProfileDto createProfileDto, string userId);
        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId);
        Task<ApiResponse<UserProfileDto>> UpdateUserProfileAsync(UpdateUserProfileDto updateProfileDto, string userId);
        Task<ApiResponse<bool>> DeleteUserProfileAsync(string userId);
        Task<ApiResponse<bool>> ActivateUserProfileAsync(string userId);
        Task<ApiResponse<bool>> DeactivateUserProfileAsync(string userId);


        // Financial Goals Management
        Task<ApiResponse<UserProfileDto>> UpdateSavingsGoalAsync(decimal monthlySavingsGoal, string userId);
        Task<ApiResponse<UserProfileDto>> UpdateInvestmentGoalAsync(decimal monthlyInvestmentGoal, string userId);
        Task<ApiResponse<UserProfileDto>> UpdateEmergencyFundGoalAsync(decimal monthlyEmergencyFundGoal, string userId);

        // Tax Management
        Task<ApiResponse<UserProfileDto>> UpdateTaxInformationAsync(decimal? taxRate, decimal? monthlyTaxDeductions, string userId);

        // Analytics and Reporting
        Task<ApiResponse<Dictionary<string, decimal>>> GetGoalsBreakdownAsync(string userId);

        // Employment Information
        Task<ApiResponse<UserProfileDto>> UpdateEmploymentInfoAsync(string jobTitle, string company, string employmentType, string industry, string location, string userId);

        // Validation and Business Rules
        Task<ApiResponse<bool>> ValidateIncomeGoalsAsync(string userId);
        Task<ApiResponse<decimal>> CalculateSavingsRateAsync(string userId);
        Task<ApiResponse<decimal>> CalculateDisposableIncomeAsync(string userId);

        // Admin Operations
        Task<ApiResponse<List<UserProfileDto>>> GetAllUserProfilesAsync(int page = 1, int limit = 50);
        Task<ApiResponse<List<UserProfileDto>>> GetProfilesByIncomeRangeAsync(decimal minIncome, decimal maxIncome, int page = 1, int limit = 50);
        Task<ApiResponse<List<UserProfileDto>>> GetProfilesByEmploymentTypeAsync(string employmentType, int page = 1, int limit = 50);
    }
}
