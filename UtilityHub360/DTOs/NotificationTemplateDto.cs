using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // ==========================================
    // NOTIFICATION TEMPLATE DTOs
    // ==========================================

    public class NotificationTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsSystemTemplate { get; set; } = false;
        public string? CreatedBy { get; set; }
        public List<string>? Variables { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateNotificationTemplateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Channel { get; set; } = "IN_APP";

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public List<string>? Variables { get; set; }
    }

    public class UpdateNotificationTemplateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(200)]
        public string? Subject { get; set; }

        public string? Body { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public List<string>? Variables { get; set; }
    }

    public class RenderNotificationTemplateDto
    {
        [Required]
        public string TemplateId { get; set; } = string.Empty;

        [Required]
        public Dictionary<string, string> Variables { get; set; } = new();
    }
}

