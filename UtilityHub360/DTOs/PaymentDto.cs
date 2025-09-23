using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class PaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required]
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
    }
}
