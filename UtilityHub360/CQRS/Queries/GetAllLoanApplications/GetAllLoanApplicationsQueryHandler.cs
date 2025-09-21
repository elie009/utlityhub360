using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Queries.GetAllLoanApplications
{
    public class GetAllLoanApplicationsQueryHandler : IRequestHandler<GetAllLoanApplicationsQuery, IEnumerable<LoanApplicationDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllLoanApplicationsQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LoanApplicationDto>> Handle(GetAllLoanApplicationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.LoanApplications
                .Include(la => la.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<LoanApplicationStatus>(request.Status, true, out var status))
                {
                    query = query.Where(la => la.Status == status);
                }
            }

            if (request.UserId.HasValue)
            {
                query = query.Where(la => la.UserId == request.UserId.Value);
            }

            var applications = await query
                .OrderByDescending(la => la.AppliedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<LoanApplicationDto>>(applications);
        }
    }
}
