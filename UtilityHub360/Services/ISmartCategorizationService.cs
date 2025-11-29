using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for smart transaction categorization using ML patterns
    /// </summary>
    public interface ISmartCategorizationService
    {
        /// <summary>
        /// Suggest category for a transaction based on patterns
        /// </summary>
        Task<CategorySuggestion> SuggestCategoryAsync(CreateTransactionRequest transaction, string userId);

        /// <summary>
        /// Get multiple category suggestions with confidence scores
        /// </summary>
        Task<List<CategorySuggestion>> GetCategorySuggestionsAsync(CreateTransactionRequest transaction, string userId, int topN = 3);

        /// <summary>
        /// Learn from user's categorization choices to improve suggestions
        /// </summary>
        Task LearnFromUserChoiceAsync(string transactionId, string selectedCategory, string userId);
    }

    /// <summary>
    /// Category suggestion with confidence score
    /// </summary>
    public class CategorySuggestion
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public double Confidence { get; set; } // 0.0 to 1.0
        public string? Reason { get; set; }
    }
}

