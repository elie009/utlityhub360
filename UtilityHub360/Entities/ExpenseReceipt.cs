using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Receipt attachments for expenses
    /// </summary>
    public class ExpenseReceipt
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? ExpenseId { get; set; } // Foreign key to Expense (nullable for orphaned receipts)

        [Required]
        [StringLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty; // Storage path

        [StringLength(50)]
        public string FileType { get; set; } = string.Empty; // image/jpeg, image/png, application/pdf

        [Column(TypeName = "bigint")]
        public long FileSize { get; set; } // File size in bytes

        [StringLength(500)]
        public string? OriginalFileName { get; set; }

        // OCR Data (extracted from receipt)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExtractedAmount { get; set; }
        public DateTime? ExtractedDate { get; set; }
        [StringLength(200)]
        public string? ExtractedMerchant { get; set; }
        [StringLength(500)]
        public string? ExtractedItems { get; set; } // JSON array of items
        [StringLength(1000)]
        public string? OcrText { get; set; } // Full OCR text
        public bool IsOcrProcessed { get; set; } = false;
        public DateTime? OcrProcessedAt { get; set; }

        // Thumbnail for images
        [StringLength(500)]
        public string? ThumbnailPath { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ExpenseId")]
        public virtual Expense? Expense { get; set; }
    }
}

