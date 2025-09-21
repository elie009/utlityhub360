using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.DTOs;
using UtilityHub360.CQRS.Commands.MakePayment;
using UtilityHub360.CQRS.Queries.GetPaymentById;
using UtilityHub360.CQRS.Queries.GetLoanPayments;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Make a payment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> MakePayment([FromBody] PaymentDto payment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new MakePaymentCommand
                {
                    LoanId = payment.LoanId,
                    UserId = 1, // TODO: Get from authenticated user
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Reference = payment.Reference
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
        /// Get payment details
        /// </summary>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPayment(int paymentId)
        {
            try
            {
                var query = new GetPaymentByIdQuery { Id = paymentId };
                var payment = await _mediator.Send(query);
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
        [HttpGet("loans/{loanId}")]
        [ProducesResponseType(typeof(IEnumerable<PaymentDto>), 200)]
        public async Task<IActionResult> GetLoanPayments(int loanId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var query = new GetLoanPaymentsQuery
                {
                    LoanId = loanId,
                    Page = page,
                    Limit = limit
                };

                var payments = await _mediator.Send(query);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

