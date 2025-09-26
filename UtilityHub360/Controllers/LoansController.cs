using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UtilityHub360.Data;
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
        private readonly ApplicationDbContext _context;

        public LoansController(ILoanService loanService, ApplicationDbContext context)
        {
            _loanService = loanService;
            _context = context;
        }

        [HttpPost("apply")]
        public async Task<ActionResult<object>> ApplyForLoan([FromBody] CreateLoanApplicationDto application)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new
                    {
                        type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                        title = "Unauthorized",
                        status = 401,
                        detail = "User not authenticated",
                        traceId = HttpContext.TraceIdentifier
                    });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Ok(new
                    {
                        type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        title = "One or more validation errors occurred.",
                        status = 400,
                        errors = errors,
                        traceId = HttpContext.TraceIdentifier
                    });
                }

                var result = await _loanService.ApplyForLoanAsync(application, userId);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = result.Data,
                        message = result.Message,
                        status = 200
                    });
                }
                
                return Ok(new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "Loan application failed",
                    status = 400,
                    detail = result.Message,
                    errors = result.Errors,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "Internal server error",
                    status = 500,
                    detail = $"Failed to apply for loan: {ex.Message}",
                    traceId = HttpContext.TraceIdentifier
                });
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

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<LoanDto>>>> GetAllLoans(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var query = _context.Loans.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(l => l.Status == status);
                }

                var totalCount = await query.CountAsync();
                var loans = await query
                    .OrderByDescending(l => l.AppliedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var loanDtos = loans.Select(loan => new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo
                }).ToList();

                var paginatedResponse = new PaginatedResponse<LoanDto>
                {
                    Data = loanDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return Ok(ApiResponse<PaginatedResponse<LoanDto>>.SuccessResult(paginatedResponse));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<LoanDto>>.ErrorResult($"Failed to get loans: {ex.Message}"));
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

        [HttpPut("{loanId}")]
        public async Task<ActionResult<ApiResponse<LoanDto>>> UpdateLoan(string loanId, [FromBody] UpdateLoanDto updateLoanDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<LoanDto>.ErrorResult("User not authenticated"));
                }

                // Debug: Check if user role is being detected correctly
                // For now, let's also check if the user exists in database and get their role
                var user = await _context.Users.FindAsync(userId);
                var dbUserRole = user?.Role;

                // Get the loan with access check
                var loan = await _loanService.GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return NotFound(ApiResponse<LoanDto>.ErrorResult("Loan not found"));
                }

                // Use database role as fallback if JWT role is not available
                var effectiveRole = !string.IsNullOrEmpty(userRole) ? userRole : dbUserRole;

                // Check if user can update this loan
                if (effectiveRole != "ADMIN" && loan.UserId != userId)
                {
                    return Forbid("You can only update your own loans");
                }

                // Update loan properties
                if (!string.IsNullOrEmpty(updateLoanDto.Purpose))
                {
                    loan.Purpose = updateLoanDto.Purpose;
                }

                if (!string.IsNullOrEmpty(updateLoanDto.AdditionalInfo))
                {
                    loan.AdditionalInfo = updateLoanDto.AdditionalInfo;
                }

                // All users can update status
                if (!string.IsNullOrEmpty(updateLoanDto.Status))
                {
                    loan.Status = updateLoanDto.Status;
                }

                // Only admin can update financial details
                if (effectiveRole == "ADMIN")
                {
                    if (updateLoanDto.InterestRate.HasValue)
                    {
                        loan.InterestRate = updateLoanDto.InterestRate.Value;
                    }

                    if (updateLoanDto.MonthlyPayment.HasValue)
                    {
                        loan.MonthlyPayment = updateLoanDto.MonthlyPayment.Value;
                    }

                    if (updateLoanDto.RemainingBalance.HasValue)
                    {
                        loan.RemainingBalance = updateLoanDto.RemainingBalance.Value;
                    }
                }

                await _context.SaveChangesAsync();

                var loanDto = new LoanDto
                {
                    Id = loan.Id,
                    UserId = loan.UserId,
                    Principal = loan.Principal,
                    InterestRate = loan.InterestRate,
                    Term = loan.Term,
                    Purpose = loan.Purpose,
                    Status = loan.Status,
                    MonthlyPayment = loan.MonthlyPayment,
                    TotalAmount = loan.TotalAmount,
                    RemainingBalance = loan.RemainingBalance,
                    AppliedAt = loan.AppliedAt,
                    ApprovedAt = loan.ApprovedAt,
                    DisbursedAt = loan.DisbursedAt,
                    CompletedAt = loan.CompletedAt,
                    AdditionalInfo = loan.AdditionalInfo
                };

                return Ok(ApiResponse<LoanDto>.SuccessResult(loanDto, "Loan updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoanDto>.ErrorResult($"Failed to update loan: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a loan
        /// </summary>
        [HttpDelete("{loanId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteLoan(string loanId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _loanService.DeleteLoanAsync(loanId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete loan: {ex.Message}"));
            }
        }

        /// <summary>
        /// Make a payment for a specific loan
        /// </summary>
        [HttpPost("{loanId}/payment")]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> MakeLoanPayment(string loanId, [FromBody] CreatePaymentDto payment)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaymentDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _loanService.MakeLoanPaymentAsync(loanId, payment, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResult($"Failed to process payment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get total outstanding loan amount for the authenticated user
        /// </summary>
        [HttpGet("outstanding-amount")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalOutstandingLoanAmount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _loanService.GetTotalOutstandingLoanAmountAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total outstanding loan amount: {ex.Message}"));
            }
        }
    }
}

