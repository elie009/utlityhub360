using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Queries.GetPaymentById
{
    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetPaymentByIdQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (payment == null)
            {
                throw new ArgumentException("Payment not found");
            }

            return _mapper.Map<PaymentDto>(payment);
        }
    }
}

