using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a loan in the loan management system
    /// </summary>
    public class Loan
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [Range(1000, 100000)]
        public decimal Principal { get; set; }
        
        [Required]
        [Range(0.01, 100)]
        public decimal InterestRate { get; set; } // Annual percentage
        
        [Required]
        public int Term { get; set; } // months
        
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Purpose { get; set; } = string.Empty;
        
        [Required]
        public LoanStatus Status { get; set; } = LoanStatus.PENDING;
        
        public decimal MonthlyPayment { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public decimal RemainingBalance { get; set; }
        
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ApprovedAt { get; set; }
        
        public DateTime? DisbursedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
    
    public enum LoanStatus
    {
        PENDING,
        APPROVED,
        REJECTED,
        ACTIVE,
        COMPLETED,
        DEFAULTED
    }
}