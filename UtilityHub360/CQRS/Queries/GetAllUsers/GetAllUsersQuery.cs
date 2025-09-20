using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Query to get all active users
    /// </summary>
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
    }
}
