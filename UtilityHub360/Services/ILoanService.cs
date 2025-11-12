using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface ILoanService
    {
        Task<ApiResponse<LoanDto>> ApplyForLoanAsync(CreateLoanApplicationDto application, string userId);
        Task<ApiResponse<LoanDto>> GetLoanAsync(string loanId, string userId);
        Task<ApiResponse<PaginatedResponse<LoanDto>>> GetUserLoansAsync(string userId, string? status, int page, int limit);
        Task<ApiResponse<object>> GetLoanStatusAsync(string loanId, string userId);
        Task<ApiResponse<List<RepaymentScheduleDto>>> GetRepaymentScheduleAsync(string loanId, string userId);
        Task<ApiResponse<List<TransactionDto>>> GetLoanTransactionsAsync(string loanId, string userId);
        Task<ApiResponse<LoanDto>> ApproveLoanAsync(string loanId, string adminId, string? notes);
        Task<ApiResponse<LoanDto>> RejectLoanAsync(string loanId, string adminId, string reason, string? notes);
        Task<ApiResponse<object>> DisburseLoanAsync(string loanId, string adminId, string disbursementMethod, string? reference, string? bankAccountId = null);
        Task<ApiResponse<LoanDto>> CloseLoanAsync(string loanId, string adminId, string? notes);
        Task<ApiResponse<bool>> DeleteLoanAsync(string loanId, string userId);
        Task<ApiResponse<PaymentDto>> MakeLoanPaymentAsync(string loanId, CreatePaymentDto payment, string userId);
        Task<ApiResponse<decimal>> GetTotalOutstandingLoanAmountAsync(string userId);
        Task<Loan?> GetLoanWithAccessCheckAsync(string loanId, string userId);
        
        // Payment Schedule Management
        Task<ApiResponse<PaymentScheduleResponseDto>> ExtendLoanTermAsync(string loanId, ExtendLoanTermDto extendDto, string userId);
        Task<ApiResponse<PaymentScheduleResponseDto>> AddPaymentScheduleAsync(string loanId, AddPaymentScheduleDto addDto, string userId);
        Task<ApiResponse<PaymentScheduleResponseDto>> AutoAddPaymentScheduleAsync(string loanId, AutoAddPaymentScheduleDto addDto, string userId);
        Task<ApiResponse<PaymentScheduleResponseDto>> RegeneratePaymentScheduleAsync(string loanId, RegenerateScheduleDto regenerateDto, string userId);
        Task<ApiResponse<bool>> DeletePaymentScheduleInstallmentAsync(string loanId, int installmentNumber, string userId);
        Task<ApiResponse<RepaymentScheduleDto>> MarkInstallmentAsPaidAsync(string loanId, int installmentNumber, MarkInstallmentPaidDto paymentDto, string userId);
        
        // Simple Schedule Update
        Task<ApiResponse<RepaymentScheduleDto>> UpdateScheduleSimpleAsync(string loanId, int installmentNumber, SimpleScheduleUpdateDto updateDto, string userId);
    }
}

