using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Queries.GetAllLoans
{
    /// <summary>
    /// Handler for getting all loans
    /// </summary>
    public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, IEnumerable<LoanDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllLoansQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LoanDto>> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Loans.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<LoanStatus>(request.Status, true, out var status))
                {
                    query = query.Where(l => l.Status == status);
                }
            }

            if (request.UserId.HasValue)
                query = query.Where(l => l.UserId == request.UserId);

            if (request.IsOverdue.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                if (request.IsOverdue.Value)
                    query = query.Where(l => l.RepaymentSchedules.Any(rs => rs.Status == RepaymentStatus.OVERDUE));
                else
                    query = query.Where(l => !l.RepaymentSchedules.Any(rs => rs.Status == RepaymentStatus.OVERDUE));
            }

            var loans = await query
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync(cancellationToken);
            
            return _mapper.Map<IEnumerable<LoanDto>>(loans);
        }
    }
}