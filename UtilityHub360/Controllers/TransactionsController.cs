using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.DTOs;
using UtilityHub360.CQRS.Commands.DisburseLoan;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Disburse a loan (Admin only)
        /// </summary>
        [HttpPost("disburse")]
        [ProducesResponseType(typeof(DisbursementDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DisburseLoan([FromBody] DisburseLoanRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new DisburseLoanCommand
                {
                    LoanId = request.LoanId,
                    DisbursedBy = request.DisbursedBy,
                    DisbursementMethod = request.DisbursementMethod,
                    Reference = request.Reference
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

    public class DisburseLoanRequest
    {
        public int LoanId { get; set; }
        public string DisbursedBy { get; set; } = string.Empty;
        public string DisbursementMethod { get; set; } = string.Empty;
        public string? Reference { get; set; }
    }

    public class DisbursementDto
    {
        public int TransactionId { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}

