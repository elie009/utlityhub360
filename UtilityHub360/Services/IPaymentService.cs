using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IPaymentService
    {
        Task<ApiResponse<PaymentDto>> MakePaymentAsync(CreatePaymentDto payment, string userId);
        Task<ApiResponse<PaymentDto>> GetPaymentAsync(string paymentId, string userId);
        Task<ApiResponse<PaginatedResponse<PaymentDto>>> GetLoanPaymentsAsync(string loanId, string userId, int page, int limit);
        Task<ApiResponse<PaymentDto>> UpdatePaymentStatusAsync(string paymentId, string status);
    }
}

