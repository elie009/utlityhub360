using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                throw new ArgumentException("User with this email already exists");
            }

            // Parse role
            if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            {
                userRole = UserRole.USER;
            }

            // Create new user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Role = userRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<UserDto>(user);
        }
    }
}

