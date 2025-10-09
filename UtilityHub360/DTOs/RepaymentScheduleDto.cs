using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class RepaymentScheduleDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }

    public class UpdateRepaymentScheduleDto
    {
        [Required]
        public DateTime NewDueDate { get; set; }
    }
}

