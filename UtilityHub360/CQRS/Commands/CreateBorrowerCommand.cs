using System;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Commands
{
    /// <summary>
    /// Command to create a new borrower
    /// </summary>
    public class CreateBorrowerCommand : IRequest<BorrowerDto>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string GovernmentId { get; set; }
        public string Status { get; set; }

        public CreateBorrowerCommand()
        {
            Status = "Active";
        }
    }
}