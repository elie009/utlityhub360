using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Command to create a new loan
    /// </summary>
    public class CreateLoanCommand : IRequest<LoanDto>
    {
        public int BorrowerId { get; set; }
        public string? LoanType { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public string? RepaymentFrequency { get; set; }
        public string? AmortizationType { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = "Active";

        public CreateLoanCommand()
        {
        }
    }
}