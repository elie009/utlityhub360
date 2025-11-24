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
        private readonly IServiceProvider _serviceProvider;
        private readonly AccountingService _accountingService;
        private readonly ITransactionRulesService? _transactionRulesService;
        private readonly IDuplicateDetectionService? _duplicateDetectionService;
        private readonly ISmartCategorizationService? _smartCategorizationService;

        public BankAccountService(
            ApplicationDbContext context, 
            IServiceProvider serviceProvider, 
            AccountingService accountingService,
            ITransactionRulesService? transactionRulesService = null,
            IDuplicateDetectionService? duplicateDetectionService = null,
            ISmartCategorizationService? smartCategorizationService = null)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _accountingService = accountingService;
            _transactionRulesService = transactionRulesService;
            _duplicateDetectionService = duplicateDetectionService;
            _smartCategorizationService = smartCategorizationService;
        }

        public async Task<ApiResponse<BankAccountDto>> CreateBankAccountAsync(CreateBankAccountDto createBankAccountDto, string userId)
        {
            try
            {
                // Check for duplicate account name
                var existingAccountByName = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.UserId == userId && ba.AccountName == createBankAccountDto.AccountName);
                
                if (existingAccountByName != null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult($"An account with the name '{createBankAccountDto.AccountName}' already exists. Please use a different account name.");
                }

                // Check for duplicate account number (if provided and not empty)
                if (!string.IsNullOrWhiteSpace(createBankAccountDto.AccountNumber))
                {
                    var existingAccountByNumber = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.UserId == userId && ba.AccountNumber == createBankAccountDto.AccountNumber);
                    
                    if (existingAccountByNumber != null)
                    {
                        return ApiResponse<BankAccountDto>.ErrorResult($"An account with this account number already exists.");
                    }
                }

                // Convert empty strings to null for optional fields to avoid unique constraint issues
                var accountNumber = string.IsNullOrWhiteSpace(createBankAccountDto.AccountNumber) ? null : createBankAccountDto.AccountNumber;
                var routingNumber = string.IsNullOrWhiteSpace(createBankAccountDto.RoutingNumber) ? null : createBankAccountDto.RoutingNumber;
                var description = string.IsNullOrWhiteSpace(createBankAccountDto.Description) ? null : createBankAccountDto.Description;
                var financialInstitution = string.IsNullOrWhiteSpace(createBankAccountDto.FinancialInstitution) ? null : createBankAccountDto.FinancialInstitution;
                var iban = string.IsNullOrWhiteSpace(createBankAccountDto.Iban) ? null : createBankAccountDto.Iban;
                var swiftCode = string.IsNullOrWhiteSpace(createBankAccountDto.SwiftCode) ? null : createBankAccountDto.SwiftCode;

                var bankAccount = new BankAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    AccountName = createBankAccountDto.AccountName,
                    AccountType = createBankAccountDto.AccountType.ToLower(),
                    InitialBalance = createBankAccountDto.InitialBalance,
                    CurrentBalance = createBankAccountDto.InitialBalance,
                    Currency = createBankAccountDto.Currency.ToUpper(),
                    Description = description,
                    FinancialInstitution = financialInstitution,
                    AccountNumber = accountNumber,
                    RoutingNumber = routingNumber,
                    SyncFrequency = createBankAccountDto.SyncFrequency.ToUpper(),
                    Iban = iban,
                    SwiftCode = swiftCode,
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
            catch (DbUpdateException dbEx)
            {
                // Handle database constraint violations
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                
                if (innerException.Contains("IX_BankAccounts_UserId_AccountName") || innerException.Contains("duplicate key"))
                {
                    return ApiResponse<BankAccountDto>.ErrorResult($"An account with the name '{createBankAccountDto.AccountName}' already exists. Please use a different account name.");
                }
                
                if (innerException.Contains("IX_BankAccounts_UserId_AccountNumber") || innerException.Contains("duplicate key"))
                {
                    return ApiResponse<BankAccountDto>.ErrorResult($"An account with this account number already exists.");
                }
                
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to create bank account: {innerException}");
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

                // Check for duplicate account name (excluding current account)
                if (!string.IsNullOrEmpty(updateBankAccountDto.AccountName) && 
                    updateBankAccountDto.AccountName != bankAccount.AccountName)
                {
                    var existingAccountByName = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.UserId == userId && 
                                                  ba.AccountName == updateBankAccountDto.AccountName &&
                                                  ba.Id != bankAccountId);
                    
                    if (existingAccountByName != null)
                    {
                        return ApiResponse<BankAccountDto>.ErrorResult($"An account with the name '{updateBankAccountDto.AccountName}' already exists. Please use a different account name.");
                    }
                }

                // Convert empty strings to null for optional fields to avoid unique constraint issues
                string? accountNumber = null;
                if (updateBankAccountDto.AccountNumber != null)
                {
                    accountNumber = string.IsNullOrWhiteSpace(updateBankAccountDto.AccountNumber) ? null : updateBankAccountDto.AccountNumber;
                }

                // Check for duplicate account number (excluding current account, only if not null/empty)
                if (accountNumber != null && accountNumber != bankAccount.AccountNumber)
                {
                    var existingAccountByNumber = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.UserId == userId && 
                                                  ba.AccountNumber == accountNumber &&
                                                  ba.Id != bankAccountId);
                    
                    if (existingAccountByNumber != null)
                    {
                        return ApiResponse<BankAccountDto>.ErrorResult($"An account with this account number already exists.");
                    }
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

                // Update optional fields if provided, converting empty strings to null
                if (updateBankAccountDto.Description != null)
                    bankAccount.Description = string.IsNullOrWhiteSpace(updateBankAccountDto.Description) ? null : updateBankAccountDto.Description;

                if (updateBankAccountDto.FinancialInstitution != null)
                    bankAccount.FinancialInstitution = string.IsNullOrWhiteSpace(updateBankAccountDto.FinancialInstitution) ? null : updateBankAccountDto.FinancialInstitution;

                if (updateBankAccountDto.AccountNumber != null)
                    bankAccount.AccountNumber = accountNumber;

                if (updateBankAccountDto.RoutingNumber != null)
                    bankAccount.RoutingNumber = string.IsNullOrWhiteSpace(updateBankAccountDto.RoutingNumber) ? null : updateBankAccountDto.RoutingNumber;

                if (updateBankAccountDto.Iban != null)
                    bankAccount.Iban = string.IsNullOrWhiteSpace(updateBankAccountDto.Iban) ? null : updateBankAccountDto.Iban;

                if (updateBankAccountDto.SwiftCode != null)
                    bankAccount.SwiftCode = string.IsNullOrWhiteSpace(updateBankAccountDto.SwiftCode) ? null : updateBankAccountDto.SwiftCode;

                if (!string.IsNullOrEmpty(updateBankAccountDto.SyncFrequency))
                    bankAccount.SyncFrequency = updateBankAccountDto.SyncFrequency.ToUpper();

                if (updateBankAccountDto.IsActive.HasValue)
                    bankAccount.IsActive = updateBankAccountDto.IsActive.Value;

                bankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var bankAccountDto = await MapToBankAccountDtoAsync(bankAccount);
                return ApiResponse<BankAccountDto>.SuccessResult(bankAccountDto, "Bank account updated successfully");
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database constraint violations
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                
                if (innerException.Contains("IX_BankAccounts_UserId_AccountName") || innerException.Contains("duplicate key"))
                {
                    return ApiResponse<BankAccountDto>.ErrorResult($"An account with the name '{updateBankAccountDto.AccountName}' already exists. Please use a different account name.");
                }
                
                if (innerException.Contains("IX_BankAccounts_UserId_AccountNumber") || innerException.Contains("duplicate key"))
                {
                    return ApiResponse<BankAccountDto>.ErrorResult($"An account with this account number already exists.");
                }
                
                return ApiResponse<BankAccountDto>.ErrorResult($"Failed to update bank account: {innerException}");
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
                        TotalRemainingCreditLimit = null,
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

                // Calculate total balance excluding credit cards (they represent debt, not assets)
                var totalBalance = bankAccounts
                    .Where(ba => {
                        var accountTypeLower = ba.AccountType?.ToLower().Trim() ?? "";
                        return accountTypeLower != "credit_card" && 
                               accountTypeLower != "credit card" && 
                               accountTypeLower != "creditcard";
                    })
                    .Sum(ba => ba.CurrentBalance);

                // Calculate total remaining credit limit for credit cards
                // Remaining credit = Credit Limit (InitialBalance) - Current Balance (debt)
                var creditCardAccounts = bankAccounts
                    .Where(ba => {
                        var accountTypeLower = ba.AccountType?.ToLower().Trim() ?? "";
                        return accountTypeLower == "credit_card" || 
                               accountTypeLower == "credit card" || 
                               accountTypeLower == "creditcard";
                    })
                    .ToList();

                decimal? totalRemainingCreditLimit = null;
                if (creditCardAccounts.Any())
                {
                    totalRemainingCreditLimit = creditCardAccounts
                        .Sum(ba => Math.Max(0, ba.InitialBalance - ba.CurrentBalance));
                }

                var summary = new BankAccountSummaryDto
                {
                    TotalBalance = totalBalance,
                    TotalRemainingCreditLimit = totalRemainingCreditLimit,
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
                // Exclude credit cards from total balance as they represent debt, not assets
                // Inline the check so EF Core can translate it to SQL
                var totalBalance = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && 
                               ba.IsActive && 
                               ba.AccountType.ToLower() != "credit_card" &&
                               ba.AccountType.ToLower() != "credit card" &&
                               ba.AccountType.ToLower() != "creditcard")
                    .SumAsync(ba => ba.CurrentBalance);

                return ApiResponse<decimal>.SuccessResult(totalBalance);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total balance: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalDebtAsync(string userId)
        {
            try
            {
                // Calculate total debt from credit card accounts
                // Inline the check so EF Core can translate it to SQL
                var creditCardDebt = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && 
                               ba.IsActive && 
                               (ba.AccountType.ToLower() == "credit_card" ||
                                ba.AccountType.ToLower() == "credit card" ||
                                ba.AccountType.ToLower() == "creditcard"))
                    .SumAsync(ba => ba.CurrentBalance);

                return ApiResponse<decimal>.SuccessResult(creditCardDebt);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total debt: {ex.Message}");
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

                // ==================== CATEGORY VALIDATION ====================
                // Validate category if provided (skip validation for special categories like [SAVINGS-...] or [LOAN-...])
                if (!string.IsNullOrEmpty(createTransactionDto.Category) && 
                    !createTransactionDto.Category.StartsWith("[") && 
                    createTransactionDto.TransactionType?.ToUpper() != "CREDIT")
                {
                    // Check if category exists in TransactionCategories table
                    var categoryExists = await _context.TransactionCategories
                        .AnyAsync(c => c.UserId == userId && 
                                     c.Name.ToUpper() == createTransactionDto.Category.ToUpper() && 
                                     c.IsActive && 
                                     !c.IsDeleted);

                    if (!categoryExists)
                    {
                        // Try to find a similar category (case-insensitive)
                        var similarCategory = await _context.TransactionCategories
                            .FirstOrDefaultAsync(c => c.UserId == userId && 
                                                     c.Name.ToUpper() == createTransactionDto.Category.ToUpper() && 
                                                     !c.IsDeleted);

                        if (similarCategory != null && !similarCategory.IsActive)
                        {
                            return ApiResponse<BankTransactionDto>.ErrorResult(
                                $"Category '{createTransactionDto.Category}' exists but is inactive. Please activate it or choose a different category.");
                        }

                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            $"Category '{createTransactionDto.Category}' not found. Please create the category first or use an existing category.");
                    }

                    // Validate category type matches transaction type
                    var category = await _context.TransactionCategories
                        .FirstOrDefaultAsync(c => c.UserId == userId && 
                                                 c.Name.ToUpper() == createTransactionDto.Category.ToUpper() && 
                                                 c.IsActive && 
                                                 !c.IsDeleted);

                    if (category != null)
                    {
                        var transactionType = createTransactionDto.TransactionType?.ToUpper();
                        var categoryType = category.Type?.ToUpper();

                        // Validate type compatibility
                        if (transactionType == "DEBIT" && categoryType == "INCOME")
                        {
                            return ApiResponse<BankTransactionDto>.ErrorResult(
                                $"Category '{category.Name}' is an income category and cannot be used for debit transactions.");
                        }
                        if (transactionType == "CREDIT" && (categoryType == "EXPENSE" || categoryType == "BILL" || categoryType == "LOAN"))
                        {
                            return ApiResponse<BankTransactionDto>.ErrorResult(
                                $"Category '{category.Name}' is an expense category and cannot be used for credit transactions.");
                        }
                    }
                }
                // ==================== END CATEGORY VALIDATION ====================

                // ==================== DOUBLE-ENTRY VALIDATION FOR TRANSFERS ====================
                // Detect if this is a bank transfer transaction
                var categoryLower = (createTransactionDto.Category ?? "").ToLower();
                bool isBankTransfer = !string.IsNullOrEmpty(createTransactionDto.ToBankAccountId) ||
                                    categoryLower.Contains("transfer") || 
                                    categoryLower == "transfer" ||
                                    (!string.IsNullOrEmpty(createTransactionDto.Category) && 
                                     createTransactionDto.Category.ToUpper() == "TRANSFER");

                // If it's a transfer, validate double-entry accounting rules
                if (isBankTransfer)
                {
                    // Validation 1: ToBankAccountId is required for transfers
                    if (string.IsNullOrEmpty(createTransactionDto.ToBankAccountId))
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            "Destination bank account (ToBankAccountId) is required for bank transfer transactions");
                    }

                    // Validation 2: Source and destination accounts must be different
                    if (createTransactionDto.BankAccountId == createTransactionDto.ToBankAccountId)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            "Source and destination accounts cannot be the same for bank transfers. Double-entry accounting requires two different accounts.");
                    }

                    // Validation 3: Destination account must exist and belong to user
                    var destinationAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == createTransactionDto.ToBankAccountId && ba.UserId == userId);

                    if (destinationAccount == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            "Destination bank account not found or does not belong to user");
                    }

                    // Validation 4: Amount must be positive for transfers
                    if (createTransactionDto.Amount <= 0)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            "Transfer amount must be greater than zero");
                    }

                    // Validation 5: For DEBIT transfers, ensure source account has sufficient balance
                    if (createTransactionDto.TransactionType?.ToUpper() == "DEBIT")
                    {
                        bool isSourceCreditCard = bankAccount.AccountType?.ToLower() == "credit_card";
                        
                        // For credit cards, check available credit (balance can be negative)
                        // For regular accounts, check if balance is sufficient
                        if (!isSourceCreditCard && bankAccount.CurrentBalance < createTransactionDto.Amount)
                        {
                            return ApiResponse<BankTransactionDto>.ErrorResult(
                                $"Insufficient balance in source account. Current balance: {bankAccount.CurrentBalance:C}, Required: {createTransactionDto.Amount:C}");
                        }
                    }
                }
                // ==================== END DOUBLE-ENTRY VALIDATION ====================

                // ==================== AUTOMATION FEATURES ====================
                // Convert to CreateTransactionRequest for automation services
                var transactionRequest = new CreateTransactionRequest
                {
                    BankAccountId = createTransactionDto.BankAccountId,
                    TransactionType = createTransactionDto.TransactionType ?? "DEBIT",
                    Amount = createTransactionDto.Amount,
                    Description = createTransactionDto.Description ?? "",
                    Category = createTransactionDto.Category,
                    MerchantName = createTransactionDto.Merchant,
                    TransactionDate = createTransactionDto.TransactionDate,
                    ReferenceNumber = createTransactionDto.ReferenceNumber
                };

                // 1. Check for duplicates
                if (_duplicateDetectionService != null)
                {
                    var duplicateCheck = await _duplicateDetectionService.CheckDuplicateAsync(transactionRequest, userId);
                    if (duplicateCheck.IsDuplicate && duplicateCheck.Confidence > 0.9)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            $"This transaction appears to be a duplicate of an existing transaction. Confidence: {duplicateCheck.Confidence:P0}. {duplicateCheck.Reason}");
                    }
                }

                // 2. Apply smart categorization if category not provided
                if (string.IsNullOrEmpty(createTransactionDto.Category) && 
                    createTransactionDto.TransactionType?.ToUpper() != "CREDIT" &&
                    _smartCategorizationService != null)
                {
                    var categorySuggestion = await _smartCategorizationService.SuggestCategoryAsync(transactionRequest, userId);
                    if (categorySuggestion.Confidence > 0.5)
                    {
                        createTransactionDto.Category = categorySuggestion.CategoryName;
                        if (string.IsNullOrEmpty(createTransactionDto.Description))
                        {
                            createTransactionDto.Description = categorySuggestion.Reason ?? "";
                        }
                    }
                }

                // 3. Apply transaction rules
                if (_transactionRulesService != null)
                {
                    var ruleResult = await _transactionRulesService.ApplyRulesAsync(transactionRequest, userId);
                    if (ruleResult.Matched)
                    {
                        if (!string.IsNullOrEmpty(ruleResult.Category))
                        {
                            createTransactionDto.Category = ruleResult.Category;
                        }
                        if (ruleResult.Tags.Any())
                        {
                            // Tags could be stored in metadata or notes
                            if (string.IsNullOrEmpty(createTransactionDto.Notes))
                            {
                                createTransactionDto.Notes = $"Tags: {string.Join(", ", ruleResult.Tags)}";
                            }
                        }
                        if (!string.IsNullOrEmpty(ruleResult.Description))
                        {
                            createTransactionDto.Description = ruleResult.Description;
                        }
                    }
                }
                // ==================== END AUTOMATION FEATURES ====================

                // Handle category-based references
                string? billId = null;
                string? savingsAccountId = null;
                string? loanId = null;
                string enhancedDescription = createTransactionDto.Description;

                // Use directly provided IDs first (from DTO) - can have multiple
                if (!string.IsNullOrEmpty(createTransactionDto.BillId))
                {
                    billId = createTransactionDto.BillId;
                    if (string.IsNullOrEmpty(enhancedDescription) || enhancedDescription == createTransactionDto.Description)
                    {
                        enhancedDescription = $"Bill Payment - {createTransactionDto.Description}";
                    }
                }
                
                if (!string.IsNullOrEmpty(createTransactionDto.SavingsAccountId))
                {
                    savingsAccountId = createTransactionDto.SavingsAccountId;
                    
                    // Fetch the savings account to get its AccountName for the category
                    var savingsAccount = await _context.SavingsAccounts
                        .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId && sa.UserId == userId);
                    
                    if (savingsAccount != null)
                    {
                        // Set category to [SAVINGS-{AccountName}]
                        createTransactionDto.Category = $"[SAVINGS-{savingsAccount.AccountName}]";
                    }
                    
                    if (string.IsNullOrEmpty(enhancedDescription) || enhancedDescription == createTransactionDto.Description)
                    {
                        enhancedDescription = $"Savings - {createTransactionDto.Description}";
                    }
                }
                
                if (!string.IsNullOrEmpty(createTransactionDto.LoanId))
                {
                    loanId = createTransactionDto.LoanId;
                    
                    // Fetch the loan to get its Purpose for the category
                    var loan = await _context.Loans
                        .FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);
                    
                    if (loan != null)
                    {
                        // Set category to [LOAN-{Purpose}]
                        createTransactionDto.Category = $"[LOAN-{loan.Purpose}]";
                    }
                    
                    if (string.IsNullOrEmpty(enhancedDescription) || enhancedDescription == createTransactionDto.Description)
                    {
                        enhancedDescription = $"Loan Payment - {createTransactionDto.Description}";
                    }
                }
                // Fallback to category-based detection if no direct IDs provided
                if (string.IsNullOrEmpty(billId) && string.IsNullOrEmpty(savingsAccountId) && string.IsNullOrEmpty(loanId) && !string.IsNullOrEmpty(createTransactionDto.Category))
                {
                    var categoryLowerFallback = createTransactionDto.Category.ToLower();
                    
                    // Bill-related categories
                    if (categoryLowerFallback.Contains("bill") || categoryLowerFallback.Contains("utility") || 
                        categoryLowerFallback.Contains("rent") || categoryLowerFallback.Contains("insurance") ||
                        categoryLowerFallback.Contains("subscription") || categoryLowerFallback.Contains("payment"))
                    {
                        if (!string.IsNullOrEmpty(createTransactionDto.BillId))
                        {
                            billId = createTransactionDto.BillId;
                            enhancedDescription = $"Bill Payment - {createTransactionDto.Description}";
                        }
                    }
                    // Savings-related categories
                    else if ((categoryLowerFallback.Contains("savings") || categoryLowerFallback.Contains("deposit") || 
                             categoryLowerFallback.Contains("investment") || categoryLowerFallback.Contains("goal")) &&
                             !string.IsNullOrEmpty(createTransactionDto.SavingsAccountId) &&
                             createTransactionDto.SavingsAccountId != createTransactionDto.BankAccountId)
                    {
                        savingsAccountId = createTransactionDto.SavingsAccountId;
                        enhancedDescription = $"Savings - {createTransactionDto.Description}";
                    }
                    // Loan-related categories
                    else if (categoryLowerFallback.Contains("loan") || categoryLowerFallback.Contains("repayment") || 
                             categoryLowerFallback.Contains("debt") || categoryLowerFallback.Contains("installment"))
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
                // For credit cards:
                // - CREDIT (refund/credit) increases balance
                // - DEBIT (purchase) decreases balance
                // For regular accounts:
                // - CREDIT adds money, so balance increases
                // - DEBIT removes money, so balance decreases
                bool isCreditCard = bankAccount.AccountType?.ToLower() == "credit_card";
                
                if (payment.TransactionType == "CREDIT")
                {
                    // Both credit cards and regular accounts: CREDIT increases balance
                    bankAccount.CurrentBalance += payment.Amount;
                }
                else if (payment.TransactionType == "DEBIT")
                {
                    if (isCreditCard)
                    {
                        // Credit card: DEBIT (purchase) decreases balance
                        bankAccount.CurrentBalance -= payment.Amount;
                    }
                    else
                    {
                        // Regular account: DEBIT removes money
                        bankAccount.CurrentBalance -= payment.Amount;
                    }
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

                // Handle savings account transaction if linked
                if (!string.IsNullOrEmpty(savingsAccountId))
                {
                    var savingsAccount = await _context.SavingsAccounts
                        .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId && sa.UserId == userId);

                    if (savingsAccount == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            $"Savings account not found. SavingsAccountId: {savingsAccountId}, UserId: {userId}");
                    }

                    // Determine savings transaction type based on bank transaction type
                    // For purchases (DEBIT), treat as DEPOSIT to savings (allocating money to savings goal)
                    // For income (CREDIT), treat as DEPOSIT to savings (saving money)
                    // Note: This means linking a transaction to savings always adds to savings balance
                    string savingsTransactionType = "DEPOSIT";

                    // Create savings transaction
                    var savingsTransaction = new SavingsTransaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        SavingsAccountId = savingsAccountId,
                        SourceBankAccountId = createTransactionDto.BankAccountId,
                        Amount = payment.Amount,
                        TransactionType = savingsTransactionType,
                        Description = payment.Description,
                        Category = payment.Category ?? "SAVINGS",
                        Notes = payment.Notes,
                        TransactionDate = payment.TransactionDate ?? DateTime.UtcNow,
                        Currency = payment.Currency,
                        IsRecurring = payment.IsRecurring,
                        RecurringFrequency = payment.RecurringFrequency,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.SavingsTransactions.Add(savingsTransaction);

                    // Update savings account balance
                    if (savingsTransactionType == "DEPOSIT")
                    {
                        savingsAccount.CurrentBalance += payment.Amount;
                    }
                    else if (savingsTransactionType == "WITHDRAWAL")
                    {
                        // Check if savings account has sufficient balance
                        if (savingsAccount.CurrentBalance < payment.Amount)
                        {
                            return ApiResponse<BankTransactionDto>.ErrorResult(
                                $"Insufficient balance in savings account. Current balance: {savingsAccount.CurrentBalance}, Required: {payment.Amount}");
                        }
                        savingsAccount.CurrentBalance -= payment.Amount;
                    }

                    savingsAccount.UpdatedAt = DateTime.UtcNow;
                }

                // Create double-entry journal entry based on transaction type
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    JournalEntry? journalEntry = null;
                    var reference = payment.Reference ?? $"BANK_TXN_{Guid.NewGuid()}";

                    if (payment.TransactionType == "CREDIT")
                    {
                        // Income transaction: Debit Bank Account, Credit Income Account
                        var category = payment.Category ?? "Other Income";
                        journalEntry = await _accountingService.CreateIncomeEntryAsync(
                            userId: userId,
                            amount: payment.Amount,
                            category: category,
                            bankAccountName: bankAccount.AccountName,
                            reference: reference,
                            description: payment.Description,
                            entryDate: payment.TransactionDate ?? DateTime.UtcNow
                        );
                    }
                    else if (payment.TransactionType == "DEBIT")
                    {
                        // Check if this is a bank transfer (has toBankAccountId or category is TRANSFER)
                        // Reuse the isBankTransfer variable from outer scope (already validated above)
                        if (isBankTransfer && !string.IsNullOrEmpty(createTransactionDto.ToBankAccountId))
                        {
                            // Bank transfer: Debit Destination Account, Credit Source Account
                            var destinationAccount = await _context.BankAccounts
                                .FirstOrDefaultAsync(ba => ba.Id == createTransactionDto.ToBankAccountId && ba.UserId == userId);
                            
                            if (destinationAccount == null)
                            {
                                await transaction.RollbackAsync();
                                return ApiResponse<BankTransactionDto>.ErrorResult("Destination bank account not found for transfer");
                            }

                            // Update destination account balance
                            destinationAccount.CurrentBalance += payment.Amount;
                            destinationAccount.UpdatedAt = DateTime.UtcNow;

                            journalEntry = await _accountingService.CreateBankTransferEntryAsync(
                                userId: userId,
                                amount: payment.Amount,
                                sourceAccountName: bankAccount.AccountName,
                                destinationAccountName: destinationAccount.AccountName,
                                reference: reference,
                                description: payment.Description,
                                entryDate: payment.TransactionDate ?? DateTime.UtcNow
                            );
                        }
                        else if (!string.IsNullOrEmpty(billId))
                        {
                            // Bill payment: Debit Expense, Credit Bank Account
                            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.Id == billId);
                            var billType = bill?.BillType ?? "Other";
                            var billName = bill?.Provider ?? "Bill";
                            journalEntry = await _accountingService.CreateBillPaymentEntryAsync(
                                billId: billId,
                                userId: userId,
                                amount: payment.Amount,
                                billName: billName,
                                billType: billType,
                                bankAccountName: bankAccount.AccountName,
                                reference: reference,
                                description: payment.Description,
                                entryDate: payment.TransactionDate ?? DateTime.UtcNow
                            );
                        }
                        else if (!string.IsNullOrEmpty(savingsAccountId))
                        {
                            // Savings deposit: Debit Savings Account, Credit Bank Account
                            var savingsAccount = await _context.SavingsAccounts
                                .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId);
                            if (savingsAccount != null)
                            {
                                journalEntry = await _accountingService.CreateSavingsDepositEntryAsync(
                                    savingsAccountId: savingsAccountId,
                                    userId: userId,
                                    amount: payment.Amount,
                                    savingsAccountName: savingsAccount.AccountName,
                                    bankAccountName: bankAccount.AccountName,
                                    reference: reference,
                                    description: payment.Description,
                                    entryDate: payment.TransactionDate ?? DateTime.UtcNow
                                );
                            }
                        }
                        else if (!string.IsNullOrEmpty(loanId))
                        {
                            // Loan payment: This should be handled by LoanService, but we can create expense entry
                            // Note: Loan payments typically have principal and interest split, which is handled in LoanService
                            // For now, create a general expense entry
                            var category = payment.Category ?? "Loan Payment";
                            journalEntry = await _accountingService.CreateExpenseEntryAsync(
                                userId: userId,
                                amount: payment.Amount,
                                category: category,
                                bankAccountName: bankAccount.AccountName,
                                reference: reference,
                                description: payment.Description,
                                entryDate: payment.TransactionDate ?? DateTime.UtcNow
                            );
                        }
                        else
                        {
                            // Regular expense: Debit Expense, Credit Bank Account
                            var category = payment.Category ?? "General Expense";
                            journalEntry = await _accountingService.CreateExpenseEntryAsync(
                                userId: userId,
                                amount: payment.Amount,
                                category: category,
                                bankAccountName: bankAccount.AccountName,
                                reference: reference,
                                description: payment.Description,
                                entryDate: payment.TransactionDate ?? DateTime.UtcNow
                            );
                        }
                    }

                    // Save all changes including journal entry
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var transactionDto = MapPaymentToBankTransactionDto(payment);
                    return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto, "Transaction created successfully with double-entry validation");
                }
                catch (Exception journalEx)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<BankTransactionDto>.ErrorResult(
                        $"Failed to create transaction with double-entry validation: {journalEx.Message}");
                }
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

        public async Task<ApiResponse<BankTransactionDto>> CreateExpenseAsync(CreateBankAccountExpenseDto expenseDto, string userId)
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
            
            // Load cards if not already loaded (handle case where Cards table doesn't exist)
            List<CardDto> cards = new List<CardDto>();
            try
            {
                // Try to load Cards collection - this will fail if Cards table doesn't exist
                try
                {
                    var isCardsLoaded = _context.Entry(bankAccount)
                        .Collection(ba => ba.Cards)
                        .IsLoaded;
                    
                    if (!isCardsLoaded)
                    {
                        await _context.Entry(bankAccount)
                            .Collection(ba => ba.Cards)
                            .LoadAsync();
                    }
                }
                catch
                {
                    // If loading fails, Cards table likely doesn't exist - skip Cards
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
                        TotalOutgoing = transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount),
                        Cards = new List<CardDto>()
                    };
                }

                // Access Cards property and map to DTOs (this might also trigger a query)
                try
                {
                    var bankAccountCards = bankAccount.Cards;
                    if (bankAccountCards != null)
                    {
                        cards = bankAccountCards
                            .Where(c => !c.IsDeleted)
                            .Select(c => new CardDto
                            {
                                Id = c.Id,
                                BankAccountId = c.BankAccountId,
                                UserId = c.UserId,
                                CardName = c.CardName,
                                CardType = c.CardType,
                                CardBrand = c.CardBrand,
                                Last4Digits = c.Last4Digits,
                                CardholderName = c.CardholderName,
                                ExpiryMonth = c.ExpiryMonth,
                                ExpiryYear = c.ExpiryYear,
                                IsPrimary = c.IsPrimary,
                                IsActive = c.IsActive,
                                Description = c.Description,
                                CreatedAt = c.CreatedAt,
                                UpdatedAt = c.UpdatedAt,
                                AccountName = bankAccount.AccountName
                            })
                            .ToList();
                    }
                }
                catch
                {
                    // Accessing Cards property failed - return empty list
                    cards = new List<CardDto>();
                }
            }
            catch (Exception)
            {
                // Cards table doesn't exist or Cards couldn't be accessed - return empty list
                cards = new List<CardDto>();
            }
            
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
                TotalOutgoing = transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount),
                Cards = cards
            };
        }

        private static BankTransactionDto MapToBankTransactionDto(BankTransaction transaction)
        {
            return new BankTransactionDto
            {
                Id = transaction.Id,
                BankAccountId = transaction.BankAccountId,
                AccountName = transaction.BankAccount?.AccountName ?? string.Empty,
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
                AccountName = payment.BankAccount?.AccountName ?? string.Empty,
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
            
            // Set endDate to end of today (23:59:59.9999999) to include all transactions on the current day
            var endOfToday = now.Date.AddDays(1).AddTicks(-1);
            
            return period.ToLower() switch
            {
                "weekly" or "week" => (now.AddDays(-7).Date, endOfToday),
                "monthly" or "month" => (now.AddDays(-30).Date, endOfToday),
                "quarterly" or "quarter" => (now.AddDays(-90).Date, endOfToday),
                "yearly" or "year" => (now.AddDays(-365).Date, endOfToday),
                _ => (now.AddDays(-30).Date, endOfToday) // Default to month
            };
        }

        /// <summary>
        /// Helper method to check if an account type is a credit card
        /// </summary>
        private static bool IsCreditCardAccount(string accountType)
        {
            if (string.IsNullOrEmpty(accountType))
                return false;

            var accountTypeLower = accountType.ToLower().Trim();
            return accountTypeLower == "credit_card" || 
                   accountTypeLower == "credit card" || 
                   accountTypeLower == "creditcard";
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

                var query = _context.Payments
                    .Include(p => p.BankAccount)
                    .Where(p => p.BankAccountId == bankAccountId && p.UserId == userId && p.IsBankTransaction);

                if (dateFrom.HasValue)
                {
                    query = query.Where(p => p.TransactionDate >= dateFrom.Value);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(p => p.TransactionDate <= dateTo.Value);
                }

                var transactions = await query
                    .OrderByDescending(p => p.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = transactions.Select(p => MapPaymentToBankTransactionDto(p)).ToList();
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
                var query = _context.Payments
                    .Include(p => p.BankAccount)
                    .Where(p => p.UserId == userId && p.IsBankTransaction);

                if (!string.IsNullOrEmpty(accountType))
                {
                    query = query.Where(p => p.BankAccount != null && p.BankAccount.AccountType == accountType);
                }

                if (dateFrom.HasValue)
                {
                    query = query.Where(p => p.TransactionDate >= dateFrom.Value);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(p => p.TransactionDate <= dateTo.Value);
                }

                var transactions = await query
                    .OrderByDescending(p => p.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = transactions.Select(p => MapPaymentToBankTransactionDto(p)).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get user transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankAccountSummaryDto>> GetBankAccountSummaryAsync(string userId, string frequency = "monthly", int? year = null, int? month = null)
        {
            try
            {
                var (startDate, endDate) = GetPeriodDates(frequency);
                
                if (year.HasValue && month.HasValue)
                {
                    startDate = new DateTime(year.Value, month.Value, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                }

                var accounts = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .ToListAsync();

                var transactions = await _context.Payments
                    .Where(p => p.UserId == userId && 
                               p.IsBankTransaction && 
                               p.TransactionDate >= startDate && 
                               p.TransactionDate <= endDate)
                    .ToListAsync();

                // Calculate total balance excluding credit cards (they represent debt, not assets)
                var totalBalance = accounts
                    .Where(a => {
                        var accountTypeLower = a.AccountType?.ToLower().Trim() ?? "";
                        return accountTypeLower != "credit_card" && 
                               accountTypeLower != "credit card" && 
                               accountTypeLower != "creditcard";
                    })
                    .Sum(a => a.CurrentBalance);

                // Calculate total remaining credit limit for credit cards
                var creditCardAccounts = accounts
                    .Where(a => {
                        var accountTypeLower = a.AccountType?.ToLower().Trim() ?? "";
                        return accountTypeLower == "credit_card" || 
                               accountTypeLower == "credit card" || 
                               accountTypeLower == "creditcard";
                    })
                    .ToList();

                decimal? totalRemainingCreditLimit = null;
                if (creditCardAccounts.Any())
                {
                    totalRemainingCreditLimit = creditCardAccounts
                        .Sum(a => Math.Max(0, a.InitialBalance - a.CurrentBalance));
                }

                var summary = new BankAccountSummaryDto
                {
                    TotalBalance = totalBalance,
                    TotalRemainingCreditLimit = totalRemainingCreditLimit,
                    TotalAccounts = accounts.Count,
                    ActiveAccounts = accounts.Count(a => a.IsActive),
                    ConnectedAccounts = accounts.Count(a => a.IsConnected),
                    TotalIncoming = transactions.Where(t => t.TransactionType == "CREDIT").Sum(t => t.Amount),
                    TotalOutgoing = transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount),
                    CurrentMonthIncoming = 0, // This method doesn't calculate current month separately
                    CurrentMonthOutgoing = 0,
                    CurrentMonthNet = transactions.Where(t => t.TransactionType == "CREDIT").Sum(t => t.Amount) -
                                     transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount),
                    Frequency = frequency,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TransactionCount = transactions.Count,
                    Accounts = accounts.Select(a => MapToBankAccountDtoAsync(a).Result).ToList(),
                    SpendingByCategory = transactions
                        .Where(t => t.TransactionType == "DEBIT" && !string.IsNullOrEmpty(t.Category))
                        .GroupBy(t => t.Category!)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount))
                };

                return ApiResponse<BankAccountSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountSummaryDto>.ErrorResult($"Failed to get bank account summary: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankTransactionDto>> AnalyzeAndCreateTransactionAsync(AnalyzeTransactionTextDto analyzeDto, string userId)
        {
            // Delegate to AI Agent service via dependency injection
            var aiAgentService = _serviceProvider.GetService<IAIAgentService>();
            if (aiAgentService != null)
            {
                return await aiAgentService.AnalyzeAndCreateTransactionWithAgentAsync(analyzeDto, userId);
            }
            
            return ApiResponse<BankTransactionDto>.ErrorResult("AI Agent service is not available");
        }
    }
}
