using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        public int Id { get; set; }
    }
}