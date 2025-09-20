using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new payment
    /// </summary>
    public class CreatePaymentDto
    {
        [Required]
        public int LoanId { get; set; }

        public int? ScheduleId { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount paid must be greater than 0")]
        public decimal AmountPaid { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(255)]
        public string Notes { get; set; } = string.Empty;
    }
}
