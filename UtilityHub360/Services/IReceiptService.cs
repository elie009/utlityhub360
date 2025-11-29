using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IReceiptService
    {
        Task<ApiResponse<ReceiptDto>> UploadReceiptAsync(string userId, Stream fileStream, string fileName, string contentType);
        Task<ApiResponse<ReceiptDto>> ProcessReceiptOcrAsync(string receiptId, string userId);
        Task<ApiResponse<ReceiptDto>> GetReceiptAsync(string receiptId, string userId);
        Task<ApiResponse<List<ReceiptDto>>> GetReceiptsAsync(string userId, ReceiptSearchDto? searchDto = null);
        Task<ApiResponse<bool>> DeleteReceiptAsync(string receiptId, string userId);
        Task<ApiResponse<ReceiptDto>> LinkReceiptToExpenseAsync(string receiptId, string expenseId, string userId);
        Task<ApiResponse<List<ExpenseMatchDto>>> FindMatchingExpensesAsync(string receiptId, string userId);
        Task<Stream> GetReceiptFileAsync(string receiptId, string userId);
        Task<Stream> GetReceiptThumbnailAsync(string receiptId, string userId);
    }
}

