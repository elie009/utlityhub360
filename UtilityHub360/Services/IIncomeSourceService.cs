using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IIncomeSourceService
    {
        // Income Source Management
        Task<ApiResponse<IncomeSourceDto>> CreateIncomeSourceAsync(CreateIncomeSourceDto createIncomeSourceDto, string userId);
        Task<ApiResponse<IncomeSourceDto>> GetIncomeSourceAsync(string incomeSourceId, string userId);
        Task<ApiResponse<List<IncomeSourceDto>>> GetUserIncomeSourcesAsync(string userId, bool activeOnly = true);
        Task<ApiResponse<IncomeSourceListResponseDto>> GetUserIncomeSourcesWithSummaryAsync(string userId, bool activeOnly = true);
        Task<ApiResponse<IncomeSourceDto>> UpdateIncomeSourceAsync(string incomeSourceId, UpdateIncomeSourceDto updateIncomeSourceDto, string userId);
        Task<ApiResponse<bool>> DeleteIncomeSourceAsync(string incomeSourceId, string userId);
        Task<ApiResponse<ToggleStatusResponseDto>> ToggleIncomeSourceStatusAsync(string incomeSourceId, string userId);
        Task<ApiResponse<ToggleStatusResponseDto>> GetIncomeSourceSummaryAsync(string userId);

        // Bulk Operations
        Task<ApiResponse<List<IncomeSourceDto>>> CreateMultipleIncomeSourcesAsync(CreateMultipleIncomeSourcesDto createMultipleDto, string userId);
        Task<ApiResponse<List<IncomeSourceDto>>> UpdateMultipleIncomeSourcesAsync(UpdateMultipleIncomeSourcesDto updateMultipleDto, string userId);

        // Analytics and Reporting
        Task<ApiResponse<IncomeSummaryDto>> GetIncomeSummaryAsync(string userId);
        Task<ApiResponse<IncomeAnalyticsDto>> GetIncomeAnalyticsAsync(string userId, string period = "month");
        Task<ApiResponse<Dictionary<string, decimal>>> GetIncomeByCategoryAsync(string userId);
        Task<ApiResponse<Dictionary<string, decimal>>> GetIncomeByFrequencyAsync(string userId);
        Task<ApiResponse<decimal>> GetTotalMonthlyIncomeAsync(string userId);
        Task<ApiResponse<decimal>> GetNetMonthlyIncomeAsync(string userId);

        // Category and Frequency Management
        Task<ApiResponse<List<string>>> GetAvailableCategoriesAsync();
        Task<ApiResponse<List<string>>> GetAvailableFrequenciesAsync();
        Task<ApiResponse<List<IncomeSourceDto>>> GetIncomeSourcesByCategoryAsync(string userId, string category);
        Task<ApiResponse<List<IncomeSourceDto>>> GetIncomeSourcesByFrequencyAsync(string userId, string frequency);

        // Admin Operations
        Task<ApiResponse<List<IncomeSourceDto>>> GetAllIncomeSourcesAsync(int page = 1, int limit = 50);
        Task<ApiResponse<List<IncomeSourceDto>>> GetIncomeSourcesByUserAsync(string userId, int page = 1, int limit = 50);
    }
}
