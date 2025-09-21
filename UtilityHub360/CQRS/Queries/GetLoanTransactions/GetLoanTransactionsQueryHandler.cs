using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Queries.GetLoanTransactions
{
    public class GetLoanTransactionsQueryHandler : IRequestHandler<GetLoanTransactionsQuery, IEnumerable<TransactionDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetLoanTransactionsQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransactionDto>> Handle(GetLoanTransactionsQuery request, CancellationToken cancellationToken)
        {
            var transactions = await _context.Transactions
                .Where(t => t.LoanId == request.LoanId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }
    }
}

