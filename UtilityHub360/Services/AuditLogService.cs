using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ==========================================
        // LOGGING METHODS
        // ==========================================

        public async Task LogActivityAsync(string? userId, CreateAuditLogDto logDto)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
                var requestMethod = httpContext?.Request?.Method;
                var requestPath = httpContext?.Request?.Path.Value;

                User? user = null;
                if (!string.IsNullOrEmpty(userId) && userId != "SYSTEM")
                {
                    user = await _context.Users.FindAsync(userId);
                }

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId ?? "SYSTEM",
                    UserEmail = user?.Email,
                    Action = logDto.Action,
                    EntityType = logDto.EntityType,
                    EntityId = logDto.EntityId,
                    EntityName = logDto.EntityName,
                    LogType = logDto.LogType,
                    Severity = logDto.Severity ?? "INFO",
                    Description = logDto.Description,
                    OldValues = logDto.OldValues != null ? JsonSerializer.Serialize(logDto.OldValues) : null,
                    NewValues = logDto.NewValues != null ? JsonSerializer.Serialize(logDto.NewValues) : null,
                    IpAddress = logDto.IpAddress ?? ipAddress,
                    UserAgent = logDto.UserAgent ?? userAgent,
                    RequestMethod = logDto.RequestMethod ?? requestMethod,
                    RequestPath = logDto.RequestPath ?? requestPath,
                    RequestId = logDto.RequestId ?? httpContext?.TraceIdentifier,
                    ComplianceType = logDto.ComplianceType,
                    Category = logDto.Category,
                    SubCategory = logDto.SubCategory,
                    Metadata = logDto.Metadata != null ? JsonSerializer.Serialize(logDto.Metadata) : null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log to console/application logs but don't throw - audit logging should not break main functionality
                Console.WriteLine($"Error logging audit event: {ex.Message}");
            }
        }

        public async Task LogUserActivityAsync(string userId, string action, string entityType, string? entityId, string description, Dictionary<string, object>? oldValues = null, Dictionary<string, object>? newValues = null)
        {
            var logDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                LogType = "USER_ACTIVITY",
                OldValues = oldValues,
                NewValues = newValues
            };

            await LogActivityAsync(userId, logDto);
        }

        public async Task LogSystemEventAsync(string action, string entityType, string description, string severity = "INFO")
        {
            var logDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = entityType,
                Description = description,
                LogType = "SYSTEM_EVENT",
                Severity = severity
            };

            // Use system user ID
            await LogActivityAsync("SYSTEM", logDto);
        }

        public async Task LogSecurityEventAsync(string userId, string action, string description, string severity = "WARNING")
        {
            var logDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "SECURITY",
                Description = description,
                LogType = "SECURITY_EVENT",
                Severity = severity,
                Category = "SECURITY"
            };

            await LogActivityAsync(userId, logDto);
        }

        public async Task LogComplianceEventAsync(string userId, string complianceType, string action, string entityType, string? entityId, string description)
        {
            var logDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                LogType = "COMPLIANCE_EVENT",
                ComplianceType = complianceType,
                Category = "COMPLIANCE",
                Severity = "INFO"
            };

            await LogActivityAsync(userId, logDto);
        }

        // ==========================================
        // QUERY METHODS
        // ==========================================

        public async Task<ApiResponse<PaginatedAuditLogsDto>> GetAuditLogsAsync(string userId, AuditLogQueryDto query)
        {
            try
            {
                var currentUser = await _context.Users.FindAsync(userId);
                var isAdmin = currentUser?.Role == "ADMIN";

                var queryable = _context.AuditLogs.AsQueryable();

                // Non-admins can only see their own logs
                if (!isAdmin)
                {
                    queryable = queryable.Where(l => l.UserId == userId);
                }

                // Apply filters
                if (!string.IsNullOrEmpty(query.UserId))
                {
                    queryable = queryable.Where(l => l.UserId == query.UserId);
                }

                if (!string.IsNullOrEmpty(query.Action))
                {
                    queryable = queryable.Where(l => l.Action == query.Action);
                }

                if (!string.IsNullOrEmpty(query.EntityType))
                {
                    queryable = queryable.Where(l => l.EntityType == query.EntityType);
                }

                if (!string.IsNullOrEmpty(query.EntityId))
                {
                    queryable = queryable.Where(l => l.EntityId == query.EntityId);
                }

                if (!string.IsNullOrEmpty(query.LogType))
                {
                    queryable = queryable.Where(l => l.LogType == query.LogType);
                }

                if (!string.IsNullOrEmpty(query.Severity))
                {
                    queryable = queryable.Where(l => l.Severity == query.Severity);
                }

                if (!string.IsNullOrEmpty(query.ComplianceType))
                {
                    queryable = queryable.Where(l => l.ComplianceType == query.ComplianceType);
                }

                if (!string.IsNullOrEmpty(query.Category))
                {
                    queryable = queryable.Where(l => l.Category == query.Category);
                }

                if (query.StartDate.HasValue)
                {
                    queryable = queryable.Where(l => l.CreatedAt >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    queryable = queryable.Where(l => l.CreatedAt <= query.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var searchTerm = query.SearchTerm.ToLower();
                    queryable = queryable.Where(l =>
                        l.Description.ToLower().Contains(searchTerm) ||
                        (l.EntityName != null && l.EntityName.ToLower().Contains(searchTerm)) ||
                        (l.UserEmail != null && l.UserEmail.ToLower().Contains(searchTerm))
                    );
                }

                // Get total count before pagination
                var totalCount = await queryable.CountAsync();

                // Apply sorting
                queryable = query.SortBy?.ToLower() switch
                {
                    "action" => query.SortOrder == "ASC" ? queryable.OrderBy(l => l.Action) : queryable.OrderByDescending(l => l.Action),
                    "entitytype" => query.SortOrder == "ASC" ? queryable.OrderBy(l => l.EntityType) : queryable.OrderByDescending(l => l.EntityType),
                    "severity" => query.SortOrder == "ASC" ? queryable.OrderBy(l => l.Severity) : queryable.OrderByDescending(l => l.Severity),
                    _ => query.SortOrder == "ASC" ? queryable.OrderBy(l => l.CreatedAt) : queryable.OrderByDescending(l => l.CreatedAt)
                };

                // Apply pagination
                var logs = await queryable
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var logDtos = logs.Select(MapToDto).ToList();

                var result = new PaginatedAuditLogsDto
                {
                    Logs = logDtos,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return ApiResponse<PaginatedAuditLogsDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedAuditLogsDto>.ErrorResult($"Error retrieving audit logs: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(string logId, string userId)
        {
            try
            {
                var currentUser = await _context.Users.FindAsync(userId);
                var isAdmin = currentUser?.Role == "ADMIN";

                var log = await _context.AuditLogs.FindAsync(logId);

                if (log == null)
                {
                    return ApiResponse<AuditLogDto>.ErrorResult("Audit log not found");
                }

                // Non-admins can only view their own logs
                if (!isAdmin && log.UserId != userId)
                {
                    return ApiResponse<AuditLogDto>.ErrorResult("Access denied");
                }

                var dto = MapToDto(log);
                return ApiResponse<AuditLogDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuditLogDto>.ErrorResult($"Error retrieving audit log: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuditLogSummaryDto>> GetAuditLogSummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var currentUser = await _context.Users.FindAsync(userId);
                var isAdmin = currentUser?.Role == "ADMIN";

                var queryable = _context.AuditLogs.AsQueryable();

                if (!isAdmin)
                {
                    queryable = queryable.Where(l => l.UserId == userId);
                }

                if (startDate.HasValue)
                {
                    queryable = queryable.Where(l => l.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    queryable = queryable.Where(l => l.CreatedAt <= endDate.Value);
                }

                var logs = await queryable.ToListAsync();

                var summary = new AuditLogSummaryDto
                {
                    TotalLogs = logs.Count,
                    UserActivityLogs = logs.Count(l => l.LogType == "USER_ACTIVITY"),
                    SystemEventLogs = logs.Count(l => l.LogType == "SYSTEM_EVENT"),
                    SecurityEventLogs = logs.Count(l => l.LogType == "SECURITY_EVENT"),
                    ComplianceEventLogs = logs.Count(l => l.LogType == "COMPLIANCE_EVENT"),
                    LogsByAction = logs.GroupBy(l => l.Action).ToDictionary(g => g.Key, g => g.Count()),
                    LogsByEntityType = logs.GroupBy(l => l.EntityType).ToDictionary(g => g.Key, g => g.Count()),
                    LogsBySeverity = logs.GroupBy(l => l.Severity ?? "INFO").ToDictionary(g => g.Key, g => g.Count()),
                    LogsByComplianceType = logs.Where(l => !string.IsNullOrEmpty(l.ComplianceType))
                        .GroupBy(l => l.ComplianceType!)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return ApiResponse<AuditLogSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuditLogSummaryDto>.ErrorResult($"Error retrieving audit log summary: {ex.Message}");
            }
        }

        // ==========================================
        // EXPORT METHODS
        // ==========================================

        public async Task<byte[]> ExportAuditLogsToCsvAsync(string userId, AuditLogQueryDto query)
        {
            try
            {
                var result = await GetAuditLogsAsync(userId, new AuditLogQueryDto
                {
                    UserId = query.UserId,
                    Action = query.Action,
                    EntityType = query.EntityType,
                    EntityId = query.EntityId,
                    LogType = query.LogType,
                    Severity = query.Severity,
                    ComplianceType = query.ComplianceType,
                    Category = query.Category,
                    StartDate = query.StartDate,
                    EndDate = query.EndDate,
                    SearchTerm = query.SearchTerm,
                    Page = 1,
                    PageSize = 10000, // Export all matching logs
                    SortBy = query.SortBy,
                    SortOrder = query.SortOrder
                });

                if (!result.Success || result.Data == null)
                {
                    throw new Exception("Failed to retrieve audit logs for export");
                }

                var csv = new StringBuilder();
                csv.AppendLine("Audit Log Export");
                csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                csv.AppendLine();

                // Header
                csv.AppendLine("Id,UserId,UserEmail,Action,EntityType,EntityId,EntityName,LogType,Severity,Description,IpAddress,RequestMethod,RequestPath,ComplianceType,Category,CreatedAt");

                // Data rows
                foreach (var log in result.Data.Logs)
                {
                    csv.AppendLine($"{log.Id},{log.UserId},{log.UserEmail ?? ""},{log.Action},{log.EntityType},{log.EntityId ?? ""},{log.EntityName ?? ""},{log.LogType},{log.Severity ?? ""},\"{log.Description.Replace("\"", "\"\"")}\",{log.IpAddress ?? ""},{log.RequestMethod ?? ""},{log.RequestPath ?? ""},{log.ComplianceType ?? ""},{log.Category ?? ""},{log.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting audit logs to CSV: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> ExportAuditLogsToPdfAsync(string userId, AuditLogQueryDto query)
        {
            try
            {
                var result = await GetAuditLogsAsync(userId, new AuditLogQueryDto
                {
                    UserId = query.UserId,
                    Action = query.Action,
                    EntityType = query.EntityType,
                    EntityId = query.EntityId,
                    LogType = query.LogType,
                    Severity = query.Severity,
                    ComplianceType = query.ComplianceType,
                    Category = query.Category,
                    StartDate = query.StartDate,
                    EndDate = query.EndDate,
                    SearchTerm = query.SearchTerm,
                    Page = 1,
                    PageSize = 10000,
                    SortBy = query.SortBy,
                    SortOrder = query.SortOrder
                });

                if (!result.Success || result.Data == null)
                {
                    throw new Exception("Failed to retrieve audit logs for export");
                }

                // Simple text-based PDF (for production, use QuestPDF or iTextSharp)
                var pdfContent = new StringBuilder();
                pdfContent.AppendLine("AUDIT LOG REPORT");
                pdfContent.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                pdfContent.AppendLine($"Total Logs: {result.Data.TotalCount}");
                pdfContent.AppendLine();

                foreach (var log in result.Data.Logs)
                {
                    pdfContent.AppendLine($"Date: {log.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    pdfContent.AppendLine($"User: {log.UserEmail ?? log.UserId}");
                    pdfContent.AppendLine($"Action: {log.Action}");
                    pdfContent.AppendLine($"Entity: {log.EntityType} ({log.EntityId ?? "N/A"})");
                    pdfContent.AppendLine($"Type: {log.LogType} | Severity: {log.Severity}");
                    pdfContent.AppendLine($"Description: {log.Description}");
                    if (!string.IsNullOrEmpty(log.ComplianceType))
                    {
                        pdfContent.AppendLine($"Compliance: {log.ComplianceType}");
                    }
                    pdfContent.AppendLine("---");
                }

                return Encoding.UTF8.GetBytes(pdfContent.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting audit logs to PDF: {ex.Message}", ex);
            }
        }

        // ==========================================
        // HELPER METHODS
        // ==========================================

        private AuditLogDto MapToDto(AuditLog log)
        {
            return new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserEmail = log.UserEmail,
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                EntityName = log.EntityName,
                LogType = log.LogType,
                Severity = log.Severity,
                Description = log.Description,
                OldValues = !string.IsNullOrEmpty(log.OldValues) ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.OldValues) : null,
                NewValues = !string.IsNullOrEmpty(log.NewValues) ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.NewValues) : null,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                RequestMethod = log.RequestMethod,
                RequestPath = log.RequestPath,
                RequestId = log.RequestId,
                ComplianceType = log.ComplianceType,
                Category = log.Category,
                SubCategory = log.SubCategory,
                Metadata = !string.IsNullOrEmpty(log.Metadata) ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.Metadata) : null,
                CreatedAt = log.CreatedAt
            };
        }
    }
}

