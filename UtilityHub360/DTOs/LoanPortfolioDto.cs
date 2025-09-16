using System;
using System.Collections.Generic;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for Loan Portfolio Summary
    /// </summary>
    public class LoanPortfolioDto
    {
        public int TotalLoans { get; set; }
        public decimal TotalPrincipalAmount { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public int ActiveLoans { get; set; }
        public int ClosedLoans { get; set; }
        public int OverdueLoans { get; set; }
        public List<LoanDto> RecentLoans { get; set; } = new List<LoanDto>();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
