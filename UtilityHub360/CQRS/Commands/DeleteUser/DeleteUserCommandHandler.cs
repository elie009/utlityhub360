using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for DeleteUserCommand
    /// </summary>
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly UtilityHubDbContext _context;

        public DeleteUserCommandHandler(UtilityHubDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { request.Id }, cancellationToken);
            if (user != null)
            {
                user.IsActive = false;
                user.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
            return Unit.Value;
        }
    }
}
