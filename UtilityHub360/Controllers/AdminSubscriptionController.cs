using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/admin/subscriptions")]
    [Authorize(Roles = "ADMIN")]
    public class AdminSubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public AdminSubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        // Subscription Plan Management
        [HttpPost("plans")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<SubscriptionPlanDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _subscriptionService.CreateSubscriptionPlanAsync(createDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SubscriptionPlanDto>.ErrorResult($"Failed to create subscription plan: {ex.Message}"));
            }
        }

        [HttpPut("plans/{planId}")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> UpdateSubscriptionPlan(string planId, [FromBody] UpdateSubscriptionPlanDto updateDto)
        {
            try
            {
                var result = await _subscriptionService.UpdateSubscriptionPlanAsync(planId, updateDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SubscriptionPlanDto>.ErrorResult($"Failed to update subscription plan: {ex.Message}"));
            }
        }

        [HttpDelete("plans/{planId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSubscriptionPlan(string planId)
        {
            try
            {
                var result = await _subscriptionService.DeleteSubscriptionPlanAsync(planId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete subscription plan: {ex.Message}"));
            }
        }

        // User Subscription Management
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<UserSubscriptionDto>>>> GetAllUserSubscriptions(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50,
            [FromQuery] string? status = null,
            [FromQuery] string? planId = null)
        {
            try
            {
                var result = await _subscriptionService.GetAllUserSubscriptionsAsync(page, limit, status, planId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserSubscriptionDto>>.ErrorResult($"Failed to get user subscriptions: {ex.Message}"));
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<ApiResponse<UserWithSubscriptionDto>>> GetUserWithSubscription(string userId)
        {
            try
            {
                var result = await _subscriptionService.GetUserWithSubscriptionAsync(userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserWithSubscriptionDto>.ErrorResult($"Failed to get user subscription: {ex.Message}"));
            }
        }

        [HttpPost("users")]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> CreateUserSubscription([FromBody] CreateUserSubscriptionDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _subscriptionService.CreateUserSubscriptionAsync(createDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResult($"Failed to create user subscription: {ex.Message}"));
            }
        }

        [HttpPut("users/{subscriptionId}")]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> UpdateUserSubscription(string subscriptionId, [FromBody] UpdateUserSubscriptionDto updateDto)
        {
            try
            {
                var result = await _subscriptionService.UpdateUserSubscriptionAsync(subscriptionId, updateDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResult($"Failed to update user subscription: {ex.Message}"));
            }
        }

        [HttpPost("users/{subscriptionId}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelUserSubscription(string subscriptionId)
        {
            try
            {
                var result = await _subscriptionService.CancelUserSubscriptionAsync(subscriptionId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to cancel subscription: {ex.Message}"));
            }
        }

        [HttpPost("users/{userId}/reset-usage")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetUserUsage(string userId)
        {
            try
            {
                var result = await _subscriptionService.ResetMonthlyUsageAsync(userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to reset usage: {ex.Message}"));
            }
        }
    }
}

