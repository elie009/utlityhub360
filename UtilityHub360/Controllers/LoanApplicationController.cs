using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.DTOs;
using UtilityHub360.CQRS.Queries.GetAllLoanApplications;
using UtilityHub360.CQRS.Queries.GetLoanApplicationById;
using UtilityHub360.CQRS.Commands.CreateLoanApplication;
using UtilityHub360.CQRS.Commands.ApproveLoanApplication;
using UtilityHub360.CQRS.Commands.RejectLoanApplication;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanApplicationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanApplicationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all loan applications
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LoanApplicationDto>), 200)]
        public async Task<IActionResult> GetAllLoanApplications()
        {
            var query = new GetAllLoanApplicationsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get loan application by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LoanApplicationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLoanApplicationById(int id)
        {
            var query = new GetLoanApplicationByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Submit a new loan application
        /// </summary>
        [HttpPost("apply")]
        [ProducesResponseType(typeof(LoanApplicationDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApplyForLoan([FromBody] LoanApplicationDto applicationDto)
        {
            var command = new CreateLoanApplicationCommand
            {
                UserId = 1, // TODO: Get from authenticated user
                Principal = applicationDto.Principal,
                Purpose = applicationDto.Purpose,
                Term = applicationDto.Term,
                MonthlyIncome = applicationDto.MonthlyIncome,
                EmploymentStatus = applicationDto.EmploymentStatus,
                AdditionalInfo = applicationDto.AdditionalInfo
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetLoanApplicationById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Approve a loan application (Admin only)
        /// </summary>
        [HttpPut("{id}/approve")]
        [ProducesResponseType(typeof(LoanApplicationDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ApproveLoanApplication(int id, [FromBody] ApproveLoanApplicationRequest request)
        {
            var command = new ApproveLoanApplicationCommand
            {
                ApplicationId = id,
                ReviewedBy = request.ReviewedBy,
                Notes = request.Notes
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Reject a loan application (Admin only)
        /// </summary>
        [HttpPut("{id}/reject")]
        [ProducesResponseType(typeof(LoanApplicationDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RejectLoanApplication(int id, [FromBody] RejectLoanApplicationRequest request)
        {
            var command = new RejectLoanApplicationCommand
            {
                ApplicationId = id,
                RejectionReason = request.RejectionReason,
                RejectedBy = request.RejectedBy,
                Notes = request.Notes
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

    public class ApproveLoanApplicationRequest
    {
        public string ReviewedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class RejectLoanApplicationRequest
    {
        public string RejectionReason { get; set; } = string.Empty;
        public string RejectedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

