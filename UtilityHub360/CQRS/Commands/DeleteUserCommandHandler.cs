using System.Data.Entity;
using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for DeleteUserCommand
    /// </summary>
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly UtilityHub360.Models.UtilityHubDbContext _context;

        public DeleteUserCommandHandler(UtilityHub360.Models.UtilityHubDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteUserCommand request)
        {
            var user = await _context.Users.FindAsync(request.Id);
            if (user != null)
            {
                user.IsActive = false;
                user.LastModifiedDate = System.DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Unit.Value;
        }
    }
}
