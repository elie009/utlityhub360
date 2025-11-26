using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IBankStatementExtractionService
    {
        Task<ApiResponse<ExtractBankStatementResponseDto>> ExtractFromFileAsync(
            Stream fileStream, 
            string fileName, 
            string bankAccountId, 
            string userId);
    }
}

