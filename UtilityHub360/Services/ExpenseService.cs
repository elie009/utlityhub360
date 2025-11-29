using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;
using System.Text.Json;

namespace UtilityHub360.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly AccountingService _accountingService;
        private readonly IWebHostEnvironment _environment;

        public ExpenseService(ApplicationDbContext context, AccountingService accountingService, IWebHostEnvironment environment)
        {
            _context = context;
            _accountingService = accountingService;
            _environment = environment;
        }

        #region Expense CRUD Operations

        public async Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto createExpenseDto, string userId)
        {
            try
            {
                // Validate category exists
                var category = await _context.ExpenseCategories
                    .FirstOrDefaultAsync(c => c.Id == createExpenseDto.CategoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<ExpenseDto>.ErrorResult("Expense category not found");
                }

                var expense = new Expense
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Description = createExpenseDto.Description,
                    Amount = createExpenseDto.Amount,
                    CategoryId = createExpenseDto.CategoryId,
                    ExpenseDate = createExpenseDto.ExpenseDate,
                    Currency = createExpenseDto.Currency,
                    Notes = createExpenseDto.Notes,
                    Merchant = createExpenseDto.Merchant,
                    PaymentMethod = createExpenseDto.PaymentMethod,
                    BankAccountId = createExpenseDto.BankAccountId,
                    Location = createExpenseDto.Location,
                    IsTaxDeductible = createExpenseDto.IsTaxDeductible,
                    IsReimbursable = createExpenseDto.IsReimbursable,
                    Mileage = createExpenseDto.Mileage,
                    MileageRate = createExpenseDto.MileageRate,
                    PerDiemAmount = createExpenseDto.PerDiemAmount,
                    NumberOfDays = createExpenseDto.NumberOfDays,
                    ApprovalStatus = createExpenseDto.ApprovalStatus,
                    IsRecurring = createExpenseDto.IsRecurring,
                    RecurringFrequency = createExpenseDto.RecurringFrequency,
                    Tags = createExpenseDto.Tags,
                    BudgetId = createExpenseDto.BudgetId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Expenses.Add(expense);

                // Create accounting entry: Debit Expense, Credit Bank Account
                if (!string.IsNullOrEmpty(createExpenseDto.BankAccountId))
                {
                    // Get bank account name for accounting entry
                    var bankAccount = await _context.BankAccounts
                        .FirstOrDefaultAsync(ba => ba.Id == createExpenseDto.BankAccountId && ba.UserId == userId);
                    
                    await _accountingService.CreateExpenseEntryAsync(
                        userId,
                        createExpenseDto.Amount,
                        category.Name,
                        bankAccountName: bankAccount?.AccountName,
                        reference: null,
                        description: createExpenseDto.Description,
                        entryDate: createExpenseDto.ExpenseDate
                    );
                }

                // If approval is required, create approval record
                if (createExpenseDto.ApprovalStatus == "PENDING_APPROVAL")
                {
                    var approval = new ExpenseApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        ExpenseId = expense.Id,
                        RequestedBy = userId,
                        Status = "PENDING",
                        RequestedAt = DateTime.UtcNow,
                        ApprovalLevel = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ExpenseApprovals.Add(approval);
                }

                await _context.SaveChangesAsync();

                var expenseDto = await MapToExpenseDtoAsync(expense);
                return ApiResponse<ExpenseDto>.SuccessResult(expenseDto, "Expense created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseDto>.ErrorResult($"Failed to create expense: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseDto>> GetExpenseAsync(string expenseId, string userId)
        {
            try
            {
                var expense = await _context.Expenses
                    .Include(e => e.Category)
                    .Include(e => e.Receipt)
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<ExpenseDto>.ErrorResult("Expense not found");
                }

                var expenseDto = await MapToExpenseDtoAsync(expense);
                return ApiResponse<ExpenseDto>.SuccessResult(expenseDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseDto>.ErrorResult($"Failed to get expense: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseDto>> UpdateExpenseAsync(string expenseId, UpdateExpenseDto updateExpenseDto, string userId)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<ExpenseDto>.ErrorResult("Expense not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateExpenseDto.Description))
                    expense.Description = updateExpenseDto.Description;
                if (updateExpenseDto.Amount.HasValue)
                    expense.Amount = updateExpenseDto.Amount.Value;
                if (!string.IsNullOrEmpty(updateExpenseDto.CategoryId))
                {
                    var category = await _context.ExpenseCategories
                        .FirstOrDefaultAsync(c => c.Id == updateExpenseDto.CategoryId && c.UserId == userId && !c.IsDeleted);
                    if (category == null)
                        return ApiResponse<ExpenseDto>.ErrorResult("Expense category not found");
                    expense.CategoryId = updateExpenseDto.CategoryId;
                }
                if (updateExpenseDto.ExpenseDate.HasValue)
                    expense.ExpenseDate = updateExpenseDto.ExpenseDate.Value;
                if (!string.IsNullOrEmpty(updateExpenseDto.Currency))
                    expense.Currency = updateExpenseDto.Currency;
                if (updateExpenseDto.Notes != null)
                    expense.Notes = updateExpenseDto.Notes;
                if (updateExpenseDto.Merchant != null)
                    expense.Merchant = updateExpenseDto.Merchant;
                if (updateExpenseDto.PaymentMethod != null)
                    expense.PaymentMethod = updateExpenseDto.PaymentMethod;
                if (updateExpenseDto.BankAccountId != null)
                    expense.BankAccountId = updateExpenseDto.BankAccountId;
                if (updateExpenseDto.Location != null)
                    expense.Location = updateExpenseDto.Location;
                if (updateExpenseDto.IsTaxDeductible.HasValue)
                    expense.IsTaxDeductible = updateExpenseDto.IsTaxDeductible.Value;
                if (updateExpenseDto.IsReimbursable.HasValue)
                    expense.IsReimbursable = updateExpenseDto.IsReimbursable.Value;
                if (updateExpenseDto.Mileage.HasValue)
                    expense.Mileage = updateExpenseDto.Mileage.Value;
                if (updateExpenseDto.MileageRate.HasValue)
                    expense.MileageRate = updateExpenseDto.MileageRate.Value;
                if (updateExpenseDto.PerDiemAmount.HasValue)
                    expense.PerDiemAmount = updateExpenseDto.PerDiemAmount.Value;
                if (updateExpenseDto.NumberOfDays.HasValue)
                    expense.NumberOfDays = updateExpenseDto.NumberOfDays.Value;
                if (updateExpenseDto.Tags != null)
                    expense.Tags = updateExpenseDto.Tags;
                if (updateExpenseDto.BudgetId != null)
                    expense.BudgetId = updateExpenseDto.BudgetId;

                expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var expenseDto = await MapToExpenseDtoAsync(expense);
                return ApiResponse<ExpenseDto>.SuccessResult(expenseDto, "Expense updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseDto>.ErrorResult($"Failed to update expense: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteExpenseAsync(string expenseId, string userId)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<bool>.ErrorResult("Expense not found");
                }

                // Soft delete
                expense.IsDeleted = true;
                expense.DeletedAt = DateTime.UtcNow;
                expense.DeletedBy = userId;
                expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "Expense deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete expense: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<ExpenseDto>>> GetExpensesAsync(string userId, ExpenseFilterDto? filter = null)
        {
            try
            {
                var query = _context.Expenses
                    .Include(e => e.Category)
                    .Include(e => e.Receipt)
                    .Where(e => e.UserId == userId && !e.IsDeleted);

                // Apply filters
                if (filter != null)
                {
                    if (filter.StartDate.HasValue)
                        query = query.Where(e => e.ExpenseDate >= filter.StartDate.Value);
                    if (filter.EndDate.HasValue)
                        query = query.Where(e => e.ExpenseDate <= filter.EndDate.Value);
                    if (!string.IsNullOrEmpty(filter.CategoryId))
                        query = query.Where(e => e.CategoryId == filter.CategoryId);
                    if (!string.IsNullOrEmpty(filter.ApprovalStatus))
                        query = query.Where(e => e.ApprovalStatus == filter.ApprovalStatus);
                    if (filter.MinAmount.HasValue)
                        query = query.Where(e => e.Amount >= filter.MinAmount.Value);
                    if (filter.MaxAmount.HasValue)
                        query = query.Where(e => e.Amount <= filter.MaxAmount.Value);
                    if (!string.IsNullOrEmpty(filter.Merchant))
                        query = query.Where(e => e.Merchant != null && e.Merchant.Contains(filter.Merchant));
                    if (filter.IsTaxDeductible.HasValue)
                        query = query.Where(e => e.IsTaxDeductible == filter.IsTaxDeductible.Value);
                    if (filter.IsReimbursable.HasValue)
                        query = query.Where(e => e.IsReimbursable == filter.IsReimbursable.Value);
                    if (filter.HasReceipt.HasValue)
                        query = query.Where(e => filter.HasReceipt.Value ? e.HasReceipt : !e.HasReceipt);
                    if (!string.IsNullOrEmpty(filter.Tags))
                        query = query.Where(e => e.Tags != null && e.Tags.Contains(filter.Tags));
                }

                var totalCount = await query.CountAsync();

                var expenses = await query
                    .OrderByDescending(e => e.ExpenseDate)
                    .ThenByDescending(e => e.CreatedAt)
                    .Skip((filter?.Page - 1 ?? 0) * (filter?.PageSize ?? 50))
                    .Take(filter?.PageSize ?? 50)
                    .ToListAsync();

                var expenseDtos = new List<ExpenseDto>();
                foreach (var expense in expenses)
                {
                    expenseDtos.Add(await MapToExpenseDtoAsync(expense));
                }

                var pageSize = filter?.PageSize ?? 50;
                var paginatedResponse = new PaginatedResponse<ExpenseDto>
                {
                    Data = expenseDtos,
                    TotalCount = totalCount,
                    Page = filter?.Page ?? 1,
                    Limit = pageSize
                };

                return ApiResponse<PaginatedResponse<ExpenseDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<ExpenseDto>>.ErrorResult($"Failed to get expenses: {ex.Message}");
            }
        }

        #endregion

        #region Expense Category Operations

        public async Task<ApiResponse<ExpenseCategoryDto>> CreateCategoryAsync(CreateExpenseCategoryDto createCategoryDto, string userId)
        {
            try
            {
                var category = new Entities.ExpenseCategory
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = createCategoryDto.Name,
                    Description = createCategoryDto.Description,
                    Icon = createCategoryDto.Icon,
                    Color = createCategoryDto.Color,
                    MonthlyBudget = createCategoryDto.MonthlyBudget,
                    YearlyBudget = createCategoryDto.YearlyBudget,
                    ParentCategoryId = createCategoryDto.ParentCategoryId,
                    IsTaxDeductible = createCategoryDto.IsTaxDeductible,
                    TaxCategory = createCategoryDto.TaxCategory,
                    DisplayOrder = createCategoryDto.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ExpenseCategories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = await MapToCategoryDtoAsync(category);
                return ApiResponse<ExpenseCategoryDto>.SuccessResult(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseCategoryDto>.ErrorResult($"Failed to create category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseCategoryDto>> GetCategoryAsync(string categoryId, string userId)
        {
            try
            {
                var category = await _context.ExpenseCategories
                    .Include(c => c.ParentCategory)
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<ExpenseCategoryDto>.ErrorResult("Category not found");
                }

                var categoryDto = await MapToCategoryDtoAsync(category);
                return ApiResponse<ExpenseCategoryDto>.SuccessResult(categoryDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseCategoryDto>.ErrorResult($"Failed to get category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseCategoryDto>> UpdateCategoryAsync(string categoryId, UpdateExpenseCategoryDto updateCategoryDto, string userId)
        {
            try
            {
                var category = await _context.ExpenseCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<ExpenseCategoryDto>.ErrorResult("Category not found");
                }

                if (!string.IsNullOrEmpty(updateCategoryDto.Name))
                    category.Name = updateCategoryDto.Name;
                if (updateCategoryDto.Description != null)
                    category.Description = updateCategoryDto.Description;
                if (updateCategoryDto.Icon != null)
                    category.Icon = updateCategoryDto.Icon;
                if (updateCategoryDto.Color != null)
                    category.Color = updateCategoryDto.Color;
                if (updateCategoryDto.MonthlyBudget.HasValue)
                    category.MonthlyBudget = updateCategoryDto.MonthlyBudget.Value;
                if (updateCategoryDto.YearlyBudget.HasValue)
                    category.YearlyBudget = updateCategoryDto.YearlyBudget.Value;
                if (updateCategoryDto.ParentCategoryId != null)
                    category.ParentCategoryId = updateCategoryDto.ParentCategoryId;
                if (updateCategoryDto.IsTaxDeductible.HasValue)
                    category.IsTaxDeductible = updateCategoryDto.IsTaxDeductible.Value;
                if (updateCategoryDto.TaxCategory != null)
                    category.TaxCategory = updateCategoryDto.TaxCategory;
                if (updateCategoryDto.IsActive.HasValue)
                    category.IsActive = updateCategoryDto.IsActive.Value;
                if (updateCategoryDto.DisplayOrder.HasValue)
                    category.DisplayOrder = updateCategoryDto.DisplayOrder.Value;

                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var categoryDto = await MapToCategoryDtoAsync(category);
                return ApiResponse<ExpenseCategoryDto>.SuccessResult(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseCategoryDto>.ErrorResult($"Failed to update category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(string categoryId, string userId)
        {
            try
            {
                var category = await _context.ExpenseCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResult("Category not found");
                }

                // Check if category has expenses
                var hasExpenses = await _context.Expenses
                    .AnyAsync(e => e.CategoryId == categoryId && !e.IsDeleted);

                if (hasExpenses)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete category with existing expenses");
                }

                // Soft delete
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ExpenseCategoryDto>>> GetCategoriesAsync(string userId, bool includeInactive = false)
        {
            try
            {
                var query = _context.ExpenseCategories
                    .Include(c => c.ParentCategory)
                    .Where(c => c.UserId == userId && !c.IsDeleted);

                if (!includeInactive)
                {
                    query = query.Where(c => c.IsActive);
                }

                var categories = await query
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = new List<ExpenseCategoryDto>();
                foreach (var category in categories)
                {
                    categoryDtos.Add(await MapToCategoryDtoAsync(category));
                }

                return ApiResponse<List<ExpenseCategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExpenseCategoryDto>>.ErrorResult($"Failed to get categories: {ex.Message}");
            }
        }

        #endregion

        #region Expense Budget Operations

        public async Task<ApiResponse<ExpenseBudgetDto>> CreateBudgetAsync(CreateExpenseBudgetDto createBudgetDto, string userId)
        {
            try
            {
                // Validate category exists
                var category = await _context.ExpenseCategories
                    .FirstOrDefaultAsync(c => c.Id == createBudgetDto.CategoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<ExpenseBudgetDto>.ErrorResult("Expense category not found");
                }

                // Validate date range
                if (createBudgetDto.StartDate >= createBudgetDto.EndDate)
                {
                    return ApiResponse<ExpenseBudgetDto>.ErrorResult("Start date must be before end date");
                }

                var budget = new ExpenseBudget
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    CategoryId = createBudgetDto.CategoryId,
                    BudgetAmount = createBudgetDto.BudgetAmount,
                    PeriodType = createBudgetDto.PeriodType,
                    StartDate = createBudgetDto.StartDate,
                    EndDate = createBudgetDto.EndDate,
                    Notes = createBudgetDto.Notes,
                    AlertThreshold = createBudgetDto.AlertThreshold,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ExpenseBudgets.Add(budget);
                await _context.SaveChangesAsync();

                var budgetDto = await MapToBudgetDtoAsync(budget);
                return ApiResponse<ExpenseBudgetDto>.SuccessResult(budgetDto, "Budget created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseBudgetDto>.ErrorResult($"Failed to create budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseBudgetDto>> GetBudgetAsync(string budgetId, string userId)
        {
            try
            {
                var budget = await _context.ExpenseBudgets
                    .Include(b => b.Category)
                    .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId && !b.IsDeleted);

                if (budget == null)
                {
                    return ApiResponse<ExpenseBudgetDto>.ErrorResult("Budget not found");
                }

                var budgetDto = await MapToBudgetDtoAsync(budget);
                return ApiResponse<ExpenseBudgetDto>.SuccessResult(budgetDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseBudgetDto>.ErrorResult($"Failed to get budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseBudgetDto>> UpdateBudgetAsync(string budgetId, UpdateExpenseBudgetDto updateBudgetDto, string userId)
        {
            try
            {
                var budget = await _context.ExpenseBudgets
                    .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId && !b.IsDeleted);

                if (budget == null)
                {
                    return ApiResponse<ExpenseBudgetDto>.ErrorResult("Budget not found");
                }

                if (updateBudgetDto.BudgetAmount.HasValue)
                    budget.BudgetAmount = updateBudgetDto.BudgetAmount.Value;
                if (!string.IsNullOrEmpty(updateBudgetDto.PeriodType))
                    budget.PeriodType = updateBudgetDto.PeriodType;
                if (updateBudgetDto.StartDate.HasValue)
                    budget.StartDate = updateBudgetDto.StartDate.Value;
                if (updateBudgetDto.EndDate.HasValue)
                    budget.EndDate = updateBudgetDto.EndDate.Value;
                if (updateBudgetDto.Notes != null)
                    budget.Notes = updateBudgetDto.Notes;
                if (updateBudgetDto.AlertThreshold.HasValue)
                    budget.AlertThreshold = updateBudgetDto.AlertThreshold.Value;
                if (updateBudgetDto.IsActive.HasValue)
                    budget.IsActive = updateBudgetDto.IsActive.Value;

                budget.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var budgetDto = await MapToBudgetDtoAsync(budget);
                return ApiResponse<ExpenseBudgetDto>.SuccessResult(budgetDto, "Budget updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseBudgetDto>.ErrorResult($"Failed to update budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBudgetAsync(string budgetId, string userId)
        {
            try
            {
                var budget = await _context.ExpenseBudgets
                    .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId && !b.IsDeleted);

                if (budget == null)
                {
                    return ApiResponse<bool>.ErrorResult("Budget not found");
                }

                // Soft delete
                budget.IsDeleted = true;
                budget.DeletedAt = DateTime.UtcNow;
                budget.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "Budget deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ExpenseBudgetDto>>> GetBudgetsAsync(string userId, string? categoryId = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.ExpenseBudgets
                    .Include(b => b.Category)
                    .Where(b => b.UserId == userId && !b.IsDeleted);

                if (!string.IsNullOrEmpty(categoryId))
                {
                    query = query.Where(b => b.CategoryId == categoryId);
                }

                if (!includeInactive)
                {
                    query = query.Where(b => b.IsActive);
                }

                var budgets = await query
                    .OrderByDescending(b => b.StartDate)
                    .ToListAsync();

                var budgetDtos = new List<ExpenseBudgetDto>();
                foreach (var budget in budgets)
                {
                    budgetDtos.Add(await MapToBudgetDtoAsync(budget));
                }

                return ApiResponse<List<ExpenseBudgetDto>>.SuccessResult(budgetDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExpenseBudgetDto>>.ErrorResult($"Failed to get budgets: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ExpenseBudgetDto>>> GetActiveBudgetsAsync(string userId, DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow;

                var budgets = await _context.ExpenseBudgets
                    .Include(b => b.Category)
                    .Where(b => b.UserId == userId 
                        && !b.IsDeleted 
                        && b.IsActive
                        && b.StartDate <= targetDate
                        && b.EndDate >= targetDate)
                    .OrderBy(b => b.Category.Name)
                    .ToListAsync();

                var budgetDtos = new List<ExpenseBudgetDto>();
                foreach (var budget in budgets)
                {
                    budgetDtos.Add(await MapToBudgetDtoAsync(budget));
                }

                return ApiResponse<List<ExpenseBudgetDto>>.SuccessResult(budgetDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExpenseBudgetDto>>.ErrorResult($"Failed to get active budgets: {ex.Message}");
            }
        }

        #endregion

        #region Receipt Operations

        public async Task<ApiResponse<ReceiptDto>> UploadReceiptAsync(string expenseId, IFormFile file, string userId)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Expense not found");
                }

                // Validate file
                if (file == null || file.Length == 0)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("File is required");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Invalid file type. Allowed: JPG, JPEG, PNG, PDF");
                }

                var maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("File size exceeds 10MB limit");
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "receipts", userId);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                var relativePath = Path.Combine("uploads", "receipts", userId, fileName).Replace("\\", "/");

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create receipt record
                var receipt = new ExpenseReceipt
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ExpenseId = expenseId,
                    FileName = fileName,
                    FilePath = relativePath,
                    FileType = file.ContentType ?? "application/octet-stream",
                    FileSize = file.Length,
                    OriginalFileName = file.FileName,
                    IsOcrProcessed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ExpenseReceipts.Add(receipt);

                // Update expense
                expense.HasReceipt = true;
                expense.ReceiptId = receipt.Id;
                expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var receiptDto = MapToReceiptDto(receipt);
                return ApiResponse<ReceiptDto>.SuccessResult(receiptDto, "Receipt uploaded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ReceiptDto>.ErrorResult($"Failed to upload receipt: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceiptDto>> GetReceiptAsync(string receiptId, string userId)
        {
            try
            {
                var receipt = await _context.ExpenseReceipts
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

                if (receipt == null)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Receipt not found");
                }

                var receiptDto = MapToReceiptDto(receipt);
                return ApiResponse<ReceiptDto>.SuccessResult(receiptDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReceiptDto>.ErrorResult($"Failed to get receipt: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteReceiptAsync(string receiptId, string userId)
        {
            try
            {
                var receipt = await _context.ExpenseReceipts
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

                if (receipt == null)
                {
                    return ApiResponse<bool>.ErrorResult("Receipt not found");
                }

                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, receipt.FilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Update expense if linked
                if (!string.IsNullOrEmpty(receipt.ExpenseId))
                {
                    var expense = await _context.Expenses
                        .FirstOrDefaultAsync(e => e.Id == receipt.ExpenseId);
                    if (expense != null)
                    {
                        expense.HasReceipt = false;
                        expense.ReceiptId = null;
                        expense.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Soft delete receipt
                receipt.IsDeleted = true;
                receipt.DeletedAt = DateTime.UtcNow;
                receipt.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "Receipt deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete receipt: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ReceiptDto>>> GetExpenseReceiptsAsync(string expenseId, string userId)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<List<ReceiptDto>>.ErrorResult("Expense not found");
                }

                var receipts = await _context.ExpenseReceipts
                    .Where(r => r.ExpenseId == expenseId && r.UserId == userId && !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var receiptDtos = receipts.Select(MapToReceiptDto).ToList();
                return ApiResponse<List<ReceiptDto>>.SuccessResult(receiptDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ReceiptDto>>.ErrorResult($"Failed to get receipts: {ex.Message}");
            }
        }

        #endregion

        #region Approval Workflow Operations

        public async Task<ApiResponse<ExpenseApprovalDto>> SubmitForApprovalAsync(SubmitExpenseForApprovalDto submitDto, string userId)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == submitDto.ExpenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<ExpenseApprovalDto>.ErrorResult("Expense not found");
                }

                // Check if already submitted
                var existingApproval = await _context.ExpenseApprovals
                    .FirstOrDefaultAsync(a => a.ExpenseId == submitDto.ExpenseId && a.Status == "PENDING");

                if (existingApproval != null)
                {
                    return ApiResponse<ExpenseApprovalDto>.ErrorResult("Expense is already pending approval");
                }

                var approval = new ExpenseApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    ExpenseId = submitDto.ExpenseId,
                    RequestedBy = userId,
                    Status = "PENDING",
                    Notes = submitDto.Notes,
                    RequestedAt = DateTime.UtcNow,
                    ApprovalLevel = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ExpenseApprovals.Add(approval);

                // Update expense status
                expense.ApprovalStatus = "PENDING_APPROVAL";
                expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var approvalDto = await MapToApprovalDtoAsync(approval);
                return ApiResponse<ExpenseApprovalDto>.SuccessResult(approvalDto, "Expense submitted for approval");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseApprovalDto>.ErrorResult($"Failed to submit for approval: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseApprovalDto>> ApproveExpenseAsync(ApproveExpenseDto approveDto, string approverId)
        {
            try
            {
                var approval = await _context.ExpenseApprovals
                    .Include(a => a.Expense)
                    .FirstOrDefaultAsync(a => a.Id == approveDto.ApprovalId && a.Status == "PENDING");

                if (approval == null)
                {
                    return ApiResponse<ExpenseApprovalDto>.ErrorResult("Approval request not found or already processed");
                }

                approval.Status = "APPROVED";
                approval.ApprovedBy = approverId;
                approval.ReviewedAt = DateTime.UtcNow;
                approval.Notes = approveDto.Notes;
                approval.UpdatedAt = DateTime.UtcNow;

                // Update expense
                approval.Expense.ApprovalStatus = "APPROVED";
                approval.Expense.ApprovedBy = approverId;
                approval.Expense.ApprovedAt = DateTime.UtcNow;
                approval.Expense.ApprovalNotes = approveDto.Notes;
                approval.Expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var approvalDto = await MapToApprovalDtoAsync(approval);
                return ApiResponse<ExpenseApprovalDto>.SuccessResult(approvalDto, "Expense approved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseApprovalDto>.ErrorResult($"Failed to approve expense: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExpenseApprovalDto>> RejectExpenseAsync(RejectExpenseDto rejectDto, string approverId)
        {
            try
            {
                var approval = await _context.ExpenseApprovals
                    .Include(a => a.Expense)
                    .FirstOrDefaultAsync(a => a.Id == rejectDto.ApprovalId && a.Status == "PENDING");

                if (approval == null)
                {
                    return ApiResponse<ExpenseApprovalDto>.ErrorResult("Approval request not found or already processed");
                }

                approval.Status = "REJECTED";
                approval.ApprovedBy = approverId;
                approval.ReviewedAt = DateTime.UtcNow;
                approval.RejectionReason = rejectDto.RejectionReason;
                approval.Notes = rejectDto.Notes;
                approval.UpdatedAt = DateTime.UtcNow;

                // Update expense
                approval.Expense.ApprovalStatus = "REJECTED";
                approval.Expense.ApprovedBy = approverId;
                approval.Expense.ApprovedAt = DateTime.UtcNow;
                approval.Expense.ApprovalNotes = rejectDto.RejectionReason;
                approval.Expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var approvalDto = await MapToApprovalDtoAsync(approval);
                return ApiResponse<ExpenseApprovalDto>.SuccessResult(approvalDto, "Expense rejected");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseApprovalDto>.ErrorResult($"Failed to reject expense: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ExpenseApprovalDto>>> GetPendingApprovalsAsync(string userId)
        {
            try
            {
                var approvals = await _context.ExpenseApprovals
                    .Include(a => a.Expense)
                    .ThenInclude(e => e.Category)
                    .Where(a => a.Status == "PENDING" && a.RequestedBy == userId)
                    .OrderByDescending(a => a.RequestedAt)
                    .ToListAsync();

                var approvalDtos = new List<ExpenseApprovalDto>();
                foreach (var approval in approvals)
                {
                    approvalDtos.Add(await MapToApprovalDtoAsync(approval));
                }

                return ApiResponse<List<ExpenseApprovalDto>>.SuccessResult(approvalDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExpenseApprovalDto>>.ErrorResult($"Failed to get pending approvals: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ExpenseApprovalDto>>> GetApprovalHistoryAsync(string expenseId, string userId)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<List<ExpenseApprovalDto>>.ErrorResult("Expense not found");
                }

                var approvals = await _context.ExpenseApprovals
                    .Include(a => a.Expense)
                    .ThenInclude(e => e.Category)
                    .Where(a => a.ExpenseId == expenseId)
                    .OrderByDescending(a => a.RequestedAt)
                    .ToListAsync();

                var approvalDtos = new List<ExpenseApprovalDto>();
                foreach (var approval in approvals)
                {
                    approvalDtos.Add(await MapToApprovalDtoAsync(approval));
                }

                return ApiResponse<List<ExpenseApprovalDto>>.SuccessResult(approvalDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExpenseApprovalDto>>.ErrorResult($"Failed to get approval history: {ex.Message}");
            }
        }

        #endregion

        #region Reporting Operations

        public async Task<ApiResponse<ExpenseManagementReportDto>> GetExpenseReportAsync(string userId, DateTime startDate, DateTime endDate, string? categoryId = null)
        {
            try
            {
                var query = _context.Expenses
                    .Include(e => e.Category)
                    .Where(e => e.UserId == userId 
                        && !e.IsDeleted 
                        && e.ExpenseDate >= startDate 
                        && e.ExpenseDate <= endDate);

                if (!string.IsNullOrEmpty(categoryId))
                {
                    query = query.Where(e => e.CategoryId == categoryId);
                }

                var expenses = await query
                    .OrderBy(e => e.ExpenseDate)
                    .ToListAsync();

                var totalExpenses = expenses.Sum(e => e.Amount);
                var totalCount = expenses.Count;
                var averageExpense = totalCount > 0 ? totalExpenses / totalCount : 0;

                // Category summaries
                var categorySummaries = expenses
                    .GroupBy(e => new { e.CategoryId, e.Category.Name })
                    .Select(g => new CategoryExpenseSummaryDto
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.Name,
                        TotalAmount = g.Sum(e => e.Amount),
                        Count = g.Count(),
                        Percentage = totalExpenses > 0 ? (g.Sum(e => e.Amount) / totalExpenses) * 100 : 0
                    })
                    .OrderByDescending(c => c.TotalAmount)
                    .ToList();

                // Daily expenses
                var dailyExpenses = expenses
                    .GroupBy(e => e.ExpenseDate.Date)
                    .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Sum(e => e.Amount));

                // Monthly expenses
                var monthlyExpenses = expenses
                    .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                    .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => g.Sum(e => e.Amount));

                var expenseDtos = new List<ExpenseDto>();
                foreach (var expense in expenses)
                {
                    expenseDtos.Add(await MapToExpenseDtoAsync(expense));
                }

                var report = new ExpenseManagementReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalExpenses = totalExpenses,
                    TotalCount = totalCount,
                    AverageExpense = averageExpense,
                    CategorySummaries = categorySummaries,
                    Expenses = expenseDtos,
                    DailyExpenses = dailyExpenses,
                    MonthlyExpenses = monthlyExpenses,
                    TaxDeductibleTotal = expenses.Where(e => e.IsTaxDeductible).Sum(e => e.Amount),
                    ReimbursableTotal = expenses.Where(e => e.IsReimbursable).Sum(e => e.Amount)
                };

                return ApiResponse<ExpenseManagementReportDto>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                return ApiResponse<ExpenseManagementReportDto>.ErrorResult($"Failed to generate report: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<CategoryExpenseSummaryDto>>> GetCategorySummariesAsync(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var expenses = await _context.Expenses
                    .Include(e => e.Category)
                    .Where(e => e.UserId == userId 
                        && !e.IsDeleted 
                        && e.ExpenseDate >= startDate 
                        && e.ExpenseDate <= endDate)
                    .ToListAsync();

                var totalExpenses = expenses.Sum(e => e.Amount);

                var summaries = expenses
                    .GroupBy(e => new { e.CategoryId, e.Category.Name })
                    .Select(g => new CategoryExpenseSummaryDto
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.Name,
                        TotalAmount = g.Sum(e => e.Amount),
                        Count = g.Count(),
                        Percentage = totalExpenses > 0 ? (g.Sum(e => e.Amount) / totalExpenses) * 100 : 0
                    })
                    .OrderByDescending(c => c.TotalAmount)
                    .ToList();

                return ApiResponse<List<CategoryExpenseSummaryDto>>.SuccessResult(summaries);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CategoryExpenseSummaryDto>>.ErrorResult($"Failed to get category summaries: {ex.Message}");
            }
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetExpensesByPeriodAsync(string userId, string periodType, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Expenses
                    .Where(e => e.UserId == userId && !e.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(e => e.ExpenseDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(e => e.ExpenseDate <= endDate.Value);

                var expenses = await query.ToListAsync();

                Dictionary<string, decimal> result = periodType.ToUpper() switch
                {
                    "DAILY" => expenses.GroupBy(e => e.ExpenseDate.Date.ToString("yyyy-MM-dd"))
                        .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                    "WEEKLY" => expenses.GroupBy(e => GetWeekKey(e.ExpenseDate))
                        .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                    "MONTHLY" => expenses.GroupBy(e => $"{e.ExpenseDate.Year}-{e.ExpenseDate.Month:D2}")
                        .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                    "YEARLY" => expenses.GroupBy(e => e.ExpenseDate.Year.ToString())
                        .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                    _ => expenses.GroupBy(e => e.ExpenseDate.Date.ToString("yyyy-MM-dd"))
                        .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount))
                };

                return ApiResponse<Dictionary<string, decimal>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get expenses by period: {ex.Message}");
            }
        }

        #endregion

        #region Analytics Operations

        public async Task<ApiResponse<decimal>> GetTotalExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Expenses
                    .Where(e => e.UserId == userId && !e.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(e => e.ExpenseDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(e => e.ExpenseDate <= endDate.Value);

                var total = await query.SumAsync(e => e.Amount);
                return ApiResponse<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total expenses: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalTaxDeductibleExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Expenses
                    .Where(e => e.UserId == userId && !e.IsDeleted && e.IsTaxDeductible);

                if (startDate.HasValue)
                    query = query.Where(e => e.ExpenseDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(e => e.ExpenseDate <= endDate.Value);

                var total = await query.SumAsync(e => e.Amount);
                return ApiResponse<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get tax deductible expenses: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalReimbursableExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Expenses
                    .Where(e => e.UserId == userId && !e.IsDeleted && e.IsReimbursable);

                if (startDate.HasValue)
                    query = query.Where(e => e.ExpenseDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(e => e.ExpenseDate <= endDate.Value);

                var total = await query.SumAsync(e => e.Amount);
                return ApiResponse<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get reimbursable expenses: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ExpenseBudgetDto>>> GetBudgetsWithStatusAsync(string userId, DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow;

                var budgets = await _context.ExpenseBudgets
                    .Include(b => b.Category)
                    .Where(b => b.UserId == userId 
                        && !b.IsDeleted 
                        && b.IsActive
                        && b.StartDate <= targetDate
                        && b.EndDate >= targetDate)
                    .ToListAsync();

                var budgetDtos = new List<ExpenseBudgetDto>();
                foreach (var budget in budgets)
                {
                    var spent = await _context.Expenses
                        .Where(e => e.BudgetId == budget.Id 
                            && !e.IsDeleted 
                            && e.ExpenseDate >= budget.StartDate 
                            && e.ExpenseDate <= budget.EndDate)
                        .SumAsync(e => e.Amount);

                    var budgetDto = await MapToBudgetDtoAsync(budget);
                    budgetDto.SpentAmount = spent;
                    budgetDto.RemainingAmount = budget.BudgetAmount - spent;
                    budgetDto.PercentageUsed = budget.BudgetAmount > 0 ? (spent / budget.BudgetAmount) * 100 : 0;
                    budgetDto.IsOverBudget = spent > budget.BudgetAmount;
                    budgetDto.IsNearLimit = budget.AlertThreshold.HasValue && budgetDto.PercentageUsed >= budget.AlertThreshold.Value;

                    budgetDtos.Add(budgetDto);
                }

                return ApiResponse<List<ExpenseBudgetDto>>.SuccessResult(budgetDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExpenseBudgetDto>>.ErrorResult($"Failed to get budgets with status: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private async Task<ExpenseDto> MapToExpenseDtoAsync(Expense expense)
        {
            var dto = new ExpenseDto
            {
                Id = expense.Id,
                UserId = expense.UserId,
                Description = expense.Description,
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
                CategoryName = expense.Category?.Name ?? "",
                ExpenseDate = expense.ExpenseDate,
                Currency = expense.Currency,
                Notes = expense.Notes,
                Merchant = expense.Merchant,
                PaymentMethod = expense.PaymentMethod,
                BankAccountId = expense.BankAccountId,
                Location = expense.Location,
                IsTaxDeductible = expense.IsTaxDeductible,
                IsReimbursable = expense.IsReimbursable,
                ReimbursementRequestId = expense.ReimbursementRequestId,
                Mileage = expense.Mileage,
                MileageRate = expense.MileageRate,
                PerDiemAmount = expense.PerDiemAmount,
                NumberOfDays = expense.NumberOfDays,
                ApprovalStatus = expense.ApprovalStatus,
                ApprovedBy = expense.ApprovedBy,
                ApprovedAt = expense.ApprovedAt,
                ApprovalNotes = expense.ApprovalNotes,
                HasReceipt = expense.HasReceipt,
                ReceiptId = expense.ReceiptId,
                BudgetId = expense.BudgetId,
                IsRecurring = expense.IsRecurring,
                RecurringFrequency = expense.RecurringFrequency,
                ParentExpenseId = expense.ParentExpenseId,
                Tags = expense.Tags,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };

            if (expense.Receipt != null)
            {
                dto.Receipt = MapToReceiptDto(expense.Receipt);
            }

            return dto;
        }

        private async Task<ExpenseCategoryDto> MapToCategoryDtoAsync(Entities.ExpenseCategory category)
        {
            var expenseCount = await _context.Expenses
                .CountAsync(e => e.CategoryId == category.Id && !e.IsDeleted);

            var totalExpenses = await _context.Expenses
                .Where(e => e.CategoryId == category.Id && !e.IsDeleted)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            return new ExpenseCategoryDto
            {
                Id = category.Id,
                UserId = category.UserId,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                Color = category.Color,
                MonthlyBudget = category.MonthlyBudget,
                YearlyBudget = category.YearlyBudget,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                IsTaxDeductible = category.IsTaxDeductible,
                TaxCategory = category.TaxCategory,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                ExpenseCount = expenseCount,
                TotalExpenses = totalExpenses,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        private async Task<ExpenseBudgetDto> MapToBudgetDtoAsync(ExpenseBudget budget)
        {
            var spent = await _context.Expenses
                .Where(e => e.BudgetId == budget.Id 
                    && !e.IsDeleted 
                    && e.ExpenseDate >= budget.StartDate 
                    && e.ExpenseDate <= budget.EndDate)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var remaining = budget.BudgetAmount - spent;
            var percentageUsed = budget.BudgetAmount > 0 ? (spent / budget.BudgetAmount) * 100 : 0;

            return new ExpenseBudgetDto
            {
                Id = budget.Id,
                UserId = budget.UserId,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.Name ?? "",
                BudgetAmount = budget.BudgetAmount,
                PeriodType = budget.PeriodType,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                Notes = budget.Notes,
                AlertThreshold = budget.AlertThreshold,
                IsActive = budget.IsActive,
                SpentAmount = spent,
                RemainingAmount = remaining,
                PercentageUsed = percentageUsed,
                IsOverBudget = spent > budget.BudgetAmount,
                IsNearLimit = budget.AlertThreshold.HasValue && percentageUsed >= budget.AlertThreshold.Value,
                CreatedAt = budget.CreatedAt,
                UpdatedAt = budget.UpdatedAt
            };
        }

        private ReceiptDto MapToReceiptDto(ExpenseReceipt receipt)
        {
            var dto = new ReceiptDto
            {
                Id = receipt.Id,
                UserId = receipt.UserId,
                ExpenseId = receipt.ExpenseId,
                FileName = receipt.FileName,
                FilePath = receipt.FilePath,
                FileType = receipt.FileType,
                FileSize = receipt.FileSize,
                OriginalFileName = receipt.OriginalFileName,
                ExtractedAmount = receipt.ExtractedAmount,
                ExtractedDate = receipt.ExtractedDate,
                ExtractedMerchant = receipt.ExtractedMerchant,
                OcrText = receipt.OcrText,
                IsOcrProcessed = receipt.IsOcrProcessed,
                OcrProcessedAt = receipt.OcrProcessedAt,
                ThumbnailPath = receipt.ThumbnailPath,
                Notes = receipt.Notes,
                CreatedAt = receipt.CreatedAt,
                UpdatedAt = receipt.UpdatedAt
            };

            // Parse extracted items from JSON string
            if (!string.IsNullOrEmpty(receipt.ExtractedItems))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<ReceiptItemDto>>(receipt.ExtractedItems);
                    dto.ExtractedItems = items;
                }
                catch
                {
                    // Ignore parsing errors
                }
            }

            return dto;
        }

        private async Task<ExpenseApprovalDto> MapToApprovalDtoAsync(ExpenseApproval approval)
        {
            var dto = new ExpenseApprovalDto
            {
                Id = approval.Id,
                ExpenseId = approval.ExpenseId,
                RequestedBy = approval.RequestedBy,
                ApprovedBy = approval.ApprovedBy,
                Status = approval.Status,
                Notes = approval.Notes,
                RejectionReason = approval.RejectionReason,
                RequestedAt = approval.RequestedAt,
                ReviewedAt = approval.ReviewedAt,
                ApprovalLevel = approval.ApprovalLevel,
                NextApproverId = approval.NextApproverId,
                CreatedAt = approval.CreatedAt,
                UpdatedAt = approval.UpdatedAt
            };

            if (approval.Expense != null)
            {
                dto.Expense = await MapToExpenseDtoAsync(approval.Expense);
            }

            return dto;
        }

        private string GetWeekKey(DateTime date)
        {
            var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
            return $"{startOfWeek.Year}-W{GetWeekOfYear(startOfWeek):D2}";
        }

        private int GetWeekOfYear(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        #endregion
    }
}

