using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.RejectLoan
{
    public class RejectLoanCommand : IRequest<LoanDto>
    {
        public int LoanId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RejectedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

