using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.CQRS.Commands;
using UtilityHub360.CQRS.Queries;
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

        #region Borrower Management

        /// <summary>
        /// Get all borrowers
        /// </summary>
        /// <param name="status">Filter by borrower status</param>
        /// <param name="creditScoreMin">Minimum credit score filter</param>
        /// <param name="creditScoreMax">Maximum credit score filter</param>
        /// <returns>List of borrowers</returns>
        [HttpGet("borrowers")]
        [ProducesResponseType(typeof(IEnumerable<BorrowerDto>), 200)]
        public async Task<ActionResult<IEnumerable<BorrowerDto>>> GetBorrowers(string? status = null, int? creditScoreMin = null, int? creditScoreMax = null)
        {
            try
            {
                var query = new GetAllBorrowersQuery
                {
                    Status = status,
                    CreditScoreMin = creditScoreMin,
                    CreditScoreMax = creditScoreMax
                };

                var borrowers = await _mediator.Send(query);
                return Ok(borrowers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Create a new borrower
        /// </summary>
        /// <param name="createBorrowerDto">Borrower details</param>
        /// <returns>Created borrower</returns>
        [HttpPost("borrowers")]
        [ProducesResponseType(typeof(BorrowerDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<BorrowerDto>> CreateBorrower([FromBody] CreateBorrowerDto createBorrowerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new CreateBorrowerCommand
                {
                    FirstName = createBorrowerDto.FirstName,
                    LastName = createBorrowerDto.LastName,
                    Email = createBorrowerDto.Email,
                    Phone = createBorrowerDto.Phone,
                    Address = createBorrowerDto.Address,
                    GovernmentId = createBorrowerDto.GovernmentId,
                    Status = createBorrowerDto.Status ?? "Active"
                };
                var borrower = await _mediator.Send(command);

                return CreatedAtAction(nameof(GetBorrowers), new { id = borrower.BorrowerId }, borrower);
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
        /// <param name="loanType">Filter by loan type</param>
        /// <param name="borrowerId">Filter by borrower ID</param>
        /// <param name="isOverdue">Filter for overdue loans</param>
        /// <returns>List of loans</returns>
        [HttpGet("loans")]
        [ProducesResponseType(typeof(IEnumerable<LoanDto>), 200)]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans(string? status = null, string? loanType = null, int? borrowerId = null, bool? isOverdue = null)
        {
            try
            {
                var query = new GetAllLoansQuery
                {
                    Status = status,
                    LoanType = loanType,
                    BorrowerId = borrowerId,
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
        /// Create a new loan
        /// </summary>
        /// <param name="createLoanDto">Loan details</param>
        /// <returns>Created loan with repayment schedule</returns>
        [HttpPost("loans")]
        [ProducesResponseType(typeof(LoanDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<LoanDto>> CreateLoan([FromBody] CreateLoanDto createLoanDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new CreateLoanCommand
                {
                    BorrowerId = createLoanDto.BorrowerId,
                    LoanType = createLoanDto.LoanType,
                    PrincipalAmount = createLoanDto.PrincipalAmount,
                    InterestRate = createLoanDto.InterestRate,
                    TermMonths = createLoanDto.TermMonths,
                    RepaymentFrequency = createLoanDto.RepaymentFrequency,
                    AmortizationType = createLoanDto.AmortizationType,
                    StartDate = createLoanDto.StartDate,
                    Status = createLoanDto.Status ?? "Active"
                };
                var loan = await _mediator.Send(command);

                return CreatedAtAction(nameof(GetLoans), new { id = loan.LoanId }, loan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Payment Management

        /// <summary>
        /// Record a payment against a loan
        /// </summary>
        /// <param name="createPaymentDto">Payment details</param>
        /// <returns>Recorded payment</returns>
        [HttpPost("payments")]
        [ProducesResponseType(typeof(PaymentDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaymentDto>> RecordPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new RecordPaymentCommand
                {
                    LoanId = createPaymentDto.LoanId,
                    ScheduleId = createPaymentDto.ScheduleId,
                    PaymentDate = createPaymentDto.PaymentDate,
                    AmountPaid = createPaymentDto.AmountPaid,
                    PaymentMethod = createPaymentDto.PaymentMethod,
                    Notes = createPaymentDto.Notes
                };
                var payment = await _mediator.Send(command);

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Reports & Analytics

        /// <summary>
        /// Get loan portfolio summary
        /// </summary>
        /// <param name="branch">Filter by branch (optional)</param>
        /// <returns>Loan portfolio summary</returns>
        [HttpGet("portfolio")]
        [ProducesResponseType(typeof(LoanPortfolioDto), 200)]
        public async Task<ActionResult<LoanPortfolioDto>> GetLoanPortfolio(string? branch = null)
        {
            try
            {
                var query = new GetLoanPortfolioQuery
                {
                    Branch = branch
                };

                var portfolio = await _mediator.Send(query);
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion
    }
}
