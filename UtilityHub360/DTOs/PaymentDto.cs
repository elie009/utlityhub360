using System;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for Payment
    /// </summary>
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int LoanId { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
    }
}
