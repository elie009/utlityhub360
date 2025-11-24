using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class LoanApplicationDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Principal { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int Term { get; set; }
        public decimal MonthlyIncome { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class CreateLoanApplicationDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal Principal { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal InterestRate { get; set; }

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Term must be at least 1 month")]
        public int Term { get; set; }

        [Required]
        [Range(1000, double.MaxValue)]
        public decimal MonthlyIncome { get; set; }

        [Required]
        [StringLength(20)]
        public string EmploymentStatus { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        // New fields for loan type and refinancing
        [StringLength(50)]
        public string LoanType { get; set; } = "PERSONAL"; // PERSONAL, MORTGAGE, AUTO, STUDENT, BUSINESS, etc.

        [StringLength(450)]
        public string? RefinancedFromLoanId { get; set; } // If this is a refinancing application

        public decimal? DownPayment { get; set; }
        public decimal? ProcessingFee { get; set; }
        public string? InterestComputationMethod { get; set; } // FLAT_RATE, AMORTIZED
    }

    public class ReviewLoanApplicationDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string? RejectionReason { get; set; }
    }
}

