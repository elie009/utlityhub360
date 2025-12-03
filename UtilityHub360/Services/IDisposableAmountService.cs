using UtilityHub360.DTOs;

namespace UtilityHub360.Services
{
    public interface IDisposableAmountService
    {
        Task<DisposableAmountDto> GetDisposableAmountAsync(string userId, DateTime startDate, DateTime endDate, decimal? targetSavings = null, decimal? investmentAllocation = null);
        Task<DisposableAmountDto> GetMonthlyDisposableAmountAsync(string userId, int year, int month, decimal? targetSavings = null, decimal? investmentAllocation = null);
        Task<FinancialSummaryDto> GetFinancialSummaryAsync(string userId);
        Task<DisposableAmountDto> GetCurrentMonthDisposableAmountAsync(string userId, decimal? targetSavings = null, decimal? investmentAllocation = null);
        Task<SimpleFinancialSummaryDto> GetSimpleFinancialSummaryAsync(string userId, int? year = null, int? month = null);
        Task<RecentActivityDto> GetRecentActivityAsync(string userId);
    }
}

