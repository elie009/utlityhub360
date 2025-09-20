using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using AutoMapper;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for creating a new loan
    /// </summary>
    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, LoanDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public CreateLoanCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanDto> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = new Loan
            {
                BorrowerId = request.BorrowerId,
                LoanType = request.LoanType,
                PrincipalAmount = request.PrincipalAmount,
                InterestRate = request.InterestRate,
                TermMonths = request.TermMonths,
                RepaymentFrequency = request.RepaymentFrequency,
                AmortizationType = request.AmortizationType,
                StartDate = request.StartDate,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync(cancellationToken);

            // Load borrower for mapping
            loan.Borrower = await _context.Borrowers.FindAsync(new object[] { loan.BorrowerId }, cancellationToken);

            return _mapper.Map<LoanDto>(loan);
        }
    }
}