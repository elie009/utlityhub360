using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new loan
    /// </summary>
    public class CreateLoanDto
    {
        [Required]
        public int BorrowerId { get; set; }

        [StringLength(50)]
        public string LoanType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Term months must be greater than 0")]
        public int TermMonths { get; set; }

        [StringLength(20)]
        public string RepaymentFrequency { get; set; }

        [StringLength(20)]
        public string AmortizationType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }
}
