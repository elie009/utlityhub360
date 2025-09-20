using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a borrower in the loan management system
    /// </summary>
    public class Borrower
    {
        public int BorrowerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? GovernmentId { get; set; }

        public int? CreditScore { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Blacklisted

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<LoanNotification> Notifications { get; set; } = new List<LoanNotification>();

        // Computed properties
        public string FullName => FirstName + " " + LastName;
    }
}
