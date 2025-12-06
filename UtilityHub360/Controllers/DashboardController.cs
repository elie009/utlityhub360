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
    public class DashboardController : ControllerBase
    {
        private readonly IDisposableAmountService _disposableAmountService;
        private readonly ISubscriptionService _subscriptionService;

        public DashboardController(IDisposableAmountService disposableAmountService, ISubscriptionService subscriptionService)
        {
            _disposableAmountService = disposableAmountService;
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Get disposable amount and complete financial details
        /// - Current month: No parameters
        /// - Specific month: Pass year and month
        /// - Custom range: Pass startDate and endDate
        /// </summary>
        [HttpGet("disposable-amount")]
        public async Task<ActionResult<ApiResponse<DisposableAmountDto>>> GetDisposableAmount(
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? targetSavings = null,
            [FromQuery] decimal? investmentAllocation = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<DisposableAmountDto>.ErrorResult("User not authenticated"));
                }

                DisposableAmountDto result;

                // Custom date range
                if (startDate.HasValue && endDate.HasValue)
                {
                    if (startDate.Value > endDate.Value)
                    {
                        return BadRequest(ApiResponse<DisposableAmountDto>.ErrorResult("Start date must be before end date"));
                    }
                    result = await _disposableAmountService.GetDisposableAmountAsync(
                        userId, 
                        startDate.Value, 
                        endDate.Value, 
                        targetSavings, 
                        investmentAllocation);
                }
                // Specific month
                else if (year.HasValue && month.HasValue)
                {
                    if (year.Value < 2000 || year.Value > 2100 || month.Value < 1 || month.Value > 12)
                    {
                        return BadRequest(ApiResponse<DisposableAmountDto>.ErrorResult("Invalid year or month"));
                    }
                    result = await _disposableAmountService.GetMonthlyDisposableAmountAsync(
                        userId, 
                        year.Value, 
                        month.Value, 
                        targetSavings, 
                        investmentAllocation);
                }
                // Current month (default)
                else
                {
                    result = await _disposableAmountService.GetCurrentMonthDisposableAmountAsync(
                        userId, 
                        targetSavings, 
                        investmentAllocation);
                }

                return Ok(ApiResponse<DisposableAmountDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<DisposableAmountDto>.ErrorResult(
                    $"Failed to calculate disposable amount: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get simple financial summary: Income - Expenses - Savings = Remaining Amount
        /// Current month by default, or specify year and month
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<SimpleFinancialSummaryDto>>> GetSimpleSummary(
            [FromQuery] int? year = null,
            [FromQuery] int? month = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<SimpleFinancialSummaryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _disposableAmountService.GetSimpleFinancialSummaryAsync(userId, year, month);

                return Ok(ApiResponse<SimpleFinancialSummaryDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SimpleFinancialSummaryDto>.ErrorResult(
                    $"Failed to get financial summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get comprehensive financial summary (year-to-date, trends, etc.)
        /// </summary>
        [HttpGet("financial-summary")]
        public async Task<ActionResult<ApiResponse<FinancialSummaryDto>>> GetFinancialSummary()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<FinancialSummaryDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Financial Health Score feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "FINANCIAL_HEALTH_SCORE");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<FinancialSummaryDto>.ErrorResult(
                        "Financial Health Score is a Premium feature. Please upgrade to Premium to access this feature."));
                }

                var result = await _disposableAmountService.GetFinancialSummaryAsync(userId);

                return Ok(ApiResponse<FinancialSummaryDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<FinancialSummaryDto>.ErrorResult(
                    $"Failed to generate financial summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get recent activity data for dashboard
        /// Returns income sources count, monthly income, goals, and disposable amount
        /// </summary>
        [HttpGet("recent-activity")]
        public async Task<ActionResult<ApiResponse<RecentActivityDto>>> GetRecentActivity()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<RecentActivityDto>.ErrorResult("User not authenticated"));
                }

                var result = await _disposableAmountService.GetRecentActivityAsync(userId);

                return Ok(ApiResponse<RecentActivityDto>.SuccessResult(result, "Recent activity retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RecentActivityDto>.ErrorResult(
                    $"Failed to get recent activity: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get disposable amount for any user (Admin only)
        /// </summary>
        [HttpGet("disposable-amount/{userId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<DisposableAmountDto>>> GetUserDisposableAmount(
            string userId,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? targetSavings = null,
            [FromQuery] decimal? investmentAllocation = null)
        {
            try
            {
                DisposableAmountDto result;

                // Custom date range
                if (startDate.HasValue && endDate.HasValue)
                {
                    if (startDate.Value > endDate.Value)
                    {
                        return BadRequest(ApiResponse<DisposableAmountDto>.ErrorResult("Start date must be before end date"));
                    }
                    result = await _disposableAmountService.GetDisposableAmountAsync(
                        userId, 
                        startDate.Value, 
                        endDate.Value, 
                        targetSavings, 
                        investmentAllocation);
                }
                // Specific month
                else if (year.HasValue && month.HasValue)
                {
                    if (year.Value < 2000 || year.Value > 2100 || month.Value < 1 || month.Value > 12)
                    {
                        return BadRequest(ApiResponse<DisposableAmountDto>.ErrorResult("Invalid year or month"));
                    }
                    result = await _disposableAmountService.GetMonthlyDisposableAmountAsync(
                        userId, 
                        year.Value, 
                        month.Value, 
                        targetSavings, 
                        investmentAllocation);
                }
                // Current month (default)
                else
                {
                    result = await _disposableAmountService.GetCurrentMonthDisposableAmountAsync(
                        userId, 
                        targetSavings, 
                        investmentAllocation);
                }

                return Ok(ApiResponse<DisposableAmountDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<DisposableAmountDto>.ErrorResult(
                    $"Failed to calculate disposable amount: {ex.Message}"));
            }
        }
    }
}

