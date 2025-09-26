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
    public class IncomeSourceController : ControllerBase
    {
        private readonly IIncomeSourceService _incomeSourceService;

        public IncomeSourceController(IIncomeSourceService incomeSourceService)
        {
            _incomeSourceService = incomeSourceService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User not authenticated.");
        }

        /// <summary>
        /// Create a new income source
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<IncomeSourceDto>>> CreateIncomeSource([FromBody] CreateIncomeSourceDto createIncomeSourceDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.CreateIncomeSourceAsync(createIncomeSourceDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<IncomeSourceDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IncomeSourceDto>.ErrorResult($"Failed to create income source: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create multiple income sources
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<ApiResponse<List<IncomeSourceDto>>>> CreateMultipleIncomeSources([FromBody] BulkCreateIncomeSourceDto bulkCreateDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.CreateMultipleIncomeSourcesAsync(new CreateMultipleIncomeSourcesDto { IncomeSources = bulkCreateDto.IncomeSources }, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<IncomeSourceDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to create income sources: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get income source by ID
        /// </summary>
        [HttpGet("{incomeSourceId}")]
        public async Task<ActionResult<ApiResponse<IncomeSourceDto>>> GetIncomeSource(string incomeSourceId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.GetIncomeSourceAsync(incomeSourceId, userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<IncomeSourceDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IncomeSourceDto>.ErrorResult($"Failed to get income source: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all income sources for the user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<IncomeSourceDto>>>> GetAllIncomeSources([FromQuery] bool activeOnly = true, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.GetUserIncomeSourcesAsync(userId, activeOnly);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<IncomeSourceDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to get income sources: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update income source
        /// </summary>
        [HttpPut("{incomeSourceId}")]
        public async Task<ActionResult<ApiResponse<IncomeSourceDto>>> UpdateIncomeSource(string incomeSourceId, [FromBody] UpdateIncomeSourceDto updateIncomeSourceDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.UpdateIncomeSourceAsync(incomeSourceId, updateIncomeSourceDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<IncomeSourceDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IncomeSourceDto>.ErrorResult($"Failed to update income source: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete income source
        /// </summary>
        [HttpDelete("{incomeSourceId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteIncomeSource(string incomeSourceId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.DeleteIncomeSourceAsync(incomeSourceId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete income source: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle income source status (active/inactive)
        /// </summary>
        [HttpPut("{incomeSourceId}/toggle-status")]
        public async Task<ActionResult<ApiResponse<IncomeSourceDto>>> ToggleIncomeSourceStatus(string incomeSourceId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.ToggleIncomeSourceStatusAsync(incomeSourceId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<IncomeSourceDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IncomeSourceDto>.ErrorResult($"Failed to toggle income source status: {ex.Message}"));
            }
        }

        // Analytics and Reporting

        /// <summary>
        /// Get total monthly income
        /// </summary>
        [HttpGet("total-monthly-income")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalMonthlyIncome()
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.GetTotalMonthlyIncomeAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<decimal>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total monthly income: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get income breakdown by category
        /// </summary>
        [HttpGet("income-by-category")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetIncomeByCategory()
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.GetIncomeByCategoryAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<Dictionary<string, decimal>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get income by category: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get income breakdown by frequency
        /// </summary>
        [HttpGet("income-by-frequency")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetIncomeByFrequency()
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.GetIncomeByFrequencyAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<Dictionary<string, decimal>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get income by frequency: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get available income categories
        /// </summary>
        [HttpGet("available-categories")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableIncomeCategories()
        {
            try
            {
                var result = await _incomeSourceService.GetAvailableCategoriesAsync();
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<string>>.ErrorResult($"Failed to get available income categories: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get available income frequencies
        /// </summary>
        [HttpGet("available-frequencies")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableIncomeFrequencies()
        {
            try
            {
                var result = await _incomeSourceService.GetAvailableFrequenciesAsync();
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<string>>.ErrorResult($"Failed to get available income frequencies: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get income sources by category
        /// </summary>
        [HttpGet("by-category/{category}")]
        public async Task<ActionResult<ApiResponse<List<IncomeSourceDto>>>> GetIncomeSourcesByCategory(string category, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.GetIncomeSourcesByCategoryAsync(userId, category);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<IncomeSourceDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to get income sources by category: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get income sources by frequency
        /// </summary>
        [HttpGet("by-frequency/{frequency}")]
        public async Task<ActionResult<ApiResponse<List<IncomeSourceDto>>>> GetIncomeSourcesByFrequency(string frequency, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incomeSourceService.GetIncomeSourcesByFrequencyAsync(userId, frequency);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<IncomeSourceDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to get income sources by frequency: {ex.Message}"));
            }
        }
    }
}