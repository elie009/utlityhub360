using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Subscription Plan Management
        public async Task<ApiResponse<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId)
        {
            try
            {
                var plan = await _context.SubscriptionPlans.FindAsync(planId);
                if (plan == null)
                {
                    return ApiResponse<SubscriptionPlanDto>.ErrorResult("Subscription plan not found");
                }

                var dto = MapToSubscriptionPlanDto(plan);
                return ApiResponse<SubscriptionPlanDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<SubscriptionPlanDto>.ErrorResult($"Failed to get subscription plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SubscriptionPlanDto>>> GetAllSubscriptionPlansAsync(bool activeOnly = false)
        {
            try
            {
                var query = _context.SubscriptionPlans.AsQueryable();
                if (activeOnly)
                {
                    query = query.Where(p => p.IsActive);
                }

                var plans = await query
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.MonthlyPrice)
                    .ToListAsync();

                var dtos = plans.Select(MapToSubscriptionPlanDto).ToList();
                return ApiResponse<List<SubscriptionPlanDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SubscriptionPlanDto>>.ErrorResult($"Failed to get subscription plans: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SubscriptionPlanDto>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto createDto)
        {
            try
            {
                var plan = new SubscriptionPlan
                {
                    Name = createDto.Name,
                    DisplayName = createDto.DisplayName,
                    Description = createDto.Description,
                    MonthlyPrice = createDto.MonthlyPrice,
                    YearlyPrice = createDto.YearlyPrice,
                    MaxBankAccounts = createDto.MaxBankAccounts,
                    MaxTransactionsPerMonth = createDto.MaxTransactionsPerMonth,
                    MaxBillsPerMonth = createDto.MaxBillsPerMonth,
                    MaxLoans = createDto.MaxLoans,
                    MaxSavingsGoals = createDto.MaxSavingsGoals,
                    MaxReceiptOcrPerMonth = createDto.MaxReceiptOcrPerMonth,
                    MaxAiQueriesPerMonth = createDto.MaxAiQueriesPerMonth,
                    MaxApiCallsPerMonth = createDto.MaxApiCallsPerMonth,
                    MaxUsers = createDto.MaxUsers,
                    TransactionHistoryMonths = createDto.TransactionHistoryMonths,
                    HasAiAssistant = createDto.HasAiAssistant,
                    HasBankFeedIntegration = createDto.HasBankFeedIntegration,
                    HasReceiptOcr = createDto.HasReceiptOcr,
                    HasAdvancedReports = createDto.HasAdvancedReports,
                    HasPrioritySupport = createDto.HasPrioritySupport,
                    HasApiAccess = createDto.HasApiAccess,
                    HasInvestmentTracking = createDto.HasInvestmentTracking,
                    HasTaxOptimization = createDto.HasTaxOptimization,
                    HasMultiUserSupport = createDto.HasMultiUserSupport,
                    HasWhiteLabelOptions = createDto.HasWhiteLabelOptions,
                    HasCustomIntegrations = createDto.HasCustomIntegrations,
                    HasDedicatedSupport = createDto.HasDedicatedSupport,
                    HasAccountManager = createDto.HasAccountManager,
                    HasCustomReporting = createDto.HasCustomReporting,
                    HasAdvancedSecurity = createDto.HasAdvancedSecurity,
                    HasComplianceReports = createDto.HasComplianceReports,
                    HasFinancialHealthScore = createDto.HasFinancialHealthScore,
                    HasBillForecasting = createDto.HasBillForecasting,
                    HasDebtOptimizer = createDto.HasDebtOptimizer,
                    DisplayOrder = createDto.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SubscriptionPlans.Add(plan);
                await _context.SaveChangesAsync();

                var dto = MapToSubscriptionPlanDto(plan);
                return ApiResponse<SubscriptionPlanDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<SubscriptionPlanDto>.ErrorResult($"Failed to create subscription plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SubscriptionPlanDto>> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto)
        {
            try
            {
                var plan = await _context.SubscriptionPlans.FindAsync(planId);
                if (plan == null)
                {
                    return ApiResponse<SubscriptionPlanDto>.ErrorResult("Subscription plan not found");
                }

                if (updateDto.DisplayName != null) plan.DisplayName = updateDto.DisplayName;
                if (updateDto.Description != null) plan.Description = updateDto.Description;
                if (updateDto.MonthlyPrice.HasValue) plan.MonthlyPrice = updateDto.MonthlyPrice.Value;
                if (updateDto.YearlyPrice.HasValue) plan.YearlyPrice = updateDto.YearlyPrice;
                if (updateDto.MaxBankAccounts.HasValue) plan.MaxBankAccounts = updateDto.MaxBankAccounts;
                if (updateDto.MaxTransactionsPerMonth.HasValue) plan.MaxTransactionsPerMonth = updateDto.MaxTransactionsPerMonth;
                if (updateDto.MaxBillsPerMonth.HasValue) plan.MaxBillsPerMonth = updateDto.MaxBillsPerMonth;
                if (updateDto.MaxLoans.HasValue) plan.MaxLoans = updateDto.MaxLoans;
                if (updateDto.MaxSavingsGoals.HasValue) plan.MaxSavingsGoals = updateDto.MaxSavingsGoals;
                if (updateDto.MaxReceiptOcrPerMonth.HasValue) plan.MaxReceiptOcrPerMonth = updateDto.MaxReceiptOcrPerMonth;
                if (updateDto.MaxAiQueriesPerMonth.HasValue) plan.MaxAiQueriesPerMonth = updateDto.MaxAiQueriesPerMonth;
                if (updateDto.MaxApiCallsPerMonth.HasValue) plan.MaxApiCallsPerMonth = updateDto.MaxApiCallsPerMonth;
                if (updateDto.MaxUsers.HasValue) plan.MaxUsers = updateDto.MaxUsers;
                if (updateDto.TransactionHistoryMonths.HasValue) plan.TransactionHistoryMonths = updateDto.TransactionHistoryMonths;
                if (updateDto.HasAiAssistant.HasValue) plan.HasAiAssistant = updateDto.HasAiAssistant.Value;
                if (updateDto.HasBankFeedIntegration.HasValue) plan.HasBankFeedIntegration = updateDto.HasBankFeedIntegration.Value;
                if (updateDto.HasReceiptOcr.HasValue) plan.HasReceiptOcr = updateDto.HasReceiptOcr.Value;
                if (updateDto.HasAdvancedReports.HasValue) plan.HasAdvancedReports = updateDto.HasAdvancedReports.Value;
                if (updateDto.HasPrioritySupport.HasValue) plan.HasPrioritySupport = updateDto.HasPrioritySupport.Value;
                if (updateDto.HasApiAccess.HasValue) plan.HasApiAccess = updateDto.HasApiAccess.Value;
                if (updateDto.HasInvestmentTracking.HasValue) plan.HasInvestmentTracking = updateDto.HasInvestmentTracking.Value;
                if (updateDto.HasTaxOptimization.HasValue) plan.HasTaxOptimization = updateDto.HasTaxOptimization.Value;
                if (updateDto.HasMultiUserSupport.HasValue) plan.HasMultiUserSupport = updateDto.HasMultiUserSupport.Value;
                if (updateDto.HasWhiteLabelOptions.HasValue) plan.HasWhiteLabelOptions = updateDto.HasWhiteLabelOptions.Value;
                if (updateDto.HasCustomIntegrations.HasValue) plan.HasCustomIntegrations = updateDto.HasCustomIntegrations.Value;
                if (updateDto.HasDedicatedSupport.HasValue) plan.HasDedicatedSupport = updateDto.HasDedicatedSupport.Value;
                if (updateDto.HasAccountManager.HasValue) plan.HasAccountManager = updateDto.HasAccountManager.Value;
                if (updateDto.HasCustomReporting.HasValue) plan.HasCustomReporting = updateDto.HasCustomReporting.Value;
                if (updateDto.HasAdvancedSecurity.HasValue) plan.HasAdvancedSecurity = updateDto.HasAdvancedSecurity.Value;
                if (updateDto.HasComplianceReports.HasValue) plan.HasComplianceReports = updateDto.HasComplianceReports.Value;
                if (updateDto.HasFinancialHealthScore.HasValue) plan.HasFinancialHealthScore = updateDto.HasFinancialHealthScore.Value;
                if (updateDto.HasBillForecasting.HasValue) plan.HasBillForecasting = updateDto.HasBillForecasting.Value;
                if (updateDto.HasDebtOptimizer.HasValue) plan.HasDebtOptimizer = updateDto.HasDebtOptimizer.Value;
                if (updateDto.IsActive.HasValue) plan.IsActive = updateDto.IsActive.Value;
                if (updateDto.DisplayOrder.HasValue) plan.DisplayOrder = updateDto.DisplayOrder.Value;

                plan.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var dto = MapToSubscriptionPlanDto(plan);
                return ApiResponse<SubscriptionPlanDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<SubscriptionPlanDto>.ErrorResult($"Failed to update subscription plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteSubscriptionPlanAsync(string planId)
        {
            try
            {
                var plan = await _context.SubscriptionPlans.FindAsync(planId);
                if (plan == null)
                {
                    return ApiResponse<bool>.ErrorResult("Subscription plan not found");
                }

                // Check if any users are subscribed to this plan
                var hasSubscriptions = await _context.UserSubscriptions
                    .AnyAsync(us => us.SubscriptionPlanId == planId && us.Status == "ACTIVE");

                if (hasSubscriptions)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete subscription plan with active subscriptions. Deactivate it instead.");
                }

                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete subscription plan: {ex.Message}");
            }
        }

        // User Subscription Management
        public async Task<ApiResponse<UserSubscriptionDto>> GetUserSubscriptionAsync(string userId)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Include(us => us.User)
                    .Where(us => us.UserId == userId)
                    .OrderByDescending(us => us.CreatedAt)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    return ApiResponse<UserSubscriptionDto>.ErrorResult("User subscription not found");
                }

                var dto = MapToUserSubscriptionDto(subscription);
                return ApiResponse<UserSubscriptionDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserSubscriptionDto>.ErrorResult($"Failed to get user subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserSubscriptionDto>> CreateUserSubscriptionAsync(CreateUserSubscriptionDto createDto)
        {
            try
            {
                // Check if user already has an active subscription
                var existingSubscription = await _context.UserSubscriptions
                    .Where(us => us.UserId == createDto.UserId && us.Status == "ACTIVE")
                    .FirstOrDefaultAsync();

                if (existingSubscription != null)
                {
                    return ApiResponse<UserSubscriptionDto>.ErrorResult("User already has an active subscription");
                }

                var plan = await _context.SubscriptionPlans.FindAsync(createDto.SubscriptionPlanId);
                if (plan == null)
                {
                    return ApiResponse<UserSubscriptionDto>.ErrorResult("Subscription plan not found");
                }

                var startDate = createDto.StartDate ?? DateTime.UtcNow;
                var price = createDto.BillingCycle == "YEARLY" && plan.YearlyPrice.HasValue
                    ? plan.YearlyPrice.Value
                    : plan.MonthlyPrice;

                var nextBillingDate = createDto.BillingCycle == "YEARLY"
                    ? startDate.AddYears(1)
                    : startDate.AddMonths(1);

                var subscription = new UserSubscription
                {
                    UserId = createDto.UserId,
                    SubscriptionPlanId = createDto.SubscriptionPlanId,
                    Status = "ACTIVE",
                    BillingCycle = createDto.BillingCycle,
                    CurrentPrice = price,
                    StartDate = startDate,
                    NextBillingDate = nextBillingDate,
                    TrialEndDate = createDto.TrialEndDate,
                    LastUsageResetDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                var subscriptionWithIncludes = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => us.Id == subscription.Id);

                var dto = MapToUserSubscriptionDto(subscriptionWithIncludes!);
                return ApiResponse<UserSubscriptionDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserSubscriptionDto>.ErrorResult($"Failed to create user subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserSubscriptionDto>> UpdateUserSubscriptionAsync(string subscriptionId, UpdateUserSubscriptionDto updateDto)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => us.Id == subscriptionId);

                if (subscription == null)
                {
                    return ApiResponse<UserSubscriptionDto>.ErrorResult("User subscription not found");
                }

                if (updateDto.SubscriptionPlanId != null)
                {
                    var newPlan = await _context.SubscriptionPlans.FindAsync(updateDto.SubscriptionPlanId);
                    if (newPlan == null)
                    {
                        return ApiResponse<UserSubscriptionDto>.ErrorResult("New subscription plan not found");
                    }
                    subscription.SubscriptionPlanId = updateDto.SubscriptionPlanId;
                    subscription.CurrentPrice = subscription.BillingCycle == "YEARLY" && newPlan.YearlyPrice.HasValue
                        ? newPlan.YearlyPrice.Value
                        : newPlan.MonthlyPrice;
                }

                if (updateDto.Status != null) subscription.Status = updateDto.Status;
                if (updateDto.BillingCycle != null)
                {
                    subscription.BillingCycle = updateDto.BillingCycle;
                    var plan = await _context.SubscriptionPlans.FindAsync(subscription.SubscriptionPlanId);
                    if (plan != null)
                    {
                        subscription.CurrentPrice = updateDto.BillingCycle == "YEARLY" && plan.YearlyPrice.HasValue
                            ? plan.YearlyPrice.Value
                            : plan.MonthlyPrice;
                    }
                }
                if (updateDto.EndDate.HasValue) subscription.EndDate = updateDto.EndDate;
                if (updateDto.NextBillingDate.HasValue) subscription.NextBillingDate = updateDto.NextBillingDate;

                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var updatedSubscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => us.Id == subscriptionId);

                var dto = MapToUserSubscriptionDto(updatedSubscription!);
                return ApiResponse<UserSubscriptionDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserSubscriptionDto>.ErrorResult($"Failed to update user subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelUserSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var subscription = await _context.UserSubscriptions.FindAsync(subscriptionId);
                if (subscription == null)
                {
                    return ApiResponse<bool>.ErrorResult("User subscription not found");
                }

                subscription.Status = "CANCELLED";
                subscription.CancelledAt = DateTime.UtcNow;
                subscription.EndDate = subscription.NextBillingDate; // End at next billing date
                subscription.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to cancel subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserSubscriptionDto>>> GetAllUserSubscriptionsAsync(int page = 1, int limit = 50, string? status = null, string? planId = null)
        {
            try
            {
                var query = _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Include(us => us.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(us => us.Status == status);
                }

                if (!string.IsNullOrEmpty(planId))
                {
                    query = query.Where(us => us.SubscriptionPlanId == planId);
                }

                var totalCount = await query.CountAsync();
                var subscriptions = await query
                    .OrderByDescending(us => us.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var dtos = subscriptions.Select(MapToUserSubscriptionDto).ToList();
                return ApiResponse<List<UserSubscriptionDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserSubscriptionDto>>.ErrorResult($"Failed to get user subscriptions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserWithSubscriptionDto>> GetUserWithSubscriptionAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserWithSubscriptionDto>.ErrorResult("User not found");
                }

                var subscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Where(us => us.UserId == userId && us.Status == "ACTIVE")
                    .OrderByDescending(us => us.CreatedAt)
                    .FirstOrDefaultAsync();

                var dto = new UserWithSubscriptionDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone ?? string.Empty,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    SubscriptionPlanId = subscription?.SubscriptionPlanId,
                    SubscriptionPlanName = subscription?.SubscriptionPlan?.Name,
                    SubscriptionStatus = subscription?.Status,
                    SubscriptionBillingCycle = subscription?.BillingCycle,
                    SubscriptionStartDate = subscription?.StartDate,
                    SubscriptionEndDate = subscription?.EndDate,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return ApiResponse<UserWithSubscriptionDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserWithSubscriptionDto>.ErrorResult($"Failed to get user with subscription: {ex.Message}");
            }
        }

        // Usage Tracking
        public async Task<ApiResponse<bool>> IncrementUsageAsync(string userId, string usageType, int amount = 1)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Where(us => us.UserId == userId && us.Status == "ACTIVE")
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    return ApiResponse<bool>.ErrorResult("User does not have an active subscription");
                }

                // Reset usage if it's a new month
                if (subscription.LastUsageResetDate == null || 
                    subscription.LastUsageResetDate.Value.Month != DateTime.UtcNow.Month ||
                    subscription.LastUsageResetDate.Value.Year != DateTime.UtcNow.Year)
                {
                    subscription.TransactionsThisMonth = 0;
                    subscription.BillsThisMonth = 0;
                    subscription.ReceiptOcrThisMonth = 0;
                    subscription.AiQueriesThisMonth = 0;
                    subscription.ApiCallsThisMonth = 0;
                    subscription.LastUsageResetDate = DateTime.UtcNow;
                }

                switch (usageType.ToUpper())
                {
                    case "TRANSACTION":
                        subscription.TransactionsThisMonth += amount;
                        break;
                    case "BILL":
                        subscription.BillsThisMonth += amount;
                        break;
                    case "RECEIPT_OCR":
                        subscription.ReceiptOcrThisMonth += amount;
                        break;
                    case "AI_QUERY":
                        subscription.AiQueriesThisMonth += amount;
                        break;
                    case "API_CALL":
                        subscription.ApiCallsThisMonth += amount;
                        break;
                }

                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to increment usage: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ResetMonthlyUsageAsync(string userId)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Where(us => us.UserId == userId && us.Status == "ACTIVE")
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    return ApiResponse<bool>.ErrorResult("User does not have an active subscription");
                }

                subscription.TransactionsThisMonth = 0;
                subscription.BillsThisMonth = 0;
                subscription.ReceiptOcrThisMonth = 0;
                subscription.AiQueriesThisMonth = 0;
                subscription.ApiCallsThisMonth = 0;
                subscription.LastUsageResetDate = DateTime.UtcNow;
                subscription.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to reset usage: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> GetUsageStatsAsync(string userId)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Where(us => us.UserId == userId && us.Status == "ACTIVE")
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    return ApiResponse<object>.ErrorResult("User does not have an active subscription");
                }

                var stats = new
                {
                    TransactionsThisMonth = subscription.TransactionsThisMonth,
                    TransactionsLimit = subscription.SubscriptionPlan.MaxTransactionsPerMonth,
                    BillsThisMonth = subscription.BillsThisMonth,
                    BillsLimit = subscription.SubscriptionPlan.MaxBillsPerMonth,
                    ReceiptOcrThisMonth = subscription.ReceiptOcrThisMonth,
                    ReceiptOcrLimit = subscription.SubscriptionPlan.MaxReceiptOcrPerMonth,
                    AiQueriesThisMonth = subscription.AiQueriesThisMonth,
                    AiQueriesLimit = subscription.SubscriptionPlan.MaxAiQueriesPerMonth,
                    ApiCallsThisMonth = subscription.ApiCallsThisMonth,
                    ApiCallsLimit = subscription.SubscriptionPlan.MaxApiCallsPerMonth,
                    LastUsageResetDate = subscription.LastUsageResetDate
                };

                return ApiResponse<object>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult($"Failed to get usage stats: {ex.Message}");
            }
        }

        // Subscription Validation
        public async Task<ApiResponse<bool>> CheckFeatureAccessAsync(string userId, string feature)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Where(us => us.UserId == userId && us.Status == "ACTIVE")
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    // Default to free plan features
                    return ApiResponse<bool>.SuccessResult(false);
                }

                var plan = subscription.SubscriptionPlan;
                var hasAccess = feature.ToUpper() switch
                {
                    "AI_ASSISTANT" => plan.HasAiAssistant,
                    "BANK_FEED" => plan.HasBankFeedIntegration,
                    "RECEIPT_OCR" => plan.HasReceiptOcr,
                    "ADVANCED_REPORTS" => plan.HasAdvancedReports,
                    "PRIORITY_SUPPORT" => plan.HasPrioritySupport,
                    "API_ACCESS" => plan.HasApiAccess,
                    "INVESTMENT_TRACKING" => plan.HasInvestmentTracking,
                    "TAX_OPTIMIZATION" => plan.HasTaxOptimization,
                    "MULTI_USER" => plan.HasMultiUserSupport,
                    "WHITE_LABEL" => plan.HasWhiteLabelOptions,
                    "CUSTOM_INTEGRATIONS" => plan.HasCustomIntegrations,
                    "DEDICATED_SUPPORT" => plan.HasDedicatedSupport,
                    "ACCOUNT_MANAGER" => plan.HasAccountManager,
                    "CUSTOM_REPORTING" => plan.HasCustomReporting,
                    "ADVANCED_SECURITY" => plan.HasAdvancedSecurity,
                    "COMPLIANCE_REPORTS" => plan.HasComplianceReports,
                    "FINANCIAL_HEALTH_SCORE" => plan.HasFinancialHealthScore,
                    "BILL_FORECASTING" => plan.HasBillForecasting,
                    "DEBT_OPTIMIZER" => plan.HasDebtOptimizer,
                    _ => false
                };

                return ApiResponse<bool>.SuccessResult(hasAccess);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to check feature access: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CheckLimitAsync(string userId, string limitType, int currentCount)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Include(us => us.SubscriptionPlan)
                    .Where(us => us.UserId == userId && us.Status == "ACTIVE")
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    // Default to free plan limits
                    return ApiResponse<bool>.SuccessResult(true);
                }

                var plan = subscription.SubscriptionPlan;
                int? limit = limitType.ToUpper() switch
                {
                    "BANK_ACCOUNTS" => plan.MaxBankAccounts,
                    "TRANSACTIONS" => plan.MaxTransactionsPerMonth,
                    "BILLS" => plan.MaxBillsPerMonth,
                    "LOANS" => plan.MaxLoans,
                    "SAVINGS_GOALS" => plan.MaxSavingsGoals,
                    "RECEIPT_OCR" => plan.MaxReceiptOcrPerMonth,
                    "AI_QUERIES" => plan.MaxAiQueriesPerMonth,
                    "API_CALLS" => plan.MaxApiCallsPerMonth,
                    "USERS" => plan.MaxUsers,
                    _ => null
                };

                // null limit means unlimited
                if (limit == null)
                {
                    return ApiResponse<bool>.SuccessResult(true);
                }

                return ApiResponse<bool>.SuccessResult(currentCount < limit.Value);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to check limit: {ex.Message}");
            }
        }

        // Helper methods
        private SubscriptionPlanDto MapToSubscriptionPlanDto(SubscriptionPlan plan)
        {
            return new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                DisplayName = plan.DisplayName,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                YearlyPrice = plan.YearlyPrice,
                MaxBankAccounts = plan.MaxBankAccounts,
                MaxTransactionsPerMonth = plan.MaxTransactionsPerMonth,
                MaxBillsPerMonth = plan.MaxBillsPerMonth,
                MaxLoans = plan.MaxLoans,
                MaxSavingsGoals = plan.MaxSavingsGoals,
                MaxReceiptOcrPerMonth = plan.MaxReceiptOcrPerMonth,
                MaxAiQueriesPerMonth = plan.MaxAiQueriesPerMonth,
                MaxApiCallsPerMonth = plan.MaxApiCallsPerMonth,
                MaxUsers = plan.MaxUsers,
                TransactionHistoryMonths = plan.TransactionHistoryMonths,
                HasAiAssistant = plan.HasAiAssistant,
                HasBankFeedIntegration = plan.HasBankFeedIntegration,
                HasReceiptOcr = plan.HasReceiptOcr,
                HasAdvancedReports = plan.HasAdvancedReports,
                HasPrioritySupport = plan.HasPrioritySupport,
                HasApiAccess = plan.HasApiAccess,
                HasInvestmentTracking = plan.HasInvestmentTracking,
                HasTaxOptimization = plan.HasTaxOptimization,
                HasMultiUserSupport = plan.HasMultiUserSupport,
                HasWhiteLabelOptions = plan.HasWhiteLabelOptions,
                HasCustomIntegrations = plan.HasCustomIntegrations,
                HasDedicatedSupport = plan.HasDedicatedSupport,
                HasAccountManager = plan.HasAccountManager,
                HasCustomReporting = plan.HasCustomReporting,
                HasAdvancedSecurity = plan.HasAdvancedSecurity,
                HasComplianceReports = plan.HasComplianceReports,
                HasFinancialHealthScore = plan.HasFinancialHealthScore,
                HasBillForecasting = plan.HasBillForecasting,
                HasDebtOptimizer = plan.HasDebtOptimizer,
                IsActive = plan.IsActive,
                DisplayOrder = plan.DisplayOrder,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt
            };
        }

        private UserSubscriptionDto MapToUserSubscriptionDto(UserSubscription subscription)
        {
            return new UserSubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                UserName = subscription.User?.Name ?? string.Empty,
                UserEmail = subscription.User?.Email ?? string.Empty,
                SubscriptionPlanId = subscription.SubscriptionPlanId,
                PlanName = subscription.SubscriptionPlan?.Name ?? string.Empty,
                PlanDisplayName = subscription.SubscriptionPlan?.DisplayName ?? string.Empty,
                Status = subscription.Status,
                BillingCycle = subscription.BillingCycle,
                CurrentPrice = subscription.CurrentPrice,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                NextBillingDate = subscription.NextBillingDate,
                CancelledAt = subscription.CancelledAt,
                TrialEndDate = subscription.TrialEndDate,
                TransactionsThisMonth = subscription.TransactionsThisMonth,
                BillsThisMonth = subscription.BillsThisMonth,
                ReceiptOcrThisMonth = subscription.ReceiptOcrThisMonth,
                AiQueriesThisMonth = subscription.AiQueriesThisMonth,
                ApiCallsThisMonth = subscription.ApiCallsThisMonth,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt
            };
        }
    }
}

