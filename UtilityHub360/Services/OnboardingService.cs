using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class OnboardingService : IOnboardingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIncomeSourceService _incomeSourceService;
        private readonly IBillService _billService;
        private readonly ILoanService _loanService;

        public OnboardingService(
            ApplicationDbContext context,
            IIncomeSourceService incomeSourceService,
            IBillService billService,
            ILoanService loanService)
        {
            _context = context;
            _incomeSourceService = incomeSourceService;
            _billService = billService;
            _loanService = loanService;
        }

        public async Task<ApiResponse<OnboardingProgressDto>> GetOnboardingProgressAsync(string userId)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                var progressDto = MapToProgressDto(onboarding);
                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to get onboarding progress: {ex.Message}");
            }
        }


        public async Task<ApiResponse<OnboardingProgressDto>> StartOnboardingAsync(string userId)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                onboarding.StartedAt = DateTime.UtcNow;
                onboarding.CurrentStep = 1;
                onboarding.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var progressDto = MapToProgressDto(onboarding);
                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to start onboarding: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OnboardingProgressDto>> UpdateCurrentStepAsync(string userId, int stepNumber)
        {
            try
            {
                if (stepNumber < 1 || stepNumber > 6)
                {
                    return ApiResponse<OnboardingProgressDto>.ErrorResult("Invalid step number. Must be between 1 and 6.");
                }

                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                onboarding.CurrentStep = stepNumber;
                onboarding.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var progressDto = MapToProgressDto(onboarding);
                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to update current step: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickSetupResponseDto>> CompleteWelcomeStepAsync(string userId, WelcomeSetupDto welcomeSetup)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                
                // Update preferences
                onboarding.PreferredCurrency = welcomeSetup.PreferredCurrency;
                onboarding.FinancialGoal = welcomeSetup.FinancialGoal;
                onboarding.MonthlyIncomeTarget = welcomeSetup.MonthlyIncomeTarget;
                onboarding.MonthlyExpenseTarget = welcomeSetup.MonthlyExpenseTarget;
                onboarding.SavingsGoalAmount = welcomeSetup.SavingsGoalAmount;
                onboarding.SavingsGoalDate = welcomeSetup.SavingsGoalDate;

                // Mark step as completed
                onboarding.MarkStepCompleted(1);
                onboarding.CurrentStep = 2; // Move to next step

                await _context.SaveChangesAsync();

                var response = new QuickSetupResponseDto
                {
                    Success = true,
                    Message = "Welcome setup completed successfully!",
                    ItemsCreated = 0,
                    Progress = MapToProgressDto(onboarding),
                    CreatedItems = new List<string> { "User preferences saved" }
                };

                return ApiResponse<QuickSetupResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete welcome step: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickSetupResponseDto>> CompleteIncomeStepAsync(string userId, IncomeSetupDto incomeSetup)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                var createdItems = new List<string>();
                var totalCreated = 0;

                // Create income sources
                foreach (var incomeSourceDto in incomeSetup.IncomeSources)
                {
                    var createIncomeDto = new CreateIncomeSourceDto
                    {
                        Name = incomeSourceDto.Name,
                        Amount = incomeSourceDto.Amount,
                        Frequency = incomeSourceDto.Frequency,
                        Category = incomeSourceDto.Category,
                        Description = incomeSourceDto.Description
                    };

                    var result = await _incomeSourceService.CreateIncomeSourceAsync(createIncomeDto, userId);
                    if (result.Success)
                    {
                        totalCreated++;
                        createdItems.Add($"Income source: {incomeSourceDto.Name}");
                    }
                }

                // Mark step as completed
                onboarding.MarkStepCompleted(2);
                onboarding.CurrentStep = 3; // Move to next step

                await _context.SaveChangesAsync();

                var response = new QuickSetupResponseDto
                {
                    Success = true,
                    Message = $"Income setup completed! Created {totalCreated} income source(s).",
                    ItemsCreated = totalCreated,
                    Progress = MapToProgressDto(onboarding),
                    CreatedItems = createdItems
                };

                return ApiResponse<QuickSetupResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete income step: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickSetupResponseDto>> CompleteBillsStepAsync(string userId, BillsSetupDto billsSetup)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                var createdItems = new List<string>();
                var totalCreated = 0;

                if (!billsSetup.SkipBills)
                {
                    // Create bills
                    foreach (var billDto in billsSetup.Bills)
                    {
                        var createBillDto = new CreateBillDto
                        {
                            BillName = billDto.BillName,
                            BillType = billDto.BillType,
                            Amount = billDto.Amount,
                            DueDate = billDto.DueDate,
                            Frequency = billDto.Frequency,
                            Notes = billDto.Description
                        };

                        var result = await _billService.CreateBillAsync(createBillDto, userId);
                        if (result.Success)
                        {
                            totalCreated++;
                            createdItems.Add($"Bill: {billDto.BillName}");
                        }
                    }
                }

                // Mark step as completed
                onboarding.MarkStepCompleted(3);
                onboarding.CurrentStep = 4; // Move to next step

                await _context.SaveChangesAsync();

                var skipMessage = billsSetup.SkipBills ? " (skipped)" : "";
                var response = new QuickSetupResponseDto
                {
                    Success = true,
                    Message = $"Bills setup completed{skipMessage}! Created {totalCreated} bill(s).",
                    ItemsCreated = totalCreated,
                    Progress = MapToProgressDto(onboarding),
                    CreatedItems = createdItems
                };

                return ApiResponse<QuickSetupResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete bills step: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickSetupResponseDto>> CompleteLoansStepAsync(string userId, LoansSetupDto loansSetup)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                var createdItems = new List<string>();
                var totalCreated = 0;

                if (!loansSetup.SkipLoans)
                {
                    // Create loans (using ApplyForLoanAsync for now)
                    foreach (var loanDto in loansSetup.Loans)
                    {
                        var createLoanApplicationDto = new CreateLoanApplicationDto
                        {
                            Principal = loanDto.PrincipalAmount,
                            InterestRate = loanDto.InterestRate,
                            Term = (int)((loanDto.EndDate - loanDto.StartDate).TotalDays / 30), // Convert to months
                            Purpose = loanDto.LoanName,
                            MonthlyIncome = 50000, // Default value for onboarding
                            EmploymentStatus = "EMPLOYED", // Default value for onboarding
                            AdditionalInfo = loanDto.Description
                        };

                        var result = await _loanService.ApplyForLoanAsync(createLoanApplicationDto, userId);
                        if (result.Success)
                        {
                            totalCreated++;
                            createdItems.Add($"Loan: {loanDto.LoanName}");
                        }
                    }
                }

                // Mark step as completed
                onboarding.MarkStepCompleted(4);
                onboarding.CurrentStep = 5; // Move to next step

                await _context.SaveChangesAsync();

                var skipMessage = loansSetup.SkipLoans ? " (skipped)" : "";
                var response = new QuickSetupResponseDto
                {
                    Success = true,
                    Message = $"Loans setup completed{skipMessage}! Created {totalCreated} loan(s).",
                    ItemsCreated = totalCreated,
                    Progress = MapToProgressDto(onboarding),
                    CreatedItems = createdItems
                };

                return ApiResponse<QuickSetupResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete loans step: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickSetupResponseDto>> CompleteVariableExpensesStepAsync(string userId, VariableExpensesSetupDto expensesSetup)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                var createdItems = new List<string>();
                var totalCreated = 0;

                if (!expensesSetup.SkipExpenses)
                {
                    // Create variable expenses
                    foreach (var expenseDto in expensesSetup.Expenses)
                    {
                        var createExpenseDto = new CreateVariableExpenseDto
                        {
                            Description = expenseDto.Description,
                            Amount = expenseDto.Amount,
                            Category = expenseDto.Category,
                            ExpenseDate = expenseDto.ExpenseDate,
                            Merchant = expenseDto.Merchant,
                            PaymentMethod = expenseDto.PaymentMethod,
                            Notes = expenseDto.Notes
                        };

                        var result = await _context.VariableExpenses.AddAsync(new VariableExpense
                        {
                            UserId = userId,
                            Description = createExpenseDto.Description,
                            Amount = createExpenseDto.Amount,
                            Category = createExpenseDto.Category,
                            ExpenseDate = createExpenseDto.ExpenseDate,
                            Merchant = createExpenseDto.Merchant,
                            PaymentMethod = createExpenseDto.PaymentMethod,
                            Notes = createExpenseDto.Notes,
                            CreatedAt = DateTime.UtcNow
                        });

                        totalCreated++;
                        createdItems.Add($"Expense: {expenseDto.Description}");
                    }

                    await _context.SaveChangesAsync();
                }

                // Mark step as completed
                onboarding.MarkStepCompleted(5);
                onboarding.CurrentStep = 6; // Move to next step

                await _context.SaveChangesAsync();

                var skipMessage = expensesSetup.SkipExpenses ? " (skipped)" : "";
                var response = new QuickSetupResponseDto
                {
                    Success = true,
                    Message = $"Variable expenses setup completed{skipMessage}! Created {totalCreated} expense(s).",
                    ItemsCreated = totalCreated,
                    Progress = MapToProgressDto(onboarding),
                    CreatedItems = createdItems
                };

                return ApiResponse<QuickSetupResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete variable expenses step: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OnboardingProgressDto>> CompleteDashboardTourAsync(string userId, DashboardTourDto dashboardTour)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                
                // Mark step as completed
                onboarding.MarkStepCompleted(6);
                
                await _context.SaveChangesAsync();

                var progressDto = MapToProgressDto(onboarding);
                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to complete dashboard tour: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OnboardingProgressDto>> CompleteOnboardingAsync(string userId, CompleteOnboardingDto completeOnboarding)
        {
            try
            {
                // Complete all steps in sequence
                await CompleteWelcomeStepAsync(userId, completeOnboarding.WelcomeSetup);
                await CompleteIncomeStepAsync(userId, completeOnboarding.IncomeSetup);
                await CompleteBillsStepAsync(userId, completeOnboarding.BillsSetup);
                await CompleteLoansStepAsync(userId, completeOnboarding.LoansSetup);
                await CompleteVariableExpensesStepAsync(userId, completeOnboarding.VariableExpensesSetup);
                await CompleteDashboardTourAsync(userId, completeOnboarding.DashboardTour);

                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                var progressDto = MapToProgressDto(onboarding);

                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to complete onboarding: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OnboardingProgressDto>> SkipOnboardingAsync(string userId)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                onboarding.OnboardingCompleted = true;
                onboarding.CompletedAt = DateTime.UtcNow;
                onboarding.CurrentStep = 6; // Mark as completed
                onboarding.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var progressDto = MapToProgressDto(onboarding);
                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to skip onboarding: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OnboardingProgressDto>> ResetOnboardingAsync(string userId)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                
                // Reset all progress
                onboarding.WelcomeCompleted = false;
                onboarding.IncomeSetupCompleted = false;
                onboarding.BillsSetupCompleted = false;
                onboarding.LoansSetupCompleted = false;
                onboarding.VariableExpensesSetupCompleted = false;
                onboarding.DashboardTourCompleted = false;
                onboarding.OnboardingCompleted = false;
                onboarding.CurrentStep = 1;
                onboarding.StartedAt = DateTime.UtcNow;
                onboarding.CompletedAt = null;
                onboarding.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var progressDto = MapToProgressDto(onboarding);
                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to reset onboarding: {ex.Message}");
            }
        }

        public async Task<bool> IsOnboardingCompletedAsync(string userId)
        {
            try
            {
                var onboarding = await _context.UserOnboardings
                    .FirstOrDefaultAsync(uo => uo.UserId == userId);
                
                return onboarding?.OnboardingCompleted ?? false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsOnboardingStartedAsync(string userId)
        {
            try
            {
                var onboarding = await _context.UserOnboardings
                    .FirstOrDefaultAsync(uo => uo.UserId == userId);
                
                return onboarding != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ApiResponse<OnboardingProgressDto>> GetOrCreateOnboardingAsync(string userId)
        {
            try
            {
                var onboarding = await GetOrCreateOnboardingEntityAsync(userId);
                var progressDto = MapToProgressDto(onboarding);
                return ApiResponse<OnboardingProgressDto>.SuccessResult(progressDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to get or create onboarding: {ex.Message}");
            }
        }

        // Helper methods
        private async Task<UserOnboarding> GetOrCreateOnboardingEntityAsync(string userId)
        {
            var onboarding = await _context.UserOnboardings
                .FirstOrDefaultAsync(uo => uo.UserId == userId);

            if (onboarding == null)
            {
                onboarding = new UserOnboarding
                {
                    UserId = userId,
                    StartedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };

                _context.UserOnboardings.Add(onboarding);
                await _context.SaveChangesAsync();
            }

            return onboarding;
        }

        private OnboardingProgressDto MapToProgressDto(UserOnboarding onboarding)
        {
            return new OnboardingProgressDto
            {
                UserId = onboarding.UserId,
                CurrentStep = onboarding.CurrentStep,
                TotalSteps = onboarding.TotalSteps,
                CompletionPercentage = onboarding.CompletionPercentage,
                IsCompleted = onboarding.OnboardingCompleted,
                StartedAt = onboarding.StartedAt,
                CompletedAt = onboarding.CompletedAt,
                Steps = onboarding.GetStepDetails().Select(s => new OnboardingStepDto
                {
                    StepNumber = s.StepNumber,
                    Title = s.Title,
                    Description = s.Description,
                    IsCompleted = s.IsCompleted,
                    IsCurrent = s.IsCurrent,
                    Icon = s.Icon,
                    Color = s.Color
                }).ToList()
            };
        }
    }
}
