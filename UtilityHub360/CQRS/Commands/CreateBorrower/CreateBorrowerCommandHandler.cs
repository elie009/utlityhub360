using MediatR;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using AutoMapper;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Handler for creating a new borrower
    /// </summary>
    public class CreateBorrowerCommandHandler : IRequestHandler<CreateBorrowerCommand, BorrowerDto>
    {
        private readonly UtilityHubDbContext _context;
        private readonly IMapper _mapper;

        public CreateBorrowerCommandHandler(UtilityHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BorrowerDto> Handle(CreateBorrowerCommand request, CancellationToken cancellationToken)
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
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<BorrowerDto>(borrower);
        }
    }
}