using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Services;
using UtilityHub360.Models;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("User not authenticated.");
        }

        /// <summary>
        /// Get audit logs with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedAuditLogsDto>>> GetAuditLogs([FromQuery] AuditLogQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _auditLogService.GetAuditLogsAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<PaginatedAuditLogsDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedAuditLogsDto>.ErrorResult($"Failed to get audit logs: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        [HttpGet("{logId}")]
        public async Task<ActionResult<ApiResponse<AuditLogDto>>> GetAuditLog(string logId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _auditLogService.GetAuditLogByIdAsync(logId, userId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuditLogDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuditLogDto>.ErrorResult($"Failed to get audit log: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get audit log summary/statistics
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<AuditLogSummaryDto>>> GetAuditLogSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _auditLogService.GetAuditLogSummaryAsync(userId, startDate, endDate);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuditLogSummaryDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuditLogSummaryDto>.ErrorResult($"Failed to get audit log summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create audit log entry (for manual logging)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AuditLogDto>>> CreateAuditLog([FromBody] CreateAuditLogDto logDto)
        {
            try
            {
                var userId = GetUserId();
                await _auditLogService.LogActivityAsync(userId, logDto);

                return Ok(ApiResponse<AuditLogDto>.SuccessResult(null, "Audit log created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuditLogDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuditLogDto>.ErrorResult($"Failed to create audit log: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export audit logs to CSV
        /// </summary>
        [HttpPost("export/csv")]
        public async Task<IActionResult> ExportAuditLogsToCsv([FromBody] AuditLogQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var csvBytes = await _auditLogService.ExportAuditLogsToCsvAsync(userId, query);

                return File(csvBytes, "text/csv", $"Audit_Logs_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to export audit logs: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export audit logs to PDF
        /// </summary>
        [HttpPost("export/pdf")]
        public async Task<IActionResult> ExportAuditLogsToPdf([FromBody] AuditLogQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var pdfBytes = await _auditLogService.ExportAuditLogsToPdfAsync(userId, query);

                return File(pdfBytes, "application/pdf", $"Audit_Logs_{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to export audit logs: {ex.Message}"));
            }
        }
    }
}

