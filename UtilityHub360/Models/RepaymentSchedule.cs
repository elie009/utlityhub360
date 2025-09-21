using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a repayment schedule entry for a loan
    /// </summary>
    public class RepaymentSchedule
    {
        public int Id { get; set; }
        
        public int LoanId { get; set; }
        
        public int InstallmentNumber { get; set; }
        
        public DateTime DueDate { get; set; }
        
        public decimal PrincipalAmount { get; set; }
        
        public decimal InterestAmount { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public RepaymentStatus Status { get; set; } = RepaymentStatus.PENDING;
        
        public DateTime? PaidAt { get; set; }
        
        // Navigation properties
        public Loan Loan { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
    
    public enum RepaymentStatus
    {
        PENDING,
        PAID,
        OVERDUE
    }
}
