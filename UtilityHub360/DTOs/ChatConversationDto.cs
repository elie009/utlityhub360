namespace UtilityHub360.DTOs
{
    public class ChatConversationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public bool IsActive { get; set; }
        public int TotalMessages { get; set; }
        public int TotalTokensUsed { get; set; }
        public List<ChatMessageResponseDto> Messages { get; set; } = new List<ChatMessageResponseDto>();
    }

    public class CreateChatConversationDto
    {
        public string Title { get; set; } = "New Conversation";
    }

    public class UpdateChatConversationDto
    {
        public string Title { get; set; } = string.Empty;
    }
}
