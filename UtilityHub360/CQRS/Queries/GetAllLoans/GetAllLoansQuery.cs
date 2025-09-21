using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetAllLoans
{
    /// <summary>
    /// Query to get all loans
    /// </summary>
    public class GetAllLoansQuery : IRequest<IEnumerable<LoanDto>>
    {
        public string? Status { get; set; }
        public int? UserId { get; set; }
        public bool? IsOverdue { get; set; }
    }
}