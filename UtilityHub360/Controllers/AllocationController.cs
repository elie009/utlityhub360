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
    public class AllocationController : ControllerBase
    {
        private readonly IAllocationService _allocationService;

        public AllocationController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        #region Template Endpoints

        [HttpGet("templates")]
        public async Task<ActionResult<ApiResponse<List<AllocationTemplateDto>>>> GetTemplates()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<AllocationTemplateDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetTemplatesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AllocationTemplateDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("templates/{templateId}")]
        public async Task<ActionResult<ApiResponse<AllocationTemplateDto>>> GetTemplate(string templateId)
        {
            try
            {
                var result = await _allocationService.GetTemplateAsync(templateId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationTemplateDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("templates")]
        public async Task<ActionResult<ApiResponse<AllocationTemplateDto>>> CreateTemplate([FromBody] CreateAllocationTemplateDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationTemplateDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.CreateTemplateAsync(dto, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetTemplate), new { templateId = result.Data?.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationTemplateDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("templates/{templateId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(string templateId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.DeleteTemplateAsync(templateId, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Plan Endpoints

        [HttpGet("plans/active")]
        public async Task<ActionResult<ApiResponse<AllocationPlanDto>>> GetActivePlan()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationPlanDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetActivePlanAsync(userId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationPlanDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("plans")]
        public async Task<ActionResult<ApiResponse<List<AllocationPlanDto>>>> GetPlans()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<AllocationPlanDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetPlansAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AllocationPlanDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("plans/{planId}")]
        public async Task<ActionResult<ApiResponse<AllocationPlanDto>>> GetPlan(string planId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationPlanDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetPlanAsync(planId, userId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationPlanDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("plans")]
        public async Task<ActionResult<ApiResponse<AllocationPlanDto>>> CreatePlan([FromBody] CreateAllocationPlanDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationPlanDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.CreatePlanAsync(dto, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetPlan), new { planId = result.Data?.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationPlanDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("plans/{planId}")]
        public async Task<ActionResult<ApiResponse<AllocationPlanDto>>> UpdatePlan(string planId, [FromBody] UpdateAllocationPlanDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationPlanDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.UpdatePlanAsync(planId, dto, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationPlanDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("plans/{planId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePlan(string planId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.DeletePlanAsync(planId, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("plans/apply-template")]
        public async Task<ActionResult<ApiResponse<AllocationPlanDto>>> ApplyTemplate([FromBody] ApplyTemplateDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationPlanDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.ApplyTemplateAsync(dto.TemplateId, dto.MonthlyIncome, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationPlanDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Category Endpoints

        [HttpGet("plans/{planId}/categories")]
        public async Task<ActionResult<ApiResponse<List<AllocationCategoryDto>>>> GetCategories(string planId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<AllocationCategoryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetCategoriesAsync(planId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AllocationCategoryDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<AllocationCategoryDto>>> GetCategory(string categoryId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetCategoryAsync(categoryId, userId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationCategoryDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<AllocationCategoryDto>>> UpdateCategory(string categoryId, [FromBody] CreateAllocationCategoryDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.UpdateCategoryAsync(categoryId, dto, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationCategoryDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(string categoryId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.DeleteCategoryAsync(categoryId, userId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region History & Tracking Endpoints

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<AllocationHistoryDto>>>> GetHistory([FromQuery] AllocationHistoryQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<AllocationHistoryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetHistoryAsync(userId, query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AllocationHistoryDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("plans/{planId}/record-history")]
        public async Task<ActionResult<ApiResponse<bool>>> RecordHistory(string planId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.RecordHistoryAsync(userId, planId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("trends")]
        public async Task<ActionResult<ApiResponse<List<AllocationTrendDto>>>> GetTrends([FromQuery] string? planId = null, [FromQuery] int months = 12)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<AllocationTrendDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetTrendsAsync(userId, planId, months);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AllocationTrendDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Recommendation Endpoints

        [HttpGet("recommendations")]
        public async Task<ActionResult<ApiResponse<List<AllocationRecommendationDto>>>> GetRecommendations([FromQuery] string? planId = null)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<AllocationRecommendationDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GetRecommendationsAsync(userId, planId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AllocationRecommendationDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("recommendations/{recommendationId}/read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkRecommendationRead(string recommendationId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.MarkRecommendationReadAsync(recommendationId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("recommendations/{recommendationId}/apply")]
        public async Task<ActionResult<ApiResponse<bool>>> ApplyRecommendation(string recommendationId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.ApplyRecommendationAsync(recommendationId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("plans/{planId}/generate-recommendations")]
        public async Task<ActionResult<ApiResponse<List<AllocationRecommendationDto>>>> GenerateRecommendations(string planId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<AllocationRecommendationDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.GenerateRecommendationsAsync(userId, planId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AllocationRecommendationDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Calculation & Chart Endpoints

        [HttpPost("calculate")]
        public async Task<ActionResult<ApiResponse<AllocationCalculationDto>>> CalculateAllocation([FromBody] CalculateAllocationDto dto)
        {
            try
            {
                var result = await _allocationService.CalculateAllocationAsync(dto.MonthlyIncome, dto.Categories);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationCalculationDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("plans/{planId}/summary")]
        public async Task<ActionResult<ApiResponse<AllocationSummaryDto>>> GetSummary(string planId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationSummaryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _allocationService.CalculateSummaryAsync(planId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationSummaryDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("plans/{planId}/chart-data")]
        public async Task<ActionResult<ApiResponse<AllocationChartDataDto>>> GetChartData(string planId, [FromQuery] DateTime? periodDate = null)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<AllocationChartDataDto>.ErrorResult("User not authenticated"));
                }

                ApiResponse<AllocationChartDataDto> result;
                if (periodDate.HasValue)
                {
                    result = await _allocationService.GetChartDataForPeriodAsync(planId, userId, periodDate.Value);
                }
                else
                {
                    result = await _allocationService.GetChartDataAsync(planId, userId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AllocationChartDataDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        #endregion
    }

    // Helper DTOs for controller
    public class ApplyTemplateDto
    {
        public string TemplateId { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
    }

    public class CalculateAllocationDto
    {
        public decimal MonthlyIncome { get; set; }
        public List<CreateAllocationTemplateCategoryDto> Categories { get; set; } = new List<CreateAllocationTemplateCategoryDto>();
    }
}

