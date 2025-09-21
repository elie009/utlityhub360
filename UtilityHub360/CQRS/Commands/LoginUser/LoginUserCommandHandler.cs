using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponseDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public LoginUserCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LoginResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException("Invalid email or password");
            }

            // TODO: Implement proper password hashing and verification
            // For now, we'll just check if password is not empty
            if (string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Invalid email or password");
            }

            // TODO: Generate JWT token
            var token = "jwt-token-placeholder";
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new LoginResponseDto
            {
                User = _mapper.Map<UserDto>(user),
                Token = token,
                ExpiresAt = expiresAt
            };
        }
    }
}

