using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanStatus
{
    public class GetLoanStatusQuery : IRequest<LoanStatusDto>
    {
        public int LoanId { get; set; }
    }

    public class LoanStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; }
    }
}

