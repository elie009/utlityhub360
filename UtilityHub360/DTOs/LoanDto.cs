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
        
        // Next payment due date (from RepaymentSchedule)
        public DateTime? NextDueDate { get; set; }

        // New fields for enhanced loan management
        public string LoanType { get; set; } = "PERSONAL"; // PERSONAL, MORTGAGE, AUTO, STUDENT, BUSINESS, etc.
        public string? RefinancedFromLoanId { get; set; }
        public string? RefinancedToLoanId { get; set; }
        public DateTime? RefinancingDate { get; set; }
        public decimal? EffectiveInterestRate { get; set; }
        public decimal? TotalInterest { get; set; }
        public decimal? DownPayment { get; set; }
        public decimal? ProcessingFee { get; set; }
        public decimal? ActualFinancedAmount { get; set; }
        public string? InterestComputationMethod { get; set; }
        public string? PaymentFrequency { get; set; }
        public DateTime? StartDate { get; set; }
    }

    public class CreateLoanDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Loan name cannot exceed 100 characters")]
        public string LoanName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Loan type cannot exceed 50 characters")]
        public string LoanType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly payment must be greater than 0")]
        public decimal MonthlyPayment { get; set; }

        [Required]
        [Range(0.01, 100, ErrorMessage = "Interest rate must be between 0.01 and 100")]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Legacy properties for backward compatibility
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal Principal { get; set; }

        [Range(0, 100)]
        public decimal InterestRateLegacy { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Term must be at least 1 month")]
        public int Term { get; set; }

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

