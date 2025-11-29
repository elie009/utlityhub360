using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UtilityHub360.Data;
using UtilityHub360.Entities;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for calculating and applying interest to savings accounts
    /// Supports different account types (High-yield, CD, Money Market) and compounding frequencies
    /// </summary>
    public class SavingsInterestCalculationService
    {
        private readonly ApplicationDbContext _context;
        private readonly AccountingService _accountingService;
        private readonly ILogger<SavingsInterestCalculationService> _logger;

        public SavingsInterestCalculationService(
            ApplicationDbContext context,
            AccountingService accountingService,
            ILogger<SavingsInterestCalculationService> logger)
        {
            _context = context;
            _accountingService = accountingService;
            _logger = logger;
        }

        /// <summary>
        /// Calculates and applies interest for all savings accounts that are due for interest calculation
        /// This should be called by a background service on a scheduled basis
        /// </summary>
        public async Task<InterestCalculationResult> CalculateInterestForAllAccountsAsync(DateTime calculationDate)
        {
            var result = new InterestCalculationResult
            {
                CalculationDate = calculationDate,
                AccountsProcessed = 0,
                TotalInterestPaid = 0,
                Errors = new List<string>()
            };

            try
            {
                // Get all active savings accounts that have interest rates and are due for calculation
                var accountsDue = await _context.SavingsAccounts
                    .Where(sa => sa.IsActive &&
                                !sa.IsDeleted &&
                                sa.InterestRate.HasValue &&
                                sa.InterestRate.Value > 0 &&
                                (sa.NextInterestCalculationDate == null || sa.NextInterestCalculationDate <= calculationDate))
                    .ToListAsync();

                _logger.LogInformation("Found {Count} savings accounts due for interest calculation", accountsDue.Count);

                foreach (var account in accountsDue)
                {
                    try
                    {
                        var interestResult = await CalculateAndApplyInterestAsync(account, calculationDate);
                        result.AccountsProcessed++;
                        result.TotalInterestPaid += interestResult.InterestAmount;
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"Error calculating interest for account {account.Id} ({account.AccountName}): {ex.Message}";
                        _logger.LogError(ex, errorMsg);
                        result.Errors.Add(errorMsg);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Interest calculation completed. Processed {Count} accounts, Total interest: {Amount:C}", 
                    result.AccountsProcessed, result.TotalInterestPaid);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CalculateInterestForAllAccountsAsync");
                result.Errors.Add($"Fatal error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calculates and applies interest for a specific savings account
        /// </summary>
        public async Task<AccountInterestResult> CalculateAndApplyInterestAsync(
            SavingsAccount account, 
            DateTime calculationDate)
        {
            if (!account.InterestRate.HasValue || account.InterestRate.Value <= 0)
            {
                throw new InvalidOperationException($"Account {account.Id} does not have a valid interest rate");
            }

            // Determine the period for interest calculation
            var periodStart = account.LastInterestCalculationDate ?? account.CreatedAt;
            var periodEnd = calculationDate;

            // Calculate number of days in the period
            var daysInPeriod = (periodEnd - periodStart).Days;
            if (daysInPeriod <= 0)
            {
                return new AccountInterestResult
                {
                    AccountId = account.Id,
                    AccountName = account.AccountName,
                    InterestAmount = 0,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    DaysInPeriod = 0
                };
            }

            // Get compounding frequency
            var compoundingFrequency = account.InterestCompoundingFrequency ?? "MONTHLY";
            var annualRate = account.InterestRate.Value;

            // Calculate interest based on compounding frequency
            decimal interestAmount = 0;

            switch (compoundingFrequency.ToUpper())
            {
                case "DAILY":
                    interestAmount = CalculateDailyCompoundInterest(account.CurrentBalance, annualRate, daysInPeriod);
                    break;
                case "MONTHLY":
                    interestAmount = CalculateMonthlyCompoundInterest(account.CurrentBalance, annualRate, daysInPeriod);
                    break;
                case "QUARTERLY":
                    interestAmount = CalculateQuarterlyCompoundInterest(account.CurrentBalance, annualRate, daysInPeriod);
                    break;
                case "ANNUALLY":
                case "YEARLY":
                    interestAmount = CalculateAnnualCompoundInterest(account.CurrentBalance, annualRate, daysInPeriod);
                    break;
                default:
                    // Default to simple interest if frequency is not recognized
                    interestAmount = CalculateSimpleInterest(account.CurrentBalance, annualRate, daysInPeriod);
                    break;
            }

            // Only apply interest if it's greater than a minimum threshold (e.g., $0.01)
            if (interestAmount >= 0.01m)
            {
                // Create interest transaction
                var interestTransaction = new SavingsTransaction
                {
                    SavingsAccountId = account.Id,
                    SourceBankAccountId = account.Id, // Interest doesn't come from a bank account
                    Amount = interestAmount,
                    TransactionType = "INTEREST",
                    Description = $"Interest earned ({compoundingFrequency} compounding, {daysInPeriod} days)",
                    Category = "INTEREST_INCOME",
                    Notes = $"Interest rate: {annualRate:P2}, Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}",
                    TransactionDate = calculationDate,
                    Currency = account.Currency,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SavingsTransactions.Add(interestTransaction);

                // Update account balance
                account.CurrentBalance += interestAmount;
                account.LastInterestCalculationDate = calculationDate;

                // Calculate next interest calculation date based on compounding frequency
                account.NextInterestCalculationDate = CalculateNextInterestDate(calculationDate, compoundingFrequency);
                account.UpdatedAt = DateTime.UtcNow;

                // Create journal entry for interest income
                // Debit: Savings Account (Asset), Credit: Interest Income (Revenue)
                var journalEntry = await _accountingService.CreateInterestIncomeEntryAsync(
                    savingsAccountId: account.Id,
                    userId: account.UserId,
                    interestAmount: interestAmount,
                    savingsAccountName: account.AccountName,
                    interestRate: annualRate,
                    periodDays: daysInPeriod,
                    reference: $"INT-{calculationDate:yyyyMMdd}-{account.Id.Substring(0, 8)}",
                    description: $"Interest earned on {account.AccountName}",
                    entryDate: calculationDate
                );

                _logger.LogInformation(
                    "Applied interest of {Amount:C} to account {AccountId} ({AccountName}). Next calculation: {NextDate}",
                    interestAmount, account.Id, account.AccountName, account.NextInterestCalculationDate);
            }
            else
            {
                // Update calculation dates even if interest is below threshold
                account.LastInterestCalculationDate = calculationDate;
                account.NextInterestCalculationDate = CalculateNextInterestDate(calculationDate, compoundingFrequency);
                account.UpdatedAt = DateTime.UtcNow;
            }

            return new AccountInterestResult
            {
                AccountId = account.Id,
                AccountName = account.AccountName,
                InterestAmount = interestAmount,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                DaysInPeriod = daysInPeriod
            };
        }

        /// <summary>
        /// Calculates daily compound interest
        /// Formula: Principal * ((1 + (Rate / 365))^Days - 1)
        /// </summary>
        private decimal CalculateDailyCompoundInterest(decimal principal, decimal annualRate, int days)
        {
            if (days <= 0) return 0;
            var dailyRate = annualRate / 365m;
            var compoundFactor = (decimal)Math.Pow((double)(1 + dailyRate), days);
            return principal * (compoundFactor - 1);
        }

        /// <summary>
        /// Calculates monthly compound interest
        /// Formula: Principal * ((1 + (Rate / 12))^Months - 1)
        /// </summary>
        private decimal CalculateMonthlyCompoundInterest(decimal principal, decimal annualRate, int days)
        {
            var months = (decimal)days / 30.0m;
            if (months <= 0) return 0;
            var monthlyRate = annualRate / 12m;
            var compoundFactor = (decimal)Math.Pow((double)(1 + monthlyRate), (double)months);
            return principal * (compoundFactor - 1);
        }

        /// <summary>
        /// Calculates quarterly compound interest
        /// Formula: Principal * ((1 + (Rate / 4))^Quarters - 1)
        /// </summary>
        private decimal CalculateQuarterlyCompoundInterest(decimal principal, decimal annualRate, int days)
        {
            var quarters = (decimal)days / 90.0m;
            if (quarters <= 0) return 0;
            var quarterlyRate = annualRate / 4m;
            var compoundFactor = (decimal)Math.Pow((double)(1 + quarterlyRate), (double)quarters);
            return principal * (compoundFactor - 1);
        }

        /// <summary>
        /// Calculates annual compound interest
        /// Formula: Principal * ((1 + Rate)^Years - 1)
        /// </summary>
        private decimal CalculateAnnualCompoundInterest(decimal principal, decimal annualRate, int days)
        {
            var years = (decimal)days / 365.0m;
            if (years <= 0) return 0;
            var compoundFactor = (decimal)Math.Pow((double)(1 + annualRate), (double)years);
            return principal * (compoundFactor - 1);
        }

        /// <summary>
        /// Calculates simple interest (fallback)
        /// Formula: Principal * Rate * (Days / 365)
        /// </summary>
        private decimal CalculateSimpleInterest(decimal principal, decimal annualRate, int days)
        {
            return principal * annualRate * ((decimal)days / 365m);
        }

        /// <summary>
        /// Calculates the next interest calculation date based on compounding frequency
        /// </summary>
        private DateTime CalculateNextInterestDate(DateTime currentDate, string compoundingFrequency)
        {
            return compoundingFrequency.ToUpper() switch
            {
                "DAILY" => currentDate.AddDays(1),
                "MONTHLY" => currentDate.AddMonths(1),
                "QUARTERLY" => currentDate.AddMonths(3),
                "ANNUALLY" or "YEARLY" => currentDate.AddYears(1),
                _ => currentDate.AddMonths(1) // Default to monthly
            };
        }
    }

    /// <summary>
    /// Result of interest calculation for a single account
    /// </summary>
    public class AccountInterestResult
    {
        public string AccountId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal InterestAmount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int DaysInPeriod { get; set; }
    }

    /// <summary>
    /// Result of batch interest calculation
    /// </summary>
    public class InterestCalculationResult
    {
        public DateTime CalculationDate { get; set; }
        public int AccountsProcessed { get; set; }
        public decimal TotalInterestPaid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

