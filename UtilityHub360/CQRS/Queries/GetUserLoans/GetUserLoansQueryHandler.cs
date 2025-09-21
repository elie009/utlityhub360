using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Queries.GetUserLoans
{
    public class GetUserLoansQueryHandler : IRequestHandler<GetUserLoansQuery, IEnumerable<LoanDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetUserLoansQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LoanDto>> Handle(GetUserLoansQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Loans
                .Include(l => l.User)
                .Where(l => l.UserId == request.UserId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<LoanStatus>(request.Status, true, out var status))
                {
                    query = query.Where(l => l.Status == status);
                }
            }

            var loans = await query
                .OrderByDescending(l => l.AppliedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<LoanDto>>(loans);
        }
    }
}

