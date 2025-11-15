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
        private readonly LoanAccountingService _loanAccountingService;
        private readonly AccountingService _accountingService;

        public LoanService(ApplicationDbContext context, LoanAccountingService loanAccountingService, AccountingService accountingService)
        {
            _context = context;
            _loanAccountingService = loanAccountingService;
            _accountingService = accountingService;
        }

        public async Task<Loan?> GetLoanWithAccessCheckAsync(string loanId, string userId)
        {
            Console.WriteLine($"[ACCESS DEBUG] Checking access for loan: {loanId}, user: {userId}");
            
            // First, check if the user is admin
            var user = await _context.Users.FindAsync(userId);
            var isAdmin = user?.Role == "ADMIN";
            
            Console.WriteLine($"[ACCESS DEBUG] User role: {user?.Role}, IsAdmin: {isAdmin}");

            if (isAdmin)
            {
                // Admin can access any loan
                var loan = await _context.Loans.FindAsync(loanId);
                Console.WriteLine($"[ACCESS DEBUG] Admin access - Loan found: {loan != null}");
                return loan;
            }
            else
            {
                // Regular users can only access their own loans
                var loan = await _context.Loans
                    .FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);
                Console.WriteLine($"[ACCESS DEBUG] User access - Loan found: {loan != null}");
                if (loan != null)
                {
                    Console.WriteLine($"[ACCESS DEBUG] Loan owner: {loan.UserId}");
                }
                return loan;
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
                var interestMethod = "AMORTIZED"; // Default to amortized, can be made configurable
                var actualFinancedAmount = application.Principal; // Will be adjusted if down payment is provided
                var monthlyPayment = CalculateMonthlyPayment(actualFinancedAmount, interestRate, application.Term, interestMethod);
                var totalAmount = monthlyPayment * application.Term;
                
                // Calculate total interest based on method
                decimal totalInterest = 0;
                if (interestMethod == "FLAT_RATE")
                {
                    totalInterest = CalculateFlatRateInterest(actualFinancedAmount, interestRate, application.Term);
                }
                else
                {
                    totalInterest = CalculateAmortizedInterest(actualFinancedAmount, interestRate, application.Term);
                }

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
                    // TEMPORARY: Commented out until database migration is applied
                    // InterestComputationMethod = interestMethod,
                    // TotalInterest = totalInterest,
                    // ActualFinancedAmount = actualFinancedAmount,
                    // PaymentFrequency = "MONTHLY",
                    // StartDate = null // Will be set when disbursed
                };

                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();

                // Create repayment schedule
                await CreateRepaymentScheduleAsync(loan.Id, monthlyPayment, application.Term, interestRate, null, interestMethod, actualFinancedAmount);

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

        public async Task<ApiResponse<object>> DisburseLoanAsync(string loanId, string adminId, string disbursementMethod, string? reference, string? bankAccountId = null)
        {
            // Use database transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
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
                // TEMPORARY: Commented out until database migration is applied
                // loan.StartDate = DateTime.UtcNow;

                // TEMPORARY: Commented out until database migration is applied
                // Handle processing fee if applicable (record before disbursement)
                // if (loan.ProcessingFee > 0)
                // {
                //     await _accountingService.CreateProcessingFeeEntryAsync(
                //         loanId,
                //         loan.UserId,
                //         loan.ProcessingFee,
                //         reference != null ? $"{reference}-FEE" : null);
                // }

                // TEMPORARY: Commented out until database migration is applied
                // Handle down payment if applicable (reduces principal before disbursement)
                // if (loan.DownPayment > 0)
                // {
                //     await _accountingService.CreateDownPaymentEntryAsync(
                //         loanId,
                //         loan.UserId,
                //         loan.DownPayment,
                //         reference != null ? $"{reference}-DOWN" : null);
                //     
                //     // Adjust remaining balance for down payment
                //     loan.RemainingBalance -= loan.DownPayment;
                // }

                // Create journal entry for loan disbursement (Debit Bank Account, Credit Loan Payable)
                var disbursementAmount = loan.Principal; // loan.ActualFinancedAmount > 0 ? loan.ActualFinancedAmount : loan.Principal;

                // Credit loan amount to bank account if BankAccountId is provided
                if (!string.IsNullOrEmpty(bankAccountId))
                {
                    var bankAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == loan.UserId);

                    if (bankAccount == null)
                    {
                        return ApiResponse<object>.ErrorResult("Bank account not found or does not belong to the loan user");
                    }

                    if (!bankAccount.IsActive)
                    {
                        return ApiResponse<object>.ErrorResult("Bank account is not active");
                    }

                    // Create bank transaction (CREDIT) - Loan disbursement credited to account
                    var bankTransaction = new Entities.BankTransaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = bankAccountId,
                        UserId = loan.UserId,
                        Amount = disbursementAmount,
                        TransactionType = "CREDIT",
                        Description = $"Loan disbursement - {loan.Purpose}",
                        Category = "LOAN",
                        ReferenceNumber = reference ?? $"LOAN-DISB-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        ExternalTransactionId = $"LOAN_{loanId}",
                        Notes = $"Loan disbursement for loan {loanId}",
                        Currency = bankAccount.Currency,
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Update bank account balance (CREDIT = add money)
                    bankAccount.CurrentBalance += disbursementAmount;
                    bankTransaction.BalanceAfterTransaction = bankAccount.CurrentBalance;
                    bankAccount.UpdatedAt = DateTime.UtcNow;

                    // Create Payment record for bank transaction
                    var bankPayment = new Entities.Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = bankAccountId,
                        LoanId = loanId,
                        UserId = loan.UserId,
                        Amount = disbursementAmount,
                        Method = "BANK_TRANSFER",
                        Reference = bankTransaction.ReferenceNumber,
                        Status = "COMPLETED",
                        IsBankTransaction = true,
                        TransactionType = "CREDIT",
                        Description = $"Loan disbursement credited to {bankAccount.AccountName}",
                        Category = "LOAN",
                        Currency = bankAccount.Currency,
                        BalanceAfterTransaction = bankAccount.CurrentBalance,
                        ProcessedAt = DateTime.UtcNow,
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.BankTransactions.Add(bankTransaction);
                    _context.Payments.Add(bankPayment);

                    // Create Journal Entry for loan disbursement (Debit Bank Account, Credit Loan Payable)
                    await _accountingService.CreateLoanDisbursementEntryAsync(
                        loanId,
                        loan.UserId,
                        disbursementAmount,
                        bankAccount.AccountName,
                        reference ?? $"DISB-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        DateTime.UtcNow
                    );
                }

                // Create disbursement transaction as Payment record (for loan transaction history)
                var disbursementPayment = new Entities.Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    LoanId = loanId,
                    UserId = loan.UserId,
                    Amount = disbursementAmount,
                    Method = disbursementMethod,
                    Reference = reference ?? $"DISB-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Status = "COMPLETED",
                    IsBankTransaction = false, // This is a loan activity, not a bank transaction
                    TransactionType = "DISBURSEMENT",
                    Description = $"Loan disbursement via {disbursementMethod}" + 
                                 (!string.IsNullOrEmpty(bankAccountId) ? $" (credited to bank account)" : ""),
                    ProcessedAt = DateTime.UtcNow,
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(disbursementPayment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new
                {
                    loanId = loan.Id,
                    disbursedAmount = loan.Principal,
                    disbursedAt = loan.DisbursedAt,
                    disbursementMethod,
                    reference,
                    bankAccountCredited = !string.IsNullOrEmpty(bankAccountId),
                    bankAccountId = bankAccountId,
                    message = !string.IsNullOrEmpty(bankAccountId) 
                        ? $"Loan disbursed and credited to bank account successfully" 
                        : $"Loan disbursed successfully (no bank account credited)"
                };

                return ApiResponse<object>.SuccessResult(result, "Loan disbursed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                Console.WriteLine($"[DELETE DEBUG] Attempting to delete loan: {loanId}");
                Console.WriteLine($"[DELETE DEBUG] User ID: {userId}");
                
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    Console.WriteLine($"[DELETE DEBUG] Loan not found or access denied");
                    return ApiResponse<bool>.ErrorResult("Loan not found");
                }

                Console.WriteLine($"[DELETE DEBUG] Loan found - Status: {loan.Status}, Owner: {loan.UserId}");
                
                // Check if loan can be deleted based on status
                if (loan.Status == "ACTIVE" || loan.Status == "COMPLETED")
                {
                    Console.WriteLine($"[DELETE DEBUG] Cannot delete - Status: {loan.Status}");
                    return ApiResponse<bool>.ErrorResult("Cannot delete active or completed loans");
                }

                Console.WriteLine($"[DELETE DEBUG] Proceeding with deletion...");

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
                Console.WriteLine($"[DELETE DEBUG] Exception: {ex.Message}");
                Console.WriteLine($"[DELETE DEBUG] Stack trace: {ex.StackTrace}");
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

                // Calculate principal and interest portions based on repayment schedule
                // Find the next unpaid installment to determine how payment should be allocated
                var nextUnpaidInstallment = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId && rs.Status == "PENDING")
                    .OrderBy(rs => rs.InstallmentNumber)
                    .FirstOrDefaultAsync();

                decimal principalAmount = 0;
                decimal interestAmount = 0;

                if (nextUnpaidInstallment != null)
                {
                    // If payment covers the full installment
                    if (payment.Amount >= nextUnpaidInstallment.TotalAmount)
                    {
                        principalAmount = nextUnpaidInstallment.PrincipalAmount;
                        interestAmount = nextUnpaidInstallment.InterestAmount;
                        
                        // Mark installment as paid
                        nextUnpaidInstallment.Status = "PAID";
                        nextUnpaidInstallment.PaidAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Partial payment - allocate proportionally
                        var ratio = payment.Amount / nextUnpaidInstallment.TotalAmount;
                        principalAmount = nextUnpaidInstallment.PrincipalAmount * ratio;
                        interestAmount = nextUnpaidInstallment.InterestAmount * ratio;
                    }
                }
                else
                {
                    // No schedule found or all paid - treat entire payment as principal
                    principalAmount = payment.Amount;
                    interestAmount = 0;
                }

                // TEMPORARY: Commented out until database migration is applied
                // Create journal entry for loan payment (Debit Loan Payable, Debit Interest Expense, Credit Cash)
                // await _accountingService.CreateLoanPaymentEntryAsync(
                //     loanId,
                //     userId,
                //     principalAmount,
                //     interestAmount,
                //     payment.Amount,
                //     payment.Reference,
                //     $"Loan payment via {payment.Method}");

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
                    Description = $"Payment via {payment.Method} - Principal: {principalAmount:C}, Interest: {interestAmount:C}",
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

        /// <summary>
        /// Calculates monthly payment based on interest computation method
        /// </summary>
        private decimal CalculateMonthlyPayment(decimal principal, decimal interestRate, int term, string method = "AMORTIZED")
        {
            if (interestRate == 0)
            {
                // For 0% interest loans, simply divide principal by term
                return principal / term;
            }

            if (method == "FLAT_RATE")
            {
                // Flat Rate Method: Interest = P × R × T
                // Total Payable = Principal + Interest
                // Monthly Payment = Total Payable / Term
                var annualInterest = principal * (interestRate / 100m) * (term / 12m);
                var totalPayable = principal + annualInterest;
                return totalPayable / term;
            }
            else
            {
                // Amortized Method (Reducing Balance): M = P × [r(1+r)^n] / [(1+r)^n - 1]
                var monthlyRate = interestRate / 100m / 12m;
                var power = (decimal)Math.Pow((double)(1 + monthlyRate), term);
                return principal * (monthlyRate * power) / (power - 1);
            }
        }

        /// <summary>
        /// Calculates total interest for flat rate method
        /// </summary>
        private decimal CalculateFlatRateInterest(decimal principal, decimal interestRate, int term)
        {
            // Interest = P × R × T (where T is in years)
            var timeInYears = term / 12m;
            return principal * (interestRate / 100m) * timeInYears;
        }

        /// <summary>
        /// Calculates total interest for amortized method
        /// </summary>
        private decimal CalculateAmortizedInterest(decimal principal, decimal interestRate, int term)
        {
            var monthlyPayment = CalculateMonthlyPayment(principal, interestRate, term, "AMORTIZED");
            var totalPayable = monthlyPayment * term;
            return totalPayable - principal;
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

        #region Payment Schedule Management

        public async Task<ApiResponse<PaymentScheduleResponseDto>> ExtendLoanTermAsync(string loanId, ExtendLoanTermDto extendDto, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Loan not found");
                }

                if (loan.Status != "ACTIVE" && loan.Status != "APPROVED")
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Can only extend active or approved loans");
                }

                // Get current payment schedule
                var currentSchedule = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .OrderBy(rs => rs.InstallmentNumber)
                    .ToListAsync();

                if (!currentSchedule.Any())
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("No existing payment schedule found");
                }

                // Get the last installment to determine starting point
                var lastInstallment = currentSchedule.Last();
                var startingInstallmentNumber = lastInstallment.InstallmentNumber + 1;
                var firstNewDueDate = lastInstallment.DueDate.AddMonths(1);

                // Use existing monthly payment amount
                var monthlyPayment = loan.MonthlyPayment;

                // Create new installments for the extension
                var newSchedules = new List<Entities.RepaymentSchedule>();
                var monthlyRate = loan.InterestRate / 100 / 12;
                
                for (int i = 0; i < extendDto.AdditionalMonths; i++)
                {
                    var installmentNumber = startingInstallmentNumber + i;
                    var dueDate = firstNewDueDate.AddMonths(i);

                    var interestAmount = loan.InterestRate > 0 ? loan.RemainingBalance * monthlyRate : 0;
                    var principalAmount = monthlyPayment - interestAmount;

                    newSchedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = installmentNumber,
                        DueDate = dueDate,
                        PrincipalAmount = Math.Max(0, principalAmount),
                        InterestAmount = interestAmount,
                        TotalAmount = monthlyPayment,
                        Status = "PENDING"
                    });
                }

                // Update loan term
                loan.Term += extendDto.AdditionalMonths;
                loan.TotalAmount += monthlyPayment * extendDto.AdditionalMonths;

                // Add new schedules to database
                _context.RepaymentSchedules.AddRange(newSchedules);
                await _context.SaveChangesAsync();

                // Return updated schedule
                var updatedSchedule = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .OrderBy(rs => rs.InstallmentNumber)
                    .ToListAsync();

                var scheduleDtos = updatedSchedule.Select(rs => new RepaymentScheduleDto
                {
                    Id = rs.Id,
                    LoanId = rs.LoanId,
                    InstallmentNumber = rs.InstallmentNumber,
                    DueDate = rs.DueDate,
                    PrincipalAmount = rs.PrincipalAmount,
                    InterestAmount = rs.InterestAmount,
                    TotalAmount = rs.TotalAmount,
                    Status = rs.Status,
                    PaidAt = rs.PaidAt
                }).ToList();

                var response = new PaymentScheduleResponseDto
                {
                    Schedule = scheduleDtos,
                    TotalInstallments = scheduleDtos.Count,
                    TotalAmount = scheduleDtos.Sum(s => s.TotalAmount),
                    FirstDueDate = scheduleDtos.FirstOrDefault()?.DueDate,
                    LastDueDate = scheduleDtos.LastOrDefault()?.DueDate,
                    Message = $"Loan term extended by {extendDto.AdditionalMonths} months. {extendDto.AdditionalMonths} new installments added."
                };

                return ApiResponse<PaymentScheduleResponseDto>.SuccessResult(response, "Loan term extended successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentScheduleResponseDto>.ErrorResult($"Failed to extend loan term: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaymentScheduleResponseDto>> AddPaymentScheduleAsync(string loanId, AddPaymentScheduleDto addDto, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Loan not found");
                }

                if (loan.Status != "ACTIVE" && loan.Status != "APPROVED")
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Can only add schedules to active or approved loans");
                }

                // Check if starting installment number conflicts with existing schedules
                var existingSchedules = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId && 
                                rs.InstallmentNumber >= addDto.StartingInstallmentNumber && 
                                rs.InstallmentNumber < addDto.StartingInstallmentNumber + addDto.NumberOfMonths)
                    .ToListAsync();

                if (existingSchedules.Any())
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult(
                        $"Installment numbers {addDto.StartingInstallmentNumber} to {addDto.StartingInstallmentNumber + addDto.NumberOfMonths - 1} already exist");
                }

                // Create new payment schedules
                var newSchedules = new List<Entities.RepaymentSchedule>();
                var monthlyRate = loan.InterestRate / 100 / 12;

                for (int i = 0; i < addDto.NumberOfMonths; i++)
                {
                    var installmentNumber = addDto.StartingInstallmentNumber + i;
                    var dueDate = addDto.FirstDueDate.AddMonths(i);
                    
                    var interestAmount = loan.InterestRate > 0 ? loan.RemainingBalance * monthlyRate : 0;
                    var principalAmount = addDto.MonthlyPayment - interestAmount;

                    newSchedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = installmentNumber,
                        DueDate = dueDate,
                        PrincipalAmount = Math.Max(0, principalAmount),
                        InterestAmount = interestAmount,
                        TotalAmount = addDto.MonthlyPayment,
                        Status = "PENDING"
                    });
                }

                // Update loan totals
                loan.TotalAmount += addDto.MonthlyPayment * addDto.NumberOfMonths;

                _context.RepaymentSchedules.AddRange(newSchedules);
                await _context.SaveChangesAsync();

                // Return new schedules
                var scheduleDtos = newSchedules.Select(rs => new RepaymentScheduleDto
                {
                    Id = rs.Id,
                    LoanId = rs.LoanId,
                    InstallmentNumber = rs.InstallmentNumber,
                    DueDate = rs.DueDate,
                    PrincipalAmount = rs.PrincipalAmount,
                    InterestAmount = rs.InterestAmount,
                    TotalAmount = rs.TotalAmount,
                    Status = rs.Status,
                    PaidAt = rs.PaidAt
                }).ToList();

                var response = new PaymentScheduleResponseDto
                {
                    Schedule = scheduleDtos,
                    TotalInstallments = scheduleDtos.Count,
                    TotalAmount = scheduleDtos.Sum(s => s.TotalAmount),
                    FirstDueDate = scheduleDtos.FirstOrDefault()?.DueDate,
                    LastDueDate = scheduleDtos.LastOrDefault()?.DueDate,
                    Message = $"{addDto.NumberOfMonths} new payment installments added starting from installment #{addDto.StartingInstallmentNumber}"
                };

                return ApiResponse<PaymentScheduleResponseDto>.SuccessResult(response, "Payment schedules added successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentScheduleResponseDto>.ErrorResult($"Failed to add payment schedules: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaymentScheduleResponseDto>> AutoAddPaymentScheduleAsync(string loanId, AutoAddPaymentScheduleDto addDto, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Loan not found");
                }

                if (loan.Status != "ACTIVE" && loan.Status != "APPROVED")
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Can only add schedules to active or approved loans");
                }

                // Automatically find the next available installment number
                var maxInstallmentNumber = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .MaxAsync(rs => (int?)rs.InstallmentNumber) ?? 0;

                var startingInstallmentNumber = maxInstallmentNumber + 1;

                // Create new payment schedules
                var newSchedules = new List<Entities.RepaymentSchedule>();
                var monthlyRate = loan.InterestRate / 100 / 12;

                for (int i = 0; i < addDto.NumberOfMonths; i++)
                {
                    var installmentNumber = startingInstallmentNumber + i;
                    var dueDate = addDto.FirstDueDate.AddMonths(i);
                    
                    var interestAmount = loan.InterestRate > 0 ? loan.RemainingBalance * monthlyRate : 0;
                    var principalAmount = addDto.MonthlyPayment - interestAmount;

                    newSchedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = installmentNumber,
                        DueDate = dueDate,
                        PrincipalAmount = Math.Max(0, principalAmount),
                        InterestAmount = interestAmount,
                        TotalAmount = addDto.MonthlyPayment,
                        Status = "PENDING"
                    });
                }

                // Update loan totals
                loan.Term += addDto.NumberOfMonths;
                loan.TotalAmount += addDto.MonthlyPayment * addDto.NumberOfMonths;

                _context.RepaymentSchedules.AddRange(newSchedules);
                await _context.SaveChangesAsync();

                // Return new schedules
                var scheduleDtos = newSchedules.Select(rs => new RepaymentScheduleDto
                {
                    Id = rs.Id,
                    LoanId = rs.LoanId,
                    InstallmentNumber = rs.InstallmentNumber,
                    DueDate = rs.DueDate,
                    PrincipalAmount = rs.PrincipalAmount,
                    InterestAmount = rs.InterestAmount,
                    TotalAmount = rs.TotalAmount,
                    Status = rs.Status,
                    PaidAt = rs.PaidAt
                }).ToList();

                var response = new PaymentScheduleResponseDto
                {
                    Schedule = scheduleDtos,
                    TotalInstallments = scheduleDtos.Count,
                    TotalAmount = scheduleDtos.Sum(s => s.TotalAmount),
                    FirstDueDate = scheduleDtos.FirstOrDefault()?.DueDate,
                    LastDueDate = scheduleDtos.LastOrDefault()?.DueDate,
                    Message = $"{addDto.NumberOfMonths} new payment installment(s) added automatically starting from installment #{startingInstallmentNumber}"
                };

                return ApiResponse<PaymentScheduleResponseDto>.SuccessResult(response, "Payment schedules added successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentScheduleResponseDto>.ErrorResult($"Failed to add payment schedules: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaymentScheduleResponseDto>> RegeneratePaymentScheduleAsync(string loanId, RegenerateScheduleDto regenerateDto, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Loan not found");
                }

                if (loan.Status != "ACTIVE" && loan.Status != "APPROVED")
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Can only regenerate schedules for active or approved loans");
                }

                // Check for existing paid installments
                var paidInstallments = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId && rs.Status == "PAID")
                    .ToListAsync();

                if (paidInstallments.Any())
                {
                    return ApiResponse<PaymentScheduleResponseDto>.ErrorResult("Cannot regenerate schedule with existing paid installments. Consider extending the term instead.");
                }

                // Delete existing pending schedules
                var existingSchedules = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId && rs.Status != "PAID")
                    .ToListAsync();

                _context.RepaymentSchedules.RemoveRange(existingSchedules);

                // Update loan parameters
                loan.MonthlyPayment = regenerateDto.NewMonthlyPayment;
                loan.Term = regenerateDto.NewTerm;
                loan.TotalAmount = regenerateDto.NewMonthlyPayment * regenerateDto.NewTerm;

                // Generate new payment schedule
                var startDate = regenerateDto.StartDate ?? DateTime.UtcNow;
                await CreateRepaymentScheduleAsync(loanId, regenerateDto.NewMonthlyPayment, regenerateDto.NewTerm, loan.InterestRate, startDate, loan.InterestComputationMethod, loan.ActualFinancedAmount);

                await _context.SaveChangesAsync();

                // Return new schedule
                var newSchedule = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .OrderBy(rs => rs.InstallmentNumber)
                    .ToListAsync();

                var scheduleDtos = newSchedule.Select(rs => new RepaymentScheduleDto
                {
                    Id = rs.Id,
                    LoanId = rs.LoanId,
                    InstallmentNumber = rs.InstallmentNumber,
                    DueDate = rs.DueDate,
                    PrincipalAmount = rs.PrincipalAmount,
                    InterestAmount = rs.InterestAmount,
                    TotalAmount = rs.TotalAmount,
                    Status = rs.Status,
                    PaidAt = rs.PaidAt
                }).ToList();

                var response = new PaymentScheduleResponseDto
                {
                    Schedule = scheduleDtos,
                    TotalInstallments = scheduleDtos.Count,
                    TotalAmount = scheduleDtos.Sum(s => s.TotalAmount),
                    FirstDueDate = scheduleDtos.FirstOrDefault()?.DueDate,
                    LastDueDate = scheduleDtos.LastOrDefault()?.DueDate,
                    Message = $"Payment schedule regenerated with {regenerateDto.NewTerm} installments of ${regenerateDto.NewMonthlyPayment:N2} each"
                };

                return ApiResponse<PaymentScheduleResponseDto>.SuccessResult(response, "Payment schedule regenerated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentScheduleResponseDto>.ErrorResult($"Failed to regenerate payment schedule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeletePaymentScheduleInstallmentAsync(string loanId, int installmentNumber, string userId)
        {
            try
            {
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<bool>.ErrorResult("Loan not found");
                }

                var installment = await _context.RepaymentSchedules
                    .FirstOrDefaultAsync(rs => rs.LoanId == loanId && rs.InstallmentNumber == installmentNumber);

                if (installment == null)
                {
                    return ApiResponse<bool>.ErrorResult("Payment installment not found");
                }

                if (installment.Status == "PAID")
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete a paid installment");
                }

                // Update loan totals
                loan.TotalAmount -= installment.TotalAmount;
                loan.Term--;

                _context.RepaymentSchedules.Remove(installment);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Payment installment deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete payment installment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RepaymentScheduleDto>> MarkInstallmentAsPaidAsync(
            string loanId, 
            int installmentNumber, 
            MarkInstallmentPaidDto paymentDto, 
            string userId)
        {
            try
            {
                // Verify loan access
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<RepaymentScheduleDto>.ErrorResult("Loan not found");
                }

                // Get the specific installment
                var installment = await _context.RepaymentSchedules
                    .FirstOrDefaultAsync(rs => rs.LoanId == loanId && rs.InstallmentNumber == installmentNumber);

                if (installment == null)
                {
                    return ApiResponse<RepaymentScheduleDto>.ErrorResult("Installment not found");
                }

                if (installment.Status == "PAID")
                {
                    return ApiResponse<RepaymentScheduleDto>.ErrorResult("Installment is already paid");
                }

                // Validate payment amount (allow partial payments)
                if (paymentDto.Amount > installment.TotalAmount)
                {
                    return ApiResponse<RepaymentScheduleDto>.ErrorResult(
                        $"Payment amount ({paymentDto.Amount:C}) cannot exceed installment amount ({installment.TotalAmount:C})");
                }

                // Mark installment as paid (full payment) or keep pending (partial payment)
                if (paymentDto.Amount == installment.TotalAmount)
                {
                    installment.Status = "PAID";
                    installment.PaidAt = paymentDto.PaymentDate ?? DateTime.UtcNow;
                }

                // Create payment record
                var reference = paymentDto.Reference ?? $"INST-{installmentNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";
                var payment = new Entities.Payment
                {
                    LoanId = loanId,
                    UserId = userId,
                    Amount = paymentDto.Amount,
                    Method = paymentDto.Method,
                    Reference = reference,
                    Status = "COMPLETED",
                    IsBankTransaction = false,
                    TransactionType = "INSTALLMENT_PAYMENT",
                    Description = $"Payment for installment #{installmentNumber} via {paymentDto.Method}" + 
                                 (string.IsNullOrEmpty(paymentDto.Notes) ? "" : $" - {paymentDto.Notes}"),
                    ProcessedAt = DateTime.UtcNow,
                    TransactionDate = paymentDto.PaymentDate ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);

                // Update loan balance
                loan.RemainingBalance -= paymentDto.Amount;

                // Ensure remaining balance doesn't go negative
                if (loan.RemainingBalance < 0)
                {
                    loan.RemainingBalance = 0;
                }

                // Check if loan is completed
                var remainingInstallments = await _context.RepaymentSchedules
                    .CountAsync(rs => rs.LoanId == loanId && rs.Status == "PENDING");

                if (remainingInstallments == 0 || loan.RemainingBalance <= 0)
                {
                    loan.Status = "COMPLETED";
                    loan.CompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Return updated installment
                var scheduleDto = new RepaymentScheduleDto
                {
                    Id = installment.Id,
                    LoanId = installment.LoanId,
                    InstallmentNumber = installment.InstallmentNumber,
                    DueDate = installment.DueDate,
                    PrincipalAmount = installment.PrincipalAmount,
                    InterestAmount = installment.InterestAmount,
                    TotalAmount = installment.TotalAmount,
                    Status = installment.Status,
                    PaidAt = installment.PaidAt
                };

                return ApiResponse<RepaymentScheduleDto>.SuccessResult(scheduleDto, 
                    $"Installment #{installmentNumber} marked as {(paymentDto.Amount == installment.TotalAmount ? "fully paid" : "partially paid")} successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<RepaymentScheduleDto>.ErrorResult($"Failed to mark installment as paid: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RepaymentScheduleDto>> UpdateScheduleSimpleAsync(
            string loanId, 
            int installmentNumber, 
            SimpleScheduleUpdateDto updateDto, 
            string userId)
        {
            try
            {
                // Verify loan access
                var loan = await GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return ApiResponse<RepaymentScheduleDto>.ErrorResult("Loan not found");
                }

                // Get the installment
                var installment = await _context.RepaymentSchedules
                    .FirstOrDefaultAsync(rs => rs.LoanId == loanId && rs.InstallmentNumber == installmentNumber);

                if (installment == null)
                {
                    return ApiResponse<RepaymentScheduleDto>.ErrorResult("Payment installment not found");
                }

                // Track changes for loan balance update
                decimal amountDifference = 0;
                var oldStatus = installment.Status;
                var oldAmount = installment.TotalAmount;

                // Update Amount if provided
                if (updateDto.Amount.HasValue && updateDto.Amount.Value != installment.TotalAmount)
                {
                    amountDifference = updateDto.Amount.Value - installment.TotalAmount;
                    
                    // Update amounts proportionally
                    var interestRatio = installment.TotalAmount > 0 ? installment.InterestAmount / installment.TotalAmount : 0;
                    installment.TotalAmount = updateDto.Amount.Value;
                    installment.InterestAmount = installment.TotalAmount * interestRatio;
                    installment.PrincipalAmount = installment.TotalAmount - installment.InterestAmount;
                }

                // Update Status if provided
                if (!string.IsNullOrEmpty(updateDto.Status))
                {
                    var newStatus = updateDto.Status.ToUpper();
                    installment.Status = newStatus;

                    // If marking as PAID
                    if (newStatus == "PAID" && oldStatus != "PAID")
                    {
                        installment.PaidAt = updateDto.PaidDate ?? DateTime.UtcNow;
                        
                        // Create payment record for audit trail
                        var paymentMethod = updateDto.PaymentMethod ?? "MANUAL_UPDATE";
                        var paymentReference = updateDto.PaymentReference ?? $"SCHED-{installmentNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";
                        
                        var payment = new Entities.Payment
                        {
                            LoanId = loanId,
                            UserId = userId,
                            Amount = installment.TotalAmount,
                            Method = paymentMethod,
                            Reference = paymentReference,
                            Status = "COMPLETED",
                            IsBankTransaction = false,
                            TransactionType = "SCHEDULE_UPDATE",
                            Description = $"Installment #{installmentNumber} marked as paid via schedule update" + 
                                         (string.IsNullOrEmpty(updateDto.Notes) ? "" : $" - {updateDto.Notes}"),
                            ProcessedAt = installment.PaidAt.Value,
                            TransactionDate = installment.PaidAt.Value,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Payments.Add(payment);

                        // Update loan balance
                        loan.RemainingBalance -= installment.TotalAmount;
                        if (loan.RemainingBalance < 0) loan.RemainingBalance = 0;
                    }
                    // If marking as PENDING from PAID
                    else if (newStatus == "PENDING" && oldStatus == "PAID")
                    {
                        installment.PaidAt = null;
                        loan.RemainingBalance += installment.TotalAmount;
                        
                        // Note: You might want to remove/reverse the payment record here
                        // depending on your business requirements
                    }
                }

                // Update Due Date if provided
                if (updateDto.DueDate.HasValue)
                {
                    installment.DueDate = updateDto.DueDate.Value;
                }

                // Update Paid Date if provided (for manual overrides)
                if (updateDto.PaidDate.HasValue && installment.Status == "PAID")
                {
                    installment.PaidAt = updateDto.PaidDate.Value;
                }

                // Update loan totals if amount changed
                if (amountDifference != 0)
                {
                    loan.TotalAmount += amountDifference;
                    
                    // Only adjust remaining balance if not paid
                    if (installment.Status != "PAID")
                    {
                        loan.RemainingBalance += amountDifference;
                        if (loan.RemainingBalance < 0) loan.RemainingBalance = 0;
                    }
                }

                // Check if loan should be completed
                var remainingInstallments = await _context.RepaymentSchedules
                    .CountAsync(rs => rs.LoanId == loanId && rs.Status == "PENDING");

                if (remainingInstallments == 0 || loan.RemainingBalance <= 0)
                {
                    loan.Status = "COMPLETED";
                    loan.CompletedAt = DateTime.UtcNow;
                }
                else if (loan.Status == "COMPLETED" && remainingInstallments > 0)
                {
                    // Reopen loan if there are pending installments
                    loan.Status = "ACTIVE";
                    loan.CompletedAt = null;
                }

                await _context.SaveChangesAsync();

                // Return updated installment
                var scheduleDto = new RepaymentScheduleDto
                {
                    Id = installment.Id,
                    LoanId = installment.LoanId,
                    InstallmentNumber = installment.InstallmentNumber,
                    DueDate = installment.DueDate,
                    PrincipalAmount = installment.PrincipalAmount,
                    InterestAmount = installment.InterestAmount,
                    TotalAmount = installment.TotalAmount,
                    Status = installment.Status,
                    PaidAt = installment.PaidAt
                };

                return ApiResponse<RepaymentScheduleDto>.SuccessResult(scheduleDto, 
                    "Payment schedule updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<RepaymentScheduleDto>.ErrorResult($"Failed to update payment schedule: {ex.Message}");
            }
        }

        // Updated CreateRepaymentScheduleAsync with optional start date parameter and interest method
        private async Task CreateRepaymentScheduleAsync(string loanId, decimal monthlyPayment, int term, decimal interestRate, DateTime? startDate = null, string interestMethod = "AMORTIZED", decimal actualFinancedAmount = 0)
        {
            var schedules = new List<Entities.RepaymentSchedule>();
            var baseDate = startDate ?? DateTime.UtcNow;
            var principal = actualFinancedAmount > 0 ? actualFinancedAmount : monthlyPayment * term; // Use actual financed amount if provided
            
            if (interestRate == 0)
            {
                // For 0% interest loans, all payments go to principal
                for (int i = 1; i <= term; i++)
                {
                    schedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = i,
                        DueDate = baseDate.AddMonths(i),
                        PrincipalAmount = monthlyPayment,
                        InterestAmount = 0,
                        TotalAmount = monthlyPayment,
                        Status = "PENDING"
                    });
                }
            }
            else if (interestMethod == "FLAT_RATE")
            {
                // Flat Rate Method: Interest is fixed for all installments
                var totalInterest = CalculateFlatRateInterest(principal, interestRate, term);
                var interestPerMonth = totalInterest / term;
                var principalPerMonth = monthlyPayment - interestPerMonth;

                for (int i = 1; i <= term; i++)
                {
                    schedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = i,
                        DueDate = baseDate.AddMonths(i),
                        PrincipalAmount = principalPerMonth,
                        InterestAmount = interestPerMonth,
                        TotalAmount = monthlyPayment,
                        Status = "PENDING"
                    });
                }
            }
            else
            {
                // Amortized Method (Reducing Balance): Interest decreases over time
                var monthlyRate = interestRate / 100m / 12m;
                var remainingBalance = principal;

                for (int i = 1; i <= term; i++)
                {
                    var interestAmount = remainingBalance * monthlyRate;
                    var principalAmount = monthlyPayment - interestAmount;
                    remainingBalance -= principalAmount;

                    // Ensure remaining balance doesn't go negative due to rounding
                    if (remainingBalance < 0)
                    {
                        principalAmount += remainingBalance; // Adjust last payment
                        remainingBalance = 0;
                    }

                    schedules.Add(new Entities.RepaymentSchedule
                    {
                        LoanId = loanId,
                        InstallmentNumber = i,
                        DueDate = baseDate.AddMonths(i),
                        PrincipalAmount = Math.Round(principalAmount, 2),
                        InterestAmount = Math.Round(interestAmount, 2),
                        TotalAmount = monthlyPayment,
                        Status = "PENDING"
                    });
                }
            }

            _context.RepaymentSchedules.AddRange(schedules);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}

