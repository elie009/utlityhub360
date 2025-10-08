using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// DTO for calculating loan preview without saving
    /// </summary>
    public class CalculateLoanPreviewDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal Principal { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Term must be at least 1 month")]
        public int Term { get; set; }
    }

    /// <summary>
    /// DTO for recalculating loan values with new interest rate
    /// </summary>
    public class RecalculateLoanPreviewDto
    {
        [Required]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal NewInterestRate { get; set; }
    }
}

