using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<LoginResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

