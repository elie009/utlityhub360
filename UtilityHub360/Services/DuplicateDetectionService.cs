using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for detecting duplicate transactions
    /// </summary>
    public class DuplicateDetectionService : IDuplicateDetectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DuplicateDetectionService> _logger;

        public DuplicateDetectionService(ApplicationDbContext context, ILogger<DuplicateDetectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DuplicateCheckResult> CheckDuplicateAsync(CreateTransactionRequest transaction, string userId)
        {
            var result = new DuplicateCheckResult();

            try
            {
                // Find transactions in the same account within a time window
                var timeWindow = TimeSpan.FromDays(7); // Check transactions within 7 days
                var startDate = transaction.TransactionDate.AddDays(-timeWindow.TotalDays);
                var endDate = transaction.TransactionDate.AddDays(timeWindow.TotalDays);

                var existingTransactions = await _context.BankTransactions
                    .Where(t => t.BankAccountId == transaction.BankAccountId
                        && t.UserId == userId
                        && t.TransactionDate >= startDate
                        && t.TransactionDate <= endDate
                        && !t.IsDeleted)
                    .ToListAsync();

                var potentialDuplicates = new List<PotentialDuplicateDto>();

                foreach (var existing in existingTransactions)
                {
                    var similarity = CalculateSimilarity(transaction, existing);
                    
                    if (similarity.SimilarityScore > 0.7) // 70% similarity threshold
                    {
                        potentialDuplicates.Add(new PotentialDuplicateDto
                        {
                            TransactionId = existing.Id,
                            TransactionDate = existing.TransactionDate,
                            Amount = existing.Amount,
                            Description = existing.Description,
                            MerchantName = existing.Merchant,
                            SimilarityScore = similarity.SimilarityScore,
                            MatchReason = similarity.Reason
                        });
                    }
                }

                result.PotentialDuplicates = potentialDuplicates.OrderByDescending(d => d.SimilarityScore).ToList();

                // If we have a very high confidence match (>90%), consider it a duplicate
                if (potentialDuplicates.Any(d => d.SimilarityScore > 0.9))
                {
                    var bestMatch = potentialDuplicates.OrderByDescending(d => d.SimilarityScore).First();
                    result.IsDuplicate = true;
                    result.Confidence = bestMatch.SimilarityScore;
                    result.DuplicateTransactionId = bestMatch.TransactionId;
                    result.Reason = bestMatch.MatchReason;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicate transactions");
            }

            return result;
        }

        public async Task<List<PotentialDuplicateDto>> FindPotentialDuplicatesAsync(CreateTransactionRequest transaction, string userId)
        {
            var result = await CheckDuplicateAsync(transaction, userId);
            return result.PotentialDuplicates;
        }

        public async Task<ApiResponse<bool>> ConfirmNotDuplicateAsync(string transactionId, string duplicateId)
        {
            try
            {
                // This could be used to train the duplicate detection algorithm
                // For now, we'll just log it
                _logger.LogInformation("User confirmed transaction {TransactionId} is not a duplicate of {DuplicateId}", transactionId, duplicateId);
                
                return ApiResponse<bool>.SuccessResult(true, "Confirmed as not duplicate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming not duplicate");
                return ApiResponse<bool>.ErrorResult($"Failed to confirm: {ex.Message}");
            }
        }

        private (double SimilarityScore, string Reason) CalculateSimilarity(CreateTransactionRequest transaction, Entities.BankTransaction existing)
        {
            double score = 0.0;
            var reasons = new List<string>();

            // Amount match (exact match = 40%, within 1% = 30%, within 5% = 20%)
            if (Math.Abs(transaction.Amount - existing.Amount) < 0.01m)
            {
                score += 0.4;
                reasons.Add("Exact amount match");
            }
            else if ((double)Math.Abs(transaction.Amount - existing.Amount) / (double)Math.Max(Math.Abs(transaction.Amount), Math.Abs(existing.Amount)) < 0.01)
            {
                score += 0.3;
                reasons.Add("Amount within 1%");
            }
            else if ((double)Math.Abs(transaction.Amount - existing.Amount) / (double)Math.Max(Math.Abs(transaction.Amount), Math.Abs(existing.Amount)) < 0.05)
            {
                score += 0.2;
                reasons.Add("Amount within 5%");
            }

            // Date match (same day = 20%, within 1 day = 15%, within 3 days = 10%)
            var dateDiff = Math.Abs((transaction.TransactionDate - existing.TransactionDate).TotalDays);
            if (dateDiff == 0)
            {
                score += 0.2;
                reasons.Add("Same date");
            }
            else if (dateDiff <= 1)
            {
                score += 0.15;
                reasons.Add("Date within 1 day");
            }
            else if (dateDiff <= 3)
            {
                score += 0.1;
                reasons.Add("Date within 3 days");
            }

            // Description similarity (using Levenshtein distance)
            var descriptionSimilarity = CalculateStringSimilarity(
                transaction.Description?.ToLower() ?? "",
                existing.Description?.ToLower() ?? "");
            
            if (descriptionSimilarity > 0.9)
            {
                score += 0.3;
                reasons.Add("Very similar description");
            }
            else if (descriptionSimilarity > 0.7)
            {
                score += 0.2;
                reasons.Add("Similar description");
            }
            else if (descriptionSimilarity > 0.5)
            {
                score += 0.1;
                reasons.Add("Somewhat similar description");
            }

            // Merchant match (exact = 10%)
            if (!string.IsNullOrEmpty(transaction.MerchantName) && 
                !string.IsNullOrEmpty(existing.Merchant) &&
                transaction.MerchantName.Equals(existing.Merchant, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.1;
                reasons.Add("Same merchant");
            }

            return (Math.Min(score, 1.0), string.Join(", ", reasons));
        }

        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
                return 1.0;
            
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0.0;

            if (str1 == str2)
                return 1.0;

            // Use Levenshtein distance
            var distance = LevenshteinDistance(str1, str2);
            var maxLength = Math.Max(str1.Length, str2.Length);
            
            return 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return string.IsNullOrEmpty(t) ? 0 : t.Length;

            if (string.IsNullOrEmpty(t))
                return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}

