using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for CreateUserCommand
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public CreateUserCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new Models.User
            {
                FirstName = request.CreateUserDto.FirstName,
                LastName = request.CreateUserDto.LastName,
                Email = request.CreateUserDto.Email,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<UserDto>(user);
        }
    }
}
