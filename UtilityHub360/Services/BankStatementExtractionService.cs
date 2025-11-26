using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace UtilityHub360.Services
{
    public class BankStatementExtractionService : IBankStatementExtractionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIAgentService _aiAgentService;
        private readonly IOcrService _ocrService;
        private readonly ILogger<BankStatementExtractionService> _logger;
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _openAISettings;

        public BankStatementExtractionService(
            ApplicationDbContext context,
            IAIAgentService aiAgentService,
            IOcrService ocrService,
            ILogger<BankStatementExtractionService> logger,
            OpenAISettings openAISettings)
        {
            _context = context;
            _aiAgentService = aiAgentService;
            _ocrService = ocrService;
            _logger = logger;
            _openAISettings = openAISettings;
            _httpClient = new HttpClient();
            
            if (!string.IsNullOrEmpty(_openAISettings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAISettings.ApiKey}");
            }
        }

        public async Task<ApiResponse<ExtractBankStatementResponseDto>> ExtractFromFileAsync(
            Stream fileStream, 
            string fileName, 
            string bankAccountId, 
            string userId)
        {
            try
            {
                // Verify bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Bank account not found or does not belong to user");
                }

                var fileExtension = Path.GetExtension(fileName).ToLower();
                string extractedText = string.Empty;
                ExtractBankStatementResponseDto result;

                if (fileExtension == ".csv")
                {
                    // Extract text from CSV
                    using var reader = new StreamReader(fileStream, Encoding.UTF8);
                    extractedText = await reader.ReadToEndAsync();
                    result = await ExtractFromCSVAsync(extractedText, fileName);
                }
                else if (fileExtension == ".pdf")
                {
                    // Extract text from PDF using AI (OpenAI Vision API for PDFs or text extraction)
                    extractedText = await ExtractTextFromPDFAsync(fileStream);
                    result = await ExtractFromPDFTextAsync(extractedText, fileName);
                }
                else
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Unsupported file format: {fileExtension}. Only CSV and PDF files are supported.");
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Could not extract text from file. The file may be empty or corrupted.");
                }

                // Only use AI enhancement if we got no results from parsing
                // CSV parsing is reliable, so we prefer it over AI
                if (result.StatementItems == null || result.StatementItems.Count == 0)
                {
                    _logger.LogInformation("No transactions found in initial parsing, trying AI enhancement");
                    result = await EnhanceExtractionWithAIAsync(extractedText, result, fileName);
                }
                else
                {
                    _logger.LogInformation($"Found {result.StatementItems.Count} transactions from parsing, skipping AI enhancement");
                }

                result.ImportFormat = fileExtension == ".csv" ? "CSV" : "PDF";
                result.ImportSource = fileName;
                result.ExtractedText = extractedText; // Include for debugging

                return ApiResponse<ExtractBankStatementResponseDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting bank statement from file");
                return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Failed to extract bank statement: {ex.Message}");
            }
        }

        private async Task<ExtractBankStatementResponseDto> ExtractFromCSVAsync(string csvText, string fileName)
        {
            var result = new ExtractBankStatementResponseDto();
            var lines = csvText.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            
            if (lines.Count < 2)
            {
                return result;
            }

            // Parse CSV header
            var headers = ParseCSVLine(lines[0]).Select(h => h.Trim().ToLower()).ToList();
            
            var dateIndex = headers.FindIndex(h => h.Contains("date"));
            var amountIndex = headers.FindIndex(h => h.Contains("amount"));
            var typeIndex = headers.FindIndex(h => h.Contains("type") || h.Contains("debit") || h.Contains("credit"));
            var descIndex = headers.FindIndex(h => h.Contains("description") || h.Contains("details") || h.Contains("memo"));
            var refIndex = headers.FindIndex(h => h.Contains("reference") || h.Contains("ref"));
            var balanceIndex = headers.FindIndex(h => h.Contains("balance"));

            if (dateIndex == -1 || amountIndex == -1)
            {
                return result;
            }

            var items = new List<BankStatementItemImportDto>();
            var dates = new List<DateTime>();

            for (int i = 1; i < lines.Count; i++)
            {
                var values = ParseCSVLine(lines[i]);
                if (values.Count <= Math.Max(dateIndex, amountIndex)) continue;

                // Try parsing date with multiple formats
                DateTime transactionDate;
                var dateStr = values[dateIndex].Trim();
                
                // Try ISO format first (YYYY-MM-DD)
                if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out transactionDate) ||
                    DateTime.TryParse(dateStr, out transactionDate))
                {
                    dates.Add(transactionDate);
                    
                    // Handle amount with currency symbols and commas (e.g., "35,000.00" or "$1,500.00" or "35,000.00 (PHP)")
                    var amountStr = values[amountIndex].Replace(",", "").Replace("$", "").Replace("PHP", "").Replace("₱", "").Replace("(", "").Replace(")", "").Trim();
                    if (decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount) && amount > 0)
                    {
                        var item = new BankStatementItemImportDto
                        {
                            TransactionDate = transactionDate,
                            Amount = Math.Abs(amount),
                            TransactionType = DetermineTransactionType(values, typeIndex, amount),
                            Description = descIndex >= 0 && descIndex < values.Count ? values[descIndex].Trim() : "",
                            ReferenceNumber = refIndex >= 0 && refIndex < values.Count ? values[refIndex].Trim() : "",
                            BalanceAfterTransaction = balanceIndex >= 0 && balanceIndex < values.Count && 
                                decimal.TryParse(values[balanceIndex].Replace(",", "").Replace("$", "").Replace("PHP", "").Replace("₱", "").Replace("(", "").Replace(")", "").Trim(), 
                                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var balance) ? balance : 0
                        };
                        items.Add(item);
                    }
                }
            }

            result.StatementItems = items;
            if (dates.Any())
            {
                result.StatementStartDate = dates.Min();
                result.StatementEndDate = dates.Max();
            }
            if (items.Any() && items[0].BalanceAfterTransaction > 0)
            {
                result.OpeningBalance = items[0].BalanceAfterTransaction;
            }
            if (items.Any() && items.Last().BalanceAfterTransaction > 0)
            {
                result.ClosingBalance = items.Last().BalanceAfterTransaction;
            }

            return result;
        }

        private async Task<string> ExtractTextFromPDFAsync(Stream pdfStream)
        {
            try
            {
                pdfStream.Position = 0;
                var textBuilder = new StringBuilder();
                
                // Use PdfPig for text extraction (better than iTextSharp.LGPLv2.Core)
                try
                {
                    using (var document = PdfDocument.Open(pdfStream))
                    {
                        foreach (var page in document.GetPages())
                        {
                            try
                            {
                                // Extract all text from the page
                                var words = page.GetWords();
                                if (words != null && words.Any())
                                {
                                    var pageText = string.Join(" ", words.Select(w => w.Text));
                                    if (!string.IsNullOrWhiteSpace(pageText))
                                    {
                                        textBuilder.AppendLine(pageText);
                                    }
                                }
                            }
                            catch (Exception pageEx)
                            {
                                _logger.LogWarning($"Error extracting text from PDF page {page.Number}: {pageEx.Message}");
                            }
                        }
                    }
                    
                    var extractedText = textBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(extractedText))
                    {
                        _logger.LogInformation($"Successfully extracted {extractedText.Length} characters from PDF using PdfPig");
                        return extractedText;
                    }
                }
                catch (Exception pdfEx)
                {
                    _logger.LogWarning($"PdfPig extraction failed: {pdfEx.Message}. Trying OCR fallback.");
                    
                    // Fallback to OCR service for image-based (scanned) PDFs
                    try
                    {
                        pdfStream.Position = 0;
                        var ocrResult = await _ocrService.ProcessPdfAsync(pdfStream);
                        
                        if (ocrResult != null && !string.IsNullOrWhiteSpace(ocrResult.FullText))
                        {
                            _logger.LogInformation($"Successfully extracted {ocrResult.FullText.Length} characters from PDF using OCR ({ocrResult.Provider})");
                            return ocrResult.FullText;
                        }
                    }
                    catch (Exception ocrEx)
                    {
                        _logger.LogError(ocrEx, "OCR fallback also failed");
                    }
                    
                    throw new Exception($"PDF text extraction failed. The PDF may be image-based (scanned) and requires OCR. Error: {pdfEx.Message}");
                }
                
                throw new Exception("No text could be extracted from PDF. The PDF may be corrupted or image-based (scanned).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF");
                throw;
            }
        }

        private async Task<ExtractBankStatementResponseDto> ExtractFromPDFTextAsync(string pdfText, string fileName)
        {
            // Basic extraction from PDF text (similar to CSV but more flexible)
            var result = new ExtractBankStatementResponseDto();
            
            // Use regex to find dates and amounts
            var datePattern = @"(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})";
            var amountPattern = @"([+-]?\$?\d{1,3}(?:,\d{3})*(?:\.\d{2})?)";
            
            var dates = new List<DateTime>();
            var items = new List<BankStatementItemImportDto>();
            
            var lines = pdfText.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l));
            
            foreach (var line in lines)
            {
                var dateMatch = Regex.Match(line, datePattern);
                var amountMatches = Regex.Matches(line, amountPattern);
                
                if (dateMatch.Success && amountMatches.Count > 0)
                {
                    if (DateTime.TryParse(dateMatch.Value, out var transactionDate))
                    {
                        dates.Add(transactionDate);
                        
                        foreach (Match amountMatch in amountMatches)
                        {
                            var amountStr = amountMatch.Value.Replace("$", "").Replace(",", "").Trim();
                            if (decimal.TryParse(amountStr, out var amount) && amount > 0 && amount < 1000000)
                            {
                                var item = new BankStatementItemImportDto
                                {
                                    TransactionDate = transactionDate,
                                    Amount = Math.Abs(amount),
                                    TransactionType = amount < 0 ? "DEBIT" : "CREDIT",
                                    Description = line.Length > 100 ? line.Substring(0, 100) : line,
                                    ReferenceNumber = "",
                                    BalanceAfterTransaction = 0
                                };
                                items.Add(item);
                                break; // Take first reasonable amount
                            }
                        }
                    }
                }
            }

            result.StatementItems = items.DistinctBy(i => new { i.TransactionDate, i.Amount }).ToList();
            if (dates.Any())
            {
                result.StatementStartDate = dates.Min();
                result.StatementEndDate = dates.Max();
            }

            return result;
        }

        private async Task<ExtractBankStatementResponseDto> EnhanceExtractionWithAIAsync(
            string extractedText, 
            ExtractBankStatementResponseDto initialResult, 
            string fileName)
        {
            try
            {
                // If we already have good results from CSV parsing, skip AI enhancement
                // Only use AI if we have no transactions or very few
                if (initialResult.StatementItems != null && initialResult.StatementItems.Count > 0)
                {
                    _logger.LogInformation($"Skipping AI enhancement - already have {initialResult.StatementItems.Count} transactions from CSV parsing");
                    return initialResult;
                }

                if (string.IsNullOrEmpty(_openAISettings.ApiKey))
                {
                    _logger.LogWarning("OpenAI API key not configured. Skipping AI enhancement.");
                    return initialResult;
                }

                var prompt = $@"CRITICAL: Extract transactions EXACTLY as they appear in the bank statement text below. DO NOT generate sample data. DO NOT use dates from 2023. Use ONLY the actual data provided.

Bank Statement Text:
{extractedText.Substring(0, Math.Min(extractedText.Length, 15000))}

Return a JSON object with this exact structure. Extract ONLY the transactions that appear in the text above:

{{
  ""statementName"": ""string (e.g., November 2025 Statement)"",
  ""statementStartDate"": ""YYYY-MM-DD"",
  ""statementEndDate"": ""YYYY-MM-DD"",
  ""openingBalance"": number,
  ""closingBalance"": number,
  ""transactions"": [
    {{
      ""transactionDate"": ""YYYY-MM-DD"",
      ""amount"": number (always positive, use EXACT amount from statement),
      ""transactionType"": ""DEBIT"" or ""CREDIT"",
      ""description"": ""string (use EXACT description from statement)"",
      ""referenceNumber"": ""string (optional)"",
      ""merchant"": ""string (optional, extract from description if possible)"",
      ""category"": ""string (optional)"",
      ""balanceAfterTransaction"": number (use EXACT balance from statement)
    }}
  ]
}}

IMPORTANT RULES:
1. Use ONLY the dates, amounts, and descriptions that appear in the text above
2. Do NOT generate sample or example data
3. If a field is not in the statement, leave it empty or use empty string
4. Preserve exact amounts and dates from the statement
5. Return ONLY valid JSON, no other text or explanations";

                var messages = new List<object>
                {
                    new { role = "system", content = "You are a financial data extraction expert. Extract bank statement transactions accurately and return only valid JSON." },
                    new { role = "user", content = prompt }
                };

                var openAIRequest = new
                {
                    model = "gpt-4o-mini",
                    messages = messages,
                    temperature = 0.1, // Low temperature for accuracy
                    max_tokens = 4000,
                    response_format = new { type = "json_object" }
                };

                var json = JsonSerializer.Serialize(openAIRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"OpenAI API error: {httpResponse.StatusCode} - {responseContent}");
                    return initialResult; // Return initial extraction if AI fails
                }

                var openAIResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var choices = openAIResponse.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var aiResponseText = message.GetProperty("content").GetString() ?? "";

                // Parse AI response
                var aiResult = JsonSerializer.Deserialize<JsonElement>(aiResponseText);
                
                var enhancedResult = new ExtractBankStatementResponseDto
                {
                    StatementName = aiResult.TryGetProperty("statementName", out var name) ? name.GetString() ?? fileName : fileName,
                    StatementStartDate = aiResult.TryGetProperty("statementStartDate", out var startDate) && 
                        DateTime.TryParse(startDate.GetString(), out var sd) ? sd : initialResult.StatementStartDate,
                    StatementEndDate = aiResult.TryGetProperty("statementEndDate", out var endDate) && 
                        DateTime.TryParse(endDate.GetString(), out var ed) ? ed : initialResult.StatementEndDate,
                    OpeningBalance = aiResult.TryGetProperty("openingBalance", out var openBal) ? 
                        openBal.GetDecimal() : initialResult.OpeningBalance ?? 0,
                    ClosingBalance = aiResult.TryGetProperty("closingBalance", out var closeBal) ? 
                        closeBal.GetDecimal() : initialResult.ClosingBalance ?? 0,
                    ImportFormat = initialResult.ImportFormat,
                    ImportSource = initialResult.ImportSource,
                    StatementItems = new List<BankStatementItemImportDto>()
                };

                if (aiResult.TryGetProperty("transactions", out var transactions))
                {
                    foreach (var trans in transactions.EnumerateArray())
                    {
                        var item = new BankStatementItemImportDto
                        {
                            TransactionDate = DateTime.Parse(trans.GetProperty("transactionDate").GetString() ?? DateTime.Now.ToString("yyyy-MM-dd")),
                            Amount = trans.GetProperty("amount").GetDecimal(),
                            TransactionType = trans.GetProperty("transactionType").GetString() ?? "DEBIT",
                            Description = trans.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                            ReferenceNumber = trans.TryGetProperty("referenceNumber", out var refNum) ? refNum.GetString() ?? "" : "",
                            Merchant = trans.TryGetProperty("merchant", out var merch) ? merch.GetString() ?? "" : "",
                            Category = trans.TryGetProperty("category", out var cat) ? cat.GetString() ?? "" : "",
                            BalanceAfterTransaction = trans.TryGetProperty("balanceAfterTransaction", out var bal) ? bal.GetDecimal() : 0
                        };
                        enhancedResult.StatementItems.Add(item);
                    }
                }

                // Only use AI result if initial result was empty
                // CSV parsing is more reliable, so prefer it over AI
                if (enhancedResult.StatementItems.Any() && (!initialResult.StatementItems.Any() || initialResult.StatementItems.Count == 0))
                {
                    _logger.LogInformation($"Using AI extracted {enhancedResult.StatementItems.Count} transactions (initial had none)");
                    return enhancedResult;
                }

                _logger.LogInformation($"Using initial parsed result with {initialResult.StatementItems?.Count ?? 0} transactions (skipping AI enhancement)");
                return initialResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing extraction with AI");
                return initialResult; // Return initial extraction if AI fails
            }
        }

        private List<string> ParseCSVLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }
            result.Add(current.ToString());
            return result;
        }

        private string DetermineTransactionType(List<string> values, int typeIndex, decimal amount)
        {
            if (typeIndex >= 0 && typeIndex < values.Count)
            {
                var typeStr = values[typeIndex].ToUpper();
                if (typeStr.Contains("CREDIT") || typeStr.Contains("DEPOSIT") || typeStr.Contains("INCOME"))
                    return "CREDIT";
                if (typeStr.Contains("DEBIT") || typeStr.Contains("WITHDRAWAL") || typeStr.Contains("PAYMENT"))
                    return "DEBIT";
            }
            return amount < 0 ? "DEBIT" : "CREDIT";
        }
    }
}

