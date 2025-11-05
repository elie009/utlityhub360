using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;

        public BankAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<BankAccountDto>> CreateBankAccountAsync(CreateBankAccountDto createBankAccountDto, string userId)
        {
            try
            {
                var bankAccount = new BankAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    AccountName = createBankAccountDto.AccountName,
                    AccountType = createBankAccountDto.AccountType.ToLower(),
                    InitialBalance = createBankAccountDto.InitialBalance,
                    CurrentBalance = createBankAccountDto.InitialBalance,
                    Currency = createBankAccountDto.Currency.ToUpper(),
                    Description = createBankAccountDto.Description,
                    FinancialInstitution = createBankAccountDto.FinancialInstitution,
                    AccountNumber = createBankAccountDto.AccountNumber,
                    RoutingNumber = createBankAccountDto.RoutingNumber,
                    SyncFrequency = createBankAccountDto.SyncFrequency.ToUpper(),
                    Iban = createBankAccountDto.Iban,
                    SwiftCode = createBankAccountDto.SwiftCode,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsConnected = false
                };

                _context.BankAccounts.Add(bankAccount);
                await _context.SaveChangesAsync();

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto, "Bank account created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to create bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountDto>> GetBankAccountAsync(string bankAccountId, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account not found");
                }

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to get bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountDto>> UpdateBankAccountAsync(string bankAccountId, UpdateBankAccountDto updateBankAccountDto, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateBankAccountDto.AccountName))
                    bankAccount.AccountName = updateBankAccountDto.AccountName;

                if (!string.IsNullOrEmpty(updateBankAccountDto.AccountType))
                    bankAccount.AccountType = updateBankAccountDto.AccountType.ToLower();

                if (updateBankAccountDto.CurrentBalance.HasValue)
                    bankAccount.CurrentBalance = updateBankAccountDto.CurrentBalance.Value;

                if (!string.IsNullOrEmpty(updateBankAccountDto.Currency))
                    bankAccount.Currency = updateBankAccountDto.Currency.ToUpper();

                if (updateBankAccountDto.Description != null)
                    bankAccount.Description = updateBankAccountDto.Description;

                if (updateBankAccountDto.FinancialInstitution != null)
                    bankAccount.FinancialInstitution = updateBankAccountDto.FinancialInstitution;

                if (updateBankAccountDto.AccountNumber != null)
                    bankAccount.AccountNumber = updateBankAccountDto.AccountNumber;

                if (updateBankAccountDto.RoutingNumber != null)
                    bankAccount.RoutingNumber = updateBankAccountDto.RoutingNumber;

                if (!string.IsNullOrEmpty(updateBankAccountDto.SyncFrequency))
                    bankAccount.SyncFrequency = updateBankAccountDto.SyncFrequency.ToUpper();

                if (updateBankAccountDto.IsActive.HasValue)
                    bankAccount.IsActive = updateBankAccountDto.IsActive.Value;

                if (updateBankAccountDto.Iban != null)
                    bankAccount.Iban = updateBankAccountDto.Iban;

                if (updateBankAccountDto.SwiftCode != null)
                    bankAccount.SwiftCode = updateBankAccountDto.SwiftCode;

                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto, "Bank account updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to update bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBankAccountAsync(string bankAccountId, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bank account not found");
                }

                _context.BankAccounts.Remove(bankAccount);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Bank account deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankAccountDto>>> GetUserBankAccountsAsync(string userId, bool includeInactive = false)
        {
            try
            {
                var query = _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .Where(ba => ba.UserId == userId);

                if (!includeInactive)
                {
                    query = query.Where(ba => ba.IsActive);
                }

                var bankAccounts = await query
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .ToListAsync();

                var bankAccountDtos = new List<BankAccountDto>();
                foreach (var account in bankAccounts)
                {
                    bankAccountDtos.Add(await MapToBankAccountDtoAsync(account));
                }

                return ApiResponse<List<BankAccountDto>>.SuccessResult(bankAccountDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get bank accounts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountSummaryDto>> GetBankAccountSummaryAsync(string userId, string frequency = "monthly", int? year = null, int? month = null)
        {
            try
            {
                var bankAccounts = await _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                // Determine the target month for transaction totals
                DateTime targetMonthStart;
                DateTime targetMonthEnd;
                
                if (year.HasValue && month.HasValue)
                {
                    // Use the provided year and month
                    targetMonthStart = new DateTime(year.Value, month.Value, 1);
                    targetMonthEnd = targetMonthStart.AddMonths(1).AddDays(-1);
                }
                else
                {
                    // Use current month
                    targetMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                    targetMonthEnd = targetMonthStart.AddMonths(1).AddDays(-1);
                }

                if (!bankAccounts.Any())
                {
                    return ApiResponse<BankAccountSummaryDto>.SuccessResult(new BankAccountSummaryDto
                    {
                        TotalBalance = 0,
                        TotalAccounts = 0,
                        ActiveAccounts = 0,
                        ConnectedAccounts = 0,
                        TotalIncoming = 0,
                        TotalOutgoing = 0,
                        CurrentMonthIncoming = 0,
                        CurrentMonthOutgoing = 0,
                        CurrentMonthNet = 0,
                        Frequency = frequency,
                        PeriodStart = targetMonthStart,
                        PeriodEnd = targetMonthEnd,
                        TransactionCount = 0,
                        Accounts = new List<BankAccountDto>(),
                        SpendingByCategory = new Dictionary<string, decimal>()
                    });
                }

                // Calculate period dates based on frequency (if year/month not provided) or use the target month
                DateTime periodStart;
                DateTime periodEnd;
                
                if (year.HasValue && month.HasValue)
                {
                    // Use the specific month for the period
                    periodStart = targetMonthStart;
                    periodEnd = targetMonthEnd;
                }
                else
                {
                    // Use frequency-based period
                    (periodStart, periodEnd) = GetPeriodDates(frequency);
                }

                // Get all transactions for the period (now from Payments table)
                var allTransactions = await _context.Payments
                    .Where(p => p.UserId == userId && p.IsBankTransaction && 
                               p.TransactionDate >= periodStart && p.TransactionDate <= periodEnd)
                    .ToListAsync();

                // Get transactions for the target month (for transaction totals)
                var targetMonthTransactions = await _context.Payments
                    .Where(p => p.UserId == userId && p.IsBankTransaction && 
                               p.TransactionDate >= targetMonthStart && p.TransactionDate <= targetMonthEnd)
                    .ToListAsync();

                var summary = new BankAccountSummaryDto
                {
                    TotalBalance = bankAccounts.Sum(ba => ba.CurrentBalance),
                    TotalAccounts = bankAccounts.Count,
                    ActiveAccounts = bankAccounts.Count(ba => ba.IsActive),
                    ConnectedAccounts = bankAccounts.Count(ba => ba.IsConnected),
                    TotalIncoming = allTransactions
                        .Where(t => t.TransactionType == "CREDIT")
                        .Sum(t => t.Amount),
                    TotalOutgoing = allTransactions
                        .Where(t => t.TransactionType == "DEBIT")
                        .Sum(t => t.Amount),
                    CurrentMonthIncoming = targetMonthTransactions
                        .Where(t => t.TransactionType == "CREDIT")
                        .Sum(t => t.Amount),
                    CurrentMonthOutgoing = targetMonthTransactions
                        .Where(t => t.TransactionType == "DEBIT")
                        .Sum(t => t.Amount),
                    CurrentMonthNet = targetMonthTransactions
                        .Where(t => t.TransactionType == "CREDIT")
                        .Sum(t => t.Amount) - targetMonthTransactions
                        .Where(t => t.TransactionType == "DEBIT")
                        .Sum(t => t.Amount),
                    Frequency = frequency,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    TransactionCount = allTransactions.Count,
                    Accounts = new List<BankAccountDto>(),
                    SpendingByCategory = allTransactions
                        .Where(t => t.TransactionType == "DEBIT" && !string.IsNullOrEmpty(t.Category))
                        .GroupBy(t => t.Category!)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount))
                };

                foreach (var account in bankAccounts)
                {
                    summary.Accounts.Add(await MapToBankAccountDtoAsync(account));
                }

                return ApiResponse<BankAccountSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountSummaryDto>.ErrorResult($"Failed to get bank account summary: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountAnalyticsDto>> GetBankAccountAnalyticsAsync(string userId, string period = "month")
        {
            try
            {
                var (startDate, endDate) = GetPeriodDates(period);

                var bankAccounts = await _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                var transactions = bankAccounts
                    .SelectMany(ba => ba.Transactions)
                    .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                    .ToList();

                var analytics = new BankAccountAnalyticsDto
                {
                    TotalBalance = bankAccounts.Sum(ba => ba.CurrentBalance),
                    TotalIncoming = transactions.Where(t => t.TransactionType == "CREDIT").Sum(t => t.Amount),
                    TotalOutgoing = transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount),
                    TotalTransactions = transactions.Count,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TopAccounts = new List<BankAccountDto>(),
                    SpendingByCategory = transactions
                        .Where(t => t.TransactionType == "DEBIT" && !string.IsNullOrEmpty(t.Category))
                        .GroupBy(t => t.Category!)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount))
                };

                // Get top 5 accounts by balance
                var topAccounts = bankAccounts
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .Take(5)
                    .ToList();

                foreach (var account in topAccounts)
                {
                    analytics.TopAccounts.Add(await MapToBankAccountDtoAsync(account));
                }

                return ApiResponse<BankAccountAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountAnalyticsDto>.ErrorResult($"Failed to get bank account analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalBalanceAsync(string userId)
        {
            try
            {
                var totalBalance = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .SumAsync(ba => ba.CurrentBalance);

                return ApiResponse<decimal>.SuccessResult(totalBalance);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total balance: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankAccountDto>>> GetTopAccountsByBalanceAsync(string userId, int limit = 5)
        {
            try
            {
                var bankAccounts = await _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .Take(limit)
                    .ToListAsync();

                var bankAccountDtos = new List<BankAccountDto>();
                foreach (var account in bankAccounts)
                {
                    bankAccountDtos.Add(await MapToBankAccountDtoAsync(account));
                }

                return ApiResponse<List<BankAccountDto>>.SuccessResult(bankAccountDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get top accounts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountDto>> ConnectBankAccountAsync(BankIntegrationDto integrationDto, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == integrationDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account not found");
                }

                // Simulate bank integration connection
                bankAccount.IsConnected = true;
                bankAccount.ConnectionId = integrationDto.ConnectionId ?? Guid.NewGuid().ToString();
                bankAccount.FinancialInstitution = integrationDto.FinancialInstitution;
                bankAccount.LastSyncedAt = DateTime.UtcNow;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto, "Bank account connected successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to connect bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountDto>> SyncBankAccountAsync(SyncBankAccountDto syncDto, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == syncDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account not found");
                }

                if (!bankAccount.IsConnected)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account is not connected");
                }

                // Simulate bank sync - in real implementation, this would call bank API
                bankAccount.LastSyncedAt = DateTime.UtcNow;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto, "Bank account synced successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to sync bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankAccountDto>>> GetConnectedAccountsAsync(string userId)
        {
            try
            {
                var bankAccounts = await _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .Where(ba => ba.UserId == userId && ba.IsConnected && ba.IsActive)
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .ToListAsync();

                var bankAccountDtos = new List<BankAccountDto>();
                foreach (var account in bankAccounts)
                {
                    bankAccountDtos.Add(await MapToBankAccountDtoAsync(account));
                }

                return ApiResponse<List<BankAccountDto>>.SuccessResult(bankAccountDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get connected accounts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DisconnectBankAccountAsync(string bankAccountId, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bank account not found");
                }

                bankAccount.IsConnected = false;
                bankAccount.ConnectionId = null;
                bankAccount.LastSyncedAt = null;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Bank account disconnected successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to disconnect bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankTransactionDto>> CreateTransactionAsync(CreateBankTransactionDto createTransactionDto, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == createTransactionDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult("Bank account not found");
                }

                // Check if this is a transfer between bank accounts
                bool isTransfer = !string.IsNullOrEmpty(createTransactionDto.ToBankAccountId) && 
                                  createTransactionDto.ToBankAccountId != createTransactionDto.BankAccountId;

                if (isTransfer)
                {
                    // Validate destination account
                    if (createTransactionDto.TransactionType.ToUpper() != "DEBIT")
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Transfers must be DEBIT type from the source account");
                    }

                    var destinationAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == createTransactionDto.ToBankAccountId && ba.UserId == userId);

                    if (destinationAccount == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Destination bank account not found");
                    }

                    if (!destinationAccount.IsActive)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Destination bank account is not active");
                    }

                    // Generate a shared reference number for both transactions
                    var transferReference = createTransactionDto.ReferenceNumber ?? $"TRANSFER_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                    var transferDescription = string.IsNullOrEmpty(createTransactionDto.Description) 
                        ? $"Transfer to {destinationAccount.AccountName}" 
                        : createTransactionDto.Description;

                    // Create DEBIT transaction from source account
                    var debitPayment = new Entities.Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = createTransactionDto.BankAccountId,
                        UserId = userId,
                        Amount = createTransactionDto.Amount,
                        Method = "BANK_TRANSFER",
                        Reference = transferReference,
                        Status = "COMPLETED",
                        IsBankTransaction = true,
                        TransactionType = "DEBIT",
                        Description = $"Transfer: {transferDescription}",
                        Category = createTransactionDto.Category ?? "TRANSFER",
                        ExternalTransactionId = $"TRANSFER_{transferReference}",
                        Notes = createTransactionDto.Notes ?? $"Transfer to {destinationAccount.AccountName}",
                        Currency = createTransactionDto.Currency.ToUpper(),
                        ProcessedAt = createTransactionDto.TransactionDate,
                        TransactionDate = createTransactionDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Update source account balance (deduct)
                    bankAccount.CurrentBalance -= debitPayment.Amount;
                    debitPayment.BalanceAfterTransaction = bankAccount.CurrentBalance;
                    bankAccount.UpdatedAt = DateTime.UtcNow;

                    // Create CREDIT transaction to destination account
                    var creditPayment = new Entities.Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = createTransactionDto.ToBankAccountId,
                        UserId = userId,
                        Amount = createTransactionDto.Amount,
                        Method = "BANK_TRANSFER",
                        Reference = transferReference,
                        Status = "COMPLETED",
                        IsBankTransaction = true,
                        TransactionType = "CREDIT",
                        Description = $"Transfer: {transferDescription}",
                        Category = createTransactionDto.Category ?? "TRANSFER",
                        ExternalTransactionId = $"TRANSFER_{transferReference}",
                        Notes = createTransactionDto.Notes ?? $"Transfer from {bankAccount.AccountName}",
                        Currency = createTransactionDto.Currency.ToUpper(),
                        ProcessedAt = createTransactionDto.TransactionDate,
                        TransactionDate = createTransactionDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Update destination account balance (add)
                    destinationAccount.CurrentBalance += creditPayment.Amount;
                    creditPayment.BalanceAfterTransaction = destinationAccount.CurrentBalance;
                    destinationAccount.UpdatedAt = DateTime.UtcNow;

                    // Add both transactions
                    _context.Payments.Add(debitPayment);
                    _context.Payments.Add(creditPayment);

                    // Also create BankTransaction records
                    var debitBankTransaction = new Entities.BankTransaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = createTransactionDto.BankAccountId,
                        UserId = userId,
                        Amount = createTransactionDto.Amount,
                        TransactionType = "DEBIT",
                        Description = $"Transfer: {transferDescription}",
                        Category = createTransactionDto.Category ?? "TRANSFER",
                        ReferenceNumber = transferReference,
                        ExternalTransactionId = $"TRANSFER_{transferReference}",
                        Notes = createTransactionDto.Notes ?? $"Transfer to {destinationAccount.AccountName}",
                        Currency = createTransactionDto.Currency.ToUpper(),
                        BalanceAfterTransaction = bankAccount.CurrentBalance,
                        TransactionDate = createTransactionDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var creditBankTransaction = new Entities.BankTransaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        BankAccountId = createTransactionDto.ToBankAccountId,
                        UserId = userId,
                        Amount = createTransactionDto.Amount,
                        TransactionType = "CREDIT",
                        Description = $"Transfer: {transferDescription}",
                        Category = createTransactionDto.Category ?? "TRANSFER",
                        ReferenceNumber = transferReference,
                        ExternalTransactionId = $"TRANSFER_{transferReference}",
                        Notes = createTransactionDto.Notes ?? $"Transfer from {bankAccount.AccountName}",
                        Currency = createTransactionDto.Currency.ToUpper(),
                        BalanceAfterTransaction = destinationAccount.CurrentBalance,
                        TransactionDate = createTransactionDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.BankTransactions.Add(debitBankTransaction);
                    _context.BankTransactions.Add(creditBankTransaction);

                    await _context.SaveChangesAsync();

                    // Return the debit transaction (source account transaction) as the primary response
                    var transferTransactionDto = MapPaymentToBankTransactionDto(debitPayment, bankAccount.AccountName);
                    return ApiResponse<BankTransactionDto>.SuccessResult(transferTransactionDto, 
                        $"Transfer of {createTransactionDto.Amount:C} from {bankAccount.AccountName} to {destinationAccount.AccountName} completed successfully");
                }

                // Regular transaction (non-transfer)
                // Handle category-based references
                string? billId = null;
                string? savingsAccountId = null;
                string? loanId = null;
                string enhancedDescription = createTransactionDto.Description;

                if (!string.IsNullOrEmpty(createTransactionDto.Category))
                {
                    var categoryLower = createTransactionDto.Category.ToLower();
                    
                    // Bill-related categories
                    if (categoryLower.Contains("bill") || categoryLower.Contains("utility") || 
                        categoryLower.Contains("rent") || categoryLower.Contains("insurance") ||
                        categoryLower.Contains("subscription") || categoryLower.Contains("payment"))
                    {
                        if (!string.IsNullOrEmpty(createTransactionDto.BillId))
                        {
                            billId = createTransactionDto.BillId;
                            enhancedDescription = $"Bill Payment - {createTransactionDto.Description}";
                        }
                    }
                    // Savings-related categories (only if explicitly linking to a different savings account)
                    else if ((categoryLower.Contains("savings") || categoryLower.Contains("deposit") || 
                             categoryLower.Contains("investment") || categoryLower.Contains("goal")) &&
                             !string.IsNullOrEmpty(createTransactionDto.SavingsAccountId) &&
                             createTransactionDto.SavingsAccountId != createTransactionDto.BankAccountId)
                    {
                        savingsAccountId = createTransactionDto.SavingsAccountId;
                        enhancedDescription = $"Savings - {createTransactionDto.Description}";
                    }
                    // Loan-related categories
                    else if (categoryLower.Contains("loan") || categoryLower.Contains("repayment") || 
                             categoryLower.Contains("debt") || categoryLower.Contains("installment"))
                    {
                        if (!string.IsNullOrEmpty(createTransactionDto.LoanId))
                        {
                            loanId = createTransactionDto.LoanId;
                            enhancedDescription = $"Loan Payment - {createTransactionDto.Description}";
                        }
                    }
                }

                // Create transaction as Payment with IsBankTransaction = true
                var payment = new Entities.Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    BankAccountId = createTransactionDto.BankAccountId,
                    BillId = billId,
                    SavingsAccountId = savingsAccountId,
                    LoanId = loanId,
                    UserId = userId,
                    Amount = createTransactionDto.Amount,
                    Method = "BANK_TRANSFER",
                    Reference = createTransactionDto.ReferenceNumber ?? $"BANK_TXN_{Guid.NewGuid()}",
                    Status = "COMPLETED",
                    IsBankTransaction = true,
                    TransactionType = createTransactionDto.TransactionType.ToUpper(),
                    Description = enhancedDescription,
                    Category = createTransactionDto.Category,
                    ExternalTransactionId = createTransactionDto.ExternalTransactionId,
                    Notes = createTransactionDto.Notes,
                    Merchant = createTransactionDto.Merchant,
                    Location = createTransactionDto.Location,
                    IsRecurring = createTransactionDto.IsRecurring,
                    RecurringFrequency = createTransactionDto.RecurringFrequency,
                    Currency = createTransactionDto.Currency.ToUpper(),
                    ProcessedAt = createTransactionDto.TransactionDate,
                    TransactionDate = createTransactionDto.TransactionDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Update account balance
                if (payment.TransactionType == "CREDIT")
                {
                    bankAccount.CurrentBalance += payment.Amount;
                }
                else if (payment.TransactionType == "DEBIT")
                {
                    bankAccount.CurrentBalance -= payment.Amount;
                }

                payment.BalanceAfterTransaction = bankAccount.CurrentBalance;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                _context.Payments.Add(payment);

                // Also create BankTransaction record
                var bankTransaction = new Entities.BankTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    BankAccountId = createTransactionDto.BankAccountId,
                    UserId = userId,
                    Amount = createTransactionDto.Amount,
                    TransactionType = createTransactionDto.TransactionType.ToUpper(),
                    Description = createTransactionDto.Description,
                    Category = createTransactionDto.Category,
                    ReferenceNumber = createTransactionDto.ReferenceNumber ?? $"BANK_TXN_{Guid.NewGuid()}",
                    ExternalTransactionId = createTransactionDto.ExternalTransactionId,
                    Notes = createTransactionDto.Notes,
                    Merchant = createTransactionDto.Merchant,
                    Location = createTransactionDto.Location,
                    IsRecurring = createTransactionDto.IsRecurring,
                    RecurringFrequency = createTransactionDto.RecurringFrequency,
                    Currency = createTransactionDto.Currency.ToUpper(),
                    BalanceAfterTransaction = bankAccount.CurrentBalance,
                    TransactionDate = createTransactionDto.TransactionDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BankTransactions.Add(bankTransaction);
                await _context.SaveChangesAsync();

                var transactionDto = MapPaymentToBankTransactionDto(payment, bankAccount.AccountName);
                return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto, "Transaction created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankTransactionDto>.ErrorResult($"Failed to create transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetAccountTransactionsAsync(string bankAccountId, string userId, int page = 1, int limit = 50, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<List<BankTransactionDto>>.ErrorResult("Bank account not found");
                }

                // Store account name since all transactions are for this account
                var accountName = bankAccount.AccountName;

                var query = _context.Payments
                    .Where(p => p.BankAccountId == bankAccountId && p.UserId == userId && p.IsBankTransaction);

                // Add date filtering
                if (dateFrom.HasValue)
                {
                    query = query.Where(p => (p.TransactionDate ?? p.ProcessedAt) >= dateFrom.Value.Date);
                }

                if (dateTo.HasValue)
                {
                    // Include the entire end date (up to end of day)
                    var endDate = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(p => (p.TransactionDate ?? p.ProcessedAt) <= endDate);
                }

                var payments = await query
                    .OrderByDescending(p => p.TransactionDate ?? p.ProcessedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = payments.Select(p => MapPaymentToBankTransactionDto(p, accountName)).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get account transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetUserTransactionsAsync(string userId, string? accountType = null, int page = 1, int limit = 50, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                // Query from Payments table (primary source for bank transactions)
                IQueryable<Entities.Payment> paymentsQuery = _context.Payments
                    .Include(p => p.BankAccount)
                    .Where(p => p.UserId == userId && p.IsBankTransaction && p.BankAccountId != null && p.BankAccountId != string.Empty);

                // Filter by account type if specified
                if (!string.IsNullOrEmpty(accountType))
                {
                    var accountTypeLower = accountType.ToLower();
                    paymentsQuery = from payment in paymentsQuery
                                   join bankAccount in _context.BankAccounts on payment.BankAccountId equals bankAccount.Id
                                   where bankAccount.UserId == userId && bankAccount.AccountType == accountTypeLower
                                   select payment;
                }

                // Add date filtering - extract dates before using in LINQ
                DateTime? filterDateFrom = null;
                DateTime? filterDateTo = null;
                
                if (dateFrom.HasValue)
                {
                    filterDateFrom = dateFrom.Value.Date;
                }

                if (dateTo.HasValue)
                {
                    // Include the entire end date (up to end of day)
                    filterDateTo = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                }

                if (filterDateFrom.HasValue)
                {
                    var dateFromValue = filterDateFrom.Value;
                    paymentsQuery = paymentsQuery.Where(p => (p.TransactionDate ?? p.ProcessedAt) >= dateFromValue);
                }

                if (filterDateTo.HasValue)
                {
                    var dateToValue = filterDateTo.Value;
                    paymentsQuery = paymentsQuery.Where(p => (p.TransactionDate ?? p.ProcessedAt) <= dateToValue);
                }

                var payments = await paymentsQuery
                    .OrderByDescending(p => p.TransactionDate ?? p.ProcessedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Get all unique bank account IDs and fetch their names
                var bankAccountIds = payments
                    .Where(p => !string.IsNullOrEmpty(p.BankAccountId))
                    .Select(p => p.BankAccountId)
                    .Distinct()
                    .ToList();

                var bankAccounts = await _context.BankAccounts
                    .Where(ba => bankAccountIds.Contains(ba.Id))
                    .ToDictionaryAsync(ba => ba.Id, ba => ba.AccountName);

                // Map to DTOs with account names
                var transactionDtos = new List<BankTransactionDto>();
                foreach (var payment in payments)
                {
                    var accountName = !string.IsNullOrEmpty(payment.BankAccountId) && bankAccounts.ContainsKey(payment.BankAccountId) 
                        ? bankAccounts[payment.BankAccountId] 
                        : null;
                    transactionDtos.Add(MapPaymentToBankTransactionDto(payment, accountName));
                }

                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get user transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankTransactionDto>> GetTransactionAsync(string transactionId, string userId)
        {
            try
            {
                // Try to find in BankTransactions first
                var bankTransaction = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

                if (bankTransaction != null)
                {
                    var bankTransactionDto = MapToBankTransactionDto(bankTransaction, bankTransaction.BankAccount?.AccountName);
                    return ApiResponse<BankTransactionDto>.SuccessResult(bankTransactionDto);
                }

                // If not found in BankTransactions, check Payments table (for transactions created via Payments API)
                var payment = await _context.Payments
                    .Include(p => p.BankAccount)
                    .FirstOrDefaultAsync(p => p.Id == transactionId && p.UserId == userId && p.IsBankTransaction);

                if (payment != null)
                {
                    var paymentTransactionDto = MapPaymentToBankTransactionDto(payment, payment.BankAccount?.AccountName);
                    return ApiResponse<BankTransactionDto>.SuccessResult(paymentTransactionDto);
                }

                return ApiResponse<BankTransactionDto>.ErrorResult("Transaction not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankTransactionDto>.ErrorResult($"Failed to get transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountAnalyticsDto>> GetTransactionAnalyticsAsync(string userId, string period = "month")
        {
            try
            {
                var (startDate, endDate) = GetPeriodDates(period);

                var transactions = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .Where(t => t.UserId == userId && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate)
                    .ToListAsync();

                var bankAccounts = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                var analytics = new BankAccountAnalyticsDto
                {
                    TotalBalance = bankAccounts.Sum(ba => ba.CurrentBalance),
                    TotalIncoming = transactions.Where(t => t.TransactionType == "CREDIT").Sum(t => t.Amount),
                    TotalOutgoing = transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount),
                    TotalTransactions = transactions.Count,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TopAccounts = new List<BankAccountDto>(),
                    SpendingByCategory = transactions
                        .Where(t => t.TransactionType == "DEBIT" && !string.IsNullOrEmpty(t.Category))
                        .GroupBy(t => t.Category!)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount))
                };

                // Get top 5 accounts by balance
                var topAccounts = bankAccounts
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .Take(5)
                    .ToList();

                foreach (var account in topAccounts)
                {
                    analytics.TopAccounts.Add(await MapToBankAccountDtoAsync(account));
                }

                return ApiResponse<BankAccountAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountAnalyticsDto>.ErrorResult($"Failed to get transaction analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetRecentTransactionsAsync(string userId, int limit = 10)
        {
            try
            {
                var transactions = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = transactions.Select(t => MapToBankTransactionDto(t, t.BankAccount?.AccountName)).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get recent transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetSpendingByCategoryAsync(string userId, string period = "month")
        {
            try
            {
                var (startDate, endDate) = GetPeriodDates(period);

                var spendingByCategory = await _context.BankTransactions
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate &&
                               !string.IsNullOrEmpty(t.Category))
                    .GroupBy(t => t.Category!)
                    .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                    .ToDictionaryAsync(x => x.Category, x => x.Amount);

                return ApiResponse<Dictionary<string, decimal>>.SuccessResult(spendingByCategory);
            }
            catch (Exception ex)
            {
                return ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get spending by category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateAccountBalanceAsync(string bankAccountId, decimal newBalance, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bank account not found");
                }

                bankAccount.CurrentBalance = newBalance;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Account balance updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to update account balance: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountDto>> ArchiveBankAccountAsync(string bankAccountId, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account not found");
                }

                bankAccount.IsActive = false;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto, "Bank account archived successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to archive bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountDto>> ActivateBankAccountAsync(string bankAccountId, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account not found");
                }

                bankAccount.IsActive = true;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto, "Bank account activated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to activate bank account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankAccountDto>>> GetAllBankAccountsAsync(int page = 1, int limit = 50)
        {
            try
            {
                var bankAccounts = await _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .OrderByDescending(ba => ba.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var bankAccountDtos = new List<BankAccountDto>();
                foreach (var account in bankAccounts)
                {
                    bankAccountDtos.Add(await MapToBankAccountDtoAsync(account));
                }

                return ApiResponse<List<BankAccountDto>>.SuccessResult(bankAccountDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get all bank accounts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetAllTransactionsAsync(int page = 1, int limit = 50)
        {
            try
            {
                var transactions = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = transactions.Select(t => MapToBankTransactionDto(t, t.BankAccount?.AccountName)).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get all transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankTransactionDto>> CreateExpenseAsync(CreateExpenseDto expenseDto, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == expenseDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult("Bank account not found");
                }

                var transaction = new BankTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    BankAccountId = expenseDto.BankAccountId,
                    UserId = userId,
                    Amount = expenseDto.Amount,
                    TransactionType = "DEBIT",
                    Description = expenseDto.Description,
                    Category = expenseDto.Category,
                    TransactionDate = expenseDto.TransactionDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Notes = expenseDto.Notes,
                    Merchant = expenseDto.Merchant,
                    Location = expenseDto.Location,
                    IsRecurring = expenseDto.IsRecurring,
                    RecurringFrequency = expenseDto.RecurringFrequency,
                    Currency = expenseDto.Currency.ToUpper()
                };

                // Update account balance
                bankAccount.CurrentBalance -= transaction.Amount;
                transaction.BalanceAfterTransaction = bankAccount.CurrentBalance;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                _context.BankTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                var transactionDto = MapToBankTransactionDto(transaction);
                return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto, "Expense recorded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankTransactionDto>.ErrorResult($"Failed to create expense: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseAnalyticsDto>> GetExpenseAnalyticsAsync(string userId, string period = "month")
        {
            try
            {
                var (startDate, endDate) = GetPeriodDates(period);

                var expenses = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate)
                    .ToListAsync();

                var analytics = new ExpenseAnalyticsDto
                {
                    TotalExpenses = expenses.Sum(t => t.Amount),
                    TotalTransactions = expenses.Count,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    SpendingByCategory = expenses
                        .Where(t => !string.IsNullOrEmpty(t.Category))
                        .GroupBy(t => t.Category!)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
                    SpendingByMerchant = expenses
                        .Where(t => !string.IsNullOrEmpty(t.Merchant))
                        .GroupBy(t => t.Merchant!)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
                    RecentExpenses = expenses
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(10)
                        .Select(t => MapToBankTransactionDto(t, t.BankAccount?.AccountName))
                        .ToList(),
                    AverageDailySpending = expenses.Count > 0 ? expenses.Sum(t => t.Amount) / (decimal)(endDate - startDate).TotalDays : 0,
                    AverageTransactionAmount = expenses.Count > 0 ? expenses.Average(t => t.Amount) : 0
                };

                return ApiResponse<ExpenseAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseAnalyticsDto>.ErrorResult($"Failed to get expense analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseSummaryDto>> GetExpenseSummaryAsync(string userId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                var lastMonthStart = thisMonthStart.AddMonths(-1);
                var lastMonthEnd = thisMonthStart.AddDays(-1);

                var todayExpenses = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT" && t.TransactionDate.Date == today)
                    .SumAsync(t => t.Amount);

                var thisWeekExpenses = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT" && t.TransactionDate >= thisWeekStart)
                    .SumAsync(t => t.Amount);

                var thisMonthExpenses = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT" && t.TransactionDate >= thisMonthStart)
                    .SumAsync(t => t.Amount);

                var lastMonthExpenses = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT" && 
                               t.TransactionDate >= lastMonthStart && t.TransactionDate <= lastMonthEnd)
                    .SumAsync(t => t.Amount);

                var topCategories = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT" && t.TransactionDate >= thisMonthStart)
                    .GroupBy(t => t.Category!)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                    .OrderByDescending(x => x.Amount)
                    .Take(5)
                    .ToDictionaryAsync(x => x.Category, x => x.Amount);

                var recentExpensesData = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT")
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .ToListAsync();

                var recentExpenses = recentExpensesData.Select(t => MapToBankTransactionDto(t, t.BankAccount?.AccountName)).ToList();

                var summary = new ExpenseSummaryDto
                {
                    TodayExpenses = todayExpenses,
                    ThisWeekExpenses = thisWeekExpenses,
                    ThisMonthExpenses = thisMonthExpenses,
                    LastMonthExpenses = lastMonthExpenses,
                    TopCategories = topCategories,
                    RecentExpenses = recentExpenses
                };

                return ApiResponse<ExpenseSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseSummaryDto>.ErrorResult($"Failed to get expense summary: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetExpensesByCategoryAsync(string userId, string category, int page = 1, int limit = 50)
        {
            try
            {
                var expenses = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               t.Category == category)
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var expenseDtos = expenses.Select(t => MapToBankTransactionDto(t, t.BankAccount?.AccountName)).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(expenseDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get expenses by category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetExpenseCategoriesAsync(string userId)
        {
            try
            {
                var categories = await _context.BankTransactions
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               !string.IsNullOrEmpty(t.Category))
                    .GroupBy(t => t.Category!)
                    .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                    .ToDictionaryAsync(x => x.Category, x => x.Amount);

                return ApiResponse<Dictionary<string, decimal>>.SuccessResult(categories);
            }
            catch (Exception ex)
            {
                return ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get expense categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteTransactionAsync(string transactionId, string userId)
        {
            try
            {
                // Find the transaction and verify it belongs to the user
                var transaction = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

                if (transaction == null)
                {
                    return ApiResponse<bool>.ErrorResult("Transaction not found or you don't have permission to delete it");
                }

                // Check if transaction can be deleted based on business rules
                var hoursSinceCreation = (DateTime.UtcNow - transaction.CreatedAt).TotalHours;
                if (hoursSinceCreation > 24)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete transactions older than 24 hours");
                }

                // Check if the bank account is still active
                if (!transaction.BankAccount.IsActive)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete transactions for inactive bank accounts");
                }

                // Reverse the transaction effect on the bank account balance
                if (transaction.TransactionType == "CREDIT")
                {
                    // If it was a credit (money in), subtract it from balance
                    transaction.BankAccount.CurrentBalance -= transaction.Amount;
                }
                else if (transaction.TransactionType == "DEBIT")
                {
                    // If it was a debit (money out), add it back to balance
                    transaction.BankAccount.CurrentBalance += transaction.Amount;
                }

                // Update the bank account's updated timestamp
                transaction.BankAccount.UpdatedAt = DateTime.UtcNow;

                // Remove the transaction
                _context.BankTransactions.Remove(transaction);

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Transaction deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete transaction: {ex.Message}");
            }
        }

        // Helper Methods
        private async Task<BankAccountDto> MapToBankAccountDtoAsync(BankAccount bankAccount)
        {
            var transactions = bankAccount.Transactions ?? new List<BankTransaction>();
            
            return new BankAccountDto
            {
                Id = bankAccount.Id,
                UserId = bankAccount.UserId,
                AccountName = bankAccount.AccountName,
                AccountType = bankAccount.AccountType,
                InitialBalance = bankAccount.InitialBalance,
                CurrentBalance = bankAccount.CurrentBalance,
                Currency = bankAccount.Currency,
                Description = bankAccount.Description,
                FinancialInstitution = bankAccount.FinancialInstitution,
                AccountNumber = bankAccount.AccountNumber,
                RoutingNumber = bankAccount.RoutingNumber,
                SyncFrequency = bankAccount.SyncFrequency,
                IsConnected = bankAccount.IsConnected,
                ConnectionId = bankAccount.ConnectionId,
                LastSyncedAt = bankAccount.LastSyncedAt,
                CreatedAt = bankAccount.CreatedAt,
                UpdatedAt = bankAccount.UpdatedAt,
                IsActive = bankAccount.IsActive,
                Iban = bankAccount.Iban,
                SwiftCode = bankAccount.SwiftCode,
                TransactionCount = transactions.Count,
                TotalIncoming = transactions.Where(t => t.TransactionType == "CREDIT").Sum(t => t.Amount),
                TotalOutgoing = transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount)
            };
        }

        private static BankTransactionDto MapToBankTransactionDto(BankTransaction transaction, string? accountName = null)
        {
            return new BankTransactionDto
            {
                Id = transaction.Id,
                BankAccountId = transaction.BankAccountId,
                AccountName = accountName ?? transaction.BankAccount?.AccountName,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType,
                Description = transaction.Description,
                Category = transaction.Category,
                ReferenceNumber = transaction.ReferenceNumber,
                ExternalTransactionId = transaction.ExternalTransactionId,
                TransactionDate = transaction.TransactionDate,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                Notes = transaction.Notes,
                Merchant = transaction.Merchant,
                Location = transaction.Location,
                IsRecurring = transaction.IsRecurring,
                RecurringFrequency = transaction.RecurringFrequency,
                Currency = transaction.Currency,
                BalanceAfterTransaction = transaction.BalanceAfterTransaction
            };
        }

        private static BankTransactionDto MapPaymentToBankTransactionDto(Entities.Payment payment, string? accountName = null)
        {
            return new BankTransactionDto
            {
                Id = payment.Id,
                BankAccountId = payment.BankAccountId ?? string.Empty,
                AccountName = accountName ?? payment.BankAccount?.AccountName,
                UserId = payment.UserId,
                Amount = payment.Amount,
                TransactionType = payment.TransactionType ?? "UNKNOWN",
                Description = payment.Description ?? "",
                Category = payment.Category,
                ReferenceNumber = payment.Reference,
                ExternalTransactionId = payment.ExternalTransactionId,
                TransactionDate = payment.TransactionDate ?? payment.ProcessedAt,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                Notes = payment.Notes,
                Merchant = payment.Merchant,
                Location = payment.Location,
                IsRecurring = payment.IsRecurring,
                RecurringFrequency = payment.RecurringFrequency,
                Currency = payment.Currency,
                BalanceAfterTransaction = payment.BalanceAfterTransaction ?? 0
            };
        }

        private static (DateTime startDate, DateTime endDate) GetPeriodDates(string period)
        {
            var now = DateTime.UtcNow;
            
            return period.ToLower() switch
            {
                "weekly" or "week" => (now.AddDays(-7).Date, now.Date),
                "monthly" or "month" => (now.AddDays(-30).Date, now.Date),
                "quarterly" or "quarter" => (now.AddDays(-90).Date, now.Date),
                "yearly" or "year" => (now.AddDays(-365).Date, now.Date),
                _ => (now.AddDays(-30).Date, now.Date) // Default to month
            };
        }
    }
}
