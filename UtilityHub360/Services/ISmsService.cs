namespace UtilityHub360.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> SendSmsAsync(string phoneNumber, string templateId, Dictionary<string, string> variables);
    }
}

