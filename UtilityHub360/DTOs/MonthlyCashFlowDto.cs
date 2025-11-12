namespace UtilityHub360.DTOs
{
    public class MonthlyCashFlowDto
    {
        public int Year { get; set; }
        public List<MonthlyDataDto> MonthlyData { get; set; } = new List<MonthlyDataDto>();
        public decimal TotalIncoming { get; set; }
        public decimal TotalOutgoing { get; set; }
        public decimal NetCashFlow { get; set; }
    }

    public class MonthlyDataDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty; // January, February, etc.
        public string MonthAbbreviation { get; set; } = string.Empty; // Jan, Feb, etc.
        public decimal Incoming { get; set; }
        public decimal Outgoing { get; set; }
        public decimal Net { get; set; }
        public int TransactionCount { get; set; }
    }
}


