namespace UtilityHub360.Models
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4-turbo-preview";
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public string SystemPrompt { get; set; } = string.Empty;
        public int MaxConversationHistory { get; set; } = 10;
        public int RateLimitPerMinute { get; set; } = 10;
    }
}

