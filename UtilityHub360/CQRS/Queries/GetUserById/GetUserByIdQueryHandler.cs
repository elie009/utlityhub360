using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using AutoMapper;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for GetUserByIdQuery
    /// </summary>
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(request.Id);
            if (user == null)
                return null;

            return _mapper.Map<UserDto>(user);
        }
    }
}
