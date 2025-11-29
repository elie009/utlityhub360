using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class ReceivablePayment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string ReceivableId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? BankAccountId { get; set; } // Bank account where payment was received

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = string.Empty; // BANK_TRANSFER, CASH, CHECK, DIGITAL_WALLET, etc.

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty; // Payment reference number

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "COMPLETED"; // COMPLETED, PENDING, FAILED

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ReceivableId")]
        public virtual Receivable Receivable { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("BankAccountId")]
        public virtual BankAccount? BankAccount { get; set; }
    }
}

