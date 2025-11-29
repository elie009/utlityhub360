using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Entity for transaction automation rules
    /// </summary>
    public class TransactionRule
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0; // Higher priority rules are applied first

        // Rule conditions
        [Required]
        [StringLength(50)]
        public string ConditionField { get; set; } = string.Empty; // description, amount, merchant, etc.

        [Required]
        [StringLength(50)]
        public string ConditionOperator { get; set; } = string.Empty; // contains, equals, greater_than, etc.

        [Required]
        [StringLength(500)]
        public string ConditionValue { get; set; } = string.Empty;

        public bool ConditionCaseSensitive { get; set; } = false;

        // Rule actions
        [StringLength(100)]
        public string? AutoCategory { get; set; }

        [StringLength(500)]
        public string? AutoTags { get; set; } // Comma-separated tags

        public bool AutoApprove { get; set; } = false;

        [StringLength(500)]
        public string? AutoDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

