using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowAll")]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        #region Expense CRUD Operations

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ExpenseDto>>> CreateExpense([FromBody] CreateExpenseDto createExpenseDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.CreateExpenseAsync(createExpenseDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseDto>.ErrorResult($"Failed to create expense: {ex.Message}"));
            }
        }

        [HttpGet("{expenseId}")]
        public async Task<ActionResult<ApiResponse<ExpenseDto>>> GetExpense(string expenseId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetExpenseAsync(expenseId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseDto>.ErrorResult($"Failed to get expense: {ex.Message}"));
            }
        }

        [HttpPut("{expenseId}")]
        public async Task<ActionResult<ApiResponse<ExpenseDto>>> UpdateExpense(string expenseId, [FromBody] UpdateExpenseDto updateExpenseDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.UpdateExpenseAsync(expenseId, updateExpenseDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseDto>.ErrorResult($"Failed to update expense: {ex.Message}"));
            }
        }

        [HttpDelete("{expenseId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteExpense(string expenseId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.DeleteExpenseAsync(expenseId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete expense: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ExpenseDto>>>> GetExpenses(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? categoryId = null,
            [FromQuery] string? approvalStatus = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? merchant = null,
            [FromQuery] bool? isTaxDeductible = null,
            [FromQuery] bool? isReimbursable = null,
            [FromQuery] bool? hasReceipt = null,
            [FromQuery] string? tags = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaginatedResponse<ExpenseDto>>.ErrorResult("User not authenticated"));
                }

                var filter = new ExpenseFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    CategoryId = categoryId,
                    ApprovalStatus = approvalStatus,
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    Merchant = merchant,
                    IsTaxDeductible = isTaxDeductible,
                    IsReimbursable = isReimbursable,
                    HasReceipt = hasReceipt,
                    Tags = tags,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _expenseService.GetExpensesAsync(userId, filter);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<ExpenseDto>>.ErrorResult($"Failed to get expenses: {ex.Message}"));
            }
        }

        #endregion

        #region Category Operations

        [HttpPost("categories")]
        public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> CreateCategory([FromBody] CreateExpenseCategoryDto createCategoryDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.CreateCategoryAsync(createCategoryDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseCategoryDto>.ErrorResult($"Failed to create category: {ex.Message}"));
            }
        }

        [HttpGet("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> GetCategory(string categoryId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetCategoryAsync(categoryId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseCategoryDto>.ErrorResult($"Failed to get category: {ex.Message}"));
            }
        }

        [HttpPut("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> UpdateCategory(string categoryId, [FromBody] UpdateExpenseCategoryDto updateCategoryDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.UpdateCategoryAsync(categoryId, updateCategoryDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseCategoryDto>.ErrorResult($"Failed to update category: {ex.Message}"));
            }
        }

        [HttpDelete("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(string categoryId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.DeleteCategoryAsync(categoryId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete category: {ex.Message}"));
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<ExpenseCategoryDto>>>> GetCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ExpenseCategoryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetCategoriesAsync(userId, includeInactive);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ExpenseCategoryDto>>.ErrorResult($"Failed to get categories: {ex.Message}"));
            }
        }

        #endregion

        #region Budget Operations

        [HttpPost("budgets")]
        public async Task<ActionResult<ApiResponse<ExpenseBudgetDto>>> CreateBudget([FromBody] CreateExpenseBudgetDto createBudgetDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseBudgetDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.CreateBudgetAsync(createBudgetDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseBudgetDto>.ErrorResult($"Failed to create budget: {ex.Message}"));
            }
        }

        [HttpGet("budgets/{budgetId}")]
        public async Task<ActionResult<ApiResponse<ExpenseBudgetDto>>> GetBudget(string budgetId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseBudgetDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetBudgetAsync(budgetId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseBudgetDto>.ErrorResult($"Failed to get budget: {ex.Message}"));
            }
        }

        [HttpPut("budgets/{budgetId}")]
        public async Task<ActionResult<ApiResponse<ExpenseBudgetDto>>> UpdateBudget(string budgetId, [FromBody] UpdateExpenseBudgetDto updateBudgetDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseBudgetDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.UpdateBudgetAsync(budgetId, updateBudgetDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseBudgetDto>.ErrorResult($"Failed to update budget: {ex.Message}"));
            }
        }

        [HttpDelete("budgets/{budgetId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBudget(string budgetId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.DeleteBudgetAsync(budgetId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete budget: {ex.Message}"));
            }
        }

        [HttpGet("budgets")]
        public async Task<ActionResult<ApiResponse<List<ExpenseBudgetDto>>>> GetBudgets(
            [FromQuery] string? categoryId = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ExpenseBudgetDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetBudgetsAsync(userId, categoryId, includeInactive);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ExpenseBudgetDto>>.ErrorResult($"Failed to get budgets: {ex.Message}"));
            }
        }

        [HttpGet("budgets/active")]
        public async Task<ActionResult<ApiResponse<List<ExpenseBudgetDto>>>> GetActiveBudgets([FromQuery] DateTime? date = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ExpenseBudgetDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetActiveBudgetsAsync(userId, date);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ExpenseBudgetDto>>.ErrorResult($"Failed to get active budgets: {ex.Message}"));
            }
        }

        [HttpGet("budgets/status")]
        public async Task<ActionResult<ApiResponse<List<ExpenseBudgetDto>>>> GetBudgetsWithStatus([FromQuery] DateTime? date = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ExpenseBudgetDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetBudgetsWithStatusAsync(userId, date);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ExpenseBudgetDto>>.ErrorResult($"Failed to get budgets with status: {ex.Message}"));
            }
        }

        #endregion

        #region Receipt Operations

        [HttpPost("{expenseId}/receipts")]
        public async Task<ActionResult<ApiResponse<ReceiptDto>>> UploadReceipt(string expenseId, IFormFile file)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceiptDto>.ErrorResult("User not authenticated"));
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<ReceiptDto>.ErrorResult("File is required"));
                }

                var result = await _expenseService.UploadReceiptAsync(expenseId, file, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceiptDto>.ErrorResult($"Failed to upload receipt: {ex.Message}"));
            }
        }

        [HttpGet("receipts/{receiptId}")]
        public async Task<ActionResult<ApiResponse<ReceiptDto>>> GetReceipt(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceiptDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetReceiptAsync(receiptId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceiptDto>.ErrorResult($"Failed to get receipt: {ex.Message}"));
            }
        }

        [HttpDelete("receipts/{receiptId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReceipt(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.DeleteReceiptAsync(receiptId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete receipt: {ex.Message}"));
            }
        }

        [HttpGet("{expenseId}/receipts")]
        public async Task<ActionResult<ApiResponse<List<ReceiptDto>>>> GetExpenseReceipts(string expenseId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ReceiptDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetExpenseReceiptsAsync(expenseId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ReceiptDto>>.ErrorResult($"Failed to get receipts: {ex.Message}"));
            }
        }

        #endregion

        #region Approval Workflow Operations

        [HttpPost("approvals/submit")]
        public async Task<ActionResult<ApiResponse<ExpenseApprovalDto>>> SubmitForApproval([FromBody] SubmitExpenseForApprovalDto submitDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseApprovalDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.SubmitForApprovalAsync(submitDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseApprovalDto>.ErrorResult($"Failed to submit for approval: {ex.Message}"));
            }
        }

        [HttpPost("approvals/approve")]
        public async Task<ActionResult<ApiResponse<ExpenseApprovalDto>>> ApproveExpense([FromBody] ApproveExpenseDto approveDto)
        {
            try
            {
                var approverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(approverId))
                {
                    return Unauthorized(ApiResponse<ExpenseApprovalDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.ApproveExpenseAsync(approveDto, approverId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseApprovalDto>.ErrorResult($"Failed to approve expense: {ex.Message}"));
            }
        }

        [HttpPost("approvals/reject")]
        public async Task<ActionResult<ApiResponse<ExpenseApprovalDto>>> RejectExpense([FromBody] RejectExpenseDto rejectDto)
        {
            try
            {
                var approverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(approverId))
                {
                    return Unauthorized(ApiResponse<ExpenseApprovalDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.RejectExpenseAsync(rejectDto, approverId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseApprovalDto>.ErrorResult($"Failed to reject expense: {ex.Message}"));
            }
        }

        [HttpGet("approvals/pending")]
        public async Task<ActionResult<ApiResponse<List<ExpenseApprovalDto>>>> GetPendingApprovals()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ExpenseApprovalDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetPendingApprovalsAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ExpenseApprovalDto>>.ErrorResult($"Failed to get pending approvals: {ex.Message}"));
            }
        }

        [HttpGet("{expenseId}/approvals")]
        public async Task<ActionResult<ApiResponse<List<ExpenseApprovalDto>>>> GetApprovalHistory(string expenseId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ExpenseApprovalDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetApprovalHistoryAsync(expenseId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ExpenseApprovalDto>>.ErrorResult($"Failed to get approval history: {ex.Message}"));
            }
        }

        #endregion

        #region Reporting Operations

        [HttpGet("reports")]
        public async Task<ActionResult<ApiResponse<ExpenseManagementReportDto>>> GetExpenseReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? categoryId = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseManagementReportDto>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetExpenseReportAsync(userId, startDate, endDate, categoryId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseManagementReportDto>.ErrorResult($"Failed to get expense report: {ex.Message}"));
            }
        }

        [HttpGet("reports/categories")]
        public async Task<ActionResult<ApiResponse<List<CategoryExpenseSummaryDto>>>> GetCategorySummaries(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<CategoryExpenseSummaryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetCategorySummariesAsync(userId, startDate, endDate);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<CategoryExpenseSummaryDto>>.ErrorResult($"Failed to get category summaries: {ex.Message}"));
            }
        }

        [HttpGet("reports/period")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetExpensesByPeriod(
            [FromQuery] string periodType,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<Dictionary<string, decimal>>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetExpensesByPeriodAsync(userId, periodType, startDate, endDate);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get expenses by period: {ex.Message}"));
            }
        }

        #endregion

        #region Analytics Operations

        [HttpGet("analytics/total")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalExpenses(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetTotalExpensesAsync(userId, startDate, endDate);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total expenses: {ex.Message}"));
            }
        }

        [HttpGet("analytics/tax-deductible")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalTaxDeductibleExpenses(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetTotalTaxDeductibleExpensesAsync(userId, startDate, endDate);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get tax deductible expenses: {ex.Message}"));
            }
        }

        [HttpGet("analytics/reimbursable")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalReimbursableExpenses(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _expenseService.GetTotalReimbursableExpensesAsync(userId, startDate, endDate);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get reimbursable expenses: {ex.Message}"));
            }
        }

        #endregion
    }
}

