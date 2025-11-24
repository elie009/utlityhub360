using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class ExpenseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Notes { get; set; }
        public string? Merchant { get; set; }
        public string? PaymentMethod { get; set; }
        public string? BankAccountId { get; set; }
        public string? Location { get; set; }
        public bool IsTaxDeductible { get; set; }
        public bool IsReimbursable { get; set; }
        public string? ReimbursementRequestId { get; set; }
        public decimal? Mileage { get; set; }
        public decimal? MileageRate { get; set; }
        public decimal? PerDiemAmount { get; set; }
        public int? NumberOfDays { get; set; }
        public string ApprovalStatus { get; set; } = "PENDING_APPROVAL";
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovalNotes { get; set; }
        public bool HasReceipt { get; set; }
        public string? ReceiptId { get; set; }
        public ReceiptDto? Receipt { get; set; }
        public string? BudgetId { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; }
        public string? ParentExpenseId { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateExpenseDto
    {
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(450)]
        public string CategoryId { get; set; } = string.Empty;

        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? Merchant { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(450)]
        public string? BankAccountId { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool IsTaxDeductible { get; set; } = false;
        public bool IsReimbursable { get; set; } = false;

        public decimal? Mileage { get; set; }
        public decimal? MileageRate { get; set; }
        public decimal? PerDiemAmount { get; set; }
        public int? NumberOfDays { get; set; }

        [StringLength(20)]
        public string ApprovalStatus { get; set; } = "PENDING_APPROVAL";

        public bool IsRecurring { get; set; } = false;
        [StringLength(50)]
        public string? RecurringFrequency { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        [StringLength(450)]
        public string? BudgetId { get; set; }
    }

    public class UpdateExpenseDto
    {
        [StringLength(200)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }

        [StringLength(450)]
        public string? CategoryId { get; set; }

        public DateTime? ExpenseDate { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? Merchant { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(450)]
        public string? BankAccountId { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool? IsTaxDeductible { get; set; }
        public bool? IsReimbursable { get; set; }

        public decimal? Mileage { get; set; }
        public decimal? MileageRate { get; set; }
        public decimal? PerDiemAmount { get; set; }
        public int? NumberOfDays { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        [StringLength(450)]
        public string? BudgetId { get; set; }
    }

    public class ExpenseCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public decimal? YearlyBudget { get; set; }
        public string? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public bool IsTaxDeductible { get; set; }
        public string? TaxCategory { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int ExpenseCount { get; set; }
        public decimal TotalExpenses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateExpenseCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be 0 or greater")]
        public decimal? MonthlyBudget { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be 0 or greater")]
        public decimal? YearlyBudget { get; set; }

        [StringLength(450)]
        public string? ParentCategoryId { get; set; }

        public bool IsTaxDeductible { get; set; } = false;

        [StringLength(50)]
        public string? TaxCategory { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateExpenseCategoryDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be 0 or greater")]
        public decimal? MonthlyBudget { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be 0 or greater")]
        public decimal? YearlyBudget { get; set; }

        [StringLength(450)]
        public string? ParentCategoryId { get; set; }

        public bool? IsTaxDeductible { get; set; }

        [StringLength(50)]
        public string? TaxCategory { get; set; }

        public bool? IsActive { get; set; }

        public int? DisplayOrder { get; set; }
    }

    public class ExpenseBudgetDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public string PeriodType { get; set; } = "MONTHLY";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }
        public decimal? AlertThreshold { get; set; }
        public bool IsActive { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PercentageUsed { get; set; }
        public bool IsOverBudget { get; set; }
        public bool IsNearLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateExpenseBudgetDto
    {
        [Required]
        [StringLength(450)]
        public string CategoryId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
        public decimal BudgetAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string PeriodType { get; set; } = "MONTHLY";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Range(0, 100, ErrorMessage = "Alert threshold must be between 0 and 100")]
        public decimal? AlertThreshold { get; set; }
    }

    public class UpdateExpenseBudgetDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
        public decimal? BudgetAmount { get; set; }

        [StringLength(20)]
        public string? PeriodType { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Range(0, 100, ErrorMessage = "Alert threshold must be between 0 and 100")]
        public decimal? AlertThreshold { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ReceiptDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? ExpenseId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? OriginalFileName { get; set; }
        public decimal? ExtractedAmount { get; set; }
        public DateTime? ExtractedDate { get; set; }
        public string? ExtractedMerchant { get; set; }
        public string? ExtractedItems { get; set; }
        public string? OcrText { get; set; }
        public bool IsOcrProcessed { get; set; }
        public DateTime? OcrProcessedAt { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ExpenseApprovalDto
    {
        public string Id { get; set; } = string.Empty;
        public string ExpenseId { get; set; } = string.Empty;
        public ExpenseDto? Expense { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public string Status { get; set; } = "PENDING";
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int ApprovalLevel { get; set; }
        public string? NextApproverId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SubmitExpenseForApprovalDto
    {
        [Required]
        [StringLength(450)]
        public string ExpenseId { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class ApproveExpenseDto
    {
        [Required]
        [StringLength(450)]
        public string ApprovalId { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class RejectExpenseDto
    {
        [Required]
        [StringLength(450)]
        public string ApprovalId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RejectionReason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class ExpenseManagementReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalExpenses { get; set; }
        public int TotalCount { get; set; }
        public decimal AverageExpense { get; set; }
        public List<CategoryExpenseSummaryDto> CategorySummaries { get; set; } = new();
        public List<ExpenseDto> Expenses { get; set; } = new();
        public Dictionary<string, decimal> DailyExpenses { get; set; } = new();
        public Dictionary<string, decimal> MonthlyExpenses { get; set; } = new();
        public decimal TaxDeductibleTotal { get; set; }
        public decimal ReimbursableTotal { get; set; }
    }

    public class CategoryExpenseSummaryDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal? BudgetAmount { get; set; }
        public decimal? BudgetRemaining { get; set; }
        public bool IsOverBudget { get; set; }
    }

    public class ExpenseFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CategoryId { get; set; }
        public string? ApprovalStatus { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? Merchant { get; set; }
        public bool? IsTaxDeductible { get; set; }
        public bool? IsReimbursable { get; set; }
        public bool? HasReceipt { get; set; }
        public string? Tags { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

