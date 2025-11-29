using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for smart transaction categorization using pattern matching
    /// </summary>
    public class SmartCategorizationService : ISmartCategorizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SmartCategorizationService> _logger;

        // Common merchant patterns for categorization
        private readonly Dictionary<string, (string Category, string Type)> _merchantPatterns = new()
        {
            // Groceries
            { "walmart", ("GROCERIES", "EXPENSE") },
            { "target", ("GROCERIES", "EXPENSE") },
            { "kroger", ("GROCERIES", "EXPENSE") },
            { "safeway", ("GROCERIES", "EXPENSE") },
            { "whole foods", ("GROCERIES", "EXPENSE") },
            { "trader joe", ("GROCERIES", "EXPENSE") },
            
            // Restaurants
            { "mcdonald", ("DINING", "EXPENSE") },
            { "starbucks", ("DINING", "EXPENSE") },
            { "restaurant", ("DINING", "EXPENSE") },
            { "pizza", ("DINING", "EXPENSE") },
            { "cafe", ("DINING", "EXPENSE") },
            
            // Gas
            { "shell", ("GAS", "EXPENSE") },
            { "exxon", ("GAS", "EXPENSE") },
            { "chevron", ("GAS", "EXPENSE") },
            { "bp", ("GAS", "EXPENSE") },
            { "gas station", ("GAS", "EXPENSE") },
            
            // Utilities
            { "electric", ("UTILITIES", "BILL") },
            { "water", ("UTILITIES", "BILL") },
            { "gas company", ("UTILITIES", "BILL") },
            { "internet", ("UTILITIES", "BILL") },
            { "phone", ("UTILITIES", "BILL") },
            
            // Transportation
            { "uber", ("TRANSPORTATION", "EXPENSE") },
            { "lyft", ("TRANSPORTATION", "EXPENSE") },
            { "taxi", ("TRANSPORTATION", "EXPENSE") },
            { "metro", ("TRANSPORTATION", "EXPENSE") },
            
            // Entertainment
            { "netflix", ("ENTERTAINMENT", "EXPENSE") },
            { "spotify", ("ENTERTAINMENT", "EXPENSE") },
            { "amazon prime", ("ENTERTAINMENT", "EXPENSE") },
            { "movie", ("ENTERTAINMENT", "EXPENSE") },
        };

        public SmartCategorizationService(ApplicationDbContext context, ILogger<SmartCategorizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CategorySuggestion> SuggestCategoryAsync(CreateTransactionRequest transaction, string userId)
        {
            var suggestions = await GetCategorySuggestionsAsync(transaction, userId, 1);
            return suggestions.FirstOrDefault() ?? new CategorySuggestion
            {
                CategoryName = "UNCATEGORIZED",
                CategoryType = "EXPENSE",
                Confidence = 0.0,
                Reason = "No pattern match found"
            };
        }

        public async Task<List<CategorySuggestion>> GetCategorySuggestionsAsync(CreateTransactionRequest transaction, string userId, int topN = 3)
        {
            var suggestions = new List<CategorySuggestion>();

            try
            {
                // 1. Check merchant patterns
                if (!string.IsNullOrEmpty(transaction.MerchantName))
                {
                    var merchantLower = transaction.MerchantName.ToLower();
                    foreach (var pattern in _merchantPatterns)
                    {
                        if (merchantLower.Contains(pattern.Key))
                        {
                            suggestions.Add(new CategorySuggestion
                            {
                                CategoryName = pattern.Value.Category,
                                CategoryType = pattern.Value.Type,
                                Confidence = 0.85,
                                Reason = $"Matched merchant pattern: {pattern.Key}"
                            });
                            break;
                        }
                    }
                }

                // 2. Check description patterns
                if (!string.IsNullOrEmpty(transaction.Description))
                {
                    var descriptionLower = transaction.Description.ToLower();
                    foreach (var pattern in _merchantPatterns)
                    {
                        if (descriptionLower.Contains(pattern.Key))
                        {
                            suggestions.Add(new CategorySuggestion
                            {
                                CategoryName = pattern.Value.Category,
                                CategoryType = pattern.Value.Type,
                                Confidence = 0.75,
                                Reason = $"Matched description pattern: {pattern.Key}"
                            });
                            break;
                        }
                    }
                }

                // 3. Check user's historical categorization patterns
                var historicalSuggestions = await GetHistoricalCategorySuggestionsAsync(transaction, userId);
                suggestions.AddRange(historicalSuggestions);

                // 4. Check active categories for the user
                var userCategories = await _context.TransactionCategories
                    .Where(c => c.UserId == userId && c.IsActive && !c.IsDeleted)
                    .ToListAsync();

                // If transaction type is DEBIT, suggest expense categories
                if (transaction.TransactionType == "DEBIT")
                {
                    var expenseCategories = userCategories
                        .Where(c => c.Type == "EXPENSE" || c.Type == "BILL")
                        .Select(c => new CategorySuggestion
                        {
                            CategoryName = c.Name,
                            CategoryType = c.Type,
                            Confidence = 0.5,
                            Reason = "Common expense category"
                        })
                        .Take(2);

                    suggestions.AddRange(expenseCategories);
                }

                // Remove duplicates and sort by confidence
                suggestions = suggestions
                    .GroupBy(s => s.CategoryName)
                    .Select(g => g.OrderByDescending(s => s.Confidence).First())
                    .OrderByDescending(s => s.Confidence)
                    .Take(topN)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category suggestions");
            }

            return suggestions;
        }

        public async Task LearnFromUserChoiceAsync(string transactionId, string selectedCategory, string userId)
        {
            try
            {
                // This could be used to improve future suggestions
                // For now, we'll just log it
                _logger.LogInformation("User selected category {Category} for transaction {TransactionId}", selectedCategory, transactionId);
                
                // TODO: Store this in a learning table to improve future suggestions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error learning from user choice");
            }
        }

        private async Task<List<CategorySuggestion>> GetHistoricalCategorySuggestionsAsync(CreateTransactionRequest transaction, string userId)
        {
            var suggestions = new List<CategorySuggestion>();

            try
            {
                // Find similar past transactions and see what categories were used
                var similarTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId
                        && !string.IsNullOrEmpty(t.Category)
                        && !t.IsDeleted
                        && (t.Description.Contains(transaction.Description ?? "", StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrEmpty(transaction.MerchantName) && t.Merchant != null && t.Merchant.Contains(transaction.MerchantName, StringComparison.OrdinalIgnoreCase))))
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .ToListAsync();

                if (similarTransactions.Any())
                {
                    var categoryGroups = similarTransactions
                        .GroupBy(t => t.Category)
                        .Select(g => new
                        {
                            Category = g.Key!,
                            Count = g.Count(),
                            AvgAmount = g.Average(t => (double)t.Amount)
                        })
                        .OrderByDescending(g => g.Count)
                        .Take(3);

                    foreach (var group in categoryGroups)
                    {
                        var category = await _context.TransactionCategories
                            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == group.Category);

                        if (category != null)
                        {
                            suggestions.Add(new CategorySuggestion
                            {
                                CategoryName = group.Category,
                                CategoryType = category.Type,
                                Confidence = Math.Min(0.9, 0.6 + (group.Count * 0.1)),
                                Reason = $"Used {group.Count} time(s) for similar transactions"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historical category suggestions");
            }

            return suggestions;
        }
    }
}

