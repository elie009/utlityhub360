using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands.RejectLoanApplication
{
    public class RejectLoanApplicationCommandHandler : IRequestHandler<RejectLoanApplicationCommand, LoanApplicationDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public RejectLoanApplicationCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanApplicationDto> Handle(RejectLoanApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.LoanApplications
                .Include(la => la.User)
                .FirstOrDefaultAsync(la => la.Id == request.ApplicationId, cancellationToken);

            if (application == null)
            {
                throw new ArgumentException("Loan application not found");
            }

            if (application.Status != LoanApplicationStatus.PENDING)
            {
                throw new InvalidOperationException("Only pending applications can be rejected");
            }

            // Update application status
            application.Status = LoanApplicationStatus.REJECTED;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = request.RejectedBy;
            application.RejectionReason = request.RejectionReason;

            await _context.SaveChangesAsync(cancellationToken);

            // Create notification for user
            var notification = new Notification
            {
                UserId = application.UserId,
                Type = NotificationType.LOAN_REJECTED,
                Title = "Loan Application Rejected",
                Message = $"Your loan application for ${application.Principal:N2} has been rejected. Reason: {request.RejectionReason}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanApplicationDto>(application);
        }
    }
}
