using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _context;

        public CardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CardDto>> CreateCardAsync(CreateCardDto createCardDto, string userId)
        {
            try
            {
                // Verify bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == createCardDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("Bank account not found or you don't have access to it");
                }

                // If setting as primary, unset other primary cards for this account
                if (createCardDto.IsPrimary)
                {
                    var existingPrimaryCards = await _context.Cards
                        .Where(c => c.BankAccountId == createCardDto.BankAccountId && 
                                   c.IsPrimary && 
                                   !c.IsDeleted)
                        .ToListAsync();

                    foreach (var existingCard in existingPrimaryCards)
                    {
                        existingCard.IsPrimary = false;
                        existingCard.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var card = new Card
                {
                    Id = Guid.NewGuid().ToString(),
                    BankAccountId = createCardDto.BankAccountId,
                    UserId = userId,
                    CardName = createCardDto.CardName,
                    CardType = createCardDto.CardType.ToUpper(),
                    CardBrand = string.IsNullOrWhiteSpace(createCardDto.CardBrand) ? null : createCardDto.CardBrand.ToUpper(),
                    Last4Digits = string.IsNullOrWhiteSpace(createCardDto.Last4Digits) ? null : createCardDto.Last4Digits,
                    CardholderName = string.IsNullOrWhiteSpace(createCardDto.CardholderName) ? null : createCardDto.CardholderName,
                    ExpiryMonth = string.IsNullOrWhiteSpace(createCardDto.ExpiryMonth) ? null : createCardDto.ExpiryMonth,
                    ExpiryYear = string.IsNullOrWhiteSpace(createCardDto.ExpiryYear) ? null : createCardDto.ExpiryYear,
                    IsPrimary = createCardDto.IsPrimary,
                    IsActive = true,
                    Description = string.IsNullOrWhiteSpace(createCardDto.Description) ? null : createCardDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Cards.Add(card);
                await _context.SaveChangesAsync();

                var cardDto = await MapToCardDtoAsync(card);
                return ApiResponse<CardDto>.SuccessResult(cardDto, "Card created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CardDto>.ErrorResult($"Failed to create card: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CardDto>> GetCardAsync(string cardId, string userId)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && !c.IsDeleted);

                if (card == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("Card not found");
                }

                var cardDto = await MapToCardDtoAsync(card);
                return ApiResponse<CardDto>.SuccessResult(cardDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CardDto>.ErrorResult($"Failed to get card: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CardDto>> UpdateCardAsync(string cardId, UpdateCardDto updateCardDto, string userId)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && !c.IsDeleted);

                if (card == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("Card not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateCardDto.CardName))
                    card.CardName = updateCardDto.CardName;

                if (!string.IsNullOrEmpty(updateCardDto.CardType))
                    card.CardType = updateCardDto.CardType.ToUpper();

                if (updateCardDto.CardBrand != null)
                    card.CardBrand = string.IsNullOrWhiteSpace(updateCardDto.CardBrand) ? null : updateCardDto.CardBrand.ToUpper();

                if (updateCardDto.Last4Digits != null)
                    card.Last4Digits = string.IsNullOrWhiteSpace(updateCardDto.Last4Digits) ? null : updateCardDto.Last4Digits;

                if (updateCardDto.CardholderName != null)
                    card.CardholderName = string.IsNullOrWhiteSpace(updateCardDto.CardholderName) ? null : updateCardDto.CardholderName;

                if (updateCardDto.ExpiryMonth != null)
                    card.ExpiryMonth = string.IsNullOrWhiteSpace(updateCardDto.ExpiryMonth) ? null : updateCardDto.ExpiryMonth;

                if (updateCardDto.ExpiryYear != null)
                    card.ExpiryYear = string.IsNullOrWhiteSpace(updateCardDto.ExpiryYear) ? null : updateCardDto.ExpiryYear;

                if (updateCardDto.IsPrimary.HasValue)
                {
                    // If setting as primary, unset other primary cards for this account
                    if (updateCardDto.IsPrimary.Value)
                    {
                        var existingPrimaryCards = await _context.Cards
                            .Where(c => c.BankAccountId == card.BankAccountId && 
                                       c.Id != cardId &&
                                       c.IsPrimary && 
                                       !c.IsDeleted)
                            .ToListAsync();

                        foreach (var primaryCard in existingPrimaryCards)
                        {
                            primaryCard.IsPrimary = false;
                            primaryCard.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    card.IsPrimary = updateCardDto.IsPrimary.Value;
                }

                if (updateCardDto.IsActive.HasValue)
                    card.IsActive = updateCardDto.IsActive.Value;

                if (updateCardDto.Description != null)
                    card.Description = string.IsNullOrWhiteSpace(updateCardDto.Description) ? null : updateCardDto.Description;

                card.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var cardDto = await MapToCardDtoAsync(card);
                return ApiResponse<CardDto>.SuccessResult(cardDto, "Card updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CardDto>.ErrorResult($"Failed to update card: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCardAsync(string cardId, string userId)
        {
            try
            {
                var card = await _context.Cards
                    .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && !c.IsDeleted);

                if (card == null)
                {
                    return ApiResponse<bool>.ErrorResult("Card not found");
                }

                // Soft delete
                card.IsDeleted = true;
                card.DeletedAt = DateTime.UtcNow;
                card.DeletedBy = userId;
                card.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Card deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete card: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<CardDto>>> GetBankAccountCardsAsync(string bankAccountId, string userId)
        {
            try
            {
                // Verify bank account belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<List<CardDto>>.ErrorResult("Bank account not found or you don't have access to it");
                }

                var cards = await _context.Cards
                    .Include(c => c.BankAccount)
                    .Where(c => c.BankAccountId == bankAccountId && 
                               c.UserId == userId && 
                               !c.IsDeleted)
                    .OrderByDescending(c => c.IsPrimary)
                    .ThenBy(c => c.CardName)
                    .ToListAsync();

                var cardDtos = new List<CardDto>();
                foreach (var card in cards)
                {
                    cardDtos.Add(await MapToCardDtoAsync(card));
                }

                return ApiResponse<List<CardDto>>.SuccessResult(cardDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CardDto>>.ErrorResult($"Failed to get bank account cards: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<CardDto>>> GetUserCardsAsync(string userId, bool includeInactive = false)
        {
            try
            {
                var query = _context.Cards
                    .Include(c => c.BankAccount)
                    .Where(c => c.UserId == userId && !c.IsDeleted);

                if (!includeInactive)
                {
                    query = query.Where(c => c.IsActive);
                }

                var cards = await query
                    .OrderByDescending(c => c.IsPrimary)
                    .ThenBy(c => c.CardName)
                    .ToListAsync();

                var cardDtos = new List<CardDto>();
                foreach (var card in cards)
                {
                    cardDtos.Add(await MapToCardDtoAsync(card));
                }

                return ApiResponse<List<CardDto>>.SuccessResult(cardDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CardDto>>.ErrorResult($"Failed to get user cards: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CardDto>> GetPrimaryCardAsync(string bankAccountId, string userId)
        {
            try
            {
                // Verify bank account belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("Bank account not found or you don't have access to it");
                }

                var card = await _context.Cards
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(c => c.BankAccountId == bankAccountId && 
                                             c.UserId == userId && 
                                             c.IsPrimary && 
                                             c.IsActive && 
                                             !c.IsDeleted);

                if (card == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("No primary card found for this bank account");
                }

                var cardDto = await MapToCardDtoAsync(card);
                return ApiResponse<CardDto>.SuccessResult(cardDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CardDto>.ErrorResult($"Failed to get primary card: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CardDto>> SetPrimaryCardAsync(string cardId, string userId)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && !c.IsDeleted);

                if (card == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("Card not found");
                }

                // Unset other primary cards for this account
                var existingPrimaryCards = await _context.Cards
                    .Where(c => c.BankAccountId == card.BankAccountId && 
                               c.Id != cardId &&
                               c.IsPrimary && 
                               !c.IsDeleted)
                    .ToListAsync();

                foreach (var primaryCard in existingPrimaryCards)
                {
                    primaryCard.IsPrimary = false;
                    primaryCard.UpdatedAt = DateTime.UtcNow;
                }

                card.IsPrimary = true;
                card.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var cardDto = await MapToCardDtoAsync(card);
                return ApiResponse<CardDto>.SuccessResult(cardDto, "Card set as primary successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CardDto>.ErrorResult($"Failed to set primary card: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CardDto>> ActivateCardAsync(string cardId, string userId)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && !c.IsDeleted);

                if (card == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("Card not found");
                }

                card.IsActive = true;
                card.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var cardDto = await MapToCardDtoAsync(card);
                return ApiResponse<CardDto>.SuccessResult(cardDto, "Card activated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CardDto>.ErrorResult($"Failed to activate card: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CardDto>> DeactivateCardAsync(string cardId, string userId)
        {
            try
            {
                var card = await _context.Cards
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && !c.IsDeleted);

                if (card == null)
                {
                    return ApiResponse<CardDto>.ErrorResult("Card not found");
                }

                card.IsActive = false;
                // If deactivating primary card, unset it as primary
                if (card.IsPrimary)
                {
                    card.IsPrimary = false;
                }
                card.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var cardDto = await MapToCardDtoAsync(card);
                return ApiResponse<CardDto>.SuccessResult(cardDto, "Card deactivated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CardDto>.ErrorResult($"Failed to deactivate card: {ex.Message}");
            }
        }

        // Helper Methods
        private async Task<CardDto> MapToCardDtoAsync(Card card)
        {
            return new CardDto
            {
                Id = card.Id,
                BankAccountId = card.BankAccountId,
                UserId = card.UserId,
                CardName = card.CardName,
                CardType = card.CardType,
                CardBrand = card.CardBrand,
                Last4Digits = card.Last4Digits,
                CardholderName = card.CardholderName,
                ExpiryMonth = card.ExpiryMonth,
                ExpiryYear = card.ExpiryYear,
                IsPrimary = card.IsPrimary,
                IsActive = card.IsActive,
                Description = card.Description,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt,
                AccountName = card.BankAccount?.AccountName ?? string.Empty
            };
        }
    }
}

