using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class RepaymentScheduleDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }

    public class RepaymentScheduleCountDto
    {
        public int Count { get; set; }
        public int LoansAffected { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateRepaymentScheduleDto
    {
        [Required]
        public DateTime NewDueDate { get; set; }
    }

    public class ExtendLoanTermDto
    {
        [Required]
        [Range(1, 60, ErrorMessage = "Additional months must be between 1 and 60")]
        public int AdditionalMonths { get; set; }

        public string? Reason { get; set; }
    }

    public class AddPaymentScheduleDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Installment number must be greater than 0")]
        public int StartingInstallmentNumber { get; set; }

        [Required]
        [Range(1, 60, ErrorMessage = "Number of months must be between 1 and 60")]
        public int NumberOfMonths { get; set; }

        [Required]
        public DateTime FirstDueDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly payment must be greater than 0")]
        public decimal MonthlyPayment { get; set; }

        public string? Reason { get; set; }
    }

    public class AutoAddPaymentScheduleDto
    {
        [Range(1, 60, ErrorMessage = "Number of months must be between 1 and 60")]
        public int NumberOfMonths { get; set; } = 1;

        [Required]
        public DateTime FirstDueDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly payment must be greater than 0")]
        public decimal MonthlyPayment { get; set; }

        public string? Reason { get; set; }
    }

    public class RegenerateScheduleDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "New monthly payment must be greater than 0")]
        public decimal NewMonthlyPayment { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "New term must be greater than 0")]
        public int NewTerm { get; set; }

        public DateTime? StartDate { get; set; }

        public string? Reason { get; set; }
    }

    public class PaymentScheduleResponseDto
    {
        public List<RepaymentScheduleDto> Schedule { get; set; } = new List<RepaymentScheduleDto>();
        public int TotalInstallments { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? FirstDueDate { get; set; }
        public DateTime? LastDueDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class MarkInstallmentPaidDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Method { get; set; } = string.Empty; // CASH, BANK_TRANSFER, CREDIT_CARD, etc.

        [StringLength(100)]
        public string? Reference { get; set; }
        
        public DateTime? PaymentDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class SimpleScheduleUpdateDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
        
        [StringLength(20)]
        public string? Status { get; set; } // PENDING, PAID, OVERDUE
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? PaidDate { get; set; }
        
        [StringLength(50)]
        public string? PaymentMethod { get; set; } // For when marking as PAID
        
        [StringLength(100)]
        public string? PaymentReference { get; set; } // For when marking as PAID
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}

