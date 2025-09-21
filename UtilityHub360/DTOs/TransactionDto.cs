using System;

namespace UtilityHub360.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

