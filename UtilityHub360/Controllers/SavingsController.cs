using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SavingsController : ControllerBase
    {
        private readonly ISavingsService _savingsService;
        private readonly IPaymentService _paymentService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ApplicationDbContext _context;

        public SavingsController(ISavingsService savingsService, IPaymentService paymentService, ISubscriptionService subscriptionService, ApplicationDbContext context)
        {
            _savingsService = savingsService;
            _paymentService = paymentService;
            _subscriptionService = subscriptionService;
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        // Savings Account Management
        [HttpPost("accounts")]
        public async Task<ActionResult<ApiResponse<SavingsAccountDto>>> CreateSavingsAccount([FromBody] CreateSavingsAccountDto savingsAccountDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Check subscription limit for savings goals
            var activeSavingsGoalsCount = await _context.SavingsAccounts
                .CountAsync(sa => sa.UserId == userId && sa.IsActive);
            
            var limitCheck = await _subscriptionService.CheckLimitAsync(userId, "SAVINGS_GOALS", activeSavingsGoalsCount);
            if (!limitCheck.Success || !limitCheck.Data)
            {
                return BadRequest(new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = "You have reached your savings goals limit. Please upgrade to Premium for unlimited savings goals."
                });
            }

            var result = await _savingsService.CreateSavingsAccountAsync(savingsAccountDto, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<ApiResponse<List<SavingsAccountDto>>>> GetUserSavingsAccounts()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<List<SavingsAccountDto>>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetUserSavingsAccountsAsync(userId);
            return Ok(result);
        }

        [HttpGet("accounts/{savingsAccountId}")]
        public async Task<ActionResult<ApiResponse<SavingsAccountDto>>> GetSavingsAccount(string savingsAccountId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetSavingsAccountByIdAsync(savingsAccountId, userId);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("accounts/{savingsAccountId}")]
        public async Task<ActionResult<ApiResponse<SavingsAccountDto>>> UpdateSavingsAccount(string savingsAccountId, [FromBody] CreateSavingsAccountDto updateDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.UpdateSavingsAccountAsync(savingsAccountId, updateDto, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("accounts/{savingsAccountId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSavingsAccount(string savingsAccountId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.DeleteSavingsAccountAsync(savingsAccountId, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // Savings Transactions
        [HttpPost("transactions")]
        public async Task<ActionResult<ApiResponse<SavingsTransactionDto>>> CreateSavingsTransaction([FromBody] CreateSavingsTransactionDto transactionDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsTransactionDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _paymentService.CreateSavingsTransactionAsync(transactionDto, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("accounts/{savingsAccountId}/transactions")]
        public async Task<ActionResult<ApiResponse<List<SavingsTransactionDto>>>> GetSavingsTransactions(
            string savingsAccountId, 
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<List<SavingsTransactionDto>>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetSavingsTransactionsAsync(savingsAccountId, userId, page, limit);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("transactions/{transactionId}")]
        public async Task<ActionResult<ApiResponse<SavingsTransactionDto>>> GetSavingsTransaction(string transactionId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsTransactionDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetSavingsTransactionByIdAsync(transactionId, userId);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        // Savings Analytics and Summary
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<SavingsSummaryDto>>> GetSavingsSummary()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsSummaryDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetSavingsSummaryAsync(userId);
            return Ok(result);
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<ApiResponse<SavingsAnalyticsDto>>> GetSavingsAnalytics([FromQuery] string period = "month")
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsAnalyticsDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetSavingsAnalyticsAsync(userId, period);
            return Ok(result);
        }

        [HttpGet("by-type")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetSavingsByType()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<Dictionary<string, decimal>>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetSavingsByTypeAsync(userId);
            return Ok(result);
        }

        // Savings Goals and Progress
        [HttpPut("accounts/{savingsAccountId}/goal")]
        public async Task<ActionResult<ApiResponse<SavingsAccountDto>>> UpdateSavingsGoal(
            string savingsAccountId, 
            [FromBody] UpdateSavingsGoalDto goalDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.UpdateSavingsGoalAsync(savingsAccountId, goalDto.TargetAmount, goalDto.TargetDate, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("goals/{savingsType}")]
        public async Task<ActionResult<ApiResponse<List<SavingsAccountDto>>>> GetSavingsGoalsByType(string savingsType)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<List<SavingsAccountDto>>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetSavingsGoalsByTypeAsync(savingsType, userId);
            return Ok(result);
        }

        [HttpGet("progress")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalSavingsProgress()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<decimal>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetTotalSavingsProgressAsync(userId);
            return Ok(result);
        }

        [HttpPut("accounts/{savingsAccountId}/mark-paid")]
        public async Task<ActionResult<ApiResponse<SavingsAccountDto>>> MarkSavingsAsPaid(
            string savingsAccountId, 
            [FromBody] MarkSavingsPaidDto? request = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var amount = request?.Amount ?? 0;
            var notes = request?.Notes;
            
            if (amount <= 0)
            {
                return BadRequest(new ApiResponse<SavingsAccountDto>
                {
                    Success = false,
                    Message = "Amount must be greater than 0"
                });
            }
            
            var result = await _savingsService.MarkSavingsAsPaidAsync(savingsAccountId, userId, amount, notes);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // Bank Account Integration
        [HttpPost("transfer/bank-to-savings")]
        public async Task<ActionResult<ApiResponse<bool>>> TransferFromBankToSavings([FromBody] TransferToSavingsDto transferDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.TransferFromBankToSavingsAsync(
                transferDto.BankAccountId, 
                transferDto.SavingsAccountId, 
                transferDto.Amount, 
                transferDto.Description, 
                userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("transfer/savings-to-bank")]
        public async Task<ActionResult<ApiResponse<bool>>> TransferFromSavingsToBank([FromBody] TransferToBankDto transferDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.TransferFromSavingsToBankAsync(
                transferDto.SavingsAccountId, 
                transferDto.BankAccountId, 
                transferDto.Amount, 
                transferDto.Description, 
                userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // Auto-Save Features (Placeholder endpoints)
        [HttpPost("auto-save")]
        public async Task<ActionResult<ApiResponse<bool>>> CreateAutoSave([FromBody] AutoSaveSettingsDto autoSaveDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.CreateAutoSaveAsync(autoSaveDto, userId);
            return Ok(result);
        }

        [HttpGet("auto-save")]
        public async Task<ActionResult<ApiResponse<List<AutoSaveSettingsDto>>>> GetAutoSaveSettings()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<List<AutoSaveSettingsDto>>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.GetAutoSaveSettingsAsync(userId);
            return Ok(result);
        }

        [HttpPost("auto-save/execute")]
        public async Task<ActionResult<ApiResponse<bool>>> ExecuteAutoSave()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _savingsService.ExecuteAutoSaveAsync(userId);
            return Ok(result);
        }
    }

    // Additional DTOs for specific endpoints
    public class UpdateSavingsGoalDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public decimal TargetAmount { get; set; }

        public DateTime? TargetDate { get; set; }
    }

    public class TransferToSavingsDto
    {
        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string SavingsAccountId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    public class TransferToBankDto
    {
        [Required]
        [StringLength(450)]
        public string SavingsAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    public class MarkSavingsPaidDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        public string? Notes { get; set; }
    }
}
