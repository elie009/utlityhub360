using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface ISavingsService
    {
        // Savings Account Management
        Task<ApiResponse<SavingsAccountDto>> CreateSavingsAccountAsync(CreateSavingsAccountDto savingsAccountDto, string userId);
        Task<ApiResponse<List<SavingsAccountDto>>> GetUserSavingsAccountsAsync(string userId);
        Task<ApiResponse<SavingsAccountDto>> GetSavingsAccountByIdAsync(string savingsAccountId, string userId);
        Task<ApiResponse<SavingsAccountDto>> UpdateSavingsAccountAsync(string savingsAccountId, CreateSavingsAccountDto updateDto, string userId);
        Task<ApiResponse<bool>> DeleteSavingsAccountAsync(string savingsAccountId, string userId);

        // Savings Transactions
        Task<ApiResponse<SavingsTransactionDto>> CreateSavingsTransactionAsync(CreateSavingsTransactionDto transactionDto, string userId);
        Task<ApiResponse<List<SavingsTransactionDto>>> GetSavingsTransactionsAsync(string savingsAccountId, string userId, int page = 1, int limit = 50);
        Task<ApiResponse<SavingsTransactionDto>> GetSavingsTransactionByIdAsync(string transactionId, string userId);

        // Savings Analytics and Summary
        Task<ApiResponse<SavingsSummaryDto>> GetSavingsSummaryAsync(string userId);
        Task<ApiResponse<SavingsAnalyticsDto>> GetSavingsAnalyticsAsync(string userId, string period = "month");
        Task<ApiResponse<Dictionary<string, decimal>>> GetSavingsByTypeAsync(string userId);

        // Auto-Save Features
        Task<ApiResponse<bool>> CreateAutoSaveAsync(AutoSaveSettingsDto autoSaveDto, string userId);
        Task<ApiResponse<List<AutoSaveSettingsDto>>> GetAutoSaveSettingsAsync(string userId);
        Task<ApiResponse<bool>> UpdateAutoSaveAsync(string autoSaveId, AutoSaveSettingsDto updateDto, string userId);
        Task<ApiResponse<bool>> DeleteAutoSaveAsync(string autoSaveId, string userId);
        Task<ApiResponse<bool>> ExecuteAutoSaveAsync(string userId);

        // Savings Goals and Progress
        Task<ApiResponse<SavingsAccountDto>> UpdateSavingsGoalAsync(string savingsAccountId, decimal newTargetAmount, DateTime? newTargetDate, string userId);
        Task<ApiResponse<List<SavingsAccountDto>>> GetSavingsGoalsByTypeAsync(string savingsType, string userId);
        Task<ApiResponse<decimal>> GetTotalSavingsProgressAsync(string userId);

        // Bank Account Integration
        Task<ApiResponse<bool>> TransferFromBankToSavingsAsync(string bankAccountId, string savingsAccountId, decimal amount, string description, string userId);
        Task<ApiResponse<bool>> TransferFromSavingsToBankAsync(string savingsAccountId, string bankAccountId, decimal amount, string description, string userId);
    }
}
