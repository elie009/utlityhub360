using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly ApplicationDbContext _context;

        public AllocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Template Operations

        public async Task<ApiResponse<List<AllocationTemplateDto>>> GetTemplatesAsync(string? userId = null)
        {
            try
            {
                var templates = await _context.Set<AllocationTemplate>()
                    .Where(t => t.IsActive && 
                               (t.IsSystemTemplate || (t.UserId == userId && !t.IsSystemTemplate)))
                    .Include(t => t.Categories)
                    .OrderBy(t => t.IsSystemTemplate ? 0 : 1)
                    .ThenBy(t => t.Name)
                    .ToListAsync();

                var dtos = templates.Select(t => MapToTemplateDto(t)).ToList();
                return ApiResponse<List<AllocationTemplateDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AllocationTemplateDto>>.ErrorResult($"Failed to get templates: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationTemplateDto>> GetTemplateAsync(string templateId)
        {
            try
            {
                var template = await _context.Set<AllocationTemplate>()
                    .Include(t => t.Categories)
                    .FirstOrDefaultAsync(t => t.Id == templateId);

                if (template == null)
                {
                    return ApiResponse<AllocationTemplateDto>.ErrorResult("Template not found");
                }

                return ApiResponse<AllocationTemplateDto>.SuccessResult(MapToTemplateDto(template));
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationTemplateDto>.ErrorResult($"Failed to get template: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationTemplateDto>> CreateTemplateAsync(CreateAllocationTemplateDto dto, string userId)
        {
            try
            {
                // Validate percentages sum to 100
                var totalPercentage = dto.Categories.Sum(c => c.Percentage);
                if (Math.Abs(totalPercentage - 100) > 0.01m)
                {
                    return ApiResponse<AllocationTemplateDto>.ErrorResult(
                        $"Category percentages must sum to 100%. Current sum: {totalPercentage}%");
                }

                var template = new AllocationTemplate
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Description = dto.Description,
                    IsSystemTemplate = false,
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Set<AllocationTemplate>().Add(template);

                foreach (var catDto in dto.Categories)
                {
                    var category = new AllocationTemplateCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        TemplateId = template.Id,
                        CategoryName = catDto.CategoryName,
                        Description = catDto.Description,
                        Percentage = catDto.Percentage,
                        DisplayOrder = catDto.DisplayOrder,
                        Color = catDto.Color
                    };
                    _context.Set<AllocationTemplateCategory>().Add(category);
                }

                await _context.SaveChangesAsync();

                var result = await GetTemplateAsync(template.Id);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationTemplateDto>.ErrorResult($"Failed to create template: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteTemplateAsync(string templateId, string userId)
        {
            try
            {
                var template = await _context.Set<AllocationTemplate>()
                    .FirstOrDefaultAsync(t => t.Id == templateId);

                if (template == null)
                {
                    return ApiResponse<bool>.ErrorResult("Template not found");
                }

                if (template.IsSystemTemplate)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete system templates");
                }

                if (template.UserId != userId)
                {
                    return ApiResponse<bool>.ErrorResult("Unauthorized to delete this template");
                }

                _context.Set<AllocationTemplate>().Remove(template);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Template deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete template: {ex.Message}");
            }
        }

        #endregion

        #region Plan Operations

        public async Task<ApiResponse<AllocationPlanDto>> GetActivePlanAsync(string userId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.UserId == userId && p.IsActive)
                    .Include(p => p.Categories)
                    .Include(p => p.Template)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<AllocationPlanDto>.ErrorResult("No active allocation plan found");
                }

                var dto = await MapToPlanDtoAsync(plan);
                return ApiResponse<AllocationPlanDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationPlanDto>.ErrorResult($"Failed to get active plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationPlanDto>> GetPlanAsync(string planId, string userId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .Include(p => p.Categories)
                    .Include(p => p.Template)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<AllocationPlanDto>.ErrorResult("Plan not found");
                }

                var dto = await MapToPlanDtoAsync(plan);
                return ApiResponse<AllocationPlanDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationPlanDto>.ErrorResult($"Failed to get plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<AllocationPlanDto>>> GetPlansAsync(string userId)
        {
            try
            {
                var plans = await _context.Set<AllocationPlan>()
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Categories)
                    .Include(p => p.Template)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var dtos = new List<AllocationPlanDto>();
                foreach (var plan in plans)
                {
                    dtos.Add(await MapToPlanDtoAsync(plan));
                }

                return ApiResponse<List<AllocationPlanDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AllocationPlanDto>>.ErrorResult($"Failed to get plans: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationPlanDto>> CreatePlanAsync(CreateAllocationPlanDto dto, string userId)
        {
            try
            {
                // Deactivate existing active plans
                var existingPlans = await _context.Set<AllocationPlan>()
                    .Where(p => p.UserId == userId && p.IsActive)
                    .ToListAsync();

                foreach (var existingPlan in existingPlans)
                {
                    existingPlan.IsActive = false;
                    existingPlan.EndDate = DateTime.UtcNow;
                }

                // Validate allocations don't exceed income
                var totalAllocated = dto.Categories.Sum(c => c.AllocatedAmount);
                if (totalAllocated > dto.MonthlyIncome)
                {
                    return ApiResponse<AllocationPlanDto>.ErrorResult(
                        $"Total allocated amount (${totalAllocated}) cannot exceed monthly income (${dto.MonthlyIncome})");
                }

                var plan = new AllocationPlan
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    TemplateId = dto.TemplateId,
                    PlanName = dto.PlanName,
                    MonthlyIncome = dto.MonthlyIncome,
                    IsActive = true,
                    StartDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Set<AllocationPlan>().Add(plan);

                foreach (var catDto in dto.Categories)
                {
                    var category = new AllocationCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        PlanId = plan.Id,
                        CategoryName = catDto.CategoryName,
                        Description = catDto.Description,
                        AllocatedAmount = catDto.AllocatedAmount,
                        Percentage = catDto.Percentage,
                        DisplayOrder = catDto.DisplayOrder,
                        Color = catDto.Color,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Set<AllocationCategory>().Add(category);
                }

                await _context.SaveChangesAsync();

                // Record initial history
                await RecordHistoryAsync(userId, plan.Id);

                var result = await GetPlanAsync(plan.Id, userId);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationPlanDto>.ErrorResult($"Failed to create plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationPlanDto>> UpdatePlanAsync(string planId, UpdateAllocationPlanDto dto, string userId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<AllocationPlanDto>.ErrorResult("Plan not found");
                }

                if (!string.IsNullOrEmpty(dto.PlanName))
                {
                    plan.PlanName = dto.PlanName;
                }

                if (dto.MonthlyIncome.HasValue)
                {
                    plan.MonthlyIncome = dto.MonthlyIncome.Value;
                }

                if (dto.Categories != null && dto.Categories.Any())
                {
                    // Remove existing categories
                    _context.Set<AllocationCategory>().RemoveRange(plan.Categories);

                    // Add new categories
                    foreach (var catDto in dto.Categories)
                    {
                        var category = new AllocationCategory
                        {
                            Id = Guid.NewGuid().ToString(),
                            PlanId = plan.Id,
                            CategoryName = catDto.CategoryName,
                            Description = catDto.Description,
                            AllocatedAmount = catDto.AllocatedAmount,
                            Percentage = catDto.Percentage,
                            DisplayOrder = catDto.DisplayOrder,
                            Color = catDto.Color,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Set<AllocationCategory>().Add(category);
                    }
                }

                plan.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var result = await GetPlanAsync(planId, userId);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationPlanDto>.ErrorResult($"Failed to update plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeletePlanAsync(string planId, string userId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<bool>.ErrorResult("Plan not found");
                }

                _context.Set<AllocationPlan>().Remove(plan);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Plan deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationPlanDto>> ApplyTemplateAsync(string templateId, decimal monthlyIncome, string userId)
        {
            try
            {
                var template = await GetTemplateAsync(templateId);
                if (!template.Success || template.Data == null)
                {
                    return ApiResponse<AllocationPlanDto>.ErrorResult("Template not found");
                }

                var categories = template.Data.Categories.Select(c => new CreateAllocationCategoryDto
                {
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    AllocatedAmount = monthlyIncome * c.Percentage / 100,
                    Percentage = c.Percentage,
                    DisplayOrder = c.DisplayOrder,
                    Color = c.Color
                }).ToList();

                var createDto = new CreateAllocationPlanDto
                {
                    PlanName = $"Plan based on {template.Data.Name}",
                    MonthlyIncome = monthlyIncome,
                    TemplateId = templateId,
                    Categories = categories
                };

                return await CreatePlanAsync(createDto, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationPlanDto>.ErrorResult($"Failed to apply template: {ex.Message}");
            }
        }

        #endregion

        #region Category Operations

        public async Task<ApiResponse<List<AllocationCategoryDto>>> GetCategoriesAsync(string planId, string userId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<List<AllocationCategoryDto>>.ErrorResult("Plan not found");
                }

                var categories = await _context.Set<AllocationCategory>()
                    .Where(c => c.PlanId == planId)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                var periodDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var actualsResponse = await GetCategoryActualsAsync(planId, userId, periodDate);
                if (!actualsResponse.Success || actualsResponse.Data == null)
                {
                    return ApiResponse<List<AllocationCategoryDto>>.ErrorResult(actualsResponse.Message ?? "Failed to get category actuals");
                }
                var actuals = actualsResponse.Data;

                var dtos = categories.Select(c =>
                {
                    var actual = actuals.ContainsKey(c.CategoryName) ? actuals[c.CategoryName] : 0;
                    var variance = c.AllocatedAmount - actual;
                    var variancePercentage = c.AllocatedAmount > 0 ? (variance / c.AllocatedAmount) * 100 : 0;
                    var status = variance < 0 ? "over_budget" : (variance > c.AllocatedAmount * 0.1m ? "under_budget" : "on_track");

                    return new AllocationCategoryDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        AllocatedAmount = c.AllocatedAmount,
                        Percentage = c.Percentage,
                        ActualAmount = actual,
                        Variance = variance,
                        VariancePercentage = variancePercentage,
                        Status = status,
                        DisplayOrder = c.DisplayOrder,
                        Color = c.Color
                    };
                }).ToList();

                return ApiResponse<List<AllocationCategoryDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AllocationCategoryDto>>.ErrorResult($"Failed to get categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationCategoryDto>> GetCategoryAsync(string categoryId, string userId)
        {
            try
            {
                var category = await _context.Set<AllocationCategory>()
                    .Include(c => c.Plan)
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                if (category == null || category.Plan.UserId != userId)
                {
                    return ApiResponse<AllocationCategoryDto>.ErrorResult("Category not found");
                }

                var periodDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var actualsResponse = await GetCategoryActualsAsync(category.PlanId, userId, periodDate);
                if (!actualsResponse.Success || actualsResponse.Data == null)
                {
                    return ApiResponse<AllocationCategoryDto>.ErrorResult(actualsResponse.Message ?? "Failed to get category actuals");
                }
                var actuals = actualsResponse.Data;
                var actual = actuals.ContainsKey(category.CategoryName) ? actuals[category.CategoryName] : 0;
                var variance = category.AllocatedAmount - actual;
                var variancePercentage = category.AllocatedAmount > 0 ? (variance / category.AllocatedAmount) * 100 : 0;
                var status = variance < 0 ? "over_budget" : (variance > category.AllocatedAmount * 0.1m ? "under_budget" : "on_track");

                var dto = new AllocationCategoryDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    AllocatedAmount = category.AllocatedAmount,
                    Percentage = category.Percentage,
                    ActualAmount = actual,
                    Variance = variance,
                    VariancePercentage = variancePercentage,
                    Status = status,
                    DisplayOrder = category.DisplayOrder,
                    Color = category.Color
                };

                return ApiResponse<AllocationCategoryDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationCategoryDto>.ErrorResult($"Failed to get category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationCategoryDto>> UpdateCategoryAsync(string categoryId, CreateAllocationCategoryDto dto, string userId)
        {
            try
            {
                var category = await _context.Set<AllocationCategory>()
                    .Include(c => c.Plan)
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                if (category == null || category.Plan.UserId != userId)
                {
                    return ApiResponse<AllocationCategoryDto>.ErrorResult("Category not found");
                }

                category.CategoryName = dto.CategoryName;
                category.Description = dto.Description;
                category.AllocatedAmount = dto.AllocatedAmount;
                category.Percentage = dto.Percentage;
                category.DisplayOrder = dto.DisplayOrder;
                category.Color = dto.Color;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetCategoryAsync(categoryId, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationCategoryDto>.ErrorResult($"Failed to update category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(string categoryId, string userId)
        {
            try
            {
                var category = await _context.Set<AllocationCategory>()
                    .Include(c => c.Plan)
                    .FirstOrDefaultAsync(c => c.Id == categoryId);

                if (category == null || category.Plan.UserId != userId)
                {
                    return ApiResponse<bool>.ErrorResult("Category not found");
                }

                _context.Set<AllocationCategory>().Remove(category);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete category: {ex.Message}");
            }
        }

        #endregion

        #region History & Tracking

        public async Task<ApiResponse<List<AllocationHistoryDto>>> GetHistoryAsync(string userId, AllocationHistoryQueryDto query)
        {
            try
            {
                var historyQuery = _context.Set<AllocationHistory>()
                    .Where(h => h.UserId == userId);

                if (!string.IsNullOrEmpty(query.PlanId))
                {
                    historyQuery = historyQuery.Where(h => h.PlanId == query.PlanId);
                }

                if (!string.IsNullOrEmpty(query.CategoryId))
                {
                    historyQuery = historyQuery.Where(h => h.CategoryId == query.CategoryId);
                }

                if (query.StartDate.HasValue)
                {
                    historyQuery = historyQuery.Where(h => h.PeriodDate >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    historyQuery = historyQuery.Where(h => h.PeriodDate <= query.EndDate.Value);
                }

                if (query.Months.HasValue)
                {
                    var startDate = DateTime.UtcNow.AddMonths(-query.Months.Value);
                    historyQuery = historyQuery.Where(h => h.PeriodDate >= startDate);
                }

                var history = await historyQuery
                    .Include(h => h.Category)
                    .OrderByDescending(h => h.PeriodDate)
                    .ToListAsync();

                var dtos = history.Select(h => new AllocationHistoryDto
                {
                    Id = h.Id,
                    PlanId = h.PlanId,
                    CategoryId = h.CategoryId,
                    CategoryName = h.Category?.CategoryName,
                    PeriodDate = h.PeriodDate,
                    AllocatedAmount = h.AllocatedAmount,
                    ActualAmount = h.ActualAmount,
                    Variance = h.Variance,
                    VariancePercentage = h.VariancePercentage,
                    Status = h.Status,
                    CreatedAt = h.CreatedAt
                }).ToList();

                return ApiResponse<List<AllocationHistoryDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AllocationHistoryDto>>.ErrorResult($"Failed to get history: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> RecordHistoryAsync(string userId, string planId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<bool>.ErrorResult("Plan not found");
                }

                var periodDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var actualsResponse = await GetCategoryActualsAsync(planId, userId, periodDate);
                if (!actualsResponse.Success || actualsResponse.Data == null)
                {
                    return ApiResponse<bool>.ErrorResult(actualsResponse.Message ?? "Failed to get category actuals");
                }
                var actuals = actualsResponse.Data;

                // Record overall plan history
                var totalAllocated = plan.Categories.Sum(c => c.AllocatedAmount);
                var totalActual = actuals.Values.Sum();
                var totalVariance = totalAllocated - totalActual;
                var totalVariancePercentage = totalAllocated > 0 ? (totalVariance / totalAllocated) * 100 : 0;
                var planStatus = totalVariance < 0 ? "over_budget" : (totalVariance > totalAllocated * 0.1m ? "under_budget" : "on_track");

                var planHistory = new AllocationHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    PlanId = planId,
                    CategoryId = null,
                    PeriodDate = periodDate,
                    AllocatedAmount = totalAllocated,
                    ActualAmount = totalActual,
                    Variance = totalVariance,
                    VariancePercentage = totalVariancePercentage,
                    Status = planStatus,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Set<AllocationHistory>().Add(planHistory);

                // Record category history
                foreach (var category in plan.Categories)
                {
                    var actual = actuals.ContainsKey(category.CategoryName) ? actuals[category.CategoryName] : 0;
                    var variance = category.AllocatedAmount - actual;
                    var variancePercentage = category.AllocatedAmount > 0 ? (variance / category.AllocatedAmount) * 100 : 0;
                    var status = variance < 0 ? "over_budget" : (variance > category.AllocatedAmount * 0.1m ? "under_budget" : "on_track");

                    var categoryHistory = new AllocationHistory
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        PlanId = planId,
                        CategoryId = category.Id,
                        PeriodDate = periodDate,
                        AllocatedAmount = category.AllocatedAmount,
                        ActualAmount = actual,
                        Variance = variance,
                        VariancePercentage = variancePercentage,
                        Status = status,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Set<AllocationHistory>().Add(categoryHistory);
                }

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "History recorded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to record history: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<AllocationTrendDto>>> GetTrendsAsync(string userId, string? planId = null, int months = 12)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                var periodDate = new DateTime(startDate.Year, startDate.Month, 1);

                var trends = new List<AllocationTrendDto>();

                for (int i = 0; i < months; i++)
                {
                    var currentPeriod = periodDate.AddMonths(i);
                    var historyQuery = _context.Set<AllocationHistory>()
                        .Where(h => h.UserId == userId && 
                                   h.PeriodDate == currentPeriod &&
                                   h.CategoryId != null); // Only category-level history

                    if (!string.IsNullOrEmpty(planId))
                    {
                        historyQuery = historyQuery.Where(h => h.PlanId == planId);
                    }

                    var history = await historyQuery
                        .Include(h => h.Category)
                        .ToListAsync();

                    if (history.Any())
                    {
                        var trend = new AllocationTrendDto
                        {
                            PeriodDate = currentPeriod,
                            TotalAllocated = history.Sum(h => h.AllocatedAmount),
                            TotalActual = history.Sum(h => h.ActualAmount),
                            Categories = history.Select(h => new AllocationTrendCategoryDto
                            {
                                CategoryName = h.Category?.CategoryName ?? "Unknown",
                                AllocatedAmount = h.AllocatedAmount,
                                ActualAmount = h.ActualAmount,
                                Variance = h.Variance
                            }).ToList()
                        };

                        trends.Add(trend);
                    }
                }

                return ApiResponse<List<AllocationTrendDto>>.SuccessResult(trends);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AllocationTrendDto>>.ErrorResult($"Failed to get trends: {ex.Message}");
            }
        }

        #endregion

        #region Recommendations

        public async Task<ApiResponse<List<AllocationRecommendationDto>>> GetRecommendationsAsync(string userId, string? planId = null)
        {
            try
            {
                var query = _context.Set<AllocationRecommendation>()
                    .Where(r => r.UserId == userId && !r.IsApplied);

                if (!string.IsNullOrEmpty(planId))
                {
                    query = query.Where(r => r.PlanId == planId);
                }

                var recommendations = await query
                    .Include(r => r.Category)
                    .OrderByDescending(r => r.Priority == "urgent" ? 4 : r.Priority == "high" ? 3 : r.Priority == "medium" ? 2 : 1)
                    .ThenByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var dtos = recommendations.Select(r => new AllocationRecommendationDto
                {
                    Id = r.Id,
                    PlanId = r.PlanId,
                    RecommendationType = r.RecommendationType,
                    Title = r.Title,
                    Message = r.Message,
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category?.CategoryName,
                    SuggestedAmount = r.SuggestedAmount,
                    SuggestedPercentage = r.SuggestedPercentage,
                    Priority = r.Priority,
                    IsRead = r.IsRead,
                    IsApplied = r.IsApplied,
                    CreatedAt = r.CreatedAt,
                    AppliedAt = r.AppliedAt
                }).ToList();

                return ApiResponse<List<AllocationRecommendationDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AllocationRecommendationDto>>.ErrorResult($"Failed to get recommendations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> MarkRecommendationReadAsync(string recommendationId, string userId)
        {
            try
            {
                var recommendation = await _context.Set<AllocationRecommendation>()
                    .FirstOrDefaultAsync(r => r.Id == recommendationId && r.UserId == userId);

                if (recommendation == null)
                {
                    return ApiResponse<bool>.ErrorResult("Recommendation not found");
                }

                recommendation.IsRead = true;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Recommendation marked as read");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to mark recommendation as read: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ApplyRecommendationAsync(string recommendationId, string userId)
        {
            try
            {
                var recommendation = await _context.Set<AllocationRecommendation>()
                    .Include(r => r.Category)
                    .FirstOrDefaultAsync(r => r.Id == recommendationId && r.UserId == userId);

                if (recommendation == null)
                {
                    return ApiResponse<bool>.ErrorResult("Recommendation not found");
                }

                if (recommendation.IsApplied)
                {
                    return ApiResponse<bool>.ErrorResult("Recommendation already applied");
                }

                // Apply the recommendation based on type
                if (recommendation.CategoryId != null && recommendation.Category != null)
                {
                    var category = recommendation.Category;
                    if (recommendation.SuggestedAmount.HasValue)
                    {
                        category.AllocatedAmount = recommendation.SuggestedAmount.Value;
                    }
                    if (recommendation.SuggestedPercentage.HasValue)
                    {
                        category.Percentage = recommendation.SuggestedPercentage.Value;
                    }
                    category.UpdatedAt = DateTime.UtcNow;
                }

                recommendation.IsApplied = true;
                recommendation.AppliedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Recommendation applied successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to apply recommendation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<AllocationRecommendationDto>>> GenerateRecommendationsAsync(string userId, string planId)
        {
            try
            {
                var plan = await GetPlanAsync(planId, userId);
                if (!plan.Success || plan.Data == null)
                {
                    return ApiResponse<List<AllocationRecommendationDto>>.ErrorResult("Plan not found");
                }

                var categories = await GetCategoriesAsync(planId, userId);
                if (!categories.Success || categories.Data == null)
                {
                    return ApiResponse<List<AllocationRecommendationDto>>.ErrorResult("Failed to get categories");
                }

                var recommendations = new List<AllocationRecommendation>();

                foreach (var category in categories.Data)
                {
                    // Over budget recommendation
                    if (category.Status == "over_budget")
                    {
                        var overage = Math.Abs(category.Variance);
                        var recommendation = new AllocationRecommendation
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            PlanId = planId,
                            CategoryId = category.Id,
                            RecommendationType = "decrease_allocation",
                            Title = $"Reduce {category.CategoryName} Allocation",
                            Message = $"You're over budget by ${overage:F2} in {category.CategoryName}. Consider reducing this category's allocation or finding ways to cut spending.",
                            SuggestedAmount = category.AllocatedAmount - (overage * 0.5m), // Suggest reducing by half the overage
                            Priority = overage > category.AllocatedAmount * 0.2m ? "urgent" : "high",
                            IsRead = false,
                            IsApplied = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        recommendations.Add(recommendation);
                    }

                    // Under budget recommendation (opportunity to save more)
                    if (category.Status == "under_budget" && category.Variance > category.AllocatedAmount * 0.2m)
                    {
                        var recommendation = new AllocationRecommendation
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            PlanId = planId,
                            CategoryId = category.Id,
                            RecommendationType = "increase_allocation",
                            Title = $"Increase {category.CategoryName} Allocation",
                            Message = $"You're consistently under budget in {category.CategoryName}. Consider reallocating funds to savings or other categories.",
                            SuggestedAmount = category.AllocatedAmount + (category.Variance * 0.3m),
                            Priority = "medium",
                            IsRead = false,
                            IsApplied = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        recommendations.Add(recommendation);
                    }
                }

                // Overall rebalancing recommendation
                var totalVariance = categories.Data.Sum(c => c.Variance);
                if (totalVariance < -plan.Data.MonthlyIncome * 0.1m) // More than 10% over total budget
                {
                    var recommendation = new AllocationRecommendation
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        PlanId = planId,
                        RecommendationType = "rebalance",
                        Title = "Rebalance Your Budget",
                        Message = $"Your total spending exceeds your budget by ${Math.Abs(totalVariance):F2}. Consider reviewing all categories and making adjustments.",
                        Priority = "high",
                        IsRead = false,
                        IsApplied = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    recommendations.Add(recommendation);
                }

                // Emergency fund recommendation
                var savingsCategory = categories.Data.FirstOrDefault(c => 
                    c.CategoryName.ToLower().Contains("savings") || 
                    c.CategoryName.ToLower().Contains("emergency"));
                
                if (savingsCategory == null || savingsCategory.Percentage < 10)
                {
                    var recommendation = new AllocationRecommendation
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        PlanId = planId,
                        RecommendationType = "emergency_fund",
                        Title = "Build Emergency Fund",
                        Message = "Consider allocating at least 10-20% of your income to savings/emergency fund for financial security.",
                        SuggestedPercentage = 15,
                        Priority = "medium",
                        IsRead = false,
                        IsApplied = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    recommendations.Add(recommendation);
                }

                _context.Set<AllocationRecommendation>().AddRange(recommendations);
                await _context.SaveChangesAsync();

                var dtos = recommendations.Select(r => new AllocationRecommendationDto
                {
                    Id = r.Id,
                    PlanId = r.PlanId,
                    RecommendationType = r.RecommendationType,
                    Title = r.Title,
                    Message = r.Message,
                    CategoryId = r.CategoryId,
                    SuggestedAmount = r.SuggestedAmount,
                    SuggestedPercentage = r.SuggestedPercentage,
                    Priority = r.Priority,
                    IsRead = r.IsRead,
                    IsApplied = r.IsApplied,
                    CreatedAt = r.CreatedAt
                }).ToList();

                return ApiResponse<List<AllocationRecommendationDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AllocationRecommendationDto>>.ErrorResult($"Failed to generate recommendations: {ex.Message}");
            }
        }

        #endregion

        #region Calculations & Formulas

        public async Task<ApiResponse<AllocationCalculationDto>> CalculateAllocationAsync(decimal monthlyIncome, List<CreateAllocationTemplateCategoryDto> categories)
        {
            try
            {
                var totalPercentage = categories.Sum(c => c.Percentage);
                if (Math.Abs(totalPercentage - 100) > 0.01m)
                {
                    return ApiResponse<AllocationCalculationDto>.ErrorResult(
                        $"Category percentages must sum to 100%. Current sum: {totalPercentage}%");
                }

                var categoryCalculations = categories.Select(c => new AllocationCategoryCalculationDto
                {
                    CategoryName = c.CategoryName,
                    Percentage = c.Percentage,
                    CalculatedAmount = monthlyIncome * c.Percentage / 100,
                    Formula = $"${monthlyIncome:F2} × {c.Percentage}% = ${monthlyIncome * c.Percentage / 100:F2}"
                }).ToList();

                var totalAllocated = categoryCalculations.Sum(c => c.CalculatedAmount);
                var surplusDeficit = monthlyIncome - totalAllocated;

                var summary = new AllocationSummaryDto
                {
                    TotalAllocated = totalAllocated,
                    TotalActual = 0, // No actuals in calculation
                    TotalVariance = surplusDeficit,
                    SurplusDeficit = surplusDeficit,
                    AllocationPercentage = monthlyIncome > 0 ? (totalAllocated / monthlyIncome) * 100 : 0
                };

                var formula = $"Allocation Formula: Income × Percentage / 100 = Allocated Amount\n" +
                             $"Total: ${monthlyIncome:F2} × 100% = ${totalAllocated:F2}\n" +
                             $"Surplus/Deficit: ${monthlyIncome:F2} - ${totalAllocated:F2} = ${surplusDeficit:F2}";

                var result = new AllocationCalculationDto
                {
                    MonthlyIncome = monthlyIncome,
                    Categories = categoryCalculations,
                    Summary = summary,
                    Formula = formula
                };

                return ApiResponse<AllocationCalculationDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationCalculationDto>.ErrorResult($"Failed to calculate allocation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationSummaryDto>> CalculateSummaryAsync(string planId, string userId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<AllocationSummaryDto>.ErrorResult("Plan not found");
                }

                var periodDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var actualsResponse = await GetCategoryActualsAsync(planId, userId, periodDate);
                if (!actualsResponse.Success || actualsResponse.Data == null)
                {
                    return ApiResponse<AllocationSummaryDto>.ErrorResult(actualsResponse.Message ?? "Failed to get category actuals");
                }
                var actuals = actualsResponse.Data;

                var totalAllocated = plan.Categories.Sum(c => c.AllocatedAmount);
                var totalActual = actuals.Values.Sum();
                var totalVariance = totalAllocated - totalActual;
                var surplusDeficit = plan.MonthlyIncome - totalAllocated;
                var allocationPercentage = plan.MonthlyIncome > 0 ? (totalAllocated / plan.MonthlyIncome) * 100 : 0;

                var summary = new AllocationSummaryDto
                {
                    TotalAllocated = totalAllocated,
                    TotalActual = totalActual,
                    TotalVariance = totalVariance,
                    SurplusDeficit = surplusDeficit,
                    AllocationPercentage = allocationPercentage
                };

                return ApiResponse<AllocationSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationSummaryDto>.ErrorResult($"Failed to calculate summary: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationChartDataDto>> GetChartDataAsync(string planId, string userId)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<AllocationChartDataDto>.ErrorResult("Plan not found");
                }

                var periodDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var actualsResponse = await GetCategoryActualsAsync(planId, userId, periodDate);
                if (!actualsResponse.Success || actualsResponse.Data == null)
                {
                    return ApiResponse<AllocationChartDataDto>.ErrorResult(actualsResponse.Message ?? "Failed to get category actuals");
                }
                var actuals = actualsResponse.Data;

                var dataPoints = plan.Categories.OrderBy(c => c.DisplayOrder).Select(c =>
                {
                    var actual = actuals.ContainsKey(c.CategoryName) ? actuals[c.CategoryName] : 0;
                    return new AllocationChartDataPointDto
                    {
                        CategoryName = c.CategoryName,
                        AllocatedAmount = c.AllocatedAmount,
                        ActualAmount = actual,
                        Percentage = c.Percentage,
                        Color = c.Color
                    };
                }).ToList();

                var totalAllocated = dataPoints.Sum(d => d.AllocatedAmount);
                var surplusDeficit = plan.MonthlyIncome - totalAllocated;

                var result = new AllocationChartDataDto
                {
                    DataPoints = dataPoints,
                    TotalIncome = plan.MonthlyIncome,
                    TotalAllocated = totalAllocated,
                    SurplusDeficit = surplusDeficit
                };

                return ApiResponse<AllocationChartDataDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationChartDataDto>.ErrorResult($"Failed to get chart data: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationChartDataDto>> GetChartDataForPeriodAsync(string planId, string userId, DateTime periodDate)
        {
            try
            {
                var normalizedDate = new DateTime(periodDate.Year, periodDate.Month, 1);
                var history = await _context.Set<AllocationHistory>()
                    .Where(h => h.UserId == userId && 
                               h.PlanId == planId && 
                               h.PeriodDate == normalizedDate &&
                               h.CategoryId != null)
                    .Include(h => h.Category)
                    .ToListAsync();

                if (!history.Any())
                {
                    return ApiResponse<AllocationChartDataDto>.ErrorResult("No history data for this period");
                }

                var dataPoints = history.Select(h => new AllocationChartDataPointDto
                {
                    CategoryName = h.Category?.CategoryName ?? "Unknown",
                    AllocatedAmount = h.AllocatedAmount,
                    ActualAmount = h.ActualAmount,
                    Percentage = h.AllocatedAmount > 0 ? (h.AllocatedAmount / history.Sum(hi => hi.AllocatedAmount)) * 100 : 0,
                    Color = h.Category?.Color
                }).ToList();

                var plan = await _context.Set<AllocationPlan>()
                    .FirstOrDefaultAsync(p => p.Id == planId);

                var result = new AllocationChartDataDto
                {
                    DataPoints = dataPoints,
                    TotalIncome = plan?.MonthlyIncome ?? 0,
                    TotalAllocated = history.Sum(h => h.AllocatedAmount),
                    SurplusDeficit = (plan?.MonthlyIncome ?? 0) - history.Sum(h => h.AllocatedAmount)
                };

                return ApiResponse<AllocationChartDataDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationChartDataDto>.ErrorResult($"Failed to get chart data: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AllocationPlanDto>> CreateCategoryBasedPlanAsync(decimal monthlyIncome, Dictionary<string, decimal> categoryPercentages, string userId)
        {
            try
            {
                var totalPercentage = categoryPercentages.Values.Sum();
                if (Math.Abs(totalPercentage - 100) > 0.01m)
                {
                    return ApiResponse<AllocationPlanDto>.ErrorResult(
                        $"Category percentages must sum to 100%. Current sum: {totalPercentage}%");
                }

                var categories = categoryPercentages.Select((kvp, index) => new CreateAllocationCategoryDto
                {
                    CategoryName = kvp.Key,
                    AllocatedAmount = monthlyIncome * kvp.Value / 100,
                    Percentage = kvp.Value,
                    DisplayOrder = index + 1
                }).ToList();

                var createDto = new CreateAllocationPlanDto
                {
                    PlanName = "Category-Based Allocation Plan",
                    MonthlyIncome = monthlyIncome,
                    Categories = categories
                };

                return await CreatePlanAsync(createDto, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<AllocationPlanDto>.ErrorResult($"Failed to create category-based plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetCategoryActualsAsync(string planId, string userId, DateTime? periodDate = null)
        {
            try
            {
                var plan = await _context.Set<AllocationPlan>()
                    .Where(p => p.Id == planId && p.UserId == userId)
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    return ApiResponse<Dictionary<string, decimal>>.ErrorResult("Plan not found");
                }

                var period = periodDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var startDate = period;
                var endDate = period.AddMonths(1).AddDays(-1);

                var actuals = new Dictionary<string, decimal>();

                // Get actual spending from various sources
                // This is a simplified mapping - in a real system, you'd map categories to transaction categories
                foreach (var category in plan.Categories)
                {
                    decimal actual = 0;

                    // Map category names to actual spending sources
                    var categoryNameLower = category.CategoryName.ToLower();

                    if (categoryNameLower.Contains("bills") || categoryNameLower.Contains("utilities"))
                    {
                        actual = await _context.Bills
                            .Where(b => b.UserId == userId && 
                                       b.DueDate >= startDate && 
                                       b.DueDate <= endDate &&
                                       (b.Status == "PENDING" || b.Status == "PAID"))
                            .SumAsync(b => b.Amount);
                    }
                    else if (categoryNameLower.Contains("loan") || categoryNameLower.Contains("debt"))
                    {
                        var loans = await _context.Loans
                            .Where(l => l.UserId == userId && 
                                       (l.Status == "ACTIVE" || l.Status == "APPROVED"))
                            .ToListAsync();
                        actual = loans.Sum(l => l.MonthlyPayment);
                    }
                    else if (categoryNameLower.Contains("savings") || categoryNameLower.Contains("emergency"))
                    {
                        actual = await _context.SavingsTransactions
                            .Where(st => st.SavingsAccount.UserId == userId &&
                                        st.TransactionDate >= startDate &&
                                        st.TransactionDate <= endDate &&
                                        st.TransactionType == "DEPOSIT")
                            .SumAsync(st => st.Amount);
                    }
                    else
                    {
                        // For other categories, use variable expenses or transactions
                        actual = await _context.VariableExpenses
                            .Where(v => v.UserId == userId &&
                                       v.ExpenseDate >= startDate &&
                                       v.ExpenseDate <= endDate)
                            .SumAsync(v => v.Amount);
                    }

                    actuals[category.CategoryName] = actual;
                }

                return ApiResponse<Dictionary<string, decimal>>.SuccessResult(actuals);
            }
            catch (Exception ex)
            {
                return ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get category actuals: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private AllocationTemplateDto MapToTemplateDto(AllocationTemplate template)
        {
            return new AllocationTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                IsSystemTemplate = template.IsSystemTemplate,
                IsActive = template.IsActive,
                Categories = template.Categories.OrderBy(c => c.DisplayOrder).Select(c => new AllocationTemplateCategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    Percentage = c.Percentage,
                    DisplayOrder = c.DisplayOrder,
                    Color = c.Color
                }).ToList(),
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }

        private async Task<AllocationPlanDto> MapToPlanDtoAsync(AllocationPlan plan)
        {
            var periodDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var actualsResponse = await GetCategoryActualsAsync(plan.Id, plan.UserId, periodDate);
            var actuals = actualsResponse.Success && actualsResponse.Data != null 
                ? actualsResponse.Data 
                : new Dictionary<string, decimal>();

            var categories = plan.Categories.OrderBy(c => c.DisplayOrder).Select(c =>
            {
                var actual = actuals.ContainsKey(c.CategoryName) ? actuals[c.CategoryName] : 0;
                var variance = c.AllocatedAmount - actual;
                var variancePercentage = c.AllocatedAmount > 0 ? (variance / c.AllocatedAmount) * 100 : 0;
                var status = variance < 0 ? "over_budget" : (variance > c.AllocatedAmount * 0.1m ? "under_budget" : "on_track");

                return new AllocationCategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    AllocatedAmount = c.AllocatedAmount,
                    Percentage = c.Percentage,
                    ActualAmount = actual,
                    Variance = variance,
                    VariancePercentage = variancePercentage,
                    Status = status,
                    DisplayOrder = c.DisplayOrder,
                    Color = c.Color
                };
            }).ToList();

            var totalAllocated = categories.Sum(c => c.AllocatedAmount);
            var totalActual = categories.Sum(c => c.ActualAmount);
            var totalVariance = totalAllocated - totalActual;
            var surplusDeficit = plan.MonthlyIncome - totalAllocated;
            var allocationPercentage = plan.MonthlyIncome > 0 ? (totalAllocated / plan.MonthlyIncome) * 100 : 0;

            var summary = new AllocationSummaryDto
            {
                TotalAllocated = totalAllocated,
                TotalActual = totalActual,
                TotalVariance = totalVariance,
                SurplusDeficit = surplusDeficit,
                AllocationPercentage = allocationPercentage
            };

            return new AllocationPlanDto
            {
                Id = plan.Id,
                UserId = plan.UserId,
                TemplateId = plan.TemplateId,
                TemplateName = plan.Template?.Name,
                PlanName = plan.PlanName,
                MonthlyIncome = plan.MonthlyIncome,
                IsActive = plan.IsActive,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Categories = categories,
                Summary = summary,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt
            };
        }

        #endregion
    }
}

