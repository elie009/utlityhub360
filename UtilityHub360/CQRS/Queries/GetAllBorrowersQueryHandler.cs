using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Handler for getting all borrowers
    /// </summary>
    public class GetAllBorrowersQueryHandler : IRequestHandler<GetAllBorrowersQuery, List<BorrowerDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllBorrowersQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<BorrowerDto>> Handle(GetAllBorrowersQuery request)
        {
            var borrowers = _context.Borrowers.ToList();
            return await Task.FromResult(_mapper.Map<List<BorrowerDto>>(borrowers));
        }
    }
}