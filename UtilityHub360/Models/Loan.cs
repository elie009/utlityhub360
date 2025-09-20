using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a loan in the loan management system
    /// </summary>
    public class Loan
    {
        public int LoanId { get; set; }

        public int BorrowerId { get; set; }

        [StringLength(50)]
        public string? LoanType { get; set; } // Personal, Business, Mortgage, etc.

        public decimal PrincipalAmount { get; set; }

        public decimal InterestRate { get; set; } // Percentage

        public int TermMonths { get; set; }

        [StringLength(20)]
        public string? RepaymentFrequency { get; set; } // Daily, Weekly, Monthly

        [StringLength(20)]
        public string? AmortizationType { get; set; } // Flat, Reducing, Compound

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Closed, Defaulted

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Borrower Borrower { get; set; } = null!;
        public ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();
        public ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();
        public ICollection<LoanPenalty> Penalties { get; set; } = new List<LoanPenalty>();

        // Computed properties
        public decimal OutstandingBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public int DaysOverdue { get; set; }
    }
}