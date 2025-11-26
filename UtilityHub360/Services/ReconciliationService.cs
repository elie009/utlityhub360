using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig;
using System.Linq;

namespace UtilityHub360.Services
{
    public class ReconciliationService : IReconciliationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBankStatementExtractionService _extractionService;
        private readonly IAIAgentService _aiAgentService;
        private readonly IOcrService _ocrService;
        private readonly IBankAccountService _bankAccountService;
        private readonly ILogger<ReconciliationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _openAISettings;

        public ReconciliationService(
            ApplicationDbContext context,
            IBankStatementExtractionService extractionService,
            IAIAgentService aiAgentService,
            IOcrService ocrService,
            IBankAccountService bankAccountService,
            ILogger<ReconciliationService> logger,
            OpenAISettings openAISettings)
        {
            _context = context;
            _extractionService = extractionService;
            _aiAgentService = aiAgentService;
            _ocrService = ocrService;
            _bankAccountService = bankAccountService;
            _logger = logger;
            _openAISettings = openAISettings;
            _httpClient = new HttpClient();
            
            if (!string.IsNullOrEmpty(_openAISettings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAISettings.ApiKey}");
            }
        }

        // ==================== BANK STATEMENT OPERATIONS ====================

        public async Task<ApiResponse<ExtractBankStatementResponseDto>> ExtractBankStatementFromFileAsync(
            Stream fileStream, 
            string fileName, 
            string bankAccountId, 
            string userId)
        {
            try
            {
                // Use the extraction service which handles both CSV and PDF
                var result = await _extractionService.ExtractFromFileAsync(fileStream, fileName, bankAccountId, userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting bank statement from file");
                return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Failed to extract bank statement: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExtractBankStatementResponseDto>> AnalyzePDFWithAIAsync(
            Stream pdfStream, 
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

                // Step 1: Extract text from PDF - try multiple methods
                pdfStream.Position = 0;
                string extractedText = string.Empty;
                string extractionMethod = "Unknown";
                
                // Check if stream is readable
                if (pdfStream.Length == 0)
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("PDF file is empty or could not be read.");
                }
                
                _logger.LogInformation($"Attempting to extract text from PDF: {fileName}, Size: {pdfStream.Length} bytes");
                
                // Try PdfPig first for direct text extraction (faster for text-based PDFs)
                try
                {
                    pdfStream.Position = 0;
                    var textBuilder = new StringBuilder();
                    
                    using (var document = PdfDocument.Open(pdfStream))
                    {
                        var pageCount = document.NumberOfPages;
                        _logger.LogInformation($"PDF has {pageCount} pages");
                        
                        foreach (var page in document.GetPages())
                        {
                            try
                            {
                                var words = page.GetWords();
                                if (words != null && words.Any())
                                {
                                    var pageText = string.Join(" ", words.Select(w => w.Text));
                                    if (!string.IsNullOrWhiteSpace(pageText))
                                    {
                                        textBuilder.AppendLine(pageText);
                                        _logger.LogInformation($"Extracted {pageText.Length} characters from page {page.Number}");
                                    }
                                }
                            }
                            catch (Exception pageEx)
                            {
                                _logger.LogWarning($"Error extracting text from PDF page {page.Number}: {pageEx.Message}");
                            }
                        }
                    }
                    
                    extractedText = textBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(extractedText))
                    {
                        extractionMethod = "PdfPig";
                        _logger.LogInformation($"Successfully extracted {extractedText.Length} characters from PDF using PdfPig");
                    }
                }
                catch (Exception pdfPigEx)
                {
                    _logger.LogWarning($"PdfPig extraction failed: {pdfPigEx.Message}. Trying OCR fallback.");
                }
                
                // Fallback: Try OCR service for image-based (scanned) PDFs
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    try
                    {
                        pdfStream.Position = 0;
                        _logger.LogInformation("Attempting OCR extraction...");
                        var ocrResult = await _ocrService.ProcessPdfAsync(pdfStream);
                        
                        if (ocrResult != null && !string.IsNullOrWhiteSpace(ocrResult.FullText))
                        {
                            extractedText = ocrResult.FullText;
                            extractionMethod = ocrResult.Provider ?? "OCR";
                            _logger.LogInformation($"Successfully extracted {extractedText.Length} characters from PDF using {extractionMethod}");
                        }
                        else
                        {
                            _logger.LogWarning($"OCR returned empty text. Confidence: {ocrResult?.Confidence ?? 0}");
                        }
                    }
                    catch (Exception ocrEx)
                    {
                        _logger.LogError(ocrEx, $"OCR service failed: {ocrEx.Message}");
                    }
                }
                
                // If still no text, return error with helpful message
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    _logger.LogError($"Failed to extract text from PDF after trying both PdfPig and OCR methods. File: {fileName}, Size: {pdfStream.Length} bytes");
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult(
                        "Could not extract text from PDF. Possible reasons:\n" +
                        "1. The PDF is password-protected (please remove password)\n" +
                        "2. The PDF is corrupted\n" +
                        "3. The PDF contains only images without text (scanned document - OCR may not be configured)\n" +
                        "4. The PDF format is not supported\n\n" +
                        "Please try:\n" +
                        "- Converting the PDF to a CSV file\n" +
                        "- Ensuring the PDF is not password-protected\n" +
                        "- Using a text-based PDF (not just scanned images)");
                }

                // Step 2: Use AI Agent to analyze and extract transactions
                if (string.IsNullOrEmpty(_openAISettings.ApiKey))
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("OpenAI API key not configured. AI analysis is not available.");
                }

                var prompt = $@"You are a financial data extraction expert. Analyze this bank statement PDF text and extract ALL transactions accurately.

CRITICAL INSTRUCTIONS:
1. Extract transactions EXACTLY as they appear in the text below
2. DO NOT generate sample data or example transactions
3. DO NOT use dates from previous years (2023, 2024) unless they appear in the text
4. Use ONLY the actual data provided in the bank statement text
5. Preserve exact amounts, dates, and descriptions from the statement
6. If a field is missing, leave it empty or use empty string

Bank Statement Text (first 20000 characters):
{extractedText.Substring(0, Math.Min(extractedText.Length, 20000))}

Extract ALL transactions and return a JSON object with this exact structure:

