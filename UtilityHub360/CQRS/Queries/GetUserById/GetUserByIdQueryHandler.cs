using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;

namespace UtilityHub360.CQRS.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            return _mapper.Map<UserDto>(user);
        }
    }
}