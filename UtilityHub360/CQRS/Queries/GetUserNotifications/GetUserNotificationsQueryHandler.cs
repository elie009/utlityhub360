using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Queries.GetUserNotifications
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, IEnumerable<NotificationDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetUserNotificationsQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == request.UserId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (request.Status.ToLower() == "unread")
                {
                    query = query.Where(n => !n.IsRead);
                }
                else if (request.Status.ToLower() == "read")
                {
                    query = query.Where(n => n.IsRead);
                }
            }

            if (!string.IsNullOrEmpty(request.Type))
            {
                if (Enum.TryParse<NotificationType>(request.Type, true, out var type))
                {
                    query = query.Where(n => n.Type == type);
                }
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }
    }
}

