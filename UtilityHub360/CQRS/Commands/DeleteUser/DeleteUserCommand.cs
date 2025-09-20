using MediatR;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Command to delete (soft delete) a user
    /// </summary>
    public class DeleteUserCommand : IRequest
    {
        public int Id { get; set; }

        public DeleteUserCommand(int id)
        {
            Id = id;
        }
    }
}
