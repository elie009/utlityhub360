using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Queries.GetLoanSchedule
{
    public class GetLoanScheduleQueryHandler : IRequestHandler<GetLoanScheduleQuery, IEnumerable<RepaymentScheduleDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetLoanScheduleQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RepaymentScheduleDto>> Handle(GetLoanScheduleQuery request, CancellationToken cancellationToken)
        {
            var schedules = await _context.RepaymentSchedules
                .Where(rs => rs.LoanId == request.LoanId)
                .OrderBy(rs => rs.InstallmentNumber)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<RepaymentScheduleDto>>(schedules);
        }
    }
}

