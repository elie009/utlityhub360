using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a borrower in the loan management system
    /// </summary>
    [Table("LnBorrowers")]
    public class Borrower
    {
        [Key]
        public int BorrowerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(50)]
        public string GovernmentId { get; set; }

        public int? CreditScore { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Blacklisted

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<LoanNotification> Notifications { get; set; } = new List<LoanNotification>();

        // Computed properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
