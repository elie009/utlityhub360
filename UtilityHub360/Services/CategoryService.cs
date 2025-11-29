using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TransactionCategoryDto>> CreateCategoryAsync(CreateTransactionCategoryDto createDto, string userId)
        {
            try
            {
                // Check for duplicate category name for this user
                var existingCategory = await _context.TransactionCategories
                    .FirstOrDefaultAsync(c => c.UserId == userId && 
                                             c.Name.ToLower() == createDto.Name.ToLower() && 
                                             !c.IsDeleted);

                if (existingCategory != null)
                {
                    return ApiResponse<TransactionCategoryDto>.ErrorResult(
                        $"A category with the name '{createDto.Name}' already exists.");
                }

                var category = new TransactionCategory
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Type = createDto.Type.ToUpper(),
                    Icon = createDto.Icon,
                    Color = createDto.Color,
                    IsSystemCategory = false,
                    IsActive = true,
                    DisplayOrder = createDto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TransactionCategories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = await MapToCategoryDtoAsync(category);
                return ApiResponse<TransactionCategoryDto>.SuccessResult(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TransactionCategoryDto>.ErrorResult($"Failed to create category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TransactionCategoryDto>> UpdateCategoryAsync(string categoryId, UpdateTransactionCategoryDto updateDto, string userId)
        {
            try
            {
                var category = await _context.TransactionCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<TransactionCategoryDto>.ErrorResult("Category not found");
                }

                if (category.IsSystemCategory)
                {
                    // Allow updating system categories but only certain fields
                    if (updateDto.Description != null) category.Description = updateDto.Description;
                    if (updateDto.Icon != null) category.Icon = updateDto.Icon;
                    if (updateDto.Color != null) category.Color = updateDto.Color;
                    if (updateDto.DisplayOrder.HasValue) category.DisplayOrder = updateDto.DisplayOrder.Value;
                }
                else
                {
                    // Check for duplicate name if name is being updated
                    if (!string.IsNullOrEmpty(updateDto.Name) && updateDto.Name.ToLower() != category.Name.ToLower())
                    {
                        var existingCategory = await _context.TransactionCategories
                            .FirstOrDefaultAsync(c => c.UserId == userId && 
                                                     c.Name.ToLower() == updateDto.Name.ToLower() && 
                                                     c.Id != categoryId && 
                                                     !c.IsDeleted);

                        if (existingCategory != null)
                        {
                            return ApiResponse<TransactionCategoryDto>.ErrorResult(
                                $"A category with the name '{updateDto.Name}' already exists.");
                        }
                    }

                    // Update all fields for custom categories
                    if (!string.IsNullOrEmpty(updateDto.Name)) category.Name = updateDto.Name;
                    if (updateDto.Description != null) category.Description = updateDto.Description;
                    if (!string.IsNullOrEmpty(updateDto.Type)) category.Type = updateDto.Type.ToUpper();
                    if (updateDto.Icon != null) category.Icon = updateDto.Icon;
                    if (updateDto.Color != null) category.Color = updateDto.Color;
                    if (updateDto.IsActive.HasValue) category.IsActive = updateDto.IsActive.Value;
                    if (updateDto.DisplayOrder.HasValue) category.DisplayOrder = updateDto.DisplayOrder.Value;
                }

                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var categoryDto = await MapToCategoryDtoAsync(category);
                return ApiResponse<TransactionCategoryDto>.SuccessResult(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TransactionCategoryDto>.ErrorResult($"Failed to update category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(string categoryId, string userId)
        {
            try
            {
                var category = await _context.TransactionCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResult("Category not found");
                }

                if (category.IsSystemCategory)
                {
                    return ApiResponse<bool>.ErrorResult("System categories cannot be deleted");
                }

                // Check if category is being used by any transactions
                var transactionCount = await _context.BankTransactions
                    .CountAsync(t => t.Category == category.Name && t.UserId == userId && !t.IsDeleted);

                if (transactionCount > 0)
                {
                    // Soft delete instead of hard delete
                    category.IsDeleted = true;
                    category.DeletedAt = DateTime.UtcNow;
                    category.IsActive = false;
                    category.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Hard delete if not in use
                    _context.TransactionCategories.Remove(category);
                }

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TransactionCategoryDto>> GetCategoryByIdAsync(string categoryId, string userId)
        {
            try
            {
                var category = await _context.TransactionCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted);

                if (category == null)
                {
                    return ApiResponse<TransactionCategoryDto>.ErrorResult("Category not found");
                }

                var categoryDto = await MapToCategoryDtoAsync(category);
                return ApiResponse<TransactionCategoryDto>.SuccessResult(categoryDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<TransactionCategoryDto>.ErrorResult($"Failed to get category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TransactionCategoryDto>>> GetAllCategoriesAsync(string userId, string? type = null)
        {
            try
            {
                var query = _context.TransactionCategories
                    .Where(c => c.UserId == userId && !c.IsDeleted);

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(c => c.Type.ToUpper() == type.ToUpper());
                }

                var categories = await query
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = new List<TransactionCategoryDto>();
                foreach (var category in categories)
                {
                    categoryDtos.Add(await MapToCategoryDtoAsync(category));
                }

                return ApiResponse<List<TransactionCategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TransactionCategoryDto>>.ErrorResult($"Failed to get categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TransactionCategoryDto>>> GetActiveCategoriesAsync(string userId, string? type = null)
        {
            try
            {
                var query = _context.TransactionCategories
                    .Where(c => c.UserId == userId && c.IsActive && !c.IsDeleted);

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(c => c.Type.ToUpper() == type.ToUpper());
                }

                var categories = await query
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = new List<TransactionCategoryDto>();
                foreach (var category in categories)
                {
                    categoryDtos.Add(await MapToCategoryDtoAsync(category));
                }

                return ApiResponse<List<TransactionCategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TransactionCategoryDto>>.ErrorResult($"Failed to get active categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SeedSystemCategoriesAsync(string userId)
        {
            try
            {
                // Check if system categories already exist for this user
                var existingSystemCategories = await _context.TransactionCategories
                    .AnyAsync(c => c.UserId == userId && c.IsSystemCategory);

                if (existingSystemCategories)
                {
                    return ApiResponse<bool>.SuccessResult(true, "System categories already exist");
                }

                var systemCategories = new List<TransactionCategory>
                {
                    // Expense Categories
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "FOOD", Type = "EXPENSE", Icon = "restaurant", Color = "#FF6B6B", IsSystemCategory = true, DisplayOrder = 1 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "GROCERIES", Type = "EXPENSE", Icon = "shopping_cart", Color = "#4ECDC4", IsSystemCategory = true, DisplayOrder = 2 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "RESTAURANTS", Type = "EXPENSE", Icon = "restaurant_menu", Color = "#FF6B6B", IsSystemCategory = true, DisplayOrder = 3 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "TRANSPORTATION", Type = "EXPENSE", Icon = "directions_car", Color = "#95E1D3", IsSystemCategory = true, DisplayOrder = 4 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "GAS", Type = "EXPENSE", Icon = "local_gas_station", Color = "#F38181", IsSystemCategory = true, DisplayOrder = 5 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "ENTERTAINMENT", Type = "EXPENSE", Icon = "movie", Color = "#AA96DA", IsSystemCategory = true, DisplayOrder = 6 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "SHOPPING", Type = "EXPENSE", Icon = "shopping_bag", Color = "#FCBAD3", IsSystemCategory = true, DisplayOrder = 7 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "HEALTHCARE", Type = "EXPENSE", Icon = "local_hospital", Color = "#A8E6CF", IsSystemCategory = true, DisplayOrder = 8 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "EDUCATION", Type = "EXPENSE", Icon = "school", Color = "#FFD93D", IsSystemCategory = true, DisplayOrder = 9 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "TRAVEL", Type = "EXPENSE", Icon = "flight", Color = "#6BCB77", IsSystemCategory = true, DisplayOrder = 10 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "UTILITIES", Type = "BILL", Icon = "bolt", Color = "#FFD93D", IsSystemCategory = true, DisplayOrder = 11 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "RENT", Type = "BILL", Icon = "home", Color = "#95E1D3", IsSystemCategory = true, DisplayOrder = 12 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "INSURANCE", Type = "BILL", Icon = "security", Color = "#F38181", IsSystemCategory = true, DisplayOrder = 13 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "SUBSCRIPTIONS", Type = "BILL", Icon = "subscriptions", Color = "#AA96DA", IsSystemCategory = true, DisplayOrder = 14 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "SAVINGS", Type = "SAVINGS", Icon = "savings", Color = "#4ECDC4", IsSystemCategory = true, DisplayOrder = 15 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "INVESTMENT", Type = "SAVINGS", Icon = "trending_up", Color = "#6BCB77", IsSystemCategory = true, DisplayOrder = 16 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "LOAN_PAYMENT", Type = "LOAN", Icon = "account_balance", Color = "#FF6B6B", IsSystemCategory = true, DisplayOrder = 17 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "TRANSFER", Type = "TRANSFER", Icon = "swap_horiz", Color = "#95E1D3", IsSystemCategory = true, DisplayOrder = 18 },
                    
                    // Income Categories
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "SALARY", Type = "INCOME", Icon = "work", Color = "#6BCB77", IsSystemCategory = true, DisplayOrder = 19 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "FREELANCE", Type = "INCOME", Icon = "laptop", Color = "#4ECDC4", IsSystemCategory = true, DisplayOrder = 20 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "INVESTMENT_INCOME", Type = "INCOME", Icon = "trending_up", Color = "#95E1D3", IsSystemCategory = true, DisplayOrder = 21 },
                    new TransactionCategory { Id = Guid.NewGuid().ToString(), UserId = userId, Name = "OTHER_INCOME", Type = "INCOME", Icon = "attach_money", Color = "#AA96DA", IsSystemCategory = true, DisplayOrder = 22 }
                };

                foreach (var category in systemCategories)
                {
                    category.CreatedAt = DateTime.UtcNow;
                    category.UpdatedAt = DateTime.UtcNow;
                    category.IsActive = true;
                }

                _context.TransactionCategories.AddRange(systemCategories);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "System categories seeded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to seed system categories: {ex.Message}");
            }
        }

        private async Task<TransactionCategoryDto> MapToCategoryDtoAsync(TransactionCategory category)
        {
            var transactionCount = await _context.BankTransactions
                .CountAsync(t => t.Category == category.Name && t.UserId == category.UserId && !t.IsDeleted);

            return new TransactionCategoryDto
            {
                Id = category.Id,
                UserId = category.UserId,
                Name = category.Name,
                Description = category.Description,
                Type = category.Type,
                Icon = category.Icon,
                Color = category.Color,
                IsSystemCategory = category.IsSystemCategory,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                TransactionCount = transactionCount
            };
        }
    }
}

