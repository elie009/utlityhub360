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
                // Use projection to avoid reading soft delete columns
                var bankAccountData = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.Id == bankAccountId && ba.UserId == userId)
                    .Select(ba => new
                    {
                        ba.Id,
                        ba.UserId,
                        ba.AccountName,
                        ba.AccountType,
                        ba.InitialBalance,
                        ba.CurrentBalance,
                        ba.Currency,
                        ba.Description,
                        ba.FinancialInstitution,
                        ba.AccountNumber,
                        ba.RoutingNumber,
                        ba.SyncFrequency,
                        ba.IsConnected,
                        ba.ConnectionId,
                        ba.LastSyncedAt,
                        ba.CreatedAt,
                        ba.UpdatedAt,
                        ba.IsActive,
                        ba.Iban,
                        ba.SwiftCode
                    })
                    .FirstOrDefaultAsync();

                if (bankAccountData == null)
                {
                    return ApiResponse<BankAccountDto>.ErrorResult("Bank account not found");
                }
                
                // Convert to BankAccount entity
                var bankAccount = new BankAccount
                {
                    Id = bankAccountData.Id,
                    UserId = bankAccountData.UserId,
                    AccountName = bankAccountData.AccountName,
                    AccountType = bankAccountData.AccountType,
                    InitialBalance = bankAccountData.InitialBalance,
                    CurrentBalance = bankAccountData.CurrentBalance,
                    Currency = bankAccountData.Currency,
                    Description = bankAccountData.Description,
                    FinancialInstitution = bankAccountData.FinancialInstitution,
                    AccountNumber = bankAccountData.AccountNumber,
                    RoutingNumber = bankAccountData.RoutingNumber,
                    SyncFrequency = bankAccountData.SyncFrequency,
                    IsConnected = bankAccountData.IsConnected,
                    ConnectionId = bankAccountData.ConnectionId,
                    LastSyncedAt = bankAccountData.LastSyncedAt,
                    CreatedAt = bankAccountData.CreatedAt,
                    UpdatedAt = bankAccountData.UpdatedAt,
                    IsActive = bankAccountData.IsActive,
                    Iban = bankAccountData.Iban,
                    SwiftCode = bankAccountData.SwiftCode,
                    Transactions = new List<BankTransaction>()
                };

                // Load transactions separately using projection to avoid soft delete columns
                var transactionsData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => t.BankAccountId == bankAccountId)
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.UserId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction
                    })
                    .ToListAsync();
                
                // Convert to BankTransaction entities (filter out soft-deleted in memory)
                var transactions = transactionsData.Select(t => new BankTransaction
                {
                    Id = t.Id,
                    BankAccountId = t.BankAccountId,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    ExternalTransactionId = t.ExternalTransactionId,
                    TransactionDate = t.TransactionDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Notes = t.Notes,
                    Merchant = t.Merchant,
                    Location = t.Location,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    Currency = t.Currency,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    IsDeleted = false
                }).ToList();
                bankAccount.Transactions = transactions;

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

                if (bankAccount.IsDeleted)
                {
                    return ApiResponse<bool>.ErrorResult("Bank account is already deleted");
                }

                // Handle foreign key constraints by setting related foreign keys to NULL
                // These entities have DeleteBehavior.NoAction, so we need to handle them manually
                
                // 1. Set BankAccountId to NULL in Payments
                var payments = await _context.Payments
                    .Where(p => p.BankAccountId == bankAccountId)
                    .ToListAsync();
                
                foreach (var payment in payments)
                {
                    payment.BankAccountId = null;
                }

                // 2. Set BankAccountId to NULL in ReceivablePayments
                var receivablePayments = await _context.ReceivablePayments
                    .Where(rp => rp.BankAccountId == bankAccountId)
                    .ToListAsync();
                
                foreach (var receivablePayment in receivablePayments)
                {
                    receivablePayment.BankAccountId = null;
                }

                // 3. Set SourceBankAccountId to NULL in SavingsTransactions
                var savingsTransactions = await _context.SavingsTransactions
                    .Where(st => st.SourceBankAccountId == bankAccountId)
                    .ToListAsync();
                
                foreach (var savingsTransaction in savingsTransactions)
                {
                    savingsTransaction.SourceBankAccountId = null;
                }

                // Perform soft delete instead of hard delete
                bankAccount.IsDeleted = true;
                bankAccount.DeletedAt = DateTime.UtcNow;
                bankAccount.DeletedBy = userId;
                bankAccount.IsActive = false; // Also deactivate the account

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Bank account deleted successfully");
            }
            catch (DbUpdateException dbEx)
            {
                // Log the inner exception for debugging
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                return ApiResponse<bool>.ErrorResult($"Failed to delete bank account: An error occurred while saving the entity changes. See the inner exception for details. Inner exception: {innerException}");
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
                // Use projection to avoid reading soft delete columns that don't exist yet
                var query = _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.UserId == userId);

                if (!includeInactive)
                {
                    query = query.Where(ba => ba.IsActive);
                }

                // Project to anonymous type to avoid materializing soft delete properties
                var bankAccountsData = await query
                    .Select(ba => new
                    {
                        ba.Id,
                        ba.UserId,
                        ba.AccountName,
                        ba.AccountType,
                        ba.InitialBalance,
                        ba.CurrentBalance,
                        ba.Currency,
                        ba.Description,
                        ba.FinancialInstitution,
                        ba.AccountNumber,
                        ba.RoutingNumber,
                        ba.SyncFrequency,
                        ba.IsConnected,
                        ba.ConnectionId,
                        ba.LastSyncedAt,
                        ba.CreatedAt,
                        ba.UpdatedAt,
                        ba.IsActive,
                        ba.Iban,
                        ba.SwiftCode
                    })
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .ToListAsync();

                // Convert to BankAccount entities for mapping
                var bankAccountIds = bankAccountsData.Select(ba => ba.Id).ToList();
                var bankAccounts = bankAccountsData.Select(ba => new BankAccount
                {
                    Id = ba.Id,
                    UserId = ba.UserId,
                    AccountName = ba.AccountName,
                    AccountType = ba.AccountType,
                    InitialBalance = ba.InitialBalance,
                    CurrentBalance = ba.CurrentBalance,
                    Currency = ba.Currency,
                    Description = ba.Description,
                    FinancialInstitution = ba.FinancialInstitution,
                    AccountNumber = ba.AccountNumber,
                    RoutingNumber = ba.RoutingNumber,
                    SyncFrequency = ba.SyncFrequency,
                    IsConnected = ba.IsConnected,
                    ConnectionId = ba.ConnectionId,
                    LastSyncedAt = ba.LastSyncedAt,
                    CreatedAt = ba.CreatedAt,
                    UpdatedAt = ba.UpdatedAt,
                    IsActive = ba.IsActive,
                    Iban = ba.Iban,
                    SwiftCode = ba.SwiftCode,
                    Transactions = new List<BankTransaction>()
                }).ToList();

                // Load transactions separately using projection to avoid soft delete columns
                var transactionsData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => bankAccountIds.Contains(t.BankAccountId))
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.UserId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction
                    })
                    .ToListAsync();
                
                // Convert to BankTransaction entities (without soft delete properties)
                var transactions = transactionsData.Select(t => new BankTransaction
                {
                    Id = t.Id,
                    BankAccountId = t.BankAccountId,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    ExternalTransactionId = t.ExternalTransactionId,
                    TransactionDate = t.TransactionDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Notes = t.Notes,
                    Merchant = t.Merchant,
                    Location = t.Location,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    Currency = t.Currency,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    IsDeleted = false // Set default since we're filtering these out anyway
                }).ToList();
                
                // Attach transactions to bank accounts in memory
                foreach (var account in bankAccounts)
                {
                    account.Transactions = transactions
                        .Where(t => t.BankAccountId == account.Id)
                        .ToList();
                }

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
                // Use projection to avoid reading soft delete columns that don't exist yet
                var bankAccountsData = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .Select(ba => new
                    {
                        ba.Id,
                        ba.UserId,
                        ba.AccountName,
                        ba.AccountType,
                        ba.InitialBalance,
                        ba.CurrentBalance,
                        ba.Currency,
                        ba.Description,
                        ba.FinancialInstitution,
                        ba.AccountNumber,
                        ba.RoutingNumber,
                        ba.SyncFrequency,
                        ba.IsConnected,
                        ba.ConnectionId,
                        ba.LastSyncedAt,
                        ba.CreatedAt,
                        ba.UpdatedAt,
                        ba.IsActive,
                        ba.Iban,
                        ba.SwiftCode
                    })
                    .ToListAsync();

                // Convert to BankAccount entities for mapping
                var bankAccounts = bankAccountsData.Select(ba => new BankAccount
                {
                    Id = ba.Id,
                    UserId = ba.UserId,
                    AccountName = ba.AccountName,
                    AccountType = ba.AccountType,
                    InitialBalance = ba.InitialBalance,
                    CurrentBalance = ba.CurrentBalance,
                    Currency = ba.Currency,
                    Description = ba.Description,
                    FinancialInstitution = ba.FinancialInstitution,
                    AccountNumber = ba.AccountNumber,
                    RoutingNumber = ba.RoutingNumber,
                    SyncFrequency = ba.SyncFrequency,
                    IsConnected = ba.IsConnected,
                    ConnectionId = ba.ConnectionId,
                    LastSyncedAt = ba.LastSyncedAt,
                    CreatedAt = ba.CreatedAt,
                    UpdatedAt = ba.UpdatedAt,
                    IsActive = ba.IsActive,
                    Iban = ba.Iban,
                    SwiftCode = ba.SwiftCode,
                    Transactions = new List<BankTransaction>()
                }).ToList();
                
                // Load transactions separately using projection to avoid soft delete columns
                var bankAccountIds = bankAccounts.Select(ba => ba.Id).ToList();
                var transactionsData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => bankAccountIds.Contains(t.BankAccountId))
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.UserId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction
                    })
                    .ToListAsync();
                
                // Convert to BankTransaction entities (without soft delete properties)
                var transactions = transactionsData.Select(t => new BankTransaction
                {
                    Id = t.Id,
                    BankAccountId = t.BankAccountId,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    ExternalTransactionId = t.ExternalTransactionId,
                    TransactionDate = t.TransactionDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Notes = t.Notes,
                    Merchant = t.Merchant,
                    Location = t.Location,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    Currency = t.Currency,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    IsDeleted = false // Set default since we're filtering these out anyway
                }).ToList();
                
                // Attach transactions to bank accounts in memory
                foreach (var account in bankAccounts)
                {
                    account.Transactions = transactions
                        .Where(t => t.BankAccountId == account.Id)
                        .ToList();
                }

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

                // Get total count of ALL transactions (not filtered by period) for TransactionCount
                var totalTransactionCount = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsBankTransaction)
                    .CountAsync();

                // Get all transactions for the period (now from Payments table) using projection
                var allTransactionsData = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsBankTransaction && 
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= periodStart && p.TransactionDate <= periodEnd)
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToListAsync();

                // Convert to Payment-like objects for processing
                var allTransactions = allTransactionsData.Select(p => new Payment
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    TransactionType = p.TransactionType,
                    Description = p.Description,
                    Category = p.Category,
                    TransactionDate = p.TransactionDate,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsDeleted = false // Set default since we're filtering these out anyway
                }).ToList();

                // Get current month transactions using projection
                var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
                var currentMonthTransactionsData = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsBankTransaction && 
                               p.TransactionDate >= currentMonthStart && p.TransactionDate <= currentMonthEnd)
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToListAsync();

                // Convert to Payment-like objects for processing
                var currentMonthTransactions = currentMonthTransactionsData.Select(p => new Payment
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    TransactionType = p.TransactionType,
                    Description = p.Description,
                    Category = p.Category,
                    TransactionDate = p.TransactionDate,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsDeleted = false // Set default since we're filtering these out anyway
                }).ToList();

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
                    TransactionCount = totalTransactionCount, // Total count of ALL transactions, not just period
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

                // Use projection to avoid reading soft delete columns
                var bankAccountsData = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .Select(ba => new
                    {
                        ba.Id,
                        ba.UserId,
                        ba.AccountName,
                        ba.AccountType,
                        ba.InitialBalance,
                        ba.CurrentBalance,
                        ba.Currency,
                        ba.Description,
                        ba.FinancialInstitution,
                        ba.AccountNumber,
                        ba.RoutingNumber,
                        ba.SyncFrequency,
                        ba.IsConnected,
                        ba.ConnectionId,
                        ba.LastSyncedAt,
                        ba.CreatedAt,
                        ba.UpdatedAt,
                        ba.IsActive,
                        ba.Iban,
                        ba.SwiftCode
                    })
                    .ToListAsync();
                
                // Convert to BankAccount entities
                var bankAccounts = bankAccountsData.Select(ba => new BankAccount
                {
                    Id = ba.Id,
                    UserId = ba.UserId,
                    AccountName = ba.AccountName,
                    AccountType = ba.AccountType,
                    InitialBalance = ba.InitialBalance,
                    CurrentBalance = ba.CurrentBalance,
                    Currency = ba.Currency,
                    Description = ba.Description,
                    FinancialInstitution = ba.FinancialInstitution,
                    AccountNumber = ba.AccountNumber,
                    RoutingNumber = ba.RoutingNumber,
                    SyncFrequency = ba.SyncFrequency,
                    IsConnected = ba.IsConnected,
                    ConnectionId = ba.ConnectionId,
                    LastSyncedAt = ba.LastSyncedAt,
                    CreatedAt = ba.CreatedAt,
                    UpdatedAt = ba.UpdatedAt,
                    IsActive = ba.IsActive,
                    Iban = ba.Iban,
                    SwiftCode = ba.SwiftCode,
                    Transactions = new List<BankTransaction>()
                }).ToList();
                
                // Load transactions separately using projection
                var bankAccountIds = bankAccounts.Select(ba => ba.Id).ToList();
                var allTransactionsData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => bankAccountIds.Contains(t.BankAccountId) && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate)
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.UserId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction
                    })
                    .ToListAsync();
                
                // Convert to BankTransaction entities
                var filteredTransactions = allTransactionsData.Select(t => new BankTransaction
                {
                    Id = t.Id,
                    BankAccountId = t.BankAccountId,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    ExternalTransactionId = t.ExternalTransactionId,
                    TransactionDate = t.TransactionDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Notes = t.Notes,
                    Merchant = t.Merchant,
                    Location = t.Location,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    Currency = t.Currency,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    IsDeleted = false
                }).ToList();
                foreach (var account in bankAccounts)
                {
                    account.Transactions = filteredTransactions
                        .Where(t => t.BankAccountId == account.Id)
                        .ToList();
                }

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
                // Load bank accounts without Include and with AsNoTracking to avoid materializing soft delete properties
                var bankAccounts = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .Take(limit)
                    .ToListAsync();
                
                // Load transactions separately
                var bankAccountIds = bankAccounts.Select(ba => ba.Id).ToList();
                var transactions = await _context.BankTransactions
                    .Where(t => bankAccountIds.Contains(t.BankAccountId) && !t.IsDeleted)
                    .ToListAsync();
                
                // Attach transactions to bank accounts in memory
                foreach (var account in bankAccounts)
                {
                    account.Transactions = transactions
                        .Where(t => t.BankAccountId == account.Id)
                        .ToList();
                }

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
                // Load bank accounts without Include and with AsNoTracking to avoid materializing soft delete properties
                var bankAccounts = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.UserId == userId && ba.IsConnected && ba.IsActive)
                    .OrderByDescending(ba => ba.CurrentBalance)
                    .ToListAsync();
                
                // Load transactions separately
                var bankAccountIds = bankAccounts.Select(ba => ba.Id).ToList();
                var transactions = await _context.BankTransactions
                    .Where(t => bankAccountIds.Contains(t.BankAccountId))
                    .ToListAsync();
                
                // Attach transactions to bank accounts in memory (filter out soft-deleted)
                foreach (var account in bankAccounts)
                {
                    account.Transactions = transactions
                        .Where(t => t.BankAccountId == account.Id && !t.IsDeleted)
                        .ToList();
                }

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

                // ==================== MONTH CLOSURE VALIDATION ====================
                // Check if the transaction month is closed
                var transactionDate = createTransactionDto.TransactionDate;
                var isMonthClosed = await _context.ClosedMonths
                    .AnyAsync(cm => cm.BankAccountId == createTransactionDto.BankAccountId &&
                                   cm.Year == transactionDate.Year &&
                                   cm.Month == transactionDate.Month);

                if (isMonthClosed)
                {
                    var monthName = new[] { "", "January", "February", "March", "April", "May", "June",
                                            "July", "August", "September", "October", "November", "December" }[transactionDate.Month];
                    return ApiResponse<BankTransactionDto>.ErrorResult(
                        $"Cannot create transaction. The month {monthName} {transactionDate.Year} is closed for this account.");
                }
                // ==================== END MONTH CLOSURE VALIDATION ====================

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

                        // Auto-create "Expenses" as a default category if it doesn't exist
                        if (createTransactionDto.Category.Equals("Expenses", StringComparison.OrdinalIgnoreCase))
                        {
                            var defaultExpensesCategory = new TransactionCategory
                            {
                                UserId = userId,
                                Name = "Expenses",
                                Description = "Default expense category for general transactions",
                                Type = "EXPENSE",
                                IsActive = true,
                                IsSystemCategory = false,
                                DisplayOrder = 0,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.TransactionCategories.Add(defaultExpensesCategory);
                            await _context.SaveChangesAsync();

                            // Category is now created, continue with validation
                        }
                        else
                        {
                            return ApiResponse<BankTransactionDto>.ErrorResult(
                                $"Category '{createTransactionDto.Category}' not found. Please create the category first or use an existing category.");
                        }
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
                    UpdatedAt = DateTime.UtcNow,
                    // Add linking fields
                    BillId = createTransactionDto.BillId,
                    LoanId = createTransactionDto.LoanId,
                    SavingsAccountId = createTransactionDto.SavingsAccountId,
                    TransactionPurpose = createTransactionDto.TransactionPurpose
                };

                // Validate and handle linked entities
                if (!string.IsNullOrEmpty(createTransactionDto.BillId))
                {
                    var bill = await _context.Bills
                        .FirstOrDefaultAsync(b => b.Id == createTransactionDto.BillId && b.UserId == userId);
                    
                    if (bill == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            $"Bill not found. BillId: {createTransactionDto.BillId}");
                    }
                    
                    // Auto-set purpose if not provided
                    if (string.IsNullOrEmpty(bankTransaction.TransactionPurpose))
                    {
                        bankTransaction.TransactionPurpose = bill.BillType.ToUpper() == "UTILITY" ? "UTILITY" : "BILL";
                    }
                    
                    // Update bill status if payment is for a bill (DEBIT transaction)
                    if (payment.TransactionType == "DEBIT" && bill.Status == "PENDING")
                    {
                        bill.Status = "PAID";
                        bill.PaidAt = DateTime.UtcNow;
                        bill.UpdatedAt = DateTime.UtcNow;
                    }
                }

                if (!string.IsNullOrEmpty(createTransactionDto.LoanId))
                {
                    var loan = await _context.Loans
                        .FirstOrDefaultAsync(l => l.Id == createTransactionDto.LoanId && l.UserId == userId);
                    
                    if (loan == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            $"Loan not found. LoanId: {createTransactionDto.LoanId}");
                    }
                    
                    // Auto-set purpose if not provided
                    if (string.IsNullOrEmpty(bankTransaction.TransactionPurpose))
                    {
                        bankTransaction.TransactionPurpose = "LOAN";
                    }
                }

                if (!string.IsNullOrEmpty(createTransactionDto.SavingsAccountId))
                {
                    // Auto-set purpose if not provided
                    if (string.IsNullOrEmpty(bankTransaction.TransactionPurpose))
                    {
                        bankTransaction.TransactionPurpose = "SAVINGS";
                    }
                }

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

        public async Task<ApiResponse<BankTransactionDto>> UpdateTransactionAsync(string transactionId, UpdateBankTransactionDto updateTransactionDto, string userId)
        {
            try
            {
                // Find the payment record
                var payment = await _context.Payments
                    .Include(p => p.BankAccount)
                    .FirstOrDefaultAsync(p => p.Id == transactionId && p.UserId == userId && p.IsBankTransaction);

                if (payment == null)
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult("Transaction not found");
                }

                var bankAccount = payment.BankAccount;
                if (bankAccount == null)
                {
                    // Try to load bank account if not included
                    if (!string.IsNullOrEmpty(payment.BankAccountId))
                    {
                        bankAccount = await _context.BankAccounts
                            .FirstOrDefaultAsync(ba => ba.Id == payment.BankAccountId);
                    }
                    
                    if (bankAccount == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Bank account not found for this transaction");
                    }
                }

                // Handle BankAccountId change (moving transaction to different account)
                BankAccount? newBankAccount = null;
                if (!string.IsNullOrEmpty(updateTransactionDto.BankAccountId) && 
                    updateTransactionDto.BankAccountId != payment.BankAccountId)
                {
                    // Verify the new bank account exists and belongs to the user
                    newBankAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == updateTransactionDto.BankAccountId && 
                                                   ba.UserId == userId && 
                                                   ba.IsActive);
                    
                    if (newBankAccount == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            "New bank account not found, inactive, or does not belong to user");
                    }
                    
                    // Check if the new account's month is closed for the transaction date
                    var transactionDateForCheck = updateTransactionDto.TransactionDate ?? payment.TransactionDate ?? DateTime.UtcNow;
                    var isNewAccountMonthClosed = await _context.ClosedMonths
                        .AnyAsync(cm => cm.BankAccountId == newBankAccount.Id &&
                                       cm.Year == transactionDateForCheck.Year &&
                                       cm.Month == transactionDateForCheck.Month);
                    
                    if (isNewAccountMonthClosed)
                    {
                        var monthName = new[] { "", "January", "February", "March", "April", "May", "June",
                                                "July", "August", "September", "October", "November", "December" }[transactionDateForCheck.Month];
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            $"Cannot move transaction to new account. The month {monthName} {transactionDateForCheck.Year} is closed for the destination account.");
                    }
                }

                // Determine the transaction date to check (use new date if provided, otherwise old date)
                var transactionDate = updateTransactionDto.TransactionDate ?? payment.TransactionDate ?? DateTime.UtcNow;
                var oldTransactionDate = payment.TransactionDate ?? DateTime.UtcNow;

                // ==================== MONTH CLOSURE VALIDATION ====================
                // Check if the old transaction month is closed
                var isOldMonthClosed = await _context.ClosedMonths
                    .AnyAsync(cm => cm.BankAccountId == payment.BankAccountId &&
                                   cm.Year == oldTransactionDate.Year &&
                                   cm.Month == oldTransactionDate.Month);

                if (isOldMonthClosed)
                {
                    var monthName = new[] { "", "January", "February", "March", "April", "May", "June",
                                            "July", "August", "September", "October", "November", "December" }[oldTransactionDate.Month];
                    return ApiResponse<BankTransactionDto>.ErrorResult(
                        $"Cannot update transaction. The month {monthName} {oldTransactionDate.Year} is closed for this account.");
                }

                // Check if the new transaction month is closed (if date changed)
                if (updateTransactionDto.TransactionDate.HasValue && 
                    (oldTransactionDate.Year != transactionDate.Year || oldTransactionDate.Month != transactionDate.Month))
                {
                    var isNewMonthClosed = await _context.ClosedMonths
                        .AnyAsync(cm => cm.BankAccountId == payment.BankAccountId &&
                                       cm.Year == transactionDate.Year &&
                                       cm.Month == transactionDate.Month);

                    if (isNewMonthClosed)
                    {
                        var monthName = new[] { "", "January", "February", "March", "April", "May", "June",
                                                "July", "August", "September", "October", "November", "December" }[transactionDate.Month];
                        return ApiResponse<BankTransactionDto>.ErrorResult(
                            $"Cannot move transaction to {monthName} {transactionDate.Year}. That month is closed for this account.");
                    }
                }
                // ==================== END MONTH CLOSURE VALIDATION ====================

                // Store old values for balance reversal
                var oldAmount = payment.Amount;
                var oldTransactionType = payment.TransactionType;
                var oldBankAccountId = payment.BankAccountId;

                // Reverse old balance impact
                bool isCreditCard = bankAccount.AccountType?.ToLower() == "credit_card";
                if (oldTransactionType == "CREDIT")
                {
                    bankAccount.CurrentBalance -= oldAmount;
                }
                else if (oldTransactionType == "DEBIT")
                {
                    bankAccount.CurrentBalance += oldAmount;
                }

                // If changing bank account, update the payment's bank account reference
                if (newBankAccount != null)
                {
                    // Update old account's updated timestamp
                    bankAccount.UpdatedAt = DateTime.UtcNow;
                    
                    // Switch to new bank account for applying new balance
                    payment.BankAccountId = newBankAccount.Id;
                    payment.BankAccount = newBankAccount;
                    bankAccount = newBankAccount;
                }

                // Update payment fields
                if (updateTransactionDto.Amount.HasValue)
                {
                    payment.Amount = updateTransactionDto.Amount.Value;
                }

                if (!string.IsNullOrEmpty(updateTransactionDto.TransactionType))
                {
                    payment.TransactionType = updateTransactionDto.TransactionType.ToUpper();
                }

                if (!string.IsNullOrEmpty(updateTransactionDto.Description))
                {
                    payment.Description = updateTransactionDto.Description;
                }

                if (updateTransactionDto.Category != null) // Allow clearing category
                {
                    payment.Category = updateTransactionDto.Category;
                }

                if (updateTransactionDto.ReferenceNumber != null)
                {
                    payment.Reference = updateTransactionDto.ReferenceNumber;
                }

                if (updateTransactionDto.ExternalTransactionId != null)
                {
                    payment.ExternalTransactionId = updateTransactionDto.ExternalTransactionId;
                }

                if (updateTransactionDto.TransactionDate.HasValue)
                {
                    payment.TransactionDate = updateTransactionDto.TransactionDate.Value;
                    payment.ProcessedAt = updateTransactionDto.TransactionDate.Value;
                }

                if (updateTransactionDto.Notes != null)
                {
                    payment.Notes = updateTransactionDto.Notes;
                }

                if (updateTransactionDto.Merchant != null)
                {
                    payment.Merchant = updateTransactionDto.Merchant;
                }

                if (updateTransactionDto.Location != null)
                {
                    payment.Location = updateTransactionDto.Location;
                }

                if (updateTransactionDto.IsRecurring.HasValue)
                {
                    payment.IsRecurring = updateTransactionDto.IsRecurring.Value;
                }

                if (updateTransactionDto.RecurringFrequency != null)
                {
                    payment.RecurringFrequency = updateTransactionDto.RecurringFrequency;
                }

                if (!string.IsNullOrEmpty(updateTransactionDto.Currency))
                {
                    payment.Currency = updateTransactionDto.Currency.ToUpper();
                }

                if (!string.IsNullOrEmpty(updateTransactionDto.BillId))
                {
                    payment.BillId = updateTransactionDto.BillId;
                }

                if (!string.IsNullOrEmpty(updateTransactionDto.SavingsAccountId))
                {
                    payment.SavingsAccountId = updateTransactionDto.SavingsAccountId;
                }

                if (!string.IsNullOrEmpty(updateTransactionDto.LoanId))
                {
                    payment.LoanId = updateTransactionDto.LoanId;
                }

                payment.UpdatedAt = DateTime.UtcNow;

                // Apply new balance impact
                var newAmount = payment.Amount;
                var newTransactionType = payment.TransactionType;

                if (newTransactionType == "CREDIT")
                {
                    bankAccount.CurrentBalance += newAmount;
                }
                else if (newTransactionType == "DEBIT")
                {
                    bankAccount.CurrentBalance -= newAmount;
                }

                payment.BalanceAfterTransaction = bankAccount.CurrentBalance;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                // If account was changed, also update the old account's timestamp
                if (newBankAccount != null && oldBankAccountId != bankAccount.Id)
                {
                    var oldAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == oldBankAccountId);
                    if (oldAccount != null)
                    {
                        oldAccount.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Update BankTransaction record if it exists
                var bankTransaction = await _context.BankTransactions
                    .FirstOrDefaultAsync(bt => bt.Id == transactionId && bt.UserId == userId);

                if (bankTransaction != null)
                {
                    if (updateTransactionDto.Amount.HasValue)
                    {
                        bankTransaction.Amount = updateTransactionDto.Amount.Value;
                    }

                    if (!string.IsNullOrEmpty(updateTransactionDto.TransactionType))
                    {
                        bankTransaction.TransactionType = updateTransactionDto.TransactionType.ToUpper();
                    }

                    if (!string.IsNullOrEmpty(updateTransactionDto.Description))
                    {
                        bankTransaction.Description = updateTransactionDto.Description;
                    }

                    if (updateTransactionDto.Category != null)
                    {
                        bankTransaction.Category = updateTransactionDto.Category;
                    }

                    if (updateTransactionDto.ReferenceNumber != null)
                    {
                        bankTransaction.ReferenceNumber = updateTransactionDto.ReferenceNumber;
                    }

                    if (updateTransactionDto.ExternalTransactionId != null)
                    {
                        bankTransaction.ExternalTransactionId = updateTransactionDto.ExternalTransactionId;
                    }

                    if (updateTransactionDto.TransactionDate.HasValue)
                    {
                        bankTransaction.TransactionDate = updateTransactionDto.TransactionDate.Value;
                    }

                    if (updateTransactionDto.Notes != null)
                    {
                        bankTransaction.Notes = updateTransactionDto.Notes;
                    }

                    if (updateTransactionDto.Merchant != null)
                    {
                        bankTransaction.Merchant = updateTransactionDto.Merchant;
                    }

                    if (updateTransactionDto.Location != null)
                    {
                        bankTransaction.Location = updateTransactionDto.Location;
                    }

                    // Update BankAccountId if account was changed
                    if (newBankAccount != null)
                    {
                        bankTransaction.BankAccountId = newBankAccount.Id;
                    }

                    if (updateTransactionDto.IsRecurring.HasValue)
                    {
                        bankTransaction.IsRecurring = updateTransactionDto.IsRecurring.Value;
                    }

                    if (updateTransactionDto.RecurringFrequency != null)
                    {
                        bankTransaction.RecurringFrequency = updateTransactionDto.RecurringFrequency;
                    }

                    if (!string.IsNullOrEmpty(updateTransactionDto.Currency))
                    {
                        bankTransaction.Currency = updateTransactionDto.Currency.ToUpper();
                    }

                    bankTransaction.BalanceAfterTransaction = bankAccount.CurrentBalance;
                    bankTransaction.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var transactionDto = MapPaymentToBankTransactionDto(payment);
                return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto, "Transaction updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankTransactionDto>.ErrorResult($"Failed to update transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetAccountTransactionsAsync(string bankAccountId, string userId, int page = 1, int limit = 50)
        {
            try
            {
                // Use projection to avoid reading soft delete columns
                var bankAccountData = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.Id == bankAccountId && ba.UserId == userId)
                    .Select(ba => new { ba.Id })
                    .FirstOrDefaultAsync();

                if (bankAccountData == null)
                {
                    return ApiResponse<List<BankTransactionDto>>.ErrorResult("Bank account not found");
                }

                // Use projection to avoid reading soft delete columns
                var paymentsData = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.BankAccountId == bankAccountId && p.UserId == userId && p.IsBankTransaction)
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.BankAccountId,
                        p.Reference,
                        p.Status,
                        p.Method,
                        p.Notes,
                        p.Merchant,
                        p.Location,
                        p.IsRecurring,
                        p.RecurringFrequency,
                        p.Currency,
                        p.BalanceAfterTransaction,
                        p.ExternalTransactionId
                    })
                    .OrderByDescending(p => p.TransactionDate ?? p.ProcessedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();
                
                // Convert to Payment entities
                var payments = paymentsData.Select(p => new Payment
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    TransactionType = p.TransactionType,
                    Description = p.Description,
                    Category = p.Category,
                    TransactionDate = p.TransactionDate,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    BankAccountId = p.BankAccountId,
                    Reference = p.Reference,
                    Status = p.Status,
                    Method = p.Method,
                    Notes = p.Notes,
                    Merchant = p.Merchant,
                    Location = p.Location,
                    IsRecurring = p.IsRecurring,
                    RecurringFrequency = p.RecurringFrequency,
                    Currency = p.Currency,
                    BalanceAfterTransaction = p.BalanceAfterTransaction,
                    ExternalTransactionId = p.ExternalTransactionId,
                    IsBankTransaction = true,
                    IsDeleted = false
                }).ToList();

                var transactionDtos = payments.Select(MapPaymentToBankTransactionDto).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get account transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetUserTransactionsAsync(string userId, string? bankAccountId = null, string? accountType = null, int page = 1, int limit = 50)
        {
            try
            {
                var allTransactionDtos = new List<BankTransactionDto>();

                // Query BankTransactions table
                var bankTransactionsQuery = _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId);

                // Filter by bankAccountId if provided
                if (!string.IsNullOrEmpty(bankAccountId))
                {
                    bankTransactionsQuery = bankTransactionsQuery.Where(t => t.BankAccountId == bankAccountId);
                }

                var bankTransactionsData = await bankTransactionsQuery
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.UserId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction,
                        BankAccount = t.BankAccount != null ? new
                        {
                            t.BankAccount.Id,
                            t.BankAccount.AccountName,
                            t.BankAccount.AccountType
                        } : null
                    })
                    .ToListAsync();

                // Convert BankTransactions to entities
                var bankTransactions = bankTransactionsData.Select(t => new BankTransaction
                {
                    Id = t.Id,
                    BankAccountId = t.BankAccountId,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    ExternalTransactionId = t.ExternalTransactionId,
                    TransactionDate = t.TransactionDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Notes = t.Notes,
                    Merchant = t.Merchant,
                    Location = t.Location,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    Currency = t.Currency,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    IsDeleted = false,
                    BankAccount = t.BankAccount != null ? new BankAccount
                    {
                        Id = t.BankAccount.Id,
                        AccountName = t.BankAccount.AccountName,
                        AccountType = t.BankAccount.AccountType
                    } : null
                }).ToList();

                // Filter by accountType if provided
                if (!string.IsNullOrEmpty(accountType))
                {
                    bankTransactions = bankTransactions
                        .Where(t => t.BankAccount != null && t.BankAccount.AccountType.ToLower() == accountType.ToLower())
                        .ToList();
                }

                // Query Payments table (where IsBankTransaction == true)
                var paymentsQuery = _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsBankTransaction);

                // Filter by bankAccountId if provided
                if (!string.IsNullOrEmpty(bankAccountId))
                {
                    paymentsQuery = paymentsQuery.Where(p => p.BankAccountId != null && p.BankAccountId == bankAccountId);
                }

                var paymentsData = await paymentsQuery
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.BankAccountId,
                        p.Reference,
                        p.Status,
                        p.Method,
                        p.Notes,
                        p.Merchant,
                        p.Location,
                        p.IsRecurring,
                        p.RecurringFrequency,
                        p.Currency,
                        p.BalanceAfterTransaction,
                        p.ExternalTransactionId,
                        BankAccount = p.BankAccount != null ? new
                        {
                            p.BankAccount.Id,
                            p.BankAccount.AccountName,
                            p.BankAccount.AccountType
                        } : null
                    })
                    .ToListAsync();

                // Convert Payments to entities
                var payments = paymentsData.Select(p => new Payment
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    TransactionType = p.TransactionType,
                    Description = p.Description,
                    Category = p.Category,
                    TransactionDate = p.TransactionDate,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    BankAccountId = p.BankAccountId,
                    Reference = p.Reference,
                    Status = p.Status,
                    Method = p.Method,
                    Notes = p.Notes,
                    Merchant = p.Merchant,
                    Location = p.Location,
                    IsRecurring = p.IsRecurring,
                    RecurringFrequency = p.RecurringFrequency,
                    Currency = p.Currency,
                    BalanceAfterTransaction = p.BalanceAfterTransaction,
                    ExternalTransactionId = p.ExternalTransactionId,
                    IsBankTransaction = true,
                    IsDeleted = false,
                    BankAccount = p.BankAccount != null ? new BankAccount
                    {
                        Id = p.BankAccount.Id,
                        AccountName = p.BankAccount.AccountName,
                        AccountType = p.BankAccount.AccountType
                    } : null
                }).ToList();

                // Filter by accountType if provided
                if (!string.IsNullOrEmpty(accountType))
                {
                    payments = payments
                        .Where(p => p.BankAccount != null && p.BankAccount.AccountType.ToLower() == accountType.ToLower())
                        .ToList();
                }

                // Convert to DTOs and combine
                allTransactionDtos.AddRange(bankTransactions.Select(MapToBankTransactionDto));
                allTransactionDtos.AddRange(payments.Select(p => MapPaymentToBankTransactionDto(p)));

                // Remove duplicates (in case same transaction exists in both tables)
                allTransactionDtos = allTransactionDtos
                    .GroupBy(dto => dto.Id)
                    .Select(g => g.First())
                    .OrderByDescending(dto => dto.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList();

                return ApiResponse<List<BankTransactionDto>>.SuccessResult(allTransactionDtos);
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
                // Use projection to avoid reading soft delete columns
                var transactionData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => t.Id == transactionId && t.UserId == userId)
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.UserId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction,
                        BankAccount = t.BankAccount != null ? new
                        {
                            t.BankAccount.Id,
                            t.BankAccount.AccountName,
                            t.BankAccount.AccountType
                        } : null
                    })
                    .FirstOrDefaultAsync();
                
                if (transactionData == null)
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult("Transaction not found");
                }
                
                // Convert to BankTransaction entity
                var transaction = new BankTransaction
                {
                    Id = transactionData.Id,
                    BankAccountId = transactionData.BankAccountId,
                    UserId = transactionData.UserId,
                    Amount = transactionData.Amount,
                    TransactionType = transactionData.TransactionType,
                    Description = transactionData.Description,
                    Category = transactionData.Category,
                    ReferenceNumber = transactionData.ReferenceNumber,
                    ExternalTransactionId = transactionData.ExternalTransactionId,
                    TransactionDate = transactionData.TransactionDate,
                    CreatedAt = transactionData.CreatedAt,
                    UpdatedAt = transactionData.UpdatedAt,
                    Notes = transactionData.Notes,
                    Merchant = transactionData.Merchant,
                    Location = transactionData.Location,
                    IsRecurring = transactionData.IsRecurring,
                    RecurringFrequency = transactionData.RecurringFrequency,
                    Currency = transactionData.Currency,
                    BalanceAfterTransaction = transactionData.BalanceAfterTransaction,
                    IsDeleted = false,
                    BankAccount = transactionData.BankAccount != null ? new BankAccount
                    {
                        Id = transactionData.BankAccount.Id,
                        AccountName = transactionData.BankAccount.AccountName,
                        AccountType = transactionData.BankAccount.AccountType
                    } : null
                };

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

                // Query from Payments table to match the transactions listing endpoint
                // This ensures analytics count matches the actual transactions displayed
                var allTransactionsData = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && 
                               p.IsBankTransaction &&
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= startDate && 
                               p.TransactionDate <= endDate)
                    .Select(p => new
                    {
                        p.Id,
                        p.BankAccountId,
                        p.UserId,
                        p.Amount,
                        TransactionType = p.TransactionType ?? "DEBIT",
                        p.Description,
                        p.Category,
                        ReferenceNumber = p.Reference,
                        p.ExternalTransactionId,
                        TransactionDate = p.TransactionDate ?? p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.Notes,
                        p.Merchant,
                        p.Location,
                        p.IsRecurring,
                        p.RecurringFrequency,
                        p.Currency,
                        p.BalanceAfterTransaction,
                        BankAccount = p.BankAccount != null ? new
                        {
                            p.BankAccount.Id,
                            p.BankAccount.AccountName,
                            p.BankAccount.AccountType,
                            p.BankAccount.CurrentBalance
                        } : null
                    })
                    .ToListAsync();
                
                // Convert to anonymous type for analytics calculation (matching transaction structure)
                var transactions = allTransactionsData.Select(t => new
                {
                    Id = t.Id,
                    BankAccountId = t.BankAccountId,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    ExternalTransactionId = t.ExternalTransactionId,
                    TransactionDate = t.TransactionDate,
                    Notes = t.Notes,
                    Merchant = t.Merchant,
                    Location = t.Location,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    Currency = t.Currency,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    BankAccount = t.BankAccount
                }).ToList();

                // Use projection to avoid reading soft delete columns
                var bankAccountsData = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .Select(ba => new
                    {
                        ba.Id,
                        ba.UserId,
                        ba.AccountName,
                        ba.AccountType,
                        ba.InitialBalance,
                        ba.CurrentBalance,
                        ba.Currency,
                        ba.Description,
                        ba.FinancialInstitution,
                        ba.AccountNumber,
                        ba.RoutingNumber,
                        ba.SyncFrequency,
                        ba.IsConnected,
                        ba.ConnectionId,
                        ba.LastSyncedAt,
                        ba.CreatedAt,
                        ba.UpdatedAt,
                        ba.IsActive,
                        ba.Iban,
                        ba.SwiftCode
                    })
                    .ToListAsync();
                
                // Convert to BankAccount entities
                var bankAccounts = bankAccountsData.Select(ba => new BankAccount
                {
                    Id = ba.Id,
                    UserId = ba.UserId,
                    AccountName = ba.AccountName,
                    AccountType = ba.AccountType,
                    InitialBalance = ba.InitialBalance,
                    CurrentBalance = ba.CurrentBalance,
                    Currency = ba.Currency,
                    Description = ba.Description,
                    FinancialInstitution = ba.FinancialInstitution,
                    AccountNumber = ba.AccountNumber,
                    RoutingNumber = ba.RoutingNumber,
                    SyncFrequency = ba.SyncFrequency,
                    IsConnected = ba.IsConnected,
                    ConnectionId = ba.ConnectionId,
                    LastSyncedAt = ba.LastSyncedAt,
                    CreatedAt = ba.CreatedAt,
                    UpdatedAt = ba.UpdatedAt,
                    IsActive = ba.IsActive,
                    Iban = ba.Iban,
                    SwiftCode = ba.SwiftCode
                }).ToList();

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
                var allTransactions = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                // Filter out soft-deleted transactions in memory
                var transactions = allTransactions
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(limit)
                    .ToList();

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

                // Use projection to avoid reading soft delete columns
                var allTransactionsData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate &&
                               !string.IsNullOrEmpty(t.Category))
                    .Select(t => new
                    {
                        t.Category,
                        t.Amount
                    })
                    .ToListAsync();

                // Group by category (no need to filter IsDeleted since we're using projection)
                var spendingByCategory = allTransactionsData
                    .GroupBy(t => t.Category!)
                    .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                    .ToDictionary(x => x.Category, x => x.Amount);

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
                // Use projection to avoid reading soft delete columns
                var bankAccountsData = await _context.BankAccounts
                    .AsNoTracking()
                    .Select(ba => new
                    {
                        ba.Id,
                        ba.UserId,
                        ba.AccountName,
                        ba.AccountType,
                        ba.InitialBalance,
                        ba.CurrentBalance,
                        ba.Currency,
                        ba.Description,
                        ba.FinancialInstitution,
                        ba.AccountNumber,
                        ba.RoutingNumber,
                        ba.SyncFrequency,
                        ba.IsConnected,
                        ba.ConnectionId,
                        ba.LastSyncedAt,
                        ba.CreatedAt,
                        ba.UpdatedAt,
                        ba.IsActive,
                        ba.Iban,
                        ba.SwiftCode
                    })
                    .OrderByDescending(ba => ba.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();
                
                // Convert to BankAccount entities
                var bankAccounts = bankAccountsData.Select(ba => new BankAccount
                {
                    Id = ba.Id,
                    UserId = ba.UserId,
                    AccountName = ba.AccountName,
                    AccountType = ba.AccountType,
                    InitialBalance = ba.InitialBalance,
                    CurrentBalance = ba.CurrentBalance,
                    Currency = ba.Currency,
                    Description = ba.Description,
                    FinancialInstitution = ba.FinancialInstitution,
                    AccountNumber = ba.AccountNumber,
                    RoutingNumber = ba.RoutingNumber,
                    SyncFrequency = ba.SyncFrequency,
                    IsConnected = ba.IsConnected,
                    ConnectionId = ba.ConnectionId,
                    LastSyncedAt = ba.LastSyncedAt,
                    CreatedAt = ba.CreatedAt,
                    UpdatedAt = ba.UpdatedAt,
                    IsActive = ba.IsActive,
                    Iban = ba.Iban,
                    SwiftCode = ba.SwiftCode,
                    Transactions = new List<BankTransaction>()
                }).ToList();
                
                // Load transactions separately using projection
                var bankAccountIds = bankAccounts.Select(ba => ba.Id).ToList();
                var transactionsData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => bankAccountIds.Contains(t.BankAccountId))
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.UserId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction
                    })
                    .ToListAsync();
                
                // Convert to BankTransaction entities
                var transactions = transactionsData.Select(t => new BankTransaction
                {
                    Id = t.Id,
                    BankAccountId = t.BankAccountId,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    Category = t.Category,
                    ReferenceNumber = t.ReferenceNumber,
                    ExternalTransactionId = t.ExternalTransactionId,
                    TransactionDate = t.TransactionDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Notes = t.Notes,
                    Merchant = t.Merchant,
                    Location = t.Location,
                    IsRecurring = t.IsRecurring,
                    RecurringFrequency = t.RecurringFrequency,
                    Currency = t.Currency,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    IsDeleted = false
                }).ToList();
                
                // Attach transactions to bank accounts in memory
                foreach (var account in bankAccounts)
                {
                    account.Transactions = transactions
                        .Where(t => t.BankAccountId == account.Id)
                        .ToList();
                }

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
                var allTransactions = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .ToListAsync();

                // Filter out soft-deleted transactions in memory
                var transactions = allTransactions
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList();

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

                var allExpenses = await _context.BankTransactions
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               t.TransactionDate >= startDate && 
                               t.TransactionDate <= endDate)
                    .ToListAsync();

                // Filter out soft-deleted transactions in memory
                var expenses = allExpenses.Where(t => !t.IsDeleted).ToList();

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

                // Load all transactions first, then filter in memory
                var allDebitTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionType == "DEBIT")
                    .ToListAsync();

                // Filter out soft-deleted transactions in memory
                var debitTransactions = allDebitTransactions.Where(t => !t.IsDeleted).ToList();

                var todayExpenses = debitTransactions
                    .Where(t => t.TransactionDate.Date == today)
                    .Sum(t => t.Amount);

                var thisWeekExpenses = debitTransactions
                    .Where(t => t.TransactionDate >= thisWeekStart)
                    .Sum(t => t.Amount);

                var thisMonthExpenses = debitTransactions
                    .Where(t => t.TransactionDate >= thisMonthStart)
                    .Sum(t => t.Amount);

                var lastMonthExpenses = debitTransactions
                    .Where(t => t.TransactionDate >= lastMonthStart && t.TransactionDate <= lastMonthEnd)
                    .Sum(t => t.Amount);

                var topCategories = debitTransactions
                    .Where(t => t.TransactionDate >= thisMonthStart && !string.IsNullOrEmpty(t.Category))
                    .GroupBy(t => t.Category!)
                    .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                    .OrderByDescending(x => x.Amount)
                    .Take(5)
                    .ToDictionary(x => x.Category, x => x.Amount);

                var recentExpenses = debitTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .Select(t => MapToBankTransactionDto(t))
                    .ToList();

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
                var allExpenses = await _context.BankTransactions
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               t.Category == category)
                    .ToListAsync();

                // Filter out soft-deleted transactions in memory
                var expenses = allExpenses
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList();

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
                var allTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId && 
                               t.TransactionType == "DEBIT" && 
                               !string.IsNullOrEmpty(t.Category))
                    .ToListAsync();

                // Filter out soft-deleted transactions in memory, then group
                var categories = allTransactions
                    .Where(t => !t.IsDeleted)
                    .GroupBy(t => t.Category!)
                    .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                    .ToDictionary(x => x.Category, x => x.Amount);

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
                // Check Payments table first (primary storage for bank transactions)
                var payment = await _context.Payments
                    .Include(p => p.BankAccount)
                    .FirstOrDefaultAsync(p => p.Id == transactionId && p.UserId == userId && p.IsBankTransaction);

                if (payment != null)
                {
                    // Found in Payments table
                    // ==================== MONTH CLOSURE VALIDATION ====================
                    if (payment.BankAccountId != null && payment.TransactionDate.HasValue)
                    {
                        var transactionDate = payment.TransactionDate.Value;
                        var isMonthClosed = await _context.ClosedMonths
                            .AnyAsync(cm => cm.BankAccountId == payment.BankAccountId &&
                                           cm.Year == transactionDate.Year &&
                                           cm.Month == transactionDate.Month);

                        if (isMonthClosed)
                        {
                            var monthName = new[] { "", "January", "February", "March", "April", "May", "June",
                                                    "July", "August", "September", "October", "November", "December" }[transactionDate.Month];
                            return ApiResponse<bool>.ErrorResult(
                                $"Cannot delete transaction. The month {monthName} {transactionDate.Year} is closed for this account.");
                        }
                    }
                    // ==================== END MONTH CLOSURE VALIDATION ====================

                    // Check if transaction is synced from bank (read-only)
                    if (!string.IsNullOrEmpty(payment.ExternalTransactionId))
                    {
                        return ApiResponse<bool>.ErrorResult("Cannot delete transactions synced from bank");
                    }

                    // Check if transaction can be deleted based on business rules
                    var hoursSinceCreation = (DateTime.UtcNow - payment.CreatedAt).TotalHours;
                    
                    // If transaction is > 24 hours old, use soft delete instead
                    if (hoursSinceCreation > 24)
                    {
                        // Automatically use soft delete
                        return await SoftDeleteTransactionAsync(transactionId, userId, "Auto-soft-deleted: Transaction older than 24 hours");
                    }

                    // Reverse the transaction effect on the bank account balance
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

                    // Remove from Payments table
                    _context.Payments.Remove(payment);
                }

                // Also check and remove from BankTransactions table if it exists
                var bankTransaction = await _context.BankTransactions
                    .Include(t => t.BankAccount)
                    .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

                if (bankTransaction != null)
                {
                    // Check if already deleted
                    if (bankTransaction.IsDeleted)
                    {
                        return ApiResponse<bool>.ErrorResult("Transaction is already deleted");
                    }

                    // ==================== MONTH CLOSURE VALIDATION ====================
                    var transactionDate = bankTransaction.TransactionDate;
                    var isMonthClosed = await _context.ClosedMonths
                        .AnyAsync(cm => cm.BankAccountId == bankTransaction.BankAccountId &&
                                       cm.Year == transactionDate.Year &&
                                       cm.Month == transactionDate.Month);

                    if (isMonthClosed)
                    {
                        var monthName = new[] { "", "January", "February", "March", "April", "May", "June",
                                                "July", "August", "September", "October", "November", "December" }[transactionDate.Month];
                        return ApiResponse<bool>.ErrorResult(
                            $"Cannot delete transaction. The month {monthName} {transactionDate.Year} is closed for this account.");
                    }
                    // ==================== END MONTH CLOSURE VALIDATION ====================

                    // Check if transaction is synced from bank (read-only)
                    if (!string.IsNullOrEmpty(bankTransaction.ExternalTransactionId))
                    {
                        return ApiResponse<bool>.ErrorResult("Cannot delete transactions synced from bank");
                    }

                    // Reverse balance impact (only if not already reversed from Payment above)
                    if (payment == null && bankTransaction.BankAccount != null)
                    {
                        if (bankTransaction.TransactionType == "CREDIT")
                        {
                            bankTransaction.BankAccount.CurrentBalance -= bankTransaction.Amount;
                        }
                        else if (bankTransaction.TransactionType == "DEBIT")
                        {
                            bankTransaction.BankAccount.CurrentBalance += bankTransaction.Amount;
                        }
                        bankTransaction.BankAccount.UpdatedAt = DateTime.UtcNow;
                    }

                    // Remove from BankTransactions table
                    _context.BankTransactions.Remove(bankTransaction);
                }

                // If neither was found, return error
                if (payment == null && bankTransaction == null)
                {
                    return ApiResponse<bool>.ErrorResult("Transaction not found or you don't have permission to delete it");
                }

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Transaction deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete transaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Soft delete (hide) a bank transaction - for transactions older than 24 hours
        /// </summary>
        public async Task<ApiResponse<bool>> SoftDeleteTransactionAsync(string transactionId, string userId, string? reason = null)
        {
            try
            {
                // Check Payments table FIRST (primary storage for bank transactions)
                var paymentTransactionData = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.Id == transactionId && p.UserId == userId && p.IsBankTransaction)
                    .Select(p => new
                    {
                        p.Id,
                        p.BankAccountId,
                        p.Amount,
                        p.TransactionType,
                        p.ExternalTransactionId
                    })
                    .FirstOrDefaultAsync();

                if (paymentTransactionData != null)
                {
                    // Check if transaction is synced from bank (read-only)
                    if (!string.IsNullOrEmpty(paymentTransactionData.ExternalTransactionId))
                    {
                        return ApiResponse<bool>.ErrorResult("Cannot delete transactions synced from bank");
                    }

                    if (string.IsNullOrEmpty(paymentTransactionData.TransactionType) || paymentTransactionData.BankAccountId == null)
                    {
                        return ApiResponse<bool>.ErrorResult("Invalid transaction data");
                    }

                    // Load BankAccount separately using projection
                    var bankAccountData = await _context.BankAccounts
                        .AsNoTracking()
                        .Where(ba => ba.Id == paymentTransactionData.BankAccountId)
                        .Select(ba => new
                        {
                            ba.Id,
                            ba.CurrentBalance
                        })
                        .FirstOrDefaultAsync();

                    if (bankAccountData == null)
                    {
                        return ApiResponse<bool>.ErrorResult("Bank account not found");
                    }

                    // Calculate new balance
                    decimal newBalance = bankAccountData.CurrentBalance;
                    if (paymentTransactionData.TransactionType == "CREDIT")
                    {
                        newBalance -= paymentTransactionData.Amount;
                    }
                    else if (paymentTransactionData.TransactionType == "DEBIT")
                    {
                        newBalance += paymentTransactionData.Amount;
                    }

                    // Update BankAccount balance using raw SQL
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE BankAccounts 
                           SET CurrentBalance = {newBalance}, 
                               UpdatedAt = {DateTime.UtcNow}
                           WHERE Id = {paymentTransactionData.BankAccountId}");

                    // Update payment soft delete properties using raw SQL
                    try
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync(
                            $@"UPDATE Payments 
                               SET IsDeleted = 1, 
                                   DeletedAt = {DateTime.UtcNow}, 
                                   DeletedBy = {userId}, 
                                   DeleteReason = {reason ?? (string?)null}, 
                                   UpdatedAt = {DateTime.UtcNow}
                               WHERE Id = {paymentTransactionData.Id}");
                    }
                    catch
                    {
                        // If soft delete columns don't exist, we can't perform soft delete
                        return ApiResponse<bool>.ErrorResult("Soft delete columns do not exist. Please run the database migration first.");
                    }

                    return ApiResponse<bool>.SuccessResult(true, "Transaction hidden successfully");
                }

                // If not found in Payments, check BankTransactions table (legacy/fallback)
                var bankTransactionData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => t.Id == transactionId && t.UserId == userId)
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.Amount,
                        t.TransactionType,
                        t.ExternalTransactionId
                    })
                    .FirstOrDefaultAsync();

                if (bankTransactionData != null)
                {
                    // Check if transaction is synced from bank (read-only)
                    if (!string.IsNullOrEmpty(bankTransactionData.ExternalTransactionId))
                    {
                        return ApiResponse<bool>.ErrorResult("Cannot delete transactions synced from bank");
                    }

                    // Load BankAccount separately using projection
                    var bankAccountData = await _context.BankAccounts
                        .AsNoTracking()
                        .Where(ba => ba.Id == bankTransactionData.BankAccountId)
                        .Select(ba => new
                        {
                            ba.Id,
                            ba.CurrentBalance
                        })
                        .FirstOrDefaultAsync();

                    if (bankAccountData == null)
                    {
                        return ApiResponse<bool>.ErrorResult("Bank account not found");
                    }

                    // Calculate new balance
                    decimal newBalance = bankAccountData.CurrentBalance;
                    if (bankTransactionData.TransactionType == "CREDIT")
                    {
                        newBalance -= bankTransactionData.Amount;
                    }
                    else if (bankTransactionData.TransactionType == "DEBIT")
                    {
                        newBalance += bankTransactionData.Amount;
                    }

                    // Update BankAccount balance using raw SQL to avoid materializing soft delete columns
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE BankAccounts 
                           SET CurrentBalance = {newBalance}, 
                               UpdatedAt = {DateTime.UtcNow}
                           WHERE Id = {bankTransactionData.BankAccountId}");

                    // Update transaction soft delete properties using raw SQL
                    // Note: This will fail if columns don't exist, but that's expected until migration is run
                    try
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync(
                            $@"UPDATE BankTransactions 
                               SET IsDeleted = 1, 
                                   DeletedAt = {DateTime.UtcNow}, 
                                   DeletedBy = {userId}, 
                                   DeleteReason = {reason ?? (string?)null}, 
                                   UpdatedAt = {DateTime.UtcNow}
                               WHERE Id = {bankTransactionData.Id}");
                    }
                    catch
                    {
                        // If soft delete columns don't exist, we can't perform soft delete
                        // This is expected until migration is run
                        return ApiResponse<bool>.ErrorResult("Soft delete columns do not exist. Please run the database migration first.");
                    }

                    return ApiResponse<bool>.SuccessResult(true, "Transaction hidden successfully");
                }

                // Transaction not found in either table
                return ApiResponse<bool>.ErrorResult("Transaction not found or you don't have permission to delete it");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to hide transaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Restore a soft-deleted (hidden) bank transaction
        /// </summary>
        public async Task<ApiResponse<BankTransactionDto>> RestoreTransactionAsync(string transactionId, string userId)
        {
            try
            {
                // Check Payments table FIRST (primary storage for bank transactions)
                var paymentTransactionData = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.Id == transactionId && p.UserId == userId && p.IsBankTransaction)
                    .Select(p => new
                    {
                        p.Id,
                        p.BankAccountId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.Reference,
                        p.ExternalTransactionId,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.Notes,
                        p.Merchant,
                        p.Location,
                        p.IsRecurring,
                        p.RecurringFrequency,
                        p.Currency,
                        p.BalanceAfterTransaction
                    })
                    .FirstOrDefaultAsync();

                if (paymentTransactionData != null)
                {
                    if (string.IsNullOrEmpty(paymentTransactionData.TransactionType) || paymentTransactionData.BankAccountId == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Invalid transaction data");
                    }

                    // Load BankAccount separately using projection
                    var bankAccountData = await _context.BankAccounts
                        .AsNoTracking()
                        .Where(ba => ba.Id == paymentTransactionData.BankAccountId)
                        .Select(ba => new
                        {
                            ba.Id,
                            ba.CurrentBalance
                        })
                        .FirstOrDefaultAsync();

                    if (bankAccountData == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Bank account not found");
                    }

                    // Calculate new balance (re-apply transaction effect)
                    decimal newBalance = bankAccountData.CurrentBalance;
                    if (paymentTransactionData.TransactionType == "CREDIT")
                    {
                        newBalance += paymentTransactionData.Amount;
                    }
                    else if (paymentTransactionData.TransactionType == "DEBIT")
                    {
                        newBalance -= paymentTransactionData.Amount;
                    }

                    // Update BankAccount balance using raw SQL
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE BankAccounts 
                           SET CurrentBalance = {newBalance}, 
                               UpdatedAt = {DateTime.UtcNow}
                           WHERE Id = {paymentTransactionData.BankAccountId}");

                    // Restore payment using raw SQL
                    try
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync(
                            $@"UPDATE Payments 
                               SET IsDeleted = 0, 
                                   DeletedAt = NULL, 
                                   DeletedBy = NULL, 
                                   DeleteReason = NULL, 
                                   UpdatedAt = {DateTime.UtcNow}
                               WHERE Id = {paymentTransactionData.Id}");
                    }
                    catch
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Soft delete columns do not exist. Please run the database migration first.");
                    }

                    // Create transaction DTO from projected data
                    var transactionDto = new BankTransactionDto
                    {
                        Id = paymentTransactionData.Id,
                        BankAccountId = paymentTransactionData.BankAccountId ?? "",
                        UserId = userId,
                        Amount = paymentTransactionData.Amount,
                        TransactionType = paymentTransactionData.TransactionType ?? "UNKNOWN",
                        Description = paymentTransactionData.Description ?? "",
                        Category = paymentTransactionData.Category,
                        ReferenceNumber = paymentTransactionData.Reference,
                        ExternalTransactionId = paymentTransactionData.ExternalTransactionId,
                        TransactionDate = paymentTransactionData.TransactionDate ?? paymentTransactionData.ProcessedAt,
                        CreatedAt = paymentTransactionData.CreatedAt,
                        UpdatedAt = DateTime.UtcNow,
                        Notes = paymentTransactionData.Notes,
                        Merchant = paymentTransactionData.Merchant,
                        Location = paymentTransactionData.Location,
                        IsRecurring = paymentTransactionData.IsRecurring,
                        RecurringFrequency = paymentTransactionData.RecurringFrequency,
                        Currency = paymentTransactionData.Currency,
                        BalanceAfterTransaction = paymentTransactionData.BalanceAfterTransaction ?? 0
                    };

                    return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto, "Transaction restored successfully");
                }

                // If not found in Payments, check BankTransactions table (legacy/fallback)
                var bankTransactionData = await _context.BankTransactions
                    .AsNoTracking()
                    .Where(t => t.Id == transactionId && t.UserId == userId)
                    .Select(t => new
                    {
                        t.Id,
                        t.BankAccountId,
                        t.Amount,
                        t.TransactionType,
                        t.Description,
                        t.Category,
                        t.ReferenceNumber,
                        t.ExternalTransactionId,
                        t.TransactionDate,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Notes,
                        t.Merchant,
                        t.Location,
                        t.IsRecurring,
                        t.RecurringFrequency,
                        t.Currency,
                        t.BalanceAfterTransaction
                    })
                    .FirstOrDefaultAsync();

                if (bankTransactionData != null)
                {
                    // Load BankAccount separately using projection
                    var bankAccountData = await _context.BankAccounts
                        .AsNoTracking()
                        .Where(ba => ba.Id == bankTransactionData.BankAccountId)
                        .Select(ba => new
                        {
                            ba.Id,
                            ba.CurrentBalance
                        })
                        .FirstOrDefaultAsync();

                    if (bankAccountData == null)
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Bank account not found");
                    }

                    // Calculate new balance (re-apply transaction effect)
                    decimal newBalance = bankAccountData.CurrentBalance;
                    if (bankTransactionData.TransactionType == "CREDIT")
                    {
                        newBalance += bankTransactionData.Amount;
                    }
                    else if (bankTransactionData.TransactionType == "DEBIT")
                    {
                        newBalance -= bankTransactionData.Amount;
                    }

                    // Update BankAccount balance using raw SQL
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE BankAccounts 
                           SET CurrentBalance = {newBalance}, 
                               UpdatedAt = {DateTime.UtcNow}
                           WHERE Id = {bankTransactionData.BankAccountId}");

                    // Restore transaction using raw SQL
                    try
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync(
                            $@"UPDATE BankTransactions 
                               SET IsDeleted = 0, 
                                   DeletedAt = NULL, 
                                   DeletedBy = NULL, 
                                   DeleteReason = NULL, 
                                   UpdatedAt = {DateTime.UtcNow}
                               WHERE Id = {bankTransactionData.Id}");
                    }
                    catch
                    {
                        return ApiResponse<BankTransactionDto>.ErrorResult("Soft delete columns do not exist. Please run the database migration first.");
                    }

                    // Create transaction DTO from projected data
                    var transactionDto = new BankTransactionDto
                    {
                        Id = bankTransactionData.Id,
                        BankAccountId = bankTransactionData.BankAccountId,
                        UserId = userId,
                        Amount = bankTransactionData.Amount,
                        TransactionType = bankTransactionData.TransactionType ?? "UNKNOWN",
                        Description = bankTransactionData.Description ?? "",
                        Category = bankTransactionData.Category,
                        ReferenceNumber = bankTransactionData.ReferenceNumber,
                        ExternalTransactionId = bankTransactionData.ExternalTransactionId,
                        TransactionDate = bankTransactionData.TransactionDate,
                        CreatedAt = bankTransactionData.CreatedAt,
                        UpdatedAt = DateTime.UtcNow,
                        Notes = bankTransactionData.Notes,
                        Merchant = bankTransactionData.Merchant,
                        Location = bankTransactionData.Location,
                        IsRecurring = bankTransactionData.IsRecurring,
                        RecurringFrequency = bankTransactionData.RecurringFrequency,
                        Currency = bankTransactionData.Currency,
                        BalanceAfterTransaction = bankTransactionData.BalanceAfterTransaction
                    };

                    return ApiResponse<BankTransactionDto>.SuccessResult(transactionDto, "Transaction restored successfully");
                }

                // Transaction not found in either table
                return ApiResponse<BankTransactionDto>.ErrorResult("Transaction not found or you don't have permission to restore it");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankTransactionDto>.ErrorResult($"Failed to restore transaction: {ex.Message}");
            }
        }

        // Helper Methods
        private async Task<BankAccountDto> MapToBankAccountDtoAsync(BankAccount bankAccount, dynamic? transactionStats = null)
        {
            var transactions = bankAccount.Transactions ?? new List<BankTransaction>();
            
            // Use provided stats if available, otherwise calculate from transactions
            int transactionCount;
            decimal totalIncoming;
            decimal totalOutgoing;
            
            if (transactionStats != null)
            {
                transactionCount = transactionStats.TransactionCount;
                totalIncoming = transactionStats.TotalIncoming ?? 0m;
                totalOutgoing = transactionStats.TotalOutgoing ?? 0m;
            }
            else
            {
                transactionCount = transactions.Count;
                totalIncoming = transactions.Where(t => t.TransactionType == "CREDIT").Sum(t => t.Amount);
                totalOutgoing = transactions.Where(t => t.TransactionType == "DEBIT").Sum(t => t.Amount);
            }
            
            // Load cards if not already loaded (handle case where Cards table doesn't exist)
            List<CardDto> cards = new List<CardDto>();
            try
            {
                // Try to load Cards collection - use direct query instead of Entry to avoid materializing entity
                try
                {
                    // Check if bankAccount is tracked by EF Core
                    var entry = _context.ChangeTracker.Entries<BankAccount>()
                        .FirstOrDefault(e => e.Entity.Id == bankAccount.Id);
                    
                    if (entry != null)
                    {
                        // Entity is tracked, check if Cards are loaded
                        var isCardsLoaded = entry.Collection(ba => ba.Cards).IsLoaded;
                        
                        if (!isCardsLoaded)
                        {
                            await entry.Collection(ba => ba.Cards).LoadAsync();
                        }
                    }
                    else
                    {
                        // Entity is not tracked (manually created), load cards directly
                        var bankAccountCards = await _context.Cards
                            .Where(c => c.BankAccountId == bankAccount.Id && !c.IsDeleted)
                            .ToListAsync();
                        bankAccount.Cards = bankAccountCards;
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
                        TransactionCount = transactionCount,
                        TotalIncoming = totalIncoming,
                        TotalOutgoing = totalOutgoing,
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
                BalanceAfterTransaction = transaction.BalanceAfterTransaction,
                // Link fields
                BillId = transaction.BillId,
                LoanId = transaction.LoanId,
                SavingsAccountId = transaction.SavingsAccountId,
                TransactionPurpose = transaction.TransactionPurpose,
                // Related entity names
                BillName = transaction.Bill?.BillName,
                LoanPurpose = transaction.Loan?.Purpose,
                SavingsAccountName = transaction.SavingsAccount?.AccountName
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
                BalanceAfterTransaction = payment.BalanceAfterTransaction ?? 0,
                // Link fields from Payment entity
                BillId = payment.BillId,
                LoanId = payment.LoanId,
                SavingsAccountId = payment.SavingsAccountId,
                // TransactionPurpose can be derived from linked entities
                TransactionPurpose = payment.BillId != null 
                    ? (payment.Bill?.BillType.ToUpper() == "UTILITY" ? "UTILITY" : "BILL")
                    : payment.LoanId != null ? "LOAN"
                    : payment.SavingsAccountId != null ? "SAVINGS"
                    : null,
                // Related entity names
                BillName = payment.Bill?.BillName,
                LoanPurpose = payment.Loan?.Purpose,
                SavingsAccountName = payment.SavingsAccount?.AccountName
            };
        }

        private static (DateTime startDate, DateTime endDate) GetPeriodDates(string period)
        {
            var now = DateTime.UtcNow;
            
            // Set endDate to end of today (23:59:59.9999999) to include all transactions on the current day
            var endOfToday = now.Date.AddDays(1).AddTicks(-1);
            
            // Start of current month (1st day of month at 00:00:00)
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            
            return period.ToLower() switch
            {
                "weekly" or "week" => (now.AddDays(-7).Date, endOfToday),
                "monthly" or "month" => (startOfCurrentMonth, endOfToday), // Current calendar month instead of last 30 days
                "quarterly" or "quarter" => (now.AddDays(-90).Date, endOfToday),
                "yearly" or "year" => (now.AddDays(-365).Date, endOfToday),
                _ => (startOfCurrentMonth, endOfToday) // Default to current month instead of last 30 days
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
                // Use projection to avoid reading soft delete columns
                var bankAccountData = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.Id == bankAccountId && ba.UserId == userId)
                    .Select(ba => new { ba.Id })
                    .FirstOrDefaultAsync();

                if (bankAccountData == null)
                {
                    return ApiResponse<List<BankTransactionDto>>.ErrorResult("Bank account not found");
                }

                // Use projection to avoid reading soft delete columns
                var query = _context.Payments
                    .AsNoTracking()
                    .Where(p => p.BankAccountId == bankAccountId && p.UserId == userId && p.IsBankTransaction)
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.BankAccountId,
                        p.Reference,
                        p.Status,
                        p.Method,
                        p.Notes,
                        p.Merchant,
                        p.Location,
                        p.IsRecurring,
                        p.RecurringFrequency,
                        p.Currency,
                        p.BalanceAfterTransaction,
                        p.ExternalTransactionId,
                        BankAccount = p.BankAccount != null ? new
                        {
                            p.BankAccount.Id,
                            p.BankAccount.AccountName,
                            p.BankAccount.AccountType
                        } : null
                    });

                if (dateFrom.HasValue)
                {
                    query = query.Where(p => p.TransactionDate >= dateFrom.Value);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(p => p.TransactionDate <= dateTo.Value);
                }

                var transactionsData = await query
                    .OrderByDescending(p => p.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();
                
                // Convert to Payment entities with BankAccount
                var transactions = transactionsData.Select(p => new Payment
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    TransactionType = p.TransactionType,
                    Description = p.Description,
                    Category = p.Category,
                    TransactionDate = p.TransactionDate,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    BankAccountId = p.BankAccountId,
                    Reference = p.Reference,
                    Status = p.Status,
                    Method = p.Method,
                    Notes = p.Notes,
                    Merchant = p.Merchant,
                    Location = p.Location,
                    IsRecurring = p.IsRecurring,
                    RecurringFrequency = p.RecurringFrequency,
                    Currency = p.Currency,
                    BalanceAfterTransaction = p.BalanceAfterTransaction,
                    ExternalTransactionId = p.ExternalTransactionId,
                    IsBankTransaction = true,
                    IsDeleted = false,
                    BankAccount = p.BankAccount != null ? new BankAccount
                    {
                        Id = p.BankAccount.Id,
                        AccountName = p.BankAccount.AccountName,
                        AccountType = p.BankAccount.AccountType
                    } : null
                }).ToList();

                var transactionDtos = transactions.Select(p => MapPaymentToBankTransactionDto(p)).ToList();
                return ApiResponse<List<BankTransactionDto>>.SuccessResult(transactionDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get account transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankTransactionDto>>> GetUserTransactionsAsync(string userId, string? bankAccountId = null, string? accountType = null, int page = 1, int limit = 50, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                // Use projection to avoid reading soft delete columns
                var query = _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsBankTransaction);

                // Filter by bankAccountId if provided
                if (!string.IsNullOrEmpty(bankAccountId))
                {
                    query = query.Where(p => p.BankAccountId == bankAccountId);
                }

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

                var transactionsQuery = query
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.BankAccountId,
                        p.Reference,
                        p.Status,
                        p.Method,
                        p.Notes,
                        p.Merchant,
                        p.Location,
                        p.IsRecurring,
                        p.RecurringFrequency,
                        p.Currency,
                        p.BalanceAfterTransaction,
                        p.ExternalTransactionId,
                        BankAccount = p.BankAccount != null ? new
                        {
                            p.BankAccount.Id,
                            p.BankAccount.AccountName,
                            p.BankAccount.AccountType
                        } : null
                    });

                var transactionsData = await transactionsQuery
                    .OrderByDescending(p => p.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Convert to Payment entities with BankAccount
                var transactions = transactionsData.Select(p => new Payment
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    TransactionType = p.TransactionType,
                    Description = p.Description,
                    Category = p.Category,
                    TransactionDate = p.TransactionDate,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    BankAccountId = p.BankAccountId,
                    Reference = p.Reference,
                    Status = p.Status,
                    Method = p.Method,
                    Notes = p.Notes,
                    Merchant = p.Merchant,
                    Location = p.Location,
                    IsRecurring = p.IsRecurring,
                    RecurringFrequency = p.RecurringFrequency,
                    Currency = p.Currency,
                    BalanceAfterTransaction = p.BalanceAfterTransaction,
                    ExternalTransactionId = p.ExternalTransactionId,
                    IsBankTransaction = true,
                    IsDeleted = false,
                    BankAccount = p.BankAccount != null ? new BankAccount
                    {
                        Id = p.BankAccount.Id,
                        AccountName = p.BankAccount.AccountName,
                        AccountType = p.BankAccount.AccountType
                    } : null
                }).ToList();

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

                // Use projection to avoid reading soft delete columns
                var accountsData = await _context.BankAccounts
                    .AsNoTracking()
                    .Where(ba => ba.UserId == userId && ba.IsActive)
                    .Select(ba => new
                    {
                        ba.Id,
                        ba.UserId,
                        ba.AccountName,
                        ba.AccountType,
                        ba.InitialBalance,
                        ba.CurrentBalance,
                        ba.Currency,
                        ba.Description,
                        ba.FinancialInstitution,
                        ba.AccountNumber,
                        ba.RoutingNumber,
                        ba.SyncFrequency,
                        ba.IsConnected,
                        ba.ConnectionId,
                        ba.LastSyncedAt,
                        ba.CreatedAt,
                        ba.UpdatedAt,
                        ba.IsActive,
                        ba.Iban,
                        ba.SwiftCode
                    })
                    .ToListAsync();
                
                // Get bank account IDs for transaction lookup
                var bankAccountIds = accountsData.Select(ba => ba.Id).ToList();
                
                // Get total count of ALL transactions (not filtered by period) for TransactionCount
                var totalTransactionCount = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsBankTransaction)
                    .CountAsync();
                
                // Get aggregated transaction statistics from Payments table grouped by BankAccountId (ALL transactions, not just period)
                var transactionStats = await _context.Payments
                    .AsNoTracking()
                    .Where(p => bankAccountIds.Contains(p.BankAccountId) && 
                               p.IsBankTransaction && 
                               !p.IsDeleted &&
                               p.BankAccountId != null)
                    .GroupBy(p => p.BankAccountId!)
                    .Select(g => new
                    {
                        BankAccountId = g.Key,
                        TransactionCount = g.Count(),
                        TotalIncoming = g.Where(p => p.TransactionType == "CREDIT").Sum(p => p.Amount),
                        TotalOutgoing = g.Where(p => p.TransactionType == "DEBIT").Sum(p => p.Amount)
                    })
                    .ToListAsync();
                
                // Create dictionary for efficient lookup
                var transactionStatsByAccountId = transactionStats
                    .ToDictionary(ts => ts.BankAccountId, ts => new
                    {
                        ts.TransactionCount,
                        ts.TotalIncoming,
                        ts.TotalOutgoing
                    });
                
                // Convert to BankAccount entities
                var accounts = accountsData.Select(ba => new BankAccount
                {
                    Id = ba.Id,
                    UserId = ba.UserId,
                    AccountName = ba.AccountName,
                    AccountType = ba.AccountType,
                    InitialBalance = ba.InitialBalance,
                    CurrentBalance = ba.CurrentBalance,
                    Currency = ba.Currency,
                    Description = ba.Description,
                    FinancialInstitution = ba.FinancialInstitution,
                    AccountNumber = ba.AccountNumber,
                    RoutingNumber = ba.RoutingNumber,
                    SyncFrequency = ba.SyncFrequency,
                    IsConnected = ba.IsConnected,
                    ConnectionId = ba.ConnectionId,
                    LastSyncedAt = ba.LastSyncedAt,
                    CreatedAt = ba.CreatedAt,
                    UpdatedAt = ba.UpdatedAt,
                    IsActive = ba.IsActive,
                    Iban = ba.Iban,
                    SwiftCode = ba.SwiftCode,
                    Transactions = new List<BankTransaction>() // Empty list - we use aggregated stats from Payments
                }).ToList();

                // Use projection to avoid reading soft delete columns (period transactions for summary calculations)
                var transactionsData = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && 
                               p.IsBankTransaction && 
                               p.TransactionDate.HasValue &&
                               p.TransactionDate >= startDate && 
                               p.TransactionDate <= endDate)
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.Amount,
                        p.TransactionType,
                        p.Description,
                        p.Category,
                        p.TransactionDate,
                        p.ProcessedAt,
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.BankAccountId,
                        p.Reference,
                        p.Status,
                        p.Method,
                        p.Notes,
                        p.Merchant,
                        p.Location,
                        p.IsRecurring,
                        p.RecurringFrequency,
                        p.Currency,
                        p.BalanceAfterTransaction,
                        p.ExternalTransactionId
                    })
                    .ToListAsync();
                
                // Convert to Payment entities
                var transactions = transactionsData.Select(p => new Payment
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    TransactionType = p.TransactionType,
                    Description = p.Description,
                    Category = p.Category,
                    TransactionDate = p.TransactionDate,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    BankAccountId = p.BankAccountId,
                    Reference = p.Reference,
                    Status = p.Status,
                    Method = p.Method,
                    Notes = p.Notes,
                    Merchant = p.Merchant,
                    Location = p.Location,
                    IsRecurring = p.IsRecurring,
                    RecurringFrequency = p.RecurringFrequency,
                    Currency = p.Currency,
                    BalanceAfterTransaction = p.BalanceAfterTransaction,
                    ExternalTransactionId = p.ExternalTransactionId,
                    IsBankTransaction = true,
                    IsDeleted = false
                }).ToList();

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
                    TransactionCount = totalTransactionCount, // Total count of ALL transactions, not just period
                    Accounts = new List<BankAccountDto>(),
                    SpendingByCategory = transactions
                        .Where(t => t.TransactionType == "DEBIT" && !string.IsNullOrEmpty(t.Category))
                        .GroupBy(t => t.Category!)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount))
                };

                // Map accounts with transaction stats
                foreach (var account in accounts)
                {
                    // Pass transaction stats if available
                    var stats = transactionStatsByAccountId.TryGetValue(account.Id, out var accountStats) 
                        ? accountStats 
                        : null;
                    summary.Accounts.Add(await MapToBankAccountDtoAsync(account, stats));
                }

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

        // Month Closing Methods
        public async Task<ApiResponse<ClosedMonthDto>> CloseMonthAsync(string bankAccountId, CloseMonthDto closeMonthDto, string userId)
        {
            try
            {
                // Validate bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<ClosedMonthDto>.ErrorResult("Bank account not found");
                }

                // Validate month range
                if (closeMonthDto.Month < 1 || closeMonthDto.Month > 12)
                {
                    return ApiResponse<ClosedMonthDto>.ErrorResult("Month must be between 1 and 12");
                }

                // Check if month is already closed
                var existingClosedMonth = await _context.ClosedMonths
                    .FirstOrDefaultAsync(cm => cm.BankAccountId == bankAccountId && 
                                              cm.Year == closeMonthDto.Year && 
                                              cm.Month == closeMonthDto.Month);

                if (existingClosedMonth != null)
                {
                    return ApiResponse<ClosedMonthDto>.ErrorResult($"Month {closeMonthDto.Month}/{closeMonthDto.Year} is already closed");
                }

                // Validate that the month being closed is not in the future
                var currentDate = DateTime.UtcNow;
                var closeDate = new DateTime(closeMonthDto.Year, closeMonthDto.Month, 1);
                if (closeDate > currentDate)
                {
                    return ApiResponse<ClosedMonthDto>.ErrorResult("Cannot close a future month");
                }

                // Create closed month record
                var closedMonth = new ClosedMonth
                {
                    Id = Guid.NewGuid().ToString(),
                    BankAccountId = bankAccountId,
                    Year = closeMonthDto.Year,
                    Month = closeMonthDto.Month,
                    ClosedBy = userId,
                    ClosedAt = DateTime.UtcNow,
                    Notes = closeMonthDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ClosedMonths.Add(closedMonth);
                await _context.SaveChangesAsync();

                // Map to DTO
                var closedMonthDto = await MapToClosedMonthDtoAsync(closedMonth);
                return ApiResponse<ClosedMonthDto>.SuccessResult(closedMonthDto, "Month closed successfully");
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                if (innerException.Contains("IX_ClosedMonths_BankAccountId_Year_Month") || innerException.Contains("duplicate key"))
                {
                    return ApiResponse<ClosedMonthDto>.ErrorResult($"Month {closeMonthDto.Month}/{closeMonthDto.Year} is already closed");
                }
                return ApiResponse<ClosedMonthDto>.ErrorResult($"Failed to close month: {innerException}");
            }
            catch (Exception ex)
            {
                return ApiResponse<ClosedMonthDto>.ErrorResult($"Failed to close month: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ClosedMonthDto>>> GetClosedMonthsAsync(string bankAccountId, string userId)
        {
            try
            {
                // Validate bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<List<ClosedMonthDto>>.ErrorResult("Bank account not found");
                }

                var closedMonths = await _context.ClosedMonths
                    .Include(cm => cm.BankAccount)
                    .Include(cm => cm.User)
                    .Where(cm => cm.BankAccountId == bankAccountId)
                    .OrderByDescending(cm => cm.Year)
                    .ThenByDescending(cm => cm.Month)
                    .ToListAsync();

                var closedMonthDtos = new List<ClosedMonthDto>();
                foreach (var closedMonth in closedMonths)
                {
                    closedMonthDtos.Add(await MapToClosedMonthDtoAsync(closedMonth));
                }

                return ApiResponse<List<ClosedMonthDto>>.SuccessResult(closedMonthDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ClosedMonthDto>>.ErrorResult($"Failed to get closed months: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> IsMonthClosedAsync(string bankAccountId, int year, int month, string userId)
        {
            try
            {
                // Validate bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bank account not found");
                }

                var isClosed = await _context.ClosedMonths
                    .AnyAsync(cm => cm.BankAccountId == bankAccountId && 
                                   cm.Year == year && 
                                   cm.Month == month);

                return ApiResponse<bool>.SuccessResult(isClosed);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to check month closure status: {ex.Message}");
            }
        }

        private async Task<ClosedMonthDto> MapToClosedMonthDtoAsync(ClosedMonth closedMonth)
        {
            var bankAccount = await _context.BankAccounts
                .FirstOrDefaultAsync(ba => ba.Id == closedMonth.BankAccountId);
            
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == closedMonth.ClosedBy);

            var monthNames = new[] { "", "January", "February", "March", "April", "May", "June", 
                                    "July", "August", "September", "October", "November", "December" };

            return new ClosedMonthDto
            {
                Id = closedMonth.Id,
                BankAccountId = closedMonth.BankAccountId,
                BankAccountName = bankAccount?.AccountName,
                Year = closedMonth.Year,
                Month = closedMonth.Month,
                MonthName = closedMonth.Month >= 1 && closedMonth.Month <= 12 ? monthNames[closedMonth.Month] : "",
                ClosedBy = closedMonth.ClosedBy,
                ClosedByName = user?.Email ?? user?.Name,
                ClosedAt = closedMonth.ClosedAt,
                Notes = closedMonth.Notes,
                CreatedAt = closedMonth.CreatedAt
            };
        }
    }
}
