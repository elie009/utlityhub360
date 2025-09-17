using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a loan in the loan management system
    /// </summary>
    [Table("LnLoans")]
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required]
        public int BorrowerId { get; set; }

        [StringLength(50)]
        public string LoanType { get; set; } // Personal, Business, Mortgage, etc.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; } // Percentage

        [Required]
        public int TermMonths { get; set; }

        [StringLength(20)]
        public string RepaymentFrequency { get; set; } // Daily, Weekly, Monthly

        [StringLength(20)]
        public string AmortizationType { get; set; } // Flat, Reducing, Compound

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Closed, Defaulted

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BorrowerId")]
        public virtual Borrower Borrower { get; set; }

        public virtual ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();
        public virtual ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();
        public virtual ICollection<LoanPenalty> Penalties { get; set; } = new List<LoanPenalty>();

        // Computed properties
        [NotMapped]
        public decimal OutstandingBalance { get; set; }

        [NotMapped]
        public decimal TotalPaid { get; set; }

        [NotMapped]
        public int DaysOverdue { get; set; }
    }
}
