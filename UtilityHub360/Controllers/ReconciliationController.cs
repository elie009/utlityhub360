using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReconciliationController : ControllerBase
    {
        private readonly IReconciliationService _reconciliationService;
        private readonly ISubscriptionService _subscriptionService;

        public ReconciliationController(IReconciliationService reconciliationService, ISubscriptionService subscriptionService)
        {
            _reconciliationService = reconciliationService;
            _subscriptionService = subscriptionService;
        }

        // ==================== BANK STATEMENT ENDPOINTS ====================

        /// <summary>
        /// Extract transactions from uploaded bank statement file (PDF/CSV) using AI
        /// </summary>
        [HttpPost("statements/extract")]
        public async Task<ActionResult<ApiResponse<ExtractBankStatementResponseDto>>> ExtractBankStatement(
            IFormFile file, 
            [FromForm] string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("User not authenticated"));
                }

                // Check bank statement upload limit (applies to all tiers including Free)
                var uploadLimitCheck = await _subscriptionService.CheckBankStatementUploadLimitAsync(userId);
                if (!uploadLimitCheck.Success || !uploadLimitCheck.Data.CanUpload)
                {
                    var limitInfo = uploadLimitCheck.Data;
                    var message = limitInfo.UploadLimit.HasValue
                        ? $"Monthly upload limit reached. You have used {limitInfo.CurrentUploads} of {limitInfo.UploadLimit} allowed uploads this month. Please upgrade to Premium for unlimited uploads."
                        : "Unable to upload bank statement at this time.";
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult(message));
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("No file provided"));
                }

                if (string.IsNullOrEmpty(bankAccountId))
                {
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Bank account ID is required"));
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension != ".csv" && fileExtension != ".pdf")
                {
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Only CSV and PDF files are supported"));
                }

                using var stream = file.OpenReadStream();
                
                // Use ReconciliationService for extraction (which uses AI for PDFs)
                var result = await _reconciliationService.ExtractBankStatementFromFileAsync(stream, file.FileName, bankAccountId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Failed to extract bank statement: {ex.Message}"));
            }
        }

        /// <summary>
        /// Analyze PDF bank statement using AI Agent (direct AI analysis)
        /// </summary>
        [HttpPost("statements/analyze-pdf")]
        public async Task<ActionResult<ApiResponse<ExtractBankStatementResponseDto>>> AnalyzePDFWithAI(
            IFormFile file, 
            [FromForm] string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("User not authenticated"));
                }

                // Check bank statement upload limit (applies to all tiers including Free)
                var uploadLimitCheck = await _subscriptionService.CheckBankStatementUploadLimitAsync(userId);
                if (!uploadLimitCheck.Success || !uploadLimitCheck.Data.CanUpload)
                {
                    var limitInfo = uploadLimitCheck.Data;
                    var message = limitInfo.UploadLimit.HasValue
                        ? $"Monthly upload limit reached. You have used {limitInfo.CurrentUploads} of {limitInfo.UploadLimit} allowed uploads this month. Please upgrade to Premium for unlimited uploads."
                        : "Unable to upload bank statement at this time.";
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult(message));
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("No file provided"));
                }

                if (string.IsNullOrEmpty(bankAccountId))
                {
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Bank account ID is required"));
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension != ".pdf")
                {
                    return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Only PDF files are supported for AI analysis"));
                }

                using var stream = file.OpenReadStream();
                var result = await _reconciliationService.AnalyzePDFWithAIAsync(stream, file.FileName, bankAccountId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Failed to analyze PDF with AI: {ex.Message}"));
            }
        }

        /// <summary>
        /// Import a bank statement
        /// </summary>
        [HttpPost("statements/import")]
        public async Task<ActionResult<ApiResponse<BankStatementDto>>> ImportBankStatement([FromBody] ImportBankStatementDto importDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankStatementDto>.ErrorResult("User not authenticated"));
                }

                // Check bank statement upload limit before importing
                var uploadLimitCheck = await _subscriptionService.CheckBankStatementUploadLimitAsync(userId);
                if (!uploadLimitCheck.Success || !uploadLimitCheck.Data.CanUpload)
                {
                    var limitInfo = uploadLimitCheck.Data;
                    var message = limitInfo.UploadLimit.HasValue
                        ? $"Monthly upload limit reached. You have used {limitInfo.CurrentUploads} of {limitInfo.UploadLimit} allowed uploads this month. Please upgrade to Premium for unlimited uploads."
                        : "Unable to upload bank statement at this time.";
                    return BadRequest(ApiResponse<BankStatementDto>.ErrorResult(message));
                }

                var result = await _reconciliationService.ImportBankStatementAsync(importDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementDto>.ErrorResult($"Failed to import bank statement: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific bank statement
        /// </summary>
        [HttpGet("statements/{statementId}")]
        public async Task<ActionResult<ApiResponse<BankStatementDto>>> GetBankStatement(string statementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankStatementDto>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.GetBankStatementAsync(statementId, userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementDto>.ErrorResult($"Failed to get bank statement: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all bank statements for an account
        /// </summary>
        [HttpGet("statements/account/{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<List<BankStatementDto>>>> GetBankStatements(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<BankStatementDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.GetBankStatementsAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankStatementDto>>.ErrorResult($"Failed to get bank statements: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a bank statement
        /// </summary>
        [HttpDelete("statements/{statementId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBankStatement(string statementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.DeleteBankStatementAsync(statementId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete bank statement: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get bank statement upload limit information for the current user
        /// </summary>
        [HttpGet("statements/upload-limit")]
        public async Task<ActionResult<ApiResponse<BankStatementUploadLimitDto>>> GetUploadLimit()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<BankStatementUploadLimitDto>.ErrorResult("User not authenticated"));
                }

                var result = await _subscriptionService.CheckBankStatementUploadLimitAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementUploadLimitDto>.ErrorResult($"Failed to get upload limit: {ex.Message}"));
            }
        }

        // ==================== RECONCILIATION ENDPOINTS ====================

        /// <summary>
        /// Create a new reconciliation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> CreateReconciliation([FromBody] CreateReconciliationDto createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.CreateReconciliationAsync(createDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult($"Failed to create reconciliation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific reconciliation
        /// </summary>
        [HttpGet("{reconciliationId}")]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> GetReconciliation(string reconciliationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.GetReconciliationAsync(reconciliationId, userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult($"Failed to get reconciliation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all reconciliations for an account
        /// </summary>
        [HttpGet("account/{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<List<ReconciliationDto>>>> GetReconciliations(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ReconciliationDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.GetReconciliationsAsync(bankAccountId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ReconciliationDto>>.ErrorResult($"Failed to get reconciliations: {ex.Message}"));
            }
        }

        /// <summary>
        /// Auto-match transactions for a reconciliation
        /// </summary>
        [HttpPost("{reconciliationId}/auto-match")]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> AutoMatchTransactions(string reconciliationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.AutoMatchTransactionsAsync(reconciliationId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult($"Failed to auto-match transactions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Manually match a transaction
        /// </summary>
        [HttpPost("match")]
        public async Task<ActionResult<ApiResponse<ReconciliationMatchDto>>> MatchTransaction([FromBody] MatchTransactionDto matchDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReconciliationMatchDto>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.MatchTransactionAsync(matchDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationMatchDto>.ErrorResult($"Failed to match transaction: {ex.Message}"));
            }
        }

        /// <summary>
        /// Unmatch a transaction
        /// </summary>
        [HttpPost("unmatch")]
        public async Task<ActionResult<ApiResponse<bool>>> UnmatchTransaction([FromBody] UnmatchTransactionDto unmatchDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.UnmatchTransactionAsync(unmatchDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to unmatch transaction: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete a reconciliation
        /// </summary>
        [HttpPost("complete")]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> CompleteReconciliation([FromBody] CompleteReconciliationDto completeDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.CompleteReconciliationAsync(completeDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult($"Failed to complete reconciliation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get match suggestions for a reconciliation
        /// </summary>
        [HttpGet("{reconciliationId}/suggestions")]
        public async Task<ActionResult<ApiResponse<List<TransactionMatchSuggestionDto>>>> GetMatchSuggestions(string reconciliationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.GetMatchSuggestionsAsync(reconciliationId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult($"Failed to get match suggestions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get reconciliation summary for an account
        /// </summary>
        [HttpGet("summary/{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<ReconciliationSummaryDto>>> GetReconciliationSummary(string bankAccountId, [FromQuery] DateTime? reconciliationDate)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReconciliationSummaryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _reconciliationService.GetReconciliationSummaryAsync(bankAccountId, reconciliationDate, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationSummaryDto>.ErrorResult($"Failed to get reconciliation summary: {ex.Message}"));
            }
        }
    }
}

