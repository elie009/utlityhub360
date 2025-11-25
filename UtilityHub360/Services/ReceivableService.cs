using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class ReceivableService : IReceivableService
    {
        private readonly ApplicationDbContext _context;
        private readonly AccountingService _accountingService;

        public ReceivableService(ApplicationDbContext context, AccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        public async Task<ApiResponse<ReceivableDto>> CreateReceivableAsync(CreateReceivableDto createReceivableDto, string userId)
        {
            try
            {
                // Verify user exists (foreign key constraint)
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    return ApiResponse<ReceivableDto>.ErrorResult($"User with ID '{userId}' does not exist in the database");
                }

                // Verify Receivables table is accessible
                try
                {
                    var testQuery = await _context.Receivables.Take(1).CountAsync();
                }
                catch (Exception tableEx)
                {
                    return ApiResponse<ReceivableDto>.ErrorResult($"Receivables table may not exist or is not accessible. Error: {tableEx.Message}. Please ensure the database table has been created.");
                }

                // Calculate total amount (Principal + Interest)
                decimal totalAmount = createReceivableDto.Principal;
                if (createReceivableDto.InterestRate > 0)
                {
                    // Simple interest calculation: Principal * (1 + InterestRate/100 * Term/12)
                    // For monthly payments, we'll use the monthly payment * term
                    totalAmount = createReceivableDto.MonthlyPayment * createReceivableDto.Term;
                }
                else
                {
                    // No interest, total is just principal
                    totalAmount = createReceivableDto.Principal;
                }

                var receivable = new Receivable
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BorrowerName = createReceivableDto.BorrowerName,
                    BorrowerContact = string.IsNullOrWhiteSpace(createReceivableDto.BorrowerContact) ? null : createReceivableDto.BorrowerContact,
                    Principal = createReceivableDto.Principal,
                    InterestRate = createReceivableDto.InterestRate,
                    Term = createReceivableDto.Term,
                    Purpose = string.IsNullOrWhiteSpace(createReceivableDto.Purpose) ? string.Empty : createReceivableDto.Purpose,
                    Status = "ACTIVE",
                    MonthlyPayment = createReceivableDto.MonthlyPayment,
                    TotalAmount = totalAmount,
                    RemainingBalance = totalAmount,
                    TotalPaid = 0,
                    LentAt = DateTime.UtcNow,
                    // StartDate and PaymentFrequency are temporarily ignored in DbContext
                    // StartDate = createReceivableDto.StartDate ?? DateTime.UtcNow,
                    // PaymentFrequency = string.IsNullOrWhiteSpace(createReceivableDto.PaymentFrequency) ? "MONTHLY" : createReceivableDto.PaymentFrequency.ToUpper(),
                    AdditionalInfo = string.IsNullOrWhiteSpace(createReceivableDto.AdditionalInfo) ? null : createReceivableDto.AdditionalInfo,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Receivables.Add(receivable);
                
                // Save the receivable first to ensure it exists in the database
                // before creating the journal entry that references it
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException saveEx)
                {
                    // Capture detailed error information and return immediately
                    var errorDetails = new List<string> { $"DbUpdateException: {saveEx.Message}" };
                    var inner = saveEx.InnerException;
                    int depth = 0;
                    while (inner != null && depth < 10) // Prevent infinite loop
                    {
                        errorDetails.Add($"Inner[{depth}] ({inner.GetType().Name}): {inner.Message}");
                        
                        // Try to get SQL-specific error details using reflection to avoid hard dependency
                        var sqlExType = inner.GetType();
                        if (sqlExType.Name == "SqlException" || sqlExType.FullName?.Contains("SqlException") == true)
                        {
                            try
                            {
                                var numberProp = sqlExType.GetProperty("Number");
                                var stateProp = sqlExType.GetProperty("State");
                                var classProp = sqlExType.GetProperty("Class");
                                var errorsProp = sqlExType.GetProperty("Errors");
                                
                                if (numberProp != null)
                                {
                                    var number = numberProp.GetValue(inner);
                                    errorDetails.Add($"SQL Error Number: {number}");
                                }
                                if (stateProp != null)
                                {
                                    var state = stateProp.GetValue(inner);
                                    errorDetails.Add($"SQL Error State: {state}");
                                }
                                if (classProp != null)
                                {
                                    var severity = classProp.GetValue(inner);
                                    errorDetails.Add($"SQL Error Severity: {severity}");
                                }
                                if (errorsProp != null)
                                {
                                    var errors = errorsProp.GetValue(inner);
                                    if (errors != null && errors is System.Collections.ICollection errorCollection)
                                    {
                                        foreach (var sqlError in errorCollection)
                                        {
                                            var messageProp = sqlError.GetType().GetProperty("Message");
                                            var lineProp = sqlError.GetType().GetProperty("LineNumber");
                                            if (messageProp != null)
                                            {
                                                var msg = messageProp.GetValue(sqlError);
                                                var line = lineProp?.GetValue(sqlError);
                                                errorDetails.Add($"SQL Error Detail: {msg}" + (line != null ? $" (Line {line})" : ""));
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // If reflection fails, just continue
                            }
                        }
                        inner = inner.InnerException;
                        depth++;
                    }
                    var fullError = string.Join(" | ", errorDetails);
                    // Return error directly instead of throwing to ensure it's properly returned
                    return ApiResponse<ReceivableDto>.ErrorResult($"Database save failed: {fullError}");
                }

                // Create accrual accounting entry when receivable is created
                // Accrual Basis: Debit Accounts Receivable, Credit Revenue
                await _accountingService.CreateReceivableAccrualEntryAsync(
                    receivable.Id,
                    userId,
                    totalAmount,
                    receivable.BorrowerName,
                    "Receivable Income",
                    null,
                    $"Receivable created: {receivable.BorrowerName}",
                    DateTime.UtcNow
                );

                // Save the journal entry
                await _context.SaveChangesAsync();

                var receivableDto = await MapToReceivableDtoAsync(receivable);
                return ApiResponse<ReceivableDto>.SuccessResult(receivableDto, "Receivable created successfully");
            }
            catch (DbUpdateException dbEx)
            {
                // Get the full exception chain
                var exceptionMessages = new List<string> { dbEx.Message };
                var innerEx = dbEx.InnerException;
                while (innerEx != null)
                {
                    exceptionMessages.Add(innerEx.Message);
                    innerEx = innerEx.InnerException;
                }
                var fullErrorMessage = string.Join(" | ", exceptionMessages);
                return ApiResponse<ReceivableDto>.ErrorResult($"Failed to create receivable: Database error - {fullErrorMessage}");
            }
            catch (Exception ex)
            {
                // Get the full exception chain
                var exceptionMessages = new List<string> { ex.Message };
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    exceptionMessages.Add(innerEx.Message);
                    innerEx = innerEx.InnerException;
                }
                var fullErrorMessage = string.Join(" | ", exceptionMessages);
                return ApiResponse<ReceivableDto>.ErrorResult($"Failed to create receivable: {fullErrorMessage}");
            }
        }

        public async Task<ApiResponse<ReceivableDto>> GetReceivableAsync(string receivableId, string userId)
        {
            try
            {
                var receivable = await _context.Receivables
                    .Include(r => r.Payments)
                    .FirstOrDefaultAsync(r => r.Id == receivableId && r.UserId == userId);

                if (receivable == null)
                {
                    return ApiResponse<ReceivableDto>.ErrorResult("Receivable not found");
                }

                var receivableDto = await MapToReceivableDtoAsync(receivable);
                return ApiResponse<ReceivableDto>.SuccessResult(receivableDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReceivableDto>.ErrorResult($"Failed to get receivable: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceivableDto>> UpdateReceivableAsync(string receivableId, UpdateReceivableDto updateReceivableDto, string userId)
        {
            try
            {
                var receivable = await _context.Receivables
                    .FirstOrDefaultAsync(r => r.Id == receivableId && r.UserId == userId);

                if (receivable == null)
                {
                    return ApiResponse<ReceivableDto>.ErrorResult("Receivable not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateReceivableDto.BorrowerName))
                    receivable.BorrowerName = updateReceivableDto.BorrowerName;

                if (updateReceivableDto.BorrowerContact != null)
                    receivable.BorrowerContact = string.IsNullOrWhiteSpace(updateReceivableDto.BorrowerContact) ? null : updateReceivableDto.BorrowerContact;

                if (updateReceivableDto.Principal.HasValue)
                    receivable.Principal = updateReceivableDto.Principal.Value;

                if (updateReceivableDto.InterestRate.HasValue)
                    receivable.InterestRate = updateReceivableDto.InterestRate.Value;

                if (updateReceivableDto.Term.HasValue)
                    receivable.Term = updateReceivableDto.Term.Value;

                if (!string.IsNullOrEmpty(updateReceivableDto.Purpose))
                    receivable.Purpose = updateReceivableDto.Purpose;

                if (updateReceivableDto.MonthlyPayment.HasValue)
                    receivable.MonthlyPayment = updateReceivableDto.MonthlyPayment.Value;

                if (!string.IsNullOrEmpty(updateReceivableDto.Status))
                    receivable.Status = updateReceivableDto.Status;

                if (!string.IsNullOrEmpty(updateReceivableDto.PaymentFrequency))
                    receivable.PaymentFrequency = updateReceivableDto.PaymentFrequency.ToUpper();

                if (updateReceivableDto.AdditionalInfo != null)
                    receivable.AdditionalInfo = string.IsNullOrWhiteSpace(updateReceivableDto.AdditionalInfo) ? null : updateReceivableDto.AdditionalInfo;

                // Recalculate total amount if principal, interest rate, or term changed
                if (updateReceivableDto.Principal.HasValue || updateReceivableDto.InterestRate.HasValue || updateReceivableDto.Term.HasValue)
                {
                    if (receivable.InterestRate > 0)
                    {
                        receivable.TotalAmount = receivable.MonthlyPayment * receivable.Term;
                    }
                    else
                    {
                        receivable.TotalAmount = receivable.Principal;
                    }
                }

                receivable.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var receivableDto = await MapToReceivableDtoAsync(receivable);
                return ApiResponse<ReceivableDto>.SuccessResult(receivableDto, "Receivable updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ReceivableDto>.ErrorResult($"Failed to update receivable: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteReceivableAsync(string receivableId, string userId)
        {
            try
            {
                var receivable = await _context.Receivables
                    .FirstOrDefaultAsync(r => r.Id == receivableId && r.UserId == userId);

                if (receivable == null)
                {
                    return ApiResponse<bool>.ErrorResult("Receivable not found");
                }

                // Hard delete (soft delete properties are ignored in DbContext)
                _context.Receivables.Remove(receivable);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Receivable deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete receivable: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ReceivableDto>>> GetUserReceivablesAsync(string userId, string? status = null)
        {
            try
            {
                var query = _context.Receivables
                    .Include(r => r.Payments)
                    .Where(r => r.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(r => r.Status == status.ToUpper());
                }

                var receivables = await query
                    .OrderByDescending(r => r.LentAt)
                    .ToListAsync();

                var receivableDtos = new List<ReceivableDto>();
                foreach (var receivable in receivables)
                {
                    receivableDtos.Add(await MapToReceivableDtoAsync(receivable));
                }

                return ApiResponse<List<ReceivableDto>>.SuccessResult(receivableDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ReceivableDto>>.ErrorResult($"Failed to get receivables: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceivablePaymentDto>> RecordPaymentAsync(CreateReceivablePaymentDto paymentDto, string userId)
        {
            try
            {
                // Verify receivable exists and belongs to user
                var receivable = await _context.Receivables
                    .FirstOrDefaultAsync(r => r.Id == paymentDto.ReceivableId && r.UserId == userId);

                if (receivable == null)
                {
                    return ApiResponse<ReceivablePaymentDto>.ErrorResult("Receivable not found or you don't have access to it");
                }

                if (receivable.Status != "ACTIVE")
                {
                    return ApiResponse<ReceivablePaymentDto>.ErrorResult($"Cannot record payment for receivable with status: {receivable.Status}");
                }

                // Check if payment reference already exists
                var existingPayment = await _context.ReceivablePayments
                    .FirstOrDefaultAsync(p => p.ReceivableId == paymentDto.ReceivableId && p.Reference == paymentDto.Reference);

                if (existingPayment != null)
                {
                    return ApiResponse<ReceivablePaymentDto>.ErrorResult("Payment reference already exists for this receivable");
                }

                // Verify bank account if provided
                BankAccount? bankAccount = null;
                if (!string.IsNullOrEmpty(paymentDto.BankAccountId))
                {
                    bankAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == paymentDto.BankAccountId && ba.UserId == userId);

                    if (bankAccount == null)
                    {
                        return ApiResponse<ReceivablePaymentDto>.ErrorResult("Bank account not found or you don't have access to it");
                    }
                }

                var paymentDate = paymentDto.PaymentDate ?? DateTime.UtcNow;

                // Create receivable payment
                var payment = new ReceivablePayment
                {
                    Id = Guid.NewGuid().ToString(),
                    ReceivableId = paymentDto.ReceivableId,
                    UserId = userId,
                    BankAccountId = string.IsNullOrWhiteSpace(paymentDto.BankAccountId) ? null : paymentDto.BankAccountId,
                    Amount = paymentDto.Amount,
                    Method = paymentDto.Method.ToUpper(),
                    Reference = paymentDto.Reference,
                    Status = "COMPLETED",
                    Description = string.IsNullOrWhiteSpace(paymentDto.Description) ? $"Payment from {receivable.BorrowerName}" : paymentDto.Description,
                    Notes = string.IsNullOrWhiteSpace(paymentDto.Notes) ? null : paymentDto.Notes,
                    PaymentDate = paymentDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ReceivablePayments.Add(payment);

                // Update receivable totals
                receivable.TotalPaid += paymentDto.Amount;
                receivable.RemainingBalance = Math.Max(0, receivable.RemainingBalance - paymentDto.Amount);
                
                // Check if fully paid
                if (receivable.RemainingBalance <= 0)
                {
                    receivable.Status = "COMPLETED";
                    receivable.CompletedAt = DateTime.UtcNow;
                }

                receivable.UpdatedAt = DateTime.UtcNow;

                // If bank account is provided, credit the payment to the bank account
                if (bankAccount != null)
                {
                    // Create bank transaction (CREDIT = money coming in)
                    var bankTransaction = new BankTransaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = bankAccount.Id,
                        UserId = userId,
                        Amount = paymentDto.Amount,
                        TransactionType = "CREDIT",
                        Description = $"Payment received from {receivable.BorrowerName} - {paymentDto.Description ?? "Receivable payment"}",
                        Category = $"[RECEIVABLE-{receivable.BorrowerName}]",
                        ReferenceNumber = paymentDto.Reference,
                        BalanceAfterTransaction = bankAccount.CurrentBalance + paymentDto.Amount,
                        TransactionDate = paymentDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Update bank account balance
                    bankAccount.CurrentBalance += paymentDto.Amount;
                    bankAccount.UpdatedAt = DateTime.UtcNow;

                    _context.BankTransactions.Add(bankTransaction);

                    // Also create Payment record for bank transaction
                    var bankPayment = new Entities.Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = bankAccount.Id,
                        UserId = userId,
                        Amount = paymentDto.Amount,
                        Method = paymentDto.Method,
                        Reference = paymentDto.Reference,
                        Status = "COMPLETED",
                        IsBankTransaction = true,
                        TransactionType = "CREDIT",
                        Description = $"Payment received from {receivable.BorrowerName}",
                        Category = $"[RECEIVABLE-{receivable.BorrowerName}]",
                        Currency = bankAccount.Currency,
                        BalanceAfterTransaction = bankAccount.CurrentBalance,
                        ProcessedAt = paymentDate,
                        TransactionDate = paymentDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(bankPayment);

                    // Check if accrual entry exists for this receivable (accrual accounting)
                    var accrualEntry = await _context.JournalEntries
                        .FirstOrDefaultAsync(je => je.ReceivableId == receivable.Id && je.EntryType == "RECEIVABLE_ACCRUAL");

                    if (accrualEntry != null)
                    {
                        // Use accrual accounting: Debit Bank Account, Credit Accounts Receivable
                        await _accountingService.CreateReceivablePaymentEntryAsync(
                            receivable.Id,
                            userId,
                            paymentDto.Amount,
                            receivable.BorrowerName,
                            bankAccount.AccountName,
                            paymentDto.Reference,
                            $"Payment received from {receivable.BorrowerName}",
                            paymentDate
                        );
                    }
                    // If no accrual entry exists, we don't create a journal entry (cash basis - revenue was already recognized)
                }

                await _context.SaveChangesAsync();

                var paymentDtoResult = await MapToReceivablePaymentDtoAsync(payment);
                return ApiResponse<ReceivablePaymentDto>.SuccessResult(paymentDtoResult, "Payment recorded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ReceivablePaymentDto>.ErrorResult($"Failed to record payment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ReceivablePaymentDto>>> GetReceivablePaymentsAsync(string receivableId, string userId)
        {
            try
            {
                // Verify receivable belongs to user
                var receivable = await _context.Receivables
                    .FirstOrDefaultAsync(r => r.Id == receivableId && r.UserId == userId);

                if (receivable == null)
                {
                    return ApiResponse<List<ReceivablePaymentDto>>.ErrorResult("Receivable not found or you don't have access to it");
                }

                var payments = await _context.ReceivablePayments
                    .Include(p => p.BankAccount)
                    .Where(p => p.ReceivableId == receivableId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                var paymentDtos = new List<ReceivablePaymentDto>();
                foreach (var payment in payments)
                {
                    paymentDtos.Add(await MapToReceivablePaymentDtoAsync(payment));
                }

                return ApiResponse<List<ReceivablePaymentDto>>.SuccessResult(paymentDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ReceivablePaymentDto>>.ErrorResult($"Failed to get payments: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceivablePaymentDto>> GetPaymentAsync(string paymentId, string userId)
        {
            try
            {
                var payment = await _context.ReceivablePayments
                    .Include(p => p.BankAccount)
                    .Include(p => p.Receivable)
                    .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

                if (payment == null)
                {
                    return ApiResponse<ReceivablePaymentDto>.ErrorResult("Payment not found");
                }

                var paymentDto = await MapToReceivablePaymentDtoAsync(payment);
                return ApiResponse<ReceivablePaymentDto>.SuccessResult(paymentDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReceivablePaymentDto>.ErrorResult($"Failed to get payment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeletePaymentAsync(string paymentId, string userId)
        {
            try
            {
                var payment = await _context.ReceivablePayments
                    .Include(p => p.Receivable)
                    .Include(p => p.BankAccount)
                    .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

                if (payment == null)
                {
                    return ApiResponse<bool>.ErrorResult("Payment not found");
                }

                var receivable = payment.Receivable;
                if (receivable == null)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete payment for deleted receivable");
                }

                // Reverse the payment effect on receivable
                receivable.TotalPaid = Math.Max(0, receivable.TotalPaid - payment.Amount);
                receivable.RemainingBalance += payment.Amount;

                // If receivable was completed, reactivate it if there's remaining balance
                if (receivable.Status == "COMPLETED" && receivable.RemainingBalance > 0)
                {
                    receivable.Status = "ACTIVE";
                    receivable.CompletedAt = null;
                }

                receivable.UpdatedAt = DateTime.UtcNow;

                // If payment was credited to bank account, reverse it
                if (payment.BankAccount != null)
                {
                    var bankAccount = payment.BankAccount;
                    bankAccount.CurrentBalance = Math.Max(0, bankAccount.CurrentBalance - payment.Amount);
                    bankAccount.UpdatedAt = DateTime.UtcNow;
                }

                // Remove payment
                _context.ReceivablePayments.Remove(payment);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Payment deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete payment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalReceivablesAsync(string userId)
        {
            try
            {
                var total = await _context.Receivables
                    .Where(r => r.UserId == userId)
                    .SumAsync(r => r.Principal);

                return ApiResponse<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total receivables: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalOutstandingAsync(string userId)
        {
            try
            {
                var total = await _context.Receivables
                    .Where(r => r.UserId == userId && r.Status == "ACTIVE")
                    .SumAsync(r => r.RemainingBalance);

                return ApiResponse<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total outstanding: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalPaidAsync(string userId)
        {
            try
            {
                var total = await _context.Receivables
                    .Where(r => r.UserId == userId)
                    .SumAsync(r => r.TotalPaid);

                return ApiResponse<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total paid: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceivableDto>> MarkReceivableAsCompletedAsync(string receivableId, string userId)
        {
            try
            {
                var receivable = await _context.Receivables
                    .FirstOrDefaultAsync(r => r.Id == receivableId && r.UserId == userId);

                if (receivable == null)
                {
                    return ApiResponse<ReceivableDto>.ErrorResult("Receivable not found");
                }

                receivable.Status = "COMPLETED";
                receivable.CompletedAt = DateTime.UtcNow;
                receivable.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var receivableDto = await MapToReceivableDtoAsync(receivable);
                return ApiResponse<ReceivableDto>.SuccessResult(receivableDto, "Receivable marked as completed");
            }
            catch (Exception ex)
            {
                return ApiResponse<ReceivableDto>.ErrorResult($"Failed to mark receivable as completed: {ex.Message}");
            }
        }

        // Helper Methods
        private async Task<ReceivableDto> MapToReceivableDtoAsync(Receivable receivable)
        {
            // Load payments if not already loaded
            if (receivable.Payments == null || !receivable.Payments.Any())
            {
                await _context.Entry(receivable)
                    .Collection(r => r.Payments)
                    .LoadAsync();
            }

            var payments = receivable.Payments?.ToList() ?? new List<ReceivablePayment>();
            var lastPayment = payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
            
            // Calculate next payment due date (assuming monthly payments)
            DateTime? nextPaymentDueDate = null;
            if (receivable.Status == "ACTIVE" && receivable.RemainingBalance > 0)
            {
                if (lastPayment != null)
                {
                    nextPaymentDueDate = lastPayment.PaymentDate.AddMonths(1);
                }
                else if (receivable.StartDate.HasValue)
                {
                    nextPaymentDueDate = receivable.StartDate.Value.AddMonths(1);
                }
            }

            return new ReceivableDto
            {
                Id = receivable.Id,
                UserId = receivable.UserId,
                BorrowerName = receivable.BorrowerName,
                BorrowerContact = receivable.BorrowerContact,
                Principal = receivable.Principal,
                InterestRate = receivable.InterestRate,
                Term = receivable.Term,
                Purpose = receivable.Purpose,
                Status = receivable.Status,
                MonthlyPayment = receivable.MonthlyPayment,
                TotalAmount = receivable.TotalAmount,
                RemainingBalance = receivable.RemainingBalance,
                TotalPaid = receivable.TotalPaid,
                LentAt = receivable.LentAt,
                StartDate = receivable.StartDate,
                CompletedAt = receivable.CompletedAt,
                AdditionalInfo = receivable.AdditionalInfo,
                PaymentFrequency = receivable.PaymentFrequency,
                CreatedAt = receivable.CreatedAt,
                UpdatedAt = receivable.UpdatedAt,
                PaymentCount = payments.Count,
                LastPaymentDate = lastPayment?.PaymentDate,
                NextPaymentDueDate = nextPaymentDueDate
            };
        }

        private async Task<ReceivablePaymentDto> MapToReceivablePaymentDtoAsync(ReceivablePayment payment)
        {
            // Load receivable if not already loaded
            if (payment.Receivable == null)
            {
                await _context.Entry(payment)
                    .Reference(p => p.Receivable)
                    .LoadAsync();
            }

            return new ReceivablePaymentDto
            {
                Id = payment.Id,
                ReceivableId = payment.ReceivableId,
                UserId = payment.UserId,
                BankAccountId = payment.BankAccountId,
                BankAccountName = payment.BankAccount?.AccountName,
                Amount = payment.Amount,
                Method = payment.Method,
                Reference = payment.Reference,
                Status = payment.Status,
                Description = payment.Description,
                Notes = payment.Notes,
                PaymentDate = payment.PaymentDate,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                BorrowerName = payment.Receivable?.BorrowerName ?? string.Empty
            };
        }
    }
}

