using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a notification sent to a borrower
    /// </summary>
    [Table("LnNotifications")]
    public class LoanNotification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int BorrowerId { get; set; }

        public int? LoanId { get; set; }

        [Required]
        [StringLength(255)]
        public string Message { get; set; }

        [StringLength(20)]
        public string NotificationType { get; set; } // SMS, Email

        public DateTime? SentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BorrowerId")]
        public virtual Borrower Borrower { get; set; }

        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; }
    }
}
