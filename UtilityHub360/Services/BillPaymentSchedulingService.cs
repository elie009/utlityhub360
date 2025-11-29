using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Background service for processing scheduled bill payments automatically
    /// </summary>
    public class BillPaymentSchedulingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BillPaymentSchedulingService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

        public BillPaymentSchedulingService(
            IServiceProvider serviceProvider,
            ILogger<BillPaymentSchedulingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bill Payment Scheduling Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledPaymentsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing scheduled bill payments");
                }

                // Wait for the next interval
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Bill Payment Scheduling Service stopped");
        }

        private async Task ProcessScheduledPaymentsAsync()
        {
            _logger.LogInformation("Processing scheduled bill payments at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var billService = scope.ServiceProvider.GetRequiredService<IBillService>();

            try
            {
                var today = DateTime.UtcNow.Date;
                var processedCount = 0;
                var failedCount = 0;

                // Find all bills that are:
                // 1. PENDING status
                // 2. Have scheduled payment enabled
                // 3. Due date matches scheduled payment date (or is today if no days before due)
                // 4. Not already paid
                // 5. Approval status is APPROVED (if approval workflow is enabled)
                var billsToPay = await context.Bills
                    .Where(b => b.Status == "PENDING" &&
                               b.IsScheduledPayment &&
                               !string.IsNullOrEmpty(b.ScheduledPaymentBankAccountId) &&
                               b.ApprovalStatus == "APPROVED" &&
                               !b.IsDeleted)
                    .ToListAsync();

                foreach (var bill in billsToPay)
                {
                    try
                    {
                        // Calculate scheduled payment date
                        var scheduledPaymentDate = bill.DueDate.Date;
                        if (bill.ScheduledPaymentDaysBeforeDue.HasValue && bill.ScheduledPaymentDaysBeforeDue.Value > 0)
                        {
                            scheduledPaymentDate = bill.DueDate.Date.AddDays(-bill.ScheduledPaymentDaysBeforeDue.Value);
                        }

                        // Check if it's time to pay
                        if (today >= scheduledPaymentDate && today <= bill.DueDate.Date)
                        {
                            // Check if we already tried today
                            if (bill.LastScheduledPaymentAttempt.HasValue &&
                                bill.LastScheduledPaymentAttempt.Value.Date == today)
                            {
                                continue; // Already attempted today
                            }

                            // Verify bank account exists and has sufficient balance
                            var bankAccount = await context.BankAccounts
                                .FirstOrDefaultAsync(ba => ba.Id == bill.ScheduledPaymentBankAccountId &&
                                                          ba.UserId == bill.UserId &&
                                                          ba.IsActive);

                            if (bankAccount == null)
                            {
                                bill.LastScheduledPaymentAttempt = DateTime.UtcNow;
                                bill.ScheduledPaymentFailureReason = "Bank account not found or inactive";
                                bill.UpdatedAt = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                                failedCount++;
                                _logger.LogWarning(
                                    "Scheduled payment failed for bill {BillId}: Bank account not found",
                                    bill.Id);
                                continue;
                            }

                            if (bankAccount.CurrentBalance < bill.Amount)
                            {
                                bill.LastScheduledPaymentAttempt = DateTime.UtcNow;
                                bill.ScheduledPaymentFailureReason = $"Insufficient balance. Required: {bill.Amount}, Available: {bankAccount.CurrentBalance}";
                                bill.UpdatedAt = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                                failedCount++;
                                _logger.LogWarning(
                                    "Scheduled payment failed for bill {BillId}: Insufficient balance",
                                    bill.Id);
                                continue;
                            }

                            // Process the payment
                            _logger.LogInformation(
                                "Processing scheduled payment for bill {BillId} ({BillName}), Amount: {Amount}",
                                bill.Id,
                                bill.BillName,
                                bill.Amount);

                            var paymentResult = await billService.MarkBillAsPaidAsync(
                                bill.Id,
                                bill.UserId,
                                $"Automatically paid via scheduled payment on {DateTime.UtcNow:yyyy-MM-dd}",
                                bill.ScheduledPaymentBankAccountId);

                            if (paymentResult.Success)
                            {
                                bill.LastScheduledPaymentAttempt = DateTime.UtcNow;
                                bill.ScheduledPaymentFailureReason = null; // Clear any previous failure
                                bill.UpdatedAt = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                                processedCount++;
                                _logger.LogInformation(
                                    "Successfully processed scheduled payment for bill {BillId}",
                                    bill.Id);
                            }
                            else
                            {
                                bill.LastScheduledPaymentAttempt = DateTime.UtcNow;
                                bill.ScheduledPaymentFailureReason = paymentResult.Message ?? "Payment processing failed";
                                bill.UpdatedAt = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                                failedCount++;
                                _logger.LogWarning(
                                    "Scheduled payment failed for bill {BillId}: {Reason}",
                                    bill.Id,
                                    paymentResult.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing scheduled payment for bill {BillId}",
                            bill.Id);
                        
                        bill.LastScheduledPaymentAttempt = DateTime.UtcNow;
                        bill.ScheduledPaymentFailureReason = $"Error: {ex.Message}";
                        bill.UpdatedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                        failedCount++;
                    }
                }

                if (processedCount > 0 || failedCount > 0)
                {
                    _logger.LogInformation(
                        "Scheduled payment processing completed. Processed: {Processed}, Failed: {Failed}",
                        processedCount,
                        failedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessScheduledPaymentsAsync");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bill Payment Scheduling Service is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}

