using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Command to update an existing user
    /// </summary>
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public UpdateUserDto UpdateUserDto { get; set; }

        public UpdateUserCommand(UpdateUserDto updateUserDto)
        {
            UpdateUserDto = updateUserDto;
        }
    }
}
