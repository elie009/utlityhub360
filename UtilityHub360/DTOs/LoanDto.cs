using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class LoanDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal MonthlyPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? DisbursedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    public class CreateLoanDto
    {
        [Required]
        [Range(1000, 100000)]
        public decimal Principal { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(6, 60)]
        public int Term { get; set; }

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }
    }

    public class UpdateLoanStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }
    }
}

