using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowAll")]
    public class UtilitiesController : ControllerBase
    {
        private readonly IUtilityService _utilityService;

        public UtilitiesController(IUtilityService utilityService)
        {
            _utilityService = utilityService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UtilityDto>>> CreateUtility([FromBody] CreateUtilityDto createUtilityDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UtilityDto>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.CreateUtilityAsync(createUtilityDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UtilityDto>.ErrorResult($"Failed to create utility: {ex.Message}"));
            }
        }

        [HttpGet("{utilityId}")]
        public async Task<ActionResult<ApiResponse<UtilityDto>>> GetUtility(string utilityId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UtilityDto>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetUtilityAsync(utilityId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UtilityDto>.ErrorResult($"Failed to get utility: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<UtilityDto>>>> GetUserUtilities(
            [FromQuery] string? status = null,
            [FromQuery] string? utilityType = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaginatedResponse<UtilityDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetUserUtilitiesAsync(userId, status, utilityType, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<UtilityDto>>.ErrorResult($"Failed to get utilities: {ex.Message}"));
            }
        }

        [HttpPut("{utilityId}")]
        public async Task<ActionResult<ApiResponse<UtilityDto>>> UpdateUtility(string utilityId, [FromBody] UpdateUtilityDto updateUtilityDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UtilityDto>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.UpdateUtilityAsync(utilityId, updateUtilityDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UtilityDto>.ErrorResult($"Failed to update utility: {ex.Message}"));
            }
        }

        [HttpDelete("{utilityId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUtility(string utilityId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.DeleteUtilityAsync(utilityId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete utility: {ex.Message}"));
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

                var result = await _utilityService.GetTotalPendingAmountAsync(userId);
                
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
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalPaidAmount([FromQuery] string period = "month")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetTotalPaidAmountAsync(userId, period);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total paid amount: {ex.Message}"));
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

                var result = await _utilityService.GetTotalOverdueAmountAsync(userId);
                
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
        public async Task<ActionResult<ApiResponse<UtilityAnalyticsDto>>> GetUtilityAnalytics()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UtilityAnalyticsDto>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetUtilityAnalyticsAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UtilityAnalyticsDto>.ErrorResult($"Failed to get utility analytics: {ex.Message}"));
            }
        }

        // Utility Management Endpoints

        [HttpPut("{utilityId}/mark-paid")]
        public async Task<ActionResult<ApiResponse<UtilityDto>>> MarkUtilityAsPaid(string utilityId, [FromBody] MarkUtilityPaidDto? request = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UtilityDto>.ErrorResult("User not authenticated"));
                }

                var notes = request?.Notes;
                var bankAccountId = request?.BankAccountId;
                var result = await _utilityService.MarkUtilityAsPaidAsync(utilityId, userId, notes, bankAccountId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UtilityDto>.ErrorResult($"Failed to mark utility as paid: {ex.Message}"));
            }
        }

        [HttpPut("{utilityId}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUtilityStatus(string utilityId, [FromBody] string status)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.UpdateUtilityStatusAsync(utilityId, status, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to update utility status: {ex.Message}"));
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<ApiResponse<List<UtilityDto>>>> GetOverdueUtilities()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<UtilityDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetOverdueUtilitiesAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UtilityDto>>.ErrorResult($"Failed to get overdue utilities: {ex.Message}"));
            }
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponse<List<UtilityDto>>>> GetUpcomingUtilities([FromQuery] int days = 7)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<UtilityDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetUpcomingUtilitiesAsync(userId, days);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UtilityDto>>.ErrorResult($"Failed to get upcoming utilities: {ex.Message}"));
            }
        }

        // Consumption Tracking Endpoints

        [HttpGet("{utilityId}/consumption-history")]
        public async Task<ActionResult<ApiResponse<UtilityConsumptionHistoryDto>>> GetConsumptionHistory(
            string utilityId,
            [FromQuery] int months = 12)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UtilityConsumptionHistoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetConsumptionHistoryAsync(utilityId, userId, months);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UtilityConsumptionHistoryDto>.ErrorResult($"Failed to get consumption history: {ex.Message}"));
            }
        }

        [HttpGet("consumption-history")]
        public async Task<ActionResult<ApiResponse<List<UtilityConsumptionHistoryDto>>>> GetAllConsumptionHistory(
            [FromQuery] string? utilityType = null,
            [FromQuery] int months = 12)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<UtilityConsumptionHistoryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.GetAllConsumptionHistoryAsync(userId, utilityType, months);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UtilityConsumptionHistoryDto>>.ErrorResult($"Failed to get consumption history: {ex.Message}"));
            }
        }

        // Comparison Tools Endpoints

        [HttpGet("compare/providers")]
        public async Task<ActionResult<ApiResponse<UtilityComparisonDto>>> CompareProviders([FromQuery] string utilityType)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UtilityComparisonDto>.ErrorResult("User not authenticated"));
                }

                if (string.IsNullOrEmpty(utilityType))
                {
                    return BadRequest(ApiResponse<UtilityComparisonDto>.ErrorResult("Utility type is required"));
                }

                var result = await _utilityService.CompareProvidersAsync(userId, utilityType);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UtilityComparisonDto>.ErrorResult($"Failed to compare providers: {ex.Message}"));
            }
        }

        [HttpGet("compare/all")]
        public async Task<ActionResult<ApiResponse<List<UtilityComparisonDto>>>> CompareAllUtilityTypes()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<UtilityComparisonDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _utilityService.CompareAllUtilityTypesAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UtilityComparisonDto>>.ErrorResult($"Failed to compare utility types: {ex.Message}"));
            }
        }

        [HttpGet("compare/providers-list")]
        public async Task<ActionResult<ApiResponse<List<ProviderComparisonDto>>>> GetProviderComparison([FromQuery] string utilityType)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ProviderComparisonDto>>.ErrorResult("User not authenticated"));
                }

                if (string.IsNullOrEmpty(utilityType))
                {
                    return BadRequest(ApiResponse<List<ProviderComparisonDto>>.ErrorResult("Utility type is required"));
                }

                var result = await _utilityService.GetProviderComparisonAsync(userId, utilityType);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ProviderComparisonDto>>.ErrorResult($"Failed to get provider comparison: {ex.Message}"));
            }
        }
    }
}

