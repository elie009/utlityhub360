using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Queries.GetLoanApplicationById
{
    public class GetLoanApplicationByIdQueryHandler : IRequestHandler<GetLoanApplicationByIdQuery, LoanApplicationDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetLoanApplicationByIdQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanApplicationDto> Handle(GetLoanApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            var application = await _context.LoanApplications
                .Include(la => la.User)
                .FirstOrDefaultAsync(la => la.Id == request.Id, cancellationToken);

            if (application == null)
            {
                throw new ArgumentException("Loan application not found");
            }

            return _mapper.Map<LoanApplicationDto>(application);
        }
    }
}
