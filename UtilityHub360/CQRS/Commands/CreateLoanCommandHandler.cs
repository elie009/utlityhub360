using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

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

        public async Task<LoanDto> Handle(CreateLoanCommand request)
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
            await _context.SaveChangesAsync();

            // Load borrower for mapping
            loan.Borrower = _context.Borrowers.Find(loan.BorrowerId);

            return _mapper.Map<LoanDto>(loan);
        }
    }
}