using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for getting all loans
    /// </summary>
    public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, List<LoanDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllLoansQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<LoanDto>> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Loans.Include(l => l.Borrower).AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(l => l.Status == request.Status);

            if (!string.IsNullOrEmpty(request.LoanType))
                query = query.Where(l => l.LoanType == request.LoanType);

            if (request.BorrowerId.HasValue)
                query = query.Where(l => l.BorrowerId == request.BorrowerId);

            if (request.IsOverdue.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                if (request.IsOverdue.Value)
                    query = query.Where(l => l.RepaymentSchedules.Any(rs => !rs.IsPaid && rs.DueDate < today));
                else
                    query = query.Where(l => !l.RepaymentSchedules.Any(rs => !rs.IsPaid && rs.DueDate < today));
            }

            var loans = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<LoanDto>>(loans);
        }
    }
}