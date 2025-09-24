using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class UpdateLoanDto
    {
        [StringLength(500)]
        public string? Purpose { get; set; }

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [Range(0, 100)]
        public decimal? InterestRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MonthlyPayment { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? RemainingBalance { get; set; }
    }
}
