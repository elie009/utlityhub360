using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class AnalyzeTransactionTextDto
    {
        [Required]
        [StringLength(2000, ErrorMessage = "Transaction text cannot exceed 2000 characters")]
        public string TransactionText { get; set; } = string.Empty;

        [StringLength(450)]
        public string? BankAccountId { get; set; }
    }
}

