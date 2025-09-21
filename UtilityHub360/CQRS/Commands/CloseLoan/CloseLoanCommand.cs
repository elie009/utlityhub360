using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.CloseLoan
{
    public class CloseLoanCommand : IRequest<LoanDto>
    {
        public int LoanId { get; set; }
        public string ClosedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

