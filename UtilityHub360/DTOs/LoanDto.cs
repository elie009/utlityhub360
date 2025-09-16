using System;
using System.Collections.Generic;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for Loan
    /// </summary>
    public class LoanDto
    {
        public int LoanId { get; set; }
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public string LoanType { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public string RepaymentFrequency { get; set; }
        public string AmortizationType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public int DaysOverdue { get; set; }
    }
}