using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ReceivableDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string? BorrowerContact { get; set; }
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal MonthlyPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public DateTime LentAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? AdditionalInfo { get; set; }
        public string PaymentFrequency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int PaymentCount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextPaymentDueDate { get; set; }
    }

    public class CreateReceivableDto
    {
        [Required]
        [StringLength(255, ErrorMessage = "Borrower name cannot exceed 255 characters")]
        public string BorrowerName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Borrower contact cannot exceed 500 characters")]
        public string? BorrowerContact { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal Principal { get; set; }

        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal InterestRate { get; set; } = 0;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Term must be at least 1 month")]
        public int Term { get; set; }

        [StringLength(500, ErrorMessage = "Purpose cannot exceed 500 characters")]
        public string? Purpose { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly payment must be greater than 0")]
        public decimal MonthlyPayment { get; set; }

        [StringLength(20)]
        public string PaymentFrequency { get; set; } = "MONTHLY"; // MONTHLY, WEEKLY, BIWEEKLY, QUARTERLY

        public DateTime? StartDate { get; set; }

        [StringLength(1000, ErrorMessage = "Additional info cannot exceed 1000 characters")]
        public string? AdditionalInfo { get; set; }
    }

    public class UpdateReceivableDto
    {
        [StringLength(255, ErrorMessage = "Borrower name cannot exceed 255 characters")]
        public string? BorrowerName { get; set; }

        [StringLength(500, ErrorMessage = "Borrower contact cannot exceed 500 characters")]
        public string? BorrowerContact { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal? Principal { get; set; }

        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal? InterestRate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Term must be at least 1 month")]
        public int? Term { get; set; }

        [StringLength(500, ErrorMessage = "Purpose cannot exceed 500 characters")]
        public string? Purpose { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly payment must be greater than 0")]
        public decimal? MonthlyPayment { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(20)]
        public string? PaymentFrequency { get; set; }

        [StringLength(1000, ErrorMessage = "Additional info cannot exceed 1000 characters")]
        public string? AdditionalInfo { get; set; }
    }
}

