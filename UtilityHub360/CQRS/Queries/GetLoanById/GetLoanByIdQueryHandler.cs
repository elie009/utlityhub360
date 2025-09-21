using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Queries.GetLoanById
{
    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetLoanByIdQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanDto> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

            if (loan == null)
            {
                throw new ArgumentException("Loan not found");
            }

            return _mapper.Map<LoanDto>(loan);
        }
    }
}
