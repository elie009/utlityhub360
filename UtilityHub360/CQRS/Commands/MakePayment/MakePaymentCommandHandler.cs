using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands.MakePayment
{
    public class MakePaymentCommandHandler : IRequestHandler<MakePaymentCommand, PaymentDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public MakePaymentCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentDto> Handle(MakePaymentCommand request, CancellationToken cancellationToken)
        {
            // Validate loan exists
            var loan = await _context.Loans.FindAsync(new object[] { request.LoanId }, cancellationToken);
            if (loan == null)
            {
                throw new ArgumentException("Loan not found");
            }

            // Validate user exists
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Validate payment method
            if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var paymentMethod))
            {
                throw new ArgumentException("Invalid payment method");
            }

            // Create payment
            var payment = new Payment
            {
                LoanId = request.LoanId,
                UserId = request.UserId,
                Amount = request.Amount,
                Method = paymentMethod,
                Reference = request.Reference,
                Status = PaymentStatus.COMPLETED,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Update loan remaining balance
            loan.RemainingBalance -= request.Amount;
            if (loan.RemainingBalance <= 0)
            {
                loan.Status = LoanStatus.COMPLETED;
                loan.CompletedAt = DateTime.UtcNow;
            }

            // Create transaction record
            var transaction = new Transaction
            {
                LoanId = request.LoanId,
                Type = TransactionType.PAYMENT,
                Amount = request.Amount,
                Description = $"Payment received via {request.Method}",
                Reference = request.Reference,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Create notification for user
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = NotificationType.PAYMENT_RECEIVED,
                Title = "Payment Received",
                Message = $"Your payment of ${request.Amount:N2} has been received and processed successfully.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PaymentDto>(payment);
        }
    }
}
