using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a payment made against a loan
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }
        
        public int LoanId { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public PaymentMethod Method { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Reference { get; set; } = string.Empty;
        
        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
        
        public DateTime ProcessedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Loan Loan { get; set; } = null!;
        public User User { get; set; } = null!;
    }
    
    public enum PaymentMethod
    {
        BANK_TRANSFER,
        CARD,
        WALLET,
        CASH
    }
    
    public enum PaymentStatus
    {
        PENDING,
        COMPLETED,
        FAILED
    }
}