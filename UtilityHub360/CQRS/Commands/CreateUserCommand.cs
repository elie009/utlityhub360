using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Command to create a new user
    /// </summary>
    public class CreateUserCommand : IRequest<DTOs.UserDto>
    {
        public CreateUserDto CreateUserDto { get; set; }

        public CreateUserCommand(CreateUserDto createUserDto)
        {
            CreateUserDto = createUserDto;
        }
    }
}
