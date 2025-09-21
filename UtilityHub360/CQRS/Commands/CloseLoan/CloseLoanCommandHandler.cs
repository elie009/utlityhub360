using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands.CloseLoan
{
    public class CloseLoanCommandHandler : IRequestHandler<CloseLoanCommand, LoanDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public CloseLoanCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanDto> Handle(CloseLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan == null)
            {
                throw new ArgumentException("Loan not found");
            }

            if (loan.Status != LoanStatus.ACTIVE)
            {
                throw new InvalidOperationException("Only active loans can be closed");
            }

            // Update loan status
            loan.Status = LoanStatus.COMPLETED;
            loan.CompletedAt = DateTime.UtcNow;

            // Create notification
            var notification = new Notification
            {
                UserId = loan.UserId,
                Type = NotificationType.GENERAL,
                Title = "Loan Completed",
                Message = $"Your loan of ${loan.Principal:N2} has been completed successfully. Thank you for your business!",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanDto>(loan);
        }
    }
}

