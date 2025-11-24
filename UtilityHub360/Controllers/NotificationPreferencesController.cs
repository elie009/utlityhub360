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
    public class NotificationPreferencesController : ControllerBase
    {
        private readonly IEnhancedNotificationService _notificationService;

        public NotificationPreferencesController(IEnhancedNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User not authenticated.");
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationPreferenceDto>>>> GetUserPreferences()
        {
            try
            {
                var userId = GetUserId();
                var result = await _notificationService.GetUserPreferencesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<NotificationPreferenceDto>>.ErrorResult($"Failed to get preferences: {ex.Message}"));
            }
        }

        [HttpGet("{notificationType}")]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> GetPreference(string notificationType)
        {
            try
            {
                var userId = GetUserId();
                var result = await _notificationService.GetPreferenceAsync(userId, notificationType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationPreferenceDto>.ErrorResult($"Failed to get preference: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> CreatePreference([FromBody] CreateNotificationPreferenceDto preference)
        {
            try
            {
                var userId = GetUserId();
                var result = await _notificationService.CreatePreferenceAsync(userId, preference);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationPreferenceDto>.ErrorResult($"Failed to create preference: {ex.Message}"));
            }
        }

        [HttpPut("{notificationType}")]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> UpdatePreference(string notificationType, [FromBody] UpdateNotificationPreferenceDto preference)
        {
            try
            {
                var userId = GetUserId();
                var result = await _notificationService.UpdatePreferenceAsync(userId, notificationType, preference);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationPreferenceDto>.ErrorResult($"Failed to update preference: {ex.Message}"));
            }
        }

        [HttpDelete("{notificationType}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePreference(string notificationType)
        {
            try
            {
                var userId = GetUserId();
                var result = await _notificationService.DeletePreferenceAsync(userId, notificationType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete preference: {ex.Message}"));
            }
        }
    }
}

