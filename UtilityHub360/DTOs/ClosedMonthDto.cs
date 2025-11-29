namespace UtilityHub360.DTOs
{
    public class ClosedMonthDto
    {
        public string Id { get; set; } = string.Empty;
        public string BankAccountId { get; set; } = string.Empty;
        public string? BankAccountName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty; // "January", "February", etc.
        public string ClosedBy { get; set; } = string.Empty;
        public string? ClosedByName { get; set; }
        public DateTime ClosedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CloseMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; } // 1-12
        public string? Notes { get; set; }
    }

    public class ClosedMonthsListDto
    {
        public List<ClosedMonthDto> ClosedMonths { get; set; } = new List<ClosedMonthDto>();
        public int TotalCount { get; set; }
    }
}

