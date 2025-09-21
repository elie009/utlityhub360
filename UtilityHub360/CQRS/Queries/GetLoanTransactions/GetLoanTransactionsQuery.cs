using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetLoanTransactions
{
    public class GetLoanTransactionsQuery : IRequest<IEnumerable<TransactionDto>>
    {
        public int LoanId { get; set; }
    }
}

