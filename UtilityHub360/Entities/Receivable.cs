using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class Receivable
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string BorrowerName { get; set; } = string.Empty; // Name of person who owes money

        [StringLength(500)]
        public string? BorrowerContact { get; set; } // Phone, email, etc.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Principal { get; set; } // Amount lent out

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; } = 0; // Interest rate (can be 0 for interest-free loans)

        [Required]
        public int Term { get; set; } // months

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty; // Reason for the loan

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, COMPLETED, DEFAULTED, CANCELLED

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPayment { get; set; } // Expected monthly payment amount

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Total amount to be repaid (Principal + Interest)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBalance { get; set; } // Amount still owed

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaid { get; set; } = 0; // Total amount paid so far

        [Required]
        public DateTime LentAt { get; set; } = DateTime.UtcNow; // When money was lent

        public DateTime? StartDate { get; set; } // Start date for payment schedule

        public DateTime? CompletedAt { get; set; } // When fully paid

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        [StringLength(20)]
        public string PaymentFrequency { get; set; } = "MONTHLY"; // MONTHLY, WEEKLY, BIWEEKLY, QUARTERLY

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        [StringLength(450)]
        public string? DeletedBy { get; set; }
        
        [StringLength(500)]
        public string? DeleteReason { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<ReceivablePayment> Payments { get; set; } = new List<ReceivablePayment>();
    }
}

