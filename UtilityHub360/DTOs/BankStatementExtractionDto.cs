using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ExtractBankStatementRequestDto
    {
        [Required]
        public string BankAccountId { get; set; } = string.Empty;
    }

    public class ExtractBankStatementResponseDto
    {
        public string StatementName { get; set; } = string.Empty;
        public DateTime? StatementStartDate { get; set; }
        public DateTime? StatementEndDate { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? ClosingBalance { get; set; }
        public string ImportFormat { get; set; } = string.Empty; // CSV, PDF
        public string ImportSource { get; set; } = string.Empty; // File name
        public List<BankStatementItemImportDto> StatementItems { get; set; } = new List<BankStatementItemImportDto>();
        public string? ExtractedText { get; set; } // For debugging/review
        public Dictionary<string, object>? Metadata { get; set; } // Additional extracted metadata
    }
}

