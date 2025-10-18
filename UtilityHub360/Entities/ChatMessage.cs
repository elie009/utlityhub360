using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class ChatMessage
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string ConversationId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // user, assistant, system

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int TokensUsed { get; set; } = 0;

        public string? Metadata { get; set; } // JSON for additional data like report paths, suggested actions

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ChatConversation Conversation { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}

