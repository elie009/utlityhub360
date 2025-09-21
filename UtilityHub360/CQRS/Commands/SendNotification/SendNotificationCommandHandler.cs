using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Commands.SendNotification
{
    public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, NotificationDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public SendNotificationCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<NotificationDto> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            // Validate user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Validate notification type
            if (!Enum.TryParse<NotificationType>(request.Type, true, out var type))
            {
                throw new ArgumentException("Invalid notification type");
            }

            // Create notification
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = type,
                Title = request.Title,
                Message = request.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<NotificationDto>(notification);
        }
    }
}

