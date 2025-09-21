using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanSchedule
{
    public class GetLoanScheduleQuery : IRequest<IEnumerable<RepaymentScheduleDto>>
    {
        public int LoanId { get; set; }
    }
}

