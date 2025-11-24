using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Background service that syncs bank accounts based on their sync frequency
    /// </summary>
    public class BankAccountSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BankAccountSyncBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15); // Check every 15 minutes

        public BankAccountSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BankAccountSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bank Account Sync Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncAccountsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while syncing bank accounts");
                }

                // Wait for the next interval
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Bank Account Sync Background Service stopped");
        }

        private async Task SyncAccountsAsync()
        {
            _logger.LogInformation("Checking for accounts to sync at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var bankFeedService = scope.ServiceProvider.GetRequiredService<IBankFeedService>();
            var bankAccountService = scope.ServiceProvider.GetRequiredService<IBankAccountService>();

            try
            {
                // Get all connected accounts
                var connectedAccounts = await context.BankAccounts
                    .Where(a => a.IsConnected && a.IsActive && !a.IsDeleted)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} connected accounts to check", connectedAccounts.Count);

                foreach (var account in connectedAccounts)
                {
                    try
                    {
                        var shouldSync = ShouldSyncAccount(account);
                        
                        if (shouldSync)
                        {
                            _logger.LogInformation("Syncing account {AccountId} ({AccountName})", account.Id, account.AccountName);
                            
                            // Fetch transactions from bank feed
                            var lastSyncDate = account.LastSyncedAt;
                            var fetchResult = await bankFeedService.FetchTransactionsAsync(account.Id, lastSyncDate);

                            if (fetchResult.Success && fetchResult.Data != null && fetchResult.Data.Any())
                            {
                                _logger.LogInformation(
                                    "Fetched {Count} new transaction(s) for account {AccountId}",
                                    fetchResult.Data.Count,
                                    account.Id);

                                // Process and import transactions
                                // This would be handled by the BankAccountService
                                // For now, we'll just update the last sync time
                                account.LastSyncedAt = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                            }
                            else
                            {
                                // Update last sync time even if no new transactions
                                account.LastSyncedAt = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing account {AccountId}", account.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in sync process");
            }
        }

        private bool ShouldSyncAccount(Entities.BankAccount account)
        {
            if (!account.IsConnected || account.LastSyncedAt == null)
            {
                return true; // First sync or never synced
            }

            var timeSinceLastSync = DateTime.UtcNow - account.LastSyncedAt.Value;
            var syncFrequency = account.SyncFrequency?.ToUpper() ?? "MANUAL";

            return syncFrequency switch
            {
                "DAILY" => timeSinceLastSync >= TimeSpan.FromHours(24),
                "WEEKLY" => timeSinceLastSync >= TimeSpan.FromDays(7),
                "MONTHLY" => timeSinceLastSync >= TimeSpan.FromDays(30),
                "MANUAL" => false, // Manual sync only
                _ => false
            };
        }
    }
}

