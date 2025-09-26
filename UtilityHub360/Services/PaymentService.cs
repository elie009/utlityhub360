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

        public async Task<ApiResponse<bool>> DeletePaymentAsync(string paymentId, string userId)
        {
            try
            {
                // Find the payment and verify it belongs to the user
                var payment = await _context.Payments
                    .Include(p => p.Loan)
                    .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

                if (payment == null)
                {
                    return ApiResponse<bool>.ErrorResult("Payment not found or you don't have permission to delete it");
                }

                // Check if payment can be deleted based on business rules
                if (payment.Status == "COMPLETED")
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete completed payments. Please contact support if this is an error.");
                }

                // Check if the loan is still active (only allow deletion if loan is active)
                if (payment.Loan.Status != "ACTIVE")
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete payments for loans that are not active");
                }

                // Check if payment is older than 24 hours (business rule)
                var hoursSinceCreation = (DateTime.UtcNow - payment.CreatedAt).TotalHours;
                if (hoursSinceCreation > 24)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete payments older than 24 hours");
                }

                // Find and remove the associated transaction
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.LoanId == payment.LoanId && 
                                            t.Type == "PAYMENT" && 
                                            t.Reference == payment.Reference);

                if (transaction != null)
                {
                    _context.Transactions.Remove(transaction);
                }

                // Restore the loan balance
                payment.Loan.RemainingBalance += payment.Amount;

                // If the loan was marked as completed due to this payment, revert it back to active
                if (payment.Loan.Status == "COMPLETED" && payment.Loan.RemainingBalance > 0)
                {
                    payment.Loan.Status = "ACTIVE";
                    payment.Loan.CompletedAt = null;
                }

                // Remove the payment
                _context.Payments.Remove(payment);

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Payment deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete payment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> DebugPaymentAsync(string paymentId, string userId)
        {
            try
            {
                var debugInfo = new
                {
                    PaymentId = paymentId,
                    UserId = userId,
                    PaymentExists = false,
                    PaymentBelongsToUser = false,
                    PaymentDetails = (object?)null,
                    AllUserPayments = new List<object>(),
                    ErrorMessage = ""
                };

                // Check if payment exists at all
                var payment = await _context.Payments
                    .Include(p => p.Loan)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    debugInfo = new
                    {
                        PaymentId = paymentId,
                        UserId = userId,
                        PaymentExists = false,
                        PaymentBelongsToUser = false,
                        PaymentDetails = (object?)null,
                        AllUserPayments = (await _context.Payments
                            .Where(p => p.UserId == userId)
                            .Select(p => new { p.Id, p.LoanId, p.Amount, p.Status, p.CreatedAt })
                            .ToListAsync()).Cast<object>().ToList(),
                        ErrorMessage = "Payment does not exist in database"
                    };
                }
                else
                {
                    var belongsToUser = payment.UserId == userId;
                    debugInfo = new
                    {
                        PaymentId = paymentId,
                        UserId = userId,
                        PaymentExists = true,
                        PaymentBelongsToUser = belongsToUser,
                        PaymentDetails = (object)new
                        {
                            payment.Id,
                            payment.LoanId,
                            payment.UserId,
                            payment.Amount,
                            payment.Status,
                            payment.Method,
                            payment.Reference,
                            payment.CreatedAt,
                            payment.ProcessedAt,
                            LoanStatus = payment.Loan?.Status,
                            HoursSinceCreation = (DateTime.UtcNow - payment.CreatedAt).TotalHours
                        },
                        AllUserPayments = (await _context.Payments
                            .Where(p => p.UserId == userId)
                            .Select(p => new { p.Id, p.LoanId, p.Amount, p.Status, p.CreatedAt })
                            .ToListAsync()).Cast<object>().ToList(),
                        ErrorMessage = belongsToUser ? "Payment exists and belongs to user" : "Payment exists but belongs to different user"
                    };
                }

                return ApiResponse<object>.SuccessResult(debugInfo, "Debug information retrieved");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult($"Debug failed: {ex.Message}");
            }
        }
    }
}

