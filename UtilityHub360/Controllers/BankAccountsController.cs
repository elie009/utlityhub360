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
        private readonly IAIAgentService _aiAgentService;

        public BankAccountsController(IBankAccountService bankAccountService, IAIAgentService aiAgentService)
        {
            _bankAccountService = bankAccountService;
            _aiAgentService = aiAgentService;
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
        public async Task<ActionResult<ApiResponse<BankAccountSummaryDto>>> GetBankAccountSummary([FromQuery] string frequency = "monthly")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankAccountSummaryDto>.ErrorResult("User not authenticated"));
                }

                // Validate frequency parameter
                var validFrequencies = new[] { "weekly", "monthly", "quarterly", "yearly" };
                if (!validFrequencies.Contains(frequency.ToLower()))
                {
                    return BadRequest(ApiResponse<BankAccountSummaryDto>.ErrorResult("Invalid frequency. Must be one of: weekly, monthly, quarterly, yearly"));
                }

                var result = await _bankAccountService.GetBankAccountSummaryAsync(userId, frequency.ToLower());
                
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
        /// Get total balance across all accounts (excluding credit cards)
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
        /// Get total debt from credit card accounts
        /// </summary>
        [HttpGet("total-debt")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalDebt()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetTotalDebtAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total debt: {ex.Message}"));
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
        public async Task<ActionResult<ApiResponse<List<BankTransactionDto>>>> GetUserTransactions([FromQuery] string? bankAccountId = null, [FromQuery] string? accountType = null, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankTransactionDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetUserTransactionsAsync(userId, bankAccountId, accountType, page, limit);
                
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
        /// Update a bank transaction
        /// </summary>
        [HttpPut("transactions/{transactionId}")]
        public async Task<ActionResult<ApiResponse<BankTransactionDto>>> UpdateTransaction(
            string transactionId,
            [FromBody] UpdateBankTransactionDto updateTransactionDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankTransactionDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.UpdateTransactionAsync(transactionId, updateTransactionDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult($"Failed to update transaction: {ex.Message}"));
            }
        }

        /// <summary>
        /// Hide (soft delete) a bank transaction
        /// </summary>
        [HttpPut("transactions/{transactionId}/hide")]
        public async Task<ActionResult<ApiResponse<bool>>> HideTransaction(
            string transactionId,
            [FromBody] HideTransactionDto? hideDto = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var reason = hideDto?.Reason;
                var result = await _bankAccountService.SoftDeleteTransactionAsync(transactionId, userId, reason);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to hide transaction: {ex.Message}"));
            }
        }

        /// <summary>
        /// Restore a hidden (soft-deleted) bank transaction
        /// </summary>
        [HttpPut("transactions/{transactionId}/restore")]
        public async Task<ActionResult<ApiResponse<BankTransactionDto>>> RestoreTransaction(string transactionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankTransactionDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.RestoreTransactionAsync(transactionId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult($"Failed to restore transaction: {ex.Message}"));
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
        /// Delete a bank transaction
        /// </summary>
        [HttpDelete("transactions/{transactionId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTransaction(string transactionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.DeleteTransactionAsync(transactionId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete transaction: {ex.Message}"));
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

        // Expense Management Endpoints

        /// <summary>
        /// Create a new expense transaction
        /// </summary>
        [HttpPost("expenses")]
        public async Task<ActionResult<ApiResponse<BankTransactionDto>>> CreateExpense([FromBody] CreateBankAccountExpenseDto expenseDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankTransactionDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _bankAccountService.CreateExpenseAsync(expenseDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult($"Failed to create expense: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get expense analytics
        /// </summary>
        [HttpGet("expenses/analytics")]
        public async Task<ActionResult<ApiResponse<ExpenseAnalyticsDto>>> GetExpenseAnalytics([FromQuery] string period = "month")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseAnalyticsDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetExpenseAnalyticsAsync(userId, period);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseAnalyticsDto>.ErrorResult($"Failed to get expense analytics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get expense summary
        /// </summary>
        [HttpGet("expenses/summary")]
        public async Task<ActionResult<ApiResponse<ExpenseSummaryDto>>> GetExpenseSummary()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExpenseSummaryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetExpenseSummaryAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseSummaryDto>.ErrorResult($"Failed to get expense summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get expenses by category
        /// </summary>
        [HttpGet("expenses/category/{category}")]
        public async Task<ActionResult<ApiResponse<List<BankTransactionDto>>>> GetExpensesByCategory(string category, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankTransactionDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetExpensesByCategoryAsync(userId, category, page, limit);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankTransactionDto>>.ErrorResult($"Failed to get expenses by category: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all expense categories with amounts
        /// </summary>
        [HttpGet("expenses/categories")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetExpenseCategories()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<Dictionary<string, decimal>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetExpenseCategoriesAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, decimal>>.ErrorResult($"Failed to get expense categories: {ex.Message}"));
            }
        }

        /// <summary>
        /// Analyze transaction text from SMS/notification and create bank transaction (AI Agent Enhanced)
        /// </summary>
        /// <remarks>
        /// This endpoint uses AI Agent to intelligently analyze transaction text and automatically create a bank transaction.
        /// It extracts: amount, currency, merchant, date/time, card info, and location.
        /// 
        /// Request body:
        /// - transactionText (required): The SMS/notification text to analyze
        /// - bankAccountId (optional): If provided, uses this bank account instead of matching by card number
        /// </remarks>
        [HttpPost("transactions/analyze-text")]
        public async Task<ActionResult<ApiResponse<BankTransactionDto>>> AnalyzeAndCreateTransaction([FromBody] AnalyzeTransactionTextDto analyzeDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankTransactionDto>.ErrorResult("User not authenticated"));
                }

                if (analyzeDto == null || string.IsNullOrWhiteSpace(analyzeDto.TransactionText))
                {
                    return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult("Transaction text is required."));
                }

                // Use AI Agent service for enhanced transaction analysis
                var result = await _aiAgentService.AnalyzeAndCreateTransactionWithAgentAsync(analyzeDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankTransactionDto>.ErrorResult($"Failed to analyze and create transaction: {ex.Message}"));
            }
        }

        /// <summary>
        /// Close a month for a bank account (prevents transaction modifications for that month)
        /// </summary>
        [HttpPost("{bankAccountId}/close-month")]
        public async Task<ActionResult<ApiResponse<ClosedMonthDto>>> CloseMonth(string bankAccountId, [FromBody] CloseMonthDto closeMonthDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ClosedMonthDto>.ErrorResult("User not authenticated"));
                }

                if (closeMonthDto == null)
                {
                    return BadRequest(ApiResponse<ClosedMonthDto>.ErrorResult("Close month data is required"));
                }

                var result = await _bankAccountService.CloseMonthAsync(bankAccountId, closeMonthDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ClosedMonthDto>.ErrorResult($"Failed to close month: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all closed months for a bank account
        /// </summary>
        [HttpGet("{bankAccountId}/closed-months")]
        public async Task<ActionResult<ApiResponse<List<ClosedMonthDto>>>> GetClosedMonths(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ClosedMonthDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _bankAccountService.GetClosedMonthsAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ClosedMonthDto>>.ErrorResult($"Failed to get closed months: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if a specific month is closed for a bank account
        /// </summary>
        [HttpGet("{bankAccountId}/is-month-closed")]
        public async Task<ActionResult<ApiResponse<bool>>> IsMonthClosed(string bankAccountId, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                if (month < 1 || month > 12)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Month must be between 1 and 12"));
                }

                var result = await _bankAccountService.IsMonthClosedAsync(bankAccountId, year, month, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to check month closure status: {ex.Message}"));
            }
        }
    }
}
