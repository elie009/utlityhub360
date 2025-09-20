using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for UpdateUserCommand
    /// </summary>
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(request.UpdateUserDto.Id);
            if (user == null)
                return null;

            user.FirstName = request.UpdateUserDto.FirstName;
            user.LastName = request.UpdateUserDto.LastName;
            user.Email = request.UpdateUserDto.Email;
            user.IsActive = request.UpdateUserDto.IsActive;
            user.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<UserDto>(user);
        }
    }
}
