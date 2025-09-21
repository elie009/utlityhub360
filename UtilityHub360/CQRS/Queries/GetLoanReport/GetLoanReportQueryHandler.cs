using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Queries.GetLoanReport
{
    public class GetLoanReportQueryHandler : IRequestHandler<GetLoanReportQuery, LoanReportDto>
    {
        private readonly UtilityHubDbContext _context;

        public GetLoanReportQueryHandler(UtilityHubDbContext context)
        {
            _context = context;
        }

        public async Task<LoanReportDto> Handle(GetLoanReportQuery request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan == null)
            {
                throw new ArgumentException("Loan not found");
            }

            var payments = await _context.Payments
                .Where(p => p.LoanId == request.LoanId)
                .ToListAsync(cancellationToken);

            var schedules = await _context.RepaymentSchedules
                .Where(rs => rs.LoanId == request.LoanId)
                .ToListAsync(cancellationToken);

            var paymentsMade = payments.Count;
            var paymentsRemaining = schedules.Count(rs => rs.Status == RepaymentStatus.PENDING);

            return new LoanReportDto
            {
                LoanId = loan.Id,
                UserName = loan.User.Name,
                Principal = loan.Principal,
                InterestRate = loan.InterestRate,
                Term = loan.Term,
                Status = loan.Status.ToString(),
                MonthlyPayment = loan.MonthlyPayment,
                TotalAmount = loan.TotalAmount,
                RemainingBalance = loan.RemainingBalance,
                PaymentsMade = paymentsMade,
                PaymentsRemaining = paymentsRemaining,
                AppliedAt = loan.AppliedAt,
                CompletedAt = loan.CompletedAt
            };
        }
    }
}

