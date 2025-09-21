using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanReport
{
    public class GetLoanReportQuery : IRequest<LoanReportDto>
    {
        public int LoanId { get; set; }
    }
}

