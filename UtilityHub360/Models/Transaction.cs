using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a transaction in the loan management system
    /// </summary>
    public class Transaction
    {
        public int Id { get; set; }
        
        public int LoanId { get; set; }
        
        [Required]
        public TransactionType Type { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Reference { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Loan Loan { get; set; } = null!;
    }
    
    public enum TransactionType
    {
        DISBURSEMENT,
        PAYMENT,
        INTEREST,
        PENALTY
    }
}

