using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.DTOs;
using UtilityHub360.CQRS.Commands.CreateLoanApplication;
using UtilityHub360.CQRS.Queries.GetLoanById;
using UtilityHub360.CQRS.Queries.GetUserLoans;
using UtilityHub360.CQRS.Queries.GetLoanStatus;
using UtilityHub360.CQRS.Queries.GetLoanSchedule;
using UtilityHub360.CQRS.Queries.GetLoanTransactions;
using UtilityHub360.CQRS.Commands.ApproveLoan;
using UtilityHub360.CQRS.Commands.RejectLoan;
using UtilityHub360.CQRS.Commands.DisburseLoan;
using UtilityHub360.CQRS.Commands.CloseLoan;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/loans")]
    public class LoansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Apply for a new loan
        /// </summary>
        [HttpPost("apply")]
        [ProducesResponseType(typeof(LoanDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApplyForLoan([FromBody] LoanApplicationDto application)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new CreateLoanApplicationCommand
                {
                    UserId = 1, // TODO: Get from authenticated user
                    Principal = application.Principal,
                    Purpose = application.Purpose,
                    Term = application.Term,
                    MonthlyIncome = application.MonthlyIncome,
                    EmploymentStatus = application.EmploymentStatus,
                    AdditionalInfo = application.AdditionalInfo
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetLoan), new { loanId = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get loan details
        /// </summary>
        [HttpGet("{loanId}")]
        [ProducesResponseType(typeof(LoanDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLoan(int loanId)
        {
            try
            {
                var query = new GetLoanByIdQuery { Id = loanId };
                var loan = await _mediator.Send(query);
                return Ok(loan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get all loans for a user
        /// </summary>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<LoanDto>), 200)]
        public async Task<IActionResult> GetUserLoans(int userId, [FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var query = new GetUserLoansQuery
                {
                    UserId = userId,
                    Status = status,
                    Page = page,
                    Limit = limit
                };

                var loans = await _mediator.Send(query);
                return Ok(loans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get loan status and outstanding balance
        /// </summary>
        [HttpGet("{loanId}/status")]
        [ProducesResponseType(typeof(LoanStatusDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLoanStatus(int loanId)
        {
            try
            {
                var query = new GetLoanStatusQuery { LoanId = loanId };
                var status = await _mediator.Send(query);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get repayment schedule for a loan
        /// </summary>
        [HttpGet("{loanId}/schedule")]
        [ProducesResponseType(typeof(IEnumerable<RepaymentScheduleDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLoanSchedule(int loanId)
        {
            try
            {
                var query = new GetLoanScheduleQuery { LoanId = loanId };
                var schedule = await _mediator.Send(query);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get transaction history for a loan
        /// </summary>
        [HttpGet("{loanId}/transactions")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLoanTransactions(int loanId)
        {
            try
            {
                var query = new GetLoanTransactionsQuery { LoanId = loanId };
                var transactions = await _mediator.Send(query);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Approve a loan (Admin only)
        /// </summary>
        [HttpPut("{loanId}/approve")]
        [ProducesResponseType(typeof(LoanDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ApproveLoan(int loanId, [FromBody] ApproveLoanRequest request)
        {
            try
            {
                var command = new ApproveLoanCommand
                {
                    LoanId = loanId,
                    ApprovedBy = request.ApprovedBy,
                    Notes = request.Notes
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Reject a loan (Admin only)
        /// </summary>
        [HttpPut("{loanId}/reject")]
        [ProducesResponseType(typeof(LoanDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RejectLoan(int loanId, [FromBody] RejectLoanRequest request)
        {
            try
            {
                var command = new RejectLoanCommand
                {
                    LoanId = loanId,
                    Reason = request.Reason,
                    RejectedBy = request.RejectedBy,
                    Notes = request.Notes
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Close a loan (Admin only)
        /// </summary>
        [HttpPut("{loanId}/close")]
        [ProducesResponseType(typeof(LoanDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CloseLoan(int loanId, [FromBody] CloseLoanRequest request)
        {
            try
            {
                var command = new CloseLoanCommand
                {
                    LoanId = loanId,
                    ClosedBy = request.ClosedBy,
                    Notes = request.Notes
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class ApproveLoanRequest
    {
        public string ApprovedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class RejectLoanRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string RejectedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CloseLoanRequest
    {
        public string ClosedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class LoanStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; }
    }
}

