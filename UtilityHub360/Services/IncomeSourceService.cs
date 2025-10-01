using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class IncomeSourceService : IIncomeSourceService
    {
        private readonly ApplicationDbContext _context;

        public IncomeSourceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<IncomeSourceDto>> CreateIncomeSourceAsync(CreateIncomeSourceDto createIncomeSourceDto, string userId)
        {
            try
            {
                // Check if income source with same name already exists for user
                var existingSource = await _context.IncomeSources
                    .FirstOrDefaultAsync(i => i.UserId == userId && i.Name == createIncomeSourceDto.Name);

                if (existingSource != null)
                {
                    return ApiResponse<IncomeSourceDto>.ErrorResult("Income source with this name already exists");
                }

                var incomeSource = new IncomeSource
                {
                    UserId = userId,
                    Name = createIncomeSourceDto.Name,
                    Amount = createIncomeSourceDto.Amount,
                    Frequency = createIncomeSourceDto.Frequency.ToUpper(),
                    Category = createIncomeSourceDto.Category.ToUpper(),
                    Currency = createIncomeSourceDto.Currency,
                    Description = createIncomeSourceDto.Description,
                    Company = createIncomeSourceDto.Company,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.IncomeSources.Add(incomeSource);
                await _context.SaveChangesAsync();

                var incomeSourceDto = MapToIncomeSourceDto(incomeSource);
                return ApiResponse<IncomeSourceDto>.SuccessResult(incomeSourceDto, "Income source created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<IncomeSourceDto>.ErrorResult($"Failed to create income source: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IncomeSourceDto>> GetIncomeSourceAsync(string incomeSourceId, string userId)
        {
            try
            {
                var incomeSource = await _context.IncomeSources
                    .FirstOrDefaultAsync(i => i.Id == incomeSourceId && i.UserId == userId);

                if (incomeSource == null)
                {
                    return ApiResponse<IncomeSourceDto>.ErrorResult("Income source not found");
                }

                var incomeSourceDto = MapToIncomeSourceDto(incomeSource);
                return ApiResponse<IncomeSourceDto>.SuccessResult(incomeSourceDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<IncomeSourceDto>.ErrorResult($"Failed to retrieve income source: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<IncomeSourceDto>>> GetUserIncomeSourcesAsync(string userId, bool activeOnly = true)
        {
            try
            {
                var query = _context.IncomeSources.Where(i => i.UserId == userId);

                if (activeOnly)
                {
                    query = query.Where(i => i.IsActive);
                }

                var incomeSources = await query
                    .OrderBy(i => i.Category)
                    .ThenBy(i => i.Name)
                    .ToListAsync();

                var incomeSourceDtos = incomeSources.Select(MapToIncomeSourceDto).ToList();
                return ApiResponse<List<IncomeSourceDto>>.SuccessResult(incomeSourceDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to retrieve income sources: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IncomeSourceDto>> UpdateIncomeSourceAsync(string incomeSourceId, UpdateIncomeSourceDto updateIncomeSourceDto, string userId)
        {
            try
            {
                var incomeSource = await _context.IncomeSources
                    .FirstOrDefaultAsync(i => i.Id == incomeSourceId && i.UserId == userId);

                if (incomeSource == null)
                {
                    return ApiResponse<IncomeSourceDto>.ErrorResult("Income source not found");
                }

                // Check if name is being changed and if new name already exists
                if (!string.IsNullOrEmpty(updateIncomeSourceDto.Name) && updateIncomeSourceDto.Name != incomeSource.Name)
                {
                    var existingSource = await _context.IncomeSources
                        .FirstOrDefaultAsync(i => i.UserId == userId && i.Name == updateIncomeSourceDto.Name && i.Id != incomeSourceId);

                    if (existingSource != null)
                    {
                        return ApiResponse<IncomeSourceDto>.ErrorResult("Income source with this name already exists");
                    }
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateIncomeSourceDto.Name))
                    incomeSource.Name = updateIncomeSourceDto.Name;
                if (updateIncomeSourceDto.Amount.HasValue)
                    incomeSource.Amount = updateIncomeSourceDto.Amount.Value;
                if (!string.IsNullOrEmpty(updateIncomeSourceDto.Frequency))
                    incomeSource.Frequency = updateIncomeSourceDto.Frequency.ToUpper();
                if (!string.IsNullOrEmpty(updateIncomeSourceDto.Category))
                    incomeSource.Category = updateIncomeSourceDto.Category.ToUpper();
                if (!string.IsNullOrEmpty(updateIncomeSourceDto.Currency))
                    incomeSource.Currency = updateIncomeSourceDto.Currency;
                if (updateIncomeSourceDto.Description != null)
                    incomeSource.Description = updateIncomeSourceDto.Description;
                if (updateIncomeSourceDto.Company != null)
                    incomeSource.Company = updateIncomeSourceDto.Company;
                if (updateIncomeSourceDto.IsActive.HasValue)
                    incomeSource.IsActive = updateIncomeSourceDto.IsActive.Value;

                incomeSource.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var incomeSourceDto = MapToIncomeSourceDto(incomeSource);
                return ApiResponse<IncomeSourceDto>.SuccessResult(incomeSourceDto, "Income source updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<IncomeSourceDto>.ErrorResult($"Failed to update income source: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteIncomeSourceAsync(string incomeSourceId, string userId)
        {
            try
            {
                var incomeSource = await _context.IncomeSources
                    .FirstOrDefaultAsync(i => i.Id == incomeSourceId && i.UserId == userId);

                if (incomeSource == null)
                {
                    return ApiResponse<bool>.ErrorResult("Income source not found");
                }

                _context.IncomeSources.Remove(incomeSource);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Income source deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete income source: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ToggleIncomeSourceStatusAsync(string incomeSourceId, string userId)
        {
            try
            {
                var incomeSource = await _context.IncomeSources
                    .FirstOrDefaultAsync(i => i.Id == incomeSourceId && i.UserId == userId);

                if (incomeSource == null)
                {
                    return ApiResponse<bool>.ErrorResult("Income source not found");
                }

                incomeSource.IsActive = !incomeSource.IsActive;
                incomeSource.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = incomeSource.IsActive ? "activated" : "deactivated";
                return ApiResponse<bool>.SuccessResult(true, $"Income source {status} successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to toggle income source status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<IncomeSourceDto>>> CreateMultipleIncomeSourcesAsync(CreateMultipleIncomeSourcesDto createMultipleDto, string userId)
        {
            try
            {
                var createdSources = new List<IncomeSourceDto>();

                foreach (var createDto in createMultipleDto.IncomeSources)
                {
                    var result = await CreateIncomeSourceAsync(createDto, userId);
                    if (result.Success)
                    {
                        createdSources.Add(result.Data!);
                    }
                    else
                    {
                        return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to create income source '{createDto.Name}': {result.Message}");
                    }
                }

                return ApiResponse<List<IncomeSourceDto>>.SuccessResult(createdSources, "All income sources created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to create multiple income sources: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<IncomeSourceDto>>> UpdateMultipleIncomeSourcesAsync(UpdateMultipleIncomeSourcesDto updateMultipleDto, string userId)
        {
            try
            {
                var updatedSources = new List<IncomeSourceDto>();

                foreach (var updateDto in updateMultipleDto.IncomeSources)
                {
                    // This would need income source IDs in the DTO for updates
                    // For now, we'll return an error suggesting to use individual updates
                    return ApiResponse<List<IncomeSourceDto>>.ErrorResult("Bulk updates require individual income source IDs. Please use individual update endpoints.");
                }

                return ApiResponse<List<IncomeSourceDto>>.SuccessResult(updatedSources, "Income sources updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to update multiple income sources: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IncomeSummaryDto>> GetIncomeSummaryAsync(string userId)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();

                var totalMonthlyIncome = incomeSources.Sum(i => i.MonthlyAmount);

                // Get user profile for tax and goals information
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                var monthlyTaxDeductions = userProfile?.MonthlyTaxDeductions ?? 0;
                var netMonthlyIncome = totalMonthlyIncome - monthlyTaxDeductions;

                var monthlySavingsGoal = userProfile?.MonthlySavingsGoal ?? 0;
                var monthlyInvestmentGoal = userProfile?.MonthlyInvestmentGoal ?? 0;
                var monthlyEmergencyFundGoal = userProfile?.MonthlyEmergencyFundGoal ?? 0;
                var totalMonthlyGoals = monthlySavingsGoal + monthlyInvestmentGoal + monthlyEmergencyFundGoal;

                var disposableIncome = netMonthlyIncome - totalMonthlyGoals;
                var savingsRate = totalMonthlyIncome > 0 ? (totalMonthlyGoals / totalMonthlyIncome) * 100 : 0;

                var incomeByCategory = incomeSources
                    .GroupBy(i => i.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                var incomeByFrequency = incomeSources
                    .GroupBy(i => i.Frequency)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                var goalsBreakdown = new Dictionary<string, decimal>
                {
                    ["Savings Goal"] = monthlySavingsGoal,
                    ["Investment Goal"] = monthlyInvestmentGoal,
                    ["Emergency Fund Goal"] = monthlyEmergencyFundGoal
                };

                var summary = new IncomeSummaryDto
                {
                    TotalMonthlyIncome = totalMonthlyIncome,
                    NetMonthlyIncome = netMonthlyIncome,
                    TotalMonthlyGoals = totalMonthlyGoals,
                    DisposableIncome = disposableIncome,
                    SavingsRate = savingsRate,
                    IncomeByCategory = incomeByCategory,
                    IncomeByFrequency = incomeByFrequency,
                    IncomeSources = incomeSources.Select(MapToIncomeSourceDto).ToList(),
                    GoalsBreakdown = goalsBreakdown
                };

                return ApiResponse<IncomeSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<IncomeSummaryDto>.ErrorResult($"Failed to get income summary: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IncomeAnalyticsDto>> GetIncomeAnalyticsAsync(string userId, string period = "month")
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();

                var totalMonthlyIncome = incomeSources.Sum(i => i.MonthlyAmount);

                var categoryDistribution = incomeSources
                    .GroupBy(i => i.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                var frequencyDistribution = incomeSources
                    .GroupBy(i => i.Frequency)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                var topIncomeSources = incomeSources
                    .OrderByDescending(i => i.MonthlyAmount)
                    .Take(5)
                    .Select(i => i.Name)
                    .ToList();

                var analytics = new IncomeAnalyticsDto
                {
                    AverageMonthlyIncome = totalMonthlyIncome, // For now, using current data
                    HighestMonthlyIncome = totalMonthlyIncome,
                    LowestMonthlyIncome = totalMonthlyIncome,
                    IncomeTrend = new Dictionary<string, decimal> { [DateTime.UtcNow.ToString("yyyy-MM")] = totalMonthlyIncome },
                    CategoryDistribution = categoryDistribution,
                    FrequencyDistribution = frequencyDistribution,
                    TopIncomeSources = topIncomeSources
                };

                return ApiResponse<IncomeAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return ApiResponse<IncomeAnalyticsDto>.ErrorResult($"Failed to get income analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetIncomeByCategoryAsync(string userId)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();

                var incomeByCategory = incomeSources
                    .GroupBy(i => i.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                return ApiResponse<Dictionary<string, decimal>>.SuccessResult(incomeByCategory);
            }
            catch (Exception ex)
            {
                return ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get income by category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetIncomeByFrequencyAsync(string userId)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();

                var incomeByFrequency = incomeSources
                    .GroupBy(i => i.Frequency)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.MonthlyAmount));

                return ApiResponse<Dictionary<string, decimal>>.SuccessResult(incomeByFrequency);
            }
            catch (Exception ex)
            {
                return ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get income by frequency: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalMonthlyIncomeAsync(string userId)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.IsActive)
                    .ToListAsync();

                var totalIncome = incomeSources.Sum(i => i.MonthlyAmount);

                return ApiResponse<decimal>.SuccessResult(totalIncome);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total monthly income: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetNetMonthlyIncomeAsync(string userId)
        {
            try
            {
                var totalIncome = await GetTotalMonthlyIncomeAsync(userId);
                if (!totalIncome.Success)
                {
                    return totalIncome;
                }

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                var monthlyTaxDeductions = userProfile?.MonthlyTaxDeductions ?? 0;
                var netIncome = totalIncome.Data - monthlyTaxDeductions;

                return ApiResponse<decimal>.SuccessResult(netIncome);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get net monthly income: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<string>>> GetAvailableCategoriesAsync()
        {
            try
            {
                var categories = new List<string>
                {
                    "PRIMARY",
                    "PASSIVE",
                    "BUSINESS",
                    "SIDE_HUSTLE",
                    "INVESTMENT",
                    "RENTAL",
                    "DIVIDEND",
                    "INTEREST",
                    "OTHER"
                };

                return ApiResponse<List<string>>.SuccessResult(categories);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<string>>.ErrorResult($"Failed to get available categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<string>>> GetAvailableFrequenciesAsync()
        {
            try
            {
                var frequencies = new List<string>
                {
                    "WEEKLY",
                    "BI_WEEKLY",
                    "MONTHLY",
                    "QUARTERLY",
                    "ANNUALLY"
                };

                return ApiResponse<List<string>>.SuccessResult(frequencies);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<string>>.ErrorResult($"Failed to get available frequencies: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<IncomeSourceDto>>> GetIncomeSourcesByCategoryAsync(string userId, string category)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.Category == category.ToUpper() && i.IsActive)
                    .OrderBy(i => i.Name)
                    .ToListAsync();

                var incomeSourceDtos = incomeSources.Select(MapToIncomeSourceDto).ToList();
                return ApiResponse<List<IncomeSourceDto>>.SuccessResult(incomeSourceDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to get income sources by category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<IncomeSourceDto>>> GetIncomeSourcesByFrequencyAsync(string userId, string frequency)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId && i.Frequency == frequency.ToUpper() && i.IsActive)
                    .OrderBy(i => i.Name)
                    .ToListAsync();

                var incomeSourceDtos = incomeSources.Select(MapToIncomeSourceDto).ToList();
                return ApiResponse<List<IncomeSourceDto>>.SuccessResult(incomeSourceDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to get income sources by frequency: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<IncomeSourceDto>>> GetAllIncomeSourcesAsync(int page = 1, int limit = 50)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .OrderBy(i => i.UserId)
                    .ThenBy(i => i.Category)
                    .ThenBy(i => i.Name)
                    .ToListAsync();

                var incomeSourceDtos = incomeSources.Select(MapToIncomeSourceDto).ToList();
                return ApiResponse<List<IncomeSourceDto>>.SuccessResult(incomeSourceDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to get all income sources: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<IncomeSourceDto>>> GetIncomeSourcesByUserAsync(string userId, int page = 1, int limit = 50)
        {
            try
            {
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .OrderBy(i => i.Category)
                    .ThenBy(i => i.Name)
                    .ToListAsync();

                var incomeSourceDtos = incomeSources.Select(MapToIncomeSourceDto).ToList();
                return ApiResponse<List<IncomeSourceDto>>.SuccessResult(incomeSourceDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IncomeSourceDto>>.ErrorResult($"Failed to get income sources by user: {ex.Message}");
            }
        }

        private static IncomeSourceDto MapToIncomeSourceDto(IncomeSource incomeSource)
        {
            return new IncomeSourceDto
            {
                Id = incomeSource.Id,
                UserId = incomeSource.UserId,
                Name = incomeSource.Name,
                Amount = incomeSource.Amount,
                Frequency = incomeSource.Frequency,
                Category = incomeSource.Category,
                Currency = incomeSource.Currency,
                IsActive = incomeSource.IsActive,
                Description = incomeSource.Description,
                Company = incomeSource.Company,
                CreatedAt = incomeSource.CreatedAt,
                UpdatedAt = incomeSource.UpdatedAt,
                MonthlyAmount = incomeSource.MonthlyAmount
            };
        }
    }
}
