using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IAnalyticsService
    {
        Task<ApiResponse<MonthlyCashFlowDto>> GetMonthlyCashFlowAsync(string userId, int? year = null);
    }
}

