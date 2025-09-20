using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Query to get all loans
    /// </summary>
    public class GetAllLoansQuery : IRequest<List<LoanDto>>
    {
        public string? Status { get; set; }
        public string? LoanType { get; set; }
        public int? BorrowerId { get; set; }
        public bool? IsOverdue { get; set; }
    }
}