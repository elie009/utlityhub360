namespace UtilityHub360.Services
{
    public interface IOcrService
    {
        Task<OcrResult> ProcessImageAsync(Stream imageStream, string fileType);
        Task<OcrResult> ProcessPdfAsync(Stream pdfStream);
    }

    public class OcrResult
    {
        public string FullText { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }
        public string? Merchant { get; set; }
        public List<ReceiptItem> Items { get; set; } = new();
        public double Confidence { get; set; }
        public string Provider { get; set; } = string.Empty;
    }

    public class ReceiptItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }
}

