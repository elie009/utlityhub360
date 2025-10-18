using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDisposableAmountService _disposableAmountService;
        private readonly IBillService _billService;
        private readonly ILoanService _loanService;
        private readonly ISavingsService _savingsService;
        private readonly IMemoryCache _cache;
        private readonly OpenAISettings _openAISettings;
        private readonly ILogger<ChatService> _logger;
        private readonly Dictionary<string, List<DateTime>> _rateLimitTracker = new();

        public ChatService(
            ApplicationDbContext context,
            IDisposableAmountService disposableAmountService,
            IBillService billService,
            ILoanService loanService,
            ISavingsService savingsService,
            IMemoryCache cache,
            OpenAISettings openAISettings,
            ILogger<ChatService> logger)
        {
            _context = context;
            _disposableAmountService = disposableAmountService;
            _billService = billService;
            _loanService = loanService;
            _savingsService = savingsService;
            _cache = cache;
            _openAISettings = openAISettings;
            _logger = logger;
        }

        public async Task<ApiResponse<ChatResponseDto>> SendMessageAsync(ChatMessageDto messageDto, string userId)
        {
            try
            {
                // Check rate limiting
                if (await IsRateLimitedAsync(userId))
                {
                    return ApiResponse<ChatResponseDto>.ErrorResult("Rate limit exceeded. Please wait before sending another message.");
                }

                // Validate OpenAI API key
                if (string.IsNullOrEmpty(_openAISettings.ApiKey) || _openAISettings.ApiKey == "YOUR_API_KEY_HERE")
                {
                    return ApiResponse<ChatResponseDto>.ErrorResult("OpenAI API key not configured. Please contact administrator.");
                }

                // Get or create conversation
                ChatConversation conversation;
                if (string.IsNullOrEmpty(messageDto.ConversationId))
                {
                    var createResult = await CreateConversationAsync(userId, "New Chat");
                    if (!createResult.Success || createResult.Data == null)
                    {
                        return ApiResponse<ChatResponseDto>.ErrorResult("Failed to create conversation");
                    }
                    conversation = await _context.ChatConversations.FindAsync(createResult.Data.Id);
                }
                else
                {
                    conversation = await _context.ChatConversations
                        .FirstOrDefaultAsync(c => c.Id == messageDto.ConversationId && c.UserId == userId);
                    if (conversation == null)
                    {
                        return ApiResponse<ChatResponseDto>.ErrorResult("Conversation not found");
                    }
                }

                // Store user message
                var userMessage = new ChatMessage
                {
                    ConversationId = conversation.Id,
                    UserId = userId,
                    Role = "user",
                    Content = messageDto.Message,
                    Timestamp = DateTime.UtcNow
                };

                _context.ChatMessages.Add(userMessage);
                await _context.SaveChangesAsync();

                // Build financial context if requested
                ChatContextDto? financialContext = null;
                if (messageDto.IncludeTransactionContext)
                {
                    var contextResult = await BuildFinancialContextAsync(userId);
                    if (contextResult.Success)
                    {
                        financialContext = contextResult.Data;
                    }
                }

                // Get conversation history for context
                var conversationHistory = await GetConversationHistoryForAI(conversation.Id, _openAISettings.MaxConversationHistory);

                // Call OpenAI API
                var aiResponse = await CallOpenAIAsync(messageDto.Message, financialContext, conversationHistory);

                // Store AI response
                var aiMessage = new ChatMessage
                {
                    ConversationId = conversation.Id,
                    UserId = userId,
                    Role = "assistant",
                    Content = aiResponse.Message,
                    TokensUsed = aiResponse.TokensUsed,
                    Metadata = JsonSerializer.Serialize(new { SuggestedActions = aiResponse.SuggestedActions })
                };

                _context.ChatMessages.Add(aiMessage);

                // Update conversation
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.TotalMessages += 2; // User + AI message
                conversation.TotalTokensUsed += aiResponse.TokensUsed;

                await _context.SaveChangesAsync();

                // Update rate limit tracker
                UpdateRateLimitTracker(userId);

                var response = new ChatResponseDto
                {
                    Message = aiResponse.Message,
                    ConversationId = conversation.Id,
                    SuggestedActions = aiResponse.SuggestedActions,
                    TokensUsed = aiResponse.TokensUsed,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation($"Chat message processed for user {userId}, conversation {conversation.Id}, tokens used: {aiResponse.TokensUsed}");

                return ApiResponse<ChatResponseDto>.SuccessResult(response, "Message processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing chat message for user {userId}");
                return ApiResponse<ChatResponseDto>.ErrorResult($"Failed to process message: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<ChatConversationDto>>> GetUserConversationsAsync(string userId, int page = 1, int limit = 10)
        {
            try
            {
                var skip = (page - 1) * limit;

                var conversations = await _context.ChatConversations
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.LastMessageAt)
                    .Skip(skip)
                    .Take(limit)
                    .Select(c => new ChatConversationDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        StartedAt = c.StartedAt,
                        LastMessageAt = c.LastMessageAt,
                        IsActive = c.IsActive,
                        TotalMessages = c.TotalMessages,
                        TotalTokensUsed = c.TotalTokensUsed
                    })
                    .ToListAsync();

                var totalCount = await _context.ChatConversations
                    .Where(c => c.UserId == userId)
                    .CountAsync();

                var paginatedResponse = new PaginatedResponse<ChatConversationDto>
                {
                    Data = conversations,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<ChatConversationDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversations for user {userId}");
                return ApiResponse<PaginatedResponse<ChatConversationDto>>.ErrorResult($"Failed to get conversations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ChatConversationDto>> GetConversationHistoryAsync(string conversationId, string userId)
        {
            try
            {
                var conversation = await _context.ChatConversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

                if (conversation == null)
                {
                    return ApiResponse<ChatConversationDto>.ErrorResult("Conversation not found");
                }

                var conversationDto = new ChatConversationDto
                {
                    Id = conversation.Id,
                    Title = conversation.Title,
                    StartedAt = conversation.StartedAt,
                    LastMessageAt = conversation.LastMessageAt,
                    IsActive = conversation.IsActive,
                    TotalMessages = conversation.TotalMessages,
                    TotalTokensUsed = conversation.TotalTokensUsed,
                    Messages = conversation.Messages
                        .OrderBy(m => m.Timestamp)
                        .Select(m => new ChatMessageResponseDto
                        {
                            Id = m.Id,
                            Role = m.Role,
                            Content = m.Content,
                            Timestamp = m.Timestamp,
                            TokensUsed = m.TokensUsed,
                            Metadata = m.Metadata
                        })
                        .ToList()
                };

                return ApiResponse<ChatConversationDto>.SuccessResult(conversationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation history for {conversationId}");
                return ApiResponse<ChatConversationDto>.ErrorResult($"Failed to get conversation history: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteConversationAsync(string conversationId, string userId)
        {
            try
            {
                var conversation = await _context.ChatConversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

                if (conversation == null)
                {
                    return ApiResponse<bool>.ErrorResult("Conversation not found");
                }

                _context.ChatConversations.Remove(conversation);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Conversation {conversationId} deleted by user {userId}");

                return ApiResponse<bool>.SuccessResult(true, "Conversation deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting conversation {conversationId}");
                return ApiResponse<bool>.ErrorResult($"Failed to delete conversation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ChatConversationDto>> CreateConversationAsync(string userId, string? title = null)
        {
            try
            {
                var conversation = new ChatConversation
                {
                    UserId = userId,
                    Title = title ?? "New Conversation",
                    StartedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ChatConversations.Add(conversation);
                await _context.SaveChangesAsync();

                var conversationDto = new ChatConversationDto
                {
                    Id = conversation.Id,
                    Title = conversation.Title,
                    StartedAt = conversation.StartedAt,
                    LastMessageAt = conversation.LastMessageAt,
                    IsActive = conversation.IsActive,
                    TotalMessages = conversation.TotalMessages,
                    TotalTokensUsed = conversation.TotalTokensUsed
                };

                return ApiResponse<ChatConversationDto>.SuccessResult(conversationDto, "Conversation created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating conversation for user {userId}");
                return ApiResponse<ChatConversationDto>.ErrorResult($"Failed to create conversation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ChatContextDto>> BuildFinancialContextAsync(string userId)
        {
            try
            {
                var cacheKey = $"financial_context_{userId}";
                if (_cache.TryGetValue(cacheKey, out ChatContextDto? cachedContext))
                {
                    return ApiResponse<ChatContextDto>.SuccessResult(cachedContext!);
                }

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var context = new ChatContextDto();

                // Get financial summary
                var startDate = DateTime.UtcNow.AddDays(-30);
                var endDate = DateTime.UtcNow;
                var disposableResult = await _disposableAmountService.GetDisposableAmountAsync(userId, startDate, endDate);
                if (disposableResult != null)
                {
                    context.FinancialSummary = new ChatFinancialSummaryDto
                    {
                        TotalIncome = disposableResult.TotalIncome,
                        TotalExpenses = disposableResult.TotalFixedExpenses + disposableResult.TotalVariableExpenses,
                        DisposableAmount = disposableResult.DisposableAmount,
                        TotalSavings = 0, // Will be calculated separately
                        NetWorth = disposableResult.NetDisposableAmount ?? 0
                    };
                }

                // Get upcoming bills (next 7 days)
                var billsResult = await _billService.GetUserBillsAsync(userId, null, null, 1, 100);
                if (billsResult.Success && billsResult.Data != null)
                {
                    context.UpcomingBills = billsResult.Data.Data
                        .Where(b => b.DueDate <= DateTime.UtcNow.AddDays(7) && b.Status == "PENDING")
                        .ToList();
                }

                // Get recent transactions
                var recentTransactions = await _context.BankTransactions
                    .Where(t => t.UserId == userId && t.TransactionDate >= thirtyDaysAgo)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(50)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        Amount = t.Amount,
                        Description = t.Description,
                        CreatedAt = t.TransactionDate,
                        Type = t.TransactionType
                    })
                    .ToListAsync();

                context.RecentTransactions = recentTransactions;

                // Get active loans
                var loansResult = await _loanService.GetUserLoansAsync(userId, null, 1, 100);
                if (loansResult.Success && loansResult.Data != null)
                {
                    context.ActiveLoans = loansResult.Data.Data
                        .Where(l => l.Status == "ACTIVE")
                        .ToList();
                }

                // Get savings accounts
                var savingsResult = await _savingsService.GetUserSavingsAccountsAsync(userId);
                if (savingsResult.Success && savingsResult.Data != null)
                {
                    context.SavingsAccounts = savingsResult.Data;
                }

                // Get recent variable expenses
                var recentExpenses = await _context.VariableExpenses
                    .Where(e => e.UserId == userId && e.ExpenseDate >= thirtyDaysAgo)
                    .OrderByDescending(e => e.ExpenseDate)
                    .Take(30)
                    .Select(e => new VariableExpenseDto
                    {
                        Id = e.Id,
                        Amount = e.Amount,
                        Category = e.Category,
                        Description = e.Description,
                        ExpenseDate = e.ExpenseDate
                    })
                    .ToListAsync();

                context.RecentExpenses = recentExpenses;

                // Cache for 5 minutes
                _cache.Set(cacheKey, context, TimeSpan.FromMinutes(5));

                return ApiResponse<ChatContextDto>.SuccessResult(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building financial context for user {userId}");
                return ApiResponse<ChatContextDto>.ErrorResult($"Failed to build financial context: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> GenerateReportAsync(string userId, string reportType, string format = "pdf")
        {
            try
            {
                // This is a placeholder implementation
                // In a real implementation, you would generate actual PDF/Excel reports
                var reportPath = $"/reports/{userId}_{reportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
                
                _logger.LogInformation($"Report generation requested: {reportType} in {format} format for user {userId}");
                
                return ApiResponse<string>.SuccessResult(reportPath, "Report generation initiated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating report for user {userId}");
                return ApiResponse<string>.ErrorResult($"Failed to generate report: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<string>>> GetBillRemindersAsync(string userId)
        {
            try
            {
                var upcomingBills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.DueDate <= DateTime.UtcNow.AddDays(7) && 
                               b.Status == "PENDING")
                    .OrderBy(b => b.DueDate)
                    .ToListAsync();

                var reminders = upcomingBills.Select(b => 
                    $"Bill '{b.BillName}' of ${b.Amount:F2} is due on {b.DueDate:MMM dd, yyyy}")
                    .ToList();

                return ApiResponse<List<string>>.SuccessResult(reminders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting bill reminders for user {userId}");
                return ApiResponse<List<string>>.ErrorResult($"Failed to get bill reminders: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<string>>> GetBudgetSuggestionsAsync(string userId)
        {
            try
            {
                var contextResult = await BuildFinancialContextAsync(userId);
                if (!contextResult.Success || contextResult.Data == null)
                {
                    return ApiResponse<List<string>>.ErrorResult("Failed to build financial context");
                }

                var context = contextResult.Data;
                var suggestions = new List<string>();

                // Analyze spending patterns and provide suggestions
                if (context.FinancialSummary.DisposableAmount < 0)
                {
                    suggestions.Add("Your expenses exceed your income. Consider reducing variable expenses or increasing income sources.");
                }

                if (context.UpcomingBills.Count > 0)
                {
                    var totalUpcomingBills = context.UpcomingBills.Sum(b => b.Amount);
                    suggestions.Add($"You have {context.UpcomingBills.Count} bills totaling ${totalUpcomingBills:F2} due in the next 7 days.");
                }

                if (context.RecentExpenses.Count > 0)
                {
                    var avgDailyExpense = context.RecentExpenses.Average(e => e.Amount);
                    suggestions.Add($"Your average daily variable expense is ${avgDailyExpense:F2}. Consider tracking this more closely.");
                }

                return ApiResponse<List<string>>.SuccessResult(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting budget suggestions for user {userId}");
                return ApiResponse<List<string>>.ErrorResult($"Failed to get budget suggestions: {ex.Message}");
            }
        }

        public async Task<bool> IsRateLimitedAsync(string userId)
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            if (!_rateLimitTracker.ContainsKey(userId))
            {
                _rateLimitTracker[userId] = new List<DateTime>();
            }

            // Remove old timestamps
            _rateLimitTracker[userId] = _rateLimitTracker[userId]
                .Where(t => t > oneMinuteAgo)
                .ToList();

            return _rateLimitTracker[userId].Count >= _openAISettings.RateLimitPerMinute;
        }

        private void UpdateRateLimitTracker(string userId)
        {
            if (!_rateLimitTracker.ContainsKey(userId))
            {
                _rateLimitTracker[userId] = new List<DateTime>();
            }

            _rateLimitTracker[userId].Add(DateTime.UtcNow);
        }

        private async Task<List<ChatMessageResponseDto>> GetConversationHistoryForAI(string conversationId, int maxMessages)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.Timestamp)
                .Take(maxMessages)
                .Select(m => new ChatMessageResponseDto
                {
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.Timestamp
                })
                .ToListAsync();

            return messages.OrderBy(m => m.Timestamp).ToList();
        }

        private async Task<ChatResponseDto> CallOpenAIAsync(string userMessage, ChatContextDto? financialContext, List<ChatMessageResponseDto> conversationHistory)
        {
            try
            {
                if (string.IsNullOrEmpty(_openAISettings.ApiKey))
                {
                    return new ChatResponseDto
                    {
                        Message = "OpenAI API key is not configured. Please contact your administrator.",
                        SuggestedActions = new List<string>(),
                        TokensUsed = 0
                    };
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAISettings.ApiKey}");

                var messages = new List<object>
                {
                    new { role = "system", content = _openAISettings.SystemPrompt }
                };

                // Add conversation history
                foreach (var msg in conversationHistory.TakeLast(_openAISettings.MaxConversationHistory))
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }

                // Add financial context if available
                if (financialContext != null)
                {
                    var contextMessage = $"User's financial context: {JsonSerializer.Serialize(financialContext)}";
                    messages.Add(new { role = "system", content = contextMessage });
                }

                // Add current user message
                messages.Add(new { role = "user", content = userMessage });

                var requestBody = new
                {
                    model = _openAISettings.Model,
                    messages = messages,
                    max_tokens = _openAISettings.MaxTokens,
                    temperature = _openAISettings.Temperature
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
                    var aiMessage = openAIResponse?.choices?.FirstOrDefault()?.message?.content ?? "I apologize, but I couldn't generate a proper response.";

                    // Format the message for better readability
                    var formattedMessage = FormatMessage(aiMessage);

                    return new ChatResponseDto
                    {
                        Message = formattedMessage,
                        SuggestedActions = ExtractSuggestedActions(formattedMessage),
                        TokensUsed = openAIResponse.usage?.total_tokens ?? 0
                    };
                }
                else
                {
                    _logger.LogError($"OpenAI API error: {response.StatusCode} - {responseContent}");
                    return new ChatResponseDto
                    {
                        Message = "I apologize, but I'm experiencing technical difficulties with the AI service. Please try again later.",
                        SuggestedActions = new List<string> { "Contact support" },
                        TokensUsed = 0
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI response generation");
                return new ChatResponseDto
                {
                    Message = "I apologize, but I'm experiencing technical difficulties. Please try again later.",
                    SuggestedActions = new List<string> { "Contact support" },
                    TokensUsed = 0
                };
            }
        }

        private string FormatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            // Split by common sentence endings and list markers
            var sentences = message.Split(new[] { ". ", "! ", "? ", ".\n", "!\n", "?\n" }, StringSplitOptions.None);
            var formattedLines = new List<string>();

            foreach (var sentence in sentences)
            {
                if (string.IsNullOrWhiteSpace(sentence))
                    continue;

                var trimmedSentence = sentence.Trim();
                
                // Add proper ending punctuation if missing
                if (!trimmedSentence.EndsWith(".") && !trimmedSentence.EndsWith("!") && !trimmedSentence.EndsWith("?"))
                {
                    trimmedSentence += ".";
                }

                // Handle numbered lists (1., 2., etc.)
                if (System.Text.RegularExpressions.Regex.IsMatch(trimmedSentence, @"^\d+\.\s"))
                {
                    formattedLines.Add($"\n{trimmedSentence}");
                }
                // Handle bullet points (-, *, •)
                else if (trimmedSentence.StartsWith("- ") || trimmedSentence.StartsWith("* ") || trimmedSentence.StartsWith("• "))
                {
                    formattedLines.Add($"\n{trimmedSentence}");
                }
                // Handle bold text (**text**)
                else if (trimmedSentence.Contains("**"))
                {
                    formattedLines.Add($"\n{trimmedSentence}");
                }
                // Regular sentences
                else
                {
                    formattedLines.Add(trimmedSentence);
                }
            }

            // Join with proper spacing
            var result = string.Join(" ", formattedLines);
            
            // Clean up multiple newlines and spaces
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\n\s*\n", "\n\n");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
            
            // Ensure proper spacing around newlines
            result = result.Replace("\n ", "\n").Replace(" \n", "\n");
            
            return result.Trim();
        }

        private List<string> ExtractSuggestedActions(string message)
        {
            var actions = new List<string>();
            var lowerMessage = message.ToLower();

            if (lowerMessage.Contains("bill") || lowerMessage.Contains("payment"))
            {
                actions.Add("View upcoming bills");
            }
            if (lowerMessage.Contains("budget") || lowerMessage.Contains("expense"))
            {
                actions.Add("Review budget");
            }
            if (lowerMessage.Contains("loan"))
            {
                actions.Add("Check loan status");
            }
            if (lowerMessage.Contains("savings") || lowerMessage.Contains("save"))
            {
                actions.Add("View savings accounts");
            }
            if (lowerMessage.Contains("income") || lowerMessage.Contains("salary"))
            {
                actions.Add("Review income sources");
            }

            return actions;
        }
    }

    // OpenAI API Response Models
    public class OpenAIResponse
    {
        public List<OpenAIChoice> choices { get; set; } = new();
        public OpenAIUsage? usage { get; set; }
    }

    public class OpenAIChoice
    {
        public OpenAIMessage message { get; set; } = new();
    }

    public class OpenAIMessage
    {
        public string role { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
    }

    public class OpenAIUsage
    {
        public int total_tokens { get; set; }
    }
}
