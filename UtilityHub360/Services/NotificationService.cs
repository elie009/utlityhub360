using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PaginatedResponse<NotificationDto>>> GetUserNotificationsAsync(string userId, string? status, string? type, int page, int limit)
        {
            try
            {
                var query = _context.Notifications.Where(n => n.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    if (status.ToLower() == "unread")
                    {
                        query = query.Where(n => !n.IsRead);
                    }
                    else if (status.ToLower() == "read")
                    {
                        query = query.Where(n => n.IsRead);
                    }
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(n => n.Type == type);
                }

                var totalCount = await query.CountAsync();
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var notificationDtos = notifications.Select(notification => new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    ReadAt = notification.ReadAt
                }).ToList();

                var paginatedResponse = new PaginatedResponse<NotificationDto>
                {
                    Data = notificationDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<NotificationDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<NotificationDto>>.ErrorResult($"Failed to get notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationDto>> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Notification not found");
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    ReadAt = notification.ReadAt
                };

                return ApiResponse<NotificationDto>.SuccessResult(notificationDto, "Notification marked as read");
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationDto>.ErrorResult($"Failed to mark notification as read: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationDto>> SendNotificationAsync(CreateNotificationDto notification)
        {
            try
            {
                var newNotification = new Entities.Notification
                {
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(newNotification);
                await _context.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = newNotification.Id,
                    UserId = newNotification.UserId,
                    Type = newNotification.Type,
                    Title = newNotification.Title,
                    Message = newNotification.Message,
                    IsRead = newNotification.IsRead,
                    CreatedAt = newNotification.CreatedAt,
                    ReadAt = newNotification.ReadAt
                };

                return ApiResponse<NotificationDto>.SuccessResult(notificationDto, "Notification sent successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationDto>.ErrorResult($"Failed to send notification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<int>> GetUnreadNotificationCountAsync(string userId)
        {
            try
            {
                var count = await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                return ApiResponse<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<int>.ErrorResult($"Failed to get unread notification count: {ex.Message}");
            }
        }
    }
}
