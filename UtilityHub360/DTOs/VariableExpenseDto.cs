using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class VariableExpenseDto
    {
        public string? Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = "OTHER";
        public string Currency { get; set; } = "USD";
        public DateTime ExpenseDate { get; set; }
        public string? Notes { get; set; }
        public string? Merchant { get; set; }
        public string? PaymentMethod { get; set; }
        public bool IsRecurring { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateVariableExpenseDto
    {
        [Required]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = "OTHER";

        [Required]
        public DateTime ExpenseDate { get; set; }

        [StringLength(100, ErrorMessage = "Merchant cannot exceed 100 characters")]
        public string? Merchant { get; set; }

        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        public string? PaymentMethod { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string Currency { get; set; } = "USD";

        public bool IsRecurring { get; set; } = false;
    }

    public class UpdateVariableExpenseDto
    {
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string? Category { get; set; }

        public DateTime? ExpenseDate { get; set; }

        [StringLength(100, ErrorMessage = "Merchant cannot exceed 100 characters")]
        public string? Merchant { get; set; }

        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        public string? PaymentMethod { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string? Currency { get; set; }

        public bool? IsRecurring { get; set; }
    }
}

