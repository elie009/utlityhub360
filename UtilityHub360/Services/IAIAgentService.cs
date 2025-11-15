using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IAIAgentService
    {
        Task<ApiResponse<AIAgentResponseDto>> ProcessAgentRequestAsync(AIAgentRequestDto request, string userId);
        Task<ApiResponse<BankTransactionDto>> AnalyzeAndCreateTransactionWithAgentAsync(AnalyzeTransactionTextDto analyzeDto, string userId);
    }
}

