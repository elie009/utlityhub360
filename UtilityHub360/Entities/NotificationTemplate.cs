using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Notification templates for reusable notification content
    /// </summary>
    public class NotificationTemplate
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [StringLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        [Required]
        public string NotificationType { get; set; } = string.Empty; // PAYMENT_DUE, PAYMENT_RECEIVED, etc.

        [StringLength(50)]
        [Required]
        public string Channel { get; set; } = "IN_APP"; // IN_APP, EMAIL, SMS, PUSH

        [StringLength(200)]
        [Required]
        public string Subject { get; set; } = string.Empty; // For email/SMS subject or push title

        [Column(TypeName = "nvarchar(max)")]
        [Required]
        public string Body { get; set; } = string.Empty; // Template body with placeholders like {{UserName}}, {{Amount}}, etc.

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystemTemplate { get; set; } = false; // System templates cannot be deleted

        [StringLength(450)]
        public string? CreatedBy { get; set; } // User ID who created (null for system templates)

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Template variables documentation (JSON)
        [Column(TypeName = "nvarchar(max)")]
        public string? Variables { get; set; } // JSON array of available variables
    }
}

