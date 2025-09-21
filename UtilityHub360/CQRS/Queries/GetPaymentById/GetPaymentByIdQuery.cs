using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetPaymentById
{
    public class GetPaymentByIdQuery : IRequest<PaymentDto>
    {
        public int Id { get; set; }
    }
}

