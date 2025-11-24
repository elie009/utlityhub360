using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ScheduledPaymentDto
    {
        public string BillId { get; set; } = string.Empty;
        public string BillName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsScheduled { get; set; }
        public string? BankAccountId { get; set; }
        public string? BankAccountName { get; set; }
        public int? DaysBeforeDue { get; set; }
        public DateTime? ScheduledPaymentDate { get; set; }
        public DateTime? LastAttempt { get; set; }
        public string? FailureReason { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ConfigureScheduledPaymentDto
    {
        [Required]
        public string BillId { get; set; } = string.Empty;
        
        [Required]
        public bool IsScheduledPayment { get; set; }
        
        [StringLength(450)]
        public string? ScheduledPaymentBankAccountId { get; set; }
        
        [Range(0, 30)]
        public int? ScheduledPaymentDaysBeforeDue { get; set; } // 0-30 days before due date
    }

    public class BillApprovalDto
    {
        [Required]
        public string BillId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string ApprovalStatus { get; set; } = string.Empty; // APPROVED, REJECTED
        
        [StringLength(500)]
        public string? ApprovalNotes { get; set; }
    }

    public class BillPaymentHistoryReportDto
    {
        public DateTime ReportDate { get; set; }
        public string Period { get; set; } = string.Empty; // month, quarter, year
        public int TotalPayments { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public int ScheduledPayments { get; set; }
        public int ManualPayments { get; set; }
        public List<BillPaymentSummaryDto> Payments { get; set; } = new List<BillPaymentSummaryDto>();
    }

    public class BillPaymentSummaryDto
    {
        public string BillId { get; set; } = string.Empty;
        public string BillName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public bool WasScheduled { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

