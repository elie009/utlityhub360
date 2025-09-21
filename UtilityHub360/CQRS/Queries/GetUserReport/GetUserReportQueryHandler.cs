using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Queries.GetUserReport
{
    public class GetUserReportQueryHandler : IRequestHandler<GetUserReportQuery, UserReportDto>
    {
        private readonly UtilityHubDbContext _context;

        public GetUserReportQueryHandler(UtilityHubDbContext context)
        {
            _context = context;
        }

        public async Task<UserReportDto> Handle(GetUserReportQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var loans = await _context.Loans
                .Where(l => l.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            var totalBorrowed = loans.Sum(l => l.Principal);
            var totalRepaid = loans.Sum(l => l.Principal - l.RemainingBalance);
            var outstandingBalance = loans.Sum(l => l.RemainingBalance);
            var activeLoans = loans.Count(l => l.Status == LoanStatus.ACTIVE);
            var completedLoans = loans.Count(l => l.Status == LoanStatus.COMPLETED);

            return new UserReportDto
            {
                UserId = user.Id,
                UserName = user.Name,
                TotalBorrowed = totalBorrowed,
                TotalRepaid = totalRepaid,
                OutstandingBalance = outstandingBalance,
                ActiveLoans = activeLoans,
                CompletedLoans = completedLoans,
                ReportDate = DateTime.UtcNow
            };
        }
    }
}

