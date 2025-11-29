using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for automated alerts and notifications based on transaction patterns
    /// </summary>
    public interface IAutomatedAlertsService
    {
        /// <summary>
        /// Check and generate alerts for a user
        /// </summary>
        Task<List<AlertDto>> CheckAndGenerateAlertsAsync(string userId);

        /// <summary>
        /// Create a custom alert rule
        /// </summary>
        Task<ApiResponse<AlertRuleDto>> CreateAlertRuleAsync(CreateAlertRuleDto rule, string userId);

        /// <summary>
        /// Get all alert rules for a user
        /// </summary>
        Task<ApiResponse<List<AlertRuleDto>>> GetAlertRulesAsync(string userId);

        /// <summary>
        /// Delete an alert rule
        /// </summary>
        Task<ApiResponse<bool>> DeleteAlertRuleAsync(string ruleId, string userId);
    }

    /// <summary>
    /// Alert DTO
    /// </summary>
    public class AlertDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // SPENDING_SPIKE, LOW_BALANCE, UNUSUAL_ACTIVITY, etc.
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "INFO"; // INFO, WARNING, CRITICAL
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Alert rule DTO
    /// </summary>
    public class AlertRuleDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Conditions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Create alert rule DTO
    /// </summary>
    public class CreateAlertRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Conditions { get; set; } = new();
    }
}

