using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIncomeSourceService _incomeSourceService;

        public UserProfileService(ApplicationDbContext context, IIncomeSourceService incomeSourceService)
        {
            _context = context;
            _incomeSourceService = incomeSourceService;
        }

        public async Task<ApiResponse<UserProfileDto>> CreateUserProfileAsync(CreateUserProfileDto createProfileDto, string userId)
        {
            try
            {
                // Check if user already has a profile
                var existingProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (existingProfile != null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile already exists. Use update instead.");
                }

                var userProfile = new UserProfile
                {
                    UserId = userId,
                    JobTitle = createProfileDto.JobTitle,
                    Company = createProfileDto.Company,
                    EmploymentType = createProfileDto.EmploymentType,
                    MonthlySavingsGoal = createProfileDto.MonthlySavingsGoal,
                    MonthlyInvestmentGoal = createProfileDto.MonthlyInvestmentGoal,
                    MonthlyEmergencyFundGoal = createProfileDto.MonthlyEmergencyFundGoal,
                    TaxRate = createProfileDto.TaxRate,
                    MonthlyTaxDeductions = createProfileDto.MonthlyTaxDeductions,
                    Industry = createProfileDto.Industry,
                    Location = createProfileDto.Location,
                    Country = createProfileDto.Country,
                    Notes = createProfileDto.Notes,
                    PreferredCurrency = !string.IsNullOrEmpty(createProfileDto.PreferredCurrency) 
                        ? createProfileDto.PreferredCurrency.ToUpper() 
                        : "USD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();

                // If income sources are provided, create them
                if (createProfileDto.IncomeSources != null && createProfileDto.IncomeSources.Any())
                {
                    foreach (var incomeSourceDto in createProfileDto.IncomeSources)
                    {
                        await _incomeSourceService.CreateIncomeSourceAsync(incomeSourceDto, userId);
                    }
                }

                var profileDto = await MapToUserProfileDto(userProfile);
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto, "User profile created successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to create user profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId)
        {
            try
            {
                Console.WriteLine($"[GET USER PROFILE] Attempting to get profile for UserId: {userId}");
                
                // Try to query without PreferredCurrency first to avoid column issues
                var userProfile = await _context.UserProfiles
                    .Where(p => p.UserId == userId && p.IsActive)
                    .Select(p => new
                    {
                        p.Id,
                        p.UserId,
                        p.JobTitle,
                        p.Company,
                        p.EmploymentType,
                        p.MonthlySavingsGoal,
                        p.MonthlyInvestmentGoal,
                        p.MonthlyEmergencyFundGoal,
                        p.TaxRate,
                        p.MonthlyTaxDeductions,
                        p.Industry,
                        p.Location,
                        p.Notes,
                        p.IsActive,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                Console.WriteLine($"[GET USER PROFILE] Query completed. Found profile: {(userProfile != null ? "Yes" : "No")}");

                if (userProfile == null)
                {
                    Console.WriteLine($"[GET USER PROFILE] No active profile found for UserId: {userId}");
                    
                    // Check if there's any profile (active or inactive)
                    var anyProfile = await _context.UserProfiles
                        .Where(p => p.UserId == userId)
                        .Select(p => new { p.Id, p.IsActive })
                        .FirstOrDefaultAsync();
                    
                    if (anyProfile != null)
                    {
                        Console.WriteLine($"[GET USER PROFILE] Found profile but IsActive = {anyProfile.IsActive} for UserId: {userId}");
                        return ApiResponse<UserProfileDto>.ErrorResult($"User profile exists but is {(anyProfile.IsActive ? "active but query failed" : "inactive")}");
                    }
                    
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                // Now fetch the full profile (will use Ignore for PreferredCurrency if column doesn't exist)
                var fullProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.Id == userProfile.Id);

                if (fullProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("Failed to load full user profile");
                }

                Console.WriteLine($"[GET USER PROFILE] Full profile loaded. PreferredCurrency: '{fullProfile.PreferredCurrency}'");
                var profileDto = await MapToUserProfileDto(fullProfile);
                Console.WriteLine($"[GET USER PROFILE] DTO mapped successfully. PreferredCurrency: '{profileDto.PreferredCurrency}'");
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GET USER PROFILE ERROR] Exception occurred: {ex.Message}");
                Console.WriteLine($"[GET USER PROFILE ERROR] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[GET USER PROFILE ERROR] Inner exception: {ex.InnerException.Message}");
                }
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to retrieve user profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateUserProfileAsync(UpdateUserProfileDto updateProfileDto, string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateProfileDto.JobTitle))
                    userProfile.JobTitle = updateProfileDto.JobTitle;
                if (!string.IsNullOrEmpty(updateProfileDto.Company))
                    userProfile.Company = updateProfileDto.Company;
                if (!string.IsNullOrEmpty(updateProfileDto.EmploymentType))
                    userProfile.EmploymentType = updateProfileDto.EmploymentType;
                if (updateProfileDto.MonthlySavingsGoal.HasValue)
                    userProfile.MonthlySavingsGoal = updateProfileDto.MonthlySavingsGoal;
                if (updateProfileDto.MonthlyInvestmentGoal.HasValue)
                    userProfile.MonthlyInvestmentGoal = updateProfileDto.MonthlyInvestmentGoal;
                if (updateProfileDto.MonthlyEmergencyFundGoal.HasValue)
                    userProfile.MonthlyEmergencyFundGoal = updateProfileDto.MonthlyEmergencyFundGoal;
                if (updateProfileDto.TaxRate.HasValue)
                    userProfile.TaxRate = updateProfileDto.TaxRate;
                if (updateProfileDto.MonthlyTaxDeductions.HasValue)
                    userProfile.MonthlyTaxDeductions = updateProfileDto.MonthlyTaxDeductions;
                if (!string.IsNullOrEmpty(updateProfileDto.Industry))
                    userProfile.Industry = updateProfileDto.Industry;
                if (!string.IsNullOrEmpty(updateProfileDto.Location))
                    userProfile.Location = updateProfileDto.Location;
                if (!string.IsNullOrEmpty(updateProfileDto.Country))
                    userProfile.Country = updateProfileDto.Country;
                if (!string.IsNullOrEmpty(updateProfileDto.Notes))
                    userProfile.Notes = updateProfileDto.Notes;
                if (!string.IsNullOrEmpty(updateProfileDto.PreferredCurrency))
                {
                    var newCurrency = updateProfileDto.PreferredCurrency.ToUpper();
                    Console.WriteLine($"[USER PROFILE UPDATE] Updating PreferredCurrency from '{userProfile.PreferredCurrency}' to '{newCurrency}'");
                    userProfile.PreferredCurrency = newCurrency;
                }
                else
                {
                    Console.WriteLine($"[USER PROFILE UPDATE] PreferredCurrency not provided in update request. Current value: '{userProfile.PreferredCurrency}'");
                }

                userProfile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                Console.WriteLine($"[USER PROFILE UPDATE] Saved successfully. PreferredCurrency is now: '{userProfile.PreferredCurrency}'");

                // Reload from database to ensure we have the latest value
                await _context.Entry(userProfile).ReloadAsync();
                Console.WriteLine($"[USER PROFILE UPDATE] After reload from DB, PreferredCurrency is: '{userProfile.PreferredCurrency}'");

                var profileDto = await MapToUserProfileDto(userProfile);
                Console.WriteLine($"[USER PROFILE UPDATE] DTO PreferredCurrency: '{profileDto.PreferredCurrency}'");
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto, "User profile updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to update user profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserProfileAsync(string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (userProfile == null)
                {
                    return ApiResponse<bool>.ErrorResult("User profile not found");
                }

                _context.UserProfiles.Remove(userProfile);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "User profile deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete user profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ActivateUserProfileAsync(string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (userProfile == null)
                {
                    return ApiResponse<bool>.ErrorResult("User profile not found");
                }

                userProfile.IsActive = true;
                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "User profile activated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to activate user profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeactivateUserProfileAsync(string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (userProfile == null)
                {
                    return ApiResponse<bool>.ErrorResult("User profile not found");
                }

                userProfile.IsActive = false;
                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "User profile deactivated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to deactivate user profile: {ex.Message}");
            }
        }

        // Financial Goals Management
        public async Task<ApiResponse<UserProfileDto>> UpdateSavingsGoalAsync(decimal monthlySavingsGoal, string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                userProfile.MonthlySavingsGoal = monthlySavingsGoal;
                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var profileDto = await MapToUserProfileDto(userProfile);
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto, "Savings goal updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to update savings goal: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateInvestmentGoalAsync(decimal monthlyInvestmentGoal, string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                userProfile.MonthlyInvestmentGoal = monthlyInvestmentGoal;
                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var profileDto = await MapToUserProfileDto(userProfile);
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto, "Investment goal updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to update investment goal: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateEmergencyFundGoalAsync(decimal monthlyEmergencyFundGoal, string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                userProfile.MonthlyEmergencyFundGoal = monthlyEmergencyFundGoal;
                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var profileDto = await MapToUserProfileDto(userProfile);
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto, "Emergency fund goal updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to update emergency fund goal: {ex.Message}");
            }
        }

        // Tax Management
        public async Task<ApiResponse<UserProfileDto>> UpdateTaxInformationAsync(decimal? taxRate, decimal? monthlyTaxDeductions, string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                if (taxRate.HasValue)
                    userProfile.TaxRate = taxRate;
                if (monthlyTaxDeductions.HasValue)
                    userProfile.MonthlyTaxDeductions = monthlyTaxDeductions;

                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var profileDto = await MapToUserProfileDto(userProfile);
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto, "Tax information updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to update tax information: {ex.Message}");
            }
        }

        // Analytics and Reporting
        public async Task<ApiResponse<Dictionary<string, decimal>>> GetGoalsBreakdownAsync(string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<Dictionary<string, decimal>>.ErrorResult("User profile not found");
                }

                var breakdown = new Dictionary<string, decimal>
                {
                    ["Savings Goal"] = userProfile.MonthlySavingsGoal ?? 0,
                    ["Investment Goal"] = userProfile.MonthlyInvestmentGoal ?? 0,
                    ["Emergency Fund Goal"] = userProfile.MonthlyEmergencyFundGoal ?? 0
                };

                return ApiResponse<Dictionary<string, decimal>>.SuccessResult(breakdown);
            }
            catch (Exception ex)
            {
                return ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get goals breakdown: {ex.Message}");
            }
        }

        // Employment Information
        public async Task<ApiResponse<UserProfileDto>> UpdateEmploymentInfoAsync(string jobTitle, string company, string employmentType, string industry, string location, string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                if (!string.IsNullOrEmpty(jobTitle))
                    userProfile.JobTitle = jobTitle;
                if (!string.IsNullOrEmpty(company))
                    userProfile.Company = company;
                if (!string.IsNullOrEmpty(employmentType))
                    userProfile.EmploymentType = employmentType;
                if (!string.IsNullOrEmpty(industry))
                    userProfile.Industry = industry;
                if (!string.IsNullOrEmpty(location))
                    userProfile.Location = location;

                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var profileDto = await MapToUserProfileDto(userProfile);
                return ApiResponse<UserProfileDto>.SuccessResult(profileDto, "Employment information updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to update employment information: {ex.Message}");
            }
        }

        // Validation and Business Rules
        public async Task<ApiResponse<bool>> ValidateIncomeGoalsAsync(string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<bool>.ErrorResult("User profile not found");
                }

                // Get total monthly income from income sources
                var totalIncomeResult = await _incomeSourceService.GetTotalMonthlyIncomeAsync(userId);
                if (!totalIncomeResult.Success)
                {
                    return ApiResponse<bool>.ErrorResult("Failed to retrieve income information");
                }

                var totalIncome = totalIncomeResult.Data;
                var totalGoals = (userProfile.MonthlySavingsGoal ?? 0) + 
                               (userProfile.MonthlyInvestmentGoal ?? 0) + 
                               (userProfile.MonthlyEmergencyFundGoal ?? 0);

                var isValid = totalGoals <= totalIncome;
                var message = isValid ? "Income goals are valid" : "Income goals exceed total income";

                return ApiResponse<bool>.SuccessResult(isValid, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to validate income goals: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> CalculateSavingsRateAsync(string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<decimal>.ErrorResult("User profile not found");
                }

                // Get total monthly income from income sources
                var totalIncomeResult = await _incomeSourceService.GetTotalMonthlyIncomeAsync(userId);
                if (!totalIncomeResult.Success)
                {
                    return ApiResponse<decimal>.ErrorResult("Failed to retrieve income information");
                }

                var totalIncome = totalIncomeResult.Data;
                var savingsAmount = (userProfile.MonthlySavingsGoal ?? 0) + (userProfile.MonthlyInvestmentGoal ?? 0);

                var savingsRate = totalIncome > 0 ? (savingsAmount / totalIncome) * 100 : 0;

                return ApiResponse<decimal>.SuccessResult(savingsRate);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to calculate savings rate: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> CalculateDisposableIncomeAsync(string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

                if (userProfile == null)
                {
                    return ApiResponse<decimal>.ErrorResult("User profile not found");
                }

                // Get total monthly income from income sources
                var totalIncomeResult = await _incomeSourceService.GetTotalMonthlyIncomeAsync(userId);
                if (!totalIncomeResult.Success)
                {
                    return ApiResponse<decimal>.ErrorResult("Failed to retrieve income information");
                }

                var totalIncome = totalIncomeResult.Data;
                var taxDeductions = userProfile.MonthlyTaxDeductions ?? 0;
                var totalGoals = (userProfile.MonthlySavingsGoal ?? 0) + 
                               (userProfile.MonthlyInvestmentGoal ?? 0) + 
                               (userProfile.MonthlyEmergencyFundGoal ?? 0);

                var disposableIncome = totalIncome - taxDeductions - totalGoals;

                return ApiResponse<decimal>.SuccessResult(disposableIncome);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to calculate disposable income: {ex.Message}");
            }
        }

        // Admin Operations
        public async Task<ApiResponse<List<UserProfileDto>>> GetAllUserProfilesAsync(int page = 1, int limit = 50)
        {
            try
            {
                var userProfiles = await _context.UserProfiles
                    .Where(p => p.IsActive)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var profileDtos = new List<UserProfileDto>();
                foreach (var profile in userProfiles)
                {
                    profileDtos.Add(await MapToUserProfileDto(profile));
                }

                return ApiResponse<List<UserProfileDto>>.SuccessResult(profileDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserProfileDto>>.ErrorResult($"Failed to retrieve user profiles: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserProfileDto>>> GetProfilesByIncomeRangeAsync(decimal minIncome, decimal maxIncome, int page = 1, int limit = 50)
        {
            try
            {
                var userProfiles = await _context.UserProfiles
                    .Where(p => p.IsActive)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var filteredProfiles = new List<UserProfileDto>();
                foreach (var profile in userProfiles)
                {
                    var totalIncomeResult = await _incomeSourceService.GetTotalMonthlyIncomeAsync(profile.UserId);
                    if (totalIncomeResult.Success)
                    {
                        var totalIncome = totalIncomeResult.Data;
                        if (totalIncome >= minIncome && totalIncome <= maxIncome)
                        {
                            filteredProfiles.Add(await MapToUserProfileDto(profile));
                        }
                    }
                }

                return ApiResponse<List<UserProfileDto>>.SuccessResult(filteredProfiles);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserProfileDto>>.ErrorResult($"Failed to filter user profiles by income range: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserProfileDto>>> GetProfilesByEmploymentTypeAsync(string employmentType, int page = 1, int limit = 50)
        {
            try
            {
                var userProfiles = await _context.UserProfiles
                    .Where(p => p.IsActive && p.EmploymentType == employmentType)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var profileDtos = new List<UserProfileDto>();
                foreach (var profile in userProfiles)
                {
                    profileDtos.Add(await MapToUserProfileDto(profile));
                }

                return ApiResponse<List<UserProfileDto>>.SuccessResult(profileDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserProfileDto>>.ErrorResult($"Failed to filter user profiles by employment type: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdatePreferredCurrencyAsync(string currency, string userId)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProfile == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User profile not found");
                }

                userProfile.PreferredCurrency = !string.IsNullOrEmpty(currency) ? currency.ToUpper() : "USD";
                userProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                var userProfileDto = await MapToUserProfileDto(userProfile);
                return ApiResponse<UserProfileDto>.SuccessResult(userProfileDto, "Preferred currency updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.ErrorResult($"Failed to update preferred currency: {ex.Message}");
            }
        }

        // Helper method to map UserProfile to UserProfileDto
        private async Task<UserProfileDto> MapToUserProfileDto(UserProfile userProfile)
        {
            if (userProfile == null) return null!;

            // Load income sources for the user profile
            var incomeSources = await _incomeSourceService.GetUserIncomeSourcesAsync(userProfile.UserId);
            var incomeSourceDtos = incomeSources.Success ? incomeSources.Data : new List<IncomeSourceDto>();

            // Calculate TotalMonthlyIncome from associated IncomeSources
            var totalMonthlyIncome = incomeSourceDtos.Sum(i => i.MonthlyAmount);

            return new UserProfileDto
            {
                Id = userProfile.Id,
                UserId = userProfile.UserId,
                JobTitle = userProfile.JobTitle,
                Company = userProfile.Company,
                EmploymentType = userProfile.EmploymentType,
                MonthlySavingsGoal = userProfile.MonthlySavingsGoal,
                MonthlyInvestmentGoal = userProfile.MonthlyInvestmentGoal,
                MonthlyEmergencyFundGoal = userProfile.MonthlyEmergencyFundGoal,
                TaxRate = userProfile.TaxRate,
                MonthlyTaxDeductions = userProfile.MonthlyTaxDeductions,
                Industry = userProfile.Industry,
                Location = userProfile.Location,
                Country = userProfile.Country,
                Notes = userProfile.Notes,
                PreferredCurrency = userProfile.PreferredCurrency,
                IsActive = userProfile.IsActive,
                CreatedAt = userProfile.CreatedAt,
                UpdatedAt = userProfile.UpdatedAt,
                TotalMonthlyIncome = totalMonthlyIncome, // Calculated here
                NetMonthlyIncome = totalMonthlyIncome - (userProfile.MonthlyTaxDeductions ?? 0),
                TotalMonthlyGoals = (userProfile.MonthlySavingsGoal ?? 0) + (userProfile.MonthlyInvestmentGoal ?? 0) + (userProfile.MonthlyEmergencyFundGoal ?? 0),
                DisposableIncome = (totalMonthlyIncome - (userProfile.MonthlyTaxDeductions ?? 0)) - ((userProfile.MonthlySavingsGoal ?? 0) + (userProfile.MonthlyInvestmentGoal ?? 0) + (userProfile.MonthlyEmergencyFundGoal ?? 0)),
                IncomeSources = incomeSourceDtos // Include the list of income sources
            };
        }
    }
}