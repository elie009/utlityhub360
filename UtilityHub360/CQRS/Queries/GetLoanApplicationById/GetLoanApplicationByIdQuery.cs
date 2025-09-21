using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanApplicationById
{
    public class GetLoanApplicationByIdQuery : IRequest<LoanApplicationDto>
    {
        public int Id { get; set; }
    }
}

