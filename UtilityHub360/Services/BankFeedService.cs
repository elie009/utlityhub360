using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Bank feed service implementation using Plaid
    /// </summary>
    public class BankFeedService : IBankFeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BankFeedService> _logger;
        private readonly PlaidService _plaidService;

        public BankFeedService(
            ApplicationDbContext context, 
            ILogger<BankFeedService> logger,
            PlaidService plaidService)
        {
            _context = context;
            _logger = logger;
            _plaidService = plaidService;
        }

        /// <summary>
        /// Store Plaid access token after public token exchange
        /// This is called after the user completes Plaid Link flow
        /// </summary>
        public async Task<ApiResponse<string>> StorePlaidAccessTokenAsync(string userId, string bankAccountId, string accessToken, string itemId)
        {
            try
            {
                var account = await _context.BankAccounts
                    .FirstOrDefaultAsync(a => a.Id == bankAccountId && a.UserId == userId);

                if (account == null)
                {
                    return ApiResponse<string>.ErrorResult("Bank account not found");
                }

                // Fetch account info from Plaid to update our local account
                var plaidAccounts = await _plaidService.GetAccountsAsync(accessToken);
                
                // Find matching account by account_id if available, or use first account
                var plaidAccount = plaidAccounts.Accounts.FirstOrDefault();

                if (plaidAccount != null)
                {
                    // Update account with Plaid data
                    account.FinancialInstitution = plaidAccount.Name ?? account.FinancialInstitution;
                    account.AccountNumber = plaidAccount.Mask != null ? $"****{plaidAccount.Mask}" : account.AccountNumber;
                    
                    // Update balance from Plaid
                    if (plaidAccount.Balances?.Current != null)
                    {
                        account.CurrentBalance = (decimal)plaidAccount.Balances.Current;
                    }
                }

                account.IsConnected = true;
                account.ConnectionId = accessToken; // Store access token in ConnectionId
                account.LastSyncedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bank account {AccountId} connected to Plaid with item {ItemId}", bankAccountId, itemId);

                return ApiResponse<string>.SuccessResult(accessToken, "Account connected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing Plaid access token for account {AccountId}", bankAccountId);
                return ApiResponse<string>.ErrorResult($"Failed to connect account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ConnectAccountAsync(string userId, string bankAccountId, string provider, Dictionary<string, string> credentials)
        {
            // This method is kept for backward compatibility
            // For Plaid, use StorePlaidAccessTokenAsync instead
            if (provider.ToLower() == "plaid" && credentials.ContainsKey("accessToken"))
            {
                var itemId = credentials.ContainsKey("itemId") ? credentials["itemId"] : string.Empty;
                return await StorePlaidAccessTokenAsync(userId, bankAccountId, credentials["accessToken"], itemId);
            }

            // Fallback for other providers or mock
            try
            {
                var account = await _context.BankAccounts
                    .FirstOrDefaultAsync(a => a.Id == bankAccountId && a.UserId == userId);

                if (account == null)
                {
                    return ApiResponse<string>.ErrorResult("Bank account not found");
                }

                account.IsConnected = true;
                account.ConnectionId = $"MOCK_{provider}_{Guid.NewGuid()}";
                account.LastSyncedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bank account {AccountId} connected to {Provider}", bankAccountId, provider);

                return ApiResponse<string>.SuccessResult(account.ConnectionId, "Account connected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting bank account {AccountId}", bankAccountId);
                return ApiResponse<string>.ErrorResult($"Failed to connect account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DisconnectAccountAsync(string bankAccountId)
        {
            try
            {
                var account = await _context.BankAccounts
                    .FirstOrDefaultAsync(a => a.Id == bankAccountId);

                if (account == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bank account not found");
                }

                // If connected via Plaid, remove the item from Plaid
                if (account.IsConnected && !string.IsNullOrEmpty(account.ConnectionId) && 
                    !account.ConnectionId.StartsWith("MOCK_"))
                {
                    try
                    {
                        await _plaidService.RemoveItemAsync(account.ConnectionId);
                        _logger.LogInformation("Removed Plaid item for account {AccountId}", bankAccountId);
                    }
                    catch (Exception plaidEx)
                    {
                        _logger.LogWarning(plaidEx, "Failed to remove Plaid item for account {AccountId}, continuing with local disconnect", bankAccountId);
                        // Continue with local disconnect even if Plaid removal fails
                    }
                }

                account.IsConnected = false;
                account.ConnectionId = null;
                account.LastSyncedAt = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bank account {AccountId} disconnected", bankAccountId);

                return ApiResponse<bool>.SuccessResult(true, "Account disconnected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting bank account {AccountId}", bankAccountId);
                return ApiResponse<bool>.ErrorResult($"Failed to disconnect account: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankFeedTransactionDto>>> FetchTransactionsAsync(string bankAccountId, DateTime? since = null)
        {
            try
            {
                var account = await _context.BankAccounts
                    .FirstOrDefaultAsync(a => a.Id == bankAccountId);

                if (account == null)
                {
                    return ApiResponse<List<BankFeedTransactionDto>>.ErrorResult("Bank account not found");
                }

                if (!account.IsConnected)
                {
                    return ApiResponse<List<BankFeedTransactionDto>>.ErrorResult("Account is not connected to bank feed");
                }

                if (string.IsNullOrEmpty(account.ConnectionId) || account.ConnectionId.StartsWith("MOCK_"))
                {
                    // Mock account - return empty list
                    _logger.LogInformation("Fetching transactions for mock account {AccountId}", bankAccountId);
                    return ApiResponse<List<BankFeedTransactionDto>>.SuccessResult(new List<BankFeedTransactionDto>(), "No transactions available for mock account");
                }

                // Fetch transactions from Plaid
                var startDate = since ?? DateTime.UtcNow.AddDays(-30); // Default to last 30 days
                var endDate = DateTime.UtcNow;

                _logger.LogInformation("Fetching Plaid transactions for account {AccountId} from {StartDate} to {EndDate}", 
                    bankAccountId, startDate, endDate);

                var plaidResponse = await _plaidService.GetTransactionsAsync(
                    account.ConnectionId, 
                    startDate, 
                    endDate);

                // Map Plaid transactions to our DTO
                var transactions = new List<BankFeedTransactionDto>();
                
                foreach (var plaidTransaction in plaidResponse.Transactions)
                {
                    var transaction = new BankFeedTransactionDto
                    {
                        ExternalTransactionId = plaidTransaction.TransactionId,
                        TransactionDate = plaidTransaction.Date.ToDateTime(TimeOnly.MinValue),
                        Amount = Math.Abs((decimal)(plaidTransaction.Amount ?? 0)),
                        Type = plaidTransaction.Amount >= 0 ? "DEBIT" : "CREDIT", // Plaid uses positive for debits
                        Description = plaidTransaction.Name ?? plaidTransaction.MerchantName ?? "Unknown",
                        MerchantName = plaidTransaction.MerchantName,
                        Category = plaidTransaction.Category != null && plaidTransaction.Category.Any() 
                            ? string.Join(", ", plaidTransaction.Category) 
                            : null,
                        ReferenceNumber = plaidTransaction.ReferenceNumber,
                        Location = plaidTransaction.Location != null 
                            ? $"{plaidTransaction.Location.City}, {plaidTransaction.Location.Region}" 
                            : null,
                        Metadata = new Dictionary<string, string>
                        {
                            { "account_id", plaidTransaction.AccountId ?? "" },
                            { "pending", plaidTransaction.Pending.ToString() },
                            { "payment_channel", plaidTransaction.PaymentChannel ?? "" }
                        }
                    };

                    transactions.Add(transaction);
                }

                // Update last sync time
                account.LastSyncedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Fetched {Count} transactions from Plaid for account {AccountId}", 
                    transactions.Count, bankAccountId);

                return ApiResponse<List<BankFeedTransactionDto>>.SuccessResult(transactions, 
                    $"Successfully fetched {transactions.Count} transactions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for account {AccountId}", bankAccountId);
                return ApiResponse<List<BankFeedTransactionDto>>.ErrorResult($"Failed to fetch transactions: {ex.Message}");
            }
        }

        public async Task<bool> IsAccountConnectedAsync(string bankAccountId)
        {
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == bankAccountId);

            return account?.IsConnected ?? false;
        }
    }
}

