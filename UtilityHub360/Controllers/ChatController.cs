using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;
using UtilityHub360.Data;
using Microsoft.EntityFrameworkCore;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IReceiptService _receiptService;
        private readonly IBankAccountService _bankAccountService;
        private readonly ApplicationDbContext _context;

        public ChatController(
            IChatService chatService, 
            ILogger<ChatController> logger,
            IReceiptService receiptService,
            IBankAccountService bankAccountService,
            ApplicationDbContext context)
        {
            _chatService = chatService;
            _logger = logger;
            _receiptService = receiptService;
            _bankAccountService = bankAccountService;
            _context = context;
        }

        /// <summary>
        /// Send a message to the AI chat assistant
        /// </summary>
        [HttpPost("message")]
        public async Task<ActionResult<ApiResponse<ChatResponseDto>>> SendMessage([FromBody] ChatMessageDto messageDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ChatResponseDto>.ErrorResult("User not authenticated"));
                }

                if (string.IsNullOrWhiteSpace(messageDto.Message))
                {
                    return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult("Message cannot be empty"));
                }

                var result = await _chatService.SendMessageAsync(messageDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage endpoint");
                return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult($"Failed to send message: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all conversations for the current user
        /// </summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ChatConversationDto>>>> GetConversations(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaginatedResponse<ChatConversationDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _chatService.GetUserConversationsAsync(userId, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetConversations endpoint");
                return BadRequest(ApiResponse<PaginatedResponse<ChatConversationDto>>.ErrorResult($"Failed to get conversations: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get conversation history with messages
        /// </summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<ApiResponse<ChatConversationDto>>> GetConversationHistory(string conversationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ChatConversationDto>.ErrorResult("User not authenticated"));
                }

                var result = await _chatService.GetConversationHistoryAsync(conversationId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetConversationHistory endpoint for conversation {conversationId}");
                return BadRequest(ApiResponse<ChatConversationDto>.ErrorResult($"Failed to get conversation history: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new conversation
        /// </summary>
        [HttpPost("conversations")]
        public async Task<ActionResult<ApiResponse<ChatConversationDto>>> CreateConversation([FromBody] CreateChatConversationDto? createDto = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ChatConversationDto>.ErrorResult("User not authenticated"));
                }

                var title = createDto?.Title ?? "New Conversation";
                var result = await _chatService.CreateConversationAsync(userId, title);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateConversation endpoint");
                return BadRequest(ApiResponse<ChatConversationDto>.ErrorResult($"Failed to create conversation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a conversation
        /// </summary>
        [HttpDelete("conversations/{conversationId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteConversation(string conversationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _chatService.DeleteConversationAsync(conversationId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DeleteConversation endpoint for conversation {conversationId}");
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete conversation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Generate a financial report
        /// </summary>
        [HttpPost("generate-report")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateReport([FromBody] GenerateReportDto reportDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResult("User not authenticated"));
                }

                var result = await _chatService.GenerateReportAsync(userId, reportDto.ReportType, reportDto.Format);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateReport endpoint");
                return BadRequest(ApiResponse<string>.ErrorResult($"Failed to generate report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get bill reminders
        /// </summary>
        [HttpGet("bill-reminders")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetBillReminders()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<string>>.ErrorResult("User not authenticated"));
                }

                var result = await _chatService.GetBillRemindersAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBillReminders endpoint");
                return BadRequest(ApiResponse<List<string>>.ErrorResult($"Failed to get bill reminders: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get budget suggestions
        /// </summary>
        [HttpGet("budget-suggestions")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetBudgetSuggestions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<string>>.ErrorResult("User not authenticated"));
                }

                var result = await _chatService.GetBudgetSuggestionsAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBudgetSuggestions endpoint");
                return BadRequest(ApiResponse<List<string>>.ErrorResult($"Failed to get budget suggestions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get financial context for the current user
        /// </summary>
        [HttpGet("financial-context")]
        public async Task<ActionResult<ApiResponse<ChatContextDto>>> GetFinancialContext()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ChatContextDto>.ErrorResult("User not authenticated"));
                }

                var result = await _chatService.BuildFinancialContextAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFinancialContext endpoint");
                return BadRequest(ApiResponse<ChatContextDto>.ErrorResult($"Failed to get financial context: {ex.Message}"));
            }
        }

        /// <summary>
        /// Search documentation for specific topics
        /// </summary>
        [HttpGet("search-documentation")]
        public async Task<ActionResult<ApiResponse<string>>> SearchDocumentation([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("Query cannot be empty"));
                }

                var result = await _chatService.SearchDocumentationAsync(query);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchDocumentation endpoint");
                return BadRequest(ApiResponse<string>.ErrorResult($"Failed to search documentation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Upload a receipt image (JPG/PNG) for processing into a transaction
        /// </summary>
        [HttpPost("upload-receipt")]
        public async Task<ActionResult<ApiResponse<ChatResponseDto>>> UploadReceipt(
            [FromForm] IFormFile file,
            [FromForm] string? conversationId = null,
            [FromForm] string? bankAccountId = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ChatResponseDto>.ErrorResult("User not authenticated"));
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult("No file uploaded"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult(
                        "Invalid file type. Only JPG and PNG files are allowed."));
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult(
                        "File size exceeds 10MB limit."));
                }

                // Upload receipt using ReceiptService
                var receiptResult = await _receiptService.UploadReceiptAsync(
                    userId, 
                    file.OpenReadStream(), 
                    file.FileName, 
                    file.ContentType);
                
                if (!receiptResult.Success || receiptResult.Data == null)
                {
                    return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult(
                        $"Failed to upload receipt: {receiptResult.Message}"));
                }

                // Process OCR if receipt was uploaded successfully
                var receiptId = receiptResult.Data.Id;
                var ocrResult = await _receiptService.ProcessReceiptOcrAsync(receiptId, userId);
                
                // Get or create conversation
                var conversation = await GetOrCreateConversationAsync(userId, conversationId);
                
                // Build AI response about the receipt
                var receiptData = ocrResult.Success && ocrResult.Data != null ? ocrResult.Data : receiptResult.Data;
                var aiResponseMessage = BuildReceiptResponseMessage(receiptData);
                
                // Create chat message about the receipt
                var messageDto = new ChatMessageDto
                {
                    Message = $"I've uploaded a receipt: {file.FileName}. Please process it and create a transaction.",
                    ConversationId = conversation.Id,
                    IncludeTransactionContext = true
                };
                
                // Send message to chat service to get AI response
                var chatResult = await _chatService.SendMessageAsync(messageDto, userId);
                
                if (chatResult.Success && chatResult.Data != null)
                {
                    // Enhance the response with receipt information
                    var enhancedMessage = $"{aiResponseMessage}\n\n{chatResult.Data.Message}";
                    chatResult.Data.Message = enhancedMessage;
                    chatResult.Data.SuggestedActions.AddRange(new List<string>
                    {
                        "Create transaction from receipt",
                        "View receipt details",
                        "Upload another receipt"
                    });
                    
                    return Ok(chatResult);
                }

                // Fallback response
                var chatResponse = new ChatResponseDto
                {
                    Message = aiResponseMessage,
                    ConversationId = conversation.Id,
                    SuggestedActions = new List<string>
                    {
                        "Create transaction from receipt",
                        "View receipt details",
                        "Upload another receipt"
                    },
                    TokensUsed = 0,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(ApiResponse<ChatResponseDto>.SuccessResult(chatResponse, "Receipt processed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadReceipt endpoint");
                return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult($"Failed to upload receipt: {ex.Message}"));
            }
        }

        private string BuildReceiptResponseMessage(ReceiptDto receiptData)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("## 📄 Receipt Processed Successfully!");
            sb.AppendLine();
            
            if (receiptData.ExtractedAmount.HasValue)
            {
                sb.AppendLine($"**Amount**: ${receiptData.ExtractedAmount.Value:F2}");
            }
            
            if (!string.IsNullOrEmpty(receiptData.ExtractedMerchant))
            {
                sb.AppendLine($"**Merchant**: {receiptData.ExtractedMerchant}");
            }
            
            if (receiptData.ExtractedDate.HasValue)
            {
                sb.AppendLine($"**Date**: {receiptData.ExtractedDate.Value:MMM dd, yyyy}");
            }
            
            if (receiptData.IsOcrProcessed)
            {
                sb.AppendLine("**Status**: OCR Processed");
            }
            
            if (receiptData.ExtractedItems != null && receiptData.ExtractedItems.Any())
            {
                sb.AppendLine();
                sb.AppendLine("**Items Found**:");
                foreach (var item in receiptData.ExtractedItems.Take(5))
                {
                    sb.AppendLine($"- {item.Description}: ${item.Price ?? 0:F2}");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("Would you like me to create a transaction from this receipt?");
            
            return sb.ToString();
        }

        private async Task<ChatConversation> GetOrCreateConversationAsync(string userId, string? conversationId)
        {
            if (!string.IsNullOrEmpty(conversationId))
            {
                var existing = await _context.ChatConversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);
                if (existing != null)
                {
                    return existing;
                }
            }
            
            // Create new conversation
            var createResult = await _chatService.CreateConversationAsync(userId, "Receipt Upload");
            if (createResult.Success && createResult.Data != null)
            {
                return await _context.ChatConversations.FindAsync(createResult.Data.Id) 
                    ?? new ChatConversation
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Title = "Receipt Upload",
                        StartedAt = DateTime.UtcNow,
                        LastMessageAt = DateTime.UtcNow,
                        IsActive = true
                    };
            }
            
            return new ChatConversation
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = "Receipt Upload",
                StartedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                IsActive = true
            };
        }
    }

    public class GenerateReportDto
    {
        public string ReportType { get; set; } = string.Empty; // "financial_summary", "bill_analysis", "expense_report"
        public string Format { get; set; } = "pdf"; // "pdf" or "excel"
    }
}

