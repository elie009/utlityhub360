using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands.ApproveLoanApplication
{
    public class ApproveLoanApplicationCommandHandler : IRequestHandler<ApproveLoanApplicationCommand, LoanApplicationDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public ApproveLoanApplicationCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanApplicationDto> Handle(ApproveLoanApplicationCommand request, CancellationToken cancellationToken)
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
                throw new InvalidOperationException("Only pending applications can be approved");
            }

            // Update application status
            application.Status = LoanApplicationStatus.APPROVED;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = request.ReviewedBy;

            // Create the loan
            var loan = new Loan
            {
                UserId = application.UserId,
                Principal = application.Principal,
                InterestRate = 12.0m, // Fixed 12% annual interest rate
                Term = application.Term,
                Purpose = application.Purpose,
                Status = LoanStatus.APPROVED,
                AppliedAt = application.AppliedAt,
                ApprovedAt = DateTime.UtcNow,
                AdditionalInfo = application.AdditionalInfo
            };

            // Calculate loan terms
            var monthlyInterestRate = loan.InterestRate / 100 / 12;
            var totalInterest = loan.Principal * monthlyInterestRate * loan.Term;
            loan.TotalAmount = loan.Principal + totalInterest;
            loan.MonthlyPayment = loan.TotalAmount / loan.Term;
            loan.RemainingBalance = loan.TotalAmount;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync(cancellationToken);

            // Create notification for user
            var notification = new Notification
            {
                UserId = application.UserId,
                Type = NotificationType.LOAN_APPROVED,
                Title = "Loan Application Approved",
                Message = $"Congratulations! Your loan application for ${application.Principal:N2} has been approved. Your loan is ready for disbursement.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanApplicationDto>(application);
        }
    }
}
