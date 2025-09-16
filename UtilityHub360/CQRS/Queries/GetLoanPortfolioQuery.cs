using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Query to get loan portfolio summary
    /// </summary>
    public class GetLoanPortfolioQuery : IRequest<LoanPortfolioDto>
    {
        public string Branch { get; set; }
    }
}