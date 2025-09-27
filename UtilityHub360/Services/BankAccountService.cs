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

        public async Task<ApiResponse<BankAccountSummaryDto>> GetBankAccountSummaryAsync(string userId, string frequency = "monthly")
        {
            try
            {
                var bankAccounts = await _context.BankAccounts
                    .Include(ba => ba.Transactions)
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

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
                        PeriodStart = DateTime.UtcNow,
                        PeriodEnd = DateTime.UtcNow,
                        TransactionCount = 0,
                        Accounts = new List<BankAccountDto>(),
                        SpendingByCategory = new Dictionary<string, decimal>()
                    });
                }

                // Calculate period dates based on frequency
                var (periodStart, periodEnd) = GetPeriodDates(frequency);

                // Get all transactions for the period (now from Payments table)
                var allTransactions = await _context.Payments
                    .Where(p => p.UserId == userId && p.IsBankTransaction && 
                               p.TransactionDate >= periodStart && p.TransactionDate <= periodEnd)
                    .ToListAsync();

                // Get current month transactions
                var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
                var currentMonthTransactions = await _context.Payments
                    .Where(p => p.UserId == userId && p.IsBankTransaction && 
                               p.TransactionDate >= currentMonthStart && p.TransactionDate <= currentMonthEnd)
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
                    CurrentMonthIncoming = currentMonthTransactions
                        .Where(t => t.TransactionType == "CREDIT")
                        .Sum(t => t.Amount),
                    CurrentMonthOutgoing = currentMonthTransactions
                        .Where(t => t.TransactionType == "DEBIT")
                        .Sum(t => t.Amount),
                    CurrentMonthNet = currentMonthTransactions
                        .Where(t => t.TransactionType == "CREDIT")
                        .Sum(t => t.Amount) - currentMonthTransactions
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

                // Create transaction as Payment with IsBankTransaction = true
                var payment = new Entities.Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    BankAccountId = createTransactionDto.BankAccountId,
                    UserId = userId,
                    Amount = createTransactionDto.Amount,
                    Method = "BANK_TRANSFER",
                    Reference = createTransactionDto.ReferenceNumber ?? $"BANK_TXN_{Guid.NewGuid()}",
                    Status = "COMPLETED",
                    IsBankTransaction = true,
                    TransactionType = createTransactionDto.TransactionType.ToUpper(),
                    Description = createTransactionDto.Description,
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

                var transactionDto = MapPaymentToBankTransactionDto(payment);
                return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto, "Transaction created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankTransactionDto>.ErrorResult($"Failed to create transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetAccountTransactionsAsync(string bankAccountId, string userId, int page = 1, int limit = 50)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<List<BankTransactionDto>>.ErrorResult("Bank account not found");
                }

                var payments = await _context.Payments
                    .Where(p => p.BankAccountId == bankAccountId && p.UserId == userId && p.IsBankTransaction)
                    .OrderByDescending(p => p.TransactionDate ?? p.ProcessedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = payments.Select(MapPaymentToBankTransactionDto).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get account transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetUserTransactionsAsync(string userId, string? accountType = null, int page = 1, int limit = 50)
        {
            try
            {
                var query = _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .Where(t => t.UserId == userId);

                if (!string.IsNullOrEmpty(accountType))
                {
                    query = query.Where(t => t.BankAccount.AccountType == accountType.ToLower());
                }

                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = transactions.Select(MapToBankTransactionDto).ToList();
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
                var transaction = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

                if (transaction == null)
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult("Transaction not found");
                }

                var transactionDto = MapToBankTransactionDto(transaction);
                return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto);
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

                var transactionDtos = transactions.Select(MapToBankTransactionDto).ToList();
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

                var transactionDtos = transactions.Select(MapToBankTransactionDto).ToList();
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
                        .Select(MapToBankTransactionDto)
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

                var recentExpenses = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT")
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .Select(t => MapToBankTransactionDto(t))
                    .ToListAsync();

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
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               t.Category == category)
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var expenseDtos = expenses.Select(MapToBankTransactionDto).ToList();
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

        private static BankTransactionDto MapToBankTransactionDto(BankTransaction transaction)
        {
            return new BankTransactionDto
            {
                Id = transaction.Id,
                BankAccountId = transaction.BankAccountId,
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

        private static BankTransactionDto MapPaymentToBankTransactionDto(Entities.Payment payment)
        {
            return new BankTransactionDto
            {
                Id = payment.Id,
                BankAccountId = payment.BankAccountId,
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
