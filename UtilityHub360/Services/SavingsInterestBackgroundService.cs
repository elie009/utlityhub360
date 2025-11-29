using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Background service that runs periodically to calculate and apply interest to savings accounts
    /// Runs daily to check for accounts due for interest calculation
    /// </summary>
    public class SavingsInterestBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SavingsInterestBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run daily

        public SavingsInterestBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SavingsInterestBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Savings Interest Calculation Background Service started");

            // Wait a bit before first run to allow application to fully start
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CalculateInterestForAllAccountsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while calculating savings interest");
                }

                // Wait for the next interval (24 hours)
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Savings Interest Calculation Background Service stopped");
        }

        private async Task CalculateInterestForAllAccountsAsync()
        {
            _logger.LogInformation("Starting interest calculation for savings accounts at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var interestService = scope.ServiceProvider.GetRequiredService<SavingsInterestCalculationService>();
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();

            try
            {
                var calculationDate = DateTime.UtcNow;
                var result = await interestService.CalculateInterestForAllAccountsAsync(calculationDate);

                if (result.Errors.Any())
                {
                    _logger.LogWarning(
                        "Interest calculation completed with {ErrorCount} errors. Processed {AccountCount} accounts, Total interest: {Amount:C}",
                        result.Errors.Count, result.AccountsProcessed, result.TotalInterestPaid);
                    
                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("Interest calculation error: {Error}", error);
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "Interest calculation completed successfully. Processed {AccountCount} accounts, Total interest: {Amount:C}",
                        result.AccountsProcessed, result.TotalInterestPaid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CalculateInterestForAllAccountsAsync");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Savings Interest Calculation Background Service is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}

