using System;

namespace UtilityHub360.DTOs
{
    public class UserReportDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal TotalBorrowed { get; set; }
        public decimal TotalRepaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int ActiveLoans { get; set; }
        public int CompletedLoans { get; set; }
        public DateTime ReportDate { get; set; }
    }
}

