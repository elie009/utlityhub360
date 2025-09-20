using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Command to record a payment against a loan
    /// </summary>
    public class RecordPaymentCommand : IRequest<PaymentDto>
    {
        public int LoanId { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }
}