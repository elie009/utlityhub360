using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Query to get a user by ID
    /// </summary>
    public class GetUserByIdQuery : IRequest<DTOs.UserDto>
    {
        public int Id { get; set; }

        public GetUserByIdQuery(int id)
        {
            Id = id;
        }
    }
}
