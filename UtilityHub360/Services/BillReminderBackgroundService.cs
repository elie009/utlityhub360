using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Background service that runs periodically to generate bill reminders and alerts
    /// </summary>
    public class BillReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BillReminderBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(12); // Run every 12 hours (reduced frequency to prevent excessive notifications)

        public BillReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BillReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bill Reminder Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing bill reminders");
                }

                // Wait for the next interval
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Bill Reminder Background Service stopped");
        }

        private async Task ProcessRemindersAsync()
        {
            _logger.LogInformation("Processing bill reminders and auto-generation at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var billAnalyticsService = scope.ServiceProvider.GetRequiredService<IBillAnalyticsService>();
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();

            try
            {
                // Get all active users
                var userIds = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                    context.Users.Where(u => u.IsActive).Select(u => u.Id));

                _logger.LogInformation("Found {Count} active users to process", userIds.Count);

                foreach (var userId in userIds)
                {
                    try
                    {
                        // 1. Auto-generate recurring bills
                        var autoGenResponse = await billAnalyticsService.AutoGenerateAllRecurringBillsAsync(userId);
                        
                        if (autoGenResponse.Success && autoGenResponse.Data != null && autoGenResponse.Data.Any())
                        {
                            _logger.LogInformation(
                                "Auto-generated {Count} bill(s) for user {UserId}", 
                                autoGenResponse.Data.Count, 
                                userId);
                        }

                        // 2. Generate alerts for this user
                        var alertsResponse = await billAnalyticsService.GenerateAlertsAsync(userId);
                        
                        if (alertsResponse.Success && alertsResponse.Data != null && alertsResponse.Data.Any())
                        {
                            _logger.LogInformation(
                                "Generated {Count} alerts for user {UserId}", 
                                alertsResponse.Data.Count, 
                                userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex, 
                            "Error processing user {UserId}", 
                            userId);
                    }
                }

                _logger.LogInformation("Completed processing bill reminders and auto-generation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessRemindersAsync");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bill Reminder Background Service is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}

