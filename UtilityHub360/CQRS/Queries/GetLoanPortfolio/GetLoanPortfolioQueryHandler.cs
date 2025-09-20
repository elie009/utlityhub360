using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for getting loan portfolio summary
    /// </summary>
    public class GetLoanPortfolioQueryHandler : IRequestHandler<GetLoanPortfolioQuery, LoanPortfolioDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetLoanPortfolioQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanPortfolioDto> Handle(GetLoanPortfolioQuery request, CancellationToken cancellationToken)
        {
            var loans = await _context.Loans
                .Include(l => l.Borrower)
                .ToListAsync(cancellationToken);

            var portfolio = new LoanPortfolioDto
            {
                TotalLoans = loans.Count,
                TotalPrincipalAmount = loans.Sum(l => l.PrincipalAmount),
                TotalOutstandingBalance = loans.Sum(l => l.OutstandingBalance),
                TotalPaid = loans.Sum(l => l.TotalPaid),
                TotalInterestEarned = loans.Sum(l => l.TotalPaid - l.PrincipalAmount),
                ActiveLoans = loans.Count(l => l.Status == "Active"),
                ClosedLoans = loans.Count(l => l.Status == "Closed"),
                OverdueLoans = loans.Count(l => l.DaysOverdue > 0),
                RecentLoans = _mapper.Map<List<LoanDto>>(loans.OrderByDescending(l => l.CreatedAt).Take(10).ToList())
            };

            return portfolio;
        }
    }
}