using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class Transaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string LoanId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // DISBURSEMENT, PAYMENT, INTEREST, PENALTY

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Reference { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; } = null!;
    }
}
