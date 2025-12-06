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
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        private readonly ILogger<ReceiptsController> _logger;
        private readonly ISubscriptionService _subscriptionService;

        public ReceiptsController(IReceiptService receiptService, ILogger<ReceiptsController> logger, ISubscriptionService subscriptionService)
        {
            _receiptService = receiptService;
            _logger = logger;
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Upload a receipt (image or PDF)
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<ReceiptDto>>> UploadReceipt(IFormFile file)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceiptDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Receipt OCR feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "RECEIPT_OCR");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<ReceiptDto>.ErrorResult(
                        "Receipt OCR is a Premium feature. Please upgrade to Premium to access this feature."));
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<ReceiptDto>.ErrorResult("No file provided"));
                }

                using var stream = file.OpenReadStream();
                var result = await _receiptService.UploadReceiptAsync(userId, stream, file.FileName, file.ContentType);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading receipt");
                return BadRequest(ApiResponse<ReceiptDto>.ErrorResult($"Failed to upload receipt: {ex.Message}"));
            }
        }

        /// <summary>
        /// Process OCR for a receipt
        /// </summary>
        [HttpPost("{receiptId}/process-ocr")]
        public async Task<ActionResult<ApiResponse<ReceiptDto>>> ProcessOcr(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceiptDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Receipt OCR feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "RECEIPT_OCR");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<ReceiptDto>.ErrorResult(
                        "Receipt OCR is a Premium feature. Please upgrade to Premium to access this feature."));
                }

                var result = await _receiptService.ProcessReceiptOcrAsync(receiptId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR for receipt {ReceiptId}", receiptId);
                return BadRequest(ApiResponse<ReceiptDto>.ErrorResult($"Failed to process OCR: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a receipt by ID
        /// </summary>
        [HttpGet("{receiptId}")]
        public async Task<ActionResult<ApiResponse<ReceiptDto>>> GetReceipt(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceiptDto>.ErrorResult("User not authenticated"));
                }

                var result = await _receiptService.GetReceiptAsync(receiptId, userId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt {ReceiptId}", receiptId);
                return BadRequest(ApiResponse<ReceiptDto>.ErrorResult($"Failed to get receipt: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all receipts with optional search filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ReceiptDto>>>> GetReceipts(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? merchant = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] bool? isOcrProcessed = null,
            [FromQuery] string? searchText = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ReceiptDto>>.ErrorResult("User not authenticated"));
                }

                var searchDto = new ReceiptSearchDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Merchant = merchant,
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    IsOcrProcessed = isOcrProcessed,
                    SearchText = searchText,
                    Page = page,
                    Limit = limit
                };

                var result = await _receiptService.GetReceiptsAsync(userId, searchDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipts");
                return BadRequest(ApiResponse<List<ReceiptDto>>.ErrorResult($"Failed to get receipts: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a receipt
        /// </summary>
        [HttpDelete("{receiptId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReceipt(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _receiptService.DeleteReceiptAsync(receiptId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting receipt {ReceiptId}", receiptId);
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete receipt: {ex.Message}"));
            }
        }

        /// <summary>
        /// Link a receipt to an expense
        /// </summary>
        [HttpPost("{receiptId}/link-expense/{expenseId}")]
        public async Task<ActionResult<ApiResponse<ReceiptDto>>> LinkToExpense(string receiptId, string expenseId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceiptDto>.ErrorResult("User not authenticated"));
                }

                var result = await _receiptService.LinkReceiptToExpenseAsync(receiptId, expenseId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking receipt {ReceiptId} to expense {ExpenseId}", receiptId, expenseId);
                return BadRequest(ApiResponse<ReceiptDto>.ErrorResult($"Failed to link receipt: {ex.Message}"));
            }
        }

        /// <summary>
        /// Find matching expenses for a receipt
        /// </summary>
        [HttpGet("{receiptId}/match-expenses")]
        public async Task<ActionResult<ApiResponse<List<ExpenseMatchDto>>>> FindMatchingExpenses(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ExpenseMatchDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _receiptService.FindMatchingExpensesAsync(receiptId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding matching expenses for receipt {ReceiptId}", receiptId);
                return BadRequest(ApiResponse<List<ExpenseMatchDto>>.ErrorResult($"Failed to find matches: {ex.Message}"));
            }
        }

        /// <summary>
        /// Download receipt file
        /// </summary>
        [HttpGet("{receiptId}/file")]
        public async Task<IActionResult> GetReceiptFile(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var stream = await _receiptService.GetReceiptFileAsync(receiptId, userId);
                
                // Get receipt to determine content type
                var receiptResult = await _receiptService.GetReceiptAsync(receiptId, userId);
                if (!receiptResult.Success || receiptResult.Data == null)
                {
                    return NotFound();
                }

                var contentType = receiptResult.Data.FileType;
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = "application/octet-stream";
                }

                return File(stream, contentType, receiptResult.Data.OriginalFileName ?? receiptResult.Data.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt file for receipt {ReceiptId}", receiptId);
                return BadRequest();
            }
        }

        /// <summary>
        /// Get receipt thumbnail
        /// </summary>
        [HttpGet("{receiptId}/thumbnail")]
        public async Task<IActionResult> GetThumbnail(string receiptId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var stream = await _receiptService.GetReceiptThumbnailAsync(receiptId, userId);
                return File(stream, "image/jpeg");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting thumbnail for receipt {ReceiptId}", receiptId);
                return BadRequest();
            }
        }
    }
}

