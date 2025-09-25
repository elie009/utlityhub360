using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BankAccountsController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountsController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Create a new bank account
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BankAccountDto>>> CreateBankAccount([FromBody] CreateBankAccountDto createBankAccountDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.CreateBankAccountAsync(createBankAccountDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountDto>.ErrorResult($"Failed to create bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific bank account by ID
        /// </summary>
        [HttpGet("{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<BankAccountDto>>> GetBankAccount(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetBankAccountAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountDto>.ErrorResult($"Failed to get bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all bank accounts for the authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<BankAccountDto>>>> GetUserBankAccounts([FromQuery] bool includeInactive = false)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankAccountDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetUserBankAccountsAsync(userId, includeInactive);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get bank accounts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update a bank account
        /// </summary>
        [HttpPut("{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<BankAccountDto>>> UpdateBankAccount(string bankAccountId, [FromBody] UpdateBankAccountDto updateBankAccountDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.UpdateBankAccountAsync(bankAccountId, updateBankAccountDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountDto>.ErrorResult($"Failed to update bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a bank account
        /// </summary>
        [HttpDelete("{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBankAccount(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.DeleteBankAccountAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get bank account summary with analytics
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<BankAccountSummaryDto>>> GetBankAccountSummary()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountSummaryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetBankAccountSummaryAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountSummaryDto>.ErrorResult($"Failed to get bank account summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get bank account analytics
        /// </summary>
        [HttpGet("analytics")]
        public async Task<ActionResult<ApiResponse<BankAccountAnalyticsDto>>> GetBankAccountAnalytics([FromQuery] string period = "month")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountAnalyticsDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetBankAccountAnalyticsAsync(userId, period);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountAnalyticsDto>.ErrorResult($"Failed to get bank account analytics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get total balance across all accounts
        /// </summary>
        [HttpGet("total-balance")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalBalance()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetTotalBalanceAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total balance: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get top accounts by balance
        /// </summary>
        [HttpGet("top-accounts")]
        public async Task<ActionResult<ApiResponse<List<BankAccountDto>>>> GetTopAccountsByBalance([FromQuery] int limit = 5)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankAccountDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetTopAccountsByBalanceAsync(userId, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get top accounts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Connect a bank account via API integration
        /// </summary>
        [HttpPost("connect")]
        public async Task<ActionResult<ApiResponse<BankAccountDto>>> ConnectBankAccount([FromBody] BankIntegrationDto integrationDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.ConnectBankAccountAsync(integrationDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountDto>.ErrorResult($"Failed to connect bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Sync bank account data
        /// </summary>
        [HttpPost("sync")]
        public async Task<ActionResult<ApiResponse<BankAccountDto>>> SyncBankAccount([FromBody] SyncBankAccountDto syncDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.SyncBankAccountAsync(syncDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountDto>.ErrorResult($"Failed to sync bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get connected bank accounts
        /// </summary>
        [HttpGet("connected")]
        public async Task<ActionResult<ApiResponse<List<BankAccountDto>>>> GetConnectedAccounts()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankAccountDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetConnectedAccountsAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get connected accounts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Disconnect a bank account
        /// </summary>
        [HttpPost("{bankAccountId}/disconnect")]
        public async Task<ActionResult<ApiResponse<bool>>> DisconnectBankAccount(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.DisconnectBankAccountAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to disconnect bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Archive a bank account
        /// </summary>
        [HttpPost("{bankAccountId}/archive")]
        public async Task<ActionResult<ApiResponse<BankAccountDto>>> ArchiveBankAccount(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.ArchiveBankAccountAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountDto>.ErrorResult($"Failed to archive bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Activate a bank account
        /// </summary>
        [HttpPost("{bankAccountId}/activate")]
        public async Task<ActionResult<ApiResponse<BankAccountDto>>> ActivateBankAccount(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.ActivateBankAccountAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountDto>.ErrorResult($"Failed to activate bank account: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update account balance
        /// </summary>
        [HttpPut("{bankAccountId}/balance")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAccountBalance(string bankAccountId, [FromBody] decimal newBalance)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.UpdateAccountBalanceAsync(bankAccountId, newBalance, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to update account balance: {ex.Message}"));
            }
        }

        // Transaction endpoints

        /// <summary>
        /// Create a new transaction
        /// </summary>
        [HttpPost("transactions")]
        public async Task<ActionResult<ApiResponse<BankTransactionDto>>> CreateTransaction([FromBody] CreateBankTransactionDto createTransactionDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankTransactionDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.CreateTransactionAsync(createTransactionDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult($"Failed to create transaction: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get transactions for a specific account
        /// </summary>
        [HttpGet("{bankAccountId}/transactions")]
        public async Task<ActionResult<ApiResponse<List<BankTransactionDto>>>> GetAccountTransactions(string bankAccountId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankTransactionDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetAccountTransactionsAsync(bankAccountId, userId, page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get account transactions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all transactions for the user
        /// </summary>
        [HttpGet("transactions")]
        public async Task<ActionResult<ApiResponse<List<BankTransactionDto>>>> GetUserTransactions([FromQuery] string? accountType = null, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankTransactionDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetUserTransactionsAsync(userId, accountType, page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get user transactions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific transaction
        /// </summary>
        [HttpGet("transactions/{transactionId}")]
        public async Task<ActionResult<ApiResponse<BankTransactionDto>>> GetTransaction(string transactionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankTransactionDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetTransactionAsync(transactionId, userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult($"Failed to get transaction: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get transaction analytics
        /// </summary>
        [HttpGet("transactions/analytics")]
        public async Task<ActionResult<ApiResponse<BankAccountAnalyticsDto>>> GetTransactionAnalytics([FromQuery] string period = "month")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountAnalyticsDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetTransactionAnalyticsAsync(userId, period);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankAccountAnalyticsDto>.ErrorResult($"Failed to get transaction analytics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get recent transactions
        /// </summary>
        [HttpGet("transactions/recent")]
        public async Task<ActionResult<ApiResponse<List<BankTransactionDto>>>> GetRecentTransactions([FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankTransactionDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetRecentTransactionsAsync(userId, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get recent transactions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get spending by category
        /// </summary>
        [HttpGet("transactions/spending-by-category")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetSpendingByCategory([FromQuery] string period = "month")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<Dictionary<string, decimal>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetSpendingByCategoryAsync(userId, period);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get spending by category: {ex.Message}"));
            }
        }

        // Admin endpoints

        /// <summary>
        /// Get all bank accounts (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<List<BankAccountDto>>>> GetAllBankAccounts([FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var result = await _bankAccountService.GetAllBankAccountsAsync(page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankAccountDto>>.ErrorResult($"Failed to get all bank accounts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all transactions (Admin only)
        /// </summary>
        [HttpGet("admin/transactions")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<List<BankTransactionDto>>>> GetAllTransactions([FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var result = await _bankAccountService.GetAllTransactionsAsync(page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get all transactions: {ex.Message}"));
            }
        }
    }
}
