using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a payment made against a loan
    /// </summary>
    public class LoanPayment
    {
        public int PaymentId { get; set; }

        public int LoanId { get; set; }

        public int? ScheduleId { get; set; } // Optional - some payments may not map to schedule

        public DateTime PaymentDate { get; set; }

        public decimal AmountPaid { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; } // Cash, Bank Transfer, Online

        [StringLength(255)]
        public string? Notes { get; set; }

        // Navigation properties
        public Loan Loan { get; set; } = null!;
        public RepaymentSchedule? RepaymentSchedule { get; set; }
    }
}