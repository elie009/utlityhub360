using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class TransactionCategory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Type { get; set; } = "EXPENSE"; // EXPENSE, INCOME, TRANSFER, BILL, SAVINGS, LOAN

        [StringLength(50)]
        public string? Icon { get; set; } // Icon name for UI

        [StringLength(20)]
        public string? Color { get; set; } // Hex color code

        public bool IsSystemCategory { get; set; } = false; // System categories cannot be deleted

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0; // For sorting in UI

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