{{
  ""statementName"": ""string (extract from statement or use filename)"",
  ""statementStartDate"": ""YYYY-MM-DD (earliest transaction date)"",
  ""statementEndDate"": ""YYYY-MM-DD (latest transaction date)"",
  ""openingBalance"": number (if available in statement),
  ""closingBalance"": number (if available in statement),
  ""transactions"": [
    {{
      ""transactionDate"": ""YYYY-MM-DD (use EXACT date from statement)"",
      ""amount"": number (always positive, use EXACT amount from statement),
      ""transactionType"": ""DEBIT"" or ""CREDIT"" (based on statement),
      ""description"": ""string (use EXACT description from statement)"",
      ""referenceNumber"": ""string (if available in statement)"",
      ""merchant"": ""string (extract from description if possible)"",
      ""category"": ""string (optional, infer from description)"",
      ""balanceAfterTransaction"": number (if available in statement)
    }}
  ]
}}

IMPORTANT: 
- Extract ONLY transactions that appear in the text above
- Use exact dates, amounts, and descriptions from the statement
- Do NOT invent or generate sample data
- Return ONLY valid JSON, no explanations or additional text";

                var messages = new List<object>
                {
                    new { role = "system", content = "You are a financial data extraction expert. Extract bank statement transactions accurately from the provided text. Return only valid JSON with the exact structure specified. Never generate sample or example data." },
                    new { role = "user", content = prompt }
                };

                var openAIRequest = new
                {
                    model = "gpt-4o-mini",
                    messages = messages,
                    temperature = 0.1, // Very low temperature for accuracy
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
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult(
                        $"AI analysis failed: {httpResponse.StatusCode}. Please try again or use manual entry.");
                }

                var openAIResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var choices = openAIResponse.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var aiResponseText = message.GetProperty("content").GetString() ?? "";

                // Parse AI response
                var aiResult = JsonSerializer.Deserialize<JsonElement>(aiResponseText);
                
                var result = new ExtractBankStatementResponseDto
                {
                    StatementName = aiResult.TryGetProperty("statementName", out var name) ? name.GetString() ?? fileName : fileName,
                    StatementStartDate = aiResult.TryGetProperty("statementStartDate", out var startDate) && 
                        DateTime.TryParse(startDate.GetString(), out var sd) ? sd : null,
                    StatementEndDate = aiResult.TryGetProperty("statementEndDate", out var endDate) && 
                        DateTime.TryParse(endDate.GetString(), out var ed) ? ed : null,
                    OpeningBalance = aiResult.TryGetProperty("openingBalance", out var openBal) ? 
                        openBal.GetDecimal() : null,
                    ClosingBalance = aiResult.TryGetProperty("closingBalance", out var closeBal) ? 
                        closeBal.GetDecimal() : null,
                    ImportFormat = "PDF",
                    ImportSource = fileName,
                    StatementItems = new List<BankStatementItemImportDto>(),
                    ExtractedText = extractedText.Substring(0, Math.Min(extractedText.Length, 5000)) // Include first 5000 chars for debugging
                };

                if (aiResult.TryGetProperty("transactions", out var transactions))
                {
                    foreach (var trans in transactions.EnumerateArray())
                    {
                        try
                        {
                            var transactionDateStr = trans.GetProperty("transactionDate").GetString();
                            if (string.IsNullOrEmpty(transactionDateStr) || !DateTime.TryParse(transactionDateStr, out var transactionDate))
                            {
                                _logger.LogWarning($"Skipping transaction with invalid date: {transactionDateStr}");
                                continue;
                            }

                            var item = new BankStatementItemImportDto
                            {
                                TransactionDate = transactionDate,
                                Amount = trans.GetProperty("amount").GetDecimal(),
                                TransactionType = trans.GetProperty("transactionType").GetString() ?? "DEBIT",
                                Description = trans.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                                ReferenceNumber = trans.TryGetProperty("referenceNumber", out var refNum) ? refNum.GetString() ?? "" : "",
                                Merchant = trans.TryGetProperty("merchant", out var merch) ? merch.GetString() ?? "" : "",
                                Category = trans.TryGetProperty("category", out var cat) ? cat.GetString() ?? "" : "",
                                BalanceAfterTransaction = trans.TryGetProperty("balanceAfterTransaction", out var bal) ? bal.GetDecimal() : 0
                            };
                            result.StatementItems.Add(item);
                        }
                        catch (Exception itemEx)
                        {
                            _logger.LogWarning($"Error parsing transaction item: {itemEx.Message}");
                            // Continue with next transaction
                        }
                    }
                }

                if (result.StatementItems.Count == 0)
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult(
                        "No transactions found in PDF. The PDF format may not be supported, or the AI could not extract valid transactions.");
                }

                _logger.LogInformation($"AI successfully extracted {result.StatementItems.Count} transactions from PDF");
                return ApiResponse<ExtractBankStatementResponseDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing PDF with AI");
                return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Failed to analyze PDF with AI: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankStatementDto>> ImportBankStatementAsync(ImportBankStatementDto importDto, string userId)
        {
            try
            {
                // Verify bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == importDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<BankStatementDto>.ErrorResult("Bank account not found or does not belong to user");
                }

                // Create bank statement
                var bankStatement = new BankStatement
                {
                    UserId = userId,
                    BankAccountId = importDto.BankAccountId,
                    StatementName = importDto.StatementName,
                    StatementStartDate = importDto.StatementStartDate,
                    StatementEndDate = importDto.StatementEndDate,
                    OpeningBalance = importDto.OpeningBalance,
                    ClosingBalance = importDto.ClosingBalance,
                    ImportFormat = importDto.ImportFormat ?? "CSV",
                    ImportSource = importDto.ImportSource,
                    TotalTransactions = importDto.StatementItems.Count,
                    MatchedTransactions = 0,
                    UnmatchedTransactions = importDto.StatementItems.Count,
                    IsReconciled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BankStatements.Add(bankStatement);
                await _context.SaveChangesAsync();

                // Create statement items
                var statementItems = importDto.StatementItems.Select(item => new BankStatementItem
                {
                    BankStatementId = bankStatement.Id,
                    TransactionDate = item.TransactionDate,
                    Amount = item.Amount,
                    TransactionType = item.TransactionType.ToUpper(),
                    Description = item.Description,
                    ReferenceNumber = item.ReferenceNumber,
                    Merchant = item.Merchant,
                    Category = item.Category,
                    BalanceAfterTransaction = item.BalanceAfterTransaction,
                    IsMatched = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                _context.BankStatementItems.AddRange(statementItems);
                await _context.SaveChangesAsync();

                // Try auto-matching
                await AutoMatchStatementItemsAsync(bankStatement.Id, userId);

                // Auto-create transactions from unmatched items
                await CreateTransactionsFromUnmatchedItemsAsync(bankStatement.Id, userId);

                // Reload with items
                var result = await GetBankStatementAsync(bankStatement.Id, userId);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<BankStatementDto>.ErrorResult($"Failed to import bank statement: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankStatementDto>> GetBankStatementAsync(string statementId, string userId)
        {
            try
            {
                // For detail view, we need StatementItems, so include them
                var statement = await _context.BankStatements
                    .AsNoTracking() // No change tracking for read-only query
                    .Include(s => s.StatementItems)
                    .FirstOrDefaultAsync(s => s.Id == statementId && s.UserId == userId);

                if (statement == null)
                {
                    return ApiResponse<BankStatementDto>.ErrorResult("Bank statement not found");
                }

                var dto = MapToBankStatementDto(statement);
                return ApiResponse<BankStatementDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankStatementDto>.ErrorResult($"Failed to get bank statement: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankStatementDto>>> GetBankStatementsAsync(string bankAccountId, string userId)
        {
            try
            {
                // Optimized: Direct projection to DTOs without loading StatementItems (not needed for list view)
                // Use simpler approach - load entities first then map (more reliable for remote databases)
                var statements = await _context.BankStatements
                    .AsNoTracking()
                    .Where(s => s.BankAccountId == bankAccountId && s.UserId == userId)
                    .OrderByDescending(s => s.StatementEndDate)
                    .ToListAsync();

                // Map to DTOs in memory (avoids complex SQL generation issues)
                var dtos = statements.Select(s => new BankStatementDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    BankAccountId = s.BankAccountId,
                    StatementName = s.StatementName,
                    StatementStartDate = s.StatementStartDate,
                    StatementEndDate = s.StatementEndDate,
                    OpeningBalance = s.OpeningBalance,
                    ClosingBalance = s.ClosingBalance,
                    ImportFormat = s.ImportFormat,
                    ImportSource = s.ImportSource,
                    TotalTransactions = s.TotalTransactions,
                    MatchedTransactions = s.MatchedTransactions,
                    UnmatchedTransactions = s.UnmatchedTransactions,
                    IsReconciled = s.IsReconciled,
                    ReconciledAt = s.ReconciledAt,
                    ReconciledBy = s.ReconciledBy,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    StatementItems = null // Don't load items in list view
                }).ToList();

                return ApiResponse<List<BankStatementDto>>.SuccessResult(dtos);
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                // Handle SQL-specific errors
                if (sqlEx.Number == 208) // Invalid object name
                {
                    return ApiResponse<List<BankStatementDto>>.ErrorResult($"Table 'BankStatements' does not exist. Please run the migration script: run_migration_direct_FIXED.sql");
                }
                if (sqlEx.Number == -2) // Timeout
                {
                    return ApiResponse<List<BankStatementDto>>.ErrorResult($"Database query timeout. The query took too long. Possible causes: missing indexes, network latency, or table locks. Error: {sqlEx.Message}");
                }
                return ApiResponse<List<BankStatementDto>>.ErrorResult($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankStatementDto>>.ErrorResult($"Failed to get bank statements: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBankStatementAsync(string statementId, string userId)
        {
            try
            {
                var statement = await _context.BankStatements
                    .FirstOrDefaultAsync(s => s.Id == statementId && s.UserId == userId);

                if (statement == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bank statement not found");
                }

                _context.BankStatements.Remove(statement);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete bank statement: {ex.Message}");
            }
        }

        // ==================== RECONCILIATION OPERATIONS ====================

        public async Task<ApiResponse<ReconciliationDto>> CreateReconciliationAsync(CreateReconciliationDto createDto, string userId)
        {
            try
            {
                // Verify bank account exists and belongs to user
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == createDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<ReconciliationDto>.ErrorResult("Bank account not found or does not belong to user");
                }

                // Verify bank statement if provided
                if (!string.IsNullOrEmpty(createDto.BankStatementId))
                {
                    var statement = await _context.BankStatements
                        .FirstOrDefaultAsync(s => s.Id == createDto.BankStatementId && s.UserId == userId);

                    if (statement == null)
                    {
                        return ApiResponse<ReconciliationDto>.ErrorResult("Bank statement not found");
                    }
                }

                // Get book balance (system balance) for the reconciliation date
                var bookBalance = bankAccount.CurrentBalance;

                // Get statement balance (from statement if provided, otherwise use book balance)
                decimal statementBalance = bookBalance;
                if (!string.IsNullOrEmpty(createDto.BankStatementId))
                {
                    var statement = await _context.BankStatements
                        .FirstOrDefaultAsync(s => s.Id == createDto.BankStatementId);
                    statementBalance = statement?.ClosingBalance ?? bookBalance;
                }

                // Get transactions for the period
                var startDate = createDto.ReconciliationDate.Date;
                var endDate = startDate.AddDays(1).AddTicks(-1);

                var transactions = await _context.Payments
                    .Where(p => p.BankAccountId == createDto.BankAccountId &&
                               p.TransactionDate >= startDate &&
                               p.TransactionDate <= endDate &&
                               p.IsBankTransaction)
                    .ToListAsync();

                var reconciliation = new Reconciliation
                {
                    UserId = userId,
                    BankAccountId = createDto.BankAccountId,
                    BankStatementId = createDto.BankStatementId,
                    ReconciliationName = createDto.ReconciliationName,
                    ReconciliationDate = createDto.ReconciliationDate,
                    BookBalance = bookBalance,
                    StatementBalance = statementBalance,
                    Difference = statementBalance - bookBalance,
                    TotalTransactions = transactions.Count,
                    MatchedTransactions = 0,
                    UnmatchedTransactions = transactions.Count,
                    PendingTransactions = 0,
                    Status = "PENDING",
                    Notes = createDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Reconciliations.Add(reconciliation);
                await _context.SaveChangesAsync();

                // Try auto-matching
                await AutoMatchTransactionsAsync(reconciliation.Id, userId);

                var result = await GetReconciliationAsync(reconciliation.Id, userId);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Failed to create reconciliation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationDto>> GetReconciliationAsync(string reconciliationId, string userId)
        {
            try
            {
                var reconciliation = await _context.Reconciliations
                    .Include(r => r.Matches)
                    .ThenInclude(m => m.StatementItem)
                    .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.UserId == userId);

                if (reconciliation == null)
                {
                    return ApiResponse<ReconciliationDto>.ErrorResult("Reconciliation not found");
                }

                var dto = MapToReconciliationDto(reconciliation);
                return ApiResponse<ReconciliationDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Failed to get reconciliation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ReconciliationDto>>> GetReconciliationsAsync(string bankAccountId, string userId)
        {
            try
            {
                var reconciliations = await _context.Reconciliations
                    .Where(r => r.BankAccountId == bankAccountId && r.UserId == userId)
                    .OrderByDescending(r => r.ReconciliationDate)
                    .ToListAsync();

                var dtos = reconciliations.Select(MapToReconciliationDto).ToList();
                return ApiResponse<List<ReconciliationDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ReconciliationDto>>.ErrorResult($"Failed to get reconciliations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationDto>> AutoMatchTransactionsAsync(string reconciliationId, string userId)
        {
            try
            {
                var reconciliation = await _context.Reconciliations
                    .Include(r => r.BankStatement)
                    .ThenInclude(s => s!.StatementItems)
                    .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.UserId == userId);

                if (reconciliation == null)
                {
                    return ApiResponse<ReconciliationDto>.ErrorResult("Reconciliation not found");
                }

                // Get system transactions for the period
                var startDate = reconciliation.ReconciliationDate.Date;
                var endDate = startDate.AddDays(1).AddTicks(-1);

                var systemTransactions = await _context.Payments
                    .Where(p => p.BankAccountId == reconciliation.BankAccountId &&
                               p.TransactionDate >= startDate &&
                               p.TransactionDate <= endDate &&
                               p.IsBankTransaction)
                    .ToListAsync();

                // Get statement items if statement exists
                List<BankStatementItem>? statementItems = null;
                if (reconciliation.BankStatementId != null)
                {
                    statementItems = await _context.BankStatementItems
                        .Where(i => i.BankStatementId == reconciliation.BankStatementId && !i.IsMatched)
                        .ToListAsync();
                }

                int matchedCount = 0;

                foreach (var transaction in systemTransactions)
                {
                    // Check if already matched
                    var existingMatch = await _context.ReconciliationMatches
                        .FirstOrDefaultAsync(m => m.ReconciliationId == reconciliationId &&
                                                 m.SystemTransactionId == transaction.Id);

                    if (existingMatch != null)
                        continue;

                    // Try to find matching statement item
                    BankStatementItem? matchedItem = null;
                    if (statementItems != null)
                    {
                        matchedItem = statementItems.FirstOrDefault(item =>
                            !item.IsMatched &&
                            Math.Abs((decimal)(item.Amount - transaction.Amount)) < 0.01m &&
                            transaction.TransactionDate.HasValue &&
                            Math.Abs((item.TransactionDate.Date - transaction.TransactionDate.Value.Date).TotalDays) <= 2 &&
                            (string.IsNullOrEmpty(item.ReferenceNumber) ||
                             string.IsNullOrEmpty(transaction.Reference) ||
                             item.ReferenceNumber == transaction.Reference));
                    }

                    // Create match
                    var match = new ReconciliationMatch
                    {
                        ReconciliationId = reconciliationId,
                        SystemTransactionId = transaction.Id,
                        SystemTransactionType = "Payment",
                        StatementItemId = matchedItem?.Id,
                        MatchType = matchedItem != null ? "AUTO" : "UNMATCHED",
                        Amount = transaction.Amount,
                        TransactionDate = transaction.TransactionDate ?? transaction.ProcessedAt,
                        Description = transaction.Description,
                        MatchStatus = matchedItem != null ? "MATCHED" : "UNMATCHED",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        MatchedBy = userId
                    };

                    if (matchedItem != null)
                    {
                        matchedItem.IsMatched = true;
                        matchedItem.MatchedTransactionId = transaction.Id;
                        matchedItem.MatchedTransactionType = "Payment";
                        matchedItem.MatchedAt = DateTime.UtcNow;
                        matchedItem.MatchedBy = userId;
                        matchedItem.UpdatedAt = DateTime.UtcNow;
                    }

                    _context.ReconciliationMatches.Add(match);
                    matchedCount++;
                }

                // Update reconciliation stats
                reconciliation.MatchedTransactions = await _context.ReconciliationMatches
                    .CountAsync(m => m.ReconciliationId == reconciliationId && m.MatchStatus == "MATCHED");
                reconciliation.UnmatchedTransactions = await _context.ReconciliationMatches
                    .CountAsync(m => m.ReconciliationId == reconciliationId && m.MatchStatus == "UNMATCHED");
                reconciliation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Update statement if exists
                if (reconciliation.BankStatementId != null)
                {
                    var statement = await _context.BankStatements
                        .FirstOrDefaultAsync(s => s.Id == reconciliation.BankStatementId);
                    if (statement != null)
                    {
                        statement.MatchedTransactions = await _context.BankStatementItems
                            .CountAsync(i => i.BankStatementId == statement.Id && i.IsMatched);
                        statement.UnmatchedTransactions = statement.TotalTransactions - statement.MatchedTransactions;
                        statement.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                var result = await GetReconciliationAsync(reconciliationId, userId);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Failed to auto-match transactions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationMatchDto>> MatchTransactionAsync(MatchTransactionDto matchDto, string userId)
        {
            try
            {
                var reconciliation = await _context.Reconciliations
                    .FirstOrDefaultAsync(r => r.Id == matchDto.ReconciliationId && r.UserId == userId);

                if (reconciliation == null)
                {
                    return ApiResponse<ReconciliationMatchDto>.ErrorResult("Reconciliation not found");
                }

                // Get system transaction
                Payment? systemTransaction = null;
                if (matchDto.SystemTransactionType == "Payment")
                {
                    systemTransaction = await _context.Payments
                        .FirstOrDefaultAsync(p => p.Id == matchDto.SystemTransactionId);
                }

                if (systemTransaction == null)
                {
                    return ApiResponse<ReconciliationMatchDto>.ErrorResult("System transaction not found");
                }

                // Get statement item if provided
                BankStatementItem? statementItem = null;
                if (!string.IsNullOrEmpty(matchDto.StatementItemId))
                {
                    statementItem = await _context.BankStatementItems
                        .FirstOrDefaultAsync(i => i.Id == matchDto.StatementItemId);
                }

                // Check if match already exists
                var existingMatch = await _context.ReconciliationMatches
                    .FirstOrDefaultAsync(m => m.ReconciliationId == matchDto.ReconciliationId &&
                                             m.SystemTransactionId == matchDto.SystemTransactionId);

                if (existingMatch != null)
                {
                    // Update existing match
                    existingMatch.StatementItemId = matchDto.StatementItemId;
                    existingMatch.MatchType = matchDto.MatchType;
                    existingMatch.MatchStatus = "MATCHED";
                    existingMatch.MatchNotes = matchDto.MatchNotes;
                    existingMatch.UpdatedAt = DateTime.UtcNow;
                    existingMatch.MatchedBy = userId;

                    if (statementItem != null)
                    {
                        statementItem.IsMatched = true;
                        statementItem.MatchedTransactionId = systemTransaction.Id;
                        statementItem.MatchedTransactionType = "Payment";
                        statementItem.MatchedAt = DateTime.UtcNow;
                        statementItem.MatchedBy = userId;
                        statementItem.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // Create new match
                    var match = new ReconciliationMatch
                    {
                        ReconciliationId = matchDto.ReconciliationId,
                        SystemTransactionId = matchDto.SystemTransactionId,
                        SystemTransactionType = matchDto.SystemTransactionType,
                        StatementItemId = matchDto.StatementItemId,
                        MatchType = matchDto.MatchType,
                        Amount = systemTransaction.Amount,
                        TransactionDate = systemTransaction.TransactionDate ?? systemTransaction.ProcessedAt,
                        Description = systemTransaction.Description,
                        MatchStatus = "MATCHED",
                        MatchNotes = matchDto.MatchNotes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        MatchedBy = userId
                    };

                    if (statementItem != null)
                    {
                        match.AmountDifference = Math.Abs(statementItem.Amount - systemTransaction.Amount);
                        statementItem.IsMatched = true;
                        statementItem.MatchedTransactionId = systemTransaction.Id;
                        statementItem.MatchedTransactionType = "Payment";
                        statementItem.MatchedAt = DateTime.UtcNow;
                        statementItem.MatchedBy = userId;
                        statementItem.UpdatedAt = DateTime.UtcNow;
                    }

                    _context.ReconciliationMatches.Add(match);
                }

                // Update reconciliation stats
                reconciliation.MatchedTransactions = await _context.ReconciliationMatches
                    .CountAsync(m => m.ReconciliationId == matchDto.ReconciliationId && m.MatchStatus == "MATCHED");
                reconciliation.UnmatchedTransactions = await _context.ReconciliationMatches
                    .CountAsync(m => m.ReconciliationId == matchDto.ReconciliationId && m.MatchStatus == "UNMATCHED");
                reconciliation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var matchResult = existingMatch ?? await _context.ReconciliationMatches
                    .FirstOrDefaultAsync(m => m.ReconciliationId == matchDto.ReconciliationId &&
                                             m.SystemTransactionId == matchDto.SystemTransactionId);

                if (matchResult == null)
                {
                    return ApiResponse<ReconciliationMatchDto>.ErrorResult("Failed to create match");
                }

                var dto = MapToReconciliationMatchDto(matchResult);
                return ApiResponse<ReconciliationMatchDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationMatchDto>.ErrorResult($"Failed to match transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UnmatchTransactionAsync(UnmatchTransactionDto unmatchDto, string userId)
        {
            try
            {
                var match = await _context.ReconciliationMatches
                    .Include(m => m.Reconciliation)
                    .FirstOrDefaultAsync(m => m.Id == unmatchDto.MatchId);

                if (match == null || match.Reconciliation.UserId != userId)
                {
                    return ApiResponse<bool>.ErrorResult("Match not found or access denied");
                }

                // Unmatch statement item if exists
                if (!string.IsNullOrEmpty(match.StatementItemId))
                {
                    var statementItem = await _context.BankStatementItems
                        .FirstOrDefaultAsync(i => i.Id == match.StatementItemId);
                    if (statementItem != null)
                    {
                        statementItem.IsMatched = false;
                        statementItem.MatchedTransactionId = null;
                        statementItem.MatchedTransactionType = null;
                        statementItem.MatchedAt = null;
                        statementItem.MatchedBy = null;
                        statementItem.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Update match status
                match.MatchStatus = "UNMATCHED";
                match.UpdatedAt = DateTime.UtcNow;

                // Update reconciliation stats
                match.Reconciliation.MatchedTransactions = await _context.ReconciliationMatches
                    .CountAsync(m => m.ReconciliationId == match.ReconciliationId && m.MatchStatus == "MATCHED");
                match.Reconciliation.UnmatchedTransactions = await _context.ReconciliationMatches
                    .CountAsync(m => m.ReconciliationId == match.ReconciliationId && m.MatchStatus == "UNMATCHED");
                match.Reconciliation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to unmatch transaction: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationDto>> CompleteReconciliationAsync(CompleteReconciliationDto completeDto, string userId)
        {
            try
            {
                var reconciliation = await _context.Reconciliations
                    .FirstOrDefaultAsync(r => r.Id == completeDto.ReconciliationId && r.UserId == userId);

                if (reconciliation == null)
                {
                    return ApiResponse<ReconciliationDto>.ErrorResult("Reconciliation not found");
                }

                reconciliation.Status = "COMPLETED";
                reconciliation.CompletedAt = DateTime.UtcNow;
                reconciliation.CompletedBy = userId;
                reconciliation.Notes = completeDto.Notes ?? reconciliation.Notes;
                reconciliation.UpdatedAt = DateTime.UtcNow;

                // Mark statement as reconciled if exists
                if (reconciliation.BankStatementId != null)
                {
                    var statement = await _context.BankStatements
                        .FirstOrDefaultAsync(s => s.Id == reconciliation.BankStatementId);
                    if (statement != null)
                    {
                        statement.IsReconciled = true;
                        statement.ReconciledAt = DateTime.UtcNow;
                        statement.ReconciledBy = userId;
                        statement.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                var result = await GetReconciliationAsync(completeDto.ReconciliationId, userId);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Failed to complete reconciliation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TransactionMatchSuggestionDto>>> GetMatchSuggestionsAsync(string reconciliationId, string userId)
        {
            try
            {
                var reconciliation = await _context.Reconciliations
                    .Include(r => r.BankStatement)
                    .ThenInclude(s => s!.StatementItems)
                    .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.UserId == userId);

                if (reconciliation == null)
                {
                    return ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult("Reconciliation not found");
                }

                var suggestions = new List<TransactionMatchSuggestionDto>();

                if (reconciliation.BankStatementId == null)
                {
                    return ApiResponse<List<TransactionMatchSuggestionDto>>.SuccessResult(suggestions);
                }

                // Get unmatched system transactions
                var startDate = reconciliation.ReconciliationDate.Date;
                var endDate = startDate.AddDays(1).AddTicks(-1);

                var systemTransactions = await _context.Payments
                    .Where(p => p.BankAccountId == reconciliation.BankAccountId &&
                               p.TransactionDate >= startDate &&
                               p.TransactionDate <= endDate &&
                               p.IsBankTransaction)
                    .ToListAsync();

                var matchedTransactionIds = await _context.ReconciliationMatches
                    .Where(m => m.ReconciliationId == reconciliationId && m.MatchStatus == "MATCHED")
                    .Select(m => m.SystemTransactionId)
                    .ToListAsync();

                var unmatchedSystemTransactions = systemTransactions
                    .Where(t => !matchedTransactionIds.Contains(t.Id))
                    .ToList();

                var unmatchedStatementItems = reconciliation.BankStatement!.StatementItems
                    .Where(i => !i.IsMatched)
                    .ToList();

                // Generate suggestions
                foreach (var transaction in unmatchedSystemTransactions)
                {
                    var bestMatch = unmatchedStatementItems
                        .Select(item =>
                        {
                            var score = CalculateMatchScore(transaction, item);
                            return new { Item = item, Score = score };
                        })
                        .Where(x => x.Score > 50) // Only suggest if score > 50%
                        .OrderByDescending(x => x.Score)
                        .FirstOrDefault();

                    if (bestMatch != null)
                    {
                        suggestions.Add(new TransactionMatchSuggestionDto
                        {
                            SystemTransactionId = transaction.Id,
                            SystemTransactionType = "Payment",
                            StatementItemId = bestMatch.Item.Id,
                            Amount = transaction.Amount,
                            TransactionDate = transaction.TransactionDate ?? transaction.ProcessedAt,
                            Description = transaction.Description,
                            MatchScore = bestMatch.Score,
                            MatchReason = GetMatchReason(transaction, bestMatch.Item, bestMatch.Score)
                        });
                    }
                }

                return ApiResponse<List<TransactionMatchSuggestionDto>>.SuccessResult(suggestions);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult($"Failed to get match suggestions: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationSummaryDto>> GetReconciliationSummaryAsync(string bankAccountId, DateTime? reconciliationDate, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    return ApiResponse<ReconciliationSummaryDto>.ErrorResult("Bank account not found");
                }

                var date = reconciliationDate ?? DateTime.UtcNow.Date;
                var startDate = date.Date;
                var endDate = startDate.AddDays(1).AddTicks(-1);

                var reconciliation = await _context.Reconciliations
                    .Where(r => r.BankAccountId == bankAccountId &&
                               r.ReconciliationDate.Date == date.Date &&
                               r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (reconciliation == null)
                {
                    // Return summary without reconciliation
                    var transactions = await _context.Payments
                        .Where(p => p.BankAccountId == bankAccountId &&
                                   p.TransactionDate >= startDate &&
                                   p.TransactionDate <= endDate &&
                                   p.IsBankTransaction)
                        .CountAsync();

                    return ApiResponse<ReconciliationSummaryDto>.SuccessResult(new ReconciliationSummaryDto
                    {
                        BankAccountId = bankAccountId,
                        BankAccountName = bankAccount.AccountName,
                        ReconciliationDate = date,
                        BookBalance = bankAccount.CurrentBalance,
                        StatementBalance = bankAccount.CurrentBalance,
                        Difference = 0,
                        TotalTransactions = transactions,
                        MatchedTransactions = 0,
                        UnmatchedTransactions = transactions,
                        PendingTransactions = 0,
                        Status = "PENDING",
                        IsBalanced = true
                    });
                }

                var summary = new ReconciliationSummaryDto
                {
                    ReconciliationId = reconciliation.Id,
                    BankAccountId = bankAccountId,
                    BankAccountName = bankAccount.AccountName,
                    ReconciliationDate = reconciliation.ReconciliationDate,
                    BookBalance = reconciliation.BookBalance,
                    StatementBalance = reconciliation.StatementBalance,
                    Difference = reconciliation.Difference,
                    TotalTransactions = reconciliation.TotalTransactions,
                    MatchedTransactions = reconciliation.MatchedTransactions,
                    UnmatchedTransactions = reconciliation.UnmatchedTransactions,
                    PendingTransactions = reconciliation.PendingTransactions,
                    Status = reconciliation.Status,
                    IsBalanced = Math.Abs(reconciliation.Difference) < 0.01m
                };

                return ApiResponse<ReconciliationSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationSummaryDto>.ErrorResult($"Failed to get reconciliation summary: {ex.Message}");
            }
        }

        // ==================== HELPER METHODS ====================

        private async Task AutoMatchStatementItemsAsync(string statementId, string userId)
        {
            var statement = await _context.BankStatements
                .Include(s => s.StatementItems)
                .FirstOrDefaultAsync(s => s.Id == statementId);

            if (statement == null) return;

            var startDate = statement.StatementStartDate.Date;
            var endDate = statement.StatementEndDate.Date.AddDays(1).AddTicks(-1);

            var systemTransactions = await _context.Payments
                .Where(p => p.BankAccountId == statement.BankAccountId &&
                           p.TransactionDate >= startDate &&
                           p.TransactionDate <= endDate &&
                           p.IsBankTransaction)
                .ToListAsync();

            foreach (var item in statement.StatementItems.Where(i => !i.IsMatched))
            {
                var match = systemTransactions.FirstOrDefault(t =>
                    Math.Abs((decimal)(t.Amount - item.Amount)) < 0.01m &&
                    t.TransactionDate.HasValue &&
                    Math.Abs((t.TransactionDate.Value.Date - item.TransactionDate.Date).TotalDays) <= 2 &&
                    (string.IsNullOrEmpty(item.ReferenceNumber) ||
                     string.IsNullOrEmpty(t.Reference) ||
                     item.ReferenceNumber == t.Reference));

                if (match != null)
                {
                    item.IsMatched = true;
                    item.MatchedTransactionId = match.Id;
                    item.MatchedTransactionType = "Payment";
                    item.MatchedAt = DateTime.UtcNow;
                    item.MatchedBy = userId;
                    item.UpdatedAt = DateTime.UtcNow;
                }
            }

            statement.MatchedTransactions = statement.StatementItems.Count(i => i.IsMatched);
            statement.UnmatchedTransactions = statement.TotalTransactions - statement.MatchedTransactions;
            statement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private async Task CreateTransactionsFromUnmatchedItemsAsync(string statementId, string userId)
        {
            try
            {
                var statement = await _context.BankStatements
                    .Include(s => s.StatementItems)
                    .FirstOrDefaultAsync(s => s.Id == statementId);

                if (statement == null)
                {
                    _logger.LogWarning($"Bank statement {statementId} not found for creating transactions");
                    return;
                }

                // Get all unmatched items
                var unmatchedItems = statement.StatementItems
                    .Where(i => !i.IsMatched)
                    .ToList();

                if (!unmatchedItems.Any())
                {
                    _logger.LogInformation($"No unmatched items to create transactions from for statement {statementId}");
                    return;
                }

                _logger.LogInformation($"Creating {unmatchedItems.Count} transactions from unmatched statement items");

                int createdCount = 0;
                int failedCount = 0;

                foreach (var item in unmatchedItems)
                {
                    try
                    {
                        // Get bank account currency
                        var bankAccount = await _context.BankAccounts
                            .FirstOrDefaultAsync(ba => ba.Id == statement.BankAccountId);
                        
                        if (bankAccount == null)
                        {
                            _logger.LogWarning($"Bank account {statement.BankAccountId} not found for statement item {item.Id}");
                            failedCount++;
                            continue;
                        }

                        // Auto-create category if it doesn't exist (for reconciliation imports)
                        if (!string.IsNullOrWhiteSpace(item.Category) && 
                            item.TransactionType?.ToUpper() != "CREDIT")
                        {
                            var categoryExists = await _context.TransactionCategories
                                .AnyAsync(c => c.UserId == userId && 
                                             c.Name.ToUpper() == item.Category.ToUpper() && 
                                             !c.IsDeleted);

                            if (!categoryExists)
                            {
                                // Auto-create the category for reconciliation imports
                                var newCategory = new TransactionCategory
                                {
                                    UserId = userId,
                                    Name = item.Category,
                                    Description = $"Auto-created from bank statement import",
                                    Type = "EXPENSE", // Default to expense for DEBIT transactions
                                    IsActive = true,
                                    IsSystemCategory = false,
                                    DisplayOrder = 0,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                _context.TransactionCategories.Add(newCategory);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"Auto-created category '{item.Category}' for statement import");
                            }
                        }

                        // Create transaction DTO from statement item
                        // For reconciliation imports, we should skip month closure check and allow empty categories
                        // as these are historical transactions being imported
                        var createTransactionDto = new CreateBankTransactionDto
                        {
                            BankAccountId = statement.BankAccountId,
                            Amount = item.Amount,
                            TransactionType = item.TransactionType,
                            Description = item.Description ?? "Bank Statement Transaction",
                            Category = string.IsNullOrWhiteSpace(item.Category) ? null : item.Category,
                            ReferenceNumber = item.ReferenceNumber ?? $"STMT_{item.Id.Substring(0, Math.Min(8, item.Id.Length))}",
                            TransactionDate = item.TransactionDate,
                            Merchant = item.Merchant,
                            Currency = bankAccount.Currency ?? "USD",
                            Notes = $"Imported from bank statement: {statement.StatementName}"
                        };

                        // Create transaction using BankAccountService
                        var transactionResult = await _bankAccountService.CreateTransactionAsync(createTransactionDto, userId);

                        if (transactionResult.Success && transactionResult.Data != null)
                        {
                            // Mark statement item as matched
                            item.IsMatched = true;
                            item.MatchedTransactionId = transactionResult.Data.Id;
                            item.MatchedTransactionType = "Payment";
                            item.MatchedAt = DateTime.UtcNow;
                            item.MatchedBy = userId;
                            item.UpdatedAt = DateTime.UtcNow;

                            createdCount++;
                            _logger.LogInformation($"Created transaction {transactionResult.Data.Id} from statement item {item.Id}");
                        }
                        else
                        {
                            var errorMessage = transactionResult.Message ?? "Unknown error";
                            _logger.LogWarning($"Failed to create transaction from statement item {item.Id}: {errorMessage}");
                            
                            // Log detailed error information
                            if (transactionResult.Errors != null && transactionResult.Errors.Any())
                            {
                                _logger.LogWarning($"Transaction creation errors for item {item.Id}: {string.Join(", ", transactionResult.Errors)}");
                            }
                            
                            // Log transaction details for debugging
                            _logger.LogWarning($"Failed transaction details - Amount: {item.Amount}, Type: {item.TransactionType}, Date: {item.TransactionDate}, Category: {item.Category}, Description: {item.Description}");
                            
                            failedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error creating transaction from statement item {item.Id}");
                        failedCount++;
                    }
                }

                // Update statement matched counts
                statement.MatchedTransactions = statement.StatementItems.Count(i => i.IsMatched);
                statement.UnmatchedTransactions = statement.TotalTransactions - statement.MatchedTransactions;
                statement.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created {createdCount} transactions from unmatched items. {failedCount} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating transactions from unmatched items for statement {statementId}");
                // Don't throw - allow import to succeed even if transaction creation fails
            }
        }

        private decimal CalculateMatchScore(Payment transaction, BankStatementItem item)
        {
            decimal score = 0;

            // Amount match (50 points)
            if (Math.Abs((decimal)(transaction.Amount - item.Amount)) < 0.01m)
            {
                score += 50;
            }
            else if (Math.Abs((decimal)(transaction.Amount - item.Amount)) < 1.00m)
            {
                score += 25; // Close match
            }

            // Date match (30 points)
            if (transaction.TransactionDate.HasValue)
            {
                var daysDiff = Math.Abs((transaction.TransactionDate.Value.Date - item.TransactionDate.Date).TotalDays);
                if (daysDiff == 0)
                {
                    score += 30;
                }
                else if (daysDiff <= 1)
                {
                    score += 20;
                }
                else if (daysDiff <= 2)
                {
                    score += 10;
                }
            }

            // Reference match (20 points)
            if (!string.IsNullOrEmpty(transaction.Reference) &&
                !string.IsNullOrEmpty(item.ReferenceNumber) &&
                transaction.Reference == item.ReferenceNumber)
            {
                score += 20;
            }
            else if (!string.IsNullOrEmpty(transaction.Description) &&
                     !string.IsNullOrEmpty(item.Description) &&
                     transaction.Description.Contains(item.Description, StringComparison.OrdinalIgnoreCase))
            {
                score += 10; // Partial description match
            }

            return score;
        }

        private string GetMatchReason(Payment transaction, BankStatementItem item, decimal score)
        {
            var reasons = new List<string>();

            if (Math.Abs((decimal)(transaction.Amount - item.Amount)) < 0.01m)
            {
                reasons.Add("Exact amount match");
            }

            if (transaction.TransactionDate.HasValue)
            {
                var daysDiff = Math.Abs((transaction.TransactionDate.Value.Date - item.TransactionDate.Date).TotalDays);
                if (daysDiff == 0)
                {
                    reasons.Add("Same date");
                }
                else if (daysDiff <= 2)
                {
                    reasons.Add($"Date within {daysDiff} days");
                }
            }

            if (!string.IsNullOrEmpty(transaction.Reference) &&
                !string.IsNullOrEmpty(item.ReferenceNumber) &&
                transaction.Reference == item.ReferenceNumber)
            {
                reasons.Add("Reference number match");
            }

            return string.Join(", ", reasons);
        }

        private BankStatementDto MapToBankStatementDto(BankStatement statement)
        {
            return new BankStatementDto
            {
                Id = statement.Id,
                UserId = statement.UserId,
                BankAccountId = statement.BankAccountId,
                StatementName = statement.StatementName,
                StatementStartDate = statement.StatementStartDate,
                StatementEndDate = statement.StatementEndDate,
                OpeningBalance = statement.OpeningBalance,
                ClosingBalance = statement.ClosingBalance,
                ImportFormat = statement.ImportFormat,
                ImportSource = statement.ImportSource,
                TotalTransactions = statement.TotalTransactions,
                MatchedTransactions = statement.MatchedTransactions,
                UnmatchedTransactions = statement.UnmatchedTransactions,
                IsReconciled = statement.IsReconciled,
                ReconciledAt = statement.ReconciledAt,
                ReconciledBy = statement.ReconciledBy,
                CreatedAt = statement.CreatedAt,
                UpdatedAt = statement.UpdatedAt,
                StatementItems = statement.StatementItems?.Select(MapToBankStatementItemDto).ToList()
            };
        }

        private BankStatementItemDto MapToBankStatementItemDto(BankStatementItem item)
        {
            return new BankStatementItemDto
            {
                Id = item.Id,
                BankStatementId = item.BankStatementId,
                TransactionDate = item.TransactionDate,
                Amount = item.Amount,
                TransactionType = item.TransactionType,
                Description = item.Description,
                ReferenceNumber = item.ReferenceNumber,
                Merchant = item.Merchant,
                Category = item.Category,
                BalanceAfterTransaction = item.BalanceAfterTransaction,
                IsMatched = item.IsMatched,
                MatchedTransactionId = item.MatchedTransactionId,
                MatchedTransactionType = item.MatchedTransactionType,
                MatchedAt = item.MatchedAt,
                MatchedBy = item.MatchedBy,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        private ReconciliationDto MapToReconciliationDto(Reconciliation reconciliation)
        {
            return new ReconciliationDto
            {
                Id = reconciliation.Id,
                UserId = reconciliation.UserId,
                BankAccountId = reconciliation.BankAccountId,
                BankStatementId = reconciliation.BankStatementId,
                ReconciliationName = reconciliation.ReconciliationName,
                ReconciliationDate = reconciliation.ReconciliationDate,
                BookBalance = reconciliation.BookBalance,
                StatementBalance = reconciliation.StatementBalance,
                Difference = reconciliation.Difference,
                TotalTransactions = reconciliation.TotalTransactions,
                MatchedTransactions = reconciliation.MatchedTransactions,
                UnmatchedTransactions = reconciliation.UnmatchedTransactions,
                PendingTransactions = reconciliation.PendingTransactions,
                Status = reconciliation.Status,
                Notes = reconciliation.Notes,
                CompletedAt = reconciliation.CompletedAt,
                CompletedBy = reconciliation.CompletedBy,
                CreatedAt = reconciliation.CreatedAt,
                UpdatedAt = reconciliation.UpdatedAt,
                Matches = reconciliation.Matches?.Select(MapToReconciliationMatchDto).ToList()
            };
        }

        private ReconciliationMatchDto MapToReconciliationMatchDto(ReconciliationMatch match)
        {
            return new ReconciliationMatchDto
            {
                Id = match.Id,
                ReconciliationId = match.ReconciliationId,
                SystemTransactionId = match.SystemTransactionId,
                SystemTransactionType = match.SystemTransactionType,
                StatementItemId = match.StatementItemId,
                MatchType = match.MatchType,
                Amount = match.Amount,
                TransactionDate = match.TransactionDate,
                Description = match.Description,
                MatchStatus = match.MatchStatus,
                MatchNotes = match.MatchNotes,
                AmountDifference = match.AmountDifference,
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt,
                MatchedBy = match.MatchedBy
            };
        }
    }
}

