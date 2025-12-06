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
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IEnhancedNotificationService _enhancedNotificationService;

        public NotificationsController(INotificationService notificationService, IEnhancedNotificationService enhancedNotificationService)
        {
            _notificationService = notificationService;
            _enhancedNotificationService = enhancedNotificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<NotificationDto>>>> GetUserNotifications(
            string userId,
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only view their own notifications unless they're admin
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                var result = await _notificationService.GetUserNotificationsAsync(userId, status, type, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<NotificationDto>>.ErrorResult($"Failed to get notifications: {ex.Message}"));
            }
        }

        [HttpPut("{notificationId}/read")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> MarkNotificationAsRead(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<NotificationDto>.ErrorResult("User not authenticated"));
                }

                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationDto>.ErrorResult($"Failed to mark notification as read: {ex.Message}"));
            }
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadNotificationCount(string userId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only view their own notification count unless they're admin
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                var result = await _notificationService.GetUnreadNotificationCountAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<int>.ErrorResult($"Failed to get unread notification count: {ex.Message}"));
            }
        }

        // Enhanced notification endpoints
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> SendNotification([FromBody] SendNotificationRequestDto request)
        {
            try
            {
                var result = await _enhancedNotificationService.SendNotificationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationDto>.ErrorResult($"Failed to send notification: {ex.Message}"));
            }
        }

        [HttpPost("send/bulk")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> SendBulkNotifications([FromBody] List<SendNotificationRequestDto> requests)
        {
            try
            {
                var result = await _enhancedNotificationService.SendBulkNotificationsAsync(requests);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<NotificationDto>>.ErrorResult($"Failed to send bulk notifications: {ex.Message}"));
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<NotificationHistoryDto>>>> GetNotificationHistory([FromQuery] NotificationHistoryQueryDto query)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaginatedResponse<NotificationHistoryDto>>.ErrorResult("User not authenticated"));
                }

                query.UserId = userId;
                var result = await _enhancedNotificationService.GetNotificationHistoryAsync(userId, query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<NotificationHistoryDto>>.ErrorResult($"Failed to get history: {ex.Message}"));
            }
        }

        [HttpGet("scheduled")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetScheduledNotifications()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<NotificationDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _enhancedNotificationService.GetScheduledNotificationsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<NotificationDto>>.ErrorResult($"Failed to get scheduled notifications: {ex.Message}"));
            }
        }

        [HttpPut("{notificationId}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelScheduledNotification(string notificationId)
        {
            try
            {
                var result = await _enhancedNotificationService.CancelScheduledNotificationAsync(notificationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to cancel scheduled notification: {ex.Message}"));
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(string notificationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete notification: {ex.Message}"));
            }
        }

        [HttpDelete("user/{userId}/all")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteAllNotifications(string userId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only delete their own notifications unless they're admin
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                var result = await _notificationService.DeleteAllNotificationsAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<int>.ErrorResult($"Failed to delete all notifications: {ex.Message}"));
            }
        }
    }
}

