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
        Task<ApiResponse<bool>> DeletePaymentAsync(string paymentId, string userId);
        Task<ApiResponse<object>> DebugPaymentAsync(string paymentId, string userId);
        
        // Bank Transaction methods (now using Payment table)
        Task<ApiResponse<PaymentDto>> CreateBankTransactionAsync(CreateBankTransactionDto transaction, string userId);
        Task<ApiResponse<PaymentDto>> GetBankTransactionAsync(string transactionId, string userId);
        Task<ApiResponse<List<PaymentDto>>> GetBankTransactionsAsync(string userId, string? accountType = null, int page = 1, int limit = 50);
        Task<ApiResponse<List<PaymentDto>>> GetAccountTransactionsAsync(string bankAccountId, string userId, int page = 1, int limit = 50);
        Task<ApiResponse<bool>> DeleteBankTransactionAsync(string transactionId, string userId);
        
        // Savings Transaction methods
        Task<ApiResponse<SavingsTransactionDto>> CreateSavingsTransactionAsync(CreateSavingsTransactionDto transaction, string userId);
    }
}

