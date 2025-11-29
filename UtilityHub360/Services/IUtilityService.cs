using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IUtilityService
    {
        // CRUD Operations
        Task<ApiResponse<UtilityDto>> CreateUtilityAsync(CreateUtilityDto createUtilityDto, string userId);
        Task<ApiResponse<UtilityDto>> GetUtilityAsync(string utilityId, string userId);
        Task<ApiResponse<UtilityDto>> UpdateUtilityAsync(string utilityId, UpdateUtilityDto updateUtilityDto, string userId);
        Task<ApiResponse<bool>> DeleteUtilityAsync(string utilityId, string userId);
        Task<ApiResponse<PaginatedResponse<UtilityDto>>> GetUserUtilitiesAsync(string userId, string? status, string? utilityType, int page, int limit);

        // Analytics and Reporting
        Task<ApiResponse<decimal>> GetTotalPendingAmountAsync(string userId);
        Task<ApiResponse<decimal>> GetTotalPaidAmountAsync(string userId, string period);
        Task<ApiResponse<decimal>> GetTotalOverdueAmountAsync(string userId);
        Task<ApiResponse<UtilityAnalyticsDto>> GetUtilityAnalyticsAsync(string userId);

        // Utility Management
        Task<ApiResponse<UtilityDto>> MarkUtilityAsPaidAsync(string utilityId, string userId, string? notes, string? bankAccountId = null);
        Task<ApiResponse<bool>> UpdateUtilityStatusAsync(string utilityId, string status, string userId);
        Task<ApiResponse<List<UtilityDto>>> GetOverdueUtilitiesAsync(string userId);
        Task<ApiResponse<List<UtilityDto>>> GetUpcomingUtilitiesAsync(string userId, int days = 7);

        // Consumption Tracking
        Task<ApiResponse<UtilityConsumptionHistoryDto>> GetConsumptionHistoryAsync(string utilityId, string userId, int months = 12);
        Task<ApiResponse<List<UtilityConsumptionHistoryDto>>> GetAllConsumptionHistoryAsync(string userId, string? utilityType = null, int months = 12);

        // Comparison Tools
        Task<ApiResponse<UtilityComparisonDto>> CompareProvidersAsync(string userId, string utilityType);
        Task<ApiResponse<List<UtilityComparisonDto>>> CompareAllUtilityTypesAsync(string userId);
        Task<ApiResponse<List<ProviderComparisonDto>>> GetProviderComparisonAsync(string userId, string utilityType);
    }
}

