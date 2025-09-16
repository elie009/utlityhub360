using System.Data.Entity;
using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for CreateUserCommand
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, DTOs.UserDto>
    {
        private readonly UtilityHub360.Models.UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public CreateUserCommandHandler(UtilityHub360.Models.UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DTOs.UserDto> Handle(CreateUserCommand request)
        {
            var user = new UtilityHub360.Models.User
            {
                FirstName = request.CreateUserDto.FirstName,
                LastName = request.CreateUserDto.LastName,
                Email = request.CreateUserDto.Email,
                CreatedDate = System.DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<DTOs.UserDto>(user);
        }
    }
}
