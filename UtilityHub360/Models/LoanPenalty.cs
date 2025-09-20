using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a penalty applied to a loan for overdue payments
    /// </summary>
    public class LoanPenalty
    {
        public int PenaltyId { get; set; }

        public int LoanId { get; set; }

        public int ScheduleId { get; set; }

        public decimal Amount { get; set; }

        public DateTime AppliedDate { get; set; }

        public bool IsPaid { get; set; }

        // Navigation properties
        public Loan Loan { get; set; } = null!;
        public RepaymentSchedule RepaymentSchedule { get; set; } = null!;
    }
}