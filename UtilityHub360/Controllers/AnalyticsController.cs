using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/Analytics")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("User not authenticated.");
        }

        /// <summary>
        /// Get monthly incoming and outgoing cash flow for a year
        /// Returns data for all 12 months (January to December)
        /// </summary>
        /// <param name="year">Year to retrieve data for (defaults to current year)</param>
        /// <returns>Monthly cash flow data with incoming and outgoing amounts per month</returns>
        [HttpGet("monthly-cash-flow")]
        public async Task<ActionResult<ApiResponse<MonthlyCashFlowDto>>> GetMonthlyCashFlow([FromQuery] int? year = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _analyticsService.GetMonthlyCashFlowAsync(userId, year);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<MonthlyCashFlowDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MonthlyCashFlowDto>.ErrorResult($"Failed to get monthly cash flow: {ex.Message}"));
            }
        }
    }
}

