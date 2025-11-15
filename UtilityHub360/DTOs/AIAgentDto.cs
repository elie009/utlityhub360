using UtilityHub360.Models;

namespace UtilityHub360.DTOs
{
    public class AIAgentRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public bool EnableActions { get; set; } = true;
    }

    public class AIAgentResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public List<AgentActionDto> ActionsPerformed { get; set; } = new();
        public string? ConversationId { get; set; }
    }

    public class AgentActionDto
    {
        public string FunctionName { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string Result { get; set; } = string.Empty;
        public bool Success { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}

