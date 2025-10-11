using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for bill analytics, forecasting, and variance analysis
    /// </summary>
    public class BillAnalyticsService : IBillAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBillService _billService;

        public BillAnalyticsService(ApplicationDbContext context, IBillService billService)
        {
            _context = context;
            _billService = billService;
        }

        #region Analytics Calculations

        public async Task<ApiResponse<BillHistoryWithAnalyticsDto>> GetBillHistoryWithAnalyticsAsync(
            string userId, 
            string? provider = null, 
            string? billType = null, 
            int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var query = _context.Bills
                    .Where(b => b.UserId == userId && b.CreatedAt >= startDate);

                if (!string.IsNullOrEmpty(provider))
                    query = query.Where(b => b.Provider == provider);

                if (!string.IsNullOrEmpty(billType))
                    query = query.Where(b => b.BillType == billType.ToLower());

                var bills = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                var billDtos = bills.Select(b => new BillDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    BillName = b.BillName,
                    BillType = b.BillType,
                    Amount = b.Amount,
                    DueDate = b.DueDate,
                    Frequency = b.Frequency,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    PaidAt = b.PaidAt,
                    Notes = b.Notes,
                    Provider = b.Provider,
                    ReferenceNumber = b.ReferenceNumber
                }).ToList();

                // Calculate analytics
                var analyticsResponse = await CalculateAnalyticsAsync(userId, provider, billType, months);
                var analytics = analyticsResponse.Data ?? new BillAnalyticsCalculationsDto();

                // Generate forecast
                var forecastResponse = provider != null && billType != null
                    ? await GetForecastAsync(userId, provider, billType, "weighted")
                    : ApiResponse<BillForecastDto>.SuccessResult(new BillForecastDto());
                
                var forecast = forecastResponse.Data ?? new BillForecastDto();

                var result = new BillHistoryWithAnalyticsDto
                {
                    Bills = billDtos,
                    Analytics = analytics,
                    Forecast = forecast,
                    TotalCount = billDtos.Count
                };

                return ApiResponse<BillHistoryWithAnalyticsDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillHistoryWithAnalyticsDto>.ErrorResult(
                    $"Failed to get bill history with analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BillAnalyticsCalculationsDto>> CalculateAnalyticsAsync(
            string userId, 
            string? provider = null, 
            string? billType = null, 
            int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var query = _context.Bills
                    .Where(b => b.UserId == userId && b.CreatedAt >= startDate);

                if (!string.IsNullOrEmpty(provider))
                    query = query.Where(b => b.Provider == provider);

                if (!string.IsNullOrEmpty(billType))
                    query = query.Where(b => b.BillType == billType.ToLower());

                var bills = await query.OrderBy(b => b.CreatedAt).ToListAsync();

                if (!bills.Any())
                {
                    return ApiResponse<BillAnalyticsCalculationsDto>.SuccessResult(
                        new BillAnalyticsCalculationsDto());
                }

                var totalSpent = bills.Sum(b => b.Amount);
                var averageSimple = bills.Average(b => b.Amount);
                var highestBill = bills.Max(b => b.Amount);
                var lowestBill = bills.Min(b => b.Amount);

                // Calculate weighted average (last 3 months)
                var recentBills = bills.OrderByDescending(b => b.CreatedAt).Take(3).ToList();
                var weightedAvg = recentBills.Count switch
                {
                    3 => (recentBills[0].Amount * 0.5m) + (recentBills[1].Amount * 0.3m) + (recentBills[2].Amount * 0.2m),
                    2 => (recentBills[0].Amount * 0.6m) + (recentBills[1].Amount * 0.4m),
                    1 => recentBills[0].Amount,
                    _ => averageSimple
                };

                // Calculate trend
                var trend = CalculateTrend(bills);

                var analytics = new BillAnalyticsCalculationsDto
                {
                    AverageSimple = averageSimple,
                    AverageWeighted = weightedAvg,
                    AverageSeasonal = averageSimple, // Will be enhanced with seasonal calculation
                    TotalSpent = totalSpent,
                    HighestBill = highestBill,
                    LowestBill = lowestBill,
                    Trend = trend,
                    BillCount = bills.Count,
                    FirstBillDate = bills.Min(b => b.CreatedAt),
                    LastBillDate = bills.Max(b => b.CreatedAt)
                };

                return ApiResponse<BillAnalyticsCalculationsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillAnalyticsCalculationsDto>.ErrorResult(
                    $"Failed to calculate analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> CalculateSimpleAverageAsync(
            string userId, 
            string provider, 
            string billType, 
            int months = 3)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Provider == provider && 
                               b.BillType == billType.ToLower() && 
                               b.CreatedAt >= startDate)
                    .ToListAsync();

                if (!bills.Any())
                    return ApiResponse<decimal>.SuccessResult(0);

                var average = bills.Average(b => b.Amount);
                return ApiResponse<decimal>.SuccessResult(average);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult(
                    $"Failed to calculate simple average: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> CalculateWeightedAverageAsync(
            string userId, 
            string provider, 
            string billType, 
            int months = 3)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Provider == provider && 
                               b.BillType == billType.ToLower() && 
                               b.CreatedAt >= startDate)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                if (!bills.Any())
                    return ApiResponse<decimal>.SuccessResult(0);

                var weightedAverage = bills.Count switch
                {
                    3 => (bills[0].Amount * 0.5m) + (bills[1].Amount * 0.3m) + (bills[2].Amount * 0.2m),
                    2 => (bills[0].Amount * 0.6m) + (bills[1].Amount * 0.4m),
                    1 => bills[0].Amount,
                    _ => bills.Average(b => b.Amount)
                };

                return ApiResponse<decimal>.SuccessResult(weightedAverage);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult(
                    $"Failed to calculate weighted average: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> CalculateSeasonalAverageAsync(
            string userId, 
            string provider, 
            string billType)
        {
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                
                // Get bills from the same month in previous years
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Provider == provider && 
                               b.BillType == billType.ToLower() && 
                               b.CreatedAt.Month == currentMonth)
                    .ToListAsync();

                if (!bills.Any())
                {
                    // Fallback to simple average if no seasonal data
                    return await CalculateSimpleAverageAsync(userId, provider, billType, 6);
                }

                var average = bills.Average(b => b.Amount);
                return ApiResponse<decimal>.SuccessResult(average);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult(
                    $"Failed to calculate seasonal average: {ex.Message}");
            }
        }

        #endregion

        #region Forecasting

        public async Task<ApiResponse<BillForecastDto>> GetForecastAsync(
            string userId, 
            string provider, 
            string billType, 
            string method = "weighted")
        {
            try
            {
                decimal estimatedAmount;
                string confidence;

                switch (method.ToLower())
                {
                    case "simple":
                        var simpleAvgResponse = await CalculateSimpleAverageAsync(userId, provider, billType, 3);
                        estimatedAmount = simpleAvgResponse.Data;
                        confidence = "medium";
                        break;
                    
                    case "seasonal":
                        var seasonalAvgResponse = await CalculateSeasonalAverageAsync(userId, provider, billType);
                        estimatedAmount = seasonalAvgResponse.Data;
                        confidence = "high";
                        break;
                    
                    case "weighted":
                    default:
                        var weightedAvgResponse = await CalculateWeightedAverageAsync(userId, provider, billType, 3);
                        estimatedAmount = weightedAvgResponse.Data;
                        confidence = "medium";
                        break;
                }

                // Determine confidence based on data availability
                var billCount = await _context.Bills
                    .Where(b => b.UserId == userId && b.Provider == provider && b.BillType == billType.ToLower())
                    .CountAsync();

                confidence = billCount switch
                {
                    >= 12 => "high",
                    >= 6 => "medium",
                    _ => "low"
                };

                var forecast = new BillForecastDto
                {
                    EstimatedAmount = Math.Round(estimatedAmount, 2),
                    CalculationMethod = method.ToLower(),
                    Confidence = confidence,
                    EstimatedForMonth = DateTime.UtcNow.AddMonths(1),
                    Recommendation = GenerateForecastRecommendation(estimatedAmount, confidence)
                };

                return ApiResponse<BillForecastDto>.SuccessResult(forecast);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillForecastDto>.ErrorResult(
                    $"Failed to generate forecast: {ex.Message}");
            }
        }

        #endregion

        #region Variance Analysis

        public async Task<ApiResponse<BillVarianceDto>> CalculateVarianceAsync(string billId, string userId)
        {
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                    return ApiResponse<BillVarianceDto>.ErrorResult("Bill not found");

                if (string.IsNullOrEmpty(bill.Provider) || string.IsNullOrEmpty(bill.BillType))
                    return ApiResponse<BillVarianceDto>.ErrorResult("Bill provider or type is missing");

                // Get estimated amount (weighted average)
                var estimatedResponse = await CalculateWeightedAverageAsync(
                    userId, bill.Provider, bill.BillType, 3);
                
                var estimatedAmount = estimatedResponse.Data;

                if (estimatedAmount == 0)
                {
                    // No historical data to compare
                    return ApiResponse<BillVarianceDto>.SuccessResult(new BillVarianceDto
                    {
                        BillId = billId,
                        ActualAmount = bill.Amount,
                        EstimatedAmount = 0,
                        Variance = 0,
                        VariancePercentage = 0,
                        Status = "no_data",
                        Message = "No historical data available for comparison",
                        Recommendation = "Continue tracking bills to build historical data"
                    });
                }

                var variance = bill.Amount - estimatedAmount;
                var variancePercentage = (variance / estimatedAmount) * 100;

                string status;
                string message;
                string recommendation;

                if (variancePercentage >= 5)
                {
                    status = "over_budget";
                    message = $"Your bill is {Math.Abs(Math.Round(variance, 2)):C} higher than expected (+{Math.Round(variancePercentage, 2)}%)";
                    recommendation = "Consider reviewing your usage. This bill is significantly higher than your average.";
                }
                else if (variancePercentage > 1)
                {
                    status = "slightly_over";
                    message = $"Your bill is slightly higher than expected (+{Math.Round(variancePercentage, 2)}%)";
                    recommendation = "Your bill is slightly above average. Monitor usage to keep costs down.";
                }
                else if (variancePercentage >= -1)
                {
                    status = "on_target";
                    message = "Your bill is right on track with your average!";
                    recommendation = "Great job maintaining consistent usage!";
                }
                else
                {
                    status = "under_budget";
                    message = $"Great job! You saved {Math.Abs(Math.Round(variance, 2)):C} this month!";
                    recommendation = "Excellent! Your bill is lower than average. Keep up the good work!";
                }

                var varianceDto = new BillVarianceDto
                {
                    BillId = billId,
                    ActualAmount = bill.Amount,
                    EstimatedAmount = estimatedAmount,
                    Variance = variance,
                    VariancePercentage = variancePercentage,
                    Status = status,
                    Message = message,
                    Recommendation = recommendation
                };

                // Create an alert if variance is significant
                if (Math.Abs(variancePercentage) >= 10)
                {
                    await CreateVarianceAlertAsync(userId, bill, varianceDto);
                }

                return ApiResponse<BillVarianceDto>.SuccessResult(varianceDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillVarianceDto>.ErrorResult(
                    $"Failed to calculate variance: {ex.Message}");
            }
        }

        #endregion

        #region Budget Management

        public async Task<ApiResponse<BudgetSettingDto>> CreateBudgetAsync(
            CreateBudgetSettingDto budgetDto, 
            string userId)
        {
            try
            {
                // Check if budget already exists for this provider/type
                var existing = await _context.BudgetSettings
                    .FirstOrDefaultAsync(b => b.UserId == userId && 
                                            b.Provider == budgetDto.Provider && 
                                            b.BillType == budgetDto.BillType.ToLower());

                if (existing != null)
                {
                    return ApiResponse<BudgetSettingDto>.ErrorResult(
                        "Budget already exists for this provider and bill type");
                }

                var budget = new BudgetSetting
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Provider = budgetDto.Provider,
                    BillType = budgetDto.BillType.ToLower(),
                    MonthlyBudget = budgetDto.MonthlyBudget,
                    EnableAlerts = budgetDto.EnableAlerts,
                    AlertThreshold = budgetDto.AlertThreshold,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BudgetSettings.Add(budget);
                await _context.SaveChangesAsync();

                var result = MapToBudgetSettingDto(budget);
                return ApiResponse<BudgetSettingDto>.SuccessResult(result, "Budget created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetSettingDto>.ErrorResult(
                    $"Failed to create budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BudgetSettingDto>> UpdateBudgetAsync(
            string budgetId, 
            CreateBudgetSettingDto budgetDto, 
            string userId)
        {
            try
            {
                var budget = await _context.BudgetSettings
                    .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);

                if (budget == null)
                    return ApiResponse<BudgetSettingDto>.ErrorResult("Budget not found");

                budget.Provider = budgetDto.Provider;
                budget.BillType = budgetDto.BillType.ToLower();
                budget.MonthlyBudget = budgetDto.MonthlyBudget;
                budget.EnableAlerts = budgetDto.EnableAlerts;
                budget.AlertThreshold = budgetDto.AlertThreshold;
                budget.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = MapToBudgetSettingDto(budget);
                return ApiResponse<BudgetSettingDto>.SuccessResult(result, "Budget updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetSettingDto>.ErrorResult(
                    $"Failed to update budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBudgetAsync(string budgetId, string userId)
        {
            try
            {
                var budget = await _context.BudgetSettings
                    .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);

                if (budget == null)
                    return ApiResponse<bool>.ErrorResult("Budget not found");

                _context.BudgetSettings.Remove(budget);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Budget deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    $"Failed to delete budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BudgetSettingDto>> GetBudgetAsync(string budgetId, string userId)
        {
            try
            {
                var budget = await _context.BudgetSettings
                    .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);

                if (budget == null)
                    return ApiResponse<BudgetSettingDto>.ErrorResult("Budget not found");

                var result = MapToBudgetSettingDto(budget);
                return ApiResponse<BudgetSettingDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetSettingDto>.ErrorResult(
                    $"Failed to get budget: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BudgetSettingDto>>> GetUserBudgetsAsync(string userId)
        {
            try
            {
                var budgets = await _context.BudgetSettings
                    .Where(b => b.UserId == userId)
                    .OrderBy(b => b.Provider)
                    .ToListAsync();

                var result = budgets.Select(MapToBudgetSettingDto).ToList();
                return ApiResponse<List<BudgetSettingDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BudgetSettingDto>>.ErrorResult(
                    $"Failed to get budgets: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BudgetStatusDto>> GetBudgetStatusAsync(
            string userId, 
            string provider, 
            string billType)
        {
            try
            {
                var budget = await _context.BudgetSettings
                    .FirstOrDefaultAsync(b => b.UserId == userId && 
                                            b.Provider == provider && 
                                            b.BillType == billType.ToLower());

                if (budget == null)
                {
                    return ApiResponse<BudgetStatusDto>.ErrorResult(
                        "No budget found for this provider and bill type");
                }

                // Get current month's bill
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                
                var currentBill = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Provider == provider && 
                               b.BillType == billType.ToLower() &&
                               b.CreatedAt.Month == currentMonth &&
                               b.CreatedAt.Year == currentYear)
                    .OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefaultAsync();

                var currentAmount = currentBill?.Amount ?? 0;
                var remaining = budget.MonthlyBudget - currentAmount;
                var percentageUsed = budget.MonthlyBudget > 0 
                    ? (currentAmount / budget.MonthlyBudget) * 100 
                    : 0;

                string status;
                bool alert = false;
                string message;

                if (percentageUsed > 100)
                {
                    status = "over_budget";
                    alert = budget.EnableAlerts;
                    message = $"You exceeded your budget by {Math.Abs(remaining):C}";
                }
                else if (percentageUsed >= budget.AlertThreshold)
                {
                    status = "approaching_limit";
                    alert = budget.EnableAlerts;
                    message = $"You've used {Math.Round(percentageUsed, 1)}% of your budget. {remaining:C} remaining.";
                }
                else
                {
                    status = "on_track";
                    message = $"You're on track. {remaining:C} remaining of your {budget.MonthlyBudget:C} budget.";
                }

                var budgetStatus = new BudgetStatusDto
                {
                    BudgetId = budget.Id,
                    Provider = budget.Provider,
                    BillType = budget.BillType,
                    MonthlyBudget = budget.MonthlyBudget,
                    CurrentBill = currentAmount,
                    Remaining = remaining,
                    PercentageUsed = Math.Round(percentageUsed, 2),
                    Status = status,
                    Alert = alert,
                    Message = message
                };

                // Create alert if needed
                if (alert && currentBill != null)
                {
                    await CreateBudgetAlertAsync(userId, currentBill, budgetStatus);
                }

                return ApiResponse<BudgetStatusDto>.SuccessResult(budgetStatus);
            }
            catch (Exception ex)
            {
                return ApiResponse<BudgetStatusDto>.ErrorResult(
                    $"Failed to get budget status: {ex.Message}");
            }
        }

        #endregion

        #region Alerts

        public async Task<ApiResponse<List<BillAlertDto>>> GetUserAlertsAsync(
            string userId, 
            bool? isRead = null, 
            int limit = 50)
        {
            try
            {
                var query = _context.BillAlerts.Where(a => a.UserId == userId);

                if (isRead.HasValue)
                    query = query.Where(a => a.IsRead == isRead.Value);

                var alerts = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                var result = alerts.Select(MapToBillAlertDto).ToList();
                return ApiResponse<List<BillAlertDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BillAlertDto>>.ErrorResult(
                    $"Failed to get alerts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> MarkAlertAsReadAsync(string alertId, string userId)
        {
            try
            {
                var alert = await _context.BillAlerts
                    .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);

                if (alert == null)
                    return ApiResponse<bool>.ErrorResult("Alert not found");

                alert.IsRead = true;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Alert marked as read");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult(
                    $"Failed to mark alert as read: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BillAlertDto>>> GenerateAlertsAsync(string userId)
        {
            try
            {
                var alerts = new List<BillAlert>();
                
                // Generate due date reminders (3 days before)
                var threeDaysFromNow = DateTime.UtcNow.AddDays(3).Date;
                var upcomingBills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PENDING" &&
                               b.DueDate.Date == threeDaysFromNow)
                    .ToListAsync();

                foreach (var bill in upcomingBills)
                {
                    alerts.Add(new BillAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        AlertType = "due_date",
                        Severity = "warning",
                        Title = "Payment Reminder",
                        Message = $"Your {bill.Provider} bill of {bill.Amount:C} is due in 3 days ({bill.DueDate:MMM dd})",
                        BillId = bill.Id,
                        Provider = bill.Provider,
                        Amount = bill.Amount,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        ActionLink = $"/bills/{bill.Id}"
                    });
                }

                // Generate overdue alerts
                var overdueBills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PENDING" &&
                               b.DueDate.Date < DateTime.UtcNow.Date)
                    .ToListAsync();

                foreach (var bill in overdueBills)
                {
                    alerts.Add(new BillAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        AlertType = "overdue",
                        Severity = "error",
                        Title = "Overdue Bill",
                        Message = $"Your {bill.Provider} bill of {bill.Amount:C} was due on {bill.DueDate:MMM dd}. Pay now to avoid late fees.",
                        BillId = bill.Id,
                        Provider = bill.Provider,
                        Amount = bill.Amount,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        ActionLink = $"/bills/{bill.Id}"
                    });
                }

                if (alerts.Any())
                {
                    _context.BillAlerts.AddRange(alerts);
                    await _context.SaveChangesAsync();
                }

                var result = alerts.Select(MapToBillAlertDto).ToList();
                return ApiResponse<List<BillAlertDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BillAlertDto>>.ErrorResult(
                    $"Failed to generate alerts: {ex.Message}");
            }
        }

        #endregion

        #region Provider Analytics

        public async Task<ApiResponse<List<ProviderAnalyticsDto>>> GetProviderAnalyticsAsync(
            string userId, 
            int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId && b.CreatedAt >= startDate)
                    .ToListAsync();

                var providerGroups = bills
                    .Where(b => !string.IsNullOrEmpty(b.Provider))
                    .GroupBy(b => new { b.Provider, b.BillType });

                var analytics = new List<ProviderAnalyticsDto>();

                foreach (var group in providerGroups)
                {
                    var providerBills = group.ToList();
                    var monthlySummary = await GetMonthlyTrendAsync(
                        userId, group.Key.Provider, group.Key.BillType, months);

                    // Get budget if exists
                    var budget = await _context.BudgetSettings
                        .FirstOrDefaultAsync(b => b.UserId == userId && 
                                                 b.Provider == group.Key.Provider && 
                                                 b.BillType == group.Key.BillType);

                    analytics.Add(new ProviderAnalyticsDto
                    {
                        Provider = group.Key.Provider!,
                        BillType = group.Key.BillType,
                        TotalSpent = providerBills.Sum(b => b.Amount),
                        AverageMonthly = providerBills.Average(b => b.Amount),
                        BillCount = providerBills.Count,
                        HighestBill = providerBills.Max(b => b.Amount),
                        LowestBill = providerBills.Min(b => b.Amount),
                        LastBillDate = providerBills.Max(b => b.CreatedAt),
                        CurrentBudget = budget?.MonthlyBudget,
                        MonthlySummary = monthlySummary.Data ?? new List<MonthlyBillSummaryDto>()
                    });
                }

                return ApiResponse<List<ProviderAnalyticsDto>>.SuccessResult(
                    analytics.OrderByDescending(a => a.TotalSpent).ToList());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProviderAnalyticsDto>>.ErrorResult(
                    $"Failed to get provider analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProviderAnalyticsDto>> GetProviderAnalyticsByProviderAsync(
            string userId, 
            string provider, 
            string billType, 
            int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Provider == provider &&
                               b.BillType == billType.ToLower() &&
                               b.CreatedAt >= startDate)
                    .ToListAsync();

                if (!bills.Any())
                {
                    return ApiResponse<ProviderAnalyticsDto>.ErrorResult(
                        "No bills found for this provider and type");
                }

                var monthlySummary = await GetMonthlyTrendAsync(userId, provider, billType, months);

                var budget = await _context.BudgetSettings
                    .FirstOrDefaultAsync(b => b.UserId == userId && 
                                             b.Provider == provider && 
                                             b.BillType == billType.ToLower());

                var analytics = new ProviderAnalyticsDto
                {
                    Provider = provider,
                    BillType = billType.ToLower(),
                    TotalSpent = bills.Sum(b => b.Amount),
                    AverageMonthly = bills.Average(b => b.Amount),
                    BillCount = bills.Count,
                    HighestBill = bills.Max(b => b.Amount),
                    LowestBill = bills.Min(b => b.Amount),
                    LastBillDate = bills.Max(b => b.CreatedAt),
                    CurrentBudget = budget?.MonthlyBudget,
                    MonthlySummary = monthlySummary.Data ?? new List<MonthlyBillSummaryDto>()
                };

                return ApiResponse<ProviderAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProviderAnalyticsDto>.ErrorResult(
                    $"Failed to get provider analytics: {ex.Message}");
            }
        }

        #endregion

        #region Dashboard

        public async Task<ApiResponse<BillDashboardDto>> GetDashboardDataAsync(string userId)
        {
            try
            {
                // Get current bills (this month)
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var currentBills = await _context.Bills
                    .Where(b => b.UserId == userId &&
                               b.CreatedAt.Month == currentMonth &&
                               b.CreatedAt.Year == currentYear)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                // Get upcoming bills (due in next 7 days)
                var upcomingResponse = await _billService.GetUpcomingBillsAsync(userId, 7);
                
                // Get overdue bills
                var overdueResponse = await _billService.GetOverdueBillsAsync(userId);

                // Get provider analytics
                var providerAnalyticsResponse = await GetProviderAnalyticsAsync(userId, 6);

                // Get budget statuses
                var budgets = await _context.BudgetSettings
                    .Where(b => b.UserId == userId)
                    .ToListAsync();

                var budgetStatuses = new List<BudgetStatusDto>();
                foreach (var budget in budgets)
                {
                    var statusResponse = await GetBudgetStatusAsync(
                        userId, budget.Provider, budget.BillType);
                    
                    if (statusResponse.Success && statusResponse.Data != null)
                        budgetStatuses.Add(statusResponse.Data);
                }

                // Get recent alerts
                var alertsResponse = await GetUserAlertsAsync(userId, false, 10);

                // Get analytics summary
                var analyticsResponse = await _billService.GetBillAnalyticsAsync(userId);

                var dashboard = new BillDashboardDto
                {
                    CurrentBills = currentBills.Select(b => new BillDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        BillName = b.BillName,
                        BillType = b.BillType,
                        Amount = b.Amount,
                        DueDate = b.DueDate,
                        Frequency = b.Frequency,
                        Status = b.Status,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt,
                        PaidAt = b.PaidAt,
                        Notes = b.Notes,
                        Provider = b.Provider,
                        ReferenceNumber = b.ReferenceNumber
                    }).ToList(),
                    UpcomingBills = upcomingResponse.Data ?? new List<BillDto>(),
                    OverdueBills = overdueResponse.Data ?? new List<BillDto>(),
                    ProviderAnalytics = providerAnalyticsResponse.Data ?? new List<ProviderAnalyticsDto>(),
                    BudgetStatuses = budgetStatuses,
                    Alerts = alertsResponse.Data ?? new List<BillAlertDto>(),
                    Summary = analyticsResponse.Data ?? new BillAnalyticsDto()
                };

                return ApiResponse<BillDashboardDto>.SuccessResult(dashboard);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillDashboardDto>.ErrorResult(
                    $"Failed to get dashboard data: {ex.Message}");
            }
        }

        #endregion

        #region Monthly Summaries

        public async Task<ApiResponse<List<MonthlyBillSummaryDto>>> GetMonthlyTrendAsync(
            string userId, 
            string? provider = null, 
            string? billType = null, 
            int months = 12)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var query = _context.Bills
                    .Where(b => b.UserId == userId && b.CreatedAt >= startDate);

                if (!string.IsNullOrEmpty(provider))
                    query = query.Where(b => b.Provider == provider);

                if (!string.IsNullOrEmpty(billType))
                    query = query.Where(b => b.BillType == billType.ToLower());

                var bills = await query.ToListAsync();

                var monthlyGroups = bills
                    .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                    .Select(g => new MonthlyBillSummaryDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        TotalAmount = g.Sum(b => b.Amount),
                        BillCount = g.Count(),
                        AverageAmount = g.Average(b => b.Amount),
                        Status = g.All(b => b.Status == "PAID") ? "paid" : 
                                g.Any(b => b.Status == "PENDING") ? "pending" : "overdue"
                    })
                    .OrderBy(m => m.Year)
                    .ThenBy(m => m.Month)
                    .ToList();

                return ApiResponse<List<MonthlyBillSummaryDto>>.SuccessResult(monthlyGroups);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<MonthlyBillSummaryDto>>.ErrorResult(
                    $"Failed to get monthly trend: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private static string CalculateTrend(List<Bill> bills)
        {
            if (bills.Count < 3) return "stable";

            var recentThree = bills.OrderByDescending(b => b.CreatedAt).Take(3).ToList();
            var olderThree = bills.OrderByDescending(b => b.CreatedAt).Skip(3).Take(3).ToList();

            if (!olderThree.Any()) return "stable";

            var recentAvg = recentThree.Average(b => b.Amount);
            var olderAvg = olderThree.Average(b => b.Amount);

            var difference = ((recentAvg - olderAvg) / olderAvg) * 100;

            return difference switch
            {
                > 5 => "increasing",
                < -5 => "decreasing",
                _ => "stable"
            };
        }

        private static string GenerateForecastRecommendation(decimal amount, string confidence)
        {
            return confidence switch
            {
                "high" => $"Based on historical patterns, expect around {amount:C} for next month.",
                "medium" => $"Estimated {amount:C} for next month. Continue tracking for better accuracy.",
                "low" => $"Limited data available. Estimated {amount:C}, but actual amount may vary significantly.",
                _ => $"Estimated amount: {amount:C}"
            };
        }

        private async Task CreateVarianceAlertAsync(string userId, Bill bill, BillVarianceDto variance)
        {
            // Check if alert already exists for this bill
            var existingAlert = await _context.BillAlerts
                .FirstOrDefaultAsync(a => a.BillId == bill.Id && 
                                        a.AlertType == "unusual_spike" &&
                                        !a.IsRead);

            if (existingAlert != null) return; // Don't create duplicate alerts

            var alert = new BillAlert
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                AlertType = variance.VariancePercentage > 0 ? "unusual_spike" : "savings",
                Severity = variance.VariancePercentage > 20 ? "error" : 
                          variance.VariancePercentage > 10 ? "warning" : "success",
                Title = variance.VariancePercentage > 0 ? "Unusual Bill Amount" : "Savings Detected",
                Message = variance.Message,
                BillId = bill.Id,
                Provider = bill.Provider,
                Amount = bill.Amount,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                ActionLink = $"/bills/{bill.Id}"
            };

            _context.BillAlerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        private async Task CreateBudgetAlertAsync(string userId, Bill bill, BudgetStatusDto budgetStatus)
        {
            // Check if alert already exists for this bill
            var existingAlert = await _context.BillAlerts
                .FirstOrDefaultAsync(a => a.BillId == bill.Id && 
                                        a.AlertType == "budget_exceeded" &&
                                        !a.IsRead);

            if (existingAlert != null) return;

            var alert = new BillAlert
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                AlertType = "budget_exceeded",
                Severity = budgetStatus.Status == "over_budget" ? "error" : "warning",
                Title = "Budget Alert",
                Message = budgetStatus.Message,
                BillId = bill.Id,
                Provider = bill.Provider,
                Amount = bill.Amount,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                ActionLink = $"/bills/{bill.Id}"
            };

            _context.BillAlerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        private static BudgetSettingDto MapToBudgetSettingDto(BudgetSetting budget)
        {
            return new BudgetSettingDto
            {
                Id = budget.Id,
                UserId = budget.UserId,
                Provider = budget.Provider,
                BillType = budget.BillType,
                MonthlyBudget = budget.MonthlyBudget,
                EnableAlerts = budget.EnableAlerts,
                AlertThreshold = budget.AlertThreshold,
                CreatedAt = budget.CreatedAt,
                UpdatedAt = budget.UpdatedAt
            };
        }

        private static BillAlertDto MapToBillAlertDto(BillAlert alert)
        {
            return new BillAlertDto
            {
                Id = alert.Id,
                AlertType = alert.AlertType,
                Severity = alert.Severity,
                Title = alert.Title,
                Message = alert.Message,
                BillId = alert.BillId,
                Provider = alert.Provider,
                Amount = alert.Amount,
                CreatedAt = alert.CreatedAt,
                IsRead = alert.IsRead,
                ActionLink = alert.ActionLink
            };
        }

        #endregion

        #region Auto-Recurring Bill Generation

        public async Task<ApiResponse<BillDto>> AutoGenerateNextMonthBillAsync(
            string userId, 
            string provider, 
            string billType)
        {
            try
            {
                // Get the last bill for this provider/type
                var lastBill = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Provider == provider && 
                               b.BillType == billType.ToLower())
                    .OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastBill == null)
                {
                    return ApiResponse<BillDto>.ErrorResult(
                        "No previous bill found. Please create the first bill manually.");
                }

                if (!lastBill.AutoGenerateNext)
                {
                    return ApiResponse<BillDto>.ErrorResult(
                        "Auto-generation is not enabled for this bill. Update the bill to enable auto-generation.");
                }

                // Calculate next month's due date
                var nextDueDate = lastBill.DueDate.AddMonths(1);
                
                // Check if next month's bill already exists
                var exists = await _context.Bills.AnyAsync(b =>
                    b.UserId == userId &&
                    b.Provider == provider &&
                    b.BillType == billType.ToLower() &&
                    b.DueDate.Month == nextDueDate.Month &&
                    b.DueDate.Year == nextDueDate.Year);

                if (exists)
                {
                    return ApiResponse<BillDto>.ErrorResult(
                        "Bill for next month already exists");
                }

                // Get forecast for this provider/type
                var forecastResponse = await GetForecastAsync(userId, provider, billType, "weighted");
                var estimatedAmount = forecastResponse.Data?.EstimatedAmount ?? lastBill.Amount;

                // Create next month's bill
                var nextBill = new Bill
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BillName = $"{provider} - {nextDueDate:MMM yyyy}",
                    BillType = billType.ToLower(),
                    Provider = provider,
                    Amount = estimatedAmount,
                    DueDate = nextDueDate,
                    Frequency = "monthly",
                    Status = "PENDING",
                    Notes = $"Auto-generated based on {forecastResponse.Data?.CalculationMethod} forecast (₱{estimatedAmount:N2}). Update amount when actual bill arrives.",
                    ReferenceNumber = lastBill.ReferenceNumber,
                    AutoGenerateNext = lastBill.AutoGenerateNext, // Carry forward the setting
                    IsAutoGenerated = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Bills.Add(nextBill);
                await _context.SaveChangesAsync();

                // Create notification alert
                var alert = new BillAlert
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    AlertType = "auto_generated",
                    Severity = "info",
                    Title = "Bill Auto-Generated",
                    Message = $"Your {provider} bill for {nextDueDate:MMM yyyy} has been estimated at ₱{estimatedAmount:N2}. Update the amount when your actual bill arrives.",
                    BillId = nextBill.Id,
                    Provider = provider,
                    Amount = estimatedAmount,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    ActionLink = $"/bills/{nextBill.Id}"
                };

                _context.BillAlerts.Add(alert);
                await _context.SaveChangesAsync();

                var billDto = new BillDto
                {
                    Id = nextBill.Id,
                    UserId = nextBill.UserId,
                    BillName = nextBill.BillName,
                    BillType = nextBill.BillType,
                    Amount = nextBill.Amount,
                    DueDate = nextBill.DueDate,
                    Frequency = nextBill.Frequency,
                    Status = nextBill.Status,
                    CreatedAt = nextBill.CreatedAt,
                    UpdatedAt = nextBill.UpdatedAt,
                    PaidAt = nextBill.PaidAt,
                    Notes = nextBill.Notes,
                    Provider = nextBill.Provider,
                    ReferenceNumber = nextBill.ReferenceNumber,
                    AutoGenerateNext = nextBill.AutoGenerateNext,
                    IsAutoGenerated = nextBill.IsAutoGenerated,
                    ConfirmedAt = nextBill.ConfirmedAt
                };

                return ApiResponse<BillDto>.SuccessResult(billDto, 
                    $"Next month's bill auto-generated with estimated amount of ₱{estimatedAmount:N2}");
            }
            catch (Exception ex)
            {
                return ApiResponse<BillDto>.ErrorResult(
                    $"Failed to auto-generate bill: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BillDto>>> AutoGenerateAllRecurringBillsAsync(string userId)
        {
            try
            {
                var generatedBills = new List<BillDto>();

                // Get all bills with auto-generation enabled
                var billsWithAutoGen = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.AutoGenerateNext && 
                               b.Frequency == "monthly")
                    .Select(b => new { b.Provider, b.BillType })
                    .Distinct()
                    .ToListAsync();

                foreach (var billInfo in billsWithAutoGen)
                {
                    if (string.IsNullOrEmpty(billInfo.Provider)) continue;

                    var result = await AutoGenerateNextMonthBillAsync(
                        userId, billInfo.Provider, billInfo.BillType);

                    if (result.Success && result.Data != null)
                    {
                        generatedBills.Add(result.Data);
                    }
                }

                return ApiResponse<List<BillDto>>.SuccessResult(generatedBills, 
                    $"Auto-generated {generatedBills.Count} bill(s) for next month");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BillDto>>.ErrorResult(
                    $"Failed to auto-generate bills: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BillDto>> ConfirmAutoGeneratedBillAsync(
            string billId, 
            ConfirmBillAmountDto confirmDto, 
            string userId)
        {
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                    return ApiResponse<BillDto>.ErrorResult("Bill not found");

                if (!bill.IsAutoGenerated)
                {
                    return ApiResponse<BillDto>.ErrorResult(
                        "This bill was not auto-generated. Use the regular update endpoint.");
                }

                // Update the amount and mark as confirmed
                bill.Amount = confirmDto.Amount;
                bill.ConfirmedAt = DateTime.UtcNow;
                bill.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(confirmDto.Notes))
                {
                    bill.Notes = confirmDto.Notes;
                }

                await _context.SaveChangesAsync();

                var billDto = new BillDto
                {
                    Id = bill.Id,
                    UserId = bill.UserId,
                    BillName = bill.BillName,
                    BillType = bill.BillType,
                    Amount = bill.Amount,
                    DueDate = bill.DueDate,
                    Frequency = bill.Frequency,
                    Status = bill.Status,
                    CreatedAt = bill.CreatedAt,
                    UpdatedAt = bill.UpdatedAt,
                    PaidAt = bill.PaidAt,
                    Notes = bill.Notes,
                    Provider = bill.Provider,
                    ReferenceNumber = bill.ReferenceNumber,
                    AutoGenerateNext = bill.AutoGenerateNext,
                    IsAutoGenerated = bill.IsAutoGenerated,
                    ConfirmedAt = bill.ConfirmedAt
                };

                return ApiResponse<BillDto>.SuccessResult(billDto, 
                    "Bill amount confirmed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BillDto>.ErrorResult(
                    $"Failed to confirm bill: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BillDto>>> GetAutoGeneratedBillsAsync(
            string userId, 
            bool? confirmed = null)
        {
            try
            {
                var query = _context.Bills
                    .Where(b => b.UserId == userId && b.IsAutoGenerated);

                if (confirmed.HasValue)
                {
                    if (confirmed.Value)
                        query = query.Where(b => b.ConfirmedAt != null);
                    else
                        query = query.Where(b => b.ConfirmedAt == null);
                }

                var bills = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                var billDtos = bills.Select(b => new BillDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    BillName = b.BillName,
                    BillType = b.BillType,
                    Amount = b.Amount,
                    DueDate = b.DueDate,
                    Frequency = b.Frequency,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    PaidAt = b.PaidAt,
                    Notes = b.Notes,
                    Provider = b.Provider,
                    ReferenceNumber = b.ReferenceNumber,
                    AutoGenerateNext = b.AutoGenerateNext,
                    IsAutoGenerated = b.IsAutoGenerated,
                    ConfirmedAt = b.ConfirmedAt
                }).ToList();

                return ApiResponse<List<BillDto>>.SuccessResult(billDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BillDto>>.ErrorResult(
                    $"Failed to get auto-generated bills: {ex.Message}");
            }
        }

        #endregion
    }
}

