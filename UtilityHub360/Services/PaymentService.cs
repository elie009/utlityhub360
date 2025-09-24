using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PaymentDto>> MakePaymentAsync(CreatePaymentDto payment, string userId)
        {
            try
            {
                // Verify loan exists and belongs to user
                var loan = await _context.Loans
                    .FirstOrDefaultAsync(l => l.Id == payment.LoanId && l.UserId == userId);

                if (loan == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Loan not found");
                }

                if (loan.Status != "ACTIVE")
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Loan is not active");
                }

                // Check if payment reference already exists for this loan
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.LoanId == payment.LoanId && p.Reference == payment.Reference);

                if (existingPayment != null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Payment reference already exists for this loan");
                }

                // Create payment
                var newPayment = new Entities.Payment
                {
                    LoanId = payment.LoanId,
                    UserId = userId,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Reference = payment.Reference,
                    Status = "PENDING",
                    ProcessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(newPayment);

                // Create transaction record
                var transaction = new Entities.Transaction
                {
                    LoanId = payment.LoanId,
                    Type = "PAYMENT",
                    Amount = payment.Amount,
                    Description = $"Payment via {payment.Method}",
                    Reference = payment.Reference,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);

                // Update loan remaining balance
                loan.RemainingBalance -= payment.Amount;

                // Check if loan is fully paid
                if (loan.RemainingBalance <= 0)
                {
                    loan.Status = "COMPLETED";
                    loan.CompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Update payment status to completed
                newPayment.Status = "COMPLETED";
                await _context.SaveChangesAsync();

                var paymentDto = new PaymentDto
                {
                    Id = newPayment.Id,
                    LoanId = newPayment.LoanId,
                    UserId = newPayment.UserId,
                    Amount = newPayment.Amount,
                    Method = newPayment.Method,
                    Reference = newPayment.Reference,
                    Status = newPayment.Status,
                    ProcessedAt = newPayment.ProcessedAt,
                    CreatedAt = newPayment.CreatedAt
                };

                return ApiResponse<PaymentDto>.SuccessResult(paymentDto, "Payment processed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentDto>.ErrorResult($"Failed to process payment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaymentDto>> GetPaymentAsync(string paymentId, string userId)
        {
            try
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

                if (payment == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Payment not found");
                }

                var paymentDto = new PaymentDto
                {
                    Id = payment.Id,
                    LoanId = payment.LoanId,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Reference = payment.Reference,
                    Status = payment.Status,
                    ProcessedAt = payment.ProcessedAt,
                    CreatedAt = payment.CreatedAt
                };

                return ApiResponse<PaymentDto>.SuccessResult(paymentDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentDto>.ErrorResult($"Failed to get payment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<PaymentDto>>> GetLoanPaymentsAsync(string loanId, string userId, int page, int limit)
        {
            try
            {
                // Verify loan belongs to user
                var loan = await _context.Loans
                    .FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);

                if (loan == null)
                {
                    return ApiResponse<PaginatedResponse<PaymentDto>>.ErrorResult("Loan not found");
                }

                var query = _context.Payments.Where(p => p.LoanId == loanId);
                var totalCount = await query.CountAsync();

                var payments = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var paymentDtos = payments.Select(payment => new PaymentDto
                {
                    Id = payment.Id,
                    LoanId = payment.LoanId,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Reference = payment.Reference,
                    Status = payment.Status,
                    ProcessedAt = payment.ProcessedAt,
                    CreatedAt = payment.CreatedAt
                }).ToList();

                var paginatedResponse = new PaginatedResponse<PaymentDto>
                {
                    Data = paymentDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<PaymentDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<PaymentDto>>.ErrorResult($"Failed to get loan payments: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaymentDto>> UpdatePaymentStatusAsync(string paymentId, string status)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Payment not found");
                }

                payment.Status = status;
                await _context.SaveChangesAsync();

                var paymentDto = new PaymentDto
                {
                    Id = payment.Id,
                    LoanId = payment.LoanId,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Reference = payment.Reference,
                    Status = payment.Status,
                    ProcessedAt = payment.ProcessedAt,
                    CreatedAt = payment.CreatedAt
                };

                return ApiResponse<PaymentDto>.SuccessResult(paymentDto, "Payment status updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentDto>.ErrorResult($"Failed to update payment status: {ex.Message}");
            }
        }
    }
}

