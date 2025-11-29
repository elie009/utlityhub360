namespace UtilityHub360.DTOs
{
    public class TicketDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }
        public List<TicketCommentDto>? Comments { get; set; }
        public List<TicketAttachmentDto>? Attachments { get; set; }
        public List<TicketStatusHistoryDto>? StatusHistory { get; set; }
    }

    public class CreateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH, URGENT
        public string Category { get; set; } = "GENERAL"; // BUG, FEATURE_REQUEST, SUPPORT, TECHNICAL, BILLING, GENERAL
    }

    public class UpdateTicketDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; } // OPEN, IN_PROGRESS, RESOLVED, CLOSED
        public string? Priority { get; set; } // LOW, NORMAL, HIGH, URGENT
        public string? Category { get; set; }
        public string? AssignedTo { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class TicketCommentDto
    {
        public string Id { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTicketCommentDto
    {
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }

    public class TicketAttachmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
        public string? UploadedBy { get; set; }
        public string? UploadedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TicketStatusHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string? ChangedBy { get; set; }
        public string? ChangedByName { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class TicketFilters
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public string? AssignedTo { get; set; }
        public string? Search { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}

