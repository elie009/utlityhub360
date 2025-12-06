using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;
using UtilityHub360.Entities;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvestmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionService _subscriptionService;

        public InvestmentsController(ApplicationDbContext context, ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Get all investment accounts for the current user
        /// Enterprise feature only
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InvestmentDto>>>> GetInvestments()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<InvestmentDto>>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Investment Tracking feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "INVESTMENT_TRACKING");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<List<InvestmentDto>>.ErrorResult(
                        "Investment Tracking is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                var investments = await _context.Investments
                    .Where(i => i.UserId == userId && !i.IsDeleted)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                var investmentDtos = investments.Select(i => new InvestmentDto
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    AccountName = i.AccountName,
                    InvestmentType = i.InvestmentType,
                    AccountType = i.AccountType,
                    BrokerName = i.BrokerName,
                    AccountNumber = i.AccountNumber,
                    InitialInvestment = i.InitialInvestment,
                    CurrentValue = i.CurrentValue,
                    TotalCostBasis = i.TotalCostBasis,
                    UnrealizedGainLoss = i.UnrealizedGainLoss,
                    RealizedGainLoss = i.RealizedGainLoss,
                    TotalReturnPercentage = i.TotalReturnPercentage,
                    Currency = i.Currency,
                    Description = i.Description,
                    IsActive = i.IsActive,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                }).ToList();

                return Ok(ApiResponse<List<InvestmentDto>>.SuccessResult(investmentDtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<InvestmentDto>>.ErrorResult($"Failed to get investments: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific investment account
        /// Enterprise feature only
        /// </summary>
        [HttpGet("{investmentId}")]
        public async Task<ActionResult<ApiResponse<InvestmentDto>>> GetInvestment(string investmentId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<InvestmentDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Investment Tracking feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "INVESTMENT_TRACKING");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<InvestmentDto>.ErrorResult(
                        "Investment Tracking is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                var investment = await _context.Investments
                    .FirstOrDefaultAsync(i => i.Id == investmentId && i.UserId == userId && !i.IsDeleted);

                if (investment == null)
                {
                    return NotFound(ApiResponse<InvestmentDto>.ErrorResult("Investment not found"));
                }

                var investmentDto = new InvestmentDto
                {
                    Id = investment.Id,
                    UserId = investment.UserId,
                    AccountName = investment.AccountName,
                    InvestmentType = investment.InvestmentType,
                    AccountType = investment.AccountType,
                    BrokerName = investment.BrokerName,
                    AccountNumber = investment.AccountNumber,
                    InitialInvestment = investment.InitialInvestment,
                    CurrentValue = investment.CurrentValue,
                    TotalCostBasis = investment.TotalCostBasis,
                    UnrealizedGainLoss = investment.UnrealizedGainLoss,
                    RealizedGainLoss = investment.RealizedGainLoss,
                    TotalReturnPercentage = investment.TotalReturnPercentage,
                    Currency = investment.Currency,
                    Description = investment.Description,
                    IsActive = investment.IsActive,
                    CreatedAt = investment.CreatedAt,
                    UpdatedAt = investment.UpdatedAt
                };

                return Ok(ApiResponse<InvestmentDto>.SuccessResult(investmentDto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<InvestmentDto>.ErrorResult($"Failed to get investment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new investment account
        /// Enterprise feature only
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<InvestmentDto>>> CreateInvestment([FromBody] CreateInvestmentDto createDto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<InvestmentDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Investment Tracking feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "INVESTMENT_TRACKING");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<InvestmentDto>.ErrorResult(
                        "Investment Tracking is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                var investment = new Investment
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    AccountName = createDto.AccountName,
                    InvestmentType = createDto.InvestmentType,
                    AccountType = createDto.AccountType,
                    BrokerName = createDto.BrokerName,
                    AccountNumber = createDto.AccountNumber,
                    InitialInvestment = createDto.InitialInvestment,
                    CurrentValue = createDto.CurrentValue ?? createDto.InitialInvestment,
                    TotalCostBasis = createDto.InitialInvestment,
                    Currency = createDto.Currency ?? "USD",
                    Description = createDto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Investments.Add(investment);
                await _context.SaveChangesAsync();

                var investmentDto = new InvestmentDto
                {
                    Id = investment.Id,
                    UserId = investment.UserId,
                    AccountName = investment.AccountName,
                    InvestmentType = investment.InvestmentType,
                    AccountType = investment.AccountType,
                    BrokerName = investment.BrokerName,
                    AccountNumber = investment.AccountNumber,
                    InitialInvestment = investment.InitialInvestment,
                    CurrentValue = investment.CurrentValue,
                    TotalCostBasis = investment.TotalCostBasis,
                    Currency = investment.Currency,
                    Description = investment.Description,
                    IsActive = investment.IsActive,
                    CreatedAt = investment.CreatedAt,
                    UpdatedAt = investment.UpdatedAt
                };

                return Ok(ApiResponse<InvestmentDto>.SuccessResult(investmentDto, "Investment account created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<InvestmentDto>.ErrorResult($"Failed to create investment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an investment account
        /// Enterprise feature only
        /// </summary>
        [HttpPut("{investmentId}")]
        public async Task<ActionResult<ApiResponse<InvestmentDto>>> UpdateInvestment(string investmentId, [FromBody] UpdateInvestmentDto updateDto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<InvestmentDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Investment Tracking feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "INVESTMENT_TRACKING");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<InvestmentDto>.ErrorResult(
                        "Investment Tracking is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                var investment = await _context.Investments
                    .FirstOrDefaultAsync(i => i.Id == investmentId && i.UserId == userId && !i.IsDeleted);

                if (investment == null)
                {
                    return NotFound(ApiResponse<InvestmentDto>.ErrorResult("Investment not found"));
                }

                if (!string.IsNullOrEmpty(updateDto.AccountName))
                    investment.AccountName = updateDto.AccountName;
                if (!string.IsNullOrEmpty(updateDto.InvestmentType))
                    investment.InvestmentType = updateDto.InvestmentType;
                if (updateDto.AccountType != null)
                    investment.AccountType = updateDto.AccountType;
                if (updateDto.BrokerName != null)
                    investment.BrokerName = updateDto.BrokerName;
                if (updateDto.CurrentValue.HasValue)
                    investment.CurrentValue = updateDto.CurrentValue.Value;
                if (updateDto.Description != null)
                    investment.Description = updateDto.Description;
                if (updateDto.IsActive.HasValue)
                    investment.IsActive = updateDto.IsActive.Value;

                investment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var investmentDto = new InvestmentDto
                {
                    Id = investment.Id,
                    UserId = investment.UserId,
                    AccountName = investment.AccountName,
                    InvestmentType = investment.InvestmentType,
                    AccountType = investment.AccountType,
                    BrokerName = investment.BrokerName,
                    AccountNumber = investment.AccountNumber,
                    InitialInvestment = investment.InitialInvestment,
                    CurrentValue = investment.CurrentValue,
                    TotalCostBasis = investment.TotalCostBasis,
                    UnrealizedGainLoss = investment.UnrealizedGainLoss,
                    RealizedGainLoss = investment.RealizedGainLoss,
                    TotalReturnPercentage = investment.TotalReturnPercentage,
                    Currency = investment.Currency,
                    Description = investment.Description,
                    IsActive = investment.IsActive,
                    CreatedAt = investment.CreatedAt,
                    UpdatedAt = investment.UpdatedAt
                };

                return Ok(ApiResponse<InvestmentDto>.SuccessResult(investmentDto, "Investment updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<InvestmentDto>.ErrorResult($"Failed to update investment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete an investment account (soft delete)
        /// Enterprise feature only
        /// </summary>
        [HttpDelete("{investmentId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteInvestment(string investmentId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Investment Tracking feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "INVESTMENT_TRACKING");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult(
                        "Investment Tracking is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                var investment = await _context.Investments
                    .FirstOrDefaultAsync(i => i.Id == investmentId && i.UserId == userId && !i.IsDeleted);

                if (investment == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Investment not found"));
                }

                investment.IsDeleted = true;
                investment.DeletedAt = DateTime.UtcNow;
                investment.DeletedBy = userId;
                investment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Investment deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete investment: {ex.Message}"));
            }
        }
    }
}

