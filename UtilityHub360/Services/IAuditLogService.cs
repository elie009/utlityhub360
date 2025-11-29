using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IAuditLogService
    {
        // Logging Methods
        Task LogActivityAsync(string? userId, CreateAuditLogDto logDto);
        Task LogUserActivityAsync(string userId, string action, string entityType, string? entityId, string description, Dictionary<string, object>? oldValues = null, Dictionary<string, object>? newValues = null);
        Task LogSystemEventAsync(string action, string entityType, string description, string severity = "INFO");
        Task LogSecurityEventAsync(string userId, string action, string description, string severity = "WARNING");
        Task LogComplianceEventAsync(string userId, string complianceType, string action, string entityType, string? entityId, string description);

        // Query Methods
        Task<ApiResponse<PaginatedAuditLogsDto>> GetAuditLogsAsync(string userId, AuditLogQueryDto query);
        Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(string logId, string userId);
        Task<ApiResponse<AuditLogSummaryDto>> GetAuditLogSummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        // Export Methods
        Task<byte[]> ExportAuditLogsToCsvAsync(string userId, AuditLogQueryDto query);
        Task<byte[]> ExportAuditLogsToPdfAsync(string userId, AuditLogQueryDto query);
    }
}

