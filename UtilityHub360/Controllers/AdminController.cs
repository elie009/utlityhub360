using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly INotificationService _notificationService;

        public AdminController(ILoanService loanService, INotificationService notificationService)
        {
            _loanService = loanService;
            _notificationService = notificationService;
        }

        [HttpPut("loans/{loanId}/approve")]
        public async Task<ActionResult<ApiResponse<LoanDto>>> ApproveLoan(
            string loanId, 
            [FromBody] ApproveLoanDto approveLoanDto)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(ApiResponse<LoanDto>.ErrorResult("Admin not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<LoanDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _loanService.ApproveLoanAsync(loanId, adminId, approveLoanDto.Notes);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoanDto>.ErrorResult($"Failed to approve loan: {ex.Message}"));
            }
        }

        [HttpPut("loans/{loanId}/reject")]
        public async Task<ActionResult<ApiResponse<LoanDto>>> RejectLoan(
            string loanId, 
            [FromBody] RejectLoanDto rejectLoanDto)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(ApiResponse<LoanDto>.ErrorResult("Admin not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<LoanDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _loanService.RejectLoanAsync(loanId, adminId, rejectLoanDto.Reason, rejectLoanDto.Notes);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoanDto>.ErrorResult($"Failed to reject loan: {ex.Message}"));
            }
        }

        [HttpPost("transactions/disburse")]
        public async Task<ActionResult<ApiResponse<object>>> DisburseLoan([FromBody] DisburseLoanDto disburseLoanDto)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Admin not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.ErrorResult("Validation failed", errors));
                }

                var result = await _loanService.DisburseLoanAsync(
                    disburseLoanDto.LoanId, 
                    adminId, 
                    disburseLoanDto.DisbursementMethod, 
                    disburseLoanDto.Reference);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to disburse loan: {ex.Message}"));
            }
        }

        [HttpPut("loans/{loanId}/close")]
        public async Task<ActionResult<ApiResponse<LoanDto>>> CloseLoan(
            string loanId, 
            [FromBody] CloseLoanDto closeLoanDto)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(ApiResponse<LoanDto>.ErrorResult("Admin not authenticated"));
                }

                var result = await _loanService.CloseLoanAsync(loanId, adminId, closeLoanDto.Notes);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoanDto>.ErrorResult($"Failed to close loan: {ex.Message}"));
            }
        }

        [HttpPost("notifications/send")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> SendNotification([FromBody] CreateNotificationDto notification)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<NotificationDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _notificationService.SendNotificationAsync(notification);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationDto>.ErrorResult($"Failed to send notification: {ex.Message}"));
            }
        }
    }

    // Additional DTOs for admin operations
    public class ApproveLoanDto
    {
        public string ApprovedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class RejectLoanDto
    {
        public string Reason { get; set; } = string.Empty;
        public string RejectedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class DisburseLoanDto
    {
        public string LoanId { get; set; } = string.Empty;
        public string DisbursedBy { get; set; } = string.Empty;
        public string DisbursementMethod { get; set; } = string.Empty;
        public string? Reference { get; set; }
    }

    public class CloseLoanDto
    {
        public string ClosedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
