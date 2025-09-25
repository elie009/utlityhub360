using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IBillService
    {
        // CRUD Operations
        Task<ApiResponse<BillDto>> CreateBillAsync(CreateBillDto createBillDto, string userId);
        Task<ApiResponse<BillDto>> GetBillAsync(string billId, string userId);
        Task<ApiResponse<BillDto>> UpdateBillAsync(string billId, UpdateBillDto updateBillDto, string userId);
        Task<ApiResponse<bool>> DeleteBillAsync(string billId, string userId);
        Task<ApiResponse<PaginatedResponse<BillDto>>> GetUserBillsAsync(string userId, string? status, string? billType, int page, int limit);

        // Analytics and Reporting
        Task<ApiResponse<decimal>> GetTotalPendingAmountAsync(string userId);
        Task<ApiResponse<BillSummaryDto>> GetTotalPaidAmountAsync(string userId, string period); // week, month, quarter, year
        Task<ApiResponse<decimal>> GetTotalOverdueAmountAsync(string userId);
        Task<ApiResponse<BillAnalyticsDto>> GetBillAnalyticsAsync(string userId);

        // Bill Management
        Task<ApiResponse<BillDto>> MarkBillAsPaidAsync(string billId, string userId, string? notes);
        Task<ApiResponse<bool>> UpdateBillStatusAsync(string billId, string status, string userId);
        Task<ApiResponse<List<BillDto>>> GetOverdueBillsAsync(string userId);
        Task<ApiResponse<List<BillDto>>> GetUpcomingBillsAsync(string userId, int days = 7);

        // Admin Operations
        Task<ApiResponse<PaginatedResponse<BillDto>>> GetAllBillsAsync(string? status, string? billType, int page, int limit);
    }
}
