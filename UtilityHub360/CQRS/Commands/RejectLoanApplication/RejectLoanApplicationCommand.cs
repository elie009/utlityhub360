using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.RejectLoanApplication
{
    public class RejectLoanApplicationCommand : IRequest<LoanApplicationDto>
    {
        public int ApplicationId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public string RejectedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

