using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Approval workflow for expenses
    /// </summary>
    public class ExpenseApproval
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string ExpenseId { get; set; } = string.Empty; // Foreign key to Expense

        [Required]
        [StringLength(450)]
        public string RequestedBy { get; set; } = string.Empty; // User who submitted for approval

        [StringLength(450)]
        public string? ApprovedBy { get; set; } // User who approved/rejected

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, CANCELLED

        [StringLength(500)]
        public string? Notes { get; set; } // Approval/rejection notes

        [StringLength(500)]
        public string? RejectionReason { get; set; } // If rejected

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        // Approval workflow level (for multi-level approvals)
        public int ApprovalLevel { get; set; } = 1; // 1 = first level, 2 = second level, etc.

        [StringLength(450)]
        public string? NextApproverId { get; set; } // Next person in approval chain

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ExpenseId")]
        public virtual Expense Expense { get; set; } = null!;

        [ForeignKey("RequestedBy")]
        public virtual User RequestedByUser { get; set; } = null!;

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }
    }
}

