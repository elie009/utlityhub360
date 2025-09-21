using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetUserLoans
{
    public class GetUserLoansQuery : IRequest<IEnumerable<LoanDto>>
    {
        public int UserId { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}

