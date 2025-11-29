using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class ClosedMonth
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; } // 1-12 (January = 1, December = 12)

        [Required]
        [StringLength(450)]
        public string ClosedBy { get; set; } = string.Empty; // User ID

        [Required]
        public DateTime ClosedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; } = null!;

        [ForeignKey("ClosedBy")]
        public virtual User User { get; set; } = null!;
    }
}

