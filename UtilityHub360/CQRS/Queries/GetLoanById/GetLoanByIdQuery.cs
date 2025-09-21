using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanById
{
    public class GetLoanByIdQuery : IRequest<LoanDto>
    {
        public int Id { get; set; }
    }
}

