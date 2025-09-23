using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class RepaymentSchedule
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string LoanId { get; set; } = string.Empty;

        [Required]
        public int InstallmentNumber { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, PAID, OVERDUE

        public DateTime? PaidAt { get; set; }

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; } = null!;
    }
}
