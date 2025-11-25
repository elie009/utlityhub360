using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class Notification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // PAYMENT_DUE, PAYMENT_RECEIVED, LOAN_APPROVED, LOAN_REJECTED, GENERAL

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Enhanced fields
        // NOTE: These columns exist in the database
        // Using explicit [Column] mapping to ensure EF recognizes them
        [Column("Channel")]
        [StringLength(50)]
        public string? Channel { get; set; } // IN_APP, EMAIL, SMS, PUSH

        [Column("Priority")]
        [StringLength(50)]
        public string? Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH, URGENT

        [Column("ScheduledFor")]
        public DateTime? ScheduledFor { get; set; } // For scheduled notifications

        [Column("TemplateId")]
        [StringLength(450)]
        public string? TemplateId { get; set; } // Reference to NotificationTemplate

        [Column("TemplateVariables", TypeName = "nvarchar(max)")]
        public string? TemplateVariables { get; set; } // JSON of variables used in template

        [Column("Status")]
        [StringLength(50)]
        public string? Status { get; set; } = "PENDING"; // PENDING, SENT, DELIVERED, FAILED

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TemplateId")]
        public virtual NotificationTemplate? Template { get; set; }
    }
}

