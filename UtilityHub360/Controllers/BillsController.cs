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
    [Authorize]
    public class BillsController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillsController(IBillService billService)
        {
            _billService = billService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BillDto>>> CreateBill([FromBody] CreateBillDto createBillDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.CreateBillAsync(createBillDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to create bill: {ex.Message}"));
            }
        }

        [HttpGet("{billId}")]
        public async Task<ActionResult<ApiResponse<BillDto>>> GetBill(string billId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetBillAsync(billId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to get bill: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<BillDto>>>> GetUserBills(
            [FromQuery] string? status = null,
            [FromQuery] string? billType = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaginatedResponse<BillDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetUserBillsAsync(userId, status, billType, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<BillDto>>.ErrorResult($"Failed to get bills: {ex.Message}"));
            }
        }

        [HttpPut("{billId}")]
        public async Task<ActionResult<ApiResponse<BillDto>>> UpdateBill(string billId, [FromBody] UpdateBillDto updateBillDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.UpdateBillAsync(billId, updateBillDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to update bill: {ex.Message}"));
            }
        }

        [HttpDelete("{billId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBill(string billId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.DeleteBillAsync(billId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete bill: {ex.Message}"));
            }
        }

        // Analytics Endpoints

        [HttpGet("analytics/total-pending")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalPendingAmount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetTotalPendingAmountAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total pending amount: {ex.Message}"));
            }
        }

        [HttpGet("analytics/total-paid")]
        public async Task<ActionResult<ApiResponse<BillSummaryDto>>> GetTotalPaidAmount([FromQuery] string period = "month")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillSummaryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetTotalPaidAmountAsync(userId, period);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillSummaryDto>.ErrorResult($"Failed to get total paid amount: {ex.Message}"));
            }
        }

        [HttpGet("analytics/total-overdue")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalOverdueAmount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetTotalOverdueAmountAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total overdue amount: {ex.Message}"));
            }
        }

        [HttpGet("analytics/summary")]
        public async Task<ActionResult<ApiResponse<BillAnalyticsDto>>> GetBillAnalytics()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillAnalyticsDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetBillAnalyticsAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillAnalyticsDto>.ErrorResult($"Failed to get bill analytics: {ex.Message}"));
            }
        }

        // Bill Management Endpoints

        [HttpPut("{billId}/mark-paid")]
        public async Task<ActionResult<ApiResponse<BillDto>>> MarkBillAsPaid(string billId, [FromBody] string? notes = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.MarkBillAsPaidAsync(billId, userId, notes);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to mark bill as paid: {ex.Message}"));
            }
        }

        [HttpPut("{billId}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateBillStatus(string billId, [FromBody] string status)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.UpdateBillStatusAsync(billId, status, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to update bill status: {ex.Message}"));
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<ApiResponse<List<BillDto>>>> GetOverdueBills()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BillDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetOverdueBillsAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillDto>>.ErrorResult($"Failed to get overdue bills: {ex.Message}"));
            }
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponse<List<BillDto>>>> GetUpcomingBills([FromQuery] int days = 7)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BillDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetUpcomingBillsAsync(userId, days);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillDto>>.ErrorResult($"Failed to get upcoming bills: {ex.Message}"));
            }
        }

        // Admin Endpoints

        [HttpGet("admin/all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<BillDto>>>> GetAllBills(
            [FromQuery] string? status = null,
            [FromQuery] string? billType = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _billService.GetAllBillsAsync(status, billType, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<BillDto>>.ErrorResult($"Failed to get all bills: {ex.Message}"));
            }
        }
    }
}
