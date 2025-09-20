using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Query to get a user by ID
    /// </summary>
    public class GetUserByIdQuery : IRequest<UserDto?>
    {
        public int Id { get; set; }

        public GetUserByIdQuery(int id)
        {
            Id = id;
        }
    }
}
