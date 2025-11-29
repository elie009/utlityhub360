using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IExpenseService
    {
        // Expense CRUD Operations
        Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto createExpenseDto, string userId);
        Task<ApiResponse<ExpenseDto>> GetExpenseAsync(string expenseId, string userId);
        Task<ApiResponse<ExpenseDto>> UpdateExpenseAsync(string expenseId, UpdateExpenseDto updateExpenseDto, string userId);
        Task<ApiResponse<bool>> DeleteExpenseAsync(string expenseId, string userId);
        Task<ApiResponse<PaginatedResponse<ExpenseDto>>> GetExpensesAsync(string userId, ExpenseFilterDto? filter = null);

        // Expense Category Operations
        Task<ApiResponse<ExpenseCategoryDto>> CreateCategoryAsync(CreateExpenseCategoryDto createCategoryDto, string userId);
        Task<ApiResponse<ExpenseCategoryDto>> GetCategoryAsync(string categoryId, string userId);
        Task<ApiResponse<ExpenseCategoryDto>> UpdateCategoryAsync(string categoryId, UpdateExpenseCategoryDto updateCategoryDto, string userId);
        Task<ApiResponse<bool>> DeleteCategoryAsync(string categoryId, string userId);
        Task<ApiResponse<List<ExpenseCategoryDto>>> GetCategoriesAsync(string userId, bool includeInactive = false);

        // Expense Budget Operations
        Task<ApiResponse<ExpenseBudgetDto>> CreateBudgetAsync(CreateExpenseBudgetDto createBudgetDto, string userId);
        Task<ApiResponse<ExpenseBudgetDto>> GetBudgetAsync(string budgetId, string userId);
        Task<ApiResponse<ExpenseBudgetDto>> UpdateBudgetAsync(string budgetId, UpdateExpenseBudgetDto updateBudgetDto, string userId);
        Task<ApiResponse<bool>> DeleteBudgetAsync(string budgetId, string userId);
        Task<ApiResponse<List<ExpenseBudgetDto>>> GetBudgetsAsync(string userId, string? categoryId = null, bool includeInactive = false);
        Task<ApiResponse<List<ExpenseBudgetDto>>> GetActiveBudgetsAsync(string userId, DateTime? date = null);

        // Receipt Operations
        Task<ApiResponse<ReceiptDto>> UploadReceiptAsync(string expenseId, IFormFile file, string userId);
        Task<ApiResponse<ReceiptDto>> GetReceiptAsync(string receiptId, string userId);
        Task<ApiResponse<bool>> DeleteReceiptAsync(string receiptId, string userId);
        Task<ApiResponse<List<ReceiptDto>>> GetExpenseReceiptsAsync(string expenseId, string userId);

        // Approval Workflow Operations
        Task<ApiResponse<ExpenseApprovalDto>> SubmitForApprovalAsync(SubmitExpenseForApprovalDto submitDto, string userId);
        Task<ApiResponse<ExpenseApprovalDto>> ApproveExpenseAsync(ApproveExpenseDto approveDto, string approverId);
        Task<ApiResponse<ExpenseApprovalDto>> RejectExpenseAsync(RejectExpenseDto rejectDto, string approverId);
        Task<ApiResponse<List<ExpenseApprovalDto>>> GetPendingApprovalsAsync(string userId);
        Task<ApiResponse<List<ExpenseApprovalDto>>> GetApprovalHistoryAsync(string expenseId, string userId);

        // Reporting Operations
        Task<ApiResponse<ExpenseManagementReportDto>> GetExpenseReportAsync(string userId, DateTime startDate, DateTime endDate, string? categoryId = null);
        Task<ApiResponse<List<CategoryExpenseSummaryDto>>> GetCategorySummariesAsync(string userId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<Dictionary<string, decimal>>> GetExpensesByPeriodAsync(string userId, string periodType, DateTime? startDate = null, DateTime? endDate = null);

        // Analytics Operations
        Task<ApiResponse<decimal>> GetTotalExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<decimal>> GetTotalTaxDeductibleExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<decimal>> GetTotalReimbursableExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<List<ExpenseBudgetDto>>> GetBudgetsWithStatusAsync(string userId, DateTime? date = null);
    }
}

