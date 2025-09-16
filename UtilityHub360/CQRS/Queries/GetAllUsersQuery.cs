using System.Collections.Generic;
using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Query to get all active users
    /// </summary>
    public class GetAllUsersQuery : IRequest<System.Collections.Generic.List<DTOs.UserDto>>
    {
    }
}
