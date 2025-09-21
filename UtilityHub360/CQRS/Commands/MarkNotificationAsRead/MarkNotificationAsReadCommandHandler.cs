using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, NotificationDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public MarkNotificationAsReadCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<NotificationDto> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

            if (notification == null)
            {
                throw new ArgumentException("Notification not found");
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<NotificationDto>(notification);
        }
    }
}

