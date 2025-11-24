using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for detecting duplicate transactions
    /// </summary>
    public interface IDuplicateDetectionService
    {
        /// <summary>
        /// Check if a transaction is a duplicate
        /// </summary>
        Task<DuplicateCheckResult> CheckDuplicateAsync(CreateTransactionRequest transaction, string userId);

        /// <summary>
        /// Get potential duplicates for a transaction
        /// </summary>
        Task<List<PotentialDuplicateDto>> FindPotentialDuplicatesAsync(CreateTransactionRequest transaction, string userId);

        /// <summary>
        /// Mark a transaction as confirmed (not a duplicate)
        /// </summary>
        Task<ApiResponse<bool>> ConfirmNotDuplicateAsync(string transactionId, string duplicateId);
    }

    /// <summary>
    /// Result of duplicate check
    /// </summary>
    public class DuplicateCheckResult
    {
        public bool IsDuplicate { get; set; }
        public double Confidence { get; set; } // 0.0 to 1.0
        public string? DuplicateTransactionId { get; set; }
        public string? Reason { get; set; }
        public List<PotentialDuplicateDto> PotentialDuplicates { get; set; } = new();
    }

    /// <summary>
    /// Potential duplicate transaction
    /// </summary>
    public class PotentialDuplicateDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? MerchantName { get; set; }
        public double SimilarityScore { get; set; } // 0.0 to 1.0
        public string? MatchReason { get; set; }
    }
}

