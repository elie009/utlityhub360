using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service to check and update loan payment due dates
    /// Handles notifications for upcoming and overdue payments
    /// </summary>
    public class LoanDueDateService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public LoanDueDateService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Check all loans and update overdue payments
        /// This should be called daily by a scheduled job
        /// </summary>
        public async Task UpdateOverduePaymentsAsync()
        {
            var today = DateTime.UtcNow.Date;

            // Find all pending payments that are past due date
            var overduePayments = await _context.RepaymentSchedules
                .Include(rs => rs.Loan)
                .Where(rs => rs.Status == "PENDING" && rs.DueDate.Date < today)
                .ToListAsync();

            foreach (var payment in overduePayments)
            {
                payment.Status = "OVERDUE";
                
                // Send notification to user (skip if notification service not available)
                if (_notificationService != null)
                {
                    // Create metadata with loanId for navigation
                    var metadata = new Dictionary<string, string>
                    {
                        { "loanId", payment.LoanId }
                    };

                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserId = payment.Loan.UserId,
                        Title = "Overdue Loan Payment",
                        Message = $"Your loan payment of ${payment.TotalAmount:F2} was due on {payment.DueDate:MMM dd, yyyy}. Please make a payment as soon as possible.",
                        Type = "PAYMENT_OVERDUE",
                        Channel = "IN_APP",
                        Priority = "HIGH",
                        TemplateVariables = metadata
                    });
                }
            }

            if (overduePayments.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Send reminders for upcoming payments
        /// Call this daily to remind users of payments due soon
        /// </summary>
        /// <param name="daysInAdvance">How many days before due date to send reminder (default: 3 days)</param>
        public async Task SendUpcomingPaymentRemindersAsync(int daysInAdvance = 3)
        {
            var today = DateTime.UtcNow.Date;
            var reminderDate = today.AddDays(daysInAdvance);

            // Find all pending payments due within the reminder period
            var upcomingPayments = await _context.RepaymentSchedules
                .Include(rs => rs.Loan)
                .Where(rs => rs.Status == "PENDING" && 
                            rs.DueDate.Date >= today && 
                            rs.DueDate.Date <= reminderDate)
                .ToListAsync();

            foreach (var payment in upcomingPayments)
            {
                var daysUntilDue = (payment.DueDate.Date - today).Days;
                var message = daysUntilDue == 0 
                    ? $"Your loan payment of ${payment.TotalAmount} is due TODAY!"
                    : $"Reminder: Your loan payment of ${payment.TotalAmount} is due in {daysUntilDue} day(s) on {payment.DueDate:MMM dd, yyyy}.";

                // Send notification (skip if notification service not available)
                if (_notificationService != null)
                {
                    // Create metadata with loanId for navigation
                    var metadata = new Dictionary<string, string>
                    {
                        { "loanId", payment.LoanId }
                    };

                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserId = payment.Loan.UserId,
                        Title = daysUntilDue == 0 ? "Loan Payment Due Today" : "Loan Payment Reminder",
                        Message = message,
                        Type = daysUntilDue == 0 ? "PAYMENT_DUE" : "UPCOMING_DUE",
                        Channel = "IN_APP",
                        Priority = daysUntilDue == 0 ? "HIGH" : "NORMAL",
                        TemplateVariables = metadata
                    });
                }
            }
        }

        /// <summary>
        /// Get next due date for a specific loan
        /// </summary>
        public async Task<DateTime?> GetNextDueDateAsync(string loanId)
        {
            var nextPayment = await _context.RepaymentSchedules
                .Where(rs => rs.LoanId == loanId && rs.Status == "PENDING")
                .OrderBy(rs => rs.DueDate)
                .FirstOrDefaultAsync();

            return nextPayment?.DueDate;
        }

        /// <summary>
        /// Get all upcoming payments for a user
        /// </summary>
        public async Task<List<UpcomingPaymentDto>> GetUpcomingPaymentsForUserAsync(string userId, int days = 30)
        {
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(days);

            var upcomingPayments = await _context.RepaymentSchedules
                .Include(rs => rs.Loan)
                .Where(rs => rs.Loan.UserId == userId &&
                            rs.Status == "PENDING" &&
                            rs.DueDate.Date >= today &&
                            rs.DueDate.Date <= endDate)
                .OrderBy(rs => rs.DueDate)
                .Select(rs => new UpcomingPaymentDto
                {
                    LoanId = rs.LoanId,
                    DueDate = rs.DueDate,
                    Amount = rs.TotalAmount,
                    InstallmentNumber = rs.InstallmentNumber,
                    DaysUntilDue = (rs.DueDate.Date - today).Days,
                    LoanPurpose = rs.Loan.Purpose
                })
                .ToListAsync();

            return upcomingPayments;
        }

        /// <summary>
        /// Get overdue payments for a user
        /// </summary>
        public async Task<List<OverduePaymentDto>> GetOverduePaymentsForUserAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;

            var overduePayments = await _context.RepaymentSchedules
                .Include(rs => rs.Loan)
                .Where(rs => rs.Loan.UserId == userId &&
                            (rs.Status == "OVERDUE" || 
                             (rs.Status == "PENDING" && rs.DueDate.Date < today)))
                .OrderBy(rs => rs.DueDate)
                .Select(rs => new OverduePaymentDto
                {
                    LoanId = rs.LoanId,
                    DueDate = rs.DueDate,
                    Amount = rs.TotalAmount,
                    InstallmentNumber = rs.InstallmentNumber,
                    DaysOverdue = (today - rs.DueDate.Date).Days,
                    LoanPurpose = rs.Loan.Purpose
                })
                .ToListAsync();

            return overduePayments;
        }
    }

    // DTOs for returning payment information
    public class UpcomingPaymentDto
    {
        public string LoanId { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public int InstallmentNumber { get; set; }
        public int DaysUntilDue { get; set; }
        public string LoanPurpose { get; set; } = string.Empty;
    }

    public class OverduePaymentDto
    {
        public string LoanId { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public int InstallmentNumber { get; set; }
        public int DaysOverdue { get; set; }
        public string LoanPurpose { get; set; } = string.Empty;
    }
}

