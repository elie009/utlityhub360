using System.Net;
using System.Net.Mail;
using System.Text;

namespace UtilityHub360.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
        {
            var resetUrl = $"{_configuration["AppSettings:BaseUrl"]}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";
            
            var subject = "Password Reset Request - UtilityHub360";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Password Reset Request</h2>
                        <p>Hello {userName},</p>
                        <p>We received a request to reset your password for your UtilityHub360 account.</p>
                        <p>Click the button below to reset your password:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetUrl}' 
                               style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Reset Password
                            </a>
                        </div>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 3px;'>
                            {resetUrl}
                        </p>
                        <p><strong>This link will expire in 1 hour.</strong></p>
                        <p>If you didn't request this password reset, please ignore this email.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #666;'>
                            This email was sent from UtilityHub360. Please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpHost = smtpSettings["Host"];
                var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
                var smtpUsername = smtpSettings["Username"];
                var smtpPassword = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"] ?? "UtilityHub360";

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("SMTP settings not configured. Email will not be sent.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var message = new MailMessage();
                message.From = new MailAddress(fromEmail ?? smtpUsername, fromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Password reset email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                return false;
            }
        }
    }
}
