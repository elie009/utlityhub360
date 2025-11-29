namespace UtilityHub360.Models
{
    /// <summary>
    /// Request model for creating a transaction (used by automation services)
    /// </summary>
    public class CreateTransactionRequest
    {
        public string BankAccountId { get; set; } = string.Empty;
        public string TransactionType { get; set; } = "DEBIT";
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? MerchantName { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Location { get; set; }
    }
}

