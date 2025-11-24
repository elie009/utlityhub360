using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // ==========================================
    // NOTIFICATION PREFERENCE DTOs
    // ==========================================

    public class NotificationPreferenceDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public bool InAppEnabled { get; set; } = true;
        public bool EmailEnabled { get; set; } = false;
        public bool SmsEnabled { get; set; } = false;
        public bool PushEnabled { get; set; } = false;
        public bool ScheduledEnabled { get; set; } = false;
        public string? ScheduleTime { get; set; }
        public List<string>? ScheduleDays { get; set; }
        public bool QuietHoursEnabled { get; set; } = false;
        public string? QuietHoursStart { get; set; }
        public string? QuietHoursEnd { get; set; }
        public int? MaxNotificationsPerDay { get; set; }
        public int? MinMinutesBetweenNotifications { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateNotificationPreferenceDto
    {
        [Required]
        public string NotificationType { get; set; } = string.Empty;

        public bool InAppEnabled { get; set; } = true;
        public bool EmailEnabled { get; set; } = false;
        public bool SmsEnabled { get; set; } = false;
        public bool PushEnabled { get; set; } = false;
        public bool ScheduledEnabled { get; set; } = false;
        public string? ScheduleTime { get; set; }
        public List<string>? ScheduleDays { get; set; }
        public bool QuietHoursEnabled { get; set; } = false;
        public string? QuietHoursStart { get; set; }
        public string? QuietHoursEnd { get; set; }
        public int? MaxNotificationsPerDay { get; set; }
        public int? MinMinutesBetweenNotifications { get; set; }
    }

    public class UpdateNotificationPreferenceDto
    {
        public bool? InAppEnabled { get; set; }
        public bool? EmailEnabled { get; set; }
        public bool? SmsEnabled { get; set; }
        public bool? PushEnabled { get; set; }
        public bool? ScheduledEnabled { get; set; }
        public string? ScheduleTime { get; set; }
        public List<string>? ScheduleDays { get; set; }
        public bool? QuietHoursEnabled { get; set; }
        public string? QuietHoursStart { get; set; }
        public string? QuietHoursEnd { get; set; }
        public int? MaxNotificationsPerDay { get; set; }
        public int? MinMinutesBetweenNotifications { get; set; }
    }
}

