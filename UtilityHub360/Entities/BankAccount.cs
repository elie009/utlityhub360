using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class BankAccount
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; } = string.Empty; // Bank, Wallet, Credit Card, Cash, Investment

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? FinancialInstitution { get; set; } // Chase, BPI, PayPal, etc.

        [StringLength(255)]
        public string? AccountNumber { get; set; } // Masked for security

        [StringLength(100)]
        public string? RoutingNumber { get; set; } // For bank accounts

        [StringLength(50)]
        public string? SyncFrequency { get; set; } = "MANUAL"; // daily, weekly, monthly, manual

        public bool IsConnected { get; set; } = false; // Connected via API/Bank Integration

        [StringLength(500)]
        public string? ConnectionId { get; set; } // External system connection ID

        public DateTime? LastSyncedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? Iban { get; set; } // For international accounts

        [StringLength(100)]
        public string? SwiftCode { get; set; } // For international transfers

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    }
}
