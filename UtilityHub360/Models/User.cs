using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; } = UserRole.USER;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    }
    
    public enum UserRole
    {
        USER,
        ADMIN
    }
}

