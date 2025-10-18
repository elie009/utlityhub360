using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ChatMessageDto
    {
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        public string? ConversationId { get; set; }

        public bool IncludeTransactionContext { get; set; } = true;

        public string? ReportFormat { get; set; } // "pdf" or "excel" for report generation requests
    }

    public class CreateChatMessageDto
    {
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        public string? ConversationId { get; set; }

        public bool IncludeTransactionContext { get; set; } = true;
    }
}

