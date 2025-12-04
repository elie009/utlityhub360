using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface ISubscriptionService
    {
        // Subscription Plan Management
        Task<ApiResponse<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId);
        Task<ApiResponse<List<SubscriptionPlanDto>>> GetAllSubscriptionPlansAsync(bool activeOnly = false);
        Task<ApiResponse<SubscriptionPlanDto>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto createDto);
        Task<ApiResponse<SubscriptionPlanDto>> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto);
        Task<ApiResponse<bool>> DeleteSubscriptionPlanAsync(string planId);

        // User Subscription Management
        Task<ApiResponse<UserSubscriptionDto>> GetUserSubscriptionAsync(string userId);
        Task<ApiResponse<UserSubscriptionDto>> CreateUserSubscriptionAsync(CreateUserSubscriptionDto createDto);
        Task<ApiResponse<UserSubscriptionDto>> UpdateUserSubscriptionAsync(string subscriptionId, UpdateUserSubscriptionDto updateDto);
        Task<ApiResponse<bool>> CancelUserSubscriptionAsync(string subscriptionId);
        Task<ApiResponse<List<UserSubscriptionDto>>> GetAllUserSubscriptionsAsync(int page = 1, int limit = 50, string? status = null, string? planId = null);
        Task<ApiResponse<UserWithSubscriptionDto>> GetUserWithSubscriptionAsync(string userId);

        // Usage Tracking
        Task<ApiResponse<bool>> IncrementUsageAsync(string userId, string usageType, int amount = 1);
        Task<ApiResponse<bool>> ResetMonthlyUsageAsync(string userId);
        Task<ApiResponse<object>> GetUsageStatsAsync(string userId);

        // Subscription Validation
        Task<ApiResponse<bool>> CheckFeatureAccessAsync(string userId, string feature);
        Task<ApiResponse<bool>> CheckLimitAsync(string userId, string limitType, int currentCount);
    }
}

