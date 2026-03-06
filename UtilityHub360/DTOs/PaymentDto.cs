using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class PaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public string? LoanId { get; set; }
        public string? BankAccountId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsBankTransaction { get; set; }
        public string? TransactionType { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? ExternalTransactionId { get; set; }
        public string? Notes { get; set; }
        public string? Merchant { get; set; }
        public string? Location { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal? BalanceAfterTransaction { get; set; }
        public DateTime ProcessedAt { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        // LoanId is provided via route for loan-specific payment endpoint; keep optional for model binding
        [StringLength(450)]
        public string LoanId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Reference { get; set; } = string.Empty;

        [StringLength(450)]
        public string? BankAccountId { get; set; } // Optional: required only for Bank transfer method
    }

}

