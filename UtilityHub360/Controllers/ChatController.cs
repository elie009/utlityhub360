using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
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
    }

    public class GenerateReportDto
    {
        public string ReportType { get; set; } = string.Empty; // "financial_summary", "bill_analysis", "expense_report"
        public string Format { get; set; } = "pdf"; // "pdf" or "excel"
    }
}

