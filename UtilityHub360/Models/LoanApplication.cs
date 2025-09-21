using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a loan application submitted by a user
    /// </summary>
    public class LoanApplication
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
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
        public EmploymentStatus EmploymentStatus { get; set; }
        
        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }
        
        [Required]
        public LoanApplicationStatus Status { get; set; } = LoanApplicationStatus.PENDING;
        
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReviewedAt { get; set; }
        
        [StringLength(100)]
        public string? ReviewedBy { get; set; }
        
        [StringLength(500)]
        public string? RejectionReason { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
    }
    
    public enum EmploymentStatus
    {
        employed,
        self_employed,
        unemployed,
        retired,
        student
    }
    
    public enum LoanApplicationStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }
}

