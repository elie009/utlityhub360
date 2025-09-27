using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IBankAccountService
    {
        // Bank Account CRUD Operations
        Task<ApiResponse<BankAccountDto>> CreateBankAccountAsync(CreateBankAccountDto createBankAccountDto, string userId);
        Task<ApiResponse<BankAccountDto>> GetBankAccountAsync(string bankAccountId, string userId);
        Task<ApiResponse<BankAccountDto>> UpdateBankAccountAsync(string bankAccountId, UpdateBankAccountDto updateBankAccountDto, string userId);
        Task<ApiResponse<bool>> DeleteBankAccountAsync(string bankAccountId, string userId);
        Task<ApiResponse<List<BankAccountDto>>> GetUserBankAccountsAsync(string userId, bool includeInactive = false);

        // Bank Account Analytics & Summary
        Task<ApiResponse<BankAccountSummaryDto>> GetBankAccountSummaryAsync(string userId, string frequency = "monthly");
        Task<ApiResponse<BankAccountAnalyticsDto>> GetBankAccountAnalyticsAsync(string userId, string period = "month");
        Task<ApiResponse<decimal>> GetTotalBalanceAsync(string userId);
        Task<ApiResponse<List<BankAccountDto>>> GetTopAccountsByBalanceAsync(string userId, int limit = 5);

        // Bank Integration & Sync
        Task<ApiResponse<BankAccountDto>> ConnectBankAccountAsync(BankIntegrationDto integrationDto, string userId);
        Task<ApiResponse<BankAccountDto>> SyncBankAccountAsync(SyncBankAccountDto syncDto, string userId);
        Task<ApiResponse<List<BankAccountDto>>> GetConnectedAccountsAsync(string userId);
        Task<ApiResponse<bool>> DisconnectBankAccountAsync(string bankAccountId, string userId);

        // Bank Transactions
        Task<ApiResponse<BankTransactionDto>> CreateTransactionAsync(CreateBankTransactionDto createTransactionDto, string userId);
        Task<ApiResponse<List<BankTransactionDto>>> GetAccountTransactionsAsync(string bankAccountId, string userId, int page = 1, int limit = 50);
        Task<ApiResponse<List<BankTransactionDto>>> GetUserTransactionsAsync(string userId, string? accountType = null, int page = 1, int limit = 50);
        Task<ApiResponse<BankTransactionDto>> GetTransactionAsync(string transactionId, string userId);
        Task<ApiResponse<bool>> DeleteTransactionAsync(string transactionId, string userId);

        // Transaction Analytics
        Task<ApiResponse<BankAccountAnalyticsDto>> GetTransactionAnalyticsAsync(string userId, string period = "month");
        Task<ApiResponse<List<BankTransactionDto>>> GetRecentTransactionsAsync(string userId, int limit = 10);
        Task<ApiResponse<Dictionary<string, decimal>>> GetSpendingByCategoryAsync(string userId, string period = "month");

        // Account Management
        Task<ApiResponse<bool>> UpdateAccountBalanceAsync(string bankAccountId, decimal newBalance, string userId);
        Task<ApiResponse<BankAccountDto>> ArchiveBankAccountAsync(string bankAccountId, string userId);
        Task<ApiResponse<BankAccountDto>> ActivateBankAccountAsync(string bankAccountId, string userId);

        // Expense Management
        Task<ApiResponse<BankTransactionDto>> CreateExpenseAsync(CreateExpenseDto expenseDto, string userId);
        Task<ApiResponse<ExpenseAnalyticsDto>> GetExpenseAnalyticsAsync(string userId, string period = "month");
        Task<ApiResponse<ExpenseSummaryDto>> GetExpenseSummaryAsync(string userId);
        Task<ApiResponse<List<BankTransactionDto>>> GetExpensesByCategoryAsync(string userId, string category, int page = 1, int limit = 50);
        Task<ApiResponse<Dictionary<string, decimal>>> GetExpenseCategoriesAsync(string userId);

        // Admin Operations
        Task<ApiResponse<List<BankAccountDto>>> GetAllBankAccountsAsync(int page = 1, int limit = 50);
        Task<ApiResponse<List<BankTransactionDto>>> GetAllTransactionsAsync(int page = 1, int limit = 50);
    }
}
