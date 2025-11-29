namespace UtilityHub360.DTOs
{
    public class ReceiptDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? ExpenseId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? OriginalFileName { get; set; }
        public decimal? ExtractedAmount { get; set; }
        public DateTime? ExtractedDate { get; set; }
        public string? ExtractedMerchant { get; set; }
        public List<ReceiptItemDto>? ExtractedItems { get; set; }
        public string? OcrText { get; set; }
        public bool IsOcrProcessed { get; set; }
        public DateTime? OcrProcessedAt { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? Notes { get; set; }
        public string? FileUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ReceiptItemDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }

    public class ReceiptSearchDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Merchant { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? IsOcrProcessed { get; set; }
        public string? SearchText { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
    }

    public class ExpenseMatchDto
    {
        public string ExpenseId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Merchant { get; set; }
        public string Category { get; set; } = string.Empty;
        public double MatchScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }
}

