using System;

namespace UtilityHub360.DTOs
{
    public class RepaymentScheduleDto
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }
}

