using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class SavingsService : ISavingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBankAccountService _bankAccountService;

        public SavingsService(ApplicationDbContext context, IBankAccountService bankAccountService)
        {
            _context = context;
            _bankAccountService = bankAccountService;
        }

        public async Task<ApiResponse<SavingsAccountDto>> CreateSavingsAccountAsync(CreateSavingsAccountDto savingsAccountDto, string userId)
        {
            try
            {
                var savingsAccount = new SavingsAccount
                {
                    UserId = userId,
                    AccountName = savingsAccountDto.AccountName,
                    SavingsType = savingsAccountDto.SavingsType,
                    TargetAmount = savingsAccountDto.TargetAmount,
                    CurrentBalance = 0,
                    Currency = savingsAccountDto.Currency,
                    Description = savingsAccountDto.Description,
                    Goal = savingsAccountDto.Goal,
                    TargetDate = savingsAccountDto.TargetDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SavingsAccounts.Add(savingsAccount);
                await _context.SaveChangesAsync();

                var resultDto = MapToSavingsAccountDto(savingsAccount);
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = true,
                    Data = resultDto,
                    Message = "Savings account created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = $"Error creating savings account: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<SavingsAccountDto>>> GetUserSavingsAccountsAsync(string userId)
        {
            try
            {
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId && sa.IsActive)
                    .OrderByDescending(sa => sa.CreatedAt)
                    .ToListAsync();

                var savingsAccountDtos = savingsAccounts.Select(MapToSavingsAccountDto).ToList();

                return new ApiResponse<List<SavingsAccountDto>>
                {
                    Success = true,
                    Data = savingsAccountDtos,
                    Message = "Savings accounts retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SavingsAccountDto>>
                {
                    Success = false,
                    Message = $"Error retrieving savings accounts: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<SavingsAccountDto>> GetSavingsAccountByIdAsync(string savingsAccountId, string userId)
        {
            try
            {
                var savingsAccount = await _context.SavingsAccounts
                    .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId && sa.UserId == userId);

                if (savingsAccount == null)
                {
                    return new ApiResponse<SavingsAccountDto>
                    {
                        Success = false,
                        Message = "Savings account not found"
                    };
                }

                var savingsAccountDto = MapToSavingsAccountDto(savingsAccount);
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = true,
                    Data = savingsAccountDto,
                    Message = "Savings account retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = $"Error retrieving savings account: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<SavingsAccountDto>> UpdateSavingsAccountAsync(string savingsAccountId, CreateSavingsAccountDto updateDto, string userId)
        {
            try
            {
                var savingsAccount = await _context.SavingsAccounts
                    .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId && sa.UserId == userId);

                if (savingsAccount == null)
                {
                    return new ApiResponse<SavingsAccountDto>
                    {
                        Success = false,
                        Message = "Savings account not found"
                    };
                }

                savingsAccount.AccountName = updateDto.AccountName;
                savingsAccount.SavingsType = updateDto.SavingsType;
                savingsAccount.TargetAmount = updateDto.TargetAmount;
                savingsAccount.Currency = updateDto.Currency;
                savingsAccount.Description = updateDto.Description;
                savingsAccount.Goal = updateDto.Goal;
                savingsAccount.TargetDate = updateDto.TargetDate;
                savingsAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var savingsAccountDto = MapToSavingsAccountDto(savingsAccount);
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = true,
                    Data = savingsAccountDto,
                    Message = "Savings account updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = $"Error updating savings account: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteSavingsAccountAsync(string savingsAccountId, string userId)
        {
            try
            {
                var savingsAccount = await _context.SavingsAccounts
                    .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId && sa.UserId == userId);

                if (savingsAccount == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Savings account not found"
                    };
                }

                // Check if there are any transactions
                var hasTransactions = await _context.SavingsTransactions
                    .AnyAsync(st => st.SavingsAccountId == savingsAccountId);

                if (hasTransactions)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete savings account with existing transactions"
                    };
                }

                _context.SavingsAccounts.Remove(savingsAccount);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Savings account deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error deleting savings account: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<SavingsTransactionDto>> CreateSavingsTransactionAsync(CreateSavingsTransactionDto transactionDto, string userId)
        {
            try
            {
                // Verify savings account belongs to user
                var savingsAccount = await _context.SavingsAccounts
                    .FirstOrDefaultAsync(sa => sa.Id == transactionDto.SavingsAccountId && sa.UserId == userId);

                if (savingsAccount == null)
                {
                    return new ApiResponse<SavingsTransactionDto>
                    {
                        Success = false,
                        Message = "Savings account not found"
                    };
                }

                // Verify source bank account belongs to user
                var sourceBankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == transactionDto.SourceBankAccountId && ba.UserId == userId);

                if (sourceBankAccount == null)
                {
                    return new ApiResponse<SavingsTransactionDto>
                    {
                        Success = false,
                        Message = "Source bank account not found"
                    };
                }

                // For deposits, check if source account has sufficient balance
                if (transactionDto.TransactionType == "DEPOSIT")
                {
                    if (sourceBankAccount.CurrentBalance < transactionDto.Amount)
                    {
                        return new ApiResponse<SavingsTransactionDto>
                        {
                            Success = false,
                            Message = "Insufficient balance in source account"
                        };
                    }
                }

                // For withdrawals, check if savings account has sufficient balance
                if (transactionDto.TransactionType == "WITHDRAWAL")
                {
                    if (savingsAccount.CurrentBalance < transactionDto.Amount)
                    {
                        return new ApiResponse<SavingsTransactionDto>
                        {
                            Success = false,
                            Message = "Insufficient balance in savings account"
                        };
                    }
                }

                // Create savings transaction
                var savingsTransaction = new SavingsTransaction
                {
                    SavingsAccountId = transactionDto.SavingsAccountId,
                    SourceBankAccountId = transactionDto.SourceBankAccountId,
                    Amount = transactionDto.Amount,
                    TransactionType = transactionDto.TransactionType,
                    Description = transactionDto.Description,
                    Category = transactionDto.Category,
                    Notes = transactionDto.Notes,
                    TransactionDate = transactionDto.TransactionDate,
                    Currency = transactionDto.Currency,
                    IsRecurring = transactionDto.IsRecurring,
                    RecurringFrequency = transactionDto.RecurringFrequency,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SavingsTransactions.Add(savingsTransaction);

                // Update balances
                if (transactionDto.TransactionType == "DEPOSIT")
                {
                    savingsAccount.CurrentBalance += transactionDto.Amount;
                    sourceBankAccount.CurrentBalance -= transactionDto.Amount;
                }
                else if (transactionDto.TransactionType == "WITHDRAWAL")
                {
                    savingsAccount.CurrentBalance -= transactionDto.Amount;
                    sourceBankAccount.CurrentBalance += transactionDto.Amount;
                }

                savingsAccount.UpdatedAt = DateTime.UtcNow;
                sourceBankAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var savingsTransactionDto = MapToSavingsTransactionDto(savingsTransaction);
                return new ApiResponse<SavingsTransactionDto>
                {
                    Success = true,
                    Data = savingsTransactionDto,
                    Message = "Savings transaction created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsTransactionDto>
                {
                    Success = false,
                    Message = $"Error creating savings transaction: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<SavingsTransactionDto>>> GetSavingsTransactionsAsync(string savingsAccountId, string userId, int page = 1, int limit = 50)
        {
            try
            {
                // Verify savings account belongs to user
                var savingsAccount = await _context.SavingsAccounts
                    .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId && sa.UserId == userId);

                if (savingsAccount == null)
                {
                    return new ApiResponse<List<SavingsTransactionDto>>
                    {
                        Success = false,
                        Message = "Savings account not found"
                    };
                }

                var transactions = await _context.SavingsTransactions
                    .Where(st => st.SavingsAccountId == savingsAccountId)
                    .OrderByDescending(st => st.TransactionDate)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var transactionDtos = transactions.Select(MapToSavingsTransactionDto).ToList();

                return new ApiResponse<List<SavingsTransactionDto>>
                {
                    Success = true,
                    Data = transactionDtos,
                    Message = "Savings transactions retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SavingsTransactionDto>>
                {
                    Success = false,
                    Message = $"Error retrieving savings transactions: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<SavingsTransactionDto>> GetSavingsTransactionByIdAsync(string transactionId, string userId)
        {
            try
            {
                var transaction = await _context.SavingsTransactions
                    .Include(st => st.SavingsAccount)
                    .FirstOrDefaultAsync(st => st.Id == transactionId && st.SavingsAccount.UserId == userId);

                if (transaction == null)
                {
                    return new ApiResponse<SavingsTransactionDto>
                    {
                        Success = false,
                        Message = "Savings transaction not found"
                    };
                }

                var transactionDto = MapToSavingsTransactionDto(transaction);
                return new ApiResponse<SavingsTransactionDto>
                {
                    Success = true,
                    Data = transactionDto,
                    Message = "Savings transaction retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsTransactionDto>
                {
                    Success = false,
                    Message = $"Error retrieving savings transaction: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<SavingsSummaryDto>> GetSavingsSummaryAsync(string userId)
        {
            try
            {
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId && sa.IsActive)
                    .ToListAsync();

                var totalBalance = savingsAccounts.Sum(sa => sa.CurrentBalance);
                var totalTarget = savingsAccounts.Sum(sa => sa.TargetAmount);
                var overallProgress = totalTarget > 0 ? (totalBalance / totalTarget) * 100 : 0;

                var activeGoals = savingsAccounts.Count(sa => sa.CurrentBalance < sa.TargetAmount);
                var completedGoals = savingsAccounts.Count(sa => sa.CurrentBalance >= sa.TargetAmount);

                // Calculate monthly savings target
                var monthlyTarget = savingsAccounts
                    .Where(sa => sa.TargetDate > DateTime.UtcNow)
                    .Sum(sa => (sa.TargetAmount - sa.CurrentBalance) / Math.Max(1, (decimal)(sa.TargetDate - DateTime.UtcNow).Days / 30.0m));

                // Calculate this month's savings
                var thisMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var thisMonthSaved = await _context.SavingsTransactions
                    .Where(st => st.SavingsAccount.UserId == userId && 
                                st.TransactionType == "DEPOSIT" && 
                                st.TransactionDate >= thisMonthStart)
                    .SumAsync(st => st.Amount);

                var recentAccounts = savingsAccounts
                    .OrderByDescending(sa => sa.UpdatedAt)
                    .Take(5)
                    .Select(MapToSavingsAccountDto)
                    .ToList();

                var summary = new SavingsSummaryDto
                {
                    TotalSavingsAccounts = savingsAccounts.Count,
                    TotalSavingsBalance = totalBalance,
                    TotalTargetAmount = totalTarget,
                    OverallProgressPercentage = overallProgress,
                    ActiveGoals = activeGoals,
                    CompletedGoals = completedGoals,
                    MonthlySavingsTarget = monthlyTarget,
                    ThisMonthSaved = thisMonthSaved,
                    RecentAccounts = recentAccounts
                };

                return new ApiResponse<SavingsSummaryDto>
                {
                    Success = true,
                    Data = summary,
                    Message = "Savings summary retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsSummaryDto>
                {
                    Success = false,
                    Message = $"Error retrieving savings summary: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<SavingsAnalyticsDto>> GetSavingsAnalyticsAsync(string userId, string period = "month")
        {
            try
            {
                var (startDate, endDate) = GetPeriodDates(period);
                
                var transactions = await _context.SavingsTransactions
                    .Where(st => st.SavingsAccount.UserId == userId && 
                                st.TransactionDate >= startDate && 
                                st.TransactionDate <= endDate)
                    .ToListAsync();

                var totalSaved = transactions
                    .Where(t => t.TransactionType == "DEPOSIT")
                    .Sum(t => t.Amount);

                var totalWithdrawn = transactions
                    .Where(t => t.TransactionType == "WITHDRAWAL")
                    .Sum(t => t.Amount);

                var netSavings = totalSaved - totalWithdrawn;

                var savingsByType = await _context.SavingsTransactions
                    .Include(st => st.SavingsAccount)
                    .Where(st => st.SavingsAccount.UserId == userId && 
                                st.TransactionType == "DEPOSIT" &&
                                st.TransactionDate >= startDate && 
                                st.TransactionDate <= endDate)
                    .GroupBy(st => st.SavingsAccount.SavingsType)
                    .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Amount));

                var savingsByCategory = transactions
                    .Where(t => t.TransactionType == "DEPOSIT" && !string.IsNullOrEmpty(t.Category))
                    .GroupBy(t => t.Category!)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                var recentTransactions = transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .Select(MapToSavingsTransactionDto)
                    .ToList();

                var averageMonthlySavings = period == "month" ? totalSaved : totalSaved / GetPeriodMonths(period);
                var averageTransactionAmount = transactions.Any() ? transactions.Average(t => t.Amount) : 0;

                var analytics = new SavingsAnalyticsDto
                {
                    TotalSaved = totalSaved,
                    TotalWithdrawn = totalWithdrawn,
                    NetSavings = netSavings,
                    TotalTransactions = transactions.Count,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    SavingsByType = savingsByType,
                    SavingsByCategory = savingsByCategory,
                    RecentTransactions = recentTransactions,
                    AverageMonthlySavings = averageMonthlySavings,
                    AverageTransactionAmount = averageTransactionAmount
                };

                return new ApiResponse<SavingsAnalyticsDto>
                {
                    Success = true,
                    Data = analytics,
                    Message = "Savings analytics retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsAnalyticsDto>
                {
                    Success = false,
                    Message = $"Error retrieving savings analytics: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetSavingsByTypeAsync(string userId)
        {
            try
            {
                var savingsByType = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId && sa.IsActive)
                    .GroupBy(sa => sa.SavingsType)
                    .ToDictionaryAsync(g => g.Key, g => g.Sum(sa => sa.CurrentBalance));

                return new ApiResponse<Dictionary<string, decimal>>
                {
                    Success = true,
                    Data = savingsByType,
                    Message = "Savings by type retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Dictionary<string, decimal>>
                {
                    Success = false,
                    Message = $"Error retrieving savings by type: {ex.Message}"
                };
            }
        }

        // Auto-Save Features (Placeholder implementations)
        public async Task<ApiResponse<bool>> CreateAutoSaveAsync(AutoSaveSettingsDto autoSaveDto, string userId)
        {
            // Implementation for auto-save settings
            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Auto-save feature coming soon"
            };
        }

        public async Task<ApiResponse<List<AutoSaveSettingsDto>>> GetAutoSaveSettingsAsync(string userId)
        {
            // Implementation for getting auto-save settings
            return new ApiResponse<List<AutoSaveSettingsDto>>
            {
                Success = true,
                Data = new List<AutoSaveSettingsDto>(),
                Message = "Auto-save settings retrieved successfully"
            };
        }

        public async Task<ApiResponse<bool>> UpdateAutoSaveAsync(string autoSaveId, AutoSaveSettingsDto updateDto, string userId)
        {
            // Implementation for updating auto-save settings
            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Auto-save updated successfully"
            };
        }

        public async Task<ApiResponse<bool>> DeleteAutoSaveAsync(string autoSaveId, string userId)
        {
            // Implementation for deleting auto-save settings
            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Auto-save deleted successfully"
            };
        }

        public async Task<ApiResponse<bool>> ExecuteAutoSaveAsync(string userId)
        {
            // Implementation for executing auto-save
            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Auto-save executed successfully"
            };
        }

        // Savings Goals and Progress
        public async Task<ApiResponse<SavingsAccountDto>> UpdateSavingsGoalAsync(string savingsAccountId, decimal newTargetAmount, DateTime? newTargetDate, string userId)
        {
            try
            {
                var savingsAccount = await _context.SavingsAccounts
                    .FirstOrDefaultAsync(sa => sa.Id == savingsAccountId && sa.UserId == userId);

                if (savingsAccount == null)
                {
                    return new ApiResponse<SavingsAccountDto>
                    {
                        Success = false,
                        Message = "Savings account not found"
                    };
                }

                savingsAccount.TargetAmount = newTargetAmount;
                if (newTargetDate.HasValue)
                {
                    savingsAccount.TargetDate = newTargetDate.Value;
                }
                savingsAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var savingsAccountDto = MapToSavingsAccountDto(savingsAccount);
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = true,
                    Data = savingsAccountDto,
                    Message = "Savings goal updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = $"Error updating savings goal: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<SavingsAccountDto>>> GetSavingsGoalsByTypeAsync(string savingsType, string userId)
        {
            try
            {
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId && sa.SavingsType == savingsType && sa.IsActive)
                    .OrderByDescending(sa => sa.CreatedAt)
                    .ToListAsync();

                var savingsAccountDtos = savingsAccounts.Select(MapToSavingsAccountDto).ToList();

                return new ApiResponse<List<SavingsAccountDto>>
                {
                    Success = true,
                    Data = savingsAccountDtos,
                    Message = "Savings goals retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SavingsAccountDto>>
                {
                    Success = false,
                    Message = $"Error retrieving savings goals: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalSavingsProgressAsync(string userId)
        {
            try
            {
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId && sa.IsActive)
                    .ToListAsync();

                var totalBalance = savingsAccounts.Sum(sa => sa.CurrentBalance);
                var totalTarget = savingsAccounts.Sum(sa => sa.TargetAmount);
                var progressPercentage = totalTarget > 0 ? (totalBalance / totalTarget) * 100 : 0;

                return new ApiResponse<decimal>
                {
                    Success = true,
                    Data = progressPercentage,
                    Message = "Total savings progress retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<decimal>
                {
                    Success = false,
                    Message = $"Error retrieving savings progress: {ex.Message}"
                };
            }
        }

        // Bank Account Integration
        public async Task<ApiResponse<bool>> TransferFromBankToSavingsAsync(string bankAccountId, string savingsAccountId, decimal amount, string description, string userId)
        {
            try
            {
                var transactionDto = new CreateSavingsTransactionDto
                {
                    SavingsAccountId = savingsAccountId,
                    SourceBankAccountId = bankAccountId,
                    Amount = amount,
                    TransactionType = "DEPOSIT",
                    Description = description,
                    Category = "TRANSFER",
                    TransactionDate = DateTime.UtcNow
                };

                var result = await CreateSavingsTransactionAsync(transactionDto, userId);
                return new ApiResponse<bool>
                {
                    Success = result.Success,
                    Data = result.Success,
                    Message = result.Message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error transferring from bank to savings: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> TransferFromSavingsToBankAsync(string savingsAccountId, string bankAccountId, decimal amount, string description, string userId)
        {
            try
            {
                var transactionDto = new CreateSavingsTransactionDto
                {
                    SavingsAccountId = savingsAccountId,
                    SourceBankAccountId = bankAccountId,
                    Amount = amount,
                    TransactionType = "WITHDRAWAL",
                    Description = description,
                    Category = "TRANSFER",
                    TransactionDate = DateTime.UtcNow
                };

                var result = await CreateSavingsTransactionAsync(transactionDto, userId);
                return new ApiResponse<bool>
                {
                    Success = result.Success,
                    Data = result.Success,
                    Message = result.Message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error transferring from savings to bank: {ex.Message}"
                };
            }
        }

        // Helper Methods
        private SavingsAccountDto MapToSavingsAccountDto(SavingsAccount savingsAccount)
        {
            var progressPercentage = savingsAccount.TargetAmount > 0 
                ? (savingsAccount.CurrentBalance / savingsAccount.TargetAmount) * 100 
                : 0;

            var remainingAmount = Math.Max(0, savingsAccount.TargetAmount - savingsAccount.CurrentBalance);
            var daysRemaining = Math.Max(0, (savingsAccount.TargetDate - DateTime.UtcNow).Days);
            var monthlyTarget = daysRemaining > 0 
                ? remainingAmount / Math.Max(1, (decimal)daysRemaining / 30.0m) 
                : 0;

            return new SavingsAccountDto
            {
                Id = savingsAccount.Id,
                AccountName = savingsAccount.AccountName,
                SavingsType = savingsAccount.SavingsType,
                TargetAmount = savingsAccount.TargetAmount,
                CurrentBalance = savingsAccount.CurrentBalance,
                Currency = savingsAccount.Currency,
                Description = savingsAccount.Description,
                Goal = savingsAccount.Goal,
                TargetDate = savingsAccount.TargetDate,
                IsActive = savingsAccount.IsActive,
                CreatedAt = savingsAccount.CreatedAt,
                UpdatedAt = savingsAccount.UpdatedAt,
                ProgressPercentage = progressPercentage,
                RemainingAmount = remainingAmount,
                DaysRemaining = daysRemaining,
                MonthlyTarget = monthlyTarget
            };
        }

        private SavingsTransactionDto MapToSavingsTransactionDto(SavingsTransaction transaction)
        {
            return new SavingsTransactionDto
            {
                Id = transaction.Id,
                SavingsAccountId = transaction.SavingsAccountId,
                SourceBankAccountId = transaction.SourceBankAccountId,
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType,
                Description = transaction.Description,
                Category = transaction.Category,
                Notes = transaction.Notes,
                TransactionDate = transaction.TransactionDate,
                Currency = transaction.Currency,
                IsRecurring = transaction.IsRecurring,
                RecurringFrequency = transaction.RecurringFrequency,
                CreatedAt = transaction.CreatedAt
            };
        }

        private (DateTime startDate, DateTime endDate) GetPeriodDates(string period)
        {
            var now = DateTime.UtcNow;
            return period.ToLower() switch
            {
                "week" => (now.AddDays(-7), now),
                "month" => (new DateTime(now.Year, now.Month, 1), now),
                "quarter" => (now.AddMonths(-3), now),
                "year" => (new DateTime(now.Year, 1, 1), now),
                _ => (new DateTime(now.Year, now.Month, 1), now)
            };
        }

        private decimal GetPeriodMonths(string period)
        {
            return period.ToLower() switch
            {
                "week" => 0.25m,
                "month" => 1,
                "quarter" => 3,
                "year" => 12,
                _ => 1
            };
        }
    }
}
