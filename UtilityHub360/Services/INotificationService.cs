using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface INotificationService
    {
        Task<ApiResponse<PaginatedResponse<NotificationDto>>> GetUserNotificationsAsync(string userId, string? status, string? type, int page, int limit);
        Task<ApiResponse<NotificationDto>> MarkNotificationAsReadAsync(string notificationId, string userId);
        Task<ApiResponse<NotificationDto>> SendNotificationAsync(CreateNotificationDto notification);
        Task<ApiResponse<int>> GetUnreadNotificationCountAsync(string userId);
        Task<ApiResponse<bool>> DeleteNotificationAsync(string notificationId, string userId);
        Task<ApiResponse<int>> DeleteAllNotificationsAsync(string userId);
    }
}

