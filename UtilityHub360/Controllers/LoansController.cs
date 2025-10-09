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
                        status = 200,
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
                        status = 200,
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
                    status = 200,
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
                    status = 200,
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
                    // NextDueDate = loan.NextDueDate,
                    // FinalDueDate = loan.FinalDueDate
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

                // ============================================
                // AUTOMATED FINANCIAL UPDATE LOGIC
                // ============================================
                // Frontend: Just send the values you want to update
                // Backend: Automatically calculates everything else
                // ============================================

                Console.WriteLine($"[DEBUG] === LOAN UPDATE START ===");
                Console.WriteLine($"[DEBUG] Loan ID: {loanId}");
                Console.WriteLine($"[DEBUG] Request - Principal: {updateLoanDto.Principal?.ToString() ?? "null"}");
                Console.WriteLine($"[DEBUG] Request - InterestRate: {updateLoanDto.InterestRate?.ToString() ?? "null"}");
                Console.WriteLine($"[DEBUG] Request - MonthlyPayment: {updateLoanDto.MonthlyPayment?.ToString() ?? "null"}");
                Console.WriteLine($"[DEBUG] Request - RemainingBalance: {updateLoanDto.RemainingBalance?.ToString() ?? "null"}");
                Console.WriteLine($"[DEBUG] Current - Principal: {loan.Principal}");
                Console.WriteLine($"[DEBUG] Current - MonthlyPayment: {loan.MonthlyPayment}");
                Console.WriteLine($"[DEBUG] Current - TotalAmount: {loan.TotalAmount}");
                Console.WriteLine($"[DEBUG] Current - RemainingBalance: {loan.RemainingBalance}");

                bool financialValuesChanged = false;
                
                // Store old values BEFORE making any changes
                decimal oldPrincipal = loan.Principal;
                decimal oldRemainingBalance = loan.RemainingBalance;
                decimal oldTotalAmount = loan.TotalAmount;

                // 1. Update Principal if provided (triggers full recalculation)
                if (updateLoanDto.Principal.HasValue)
                {
                    loan.Principal = updateLoanDto.Principal.Value;
                    financialValuesChanged = true;
                    Console.WriteLine($"[UPDATE] Principal: {oldPrincipal} -> {loan.Principal}");
                }

                // 2. Update Interest Rate if provided
                if (updateLoanDto.InterestRate.HasValue)
                {
                    loan.InterestRate = updateLoanDto.InterestRate.Value;
                    financialValuesChanged = true;
                    Console.WriteLine($"[UPDATE] Interest Rate: {loan.InterestRate}%");
                }

                Console.WriteLine($"[DEBUG] financialValuesChanged after Principal/InterestRate: {financialValuesChanged}");

                // 3. Update Monthly Payment if provided (manual override)
                if (updateLoanDto.MonthlyPayment.HasValue)
                {
                    loan.MonthlyPayment = updateLoanDto.MonthlyPayment.Value;
                    financialValuesChanged = true;
                    Console.WriteLine($"[UPDATE] Monthly Payment (Manual): {loan.MonthlyPayment}");
                }
                // If principal or interest rate changed but no manual payment, calculate it
                else if (updateLoanDto.Principal.HasValue || updateLoanDto.InterestRate.HasValue)
                {
                    Console.WriteLine($"[DEBUG] Calculating monthly payment...");
                    Console.WriteLine($"[DEBUG] Parameters - Principal: {loan.Principal}, InterestRate: {loan.InterestRate}, Term: {loan.Term}");
                    loan.MonthlyPayment = CalculateMonthlyPayment(loan.Principal, loan.InterestRate, loan.Term);
                    Console.WriteLine($"[UPDATE] Monthly Payment (Calculated): {loan.MonthlyPayment}");
                    financialValuesChanged = true;  // Make sure this is set!
                }

                Console.WriteLine($"[DEBUG] financialValuesChanged after MonthlyPayment: {financialValuesChanged}");

                // 4. Recalculate Total Amount based on monthly payment and term
                if (financialValuesChanged)
                {
                    loan.TotalAmount = loan.MonthlyPayment * loan.Term;
                    Console.WriteLine($"[UPDATE] Total Amount: {loan.TotalAmount}");
                }

                // 5. Update Remaining Balance if provided (manual override)
                if (updateLoanDto.RemainingBalance.HasValue)
                {
                    loan.RemainingBalance = updateLoanDto.RemainingBalance.Value;
                    Console.WriteLine($"[UPDATE] Remaining Balance (Manual): {loan.RemainingBalance}");
                }
                // If financial values changed but no manual remaining balance, recalculate it
                else if (financialValuesChanged)
                {
                    // Use the old values we saved at the beginning
                    if (updateLoanDto.Principal.HasValue)
                    {
                        // Principal changed - check if this is a major increase or just a minor adjustment
                        // If old remaining balance was close to old principal, no payments were made
                        if (oldRemainingBalance >= oldPrincipal * 0.95m)
                        {
                            // No significant payments made - set to new total
                            loan.RemainingBalance = loan.TotalAmount;
                            Console.WriteLine($"[UPDATE] Remaining Balance (No Payments): {loan.RemainingBalance}");
                        }
                        else
                        {
                            // Payments have been made - calculate how much was paid
                            var paidAmount = oldTotalAmount - oldRemainingBalance;
                            
                            // Maintain the paid amount
                            loan.RemainingBalance = loan.TotalAmount - paidAmount;
                            
                            // Make sure remaining balance doesn't go negative
                            if (loan.RemainingBalance < 0)
                            {
                                loan.RemainingBalance = 0;
                            }
                            
                            Console.WriteLine($"[UPDATE] Remaining Balance (After {paidAmount} paid): {loan.RemainingBalance}");
                        }
                    }
                    else
                    {
                        // Interest rate changed but not principal
                        if (loan.RemainingBalance >= loan.Principal)
                        {
                            // No payments made yet - set to new total amount
                            loan.RemainingBalance = loan.TotalAmount;
                            Console.WriteLine($"[UPDATE] Remaining Balance (No Payments): {loan.RemainingBalance}");
                        }
                        else
                        {
                            // Payments have been made - maintain the same paid amount
                            var paidAmount = loan.TotalAmount - loan.RemainingBalance;
                            loan.RemainingBalance = loan.TotalAmount - paidAmount;
                            Console.WriteLine($"[UPDATE] Remaining Balance (After {paidAmount} paid): {loan.RemainingBalance}");
                        }
                    }
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] === LOAN UPDATE END ===");
                Console.WriteLine($"[DEBUG] Final - Principal: {loan.Principal}");
                Console.WriteLine($"[DEBUG] Final - MonthlyPayment: {loan.MonthlyPayment}");
                Console.WriteLine($"[DEBUG] Final - TotalAmount: {loan.TotalAmount}");
                Console.WriteLine($"[DEBUG] Final - RemainingBalance: {loan.RemainingBalance}");

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
                    // NextDueDate = loan.NextDueDate,
                    // FinalDueDate = loan.FinalDueDate
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

        /// <summary>
        /// Calculate loan values preview (without saving) - for frontend form validation
        /// </summary>
        [HttpPost("calculate-preview")]
        public ActionResult<ApiResponse<object>> CalculateLoanPreview([FromBody] CalculateLoanPreviewDto previewDto)
        {
            try
            {
                var monthlyPayment = CalculateMonthlyPayment(previewDto.Principal, previewDto.InterestRate, previewDto.Term);
                var totalAmount = monthlyPayment * previewDto.Term;
                
                var preview = new
                {
                    principal = previewDto.Principal,
                    interestRate = previewDto.InterestRate,
                    term = previewDto.Term,
                    monthlyPayment = Math.Round(monthlyPayment, 2),
                    totalAmount = Math.Round(totalAmount, 2),
                    totalInterest = Math.Round(totalAmount - previewDto.Principal, 2)
                };

                return Ok(ApiResponse<object>.SuccessResult(preview, "Loan calculation preview generated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to calculate loan preview: {ex.Message}"));
            }
        }

        /// <summary>
        /// Calculate updated loan values when changing interest rate
        /// </summary>
        [HttpPost("{loanId}/recalculate-preview")]
        public async Task<ActionResult<ApiResponse<object>>> RecalculateLoanPreview(string loanId, [FromBody] RecalculateLoanPreviewDto previewDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("User not authenticated"));
                }

                var loan = await _loanService.GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Loan not found"));
                }

                // Calculate new values based on new interest rate
                var newMonthlyPayment = CalculateMonthlyPayment(loan.Principal, previewDto.NewInterestRate, loan.Term);
                var newTotalAmount = newMonthlyPayment * loan.Term;
                
                // Calculate new remaining balance based on payments made
                decimal newRemainingBalance;
                if (loan.RemainingBalance >= loan.Principal)
                {
                    // No payments made yet
                    newRemainingBalance = newTotalAmount;
                }
                else
                {
                    // Payments have been made
                    var paidAmount = loan.Principal - loan.RemainingBalance;
                    newRemainingBalance = newTotalAmount - paidAmount;
                }

                var preview = new
                {
                    currentValues = new
                    {
                        principal = loan.Principal,
                        interestRate = loan.InterestRate,
                        term = loan.Term,
                        monthlyPayment = loan.MonthlyPayment,
                        totalAmount = loan.TotalAmount,
                        remainingBalance = loan.RemainingBalance
                    },
                    newValues = new
                    {
                        principal = loan.Principal,
                        interestRate = previewDto.NewInterestRate,
                        term = loan.Term,
                        monthlyPayment = Math.Round(newMonthlyPayment, 2),
                        totalAmount = Math.Round(newTotalAmount, 2),
                        remainingBalance = Math.Round(newRemainingBalance, 2),
                        totalInterest = Math.Round(newTotalAmount - loan.Principal, 2)
                    },
                    changes = new
                    {
                        monthlyPaymentChange = Math.Round(newMonthlyPayment - loan.MonthlyPayment, 2),
                        totalAmountChange = Math.Round(newTotalAmount - loan.TotalAmount, 2),
                        remainingBalanceChange = Math.Round(newRemainingBalance - loan.RemainingBalance, 2)
                    }
                };

                return Ok(ApiResponse<object>.SuccessResult(preview, "Loan recalculation preview generated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to calculate preview: {ex.Message}"));
            }
        }

        /// <summary>
        /// Calculate monthly payment using the standard loan payment formula
        /// </summary>
        private decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termInMonths)
        {
            if (annualInterestRate == 0)
            {
                return principal / termInMonths;
            }

            decimal monthlyInterestRate = annualInterestRate / 100 / 12;
            decimal monthlyPayment = principal * (monthlyInterestRate * (decimal)Math.Pow((double)(1 + monthlyInterestRate), termInMonths)) 
                                   / ((decimal)Math.Pow((double)(1 + monthlyInterestRate), termInMonths) - 1);

            return Math.Round(monthlyPayment, 2);
        }

        /// <summary>
        /// Get upcoming payments for the authenticated user
        /// </summary>
        [HttpGet("upcoming-payments")]
        public async Task<ActionResult<ApiResponse<List<UpcomingPaymentDto>>>> GetUpcomingPayments([FromQuery] int days = 30)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<UpcomingPaymentDto>>.ErrorResult("User not authenticated"));
                }

                var dueDateService = new LoanDueDateService(_context, null!); // NotificationService not needed for this
                var upcomingPayments = await dueDateService.GetUpcomingPaymentsForUserAsync(userId, days);

                return Ok(ApiResponse<List<UpcomingPaymentDto>>.SuccessResult(upcomingPayments));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UpcomingPaymentDto>>.ErrorResult($"Failed to get upcoming payments: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get overdue payments for the authenticated user
        /// </summary>
        [HttpGet("overdue-payments")]
        public async Task<ActionResult<ApiResponse<List<OverduePaymentDto>>>> GetOverduePayments()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<OverduePaymentDto>>.ErrorResult("User not authenticated"));
                }

                var dueDateService = new LoanDueDateService(_context, null!); // NotificationService not needed for this
                var overduePayments = await dueDateService.GetOverduePaymentsForUserAsync(userId);

                return Ok(ApiResponse<List<OverduePaymentDto>>.SuccessResult(overduePayments));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<OverduePaymentDto>>.ErrorResult($"Failed to get overdue payments: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get next due date for a specific loan
        /// </summary>
        [HttpGet("{loanId}/next-due-date")]
        public async Task<ActionResult<ApiResponse<DateTime?>>> GetNextDueDate(string loanId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<DateTime?>.ErrorResult("User not authenticated"));
                }

                // Verify user has access to this loan
                var loan = await _loanService.GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return NotFound(ApiResponse<DateTime?>.ErrorResult("Loan not found"));
                }

                var dueDateService = new LoanDueDateService(_context, null!);
                var nextDueDate = await dueDateService.GetNextDueDateAsync(loanId);

                return Ok(ApiResponse<DateTime?>.SuccessResult(nextDueDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<DateTime?>.ErrorResult($"Failed to get next due date: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update a repayment schedule due date (Admin or loan owner)
        /// </summary>
        [HttpPut("{loanId}/schedule/{installmentNumber}")]
        public async Task<ActionResult<ApiResponse<RepaymentScheduleDto>>> UpdateRepaymentScheduleDueDate(
            string loanId, 
            int installmentNumber, 
            [FromBody] UpdateRepaymentScheduleDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<RepaymentScheduleDto>.ErrorResult("User not authenticated"));
                }

                // Get the loan to check access
                var loan = await _loanService.GetLoanWithAccessCheckAsync(loanId, userId);
                if (loan == null)
                {
                    return NotFound(ApiResponse<RepaymentScheduleDto>.ErrorResult("Loan not found"));
                }

                // Get the repayment schedule entry
                var schedule = await _context.RepaymentSchedules
                    .FirstOrDefaultAsync(rs => rs.LoanId == loanId && rs.InstallmentNumber == installmentNumber);

                if (schedule == null)
                {
                    return NotFound(ApiResponse<RepaymentScheduleDto>.ErrorResult("Repayment schedule not found"));
                }

                // Check if already paid
                if (schedule.Status == "PAID")
                {
                    return BadRequest(ApiResponse<RepaymentScheduleDto>.ErrorResult("Cannot update a paid installment"));
                }

                // Update the due date
                schedule.DueDate = updateDto.NewDueDate;

                await _context.SaveChangesAsync();

                var scheduleDto = new RepaymentScheduleDto
                {
                    Id = schedule.Id,
                    LoanId = schedule.LoanId,
                    InstallmentNumber = schedule.InstallmentNumber,
                    DueDate = schedule.DueDate,
                    PrincipalAmount = schedule.PrincipalAmount,
                    InterestAmount = schedule.InterestAmount,
                    TotalAmount = schedule.TotalAmount,
                    Status = schedule.Status,
                    PaidAt = schedule.PaidAt
                };

                return Ok(ApiResponse<RepaymentScheduleDto>.SuccessResult(scheduleDto, "Repayment schedule updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RepaymentScheduleDto>.ErrorResult($"Failed to update repayment schedule: {ex.Message}"));
            }
        }
    }
}

