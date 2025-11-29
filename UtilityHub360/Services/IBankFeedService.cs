using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Interface for bank feed integration services (Plaid, Yodlee, etc.)
    /// </summary>
    public interface IBankFeedService
    {
        /// <summary>
        /// Connect a bank account to a bank feed provider
        /// </summary>
        Task<ApiResponse<string>> ConnectAccountAsync(string userId, string bankAccountId, string provider, Dictionary<string, string> credentials);

        /// <summary>
        /// Disconnect a bank account from bank feed
        /// </summary>
        Task<ApiResponse<bool>> DisconnectAccountAsync(string bankAccountId);

        /// <summary>
        /// Fetch new transactions from bank feed
        /// </summary>
        Task<ApiResponse<List<BankFeedTransactionDto>>> FetchTransactionsAsync(string bankAccountId, DateTime? since = null);

        /// <summary>
        /// Check if account is connected to bank feed
        /// </summary>
        Task<bool> IsAccountConnectedAsync(string bankAccountId);
    }

    /// <summary>
    /// DTO for bank feed transactions
    /// </summary>
    public class BankFeedTransactionDto
    {
        public string ExternalTransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // DEBIT or CREDIT
        public string Description { get; set; } = string.Empty;
        public string? MerchantName { get; set; }
        public string? Category { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Location { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}

