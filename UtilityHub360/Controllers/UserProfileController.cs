using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User not authenticated.");
        }

        /// <summary>
        /// Create a new user profile
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> CreateUserProfile([FromBody] CreateUserProfileDto createUserProfileDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.CreateUserProfileAsync(createUserProfileDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to create user profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.GetUserProfileAsync(userId);
                
                if (!result.Success)
                {
                    // Return 200 OK with null data instead of 404 Not Found
                    return Ok(ApiResponse<UserProfileDto>.SuccessResult(null, "No user profile found. Create a profile first."));
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to get user profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut]
        [HttpPost("update")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.UpdateUserProfileAsync(updateUserProfileDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to update user profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete user profile
        /// </summary>
        [HttpDelete]
        [HttpPost("delete")]  // POST alternative for environments where DELETE is blocked
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUserProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.DeleteUserProfileAsync(userId);
                
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
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete user profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Activate user profile
        /// </summary>
        [HttpPut("activate")]
        [HttpPost("activate")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<bool>>> ActivateUserProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.ActivateUserProfileAsync(userId);
                
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
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to activate user profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// Deactivate user profile
        /// </summary>
        [HttpPut("deactivate")]
        [HttpPost("deactivate")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateUserProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.DeactivateUserProfileAsync(userId);
                
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
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to deactivate user profile: {ex.Message}"));
            }
        }

        // Financial Goals Management

        /// <summary>
        /// Update savings goal
        /// </summary>
        [HttpPut("savings-goal")]
        [HttpPost("savings-goal")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateSavingsGoal([FromBody] UpdateGoalDto updateGoalDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.UpdateSavingsGoalAsync(updateGoalDto.Amount, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to update savings goal: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update investment goal
        /// </summary>
        [HttpPut("investment-goal")]
        [HttpPost("investment-goal")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateInvestmentGoal([FromBody] UpdateGoalDto updateGoalDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.UpdateInvestmentGoalAsync(updateGoalDto.Amount, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to update investment goal: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update emergency fund goal
        /// </summary>
        [HttpPut("emergency-fund-goal")]
        [HttpPost("emergency-fund-goal")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateEmergencyFundGoal([FromBody] UpdateGoalDto updateGoalDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.UpdateEmergencyFundGoalAsync(updateGoalDto.Amount, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to update emergency fund goal: {ex.Message}"));
            }
        }

        // Tax Management

        /// <summary>
        /// Update tax information
        /// </summary>
        [HttpPut("tax-information")]
        [HttpPost("tax-information")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateTaxInformation([FromBody] UpdateTaxInformationDto updateTaxInformationDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.UpdateTaxInformationAsync(
                    updateTaxInformationDto.TaxRate,
                    updateTaxInformationDto.MonthlyTaxDeductions,
                    userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to update tax information: {ex.Message}"));
            }
        }

        // Employment Information

        /// <summary>
        /// Update employment information
        /// </summary>
        [HttpPut("employment-info")]
        [HttpPost("employment-info")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateEmploymentInfo([FromBody] UpdateEmploymentInfoDto updateEmploymentInfoDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.UpdateEmploymentInfoAsync(
                    updateEmploymentInfoDto.JobTitle,
                    updateEmploymentInfoDto.Company,
                    updateEmploymentInfoDto.EmploymentType,
                    updateEmploymentInfoDto.Industry ?? string.Empty,
                    updateEmploymentInfoDto.Location ?? string.Empty,
                    userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to update employment information: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update preferred currency
        /// </summary>
        [HttpPut("currency")]
        [HttpPost("currency")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdatePreferredCurrency([FromBody] UpdateCurrencyDto updateCurrencyDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.UpdatePreferredCurrencyAsync(updateCurrencyDto.Currency, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResult($"Failed to update preferred currency: {ex.Message}"));
            }
        }

        // Analytics and Reporting

        /// <summary>
        /// Get goals breakdown
        /// </summary>
        [HttpGet("goals-breakdown")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetGoalsBreakdown()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.GetGoalsBreakdownAsync(userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<Dictionary<string, decimal>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get goals breakdown: {ex.Message}"));
            }
        }

        /// <summary>
        /// Validate income goals
        /// </summary>
        [HttpGet("validate-goals")]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateIncomeGoals()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.ValidateIncomeGoalsAsync(userId);
                
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
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to validate income goals: {ex.Message}"));
            }
        }

        /// <summary>
        /// Calculate savings rate
        /// </summary>
        [HttpGet("savings-rate")]
        public async Task<ActionResult<ApiResponse<decimal>>> CalculateSavingsRate()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.CalculateSavingsRateAsync(userId);
                
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
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to calculate savings rate: {ex.Message}"));
            }
        }

        /// <summary>
        /// Calculate disposable income
        /// </summary>
        [HttpGet("disposable-income")]
        public async Task<ActionResult<ApiResponse<decimal>>> CalculateDisposableIncome()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userProfileService.CalculateDisposableIncomeAsync(userId);
                
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
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to calculate disposable income: {ex.Message}"));
            }
        }

        // Admin Operations

        /// <summary>
        /// Get all user profiles (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<List<UserProfileDto>>>> GetAllUserProfiles([FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var result = await _userProfileService.GetAllUserProfilesAsync(page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserProfileDto>>.ErrorResult($"Failed to get all user profiles: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user profiles by income range (Admin only)
        /// </summary>
        [HttpGet("admin/filter-by-income")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<List<UserProfileDto>>>> GetProfilesByIncomeRange([FromQuery] decimal minIncome, [FromQuery] decimal maxIncome, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var result = await _userProfileService.GetProfilesByIncomeRangeAsync(minIncome, maxIncome, page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserProfileDto>>.ErrorResult($"Failed to filter user profiles by income range: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user profiles by employment type (Admin only)
        /// </summary>
        [HttpGet("admin/filter-by-employment-type")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<List<UserProfileDto>>>> GetProfilesByEmploymentType([FromQuery] string employmentType, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var result = await _userProfileService.GetProfilesByEmploymentTypeAsync(employmentType, page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserProfileDto>>.ErrorResult($"Failed to filter user profiles by employment type: {ex.Message}"));
            }
        }
    }

    // DTOs for the controller
    public class UpdateGoalDto
    {
        public decimal Amount { get; set; }
    }

    public class UpdateTaxInformationDto
    {
        public decimal? TaxRate { get; set; }
        public decimal? MonthlyTaxDeductions { get; set; }
    }

    public class UpdateEmploymentInfoDto
    {
        public string? JobTitle { get; set; }
        public string? Company { get; set; }
        public string? EmploymentType { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
    }

    public class UpdateCurrencyDto
    {
        [Required]
        [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters")]
        public string Currency { get; set; } = string.Empty;
    }
}