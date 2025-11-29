using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class TransactionCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "EXPENSE"; // EXPENSE, INCOME, TRANSFER, BILL, SAVINGS, LOAN
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public bool IsSystemCategory { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TransactionCount { get; set; } // Number of transactions using this category
    }

    public class CreateTransactionCategoryDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "EXPENSE"; // EXPENSE, INCOME, TRANSFER, BILL, SAVINGS, LOAN

        [StringLength(50)]
        public string? Icon { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateTransactionCategoryDto
    {
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public bool? IsActive { get; set; }

        public int? DisplayOrder { get; set; }
    }
}

