using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // ==========================================
    // AUDIT LOG DTOs
    // ==========================================

    public class AuditLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? EntityName { get; set; }
        public string LogType { get; set; } = string.Empty;
        public string? Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object>? OldValues { get; set; }
        public Dictionary<string, object>? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestId { get; set; }
        public string? ComplianceType { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAuditLogDto
    {
        [Required]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string EntityType { get; set; } = string.Empty;

        public string? EntityId { get; set; }
        public string? EntityName { get; set; }

        [Required]
        public string LogType { get; set; } = "USER_ACTIVITY"; // USER_ACTIVITY, SYSTEM_EVENT, SECURITY_EVENT, COMPLIANCE_EVENT

        public string? Severity { get; set; } = "INFO"; // INFO, WARNING, ERROR, CRITICAL

        [Required]
        public string Description { get; set; } = string.Empty;

        public Dictionary<string, object>? OldValues { get; set; }
        public Dictionary<string, object>? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestId { get; set; }
        public string? ComplianceType { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AuditLogQueryDto
    {
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? LogType { get; set; }
        public string? Severity { get; set; }
        public string? ComplianceType { get; set; }
        public string? Category { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; } // Search in description, entity name, etc.
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "DESC"; // ASC or DESC
    }

    public class AuditLogSummaryDto
    {
        public int TotalLogs { get; set; }
        public int UserActivityLogs { get; set; }
        public int SystemEventLogs { get; set; }
        public int SecurityEventLogs { get; set; }
        public int ComplianceEventLogs { get; set; }
        public Dictionary<string, int> LogsByAction { get; set; } = new();
        public Dictionary<string, int> LogsByEntityType { get; set; } = new();
        public Dictionary<string, int> LogsBySeverity { get; set; } = new();
        public Dictionary<string, int> LogsByComplianceType { get; set; } = new();
    }

    public class PaginatedAuditLogsDto
    {
        public List<AuditLogDto> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}

