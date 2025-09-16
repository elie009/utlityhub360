using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using UtilityHub360.CQRS.Commands;
using UtilityHub360.CQRS.MediatR;
using UtilityHub360.CQRS.Queries;
using UtilityHub360.DTOs;
using UtilityHub360.DependencyInjection;

namespace UtilityHub360.Controllers
{
    /// <summary>
    /// API Controller for Loan Management System
    /// </summary>
    [RoutePrefix("api/loan-management")]
    public class LoanManagementController : ApiController
    {
        private readonly IMediator _mediator;

        public LoanManagementController()
        {
            _mediator = ServiceContainer.CreateDefault().GetService<IMediator>();
        }

        #region Borrower Management

        /// <summary>
        /// Get all borrowers
        /// </summary>
        /// <param name="status">Filter by borrower status</param>
        /// <param name="creditScoreMin">Minimum credit score filter</param>
        /// <param name="creditScoreMax">Maximum credit score filter</param>
        /// <returns>List of borrowers</returns>
        [HttpGet]
        [Route("borrowers")]
        [ResponseType(typeof(IEnumerable<BorrowerDto>))]
        public async Task<IHttpActionResult> GetBorrowers(string status = null, int? creditScoreMin = null, int? creditScoreMax = null)
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
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a new borrower
        /// </summary>
        /// <param name="createBorrowerDto">Borrower details</param>
        /// <returns>Created borrower</returns>
        [HttpPost]
        [Route("borrowers")]
        [ResponseType(typeof(BorrowerDto))]
        public async Task<IHttpActionResult> CreateBorrower([FromBody] CreateBorrowerDto createBorrowerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new CreateBorrowerCommand(createBorrowerDto);
                var borrower = await _mediator.Send(command);

                return CreatedAtRoute("DefaultApi", new { id = borrower.BorrowerId }, borrower);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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
        [HttpGet]
        [Route("loans")]
        [ResponseType(typeof(IEnumerable<LoanDto>))]
        public async Task<IHttpActionResult> GetLoans(string status = null, string loanType = null, int? borrowerId = null, bool? isOverdue = null)
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
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a new loan
        /// </summary>
        /// <param name="createLoanDto">Loan details</param>
        /// <returns>Created loan with repayment schedule</returns>
        [HttpPost]
        [Route("loans")]
        [ResponseType(typeof(LoanDto))]
        public async Task<IHttpActionResult> CreateLoan([FromBody] CreateLoanDto createLoanDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new CreateLoanCommand(createLoanDto);
                var loan = await _mediator.Send(command);

                return CreatedAtRoute("DefaultApi", new { id = loan.LoanId }, loan);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region Payment Management

        /// <summary>
        /// Record a payment against a loan
        /// </summary>
        /// <param name="createPaymentDto">Payment details</param>
        /// <returns>Recorded payment</returns>
        [HttpPost]
        [Route("payments")]
        [ResponseType(typeof(PaymentDto))]
        public async Task<IHttpActionResult> RecordPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new RecordPaymentCommand(createPaymentDto);
                var payment = await _mediator.Send(command);

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region Reports & Analytics

        /// <summary>
        /// Get loan portfolio summary
        /// </summary>
        /// <param name="branch">Filter by branch (optional)</param>
        /// <returns>Loan portfolio summary</returns>
        [HttpGet]
        [Route("portfolio")]
        [ResponseType(typeof(LoanPortfolioDto))]
        public async Task<IHttpActionResult> GetLoanPortfolio(string branch = null)
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
                return InternalServerError(ex);
            }
        }

        #endregion
    }
}
