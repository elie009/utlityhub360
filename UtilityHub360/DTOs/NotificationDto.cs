namespace UtilityHub360.DTOs
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? Channel { get; set; }
        public string? Priority { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public string? TemplateId { get; set; }
        public Dictionary<string, string>? TemplateVariables { get; set; }
        public string? Status { get; set; }
    }

    public class CreateNotificationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Channel { get; set; } = "IN_APP";
        public string? Priority { get; set; } = "NORMAL";
        public DateTime? ScheduledFor { get; set; }
        public string? TemplateId { get; set; }
        public Dictionary<string, string>? TemplateVariables { get; set; }
    }

    public class SendNotificationRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public List<string> Channels { get; set; } = new() { "IN_APP" }; // IN_APP, EMAIL, SMS, PUSH
        public string? TemplateId { get; set; }
        public Dictionary<string, string>? TemplateVariables { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Priority { get; set; } = "NORMAL";
        public DateTime? ScheduledFor { get; set; }
    }

    public class MarkNotificationReadDto
    {
        public bool IsRead { get; set; } = true;
    }
}

