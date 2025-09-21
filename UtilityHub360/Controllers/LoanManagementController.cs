using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.CQRS.Commands.MakePayment;
using UtilityHub360.CQRS.Queries;
using UtilityHub360.CQRS.Queries.GetAllLoans;
using UtilityHub360.CQRS.Queries.GetLoanById;
using UtilityHub360.CQRS.Queries.GetLoanPayments;
using UtilityHub360.DTOs;

namespace UtilityHub360.Controllers
{
    /// <summary>
    /// API Controller for Loan Management System
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoanManagementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region User Management

        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="role">Filter by user role</param>
        /// <param name="isActive">Filter by active status</param>
        /// <returns>List of users</returns>
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(string? role = null, bool? isActive = null)
        {
            try
            {
                var query = new GetAllUsersQuery
                {
                    Role = role,
                    IsActive = isActive
                };

                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Loan Management

        /// <summary>
        /// Get all loans
        /// </summary>
        /// <param name="status">Filter by loan status</param>
        /// <param name="userId">Filter by user ID</param>
        /// <param name="isOverdue">Filter for overdue loans</param>
        /// <returns>List of loans</returns>
        [HttpGet("loans")]
        [ProducesResponseType(typeof(IEnumerable<LoanDto>), 200)]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans(string? status = null, int? userId = null, bool? isOverdue = null)
        {
            try
            {
                var query = new GetAllLoansQuery
                {
                    Status = status,
                    UserId = userId,
                    IsOverdue = isOverdue
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
        /// Get loan by ID
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <returns>Loan details</returns>
        [HttpGet("loans/{id}")]
        [ProducesResponseType(typeof(LoanDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LoanDto>> GetLoanById(int id)
        {
            try
            {
                var query = new GetLoanByIdQuery { Id = id };
                var loan = await _mediator.Send(query);
                return Ok(loan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Payment Management

        /// <summary>
        /// Make a payment against a loan
        /// </summary>
        /// <param name="paymentDto">Payment details</param>
        /// <returns>Recorded payment</returns>
        [HttpPost("payments")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaymentDto>> MakePayment([FromBody] PaymentDto paymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new MakePaymentCommand
                {
                    LoanId = paymentDto.LoanId,
                    UserId = 1, // TODO: Get from authenticated user
                    Amount = paymentDto.Amount,
                    Method = paymentDto.Method,
                    Reference = paymentDto.Reference
                };
                var payment = await _mediator.Send(command);

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get all payments for a loan
        /// </summary>
        /// <param name="loanId">Loan ID</param>
        /// <returns>List of payments</returns>
        [HttpGet("loans/{loanId}/payments")]
        [ProducesResponseType(typeof(IEnumerable<PaymentDto>), 200)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetLoanPayments(int loanId)
        {
            try
            {
                var query = new GetLoanPaymentsQuery { LoanId = loanId };
                var payments = await _mediator.Send(query);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Reports & Analytics


        #endregion
    }
}
