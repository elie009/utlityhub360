using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<TransactionCategoryDto>> CreateCategoryAsync(CreateTransactionCategoryDto createDto, string userId);
        Task<ApiResponse<TransactionCategoryDto>> UpdateCategoryAsync(string categoryId, UpdateTransactionCategoryDto updateDto, string userId);
        Task<ApiResponse<bool>> DeleteCategoryAsync(string categoryId, string userId);
        Task<ApiResponse<TransactionCategoryDto>> GetCategoryByIdAsync(string categoryId, string userId);
        Task<ApiResponse<List<TransactionCategoryDto>>> GetAllCategoriesAsync(string userId, string? type = null);
        Task<ApiResponse<List<TransactionCategoryDto>>> GetActiveCategoriesAsync(string userId, string? type = null);
        Task<ApiResponse<bool>> SeedSystemCategoriesAsync(string userId);
    }
}

