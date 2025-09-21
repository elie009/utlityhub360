using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands.DisburseLoan
{
    public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, DisbursementDto>
    {
        private readonly UtilityHubDbContext _context;

        public DisburseLoanCommandHandler(UtilityHubDbContext context)
        {
            _context = context;
        }

        public async Task<DisbursementDto> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan == null)
            {
                throw new ArgumentException("Loan not found");
            }

            if (loan.Status != LoanStatus.APPROVED)
            {
                throw new InvalidOperationException("Only approved loans can be disbursed");
            }

            // Update loan status
            loan.Status = LoanStatus.ACTIVE;
            loan.DisbursedAt = DateTime.UtcNow;

            // Create disbursement transaction
            var transaction = new Transaction
            {
                LoanId = request.LoanId,
                Type = TransactionType.DISBURSEMENT,
                Amount = loan.Principal,
                Description = $"Loan disbursed via {request.DisbursementMethod}",
                Reference = request.Reference,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Create notification
            var notification = new Notification
            {
                UserId = loan.UserId,
                Type = NotificationType.GENERAL,
                Title = "Loan Disbursed",
                Message = $"Your loan of ${loan.Principal:N2} has been disbursed successfully. Your loan is now active.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return new DisbursementDto
            {
                TransactionId = transaction.Id,
                LoanId = request.LoanId,
                Amount = loan.Principal,
                Method = request.DisbursementMethod,
                Reference = request.Reference,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}

