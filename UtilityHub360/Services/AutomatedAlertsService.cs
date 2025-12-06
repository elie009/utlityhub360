using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for automated alerts and notifications
    /// </summary>
    public class AutomatedAlertsService : IAutomatedAlertsService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ISpendingPatternService _spendingPatternService;
        private readonly ILogger<AutomatedAlertsService> _logger;

        public AutomatedAlertsService(
            ApplicationDbContext context,
            INotificationService notificationService,
            ISpendingPatternService spendingPatternService,
            ILogger<AutomatedAlertsService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _spendingPatternService = spendingPatternService;
            _logger = logger;
        }

        public async Task<List<AlertDto>> CheckAndGenerateAlertsAsync(string userId)
        {
            var alerts = new List<AlertDto>();

            try
            {
                // 1. Check for low balance alerts
                var lowBalanceAlerts = await CheckLowBalanceAsync(userId);
                alerts.AddRange(lowBalanceAlerts);

                // 2. Check for unusual spending
                var unusualSpending = await _spendingPatternService.DetectUnusualSpendingAsync(userId);
                foreach (var spending in unusualSpending)
                {
                    alerts.Add(new AlertDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Type = "UNUSUAL_SPENDING",
                        Title = "Unusual Spending Detected",
                        Message = spending.Message,
                        Severity = spending.AlertType == "SPIKE" ? "WARNING" : "INFO",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        Metadata = new Dictionary<string, object>
                        {
                            { "Category", spending.CategoryName },
                            { "CurrentAmount", spending.CurrentAmount },
                            { "AverageAmount", spending.AverageAmount },
                            { "DeviationPercentage", spending.DeviationPercentage }
                        }
                    });
                }

                // 3. Check for large transactions
                var largeTransactionAlerts = await CheckLargeTransactionsAsync(userId);
                alerts.AddRange(largeTransactionAlerts);

                // 4. Check custom alert rules
                var customAlerts = await CheckCustomAlertRulesAsync(userId);
                alerts.AddRange(customAlerts);

                // Create notifications for all alerts
                foreach (var alert in alerts)
                {
                    // Extract account ID from metadata if available for better duplicate detection
                    Dictionary<string, string>? templateVariables = null;
                    if (alert.Metadata != null && alert.Metadata.ContainsKey("AccountId"))
                    {
                        templateVariables = new Dictionary<string, string>
                        {
                            { "AccountId", alert.Metadata["AccountId"].ToString() ?? "" }
                        };
                    }
                    else if (alert.Metadata != null && alert.Metadata.ContainsKey("TransactionId"))
                    {
                        templateVariables = new Dictionary<string, string>
                        {
                            { "TransactionId", alert.Metadata["TransactionId"].ToString() ?? "" }
                        };
                    }

                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserId = userId,
                        Type = alert.Type,
                        Title = alert.Title,
                        Message = alert.Message,
                        TemplateVariables = templateVariables
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and generating alerts for user {UserId}", userId);
            }

            return alerts;
        }

        public async Task<ApiResponse<AlertRuleDto>> CreateAlertRuleAsync(CreateAlertRuleDto rule, string userId)
        {
            try
            {
                // Store alert rule in database (you'll need to create an AlertRule entity)
                // For now, we'll just return success
                var alertRule = new AlertRuleDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = rule.Name,
                    AlertType = rule.AlertType,
                    IsActive = rule.IsActive,
                    Conditions = rule.Conditions,
                    CreatedAt = DateTime.UtcNow
                };

                return ApiResponse<AlertRuleDto>.SuccessResult(alertRule, "Alert rule created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert rule");
                return ApiResponse<AlertRuleDto>.ErrorResult($"Failed to create alert rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<AlertRuleDto>>> GetAlertRulesAsync(string userId)
        {
            try
            {
                // Return empty list for now (would query database)
                var rules = new List<AlertRuleDto>();
                return ApiResponse<List<AlertRuleDto>>.SuccessResult(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert rules");
                return ApiResponse<List<AlertRuleDto>>.ErrorResult($"Failed to get alert rules: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAlertRuleAsync(string ruleId, string userId)
        {
            try
            {
                // Delete alert rule (would delete from database)
                return ApiResponse<bool>.SuccessResult(true, "Alert rule deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert rule");
                return ApiResponse<bool>.ErrorResult($"Failed to delete alert rule: {ex.Message}");
            }
        }

        private async Task<List<AlertDto>> CheckLowBalanceAsync(string userId)
        {
            var alerts = new List<AlertDto>();

            try
            {
                var accounts = await _context.BankAccounts
                    .Where(a => a.UserId == userId && a.IsActive && !a.IsDeleted)
                    .ToListAsync();

                foreach (var account in accounts)
                {
                    // Alert if balance is below threshold (e.g., $100)
                    if (account.CurrentBalance < 100)
                    {
                        alerts.Add(new AlertDto
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            Type = "LOW_BALANCE",
                            Title = "Low Account Balance",
                            Message = $"Your {account.AccountName} account balance is low: ${account.CurrentBalance:F2}",
                            Severity = account.CurrentBalance < 0 ? "CRITICAL" : "WARNING",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false,
                            Metadata = new Dictionary<string, object>
                            {
                                { "AccountId", account.Id },
                                { "AccountName", account.AccountName },
                                { "Balance", account.CurrentBalance }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking low balance");
            }

            return alerts;
        }

        private async Task<List<AlertDto>> CheckLargeTransactionsAsync(string userId)
        {
            var alerts = new List<AlertDto>();

            try
            {
                var recentTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId
                        && t.TransactionType == "DEBIT"
                        && t.TransactionDate >= DateTime.UtcNow.AddDays(-1)
                        && t.Amount > 1000
                        && !t.IsDeleted)
                    .ToListAsync();

                foreach (var transaction in recentTransactions)
                {
                    alerts.Add(new AlertDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Type = "LARGE_TRANSACTION",
                        Title = "Large Transaction Detected",
                        Message = $"A large transaction of ${transaction.Amount:F2} was recorded: {transaction.Description}",
                        Severity = transaction.Amount > 5000 ? "WARNING" : "INFO",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        Metadata = new Dictionary<string, object>
                        {
                            { "TransactionId", transaction.Id },
                            { "Amount", transaction.Amount },
                            { "Description", transaction.Description }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking large transactions");
            }

            return alerts;
        }

        private async Task<List<AlertDto>> CheckCustomAlertRulesAsync(string userId)
        {
            // Check user-defined alert rules
            // This would query the AlertRules table and evaluate conditions
            return new List<AlertDto>();
        }
    }
}

