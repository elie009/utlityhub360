using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowAll")]
    public class BillsController : ControllerBase
    {
        private readonly IBillService _billService;
        private readonly IBillAnalyticsService _billAnalyticsService;
        private readonly ApplicationDbContext _context;

        public BillsController(IBillService billService, IBillAnalyticsService billAnalyticsService, ApplicationDbContext context)
        {
            _billService = billService;
            _billAnalyticsService = billAnalyticsService;
            _context = context;
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

                // ============================================
                // CONTROLLER-LEVEL VALIDATION: Current year only
                // ============================================
                var currentYear = DateTime.UtcNow.Year;
                var billYear = createBillDto.DueDate.Year;

                if (billYear != currentYear)
                {
                    return BadRequest(ApiResponse<BillDto>.ErrorResult(
                        $"Bills can only be created for the current year ({currentYear}). " +
                        $"You tried to create a bill for {createBillDto.DueDate:MMMM yyyy}. " +
                        $"Please select a date within {currentYear}."));
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

                // ============================================
                // CONTROLLER-LEVEL VALIDATION: Current year only for due date updates
                // ============================================
                if (updateBillDto.DueDate.HasValue)
                {
                    var currentYear = DateTime.UtcNow.Year;
                    var newDueDateYear = updateBillDto.DueDate.Value.Year;

                    if (newDueDateYear != currentYear)
                    {
                        return BadRequest(ApiResponse<BillDto>.ErrorResult(
                            $"Bill due dates can only be set for the current year ({currentYear}). " +
                            $"You tried to set a due date for {updateBillDto.DueDate.Value:MMMM yyyy}. " +
                            $"Please select a date within {currentYear}."));
                    }
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
        public async Task<ActionResult<ApiResponse<BillDto>>> MarkBillAsPaid(string billId, [FromBody] MarkBillPaidDto? request = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));
                }

                var notes = request?.Notes;
                var bankAccountId = request?.BankAccountId;
                var result = await _billService.MarkBillAsPaidAsync(billId, userId, notes, bankAccountId);
                
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

        // Bill Payment Endpoints
        [HttpPost("payments")]
        public async Task<ActionResult<ApiResponse<BillPaymentDto>>> MakeBillPayment([FromBody] CreateBillPaymentDto paymentDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillPaymentDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.MakeBillPaymentAsync(paymentDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResult($"Failed to process bill payment: {ex.Message}"));
            }
        }

        [HttpGet("{billId}/payments")]
        public async Task<ActionResult<ApiResponse<List<BillPaymentDto>>>> GetBillPaymentHistory(string billId, int page = 1, int limit = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BillPaymentDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetBillPaymentHistoryAsync(billId, userId, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillPaymentDto>>.ErrorResult($"Failed to get bill payment history: {ex.Message}"));
            }
        }

        [HttpGet("payments/{paymentId}")]
        public async Task<ActionResult<ApiResponse<BillPaymentDto>>> GetBillPayment(string paymentId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BillPaymentDto>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.GetBillPaymentAsync(paymentId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillPaymentDto>.ErrorResult($"Failed to get bill payment: {ex.Message}"));
            }
        }

        [HttpDelete("payments/{paymentId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBillPayment(string paymentId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _billService.DeleteBillPaymentAsync(paymentId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete bill payment: {ex.Message}"));
            }
        }

        // ============================================
        // Variable Monthly Billing - Analytics Endpoints
        // ============================================

        /// <summary>
        /// Get bill history with analytics for a specific provider/type
        /// </summary>
        [HttpGet("analytics/history")]
        public async Task<ActionResult<ApiResponse<BillHistoryWithAnalyticsDto>>> GetBillHistoryWithAnalytics(
            [FromQuery] string? provider = null,
            [FromQuery] string? billType = null,
            [FromQuery] int months = 6)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillHistoryWithAnalyticsDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetBillHistoryWithAnalyticsAsync(userId, provider, billType, months);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillHistoryWithAnalyticsDto>.ErrorResult($"Failed to get bill history: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get analytics calculations for bills
        /// </summary>
        [HttpGet("analytics/calculations")]
        public async Task<ActionResult<ApiResponse<BillAnalyticsCalculationsDto>>> GetAnalyticsCalculations(
            [FromQuery] string? provider = null,
            [FromQuery] string? billType = null,
            [FromQuery] int months = 6)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillAnalyticsCalculationsDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.CalculateAnalyticsAsync(userId, provider, billType, months);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillAnalyticsCalculationsDto>.ErrorResult($"Failed to calculate analytics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get forecast for next bill
        /// </summary>
        [HttpGet("analytics/forecast")]
        public async Task<ActionResult<ApiResponse<BillForecastDto>>> GetForecast(
            [FromQuery] string provider,
            [FromQuery] string billType,
            [FromQuery] string method = "weighted")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillForecastDto>.ErrorResult("User not authenticated"));

                if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(billType))
                    return BadRequest(ApiResponse<BillForecastDto>.ErrorResult("Provider and billType are required"));

                var result = await _billAnalyticsService.GetForecastAsync(userId, provider, billType, method);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillForecastDto>.ErrorResult($"Failed to get forecast: {ex.Message}"));
            }
        }

        /// <summary>
        /// Calculate variance for a specific bill
        /// </summary>
        [HttpGet("{billId}/variance")]
        public async Task<ActionResult<ApiResponse<BillVarianceDto>>> GetBillVariance(string billId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillVarianceDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.CalculateVarianceAsync(billId, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillVarianceDto>.ErrorResult($"Failed to calculate variance: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get variance dashboard with aggregated variance data for all bills
        /// </summary>
        [HttpGet("analytics/variance-dashboard")]
        public async Task<ActionResult<ApiResponse<VarianceDashboardDto>>> GetVarianceDashboard()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<VarianceDashboardDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetVarianceDashboardAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<VarianceDashboardDto>.ErrorResult($"Failed to get variance dashboard: {ex.Message}"));
            }
        }

        // ============================================
        // Budget Management Endpoints
        // ============================================

        /// <summary>
        /// Create a new budget for a provider/bill type
        /// </summary>
        [HttpPost("budgets")]
        public async Task<ActionResult<ApiResponse<BudgetSettingDto>>> CreateBudget([FromBody] CreateBudgetSettingDto budgetDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BudgetSettingDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.CreateBudgetAsync(budgetDto, userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BudgetSettingDto>.ErrorResult($"Failed to create budget: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing budget
        /// </summary>
        [HttpPut("budgets/{budgetId}")]
        public async Task<ActionResult<ApiResponse<BudgetSettingDto>>> UpdateBudget(
            string budgetId,
            [FromBody] CreateBudgetSettingDto budgetDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BudgetSettingDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.UpdateBudgetAsync(budgetId, budgetDto, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BudgetSettingDto>.ErrorResult($"Failed to update budget: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a budget
        /// </summary>
        [HttpDelete("budgets/{budgetId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBudget(string budgetId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.DeleteBudgetAsync(budgetId, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete budget: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific budget
        /// </summary>
        [HttpGet("budgets/{budgetId}")]
        public async Task<ActionResult<ApiResponse<BudgetSettingDto>>> GetBudget(string budgetId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BudgetSettingDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetBudgetAsync(budgetId, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BudgetSettingDto>.ErrorResult($"Failed to get budget: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all budgets for the user
        /// </summary>
        [HttpGet("budgets")]
        public async Task<ActionResult<ApiResponse<List<BudgetSettingDto>>>> GetUserBudgets()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<BudgetSettingDto>>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetUserBudgetsAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BudgetSettingDto>>.ErrorResult($"Failed to get budgets: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get budget status for a provider/bill type
        /// </summary>
        [HttpGet("budgets/status")]
        public async Task<ActionResult<ApiResponse<BudgetStatusDto>>> GetBudgetStatus(
            [FromQuery] string provider,
            [FromQuery] string billType)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BudgetStatusDto>.ErrorResult("User not authenticated"));

                if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(billType))
                    return BadRequest(ApiResponse<BudgetStatusDto>.ErrorResult("Provider and billType are required"));

                var result = await _billAnalyticsService.GetBudgetStatusAsync(userId, provider, billType);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BudgetStatusDto>.ErrorResult($"Failed to get budget status: {ex.Message}"));
            }
        }

        // ============================================
        // Alerts Endpoints
        // ============================================

        /// <summary>
        /// Get user alerts
        /// </summary>
        [HttpGet("alerts")]
        public async Task<ActionResult<ApiResponse<List<BillAlertDto>>>> GetAlerts(
            [FromQuery] bool? isRead = null,
            [FromQuery] int limit = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<BillAlertDto>>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetUserAlertsAsync(userId, isRead, limit);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillAlertDto>>.ErrorResult($"Failed to get alerts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark an alert as read
        /// </summary>
        [HttpPut("alerts/{alertId}/read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAlertAsRead(string alertId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.MarkAlertAsReadAsync(alertId, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to mark alert as read: {ex.Message}"));
            }
        }

        /// <summary>
        /// Generate alerts for the user
        /// </summary>
        [HttpPost("alerts/generate")]
        public async Task<ActionResult<ApiResponse<List<BillAlertDto>>>> GenerateAlerts()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<BillAlertDto>>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GenerateAlertsAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillAlertDto>>.ErrorResult($"Failed to generate alerts: {ex.Message}"));
            }
        }

        // ============================================
        // Provider Analytics Endpoints
        // ============================================

        /// <summary>
        /// Get analytics for all providers
        /// </summary>
        [HttpGet("analytics/providers")]
        public async Task<ActionResult<ApiResponse<List<ProviderAnalyticsDto>>>> GetProviderAnalytics(
            [FromQuery] int months = 6)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<ProviderAnalyticsDto>>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetProviderAnalyticsAsync(userId, months);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ProviderAnalyticsDto>>.ErrorResult($"Failed to get provider analytics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get analytics for a specific provider
        /// </summary>
        [HttpGet("analytics/providers/{provider}")]
        public async Task<ActionResult<ApiResponse<ProviderAnalyticsDto>>> GetProviderAnalyticsByProvider(
            string provider,
            [FromQuery] string billType,
            [FromQuery] int months = 6)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<ProviderAnalyticsDto>.ErrorResult("User not authenticated"));

                if (string.IsNullOrEmpty(billType))
                    return BadRequest(ApiResponse<ProviderAnalyticsDto>.ErrorResult("Bill type is required"));

                var result = await _billAnalyticsService.GetProviderAnalyticsByProviderAsync(userId, provider, billType, months);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ProviderAnalyticsDto>.ErrorResult($"Failed to get provider analytics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get monthly trend data
        /// </summary>
        [HttpGet("analytics/trend")]
        public async Task<ActionResult<ApiResponse<List<MonthlyBillSummaryDto>>>> GetMonthlyTrend(
            [FromQuery] string? provider = null,
            [FromQuery] string? billType = null,
            [FromQuery] int months = 12)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<MonthlyBillSummaryDto>>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetMonthlyTrendAsync(userId, provider, billType, months);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<MonthlyBillSummaryDto>>.ErrorResult($"Failed to get monthly trend: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get comprehensive dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<BillDashboardDto>>> GetDashboard()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillDashboardDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetDashboardDataAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDashboardDto>.ErrorResult($"Failed to get dashboard: {ex.Message}"));
            }
        }

        // ============================================
        // Auto-Generated Bills Cleanup Endpoints
        // ============================================

        /// <summary>
        /// Clean up auto-generated bills that are not within the current year
        /// </summary>
        [HttpPost("cleanup/out-of-year")]
        public async Task<ActionResult<ApiResponse<CleanupResultDto>>> CleanupOutOfYearAutoGeneratedBills()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<CleanupResultDto>.ErrorResult("User not authenticated"));

                var result = await _billService.CleanupOutOfYearAutoGeneratedBillsAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CleanupResultDto>.ErrorResult($"Failed to cleanup out-of-year auto-generated bills: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get count of auto-generated bills that are outside the current year
        /// </summary>
        [HttpGet("cleanup/out-of-year/count")]
        public async Task<ActionResult<ApiResponse<OutOfYearBillsCountDto>>> GetOutOfYearAutoGeneratedBillsCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<OutOfYearBillsCountDto>.ErrorResult("User not authenticated"));

                var result = await _billService.GetOutOfYearAutoGeneratedBillsCountAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OutOfYearBillsCountDto>.ErrorResult($"Failed to get out-of-year bills count: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete auto-generated bills within a specific date range
        /// </summary>
        [HttpPost("cleanup/date-range")]
        public async Task<ActionResult<ApiResponse<CleanupResultDto>>> CleanupAutoGeneratedBillsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<CleanupResultDto>.ErrorResult("User not authenticated"));

                if (startDate >= endDate)
                    return BadRequest(ApiResponse<CleanupResultDto>.ErrorResult("Start date must be before end date"));

                // Validate date range is reasonable (not too far in the past or future)
                var minDate = new DateTime(2020, 1, 1);
                var maxDate = new DateTime(2050, 12, 31);
                
                if (startDate < minDate || endDate > maxDate)
                    return BadRequest(ApiResponse<CleanupResultDto>.ErrorResult("Date range must be between 2020 and 2050"));

                var result = await _billService.DeleteAutoGeneratedBillsByDateRangeAsync(userId, startDate, endDate);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CleanupResultDto>.ErrorResult($"Failed to cleanup bills by date range: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get count of auto-generated bills within a specific date range
        /// </summary>
        [HttpGet("cleanup/date-range/count")]
        public async Task<ActionResult<ApiResponse<DateRangeBillsCountDto>>> GetAutoGeneratedBillsCountByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<DateRangeBillsCountDto>.ErrorResult("User not authenticated"));

                if (startDate >= endDate)
                    return BadRequest(ApiResponse<DateRangeBillsCountDto>.ErrorResult("Start date must be before end date"));

                var result = await _billService.GetAutoGeneratedBillsCountByDateRangeAsync(userId, startDate, endDate);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<DateRangeBillsCountDto>.ErrorResult($"Failed to get bills count by date range: {ex.Message}"));
            }
        }

        /// <summary>
        /// EMERGENCY DELETE: Remove all auto-generated bills from 2026-2031 immediately
        /// This is a direct, simple deletion method
        /// </summary>
        [HttpPost("emergency-delete-2026-2031")]
        public async Task<ActionResult<ApiResponse<string>>> EmergencyDelete2026to2031()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<string>.ErrorResult("User not authenticated"));

                // Direct database query to find and delete these bills
                var startDate = new DateTime(2026, 1, 1);
                var endDate = new DateTime(2031, 12, 31, 23, 59, 59);

                // Find all auto-generated bills in this range
                var billsToDelete = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.IsAutoGenerated && 
                               b.DueDate >= startDate && 
                               b.DueDate <= endDate)
                    .ToListAsync();

                if (!billsToDelete.Any())
                {
                    return Ok(ApiResponse<string>.SuccessResult("No auto-generated bills found in 2026-2031 range"));
                }

                var billIds = billsToDelete.Select(b => b.Id).ToList();
                
                // Delete related records first
                var payments = await _context.Payments.Where(p => billIds.Contains(p.BillId)).ToListAsync();
                var alerts = await _context.BillAlerts.Where(a => billIds.Contains(a.BillId)).ToListAsync();
                
                // Handle bank transactions
                foreach (var payment in payments.Where(p => p.IsBankTransaction && p.BankAccountId != null))
                {
                    var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == payment.BankAccountId);
                    if (bankAccount != null)
                    {
                        bankAccount.CurrentBalance += payment.Amount;
                        bankAccount.UpdatedAt = DateTime.UtcNow;
                    }

                    var bankTransaction = await _context.BankTransactions
                        .FirstOrDefaultAsync(bt => bt.ReferenceNumber == payment.Reference);
                    if (bankTransaction != null)
                    {
                        _context.BankTransactions.Remove(bankTransaction);
                    }
                }

                // Remove all related records
                if (payments.Any()) _context.Payments.RemoveRange(payments);
                if (alerts.Any()) _context.BillAlerts.RemoveRange(alerts);
                
                // Remove the bills
                _context.Bills.RemoveRange(billsToDelete);
                
                await _context.SaveChangesAsync();

                var message = $"EMERGENCY DELETE COMPLETED: Removed {billsToDelete.Count} auto-generated bills from 2026-2031, {payments.Count} payments, {alerts.Count} alerts";
                
                return Ok(ApiResponse<string>.SuccessResult(message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Emergency delete failed: {ex.Message}"));
            }
        }

        // ============================================
        // Auto-Recurring Bill Generation Endpoints
        // ============================================

        /// <summary>
        /// Auto-generate next month's bill for a specific provider
        /// </summary>
        [HttpPost("auto-generate")]
        public async Task<ActionResult<ApiResponse<BillDto>>> AutoGenerateNextMonthBill(
            [FromQuery] string provider,
            [FromQuery] string billType)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));

                if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(billType))
                    return BadRequest(ApiResponse<BillDto>.ErrorResult("Provider and billType are required"));

                var result = await _billAnalyticsService.AutoGenerateNextMonthBillAsync(userId, provider, billType);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to auto-generate bill: {ex.Message}"));
            }
        }

        /// <summary>
        /// Auto-generate all recurring bills for the user
        /// </summary>
        [HttpPost("auto-generate-all")]
        public async Task<ActionResult<ApiResponse<List<BillDto>>>> AutoGenerateAllRecurringBills()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<BillDto>>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.AutoGenerateAllRecurringBillsAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillDto>>.ErrorResult($"Failed to auto-generate bills: {ex.Message}"));
            }
        }

        /// <summary>
        /// Confirm and update auto-generated bill amount
        /// </summary>
        [HttpPut("{billId}/confirm-amount")]
        public async Task<ActionResult<ApiResponse<BillDto>>> ConfirmAutoGeneratedBill(
            string billId,
            [FromBody] ConfirmBillAmountDto confirmDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.ConfirmAutoGeneratedBillAsync(billId, confirmDto, userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to confirm bill: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all auto-generated bills
        /// </summary>
        [HttpGet("auto-generated")]
        public async Task<ActionResult<ApiResponse<List<BillDto>>>> GetAutoGeneratedBills(
            [FromQuery] bool? confirmed = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<BillDto>>.ErrorResult("User not authenticated"));

                var result = await _billAnalyticsService.GetAutoGeneratedBillsAsync(userId, confirmed);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillDto>>.ErrorResult($"Failed to get auto-generated bills: {ex.Message}"));
            }
        }

        // ============================================
        // Monthly Bill Management Endpoints
        // ============================================

        /// <summary>
        /// Get all bills for a specific month
        /// </summary>
        [HttpGet("monthly")]
        public async Task<ActionResult<ApiResponse<List<BillDto>>>> GetBillsByMonth(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] string? provider = null,
            [FromQuery] string? billType = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<BillDto>>.ErrorResult("User not authenticated"));

                // ============================================
                // VALIDATION: Only allow current year for consistency
                // ============================================
                var currentYear = DateTime.UtcNow.Year;
                
                if (year != currentYear)
                    return BadRequest(ApiResponse<List<BillDto>>.ErrorResult(
                        $"Can only view bills for the current year ({currentYear}). " +
                        $"You requested {year}. Please use {currentYear}."));

                if (month < 1 || month > 12)
                    return BadRequest(ApiResponse<List<BillDto>>.ErrorResult("Invalid month"));

                var result = await _billService.GetBillsByMonthAsync(userId, year, month, provider, billType);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BillDto>>.ErrorResult($"Failed to get monthly bills: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update a specific month's bill amount and details
        /// </summary>
        [HttpPut("{billId}/monthly")]
        public async Task<ActionResult<ApiResponse<BillDto>>> UpdateMonthlyBill(
            string billId,
            [FromBody] UpdateMonthlyBillDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));

                var result = await _billService.UpdateMonthlyBillAsync(billId, updateDto, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to update monthly bill: {ex.Message}"));
            }
        }

        // ============================================
        // SCHEDULED PAYMENT ENDPOINTS
        // ============================================

        [HttpPost("{billId}/schedule-payment")]
        public async Task<ActionResult<ApiResponse<BillDto>>> ConfigureScheduledPayment(
            string billId,
            [FromBody] ConfigureScheduledPaymentDto config)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));

                var result = await _billService.ConfigureScheduledPaymentAsync(billId, userId, config);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to configure scheduled payment: {ex.Message}"));
            }
        }

        [HttpGet("scheduled-payments")]
        public async Task<ActionResult<ApiResponse<List<ScheduledPaymentDto>>>> GetScheduledPayments()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<ScheduledPaymentDto>>.ErrorResult("User not authenticated"));

                var result = await _billService.GetScheduledPaymentsAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ScheduledPaymentDto>>.ErrorResult($"Failed to get scheduled payments: {ex.Message}"));
            }
        }

        // ============================================
        // BILL APPROVAL ENDPOINTS
        // ============================================

        [HttpPost("{billId}/approve")]
        public async Task<ActionResult<ApiResponse<BillDto>>> ApproveBill(
            string billId,
            [FromBody] BillApprovalDto approval)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillDto>.ErrorResult("User not authenticated"));

                var result = await _billService.ApproveBillAsync(billId, userId, approval);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillDto>.ErrorResult($"Failed to approve bill: {ex.Message}"));
            }
        }

        // ============================================
        // PAYMENT HISTORY REPORTS
        // ============================================

        [HttpGet("payment-history-report")]
        public async Task<ActionResult<ApiResponse<BillPaymentHistoryReportDto>>> GetPaymentHistoryReport(
            [FromQuery] string period = "month")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<BillPaymentHistoryReportDto>.ErrorResult("User not authenticated"));

                var result = await _billService.GetBillPaymentHistoryReportAsync(userId, period);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillPaymentHistoryReportDto>.ErrorResult($"Failed to get payment history report: {ex.Message}"));
            }
        }
    }
}
