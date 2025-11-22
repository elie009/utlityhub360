using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class Card
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CardName { get; set; } = string.Empty; // e.g., "Primary Debit Card", "Spouse's Card"

        [Required]
        [StringLength(50)]
        public string CardType { get; set; } = string.Empty; // DEBIT, CREDIT, ATM, PREPAID

        [StringLength(50)]
        public string? CardBrand { get; set; } // VISA, MASTERCARD, AMEX, etc.

        [StringLength(20)]
        public string? Last4Digits { get; set; } // Last 4 digits of card number (masked for security)

        [StringLength(100)]
        public string? CardholderName { get; set; } // Name on the card

        [StringLength(10)]
        public string? ExpiryMonth { get; set; } // MM format

        [StringLength(10)]
        public string? ExpiryYear { get; set; } // YYYY format

        [StringLength(10)]
        public string? Cvv { get; set; } // Should be encrypted in production

        public bool IsPrimary { get; set; } = false; // Primary card for the account

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

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
        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

