namespace UtilityHub360.DTOs
{
    public class VariableExpenseDto
    {
        public string? Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = "OTHER";
        public string Currency { get; set; } = "USD";
        public DateTime ExpenseDate { get; set; }
        public string? Notes { get; set; }
        public string? Merchant { get; set; }
        public string? PaymentMethod { get; set; }
        public bool IsRecurring { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

