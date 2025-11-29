using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Dedicated Expense entity for comprehensive expense management
    /// </summary>
    public class Expense
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(450)]
        public string CategoryId { get; set; } = string.Empty; // Foreign key to ExpenseCategory

        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? Merchant { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; } // CASH, CARD, BANK_TRANSFER, WALLET, CHECK

        [StringLength(450)]
        public string? BankAccountId { get; set; } // If paid from bank account

        [StringLength(100)]
        public string? Location { get; set; }

        // Tax and Reimbursement
        public bool IsTaxDeductible { get; set; } = false;
        public bool IsReimbursable { get; set; } = false;
        [StringLength(450)]
        public string? ReimbursementRequestId { get; set; } // For linking to reimbursement requests

        // Mileage and Per-diem
        public decimal? Mileage { get; set; } // Miles/KM traveled
        public decimal? MileageRate { get; set; } // Rate per mile/km
        public decimal? PerDiemAmount { get; set; } // Per diem allowance
        public int? NumberOfDays { get; set; } // Number of days for per diem

        // Approval Workflow
        [StringLength(20)]
        public string ApprovalStatus { get; set; } = "PENDING_APPROVAL"; // PENDING_APPROVAL, APPROVED, REJECTED, NOT_REQUIRED
        [StringLength(450)]
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        [StringLength(500)]
        public string? ApprovalNotes { get; set; }

        // Receipt
        public bool HasReceipt { get; set; } = false;
        [StringLength(450)]
        public string? ReceiptId { get; set; } // Foreign key to ExpenseReceipt

        // Budget Tracking
        [StringLength(450)]
        public string? BudgetId { get; set; } // Foreign key to ExpenseBudget

        // Recurring Expenses
        public bool IsRecurring { get; set; } = false;
        [StringLength(50)]
        public string? RecurringFrequency { get; set; } // DAILY, WEEKLY, MONTHLY, QUARTERLY, YEARLY
        [StringLength(450)]
        public string? ParentExpenseId { get; set; } // For recurring expense series

        // Tags for better organization
        [StringLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

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

        [ForeignKey("CategoryId")]
        public virtual ExpenseCategory Category { get; set; } = null!;

        [ForeignKey("ReceiptId")]
        public virtual ExpenseReceipt? Receipt { get; set; }

        [ForeignKey("BudgetId")]
        public virtual ExpenseBudget? Budget { get; set; }
    }
}

