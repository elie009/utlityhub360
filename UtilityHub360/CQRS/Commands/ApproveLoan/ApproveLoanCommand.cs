using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.ApproveLoan
{
    public class ApproveLoanCommand : IRequest<LoanDto>
    {
        public int LoanId { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

