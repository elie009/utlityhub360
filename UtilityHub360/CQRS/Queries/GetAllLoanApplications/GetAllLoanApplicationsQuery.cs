using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetAllLoanApplications
{
    public class GetAllLoanApplicationsQuery : IRequest<IEnumerable<LoanApplicationDto>>
    {
        public string? Status { get; set; }
        public int? UserId { get; set; }
    }
}

