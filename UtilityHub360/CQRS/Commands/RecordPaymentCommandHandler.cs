using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for recording a payment
    /// </summary>
    public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, PaymentDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public RecordPaymentCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentDto> Handle(RecordPaymentCommand request)
        {
            var payment = new LoanPayment
            {
                LoanId = request.LoanId,
                ScheduleId = request.ScheduleId,
                PaymentDate = request.PaymentDate,
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return _mapper.Map<PaymentDto>(payment);
        }
    }
}