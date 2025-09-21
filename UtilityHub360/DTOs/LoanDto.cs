using System;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for Loan
    /// </summary>
    public class LoanDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal MonthlyPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? DisbursedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}
