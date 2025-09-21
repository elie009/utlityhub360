using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.MakePayment
{
    public class MakePaymentCommand : IRequest<PaymentDto>
    {
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }
}

