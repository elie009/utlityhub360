using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.DisburseLoan
{
    public class DisburseLoanCommand : IRequest<DisbursementDto>
    {
        public int LoanId { get; set; }
        public string DisbursedBy { get; set; } = string.Empty;
        public string DisbursementMethod { get; set; } = string.Empty;
        public string? Reference { get; set; }
    }

    public class DisbursementDto
    {
        public int TransactionId { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}

