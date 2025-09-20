using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for getting all borrowers
    /// </summary>
    public class GetAllBorrowersQueryHandler : IRequestHandler<GetAllBorrowersQuery, List<BorrowerDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllBorrowersQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<BorrowerDto>> Handle(GetAllBorrowersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Borrowers.AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(b => b.Status == request.Status);

            if (request.CreditScoreMin.HasValue)
                query = query.Where(b => b.CreditScore >= request.CreditScoreMin);

            if (request.CreditScoreMax.HasValue)
                query = query.Where(b => b.CreditScore <= request.CreditScoreMax);

            var borrowers = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<BorrowerDto>>(borrowers);
        }
    }
}