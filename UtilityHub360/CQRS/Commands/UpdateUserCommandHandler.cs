using System.Data.Entity;
using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for UpdateUserCommand
    /// </summary>
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, DTOs.UserDto>
    {
        private readonly UtilityHub360.Models.UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(UtilityHub360.Models.UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DTOs.UserDto> Handle(UpdateUserCommand request)
        {
            var user = await _context.Users.FindAsync(request.UpdateUserDto.Id);
            if (user == null)
                return null;

            user.FirstName = request.UpdateUserDto.FirstName;
            user.LastName = request.UpdateUserDto.LastName;
            user.Email = request.UpdateUserDto.Email;
            user.IsActive = request.UpdateUserDto.IsActive;
            user.LastModifiedDate = System.DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<DTOs.UserDto>(user);
        }
    }
}
