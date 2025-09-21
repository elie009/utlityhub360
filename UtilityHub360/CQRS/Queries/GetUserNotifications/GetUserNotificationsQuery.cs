using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetUserNotifications
{
    public class GetUserNotificationsQuery : IRequest<IEnumerable<NotificationDto>>
    {
        public int UserId { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}

