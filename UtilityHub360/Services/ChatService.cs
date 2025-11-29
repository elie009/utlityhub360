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
        private readonly IDocumentationSearchService _documentationSearchService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IFinancialReportService _financialReportService;
        private readonly Dictionary<string, List<DateTime>> _rateLimitTracker = new();

        public ChatService(
            ApplicationDbContext context,
            IDisposableAmountService disposableAmountService,
            IBillService billService,
            ILoanService loanService,
            ISavingsService savingsService,
            IMemoryCache cache,
            OpenAISettings openAISettings,
            ILogger<ChatService> logger,
            IDocumentationSearchService documentationSearchService,
            IBankAccountService bankAccountService,
            IFinancialReportService financialReportService)
        {
            _context = context;
            _disposableAmountService = disposableAmountService;
            _billService = billService;
            _loanService = loanService;
            _savingsService = savingsService;
            _cache = cache;
            _openAISettings = openAISettings;
            _logger = logger;
            _documentationSearchService = documentationSearchService;
            _bankAccountService = bankAccountService;
            _financialReportService = financialReportService;
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

                // Process special commands (reports, entity creation)
                var specialCommandResult = await ProcessSpecialCommandsAsync(messageDto.Message, userId, financialContext);
                if (specialCommandResult != null)
                {
                    return specialCommandResult;
                }

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

                // Build enhanced system prompt
                var systemPrompt = BuildEnhancedSystemPrompt();
                
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                // Add documentation context ONLY for system functionality questions (not programming)
                var lowerMessage = userMessage.ToLower();
                var isSystemQuestion = lowerMessage.Contains("how do i") || 
                                      lowerMessage.Contains("how to") || 
                                      lowerMessage.Contains("what is") ||
                                      lowerMessage.Contains("explain") ||
                                      lowerMessage.Contains("feature");
                
                // Filter out programming-related questions
                var isProgrammingQuestion = lowerMessage.Contains("code") || 
                                           lowerMessage.Contains("programming") ||
                                           lowerMessage.Contains("api endpoint") ||
                                           lowerMessage.Contains("controller") ||
                                           lowerMessage.Contains("service class") ||
                                           lowerMessage.Contains("database") ||
                                           lowerMessage.Contains("sql") ||
                                           lowerMessage.Contains("entity framework");
                
                if (isSystemQuestion && !isProgrammingQuestion)
                {
                    var documentationContext = await _documentationSearchService.GetDocumentationContextAsync(userMessage);
                    if (!string.IsNullOrEmpty(documentationContext))
                    {
                        messages.Add(new { role = "system", content = documentationContext });
                    }
                }

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

            // First, normalize existing line breaks
            message = message.Replace("\r\n", "\n").Replace("\r", "\n");
            
            var lines = new List<string>();
            var paragraphs = message.Split(new[] { "\n\n" }, StringSplitOptions.None);
            
            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                    continue;
                    
                var trimmed = paragraph.Trim();
                
                // Check if it's a heading (starts with #)
                if (trimmed.StartsWith("#"))
                {
                    lines.Add("");  // Add blank line before heading
                    lines.Add(trimmed);
                    lines.Add("");  // Add blank line after heading
                    continue;
                }
                
                // Check if it's a list item
                if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(\d+\.|-|\*|•)\s"))
                {
                    // Split list into individual items
                    var listLines = trimmed.Split('\n');
                    foreach (var item in listLines)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            lines.Add(item.Trim());
                        }
                    }
                    lines.Add("");  // Blank line after list
                    continue;
                }
                
                // Check if it contains bullet points or numbered lists inline
                if (trimmed.Contains("\n-") || trimmed.Contains("\n*") || 
                    System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"\n\d+\."))
                {
                    var parts = trimmed.Split('\n');
                    foreach (var part in parts)
                    {
                        var cleanPart = part.Trim();
                        if (!string.IsNullOrWhiteSpace(cleanPart))
                        {
                            lines.Add(cleanPart);
                        }
                    }
                    lines.Add("");  // Blank line after list
                    continue;
                }
                
                // Check for bold/emphasis markers
                if (trimmed.Contains("**"))
                {
                    lines.Add("");
                    lines.Add(trimmed);
                    lines.Add("");
                    continue;
                }
                
                // Regular paragraph - break long sentences
                var sentences = System.Text.RegularExpressions.Regex.Split(trimmed, @"(?<=[.!?])\s+");
                var currentParagraph = new StringBuilder();
                
                foreach (var sentence in sentences)
                {
                    if (string.IsNullOrWhiteSpace(sentence))
                        continue;
                        
                    currentParagraph.Append(sentence);
                    currentParagraph.Append(" ");
                    
                    // Add line break after every 2-3 sentences for readability
                    if (currentParagraph.Length > 150)
                    {
                        lines.Add(currentParagraph.ToString().Trim());
                        currentParagraph.Clear();
                    }
                }
                
                if (currentParagraph.Length > 0)
                {
                    lines.Add(currentParagraph.ToString().Trim());
                }
                
                lines.Add("");  // Blank line after paragraph
            }
            
            // Join with newlines and clean up
            var result = string.Join("\n", lines);
            
            // Clean up excessive blank lines (max 2 in a row)
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\n{3,}", "\n\n");
            
            // Trim leading/trailing whitespace
            return result.Trim();
        }

        private string BuildEnhancedSystemPrompt()
        {
            var basePrompt = _openAISettings.SystemPrompt;
            
            // If system prompt is empty or default, use enhanced accountant prompt
            if (string.IsNullOrEmpty(basePrompt) || basePrompt.Contains("helpful assistant"))
            {
                return @"You are a senior accountant and financial advisor for UtilityHub360, a comprehensive financial management system. Your role is to:

1. **Act as a Senior Accountant**: Provide expert financial advice, analysis, and guidance based on accounting principles and best practices.

2. **Answer System Questions**: Help users understand how to use UtilityHub360 features, including:
   - How to create and manage bills, loans, bank accounts, and savings goals
   - How to view financial reports and analytics
   - How to track expenses and income
   - How to use the system's accounting features

3. **Provide Financial Reports**: When users request reports or figures, provide detailed financial information including:
   - Income and expense summaries
   - Bill analytics and upcoming payments
   - Loan status and payment schedules
   - Savings progress and goals
   - Financial summaries and trends

4. **Assist with Entity Creation**: Guide users through creating:
   - New bills (with amount, due date, frequency, type)
   - Loan applications (with purpose, principal, interest rate, term)
   - Bank accounts (with account type, initial balance, currency)
   - Savings goals (with target amount, target date, savings type)

5. **Receipt Processing**: When users upload receipt images (JPG/PNG), help them convert them into transactions by extracting:
   - Amount, date, merchant name
   - Category and description
   - Link to appropriate bank account

**IMPORTANT RESTRICTIONS:**
- DO NOT answer programming-related questions (code, API endpoints, database queries, entity framework, SQL, controllers, service classes)
- DO NOT provide technical implementation details
- DO NOT explain how the system is built internally
- If asked about programming/technical implementation, politely redirect: ""I'm here to help with your finances and using UtilityHub360. For technical questions about the system's code, please contact the development team.""

**Response Style:**
- Be professional, clear, and concise
- Use accounting terminology appropriately
- Provide actionable financial advice
- Format numbers and dates clearly
- Suggest relevant next steps based on the user's financial situation

**When providing reports or figures:**
- Include specific numbers from the user's financial context
- Highlight important trends or concerns
- Provide recommendations when appropriate
- Format data in a clear, readable manner";
            }
            
            return basePrompt;
        }

        private List<string> ExtractSuggestedActions(string message)
        {
            var actions = new List<string>();
            var lowerMessage = message.ToLower();

            if (lowerMessage.Contains("bill") || lowerMessage.Contains("payment"))
            {
                actions.Add("View upcoming bills");
                actions.Add("Create new bill");
            }
            if (lowerMessage.Contains("budget") || lowerMessage.Contains("expense"))
            {
                actions.Add("Review budget");
                actions.Add("View expense report");
            }
            if (lowerMessage.Contains("loan"))
            {
                actions.Add("Check loan status");
                actions.Add("Apply for loan");
            }
            if (lowerMessage.Contains("savings") || lowerMessage.Contains("save"))
            {
                actions.Add("View savings accounts");
                actions.Add("Create savings goal");
            }
            if (lowerMessage.Contains("income") || lowerMessage.Contains("salary"))
            {
                actions.Add("Review income sources");
            }
            if (lowerMessage.Contains("report") || lowerMessage.Contains("analytics"))
            {
                actions.Add("Generate financial report");
                actions.Add("View financial summary");
            }
            if (lowerMessage.Contains("bank account") || lowerMessage.Contains("account"))
            {
                actions.Add("View bank accounts");
                actions.Add("Add bank account");
            }

            return actions;
        }

        public async Task<ApiResponse<string>> SearchDocumentationAsync(string query)
        {
            try
            {
                var context = await _documentationSearchService.GetDocumentationContextAsync(query);
                
                if (string.IsNullOrEmpty(context))
                {
                    return ApiResponse<string>.ErrorResult("No relevant documentation found for your query.");
                }

                return ApiResponse<string>.SuccessResult(context, "Documentation retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching documentation for query: {query}");
                return ApiResponse<string>.ErrorResult($"Failed to search documentation: {ex.Message}");
            }
        }

        private async Task<ApiResponse<ChatResponseDto>?> ProcessSpecialCommandsAsync(string userMessage, string userId, ChatContextDto? financialContext)
        {
            var lowerMessage = userMessage.ToLower().Trim();
            
            // Check for report requests
            if (lowerMessage.Contains("report") || lowerMessage.Contains("generate report") || 
                lowerMessage.Contains("financial report") || lowerMessage.Contains("show me") && 
                (lowerMessage.Contains("summary") || lowerMessage.Contains("analytics")))
            {
                return await HandleReportRequestAsync(userMessage, userId, financialContext);
            }
            
            // Check for entity creation requests
            if (lowerMessage.Contains("create") || lowerMessage.Contains("add") || lowerMessage.Contains("new"))
            {
                if (lowerMessage.Contains("bill"))
                {
                    return await HandleCreateBillRequestAsync(userMessage, userId);
                }
                else if (lowerMessage.Contains("loan"))
                {
                    return await HandleCreateLoanRequestAsync(userMessage, userId);
                }
                else if (lowerMessage.Contains("bank account") || lowerMessage.Contains("account"))
                {
                    return await HandleCreateBankAccountRequestAsync(userMessage, userId);
                }
                else if (lowerMessage.Contains("savings") || lowerMessage.Contains("saving goal"))
                {
                    return await HandleCreateSavingsRequestAsync(userMessage, userId);
                }
            }
            
            return null; // Not a special command, continue with normal AI processing
        }

        private async Task<ApiResponse<ChatResponseDto>> HandleReportRequestAsync(string userMessage, string userId, ChatContextDto? financialContext)
        {
            try
            {
                var reportFormat = "json"; // Default to JSON for chat display
                var lowerMessage = userMessage.ToLower();
                
                if (lowerMessage.Contains("pdf"))
                {
                    reportFormat = "pdf";
                }
                else if (lowerMessage.Contains("excel") || lowerMessage.Contains("csv"))
                {
                    reportFormat = "excel";
                }

                // Determine report type
                var reportType = "financial_summary";
                if (lowerMessage.Contains("bill"))
                {
                    reportType = "bill_analysis";
                }
                else if (lowerMessage.Contains("expense"))
                {
                    reportType = "expense_report";
                }
                else if (lowerMessage.Contains("income"))
                {
                    reportType = "income_report";
                }

                // Generate report data
                var query = new ReportQueryDto
                {
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow,
                    Period = "MONTHLY"
                };

                var reportResult = await _financialReportService.GenerateFullReportAsync(userId, query);
                
                if (reportResult.Success && reportResult.Data != null)
                {
                    var report = reportResult.Data;
                    var reportText = BuildReportText(report, financialContext);
                    
                    return ApiResponse<ChatResponseDto>.SuccessResult(new ChatResponseDto
                    {
                        Message = reportText,
                        SuggestedActions = new List<string> 
                        { 
                            "Export as PDF", 
                            "Export as Excel",
                            "View detailed analytics"
                        },
                        TokensUsed = 0,
                        Timestamp = DateTime.UtcNow
                    });
                }
                
                // Fallback to AI if report generation fails
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling report request");
                return null; // Fallback to normal AI processing
            }
        }

        private string BuildReportText(FinancialReportDto report, ChatContextDto? financialContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## 📊 Financial Report Summary");
            sb.AppendLine();

            if (report.Summary != null)
            {
                sb.AppendLine("### Financial Overview");
                sb.AppendLine($"- **Total Income**: ${report.Summary.TotalIncome:F2}");
                sb.AppendLine($"- **Total Expenses**: ${report.Summary.TotalExpenses:F2}");
                sb.AppendLine($"- **Disposable Income**: ${report.Summary.DisposableIncome:F2}");
                sb.AppendLine($"- **Net Worth**: ${report.Summary.NetWorth:F2}");
                sb.AppendLine();
            }

            if (report.BillsReport != null)
            {
                sb.AppendLine("### Bills Summary");
                sb.AppendLine($"- **Total Monthly Bills**: ${report.BillsReport.TotalMonthlyBills:F2}");
                sb.AppendLine($"- **Unpaid Bills Count**: {report.BillsReport.UnpaidBillsCount}");
                sb.AppendLine($"- **Overdue Bills Count**: {report.BillsReport.OverdueBillsCount}");
                sb.AppendLine($"- **Average Monthly Bills**: ${report.BillsReport.AverageMonthlyBills:F2}");
                sb.AppendLine();
            }

            if (report.LoanReport != null)
            {
                sb.AppendLine("### Loans Summary");
                sb.AppendLine($"- **Active Loans**: {report.LoanReport.ActiveLoansCount}");
                sb.AppendLine($"- **Total Principal**: ${report.LoanReport.TotalPrincipal:F2}");
                sb.AppendLine($"- **Total Monthly Payment**: ${report.LoanReport.TotalMonthlyPayment:F2}");
                sb.AppendLine($"- **Remaining Balance**: ${report.LoanReport.TotalRemainingBalance:F2}");
                sb.AppendLine();
            }

            if (financialContext?.UpcomingBills != null && financialContext.UpcomingBills.Any())
            {
                sb.AppendLine("### ⚠️ Upcoming Bills (Next 7 Days)");
                foreach (var bill in financialContext.UpcomingBills.Take(5))
                {
                    sb.AppendLine($"- **{bill.BillName}**: ${bill.Amount:F2} due on {bill.DueDate:MMM dd, yyyy}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("Would you like me to export this report as PDF or Excel?");
            
            return sb.ToString();
        }

        private async Task<ApiResponse<ChatResponseDto>> HandleCreateBillRequestAsync(string userMessage, string userId)
        {
            // Extract bill information from user message using AI or pattern matching
            var response = new ChatResponseDto
            {
                Message = "I'd be happy to help you create a new bill! To get started, I'll need the following information:\n\n" +
                         "1. **Bill Name** (e.g., 'Electricity Bill')\n" +
                         "2. **Amount** (e.g., $150.00)\n" +
                         "3. **Due Date** (e.g., 2024-12-15)\n" +
                         "4. **Bill Type** (Utility, Insurance, Subscription, etc.)\n" +
                         "5. **Frequency** (Monthly, Quarterly, Yearly)\n\n" +
                         "Please provide these details, or you can use the form in the chat interface.",
                SuggestedActions = new List<string> 
                { 
                    "Show bill creation form",
                    "View existing bills"
                },
                TokensUsed = 0,
                Timestamp = DateTime.UtcNow
            };
            
            return ApiResponse<ChatResponseDto>.SuccessResult(response);
        }

        private async Task<ApiResponse<ChatResponseDto>> HandleCreateLoanRequestAsync(string userMessage, string userId)
        {
            var response = new ChatResponseDto
            {
                Message = "I can help you apply for a loan! Here's what I'll need:\n\n" +
                         "1. **Loan Purpose** (e.g., 'Home Purchase', 'Car Loan')\n" +
                         "2. **Principal Amount** (e.g., $50,000)\n" +
                         "3. **Interest Rate** (e.g., 5.5%)\n" +
                         "4. **Term** in months (e.g., 60 months)\n" +
                         "5. **Monthly Income**\n" +
                         "6. **Employment Status**\n\n" +
                         "Please provide these details, or use the loan application form in the chat.",
                SuggestedActions = new List<string> 
                { 
                    "Show loan application form",
                    "View existing loans"
                },
                TokensUsed = 0,
                Timestamp = DateTime.UtcNow
            };
            
            return ApiResponse<ChatResponseDto>.SuccessResult(response);
        }

        private async Task<ApiResponse<ChatResponseDto>> HandleCreateBankAccountRequestAsync(string userMessage, string userId)
        {
            var response = new ChatResponseDto
            {
                Message = "Let's add a new bank account! I'll need:\n\n" +
                         "1. **Account Name** (e.g., 'Chase Checking')\n" +
                         "2. **Account Type** (Checking, Savings, Credit Card, Investment)\n" +
                         "3. **Initial Balance** (e.g., $1,000.00)\n" +
                         "4. **Currency** (e.g., USD)\n" +
                         "5. **Financial Institution** (optional)\n\n" +
                         "Please provide these details, or use the form in the chat interface.",
                SuggestedActions = new List<string> 
                { 
                    "Show bank account form",
                    "View existing accounts"
                },
                TokensUsed = 0,
                Timestamp = DateTime.UtcNow
            };
            
            return ApiResponse<ChatResponseDto>.SuccessResult(response);
        }

        private async Task<ApiResponse<ChatResponseDto>> HandleCreateSavingsRequestAsync(string userMessage, string userId)
        {
            var response = new ChatResponseDto
            {
                Message = "Great! Let's create a savings goal. I'll need:\n\n" +
                         "1. **Goal Name** (e.g., 'Vacation Fund')\n" +
                         "2. **Savings Type** (Emergency, Vacation, Retirement, etc.)\n" +
                         "3. **Target Amount** (e.g., $10,000)\n" +
                         "4. **Target Date** (e.g., 2025-12-31)\n" +
                         "5. **Goal Description** (optional)\n\n" +
                         "Please provide these details, or use the savings goal form in the chat.",
                SuggestedActions = new List<string> 
                { 
                    "Show savings goal form",
                    "View existing savings goals"
                },
                TokensUsed = 0,
                Timestamp = DateTime.UtcNow
            };
            
            return ApiResponse<ChatResponseDto>.SuccessResult(response);
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
