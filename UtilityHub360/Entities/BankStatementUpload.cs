using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UtilityHub360.Models;

namespace UtilityHub360.Entities
{
    public class BankStatementUpload
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty; // Server file path

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty; // PDF, CSV

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "PENDING"; // PENDING, PROCESSING, COMPLETED, FAILED

        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        [StringLength(450)]
        public string? ProcessedBankStatementId { get; set; } // Link to BankStatement after processing

        public int RetryCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; } = null!;

        [ForeignKey("ProcessedBankStatementId")]
        public virtual BankStatement? ProcessedBankStatement { get; set; }
    }
}


