using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.DTOs;
using UtilityHub360.CQRS.Queries.GetUserReport;
using UtilityHub360.CQRS.Queries.GetLoanReport;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get user financial report
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(UserReportDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserReport(int userId, [FromQuery] string? period = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = new GetUserReportQuery
                {
                    UserId = userId,
                    Period = period,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var report = await _mediator.Send(query);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get loan detailed report
        /// </summary>
        [HttpGet("loan/{loanId}")]
        [ProducesResponseType(typeof(LoanReportDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLoanReport(int loanId)
        {
            try
            {
                var query = new GetLoanReportQuery { LoanId = loanId };
                var report = await _mediator.Send(query);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class UserReportDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal TotalBorrowed { get; set; }
        public decimal TotalRepaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int ActiveLoans { get; set; }
        public int CompletedLoans { get; set; }
        public DateTime ReportDate { get; set; }
    }

    public class LoanReportDto
    {
        public int LoanId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal MonthlyPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public int PaymentsMade { get; set; }
        public int PaymentsRemaining { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}

