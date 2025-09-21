using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.DTOs;
using UtilityHub360.CQRS.Queries.GetUserNotifications;
using UtilityHub360.CQRS.Commands.MarkNotificationAsRead;
using UtilityHub360.CQRS.Commands.SendNotification;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get user notifications
        /// </summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), 200)]
        public async Task<IActionResult> GetUserNotifications(int userId, [FromQuery] string? status = null, [FromQuery] string? type = null, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var query = new GetUserNotificationsQuery
                {
                    UserId = userId,
                    Status = status,
                    Type = type,
                    Page = page,
                    Limit = limit
                };

                var notifications = await _mediator.Send(query);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        [ProducesResponseType(typeof(NotificationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var command = new MarkNotificationAsReadCommand
                {
                    NotificationId = notificationId
                };

                var notification = await _mediator.Send(command);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Send notification (Admin only)
        /// </summary>
        [HttpPost("send")]
        [ProducesResponseType(typeof(NotificationDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new SendNotificationCommand
                {
                    UserId = request.UserId,
                    Type = request.Type,
                    Title = request.Title,
                    Message = request.Message
                };

                var notification = await _mediator.Send(command);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class SendNotificationRequest
    {
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

