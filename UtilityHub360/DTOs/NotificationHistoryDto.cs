namespace UtilityHub360.DTOs
{
    // ==========================================
    // NOTIFICATION HISTORY DTOs
    // ==========================================

    public class NotificationHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? NotificationId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public string? Recipient { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? Provider { get; set; }
        public string? ExternalId { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationHistoryQueryDto
    {
        public string? UserId { get; set; }
        public string? NotificationType { get; set; }
        public string? Channel { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

