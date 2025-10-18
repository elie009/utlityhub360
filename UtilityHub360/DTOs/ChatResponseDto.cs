namespace UtilityHub360.DTOs
{
    public class ChatResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public List<string> SuggestedActions { get; set; } = new List<string>();
        public string? GeneratedReportPath { get; set; }
        public string? ReportFormat { get; set; }
        public int TokensUsed { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsReportGenerated { get; set; } = false;
    }

    public class ChatMessageResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int TokensUsed { get; set; }
        public string? Metadata { get; set; }
    }
}

