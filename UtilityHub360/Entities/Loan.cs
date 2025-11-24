using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class Loan
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Principal { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; }

        [Required]
        public int Term { get; set; } // months

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, ACTIVE, COMPLETED, DEFAULTED

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPayment { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBalance { get; set; }

        [Required]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? DisbursedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        // Accounting fields
        [StringLength(20)]
        public string InterestComputationMethod { get; set; } = "AMORTIZED"; // FLAT_RATE, AMORTIZED

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalInterest { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DownPayment { get; set; } = 0; // Optional down payment that reduces principal

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProcessingFee { get; set; } = 0; // Optional processing fee

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualFinancedAmount { get; set; } = 0; // Principal - DownPayment (used for interest calculation)

        [StringLength(20)]
        public string PaymentFrequency { get; set; } = "MONTHLY"; // MONTHLY, WEEKLY, BIWEEKLY, QUARTERLY

        public DateTime? StartDate { get; set; } // Loan start date for interest calculation

        // Loan type field
        [StringLength(50)]
        public string LoanType { get; set; } = "PERSONAL"; // PERSONAL, MORTGAGE, AUTO, STUDENT, BUSINESS, CREDIT_CARD, LINE_OF_CREDIT, OTHER

        // Refinancing tracking fields
        [StringLength(450)]
        public string? RefinancedFromLoanId { get; set; } // ID of the loan that was refinanced (if this is a refinanced loan)

        [StringLength(450)]
        public string? RefinancedToLoanId { get; set; } // ID of the loan that refinanced this loan (if this loan was refinanced)

        public DateTime? RefinancingDate { get; set; } // Date when refinancing occurred

        [Column(TypeName = "decimal(5,2)")]
        public decimal? EffectiveInterestRate { get; set; } // Calculated effective interest rate (APR) including fees

        // Due date information - TEMPORARILY COMMENTED OUT
        // public DateTime? NextDueDate { get; set; }
        
        // public DateTime? FinalDueDate { get; set; }


        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Navigation properties for refinancing
        [ForeignKey("RefinancedFromLoanId")]
        public virtual Loan? RefinancedFromLoan { get; set; }

        [ForeignKey("RefinancedToLoanId")]
        public virtual Loan? RefinancedToLoan { get; set; }

        public virtual ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }
}
