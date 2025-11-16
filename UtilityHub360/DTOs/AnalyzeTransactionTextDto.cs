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

        // Optional: Link transaction to a bill, loan, or savings account
        // Only one of these should be provided at a time
        [StringLength(450)]
        public string? BillId { get; set; }

        [StringLength(450)]
        public string? LoanId { get; set; }

        [StringLength(450)]
        public string? SavingsAccountId { get; set; }
    }
}

