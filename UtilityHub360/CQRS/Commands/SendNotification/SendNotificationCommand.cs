using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.SendNotification
{
    public class SendNotificationCommand : IRequest<NotificationDto>
    {
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

