using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Queries.GetLoanPayments
{
    public class GetLoanPaymentsQueryHandler : IRequestHandler<GetLoanPaymentsQuery, IEnumerable<PaymentDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetLoanPaymentsQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentDto>> Handle(GetLoanPaymentsQuery request, CancellationToken cancellationToken)
        {
            var payments = await _context.Payments
                .Where(p => p.LoanId == request.LoanId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }
    }
}
