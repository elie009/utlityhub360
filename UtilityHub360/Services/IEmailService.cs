namespace UtilityHub360.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName);
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }
}
