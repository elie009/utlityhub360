using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class LoanService : ILoanService
    {
        private readonly ApplicationDbContext _context;

        public LoanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Loan?> GetLoanWithAccessCheckAsync(string loanId, string userId)
        {
            // First, check if the user is admin
            var user = await _context.Users.FindAsync(userId);
            var isAdmin = user?.Role == "ADMIN";

            if (isAdmin)
            {
                // Admin can access any loan
                return await _context.Loans.FindAsync(loanId);
            }
            else
            {
                // Regular users can only access their own loans
                return await _context.Loans
                    .FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);
            }
        }

        public async Task<ApiResponse<LoanDto>> ApplyForLoanAsync(CreateLoanApplicationDto application, string userId)
        {
            try
            {
                // Create loan application
                var loanApplication = new Entities.LoanApplication
                {
                    UserId = userId,
                    Principal = application.Principal,
                    Purpose = application.Purpose,
                    Term = application.Term,
                    MonthlyIncome = application.MonthlyIncome,
                    EmploymentStatus = application.EmploymentStatus,
                    AdditionalInfo = application.AdditionalInfo,
                    Status = "PENDING",
                    AppliedAt = DateTime.UtcNow
                };

                _context.LoanApplications.Add(loanApplication);
                await _context.SaveChangesAsync();

                // Create loan from application using provided interest rate
                var interestRate = application.InterestRate;
                var monthlyPayment = CalculateMonthlyPayment(application.Principal, interestRate, application.Term);
                var totalAmount = monthlyPayment * application.Term;

                var loan = new Entities.Loan
                {
                    UserId = userId,
                    Principal = application.Principal,
                    InterestRate = interestRate,
                    Term = application.Term,
                    Purpose = application.Purpose,
                    Status = "PENDING",
                    MonthlyPayment = monthlyPayment,
                    TotalAmount = totalAmount,
                    RemainingBalance = totalAmount,
                    AppliedAt = DateTime.UtcNow,
                    AdditionalInfo = application.AdditionalInfo
                };

                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();

                // Create repayment schedule
                await CreateRepaymentScheduleAsync(loan.Id, monthlyPayment, application.Term, interestRate);

                // Update due dates based on repayment schedule
                await UpdateLoanDueDatesAsync(loan);
                await _context.SaveChangesAsync();

                var loanDto = new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo
                    // NextDueDate = loan.NextDueDate,
                    // FinalDueDate = loan.FinalDueDate
                };

                return ApiResponse<LoanDto>.SuccessResult(loanDto, "Loan application submitted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoanDto>.ErrorResult($"Failed to apply for loan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoanDto>> GetLoanAsync(string loanId, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);

                if (loan == null)
                {
                    return ApiResponse<LoanDto>.ErrorResult("Loan not found");
                }

                // Get next due date from RepaymentSchedule
                var nextDueDate = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId && rs.Status == "PENDING")
                    .OrderBy(rs => rs.DueDate)
                    .Select(rs => rs.DueDate)
                    .FirstOrDefaultAsync();

                var loanDto = new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo,
                    NextDueDate = nextDueDate == default(DateTime) ? null : nextDueDate
                };

                return ApiResponse<LoanDto>.SuccessResult(loanDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<LoanDto>.ErrorResult($"Failed to get loan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<LoanDto>>> GetUserLoansAsync(string userId, string? status, int page, int limit)
        {
            try
            {
                var query = _context.Loans.Where(l => l.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(l => l.Status == status);
                }

                var totalCount = await query.CountAsync();
                var loans = await query
                    .OrderByDescending(l => l.AppliedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Get loan IDs for batch query
                var loanIds = loans.Select(l => l.Id).ToList();
                
                // Get next due dates from RepaymentSchedule for all loans at once
                var nextDueDates = await _context.RepaymentSchedules
                    .Where(rs => loanIds.Contains(rs.LoanId) && rs.Status == "PENDING")
                    .GroupBy(rs => rs.LoanId)
                    .Select(g => new
                    {
                        LoanId = g.Key,
                        NextDueDate = g.OrderBy(rs => rs.DueDate).FirstOrDefault()!.DueDate
                    })
                    .ToListAsync();

                var loanDtos = loans.Select(loan => new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo,
                    NextDueDate = nextDueDates.FirstOrDefault(d => d.LoanId == loan.Id)?.NextDueDate
                }).ToList();

                var paginatedResponse = new PaginatedResponse<LoanDto>
                {
                    Data = loanDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<LoanDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<LoanDto>>.ErrorResult($"Failed to get user loans: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> GetLoanStatusAsync(string loanId, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);

                if (loan == null)
                {
                    return ApiResponse<object>.ErrorResult("Loan not found");
                }

                var status = new
                {
                    status = loan.Status,
                    outstandingBalance = loan.RemainingBalance
                };

                return ApiResponse<object>.SuccessResult(status);
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult($"Failed to get loan status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<RepaymentScheduleDto>>> GetRepaymentScheduleAsync(string loanId, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);

                if (loan == null)
                {
                    return ApiResponse<List<RepaymentScheduleDto>>.ErrorResult("Loan not found");
                }

                var schedules = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .OrderBy(rs => rs.InstallmentNumber)
                    .ToListAsync();

                var scheduleDtos = schedules.Select(schedule => new RepaymentScheduleDto
                {
                    Id = schedule.Id,
                    LoanId = schedule.LoanId,
                    InstallmentNumber = schedule.InstallmentNumber,
                    DueDate = schedule.DueDate,
                    PrincipalAmount = schedule.PrincipalAmount,
                    InterestAmount = schedule.InterestAmount,
                    TotalAmount = schedule.TotalAmount,
                    Status = schedule.Status,
                    PaidAt = schedule.PaidAt
                }).ToList();

                return ApiResponse<List<RepaymentScheduleDto>>.SuccessResult(scheduleDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<RepaymentScheduleDto>>.ErrorResult($"Failed to get repayment schedule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TransactionDto>>> GetLoanTransactionsAsync(string loanId, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);

                if (loan == null)
                {
                    return ApiResponse<List<TransactionDto>>.ErrorResult("Loan not found");
                }

                var payments = await _context.Payments
                    .Where(p => p.LoanId == loanId && !p.IsBankTransaction) // Only loan activities, not bank transactions
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var transactionDtos = payments.Select(payment => new TransactionDto
                {
                    Id = payment.Id,
                    LoanId = payment.LoanId ?? "",
                    Type = payment.TransactionType ?? "UNKNOWN",
                    Amount = payment.Amount,
                    Description = payment.Description ?? "",
                    Reference = payment.Reference,
                    CreatedAt = payment.CreatedAt
                }).ToList();

                return ApiResponse<List<TransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TransactionDto>>.ErrorResult($"Failed to get loan transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoanDto>> ApproveLoanAsync(string loanId, string adminId, string? notes)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan == null)
                {
                    return ApiResponse<LoanDto>.ErrorResult("Loan not found");
                }

                loan.Status = "APPROVED";
                loan.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var loanDto = new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo
                };

                return ApiResponse<LoanDto>.SuccessResult(loanDto, "Loan approved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoanDto>.ErrorResult($"Failed to approve loan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoanDto>> RejectLoanAsync(string loanId, string adminId, string reason, string? notes)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan == null)
                {
                    return ApiResponse<LoanDto>.ErrorResult("Loan not found");
                }

                loan.Status = "REJECTED";

                await _context.SaveChangesAsync();

                var loanDto = new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo
                };

                return ApiResponse<LoanDto>.SuccessResult(loanDto, "Loan rejected successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoanDto>.ErrorResult($"Failed to reject loan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> DisburseLoanAsync(string loanId, string adminId, string disbursementMethod, string? reference)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan == null)
                {
                    return ApiResponse<object>.ErrorResult("Loan not found");
                }

                if (loan.Status != "APPROVED")
                {
                    return ApiResponse<object>.ErrorResult("Loan must be approved before disbursement");
                }

                loan.Status = "ACTIVE";
                loan.DisbursedAt = DateTime.UtcNow;

                // Create disbursement transaction as Payment record
                var disbursementPayment = new Entities.Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    LoanId = loanId,
                    UserId = loan.UserId,
                    Amount = loan.Principal,
                    Method = disbursementMethod,
                    Reference = reference,
                    Status = "COMPLETED",
                    IsBankTransaction = false, // This is a loan activity, not a bank transaction
                    TransactionType = "DISBURSEMENT",
                    Description = $"Loan disbursement via {disbursementMethod}",
                    ProcessedAt = DateTime.UtcNow,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(disbursementPayment);
                await _context.SaveChangesAsync();

                var result = new
                {
                    loanId = loan.Id,
                    disbursedAmount = loan.Principal,
                    disbursedAt = loan.DisbursedAt,
                    disbursementMethod,
                    reference
                };

                return ApiResponse<object>.SuccessResult(result, "Loan disbursed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult($"Failed to disburse loan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoanDto>> CloseLoanAsync(string loanId, string adminId, string? notes)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan == null)
                {
                    return ApiResponse<LoanDto>.ErrorResult("Loan not found");
                }

                loan.Status = "COMPLETED";
                loan.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var loanDto = new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo
                };

                return ApiResponse<LoanDto>.SuccessResult(loanDto, "Loan closed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoanDto>.ErrorResult($"Failed to close loan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteLoanAsync(string loanId, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<bool>.ErrorResult("Loan not found");
                }

                // Check if loan can be deleted based on status
                if (loan.Status == "ACTIVE" || loan.Status == "COMPLETED")
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete active or completed loans");
                }

                // Payment check removed - deletion will automatically clean up all related payments
                // This allows deletion of PENDING, CANCELLED, and REJECTED loans with their payment history

                // Delete related data first (due to foreign key constraints)
                var repaymentSchedules = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .ToListAsync();
                _context.RepaymentSchedules.RemoveRange(repaymentSchedules);

                // Remove all loan activities for this loan (Payment records with IsBankTransaction = false)
                var loanActivities = await _context.Payments
                    .Where(p => p.LoanId == loanId && !p.IsBankTransaction)
                    .ToListAsync();
                _context.Payments.RemoveRange(loanActivities);

                // Delete the loan
                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Loan deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete loan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaymentDto>> MakeLoanPaymentAsync(string loanId, CreatePaymentDto payment, string userId)
        {
            try
            {
                // Verify loan exists and belongs to user
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
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
                    .FirstOrDefaultAsync(p => p.LoanId == loanId && p.Reference == payment.Reference);

                if (existingPayment != null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Payment reference already exists for this loan");
                }

                // Validate payment amount
                if (payment.Amount <= 0)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Payment amount must be greater than 0");
                }

                if (payment.Amount > loan.RemainingBalance)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Payment amount cannot exceed remaining loan balance");
                }

                // Create payment (combining both payment and loan activity tracking)
                var newPayment = new Entities.Payment
                {
                    LoanId = loanId,
                    UserId = userId,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Reference = payment.Reference,
                    Status = "COMPLETED",
                    IsBankTransaction = false, // This is a loan payment, not a bank transaction
                    TransactionType = "PAYMENT",
                    Description = $"Payment via {payment.Method}",
                    ProcessedAt = DateTime.UtcNow,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(newPayment);

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

        public async Task<ApiResponse<decimal>> GetTotalOutstandingLoanAmountAsync(string userId)
        {
            try
            {
                // Get all active loans for the user
                var activeLoans = await _context.Loans
                    .Where(l => l.UserId == userId && l.Status == "ACTIVE")
                    .ToListAsync();

                // Calculate total outstanding amount from remaining balances
                var totalOutstanding = activeLoans.Sum(l => l.RemainingBalance);

                return ApiResponse<decimal>.SuccessResult(totalOutstanding);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total outstanding loan amount: {ex.Message}");
            }
        }

        private decimal CalculateInterestRate(decimal principal, int term, decimal monthlyIncome)
        {
            // Simple interest rate calculation based on principal amount and income
            var baseRate = 12.0m; // 12% base rate
            var principalFactor = principal > 50000 ? 0.5m : 0m; // Lower rate for higher amounts
            var incomeFactor = monthlyIncome > 10000 ? -1.0m : 0m; // Lower rate for higher income
            
            return Math.Max(5.0m, baseRate + principalFactor + incomeFactor);
        }

        private decimal CalculateMonthlyPayment(decimal principal, decimal interestRate, int term)
        {
            if (interestRate == 0)
            {
                // For 0% interest loans, simply divide principal by term
                return principal / term;
            }

            var monthlyRate = interestRate / 100 / 12;
            var power = (decimal)Math.Pow((double)(1 + monthlyRate), term);
            return principal * (monthlyRate * power) / (power - 1);
        }

        private async Task CreateRepaymentScheduleAsync(string loanId, decimal monthlyPayment, int term, decimal interestRate)
        {
            var schedules = new List<Entities.RepaymentSchedule>();
            
            if (interestRate == 0)
            {
                // For 0% interest loans, all payments go to principal
                for (int i = 1; i <= term; i++)
                {
                    schedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = i,
                        DueDate = DateTime.UtcNow.AddMonths(i),
                        PrincipalAmount = monthlyPayment,
                        InterestAmount = 0,
                        TotalAmount = monthlyPayment,
                        Status = "PENDING"
                    });
                }
            }
            else
            {
                // For loans with interest, calculate principal and interest portions
                var monthlyRate = interestRate / 100 / 12;
                var remainingPrincipal = monthlyPayment * term;

                for (int i = 1; i <= term; i++)
                {
                    var interestAmount = remainingPrincipal * monthlyRate;
                    var principalAmount = monthlyPayment - interestAmount;
                    remainingPrincipal -= principalAmount;

                    schedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = i,
                        DueDate = DateTime.UtcNow.AddMonths(i),
                        PrincipalAmount = principalAmount,
                        InterestAmount = interestAmount,
                        TotalAmount = monthlyPayment,
                        Status = "PENDING"
                    });
                }
            }

            _context.RepaymentSchedules.AddRange(schedules);
            await _context.SaveChangesAsync();
        }

        /// Updates NextDueDate and FinalDueDate for a loan based on its repayment schedule - TEMPORARILY DISABLED
        /// 
        private async Task UpdateLoanDueDatesAsync(Loan loan)
        {
            // TEMPORARILY DISABLED - causing issues
            return;
            /*
            if (loan.Status == "COMPLETED" || loan.Status == "CANCELLED" || loan.Status == "REJECTED")
            {
                // Completed/cancelled/rejected loans don't have due dates
                loan.NextDueDate = null;
                loan.FinalDueDate = null;
                return;
            }

            var repaymentSchedules = await _context.RepaymentSchedules
                .Where(rs => rs.LoanId == loan.Id)
                .OrderBy(rs => rs.InstallmentNumber)
                .ToListAsync();

            if (repaymentSchedules.Any())
            {
                // Get the next unpaid installment
                var nextUnpaid = repaymentSchedules.FirstOrDefault(rs => rs.Status == "PENDING");
                loan.NextDueDate = nextUnpaid?.DueDate;

                // Get the final installment's due date
                var lastInstallment = repaymentSchedules.Last();
                loan.FinalDueDate = lastInstallment.DueDate;
            }
            else if (loan.DisbursedAt.HasValue)
            {
                // If no repayment schedule exists but loan is disbursed, calculate estimated dates
                loan.NextDueDate = loan.DisbursedAt.Value.AddMonths(1);
                loan.FinalDueDate = loan.DisbursedAt.Value.AddMonths(loan.Term);
            }
            else
            {
                // Loan not yet disbursed
                loan.NextDueDate = null;
                loan.FinalDueDate = null;
            }
            */
        }
    }
}

