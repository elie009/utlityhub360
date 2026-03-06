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

        [HttpPost("statements/import")]
        public async Task<ActionResult<ApiResponse<BankStatementDto>>> ImportBankStatement([FromBody] ImportBankStatementDto importDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<BankStatementDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.ImportBankStatementAsync(importDto, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementDto>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/{statementId}")]
        public async Task<ActionResult<ApiResponse<BankStatementDto>>> GetBankStatement(string statementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<BankStatementDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetBankStatementAsync(statementId, userId);
                if (!result.Success) return NotFound(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementDto>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/account/{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<List<BankStatementDto>>>> GetBankStatements(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<List<BankStatementDto>>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetBankStatementsAsync(bankAccountId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankStatementDto>>.ErrorResult(ex.Message));
            }
        }

        [HttpDelete("statements/{statementId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBankStatement(string statementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<bool>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.DeleteBankStatementAsync(statementId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        // ==================== ASYNC BANK STATEMENT UPLOAD ENDPOINTS ====================

        [HttpPost("statements/upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<BankStatementUploadDto>>> UploadBankStatement(IFormFile file, [FromForm] string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<BankStatementUploadDto>.ErrorResult("Not authenticated"));

                if (file == null || file.Length == 0) return BadRequest(ApiResponse<BankStatementUploadDto>.ErrorResult("No file"));
                if (string.IsNullOrEmpty(bankAccountId)) return BadRequest(ApiResponse<BankStatementUploadDto>.ErrorResult("No account ID"));

                var result = await _reconciliationService.UploadBankStatementAsync(file, bankAccountId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementUploadDto>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/uploads/pending")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BankStatementUploadDto>>>> GetPendingUploads([FromQuery] int limit = 10)
        {
            try
            {
                var result = await _reconciliationService.GetPendingUploadsAsync(limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankStatementUploadDto>>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/uploads/{uploadId}/file")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUploadFile(string uploadId)
        {
            try
            {
                var result = await _reconciliationService.GetUploadFileAsync(uploadId);
                if (!result.Success) return BadRequest(result);

                var (stream, fileName, contentType) = result.Data;
                return File(stream, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("statements/process-extracted-text")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ProcessExtractedText([FromBody] ProcessExtractedTextDto processDto)
        {
            try
            {
                var result = await _reconciliationService.ProcessExtractedTextAsync(processDto);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/uploads/{uploadId}/staging")]
        public async Task<ActionResult<ApiResponse<List<StagingTransactionDto>>>> GetStagingTransactions(string uploadId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<List<StagingTransactionDto>>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetStagingTransactionsAsync(uploadId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<StagingTransactionDto>>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("statements/uploads/{uploadId}/save-staging")]
        public async Task<ActionResult<ApiResponse<bool>>> SaveStagingTransactions(string uploadId, [FromBody] ConfirmBankStatementUploadDto saveDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<bool>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.SaveStagingTransactionsAsync(uploadId, saveDto, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("statements/uploads/{uploadId}/confirm")]
        public async Task<ActionResult<ApiResponse<BankStatementDto>>> ConfirmUpload(string uploadId, [FromBody] ConfirmBankStatementUploadDto confirmDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<BankStatementDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.ConfirmUploadAsync(uploadId, confirmDto, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementDto>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/uploads/account/{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<List<BankStatementUploadDto>>>> GetUserUploads(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<List<BankStatementUploadDto>>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetUserUploadsAsync(bankAccountId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BankStatementUploadDto>>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/uploads/{uploadId}/status")]
        public async Task<ActionResult<ApiResponse<BankStatementUploadDto>>> GetUploadStatus(string uploadId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<BankStatementUploadDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetUploadStatusAsync(uploadId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementUploadDto>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("statements/uploads/{uploadId}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelUpload(string uploadId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<bool>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.CancelUploadAsync(uploadId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("statements/uploads/{uploadId}/error")]
        [AllowAnonymous]  // Allow utility to report errors without authentication
        public async Task<ActionResult<ApiResponse<bool>>> ReportUploadError(string uploadId, [FromBody] UpdateUploadErrorDto errorDto)
        {
            try
            {
                var result = await _reconciliationService.UpdateUploadErrorAsync(uploadId, errorDto?.ErrorMessage ?? "Processing failed");
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("statements/upload-limit")]
        public async Task<ActionResult<ApiResponse<BankStatementUploadLimitDto>>> GetUploadLimit()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<BankStatementUploadLimitDto>.ErrorResult("Not authenticated"));

                var result = await _subscriptionService.CheckBankStatementUploadLimitAsync(userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BankStatementUploadLimitDto>.ErrorResult(ex.Message));
            }
        }

        // ==================== RECONCILIATION ENDPOINTS ====================

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> CreateReconciliation([FromBody] CreateReconciliationDto createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.CreateReconciliationAsync(createDto, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("{reconciliationId}")]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> GetReconciliation(string reconciliationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetReconciliationAsync(reconciliationId, userId);
                if (!result.Success) return NotFound(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("account/{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<List<ReconciliationDto>>>> GetReconciliations(string bankAccountId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<List<ReconciliationDto>>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetReconciliationsAsync(bankAccountId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ReconciliationDto>>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("{reconciliationId}/auto-match")]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> AutoMatchTransactions(string reconciliationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.AutoMatchTransactionsAsync(reconciliationId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("match")]
        public async Task<ActionResult<ApiResponse<ReconciliationMatchDto>>> MatchTransaction([FromBody] MatchTransactionDto matchDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<ReconciliationMatchDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.MatchTransactionAsync(matchDto, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationMatchDto>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("unmatch")]
        public async Task<ActionResult<ApiResponse<bool>>> UnmatchTransaction([FromBody] UnmatchTransactionDto unmatchDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<bool>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.UnmatchTransactionAsync(unmatchDto, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("complete")]
        public async Task<ActionResult<ApiResponse<ReconciliationDto>>> CompleteReconciliation([FromBody] CompleteReconciliationDto completeDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<ReconciliationDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.CompleteReconciliationAsync(completeDto, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationDto>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("{reconciliationId}/suggestions")]
        public async Task<ActionResult<ApiResponse<List<TransactionMatchSuggestionDto>>>> GetMatchSuggestions(string reconciliationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetMatchSuggestionsAsync(reconciliationId, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("summary/{bankAccountId}")]
        public async Task<ActionResult<ApiResponse<ReconciliationSummaryDto>>> GetReconciliationSummary(string bankAccountId, [FromQuery] DateTime? reconciliationDate)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiResponse<ReconciliationSummaryDto>.ErrorResult("Not authenticated"));

                var result = await _reconciliationService.GetReconciliationSummaryAsync(bankAccountId, reconciliationDate, userId);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReconciliationSummaryDto>.ErrorResult(ex.Message));
            }
        }
    }
}
