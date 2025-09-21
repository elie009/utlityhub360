using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.ApproveLoanApplication
{
    public class ApproveLoanApplicationCommand : IRequest<LoanApplicationDto>
    {
        public int ApplicationId { get; set; }
        public string ReviewedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

