using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a penalty applied to a loan for overdue payments
    /// </summary>
    [Table("LnPenalties")]
    public class LoanPenalty
    {
        [Key]
        public int PenaltyId { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime AppliedDate { get; set; }

        public bool IsPaid { get; set; }

        public LoanPenalty()
        {
            AppliedDate = DateTime.UtcNow;
            IsPaid = false;
        }

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual RepaymentSchedule RepaymentSchedule { get; set; }
    }
}
