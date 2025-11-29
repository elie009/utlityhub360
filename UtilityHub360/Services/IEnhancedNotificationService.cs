using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IEnhancedNotificationService
    {
        // Notification Preferences
        Task<ApiResponse<List<NotificationPreferenceDto>>> GetUserPreferencesAsync(string userId);
        Task<ApiResponse<NotificationPreferenceDto>> GetPreferenceAsync(string userId, string notificationType);
        Task<ApiResponse<NotificationPreferenceDto>> CreatePreferenceAsync(string userId, CreateNotificationPreferenceDto preference);
        Task<ApiResponse<NotificationPreferenceDto>> UpdatePreferenceAsync(string userId, string notificationType, UpdateNotificationPreferenceDto preference);
        Task<ApiResponse<bool>> DeletePreferenceAsync(string userId, string notificationType);

        // Notification Templates
        Task<ApiResponse<List<NotificationTemplateDto>>> GetTemplatesAsync(string? notificationType = null, string? channel = null);
        Task<ApiResponse<NotificationTemplateDto>> GetTemplateAsync(string templateId);
        Task<ApiResponse<NotificationTemplateDto>> CreateTemplateAsync(CreateNotificationTemplateDto template);
        Task<ApiResponse<NotificationTemplateDto>> UpdateTemplateAsync(string templateId, UpdateNotificationTemplateDto template);
        Task<ApiResponse<bool>> DeleteTemplateAsync(string templateId);
        Task<ApiResponse<string>> RenderTemplateAsync(string templateId, Dictionary<string, string> variables);

        // Enhanced Notification Sending
        Task<ApiResponse<NotificationDto>> SendNotificationAsync(SendNotificationRequestDto request);
        Task<ApiResponse<List<NotificationDto>>> SendBulkNotificationsAsync(List<SendNotificationRequestDto> requests);

        // Notification History
        Task<ApiResponse<PaginatedResponse<NotificationHistoryDto>>> GetNotificationHistoryAsync(string userId, NotificationHistoryQueryDto query);
        Task<ApiResponse<NotificationHistoryDto>> GetHistoryItemAsync(string historyId);

        // Scheduled Notifications
        Task<ApiResponse<List<NotificationDto>>> GetScheduledNotificationsAsync(string userId);
        Task<ApiResponse<bool>> CancelScheduledNotificationAsync(string notificationId);
    }
}

