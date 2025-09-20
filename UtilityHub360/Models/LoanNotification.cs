using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a notification sent to a borrower
    /// </summary>
    public class LoanNotification
    {
        public int NotificationId { get; set; }

        public int BorrowerId { get; set; }

        public int? LoanId { get; set; }

        [Required]
        [StringLength(255)]
        public string Message { get; set; } = string.Empty;

        [StringLength(20)]
        public string? NotificationType { get; set; } // SMS, Email

        public DateTime? SentDate { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Borrower Borrower { get; set; } = null!;
        public Loan? Loan { get; set; }
    }
}