using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.DependencyInjection;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for creating a new borrower
    /// </summary>
    public class CreateBorrowerCommandHandler : IRequestHandler<CreateBorrowerCommand, BorrowerDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public CreateBorrowerCommandHandler()
        {
            var container = ServiceContainer.CreateDefault();
            _context = container.GetService<UtilityHubDbContext>();
            _mapper = container.GetService<IMapper>();
        }

        public async Task<BorrowerDto> Handle(CreateBorrowerCommand request)
        {
            var borrower = new Borrower
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                GovernmentId = request.GovernmentId,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Borrowers.Add(borrower);
            await _context.SaveChangesAsync();

            return _mapper.Map<BorrowerDto>(borrower);
        }
    }
}