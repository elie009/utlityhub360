using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for GetAllUsersQuery
    /// </summary>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, System.Collections.Generic.List<DTOs.UserDto>>
    {
        private readonly UtilityHub360.Models.UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(UtilityHub360.Models.UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<System.Collections.Generic.List<DTOs.UserDto>> Handle(GetAllUsersQuery request)
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            return _mapper.Map<System.Collections.Generic.List<DTOs.UserDto>>(users);
        }
    }
}
