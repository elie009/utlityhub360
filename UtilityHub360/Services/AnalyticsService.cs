using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<MonthlyCashFlowDto>> GetMonthlyCashFlowAsync(string userId, int? year = null)
        {
            try
            {
                var targetYear = year ?? DateTime.UtcNow.Year;
                var startDate = new DateTime(targetYear, 1, 1);
                var endDate = new DateTime(targetYear, 12, 31, 23, 59, 59);

                // Get all bank transactions for the year
                var transactions = await _context.Payments
                    .Where(p => p.UserId == userId 
                             && p.IsBankTransaction 
                             && p.TransactionDate.HasValue
                             && p.TransactionDate.Value >= startDate 
                             && p.TransactionDate.Value <= endDate
                             && (p.TransactionType == "CREDIT" || p.TransactionType == "DEBIT"))
                    .ToListAsync();

                // Group by month
                var monthlyData = new List<MonthlyDataDto>();
                var monthNames = new[] { "January", "February", "March", "April", "May", "June", 
                                        "July", "August", "September", "October", "November", "December" };
                var monthAbbreviations = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                                  "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

                for (int month = 1; month <= 12; month++)
                {
                    var monthStart = new DateTime(targetYear, month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var monthTransactions = transactions
                        .Where(t => t.TransactionDate.HasValue
                                 && t.TransactionDate.Value.Year == targetYear
                                 && t.TransactionDate.Value.Month == month)
                        .ToList();

                    var incoming = monthTransactions
                        .Where(t => t.TransactionType == "CREDIT")
                        .Sum(t => t.Amount);

                    var outgoing = monthTransactions
                        .Where(t => t.TransactionType == "DEBIT")
                        .Sum(t => t.Amount);

                    monthlyData.Add(new MonthlyDataDto
                    {
                        Month = month,
                        MonthName = monthNames[month - 1],
                        MonthAbbreviation = monthAbbreviations[month - 1],
                        Incoming = incoming,
                        Outgoing = outgoing,
                        Net = incoming - outgoing,
                        TransactionCount = monthTransactions.Count
                    });
                }

                var totalIncoming = monthlyData.Sum(m => m.Incoming);
                var totalOutgoing = monthlyData.Sum(m => m.Outgoing);

                var result = new MonthlyCashFlowDto
                {
                    Year = targetYear,
                    MonthlyData = monthlyData,
                    TotalIncoming = totalIncoming,
                    TotalOutgoing = totalOutgoing,
                    NetCashFlow = totalIncoming - totalOutgoing
                };

                return ApiResponse<MonthlyCashFlowDto>.SuccessResult(result, $"Monthly cash flow data retrieved successfully for {targetYear}");
            }
            catch (Exception ex)
            {
                return ApiResponse<MonthlyCashFlowDto>.ErrorResult($"Error retrieving monthly cash flow: {ex.Message}");
            }
        }
    }
}

