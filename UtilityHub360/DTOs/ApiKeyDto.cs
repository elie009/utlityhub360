using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ApiKeyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Key { get; set; } // Only shown on creation
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class CreateApiKeyDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }
    }
}

