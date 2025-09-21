using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class LoanApplicationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Range(1000, 100000)]
        public decimal Principal { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Purpose { get; set; } = string.Empty;
        
        [Required]
        public int Term { get; set; } // months
        
        [Required]
        [Range(1000, double.MaxValue)]
        public decimal MonthlyIncome { get; set; }
        
        [Required]
        public string EmploymentStatus { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? RejectionReason { get; set; }
    }
}

