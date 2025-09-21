using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<UserDto>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

