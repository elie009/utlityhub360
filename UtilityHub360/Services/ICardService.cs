using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface ICardService
    {
        // Card CRUD Operations
        Task<ApiResponse<CardDto>> CreateCardAsync(CreateCardDto createCardDto, string userId);
        Task<ApiResponse<CardDto>> GetCardAsync(string cardId, string userId);
        Task<ApiResponse<CardDto>> UpdateCardAsync(string cardId, UpdateCardDto updateCardDto, string userId);
        Task<ApiResponse<bool>> DeleteCardAsync(string cardId, string userId);
        
        // Card Queries
        Task<ApiResponse<List<CardDto>>> GetBankAccountCardsAsync(string bankAccountId, string userId);
        Task<ApiResponse<List<CardDto>>> GetUserCardsAsync(string userId, bool includeInactive = false);
        Task<ApiResponse<CardDto>> GetPrimaryCardAsync(string bankAccountId, string userId);
        
        // Card Management
        Task<ApiResponse<CardDto>> SetPrimaryCardAsync(string cardId, string userId);
        Task<ApiResponse<CardDto>> ActivateCardAsync(string cardId, string userId);
        Task<ApiResponse<CardDto>> DeactivateCardAsync(string cardId, string userId);
    }
}

