using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanPayments
{
    public class GetLoanPaymentsQuery : IRequest<IEnumerable<PaymentDto>>
    {
        public int LoanId { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}

