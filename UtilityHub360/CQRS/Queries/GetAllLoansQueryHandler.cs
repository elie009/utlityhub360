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
    /// Handler for getting all loans
    /// </summary>
    public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, List<LoanDto>>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public GetAllLoansQueryHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<LoanDto>> Handle(GetAllLoansQuery request)
        {
            var loans = _context.Loans
                .Include("Borrower")
                .ToList();
            return await Task.FromResult(_mapper.Map<List<LoanDto>>(loans));
        }
    }
}