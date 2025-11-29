using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// History of sent notifications across all channels
    /// </summary>
    public class NotificationHistory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? NotificationId { get; set; } // Reference to Notification entity if applicable

        [StringLength(50)]
        [Required]
        public string NotificationType { get; set; } = string.Empty;

        [StringLength(50)]
        [Required]
        public string Channel { get; set; } = "IN_APP"; // IN_APP, EMAIL, SMS, PUSH

        [StringLength(200)]
        public string? Subject { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Message { get; set; }

        [StringLength(255)]
        public string? Recipient { get; set; } // Email address, phone number, or device token

        [StringLength(50)]
        public string Status { get; set; } = "PENDING"; // PENDING, SENT, FAILED, DELIVERED

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        [StringLength(100)]
        public string? Provider { get; set; } // Email provider, SMS provider, etc.

        [StringLength(450)]
        public string? ExternalId { get; set; } // External service message ID

        public DateTime? SentAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Metadata { get; set; } // Additional JSON metadata

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("NotificationId")]
        public virtual Notification? Notification { get; set; }
    }
}

