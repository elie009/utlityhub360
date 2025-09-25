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

                // Create loan from application
                var interestRate = CalculateInterestRate(application.Principal, application.Term, application.MonthlyIncome);
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
                    AdditionalInfo = loan.AdditionalInfo
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

                var transactions = await _context.Transactions
                    .Where(t => t.LoanId == loanId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                var transactionDtos = transactions.Select(transaction => new TransactionDto
                {
                    Id = transaction.Id,
                    LoanId = transaction.LoanId,
                    Type = transaction.Type,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    Reference = transaction.Reference,
                    CreatedAt = transaction.CreatedAt
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

                // Create disbursement transaction
                var transaction = new Entities.Transaction
                {
                    LoanId = loanId,
                    Type = "DISBURSEMENT",
                    Amount = loan.Principal,
                    Description = $"Loan disbursement via {disbursementMethod}",
                    Reference = reference,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);
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

                // Check if there are any payments made
                var hasPayments = await _context.Payments.AnyAsync(p => p.LoanId == loanId);
                if (hasPayments)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete loan with existing payments");
                }

                // Delete related data first (due to foreign key constraints)
                var repaymentSchedules = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .ToListAsync();
                _context.RepaymentSchedules.RemoveRange(repaymentSchedules);

                var transactions = await _context.Transactions
                    .Where(t => t.LoanId == loanId)
                    .ToListAsync();
                _context.Transactions.RemoveRange(transactions);

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
            var monthlyRate = interestRate / 100 / 12;
            var power = (decimal)Math.Pow((double)(1 + monthlyRate), term);
            return principal * (monthlyRate * power) / (power - 1);
        }

        private async Task CreateRepaymentScheduleAsync(string loanId, decimal monthlyPayment, int term, decimal interestRate)
        {
            var schedules = new List<Entities.RepaymentSchedule>();
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

            _context.RepaymentSchedules.AddRange(schedules);
            await _context.SaveChangesAsync();
        }
    }
}

