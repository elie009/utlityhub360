namespace UtilityHub360.DTOs
{
    public class TransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

