using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class PaymentDto
    {
        [Required]
        public int LoanId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public string Method { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Reference { get; set; } = string.Empty;
    }
}