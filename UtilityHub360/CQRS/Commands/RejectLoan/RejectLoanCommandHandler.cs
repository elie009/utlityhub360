using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands.RejectLoan
{
    public class RejectLoanCommandHandler : IRequestHandler<RejectLoanCommand, LoanDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public RejectLoanCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanDto> Handle(RejectLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan == null)
            {
                throw new ArgumentException("Loan not found");
            }

            if (loan.Status != LoanStatus.PENDING)
            {
                throw new InvalidOperationException("Only pending loans can be rejected");
            }

            // Update loan status
            loan.Status = LoanStatus.REJECTED;

            // Create notification
            var notification = new Notification
            {
                UserId = loan.UserId,
                Type = NotificationType.LOAN_REJECTED,
                Title = "Loan Rejected",
                Message = $"Your loan application for ${loan.Principal:N2} has been rejected. Reason: {request.Reason}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanDto>(loan);
        }
    }
}

