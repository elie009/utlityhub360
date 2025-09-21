using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommand : IRequest<NotificationDto>
    {
        public int NotificationId { get; set; }
    }
}

