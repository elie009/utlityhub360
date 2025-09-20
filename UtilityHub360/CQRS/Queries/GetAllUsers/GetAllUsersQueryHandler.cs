using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for GetAllUsersQuery
    /// </summary>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync(cancellationToken);
            return _mapper.Map<List<UserDto>>(users);
        }
    }
}
