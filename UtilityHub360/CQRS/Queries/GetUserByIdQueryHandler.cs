using System.Data.Entity;
using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for GetUserByIdQuery
    /// </summary>
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, DTOs.UserDto>
    {
        private readonly UtilityHub360.Models.UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(UtilityHub360.Models.UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DTOs.UserDto> Handle(GetUserByIdQuery request)
        {
            var user = await _context.Users.FindAsync(request.Id);
            if (user == null)
                return null;

            return _mapper.Map<DTOs.UserDto>(user);
        }
    }
}
