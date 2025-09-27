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
                Console.WriteLine($"DEBUG: Starting MakePaymentAsync. LoanId: {payment.LoanId}, Amount: {payment.Amount}, Method: {payment.Method}, Reference: {payment.Reference}");
                
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

                // Handle different payment methods
                var methodLower = payment.Method.ToLower().Trim();
                if (methodLower == "bank transfer" || methodLower == "bank transaction" || methodLower == "bank_transfer" || methodLower == "banktransfer")
                {
                    // Debug: Log that we're processing bank transfer
                    Console.WriteLine($"DEBUG: Processing bank transfer payment. Method: '{payment.Method}', MethodLower: '{methodLower}', BankAccountId: '{payment.BankAccountId}'");
                    // Validate bank account for bank transfer
                    if (string.IsNullOrEmpty(payment.BankAccountId))
                    {
                        return ApiResponse<PaymentDto>.ErrorResult("BankAccountId is required for Bank transfer method");
                    }

                    // Verify bank account exists and belongs to user
                    var bankAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == payment.BankAccountId && ba.UserId == userId);

                    Console.WriteLine($"DEBUG: Bank account lookup. BankAccountId: {payment.BankAccountId}, UserId: {userId}, Found: {bankAccount != null}");

                    if (bankAccount == null)
                    {
                        return ApiResponse<PaymentDto>.ErrorResult("Bank account not found or does not belong to user");
                    }

                    Console.WriteLine($"DEBUG: Bank account found. CurrentBalance: {bankAccount.CurrentBalance}, AccountName: {bankAccount.AccountName}");

                    // Create loan payment record
                    //var loanPayment = new Entities.Payment
                    //{
                    //    LoanId = payment.LoanId,
                    //    UserId = userId,
                    //    Amount = payment.Amount,
                    //    Method = payment.Method,
                    //    Reference = payment.Reference,
                    //    Status = "COMPLETED",
                    //    IsBankTransaction = false, // This is a loan payment
                    //    TransactionType = "PAYMENT",
                    //    Description = $"Loan payment via {payment.Method}",
                    //    ProcessedAt = DateTime.UtcNow,
                    //    TransactionDate = DateTime.UtcNow,
                    //    CreatedAt = DateTime.UtcNow,
                    //    UpdatedAt = DateTime.UtcNow
                    //};

                    //_context.Payments.Add(loanPayment);

                    // Create single payment record that serves both loan payment and bank transaction
                    var bankTransaction = new Entities.Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = payment.BankAccountId,
                        LoanId = payment.LoanId,
                        UserId = userId,
                        Amount = payment.Amount,
                        Method = "BANK_TRANSFER",
                        Reference = $"BANK_TXN_{payment.Reference}",
                        Status = "COMPLETED",
                        IsBankTransaction = true, // This is a bank transaction
                        TransactionType = "DEBIT",
                        Description = $"Loan payment - {payment.Reference}",
                        Category = "LOAN_PAYMENT",
                        ExternalTransactionId = $"LOAN_PAY_{payment.Reference}",
                        ProcessedAt = DateTime.UtcNow,
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(bankTransaction);
                    Console.WriteLine($"DEBUG: Added combined loan payment + bank transaction to Payments table. ID: {bankTransaction.Id}, Reference: {bankTransaction.Reference}");

                    // Update bank account balance (debit)
                    bankAccount.CurrentBalance -= payment.Amount;
                    var newBalance = bankAccount.CurrentBalance;

                    // Also create BankTransaction record
                    var bankTxnRecord = new Entities.BankTransaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = payment.BankAccountId,
                        UserId = userId,
                        Amount = payment.Amount,
                        TransactionType = "DEBIT",
                        Description = $"Loan payment - {payment.Reference}",
                        Category = "LOAN_PAYMENT",
                        ReferenceNumber = $"BANK_TXN_{payment.Reference}",
                        ExternalTransactionId = $"LOAN_PAY_{payment.Reference}",
                        BalanceAfterTransaction = newBalance, // Set the balance after transaction
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.BankTransactions.Add(bankTxnRecord);
                    Console.WriteLine($"DEBUG: Added bank transaction to BankTransactions table. ID: {bankTxnRecord.Id}, Reference: {bankTxnRecord.ReferenceNumber}, BalanceAfter: {bankTxnRecord.BalanceAfterTransaction}");

                    // Set balance after transaction for bank transaction Payment record
                    bankTransaction.BalanceAfterTransaction = newBalance;
                    bankAccount.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Debug: Log that we're processing cash/other payment
                    Console.WriteLine($"DEBUG: Processing cash/other payment. Method: '{payment.Method}', MethodLower: '{methodLower}'");
                    // Cash or other payment methods - only create loan payment record
                    var loanPayment = new Entities.Payment
                    {
                        LoanId = payment.LoanId,
                        UserId = userId,
                        Amount = payment.Amount,
                        Method = payment.Method,
                        Reference = payment.Reference,
                        Status = "COMPLETED",
                        IsBankTransaction = false, // This is a loan payment only
                        TransactionType = "PAYMENT",
                        Description = $"Loan payment via {payment.Method}",
                        ProcessedAt = DateTime.UtcNow,
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(loanPayment);
                }

                // Update loan remaining balance
                var oldBalance = loan.RemainingBalance;
                loan.RemainingBalance -= payment.Amount;
                
                Console.WriteLine($"DEBUG: Loan balance update - Old: {oldBalance}, Payment: {payment.Amount}, New: {loan.RemainingBalance}");

                // Check if loan is fully paid
                if (loan.RemainingBalance <= 0)
                {
                    loan.RemainingBalance = 0; // Ensure it's not negative
                    loan.Status = "COMPLETED";
                    loan.CompletedAt = DateTime.UtcNow;
                    Console.WriteLine($"DEBUG: Loan marked as COMPLETED. Final balance: {loan.RemainingBalance}");
                }

                try
                {
                    Console.WriteLine($"DEBUG: About to save changes. Entities being tracked:");
                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        Console.WriteLine($"  - {entry.Entity.GetType().Name}: {entry.State}");
                    }
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: Successfully saved changes to database. Total changes: {_context.ChangeTracker.Entries().Count()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: SaveChanges failed with error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"DEBUG: Inner exception: {ex.InnerException.Message}");
                    }
                    throw; // Re-throw to preserve the original error
                }

                // Get the payment record to return
                var loanPaymentRecord = await _context.Payments
                    .FirstOrDefaultAsync(p => p.LoanId == payment.LoanId && 
                                            p.Reference == (methodLower == "bank transfer" || methodLower == "bank transaction" || methodLower == "bank_transfer" || methodLower == "banktransfer" 
                                                ? $"BANK_TXN_{payment.Reference}" 
                                                : payment.Reference));

                if (loanPaymentRecord == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Failed to retrieve payment record");
                }

                var paymentDto = new PaymentDto
                {
                    Id = loanPaymentRecord.Id,
                    LoanId = loanPaymentRecord.LoanId,
                    BankAccountId = loanPaymentRecord.BankAccountId,
                    UserId = loanPaymentRecord.UserId,
                    Amount = loanPaymentRecord.Amount,
                    Method = loanPaymentRecord.Method,
                    Reference = loanPaymentRecord.Reference,
                    Status = loanPaymentRecord.Status,
                    IsBankTransaction = loanPaymentRecord.IsBankTransaction,
                    TransactionType = loanPaymentRecord.TransactionType,
                    Description = loanPaymentRecord.Description,
                    Category = loanPaymentRecord.Category,
                    ExternalTransactionId = loanPaymentRecord.ExternalTransactionId,
                    Notes = loanPaymentRecord.Notes,
                    Merchant = loanPaymentRecord.Merchant,
                    Location = loanPaymentRecord.Location,
                    IsRecurring = loanPaymentRecord.IsRecurring,
                    RecurringFrequency = loanPaymentRecord.RecurringFrequency,
                    Currency = loanPaymentRecord.Currency,
                    BalanceAfterTransaction = loanPaymentRecord.BalanceAfterTransaction,
                    ProcessedAt = loanPaymentRecord.ProcessedAt,
                    TransactionDate = loanPaymentRecord.TransactionDate,
                    CreatedAt = loanPaymentRecord.CreatedAt,
                    UpdatedAt = loanPaymentRecord.UpdatedAt
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

                // The payment record itself is the loan activity, so we just need to remove it
                // No need to find a separate loan activity record

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

        // Bank Transaction methods (now using Payment table)
        public async Task<ApiResponse<PaymentDto>> CreateBankTransactionAsync(CreateBankTransactionDto transaction, string userId)
        {
            try
            {
                // Verify bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == transaction.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Bank account not found");
                }

                // Create bank transaction as Payment
                var newPayment = new Entities.Payment
                {
                    BankAccountId = transaction.BankAccountId,
                    UserId = userId,
                    Amount = transaction.Amount,
                    Method = "BANK_TRANSFER",
                    Reference = transaction.ReferenceNumber ?? $"BANK_TXN_{Guid.NewGuid()}",
                    Status = "COMPLETED",
                    IsBankTransaction = true,
                    TransactionType = transaction.TransactionType,
                    Description = transaction.Description,
                    Category = transaction.Category,
                    ExternalTransactionId = transaction.ExternalTransactionId,
                    Notes = transaction.Notes,
                    Merchant = transaction.Merchant,
                    Location = transaction.Location,
                    IsRecurring = transaction.IsRecurring,
                    RecurringFrequency = transaction.RecurringFrequency,
                    Currency = transaction.Currency,
                    ProcessedAt = transaction.TransactionDate,
                    TransactionDate = transaction.TransactionDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(newPayment);

                // Update bank account balance
                if (transaction.TransactionType == "CREDIT")
                {
                    bankAccount.CurrentBalance += transaction.Amount;
                }
                else if (transaction.TransactionType == "DEBIT")
                {
                    bankAccount.CurrentBalance -= transaction.Amount;
                }

                newPayment.BalanceAfterTransaction = bankAccount.CurrentBalance;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var paymentDto = MapToPaymentDto(newPayment);
                return ApiResponse<PaymentDto>.SuccessResult(paymentDto, "Bank transaction created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentDto>.ErrorResult($"Failed to create bank transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaymentDto>> GetBankTransactionAsync(string transactionId, string userId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.BankAccount)
                    .FirstOrDefaultAsync(p => p.Id == transactionId && p.UserId == userId && p.IsBankTransaction);

                if (payment == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResult("Bank transaction not found");
                }

                var paymentDto = MapToPaymentDto(payment);
                return ApiResponse<PaymentDto>.SuccessResult(paymentDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentDto>.ErrorResult($"Failed to get bank transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PaymentDto>>> GetBankTransactionsAsync(string userId, string? accountType = null, int page = 1, int limit = 50)
        {
            try
            {
                var query = _context.Payments
                    .Include(p => p.BankAccount)
                    .Where(p => p.UserId == userId && p.IsBankTransaction);

                if (!string.IsNullOrEmpty(accountType))
                {
                    query = query.Where(p => p.BankAccount != null && p.BankAccount.AccountType == accountType.ToLower());
                }

                var payments = await query
                    .OrderByDescending(p => p.TransactionDate ?? p.ProcessedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var paymentDtos = payments.Select(MapToPaymentDto).ToList();
                return ApiResponse<List<PaymentDto>>.SuccessResult(paymentDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PaymentDto>>.ErrorResult($"Failed to get bank transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PaymentDto>>> GetAccountTransactionsAsync(string bankAccountId, string userId, int page = 1, int limit = 50)
        {
            try
            {
                var payments = await _context.Payments
                    .Include(p => p.BankAccount)
                    .Where(p => p.BankAccountId == bankAccountId && p.UserId == userId && p.IsBankTransaction)
                    .OrderByDescending(p => p.TransactionDate ?? p.ProcessedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var paymentDtos = payments.Select(MapToPaymentDto).ToList();
                return ApiResponse<List<PaymentDto>>.SuccessResult(paymentDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PaymentDto>>.ErrorResult($"Failed to get account transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBankTransactionAsync(string transactionId, string userId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.BankAccount)
                    .FirstOrDefaultAsync(p => p.Id == transactionId && p.UserId == userId && p.IsBankTransaction);

                if (payment == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bank transaction not found");
                }

                // Check if transaction is too old to delete (e.g., more than 24 hours)
                var timeSinceCreation = DateTime.UtcNow - payment.CreatedAt;
                if (timeSinceCreation.TotalHours > 24)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete bank transaction older than 24 hours");
                }

                // Reverse the bank account balance
                if (payment.BankAccount != null)
                {
                    if (payment.TransactionType == "CREDIT")
                    {
                        payment.BankAccount.CurrentBalance -= payment.Amount;
                    }
                    else if (payment.TransactionType == "DEBIT")
                    {
                        payment.BankAccount.CurrentBalance += payment.Amount;
                    }
                    payment.BankAccount.UpdatedAt = DateTime.UtcNow;
                }

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Bank transaction deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete bank transaction: {ex.Message}");
            }
        }

        private PaymentDto MapToPaymentDto(Entities.Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                LoanId = payment.LoanId,
                BankAccountId = payment.BankAccountId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                Method = payment.Method,
                Reference = payment.Reference,
                Status = payment.Status,
                IsBankTransaction = payment.IsBankTransaction,
                TransactionType = payment.TransactionType,
                Description = payment.Description,
                Category = payment.Category,
                ExternalTransactionId = payment.ExternalTransactionId,
                Notes = payment.Notes,
                Merchant = payment.Merchant,
                Location = payment.Location,
                IsRecurring = payment.IsRecurring,
                RecurringFrequency = payment.RecurringFrequency,
                Currency = payment.Currency,
                BalanceAfterTransaction = payment.BalanceAfterTransaction,
                ProcessedAt = payment.ProcessedAt,
                TransactionDate = payment.TransactionDate,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}

