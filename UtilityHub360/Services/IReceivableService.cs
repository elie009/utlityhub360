using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IReceivableService
    {
        // Receivable CRUD Operations
        Task<ApiResponse<ReceivableDto>> CreateReceivableAsync(CreateReceivableDto createReceivableDto, string userId);
        Task<ApiResponse<ReceivableDto>> GetReceivableAsync(string receivableId, string userId);
        Task<ApiResponse<ReceivableDto>> UpdateReceivableAsync(string receivableId, UpdateReceivableDto updateReceivableDto, string userId);
        Task<ApiResponse<bool>> DeleteReceivableAsync(string receivableId, string userId);
        Task<ApiResponse<List<ReceivableDto>>> GetUserReceivablesAsync(string userId, string? status = null);

        // Receivable Payment Operations
        Task<ApiResponse<ReceivablePaymentDto>> RecordPaymentAsync(CreateReceivablePaymentDto paymentDto, string userId);
        Task<ApiResponse<List<ReceivablePaymentDto>>> GetReceivablePaymentsAsync(string receivableId, string userId);
        Task<ApiResponse<ReceivablePaymentDto>> GetPaymentAsync(string paymentId, string userId);
        Task<ApiResponse<bool>> DeletePaymentAsync(string paymentId, string userId);

        // Analytics & Summary
        Task<ApiResponse<decimal>> GetTotalReceivablesAsync(string userId);
        Task<ApiResponse<decimal>> GetTotalOutstandingAsync(string userId);
        Task<ApiResponse<decimal>> GetTotalPaidAsync(string userId);
        Task<ApiResponse<ReceivableDto>> MarkReceivableAsCompletedAsync(string receivableId, string userId);
    }
}

