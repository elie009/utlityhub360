using System;

namespace UtilityHub360.DTOs
{
    public class LoanReportDto
    {
        public int LoanId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal MonthlyPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public int PaymentsMade { get; set; }
        public int PaymentsRemaining { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}

