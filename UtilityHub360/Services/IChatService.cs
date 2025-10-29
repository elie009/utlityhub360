using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IChatService
    {
        Task<ApiResponse<ChatResponseDto>> SendMessageAsync(ChatMessageDto messageDto, string userId);
        Task<ApiResponse<PaginatedResponse<ChatConversationDto>>> GetUserConversationsAsync(string userId, int page = 1, int limit = 10);
        Task<ApiResponse<ChatConversationDto>> GetConversationHistoryAsync(string conversationId, string userId);
        Task<ApiResponse<bool>> DeleteConversationAsync(string conversationId, string userId);
        Task<ApiResponse<ChatConversationDto>> CreateConversationAsync(string userId, string? title = null);
        Task<ApiResponse<ChatContextDto>> BuildFinancialContextAsync(string userId);
        Task<ApiResponse<string>> GenerateReportAsync(string userId, string reportType, string format = "pdf");
        Task<ApiResponse<List<string>>> GetBillRemindersAsync(string userId);
        Task<ApiResponse<List<string>>> GetBudgetSuggestionsAsync(string userId);
        Task<bool> IsRateLimitedAsync(string userId);
        Task<ApiResponse<string>> SearchDocumentationAsync(string query);
    }
}

