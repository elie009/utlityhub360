using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class SavingsAccount
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SavingsType { get; set; } = string.Empty; // EMERGENCY, VACATION, INVESTMENT, etc.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0;

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Goal { get; set; } // "6 months emergency fund", "Summer vacation", etc.

        public DateTime TargetDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        [StringLength(450)]
        public string? DeletedBy { get; set; }
        
        [StringLength(500)]
        public string? DeleteReason { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<SavingsTransaction> SavingsTransactions { get; set; } = new List<SavingsTransaction>();
    }
}
