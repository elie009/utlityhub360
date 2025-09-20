using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a repayment schedule entry for a loan
    /// </summary>
    public class RepaymentSchedule
    {
        public int ScheduleId { get; set; }

        public int LoanId { get; set; }

        public DateTime DueDate { get; set; }

        public decimal AmountDue { get; set; }

        public decimal? PrincipalPortion { get; set; }

        public decimal? InterestPortion { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidDate { get; set; }

        // Navigation properties
        public Loan Loan { get; set; } = null!;
        public ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();
        public ICollection<LoanPenalty> Penalties { get; set; } = new List<LoanPenalty>();

        // Computed properties
        public bool IsOverdue => !IsPaid && DueDate < DateTime.UtcNow.Date;
        public int DaysOverdue => IsOverdue ? (DateTime.UtcNow.Date - DueDate).Days : 0;
    }
}
