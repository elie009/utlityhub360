using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.Json;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class AIAgentService : IAIAgentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBankAccountService _bankAccountService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIAgentService> _logger;

        private const string OpenAIApiKey = "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";

        public AIAgentService(
            ApplicationDbContext context,
            IBankAccountService bankAccountService,
            ILogger<AIAgentService> logger)
        {
            _context = context;
            _bankAccountService = bankAccountService;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAIApiKey}");
        }

        public async Task<ApiResponse<AIAgentResponseDto>> ProcessAgentRequestAsync(AIAgentRequestDto request, string userId)
        {
            try
            {
                var response = new AIAgentResponseDto
                {
                    ActionsPerformed = new List<AgentActionDto>()
                };

                var messages = new List<object>
                {
                    new { role = "system", content = GetSystemPrompt() },
                    new { role = "user", content = request.Message }
                };

                var openAIRequest = new
                {
                    model = "gpt-4o-mini",
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 2000
                };

                var json = JsonSerializer.Serialize(openAIRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"OpenAI API error: {httpResponse.StatusCode} - {responseContent}");
                    return ApiResponse<AIAgentResponseDto>.ErrorResult($"AI service error: {httpResponse.StatusCode}");
                }

                var openAIResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var choices = openAIResponse.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                response.Response = message.GetProperty("content").GetString() ?? "I'm here to help!";

                return ApiResponse<AIAgentResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessAgentRequestAsync");
                return ApiResponse<AIAgentResponseDto>.ErrorResult($"Failed to process agent request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankTransactionDto>> AnalyzeAndCreateTransactionWithAgentAsync(AnalyzeTransactionTextDto analyzeDto, string userId)
        {
            try
            {
                if (analyzeDto == null || string.IsNullOrWhiteSpace(analyzeDto.TransactionText))
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult("Transaction text is required.");
                }

                var text = analyzeDto.TransactionText.Trim();

                // Use AI to extract transaction details
                var extractedData = await ExtractTransactionDetailsWithAIAsync(text);
                
                if (extractedData == null || extractedData.Amount <= 0)
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult(
                        "Could not extract valid transaction details from the text. " +
                        "Please ensure the text contains: amount, date/time, and merchant information.");
                }

                // Find or use bank account
                BankAccount? bankAccount = null;
                
                if (!string.IsNullOrEmpty(analyzeDto.BankAccountId))
                {
                    var accounts = await _context.BankAccounts
                        .Where(ba => ba.Id == analyzeDto.BankAccountId && ba.UserId == userId && ba.IsActive)
                        .ToListAsync();
                    bankAccount = accounts.FirstOrDefault();
                }
                else if (!string.IsNullOrEmpty(extractedData.CardLast4))
                {
                    bankAccount = await FindBankAccountByCardLast4Async(extractedData.CardLast4, userId);
                }

                if (bankAccount == null)
                {
                    return ApiResponse<BankTransactionDto>.ErrorResult(
                        "Bank account not found. Please provide BankAccountId or ensure card number is in your account.");
                }

                // Parse transaction date
                var transactionDateTime = ParseTransactionDateTime(extractedData.DateText, extractedData.TimeText);

                // Create transaction
                var createTransactionDto = new CreateBankTransactionDto
                {
                    BankAccountId = bankAccount.Id,
                    Amount = extractedData.Amount,
                    TransactionType = "DEBIT",
                    Description = extractedData.Description ?? extractedData.Merchant ?? "Transaction from SMS",
                    Merchant = extractedData.Merchant,
                    Location = extractedData.Location,
                    Category = extractedData.Category,
                    Currency = extractedData.Currency ?? bankAccount.Currency,
                    TransactionDate = transactionDateTime,
                    ReferenceNumber = $"SMS_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Notes = $"Parsed from SMS: {text}"
                };

                return await _bankAccountService.CreateTransactionAsync(createTransactionDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AnalyzeAndCreateTransactionWithAgentAsync");
                return ApiResponse<BankTransactionDto>.ErrorResult($"Failed to analyze and create transaction: {ex.Message}");
            }
        }

        private async Task<TransactionExtractedData?> ExtractTransactionDetailsWithAIAsync(string text)
        {
            try
            {
                var systemPrompt = @"You are a financial transaction parser. Extract transaction details from SMS/notification text and return ONLY valid JSON in this exact format:
{
  ""amount"": 84.30,
  ""currency"": ""SAR"",
  ""cardLast4"": ""0655"",
  ""merchant"": ""Dan Beverage Company"",
  ""location"": ""SAUDI ARABIA"",
  ""dateText"": ""2025-11-14"",
  ""timeText"": ""21:31:17"",
  ""description"": ""POS Purchase (Apple Pay)"",
  ""category"": null,
  ""isApplePay"": true
}

Rules:
- Extract amount as decimal number (no currency symbol)
- Extract currency code (3 letters, uppercase)
- Extract card last 4 digits (only digits, no X, *, or other characters)
- Extract merchant name (clean text, no extra formatting)
- Extract location if available
- Extract date in format YYYY-MM-DD or DD/MM/YYYY
- Extract time in format HH:MM:SS or HH:MM
- Set isApplePay to true if Apple Pay is mentioned
- Return null for optional fields if not found
- Return ONLY the JSON object, no explanations or markdown";

                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = $"Extract transaction details from this text:\n\n{text}" }
                };

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = messages,
                    max_tokens = 500,
                    temperature = 0.1,
                    response_format = new { type = "json_object" }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                int maxRetries = 3;
                int retryDelay = 1000;

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var openAIResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var aiMessage = openAIResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                        if (!string.IsNullOrEmpty(aiMessage))
                        {
                            var extractedJson = JsonSerializer.Deserialize<JsonElement>(aiMessage);
                            var data = new TransactionExtractedData();

                            if (extractedJson.TryGetProperty("amount", out var amountElement) && amountElement.ValueKind == JsonValueKind.Number)
                            {
                                data.Amount = amountElement.GetDecimal();
                            }

                            if (extractedJson.TryGetProperty("currency", out var currencyElement))
                            {
                                data.Currency = currencyElement.GetString()?.ToUpper();
                            }

                            if (extractedJson.TryGetProperty("cardLast4", out var cardElement))
                            {
                                var cardValue = cardElement.GetString();
                                if (!string.IsNullOrEmpty(cardValue))
                                {
                                    data.CardLast4 = new string(cardValue.Where(char.IsDigit).ToArray());
                                }
                            }

                            if (extractedJson.TryGetProperty("merchant", out var merchantElement))
                            {
                                data.Merchant = merchantElement.GetString();
                            }

                            if (extractedJson.TryGetProperty("location", out var locationElement))
                            {
                                data.Location = locationElement.GetString();
                            }

                            if (extractedJson.TryGetProperty("dateText", out var dateElement))
                            {
                                data.DateText = dateElement.GetString();
                            }

                            if (extractedJson.TryGetProperty("timeText", out var timeElement))
                            {
                                data.TimeText = timeElement.GetString();
                            }

                            if (extractedJson.TryGetProperty("description", out var descElement))
                            {
                                data.Description = descElement.GetString();
                            }

                            if (extractedJson.TryGetProperty("category", out var categoryElement) && categoryElement.ValueKind != JsonValueKind.Null)
                            {
                                data.Category = categoryElement.GetString();
                            }

                            if (extractedJson.TryGetProperty("isApplePay", out var applePayElement))
                            {
                                data.IsApplePay = applePayElement.GetBoolean();
                            }

                            if (data.Amount > 0)
                            {
                                return data;
                            }
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < maxRetries - 1)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        continue;
                    }
                    else
                    {
                        _logger.LogError($"OpenAI API error: {response.StatusCode} - {responseContent}");
                        return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI extraction");
                return null;
            }
        }

        private async Task<BankAccount?> FindBankAccountByCardLast4Async(string cardLast4, string userId)
        {
            var accounts = await _context.BankAccounts
                .Where(ba => ba.UserId == userId && ba.IsActive)
                .ToListAsync();

            return accounts.FirstOrDefault(ba =>
                (!string.IsNullOrEmpty(ba.Iban) && ba.Iban.EndsWith(cardLast4)) ||
                (!string.IsNullOrEmpty(ba.AccountNumber) && ba.AccountNumber.EndsWith(cardLast4)));
        }

        private DateTime ParseTransactionDateTime(string? dateText, string? timeText)
        {
            var now = DateTime.UtcNow;
            var date = now.Date;
            var time = now.TimeOfDay;

            if (!string.IsNullOrEmpty(dateText))
            {
                if (DateTime.TryParse(dateText, out var parsedDate))
                {
                    date = parsedDate.Date;
                }
                else if (dateText.Contains('/'))
                {
                    var parts = dateText.Split('/');
                    if (parts.Length >= 3 && int.TryParse(parts[0], out var day) &&
                        int.TryParse(parts[1], out var month) && int.TryParse(parts[2], out var year))
                    {
                        date = new DateTime(year < 100 ? 2000 + year : year, month, day);
                    }
                }
            }

            if (!string.IsNullOrEmpty(timeText))
            {
                if (TimeSpan.TryParse(timeText, out var parsedTime))
                {
                    time = parsedTime;
                }
                else if (timeText.Contains(':'))
                {
                    var parts = timeText.Split(':');
                    if (parts.Length >= 2 && int.TryParse(parts[0], out var hour) &&
                        int.TryParse(parts[1], out var minute))
                    {
                        time = new TimeSpan(hour, minute, parts.Length > 2 && int.TryParse(parts[2], out var second) ? second : 0);
                    }
                }
            }

            return date.Add(time);
        }

        private string GetSystemPrompt()
        {
            return @"You are an AI financial assistant for UtilityHub360. Help users with financial questions and transaction analysis.";
        }
    }

    internal class TransactionExtractedData
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? CardLast4 { get; set; }
        public string? Merchant { get; set; }
        public string? Location { get; set; }
        public string? DateText { get; set; }
        public string? TimeText { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsApplePay { get; set; }
    }
}

