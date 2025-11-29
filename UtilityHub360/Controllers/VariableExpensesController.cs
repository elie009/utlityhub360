using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VariableExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VariableExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all variable expenses for the authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<VariableExpenseDto>>>> GetVariableExpenses(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? category = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<VariableExpenseDto>>.ErrorResult("User not authenticated"));
                }

                var query = _context.VariableExpenses.Where(v => v.UserId == userId);

                if (startDate.HasValue)
                {
                    query = query.Where(v => v.ExpenseDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(v => v.ExpenseDate <= endDate.Value);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(v => v.Category == category.ToUpper());
                }

                var expenses = await query
                    .OrderByDescending(v => v.ExpenseDate)
                    .ToListAsync();

                var dtos = expenses.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<VariableExpenseDto>>.SuccessResult(dtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<VariableExpenseDto>>.ErrorResult(
                    $"Failed to retrieve variable expenses: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific variable expense by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<VariableExpenseDto>>> GetVariableExpense(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var expense = await _context.VariableExpenses
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

                if (expense == null)
                {
                    return NotFound(ApiResponse<VariableExpenseDto>.ErrorResult("Variable expense not found"));
                }

                return Ok(ApiResponse<VariableExpenseDto>.SuccessResult(MapToDto(expense)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<VariableExpenseDto>.ErrorResult(
                    $"Failed to retrieve variable expense: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new variable expense
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<VariableExpenseDto>>> CreateVariableExpense(
            [FromBody] VariableExpenseDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<VariableExpenseDto>.ErrorResult("User not authenticated"));
                }

                var expense = new VariableExpense
                {
                    UserId = userId,
                    Description = dto.Description,
                    Amount = dto.Amount,
                    Category = dto.Category.ToUpper(),
                    Currency = dto.Currency,
                    ExpenseDate = dto.ExpenseDate,
                    Notes = dto.Notes,
                    Merchant = dto.Merchant,
                    PaymentMethod = dto.PaymentMethod,
                    IsRecurring = dto.IsRecurring,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.VariableExpenses.Add(expense);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetVariableExpense),
                    new { id = expense.Id },
                    ApiResponse<VariableExpenseDto>.SuccessResult(MapToDto(expense)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<VariableExpenseDto>.ErrorResult(
                    $"Failed to create variable expense: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing variable expense
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<VariableExpenseDto>>> UpdateVariableExpense(
            string id,
            [FromBody] VariableExpenseDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var expense = await _context.VariableExpenses
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

                if (expense == null)
                {
                    return NotFound(ApiResponse<VariableExpenseDto>.ErrorResult("Variable expense not found"));
                }

                expense.Description = dto.Description;
                expense.Amount = dto.Amount;
                expense.Category = dto.Category.ToUpper();
                expense.Currency = dto.Currency;
                expense.ExpenseDate = dto.ExpenseDate;
                expense.Notes = dto.Notes;
                expense.Merchant = dto.Merchant;
                expense.PaymentMethod = dto.PaymentMethod;
                expense.IsRecurring = dto.IsRecurring;
                expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<VariableExpenseDto>.SuccessResult(MapToDto(expense)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<VariableExpenseDto>.ErrorResult(
                    $"Failed to update variable expense: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a variable expense
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVariableExpense(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var expense = await _context.VariableExpenses
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

                if (expense == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Variable expense not found"));
                }

                _context.VariableExpenses.Remove(expense);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(new { message = "Variable expense deleted successfully" }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    $"Failed to delete variable expense: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get expense statistics by category
        /// </summary>
        [HttpGet("statistics/by-category")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetExpensesByCategory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<object>>.ErrorResult("User not authenticated"));
                }

                var now = DateTime.UtcNow;
                var start = startDate ?? new DateTime(now.Year, now.Month, 1);
                var end = endDate ?? now;

                var expenses = await _context.VariableExpenses
                    .Where(v => v.UserId == userId && v.ExpenseDate >= start && v.ExpenseDate <= end)
                    .ToListAsync();

                var statistics = expenses
                    .GroupBy(v => v.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(v => v.Amount),
                        Count = g.Count(),
                        AverageAmount = g.Average(v => v.Amount),
                        Percentage = expenses.Sum(v => v.Amount) > 0
                            ? (g.Sum(v => v.Amount) / expenses.Sum(v => v.Amount)) * 100
                            : 0
                    })
                    .OrderByDescending(s => s.TotalAmount)
                    .ToList();

                return Ok(ApiResponse<List<object>>.SuccessResult(statistics.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<object>>.ErrorResult(
                    $"Failed to retrieve statistics: {ex.Message}"));
            }
        }

        // Helper method to map entity to DTO
        private static VariableExpenseDto MapToDto(VariableExpense expense)
        {
            return new VariableExpenseDto
            {
                Id = expense.Id,
                UserId = expense.UserId,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category,
                Currency = expense.Currency,
                ExpenseDate = expense.ExpenseDate,
                Notes = expense.Notes,
                Merchant = expense.Merchant,
                PaymentMethod = expense.PaymentMethod,
                IsRecurring = expense.IsRecurring,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };
        }
    }
}

