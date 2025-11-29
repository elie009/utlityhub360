using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ReceivablePaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public string ReceivableId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? BankAccountId { get; set; }
        public string? BankAccountName { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string BorrowerName { get; set; } = string.Empty; // From Receivable
    }

    public class CreateReceivablePaymentDto
    {
        [Required]
        [StringLength(450)]
        public string ReceivableId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? BankAccountId { get; set; } // Optional - bank account where payment was received

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = string.Empty; // BANK_TRANSFER, CASH, CHECK, DIGITAL_WALLET

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? PaymentDate { get; set; } // Optional - defaults to now
    }
}

