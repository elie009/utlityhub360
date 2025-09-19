using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a payment made against a loan
    /// </summary>
    [Table("LnPayments")]
    public class LoanPayment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int LoanId { get; set; }

        public int? ScheduleId { get; set; } // Optional - some payments may not map to schedule

        [Required]
        public DateTime PaymentDate { get; set; }

        public LoanPayment()
        {
            PaymentDate = DateTime.UtcNow;
        }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } // Cash, Bank Transfer, Online

        [StringLength(255)]
        public string Notes { get; set; }

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual RepaymentSchedule RepaymentSchedule { get; set; }
    }
}
