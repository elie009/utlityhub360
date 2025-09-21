using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanStatus
{
    public class GetLoanStatusQueryHandler : IRequestHandler<GetLoanStatusQuery, LoanStatusDto>
    {
        private readonly UtilityHubDbContext _context;

        public GetLoanStatusQueryHandler(UtilityHubDbContext context)
        {
            _context = context;
        }

        public async Task<LoanStatusDto> Handle(GetLoanStatusQuery request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan == null)
            {
                throw new ArgumentException("Loan not found");
            }

            return new LoanStatusDto
            {
                Status = loan.Status.ToString(),
                OutstandingBalance = loan.RemainingBalance
            };
        }
    }
}

