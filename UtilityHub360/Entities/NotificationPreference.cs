using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// User notification preferences for different channels and notification types
    /// </summary>
    public class NotificationPreference
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(50)]
        [Required]
        public string NotificationType { get; set; } = string.Empty; // PAYMENT_DUE, PAYMENT_RECEIVED, LOAN_APPROVED, etc.

        // Channel preferences
        public bool InAppEnabled { get; set; } = true;
        public bool EmailEnabled { get; set; } = false;
        public bool SmsEnabled { get; set; } = false;
        public bool PushEnabled { get; set; } = false;

        // Scheduling preferences
        public bool ScheduledEnabled { get; set; } = false;
        public string? ScheduleTime { get; set; } // HH:mm format (e.g., "09:00")
        public string? ScheduleDays { get; set; } // JSON array of days (e.g., ["MONDAY", "WEDNESDAY", "FRIDAY"])

        // Quiet hours
        public bool QuietHoursEnabled { get; set; } = false;
        public string? QuietHoursStart { get; set; } // HH:mm format
        public string? QuietHoursEnd { get; set; } // HH:mm format

        // Frequency control
        public int? MaxNotificationsPerDay { get; set; } // Limit notifications per day for this type
        public int? MinMinutesBetweenNotifications { get; set; } // Minimum time between same type notifications

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}

