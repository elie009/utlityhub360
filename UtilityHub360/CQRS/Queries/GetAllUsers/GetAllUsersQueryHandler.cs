using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;
using UtilityHub360.Models;

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
            var query = _context.Users.AsQueryable();

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                if (Enum.TryParse<UserRole>(request.Role, true, out var role))
                {
                    query = query.Where(u => u.Role == role);
                }
            }

            var users = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<UserDto>>(users);
        }
    }
}
