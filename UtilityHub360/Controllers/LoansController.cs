using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost("apply")]
        public async Task<ActionResult<ApiResponse<LoanDto>>> ApplyForLoan([FromBody] CreateLoanApplicationDto application)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<LoanDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<LoanDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _loanService.ApplyForLoanAsync(application, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoanDto>.ErrorResult($"Failed to apply for loan: {ex.Message}"));
            }
        }

        [HttpGet("{loanId}")]
        public async Task<ActionResult<ApiResponse<LoanDto>>> GetLoan(string loanId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<LoanDto>.ErrorResult("User not authenticated"));
                }

                var result = await _loanService.GetLoanAsync(loanId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoanDto>.ErrorResult($"Failed to get loan: {ex.Message}"));
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<LoanDto>>>> GetUserLoans(
            string userId,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only view their own loans unless they're admin
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                var result = await _loanService.GetUserLoansAsync(userId, status, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<LoanDto>>.ErrorResult($"Failed to get user loans: {ex.Message}"));
            }
        }

        [HttpGet("{loanId}/status")]
        public async Task<ActionResult<ApiResponse<object>>> GetLoanStatus(string loanId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("User not authenticated"));
                }

                var result = await _loanService.GetLoanStatusAsync(loanId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to get loan status: {ex.Message}"));
            }
        }

        [HttpGet("{loanId}/schedule")]
        public async Task<ActionResult<ApiResponse<List<RepaymentScheduleDto>>>> GetRepaymentSchedule(string loanId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<RepaymentScheduleDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _loanService.GetRepaymentScheduleAsync(loanId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<RepaymentScheduleDto>>.ErrorResult($"Failed to get repayment schedule: {ex.Message}"));
            }
        }

        [HttpGet("{loanId}/transactions")]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetLoanTransactions(string loanId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TransactionDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _loanService.GetLoanTransactionsAsync(loanId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionDto>>.ErrorResult($"Failed to get loan transactions: {ex.Message}"));
            }
        }
    }
}
