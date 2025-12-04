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
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("plans")]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetSubscriptionPlans([FromQuery] bool activeOnly = false)
        {
            try
            {
                var result = await _subscriptionService.GetAllSubscriptionPlansAsync(activeOnly);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<SubscriptionPlanDto>>.ErrorResult($"Failed to get subscription plans: {ex.Message}"));
            }
        }

        [HttpGet("plans/{planId}")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> GetSubscriptionPlan(string planId)
        {
            try
            {
                var result = await _subscriptionService.GetSubscriptionPlanAsync(planId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SubscriptionPlanDto>.ErrorResult($"Failed to get subscription plan: {ex.Message}"));
            }
        }

        [HttpGet("my-subscription")]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> GetMySubscription()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UserSubscriptionDto>.ErrorResult("User not authenticated"));
                }

                var result = await _subscriptionService.GetUserSubscriptionAsync(userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResult($"Failed to get subscription: {ex.Message}"));
            }
        }

        [HttpGet("usage-stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetUsageStats()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("User not authenticated"));
                }

                var result = await _subscriptionService.GetUsageStatsAsync(userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to get usage stats: {ex.Message}"));
            }
        }

        [HttpPost("check-feature")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckFeatureAccess([FromBody] CheckFeatureRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _subscriptionService.CheckFeatureAccessAsync(userId, request.Feature);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to check feature access: {ex.Message}"));
            }
        }

        [HttpPost("check-limit")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckLimit([FromBody] CheckLimitRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _subscriptionService.CheckLimitAsync(userId, request.LimitType, request.CurrentCount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to check limit: {ex.Message}"));
            }
        }
    }

    public class CheckFeatureRequest
    {
        public string Feature { get; set; } = string.Empty;
    }

    public class CheckLimitRequest
    {
        public string LimitType { get; set; } = string.Empty;
        public int CurrentCount { get; set; }
    }
}

