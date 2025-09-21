using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Commands.CreateLoanApplication
{
    public class CreateLoanApplicationCommandHandler : IRequestHandler<CreateLoanApplicationCommand, LoanApplicationDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public CreateLoanApplicationCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoanApplicationDto> Handle(CreateLoanApplicationCommand request, CancellationToken cancellationToken)
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Validate employment status
            if (!Enum.TryParse<EmploymentStatus>(request.EmploymentStatus, true, out var employmentStatus))
            {
                throw new ArgumentException("Invalid employment status");
            }

            // Validate term is valid (6, 12, 24, 36, 48, or 60 months)
            var validTerms = new[] { 6, 12, 24, 36, 48, 60 };
            if (!validTerms.Contains(request.Term))
            {
                throw new ArgumentException("Term must be 6, 12, 24, 36, 48, or 60 months");
            }

            // Create loan application
            var application = new LoanApplication
            {
                UserId = request.UserId,
                Principal = request.Principal,
                Purpose = request.Purpose,
                Term = request.Term,
                MonthlyIncome = request.MonthlyIncome,
                EmploymentStatus = employmentStatus,
                AdditionalInfo = request.AdditionalInfo,
                Status = LoanApplicationStatus.PENDING,
                AppliedAt = DateTime.UtcNow
            };

            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);

            // Create notification for user
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = NotificationType.GENERAL,
                Title = "Loan Application Submitted",
                Message = $"Your loan application for ${request.Principal:N2} has been submitted and is under review.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanApplicationDto>(application);
        }
    }
}
