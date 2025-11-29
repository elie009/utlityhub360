using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IAllocationService
    {
        // Template Operations
        Task<ApiResponse<List<AllocationTemplateDto>>> GetTemplatesAsync(string? userId = null);
        Task<ApiResponse<AllocationTemplateDto>> GetTemplateAsync(string templateId);
        Task<ApiResponse<AllocationTemplateDto>> CreateTemplateAsync(CreateAllocationTemplateDto dto, string userId);
        Task<ApiResponse<bool>> DeleteTemplateAsync(string templateId, string userId);

        // Plan Operations
        Task<ApiResponse<AllocationPlanDto>> GetActivePlanAsync(string userId);
        Task<ApiResponse<AllocationPlanDto>> GetPlanAsync(string planId, string userId);
        Task<ApiResponse<List<AllocationPlanDto>>> GetPlansAsync(string userId);
        Task<ApiResponse<AllocationPlanDto>> CreatePlanAsync(CreateAllocationPlanDto dto, string userId);
        Task<ApiResponse<AllocationPlanDto>> UpdatePlanAsync(string planId, UpdateAllocationPlanDto dto, string userId);
        Task<ApiResponse<bool>> DeletePlanAsync(string planId, string userId);
        Task<ApiResponse<AllocationPlanDto>> ApplyTemplateAsync(string templateId, decimal monthlyIncome, string userId);

        // Category Operations
        Task<ApiResponse<List<AllocationCategoryDto>>> GetCategoriesAsync(string planId, string userId);
        Task<ApiResponse<AllocationCategoryDto>> GetCategoryAsync(string categoryId, string userId);
        Task<ApiResponse<AllocationCategoryDto>> UpdateCategoryAsync(string categoryId, CreateAllocationCategoryDto dto, string userId);
        Task<ApiResponse<bool>> DeleteCategoryAsync(string categoryId, string userId);

        // History & Tracking
        Task<ApiResponse<List<AllocationHistoryDto>>> GetHistoryAsync(string userId, AllocationHistoryQueryDto query);
        Task<ApiResponse<bool>> RecordHistoryAsync(string userId, string planId);
        Task<ApiResponse<List<AllocationTrendDto>>> GetTrendsAsync(string userId, string? planId = null, int months = 12);

        // Recommendations
        Task<ApiResponse<List<AllocationRecommendationDto>>> GetRecommendationsAsync(string userId, string? planId = null);
        Task<ApiResponse<bool>> MarkRecommendationReadAsync(string recommendationId, string userId);
        Task<ApiResponse<bool>> ApplyRecommendationAsync(string recommendationId, string userId);
        Task<ApiResponse<List<AllocationRecommendationDto>>> GenerateRecommendationsAsync(string userId, string planId);

        // Calculations & Formulas
        Task<ApiResponse<AllocationCalculationDto>> CalculateAllocationAsync(decimal monthlyIncome, List<CreateAllocationTemplateCategoryDto> categories);
        Task<ApiResponse<AllocationSummaryDto>> CalculateSummaryAsync(string planId, string userId);
        Task<ApiResponse<AllocationChartDataDto>> GetChartDataAsync(string planId, string userId);
        Task<ApiResponse<AllocationChartDataDto>> GetChartDataForPeriodAsync(string planId, string userId, DateTime periodDate);

        // Category-based Allocation
        Task<ApiResponse<AllocationPlanDto>> CreateCategoryBasedPlanAsync(decimal monthlyIncome, Dictionary<string, decimal> categoryPercentages, string userId);
        Task<ApiResponse<Dictionary<string, decimal>>> GetCategoryActualsAsync(string planId, string userId, DateTime? periodDate = null);
    }
}

