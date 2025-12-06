using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Background service that runs periodically to check loan payment due dates
    /// and send notifications for overdue and upcoming payments
    /// </summary>
    public class LoanDueDateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LoanDueDateBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run every 24 hours (reduced frequency to prevent excessive notifications)

        public LoanDueDateBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<LoanDueDateBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Loan Due Date Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessLoanDueDatesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing loan due dates");
                }

                // Wait for the next interval
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Loan Due Date Background Service stopped");
        }

        private async Task ProcessLoanDueDatesAsync()
        {
            _logger.LogInformation("Processing loan due dates and sending notifications at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var loanDueDateService = new LoanDueDateService(context, notificationService);

            try
            {
                // 1. Update overdue payments and send notifications
                await loanDueDateService.UpdateOverduePaymentsAsync();
                _logger.LogInformation("Completed checking for overdue loan payments");

                // 2. Send reminders for upcoming payments (3 days in advance)
                await loanDueDateService.SendUpcomingPaymentRemindersAsync(daysInAdvance: 3);
                _logger.LogInformation("Completed sending upcoming payment reminders");

                _logger.LogInformation("Completed processing loan due dates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessLoanDueDatesAsync");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Loan Due Date Background Service is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}

