using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserReport(
            string userId,
            [FromQuery] string? period = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only view their own reports unless they're admin
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                // Set default date range if not provided
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    if (period?.ToLower() == "year")
                    {
                        startDate = DateTime.UtcNow.AddYears(-1);
                        endDate = DateTime.UtcNow;
                    }
                    else
                    {
                        startDate = DateTime.UtcNow.AddMonths(-6);
                        endDate = DateTime.UtcNow;
                    }
                }

                // Get user loans
                var loans = await _context.Loans
                    .Where(l => l.UserId == userId)
                    .ToListAsync();

                // Get user payments
                var payments = await _context.Payments
                    .Where(p => p.UserId == userId && p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                    .ToListAsync();

                // Calculate totals
                var totalLoans = loans.Count;
                var activeLoans = loans.Count(l => l.Status == "ACTIVE");
                var completedLoans = loans.Count(l => l.Status == "COMPLETED");
                var totalBorrowed = loans.Sum(l => l.Principal);
                var totalPaid = payments.Sum(p => p.Amount);
                var totalOutstanding = loans.Where(l => l.Status == "ACTIVE").Sum(l => l.RemainingBalance);

                var report = new
                {
                    userId,
                    period = new { startDate, endDate },
                    summary = new
                    {
                        totalLoans,
                        activeLoans,
                        completedLoans,
                        totalBorrowed,
                        totalPaid,
                        totalOutstanding
                    },
                    loans = loans.Select(l => new
                    {
                        id = l.Id,
                        principal = l.Principal,
                        status = l.Status,
                        appliedAt = l.AppliedAt,
                        remainingBalance = l.RemainingBalance
                    }),
                    recentPayments = payments.OrderByDescending(p => p.CreatedAt).Take(10).Select(p => new
                    {
                        id = p.Id,
                        amount = p.Amount,
                        method = p.Method,
                        createdAt = p.CreatedAt,
                        status = p.Status
                    })
                };

                return Ok(ApiResponse<object>.SuccessResult(report));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to generate user report: {ex.Message}"));
            }
        }

        [HttpGet("loan/{loanId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetLoanReport(string loanId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Get loan details
                var loan = await _context.Loans
                    .FirstOrDefaultAsync(l => l.Id == loanId);

                if (loan == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Loan not found"));
                }

                // Users can only view their own loan reports unless they're admin
                if (loan.UserId != currentUserId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                // Get repayment schedule
                var repaymentSchedule = await _context.RepaymentSchedules
                    .Where(rs => rs.LoanId == loanId)
                    .OrderBy(rs => rs.InstallmentNumber)
                    .ToListAsync();

                // Get payments
                var payments = await _context.Payments
                    .Where(p => p.LoanId == loanId)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();

                // Get loan activities (Payment records with IsBankTransaction = false)
                var transactions = await _context.Payments
                    .Where(p => p.LoanId == loanId && !p.IsBankTransaction)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Calculate payment summary
                var totalPaid = payments.Sum(p => p.Amount);
                var paidInstallments = repaymentSchedule.Count(rs => rs.Status == "PAID");
                var overdueInstallments = repaymentSchedule.Count(rs => rs.Status == "OVERDUE");
                var nextDueDate = repaymentSchedule
                    .Where(rs => rs.Status == "PENDING")
                    .OrderBy(rs => rs.DueDate)
                    .FirstOrDefault()?.DueDate;

                var report = new
                {
                    loan = new
                    {
                        id = loan.Id,
                        principal = loan.Principal,
                        interestRate = loan.InterestRate,
                        term = loan.Term,
                        purpose = loan.Purpose,
                        status = loan.Status,
                        monthlyPayment = loan.MonthlyPayment,
                        totalAmount = loan.TotalAmount,
                        remainingBalance = loan.RemainingBalance,
                        appliedAt = loan.AppliedAt,
                        approvedAt = loan.ApprovedAt,
                        disbursedAt = loan.DisbursedAt,
                        completedAt = loan.CompletedAt
                    },
                    paymentSummary = new
                    {
                        totalPaid,
                        paidInstallments,
                        overdueInstallments,
                        nextDueDate,
                        totalInstallments = repaymentSchedule.Count
                    },
                    repaymentSchedule = repaymentSchedule.Select(rs => new
                    {
                        installmentNumber = rs.InstallmentNumber,
                        dueDate = rs.DueDate,
                        principalAmount = rs.PrincipalAmount,
                        interestAmount = rs.InterestAmount,
                        totalAmount = rs.TotalAmount,
                        status = rs.Status,
                        paidAt = rs.PaidAt
                    }),
                    payments = payments.Select(p => new
                    {
                        id = p.Id,
                        amount = p.Amount,
                        method = p.Method,
                        reference = p.Reference,
                        status = p.Status,
                        createdAt = p.CreatedAt
                    }),
                    transactions = transactions.Select(t => new
                    {
                        id = t.Id,
                        type = t.TransactionType,
                        amount = t.Amount,
                        description = t.Description,
                        reference = t.Reference,
                        createdAt = t.CreatedAt
                    })
                };

                return Ok(ApiResponse<object>.SuccessResult(report));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to generate loan report: {ex.Message}"));
            }
        }
    }
}

