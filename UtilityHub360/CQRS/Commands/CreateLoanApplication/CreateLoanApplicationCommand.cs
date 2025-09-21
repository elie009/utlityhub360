using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.CreateLoanApplication
{
    public class CreateLoanApplicationCommand : IRequest<LoanApplicationDto>
    {
        public int UserId { get; set; }
        public decimal Principal { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int Term { get; set; }
        public decimal MonthlyIncome { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
    }
}

