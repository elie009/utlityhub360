// FORCE CHANGE
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace UtilityHub360.Services
{
    public class ReconciliationService : IReconciliationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIAgentService _aiAgentService;
        private readonly IOcrService _ocrService;
        private readonly IBankAccountService _bankAccountService;
        private readonly ILogger<ReconciliationService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _openAISettings;

        public ReconciliationService(
            ApplicationDbContext context,
            IAIAgentService aiAgentService,
            IOcrService ocrService,
            IBankAccountService bankAccountService,
            ILogger<ReconciliationService> logger,
            ILoggerFactory loggerFactory,
            OpenAISettings openAISettings)
        {
            _context = context;
            _aiAgentService = aiAgentService;
            _ocrService = ocrService;
            _bankAccountService = bankAccountService;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _openAISettings = openAISettings;
            _httpClient = new HttpClient();
            
            if (!string.IsNullOrEmpty(_openAISettings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _openAISettings.ApiKey);
            }
        }

        public async Task<ApiResponse<BankStatementDto>> ImportBankStatementAsync(ImportBankStatementDto importDto, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == importDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null) return ApiResponse<BankStatementDto>.ErrorResult("Bank account not found");

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

                await AutoMatchStatementItemsAsync(bankStatement.Id, userId);
                await CreateTransactionsFromUnmatchedItemsAsync(bankStatement.Id, userId);

                return await GetBankStatementAsync(bankStatement.Id, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankStatementDto>.ErrorResult($"Failed to import: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankStatementDto>> GetBankStatementAsync(string statementId, string userId)
        {
            try
            {
                var statement = await _context.BankStatements
                    .AsNoTracking()
                    .Include(s => s.StatementItems)
                    .FirstOrDefaultAsync(s => s.Id == statementId && s.UserId == userId);

                if (statement == null) return ApiResponse<BankStatementDto>.ErrorResult("Not found");

                return ApiResponse<BankStatementDto>.SuccessResult(MapToBankStatementDto(statement));
            }
            catch (Exception ex)
            {
                return ApiResponse<BankStatementDto>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankStatementDto>>> GetBankStatementsAsync(string bankAccountId, string userId)
        {
            try
            {
                var statements = await _context.BankStatements
                    .AsNoTracking()
                    .Where(s => s.BankAccountId == bankAccountId && s.UserId == userId)
                    .OrderByDescending(s => s.StatementEndDate)
                    .ToListAsync();

                return ApiResponse<List<BankStatementDto>>.SuccessResult(statements.Select(MapToBankStatementDto).ToList());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankStatementDto>>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBankStatementAsync(string statementId, string userId)
        {
            try
            {
                var statement = await _context.BankStatements
                    .Include(s => s.StatementItems)
                    .FirstOrDefaultAsync(s => s.Id == statementId && s.UserId == userId);

                if (statement == null) return ApiResponse<bool>.ErrorResult("Not found");
                if (statement.IsReconciled) return ApiResponse<bool>.ErrorResult("Cannot delete reconciled statement");

                _context.BankStatementItems.RemoveRange(statement.StatementItems);
                _context.BankStatements.Remove(statement);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Error: {ex.Message}");
            }
        }

        // ==================== ASYNC BANK STATEMENT UPLOAD OPERATIONS ====================

        public async Task<ApiResponse<BankStatementUploadDto>> UploadBankStatementAsync(IFormFile file, string bankAccountId, string userId)
        {
            try
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "bankstatements");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var fileType = fileExtension == ".pdf" ? "PDF" : "CSV";
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var upload = new BankStatementUpload
                {
                    UserId = userId,
                    BankAccountId = bankAccountId,
                    FilePath = filePath,
                    OriginalFileName = file.FileName,
                    FileType = fileType,
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BankStatementUploads.Add(upload);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId, Action = "UPLOAD_BANK_STATEMENT", EntityType = "BankStatementUpload",
                    EntityId = upload.Id, Description = $"Uploaded: {file.FileName}", CreatedAt = DateTime.UtcNow,
                    LogType = "USER_ACTIVITY", Severity = "INFO"
                });

                await _context.SaveChangesAsync();
                return ApiResponse<BankStatementUploadDto>.SuccessResult(MapToUploadDto(upload));
            }
            catch (Exception ex)
            {
                return ApiResponse<BankStatementUploadDto>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankStatementUploadDto>>> GetPendingUploadsAsync(int limit)
        {
            try
            {
                var uploads = await _context.BankStatementUploads
                    .Where(u => u.Status == "PENDING")
                    .OrderBy(u => u.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                foreach (var upload in uploads)
                {
                    upload.Status = "PROCESSING";
                    upload.UpdatedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();

                return ApiResponse<List<BankStatementUploadDto>>.SuccessResult(uploads.Select(MapToUploadDto).ToList());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankStatementUploadDto>>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BankStatementUploadDto>>> GetUserUploadsAsync(string bankAccountId, string userId)
        {
            try
            {
                var uploads = await _context.BankStatementUploads
                    .Where(u => u.BankAccountId == bankAccountId && u.UserId == userId)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                return ApiResponse<List<BankStatementUploadDto>>.SuccessResult(uploads.Select(MapToUploadDto).ToList());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankStatementUploadDto>>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<(Stream fileStream, string fileName, string contentType)>> GetUploadFileAsync(string uploadId)
        {
            try
            {
                var upload = await _context.BankStatementUploads.FindAsync(uploadId);
                if (upload == null) return ApiResponse<(Stream, string, string)>.ErrorResult("Not found");

                if (!File.Exists(upload.FilePath)) return ApiResponse<(Stream, string, string)>.ErrorResult("File missing");

                var stream = new FileStream(upload.FilePath, FileMode.Open, FileAccess.Read);
                var contentType = upload.FileType == "PDF" ? "application/pdf" : "text/csv";
                
                return ApiResponse<(Stream, string, string)>.SuccessResult((stream, upload.OriginalFileName, contentType));
            }
            catch (Exception ex)
            {
                return ApiResponse<(Stream, string, string)>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ProcessExtractedTextAsync(ProcessExtractedTextDto processDto)
        {
            try
            {
                var upload = await _context.BankStatementUploads.FindAsync(processDto.UploadId);
                if (upload == null) return ApiResponse<bool>.ErrorResult("Not found");

                var aiResult = await ParseExtractedTextWithAIAsync(processDto.ExtractedText, upload.OriginalFileName);

                if (!aiResult.Success)
                {
                    upload.Status = "FAILED";
                    upload.ErrorMessage = aiResult.Message;
                    await _context.SaveChangesAsync();
                    return ApiResponse<bool>.ErrorResult(aiResult.Message);
                }

                var stagingTransactions = aiResult.Data.StatementItems.Select(item => new StagingTransaction
                {
                    Id = Guid.NewGuid().ToString(), // Explicitly set ID
                    UploadId = upload.Id,
                    TransactionDate = item.TransactionDate,
                    Amount = item.Amount,
                    // Normalize and truncate TransactionType to max 10 characters
                    TransactionType = NormalizeTransactionType(item.TransactionType ?? "DEBIT"),
                    Description = item.Description != null && item.Description.Length > 500 ? item.Description.Substring(0, 500) : item.Description, // Truncate if too long
                    ReferenceNumber = item.ReferenceNumber != null && item.ReferenceNumber.Length > 255 ? item.ReferenceNumber.Substring(0, 255) : item.ReferenceNumber, // Truncate if too long
                    Merchant = item.Merchant != null && item.Merchant.Length > 255 ? item.Merchant.Substring(0, 255) : item.Merchant, // Truncate if too long
                    Category = item.Category != null && item.Category.Length > 255 ? item.Category.Substring(0, 255) : item.Category, // Truncate if too long
                    BalanceAfterTransaction = item.BalanceAfterTransaction,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                if (stagingTransactions.Count > 0)
                {
                    _context.StagingTransactions.AddRange(stagingTransactions);
                }
                upload.Status = "DONE";
                upload.UpdatedAt = DateTime.UtcNow;
                
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    // Log the inner exception for debugging
                    var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                    var fullError = dbEx.ToString();
                    
                    // Log to console for debugging
                    Console.WriteLine($"Database Save Error: {fullError}");
                    
                    // Check if it's a table missing error
                    if (innerException.Contains("Invalid object name") || innerException.Contains("StagingTransactions"))
                    {
                        return ApiResponse<bool>.ErrorResult("Database table 'StagingTransactions' is missing. Please run the migration script: Documentation/Database/Scripts/create_bank_statement_upload_tables.sql");
                    }
                    
                    // Check if it's a foreign key constraint error
                    if (innerException.Contains("FOREIGN KEY") || innerException.Contains("constraint"))
                    {
                        return ApiResponse<bool>.ErrorResult($"Foreign key constraint error: {innerException}. The BankStatementUpload record may not exist.");
                    }
                    
                    // Check if it's a string length error
                    if (innerException.Contains("String or binary data would be truncated") || innerException.Contains("StringLength"))
                    {
                        return ApiResponse<bool>.ErrorResult($"Data validation error: One or more fields exceed maximum length. Details: {innerException}");
                    }
                    
                    return ApiResponse<bool>.ErrorResult($"Database error: {innerException}. Full error logged to console.");
                }
                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                // Log detailed error information
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner: {ex.InnerException.Message}";
                }
                
                return ApiResponse<bool>.ErrorResult($"Error: {errorMessage}");
            }
        }

        public async Task<ApiResponse<List<StagingTransactionDto>>> GetStagingTransactionsAsync(string uploadId, string userId)
        {
            try
            {
                var upload = await _context.BankStatementUploads
                    .FirstOrDefaultAsync(u => u.Id == uploadId && u.UserId == userId);
                
                if (upload == null) return ApiResponse<List<StagingTransactionDto>>.ErrorResult("Not found");

                var transactions = await _context.StagingTransactions
                    .Where(t => t.UploadId == uploadId)
                    .OrderBy(t => t.TransactionDate)
                    .ToListAsync();

                return ApiResponse<List<StagingTransactionDto>>.SuccessResult(transactions.Select(MapToStagingDto).ToList());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<StagingTransactionDto>>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankStatementDto>> ConfirmUploadAsync(string uploadId, ConfirmBankStatementUploadDto confirmDto, string userId)
        {
            try
            {
                var upload = await _context.BankStatementUploads
                    .FirstOrDefaultAsync(u => u.Id == uploadId && u.UserId == userId);
                
                if (upload == null) return ApiResponse<BankStatementDto>.ErrorResult("Not found");

                var statement = new BankStatement
                {
                    UserId = userId,
                    BankAccountId = upload.BankAccountId,
                    StatementName = confirmDto.StatementName,
                    StatementStartDate = confirmDto.StatementStartDate,
                    StatementEndDate = confirmDto.StatementEndDate,
                    OpeningBalance = confirmDto.OpeningBalance,
                    ClosingBalance = confirmDto.ClosingBalance,
                    ImportFormat = upload.FileType,
                    ImportSource = upload.OriginalFileName,
                    TotalTransactions = confirmDto.Transactions?.Count ?? 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BankStatements.Add(statement);
                await _context.SaveChangesAsync();

                if (confirmDto.Transactions != null)
                {
                    var items = confirmDto.Transactions.Select(t => new BankStatementItem
                    {
                        BankStatementId = statement.Id,
                        TransactionDate = t.TransactionDate,
                        Amount = t.Amount,
                        TransactionType = t.TransactionType,
                        Description = t.Description,
                        ReferenceNumber = t.ReferenceNumber,
                        Merchant = t.Merchant,
                        Category = t.Category,
                        BalanceAfterTransaction = t.BalanceAfterTransaction,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.BankStatementItems.AddRange(items);
                }

                var staging = await _context.StagingTransactions.Where(t => t.UploadId == uploadId).ToListAsync();
                _context.StagingTransactions.RemoveRange(staging);

                upload.Status = "COMPLETED";
                upload.ProcessedBankStatementId = statement.Id;
                upload.ProcessedAt = DateTime.UtcNow;
                upload.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Auto-match with existing transactions and create new ones for unmatched items
                await AutoMatchStatementItemsAsync(statement.Id, userId);
                await CreateTransactionsFromUnmatchedItemsAsync(statement.Id, userId);

                return await GetBankStatementAsync(statement.Id, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankStatementDto>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BankStatementUploadDto>> GetUploadStatusAsync(string uploadId, string userId)
        {
            try
            {
                var upload = await _context.BankStatementUploads
                    .FirstOrDefaultAsync(u => u.Id == uploadId && u.UserId == userId);
                
                if (upload == null) return ApiResponse<BankStatementUploadDto>.ErrorResult("Not found");

                return ApiResponse<BankStatementUploadDto>.SuccessResult(MapToUploadDto(upload));
            }
            catch (Exception ex)
            {
                return ApiResponse<BankStatementUploadDto>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelUploadAsync(string uploadId, string userId)
        {
            try
            {
                var upload = await _context.BankStatementUploads
                    .FirstOrDefaultAsync(u => u.Id == uploadId && u.UserId == userId);
                
                if (upload == null) return ApiResponse<bool>.ErrorResult("Not found");

                if (upload.Status == "COMPLETED" || upload.Status == "DONE")
                {
                    return ApiResponse<bool>.ErrorResult("Already processed");
                }

                upload.Status = "CANCELLED";
                upload.UpdatedAt = DateTime.UtcNow;

                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId, Action = "CANCEL_BANK_STATEMENT_UPLOAD", EntityType = "BankStatementUpload",
                    EntityId = upload.Id, Description = $"Cancelled: {upload.OriginalFileName}", CreatedAt = DateTime.UtcNow,
                    LogType = "USER_ACTIVITY", Severity = "INFO"
                });

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Error: {ex.Message}");
            }
        }

        // ==================== RECONCILIATION OPERATIONS ====================

        public async Task<ApiResponse<ReconciliationDto>> CreateReconciliationAsync(CreateReconciliationDto createDto, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(ba => ba.Id == createDto.BankAccountId && ba.UserId == userId);

                if (bankAccount == null) return ApiResponse<ReconciliationDto>.ErrorResult("Bank account not found");

                var reconciliation = new Reconciliation
                {
                    UserId = userId,
                    BankAccountId = createDto.BankAccountId,
                    BankStatementId = createDto.BankStatementId,
                    ReconciliationName = createDto.ReconciliationName,
                    ReconciliationDate = createDto.ReconciliationDate,
                    BookBalance = bankAccount.CurrentBalance,
                    Status = "PENDING",
                    Notes = createDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Reconciliations.Add(reconciliation);
                await _context.SaveChangesAsync();

                await AutoMatchTransactionsAsync(reconciliation.Id, userId);
                return await GetReconciliationAsync(reconciliation.Id, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Error: {ex.Message}");
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

                if (reconciliation == null) return ApiResponse<ReconciliationDto>.ErrorResult("Not found");

                return ApiResponse<ReconciliationDto>.SuccessResult(MapToReconciliationDto(reconciliation));
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Error: {ex.Message}");
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

                return ApiResponse<List<ReconciliationDto>>.SuccessResult(reconciliations.Select(MapToReconciliationDto).ToList());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ReconciliationDto>>.ErrorResult($"Error: {ex.Message}");
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

                if (reconciliation == null) return ApiResponse<ReconciliationDto>.ErrorResult("Not found");

                var startDate = reconciliation.ReconciliationDate.Date;
                var endDate = startDate.AddDays(1).AddTicks(-1);

                var systemTransactions = await _context.Payments
                    .Where(p => p.BankAccountId == reconciliation.BankAccountId &&
                               p.TransactionDate >= startDate &&
                               p.TransactionDate <= endDate &&
                               p.IsBankTransaction)
                    .ToListAsync();

                List<BankStatementItem>? statementItems = null;
                if (reconciliation.BankStatementId != null)
                {
                    statementItems = await _context.BankStatementItems
                        .Where(i => i.BankStatementId == reconciliation.BankStatementId && !i.IsMatched)
                        .ToListAsync();
                }

                foreach (var transaction in systemTransactions)
                {
                    var existingMatch = await _context.ReconciliationMatches
                        .FirstOrDefaultAsync(m => m.ReconciliationId == reconciliationId && m.SystemTransactionId == transaction.Id);

                    if (existingMatch != null) continue;

                    BankStatementItem? matchedItem = null;
                    if (statementItems != null)
                    {
                        matchedItem = statementItems.FirstOrDefault(item =>
                            !item.IsMatched &&
                            Math.Abs((decimal)(item.Amount - transaction.Amount)) < 0.01m &&
                            transaction.TransactionDate.HasValue &&
                            Math.Abs((item.TransactionDate.Date - transaction.TransactionDate.Value.Date).TotalDays) <= 2);
                    }

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
                }

                await _context.SaveChangesAsync();
                return await GetReconciliationAsync(reconciliationId, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationMatchDto>> MatchTransactionAsync(MatchTransactionDto matchDto, string userId)
        {
            try
            {
                var reconciliation = await _context.Reconciliations
                    .FirstOrDefaultAsync(r => r.Id == matchDto.ReconciliationId && r.UserId == userId);

                if (reconciliation == null) return ApiResponse<ReconciliationMatchDto>.ErrorResult("Not found");

                var systemTransaction = await _context.Payments.FindAsync(matchDto.SystemTransactionId);
                if (systemTransaction == null) return ApiResponse<ReconciliationMatchDto>.ErrorResult("Not found");

                BankStatementItem? statementItem = null;
                if (!string.IsNullOrEmpty(matchDto.StatementItemId))
                {
                    statementItem = await _context.BankStatementItems.FindAsync(matchDto.StatementItemId);
                }

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
                await _context.SaveChangesAsync();

                return ApiResponse<ReconciliationMatchDto>.SuccessResult(MapToReconciliationMatchDto(match));
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationMatchDto>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UnmatchTransactionAsync(UnmatchTransactionDto unmatchDto, string userId)
        {
            try
            {
                var match = await _context.ReconciliationMatches
                    .Include(m => m.Reconciliation)
                    .FirstOrDefaultAsync(m => m.Id == unmatchDto.MatchId);

                if (match == null || match.Reconciliation.UserId != userId) return ApiResponse<bool>.ErrorResult("Not found");

                if (!string.IsNullOrEmpty(match.StatementItemId))
                {
                    var statementItem = await _context.BankStatementItems.FindAsync(match.StatementItemId);
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

                match.MatchStatus = "UNMATCHED";
                match.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationDto>> CompleteReconciliationAsync(CompleteReconciliationDto completeDto, string userId)
        {
            try
            {
                var reconciliation = await _context.Reconciliations.FirstOrDefaultAsync(r => r.Id == completeDto.ReconciliationId && r.UserId == userId);
                if (reconciliation == null) return ApiResponse<ReconciliationDto>.ErrorResult("Not found");

                reconciliation.Status = "COMPLETED";
                reconciliation.CompletedAt = DateTime.UtcNow;
                reconciliation.CompletedBy = userId;
                reconciliation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return await GetReconciliationAsync(completeDto.ReconciliationId, userId);
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationDto>.ErrorResult($"Error: {ex.Message}");
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

                if (reconciliation == null) return ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult("Not found");

                var suggestions = new List<TransactionMatchSuggestionDto>();
                return ApiResponse<List<TransactionMatchSuggestionDto>>.SuccessResult(suggestions);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TransactionMatchSuggestionDto>>.ErrorResult($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReconciliationSummaryDto>> GetReconciliationSummaryAsync(string bankAccountId, DateTime? reconciliationDate, string userId)
        {
            try
            {
                var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.UserId == userId);
                if (bankAccount == null) return ApiResponse<ReconciliationSummaryDto>.ErrorResult("Not found");

                return ApiResponse<ReconciliationSummaryDto>.SuccessResult(new ReconciliationSummaryDto
                {
                    BankAccountId = bankAccountId,
                    BankAccountName = bankAccount.AccountName,
                    ReconciliationDate = reconciliationDate ?? DateTime.UtcNow,
                    BookBalance = bankAccount.CurrentBalance,
                    Status = "PENDING"
                });
            }
            catch (Exception ex)
            {
                return ApiResponse<ReconciliationSummaryDto>.ErrorResult($"Error: {ex.Message}");
            }
        }

        // ==================== BANK STATEMENT EXTRACTION METHODS ====================

        public async Task<ApiResponse<ExtractBankStatementResponseDto>> ExtractBankStatementFromFileAsync(Stream fileStream, string fileName, string bankAccountId, string userId)
        {
            try
            {
                var fileExtension = Path.GetExtension(fileName).ToLower();
                string extractedText = string.Empty;

                if (fileExtension == ".csv")
                {
                    // Read CSV file
                    using var reader = new StreamReader(fileStream);
                    extractedText = await reader.ReadToEndAsync();
                }
                else if (fileExtension == ".pdf")
                {
                    // Extract text from PDF using OCR service
                    var ocrResult = await _ocrService.ProcessPdfAsync(fileStream);
                    if (string.IsNullOrWhiteSpace(ocrResult.FullText))
                    {
                        return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Failed to extract text from PDF");
                    }
                    extractedText = ocrResult.FullText;
                }
                else
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Unsupported file format. Only CSV and PDF are supported.");
                }

                // Parse extracted text with AI
                var result = await ParseExtractedTextWithAIAsync(extractedText, fileName);
                if (result.Success)
                {
                    result.Data.ExtractedText = extractedText; // Include extracted text for debugging
                    result.Data.ImportFormat = fileExtension == ".csv" ? "CSV" : "PDF";
                    result.Data.ImportSource = fileName;
                }
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Error extracting bank statement: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ExtractBankStatementResponseDto>> AnalyzePDFWithAIAsync(Stream fileStream, string fileName, string bankAccountId, string userId)
        {
            try
            {
                // Extract text from PDF using OCR service
                var ocrResult = await _ocrService.ProcessPdfAsync(fileStream);
                if (string.IsNullOrWhiteSpace(ocrResult.FullText))
                {
                    return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("Failed to extract text from PDF");
                }

                // Parse extracted text with AI
                var result = await ParseExtractedTextWithAIAsync(ocrResult.FullText, fileName);
                if (result.Success)
                {
                    result.Data.ExtractedText = ocrResult.FullText; // Include extracted text for debugging
                    result.Data.ImportFormat = "PDF";
                    result.Data.ImportSource = fileName;
                }
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult($"Error analyzing PDF with AI: {ex.Message}");
            }
        }

        // ==================== HELPER METHODS ====================

        private async Task<ApiResponse<ExtractBankStatementResponseDto>> ParseExtractedTextWithAIAsync(string extractedText, string fileName)
        {
            if (string.IsNullOrEmpty(_openAISettings.ApiKey)) return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("No API key");

            var prompt = "Extract bank statement JSON. Text: " + extractedText.Substring(0, Math.Min(extractedText.Length, 10000));
            var messages = new List<object> { new { role = "user", content = prompt } };
            var openAIRequest = new { model = "gpt-4o-mini", messages = messages, response_format = new { type = "json_object" } };
            
            var httpResponse = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", new StringContent(JsonSerializer.Serialize(openAIRequest), Encoding.UTF8, "application/json"));
            if (!httpResponse.IsSuccessStatusCode) return ApiResponse<ExtractBankStatementResponseDto>.ErrorResult("AI failed");

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var aiResult = JsonSerializer.Deserialize<JsonElement>(responseContent).GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
            var parsed = JsonSerializer.Deserialize<JsonElement>(aiResult);
            
            var result = new ExtractBankStatementResponseDto
            {
                StatementName = parsed.TryGetProperty("statementName", out var name) ? name.GetString() ?? fileName : fileName,
                StatementStartDate = parsed.TryGetProperty("statementStartDate", out var sd) && DateTime.TryParse(sd.GetString(), out var sdt) ? sdt : null,
                StatementEndDate = parsed.TryGetProperty("statementEndDate", out var ed) && DateTime.TryParse(ed.GetString(), out var edt) ? edt : null,
                OpeningBalance = parsed.TryGetProperty("openingBalance", out var ob) ? GetDecimalFromJsonElement(ob) : null,
                ClosingBalance = parsed.TryGetProperty("closingBalance", out var cb) ? GetDecimalFromJsonElement(cb) : null,
                StatementItems = new List<BankStatementItemImportDto>()
            };

            if (parsed.TryGetProperty("transactions", out var transactions))
            {
                foreach (var trans in transactions.EnumerateArray())
                {
                    result.StatementItems.Add(new BankStatementItemImportDto
                    {
                        TransactionDate = trans.TryGetProperty("transactionDate", out var d) && DateTime.TryParse(d.GetString(), out var dt) ? dt : DateTime.UtcNow,
                        Amount = trans.TryGetProperty("amount", out var a) ? GetDecimalFromJsonElement(a) ?? 0 : 0,
                        TransactionType = trans.TryGetProperty("transactionType", out var t) ? t.GetString() ?? "DEBIT" : "DEBIT",
                        Description = trans.TryGetProperty("description", out var desc) ? desc.GetString() : "",
                        ReferenceNumber = trans.TryGetProperty("referenceNumber", out var refNum) ? refNum.GetString() : null,
                        Merchant = trans.TryGetProperty("merchant", out var merch) ? merch.GetString() : null,
                        Category = trans.TryGetProperty("category", out var cat) ? cat.GetString() : null,
                        BalanceAfterTransaction = trans.TryGetProperty("balanceAfterTransaction", out var bal) ? GetDecimalFromJsonElement(bal) ?? 0 : 0
                    });
                }
            }
            return ApiResponse<ExtractBankStatementResponseDto>.SuccessResult(result);
        }

        private decimal? GetDecimalFromJsonElement(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number) return element.GetDecimal();
            if (element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), out var val)) return val;
            return null;
        }

        private string NormalizeTransactionType(string transactionType)
        {
            if (string.IsNullOrWhiteSpace(transactionType))
                return "DEBIT";
            
            // Normalize to uppercase and truncate to 10 characters
            var normalized = transactionType.Trim().ToUpper();
            
            // Map common variations to DEBIT/CREDIT
            if (normalized.StartsWith("DEBIT") || normalized.StartsWith("WITHDRAW") || normalized.StartsWith("PAYMENT") || normalized.StartsWith("OUT"))
                return "DEBIT";
            
            if (normalized.StartsWith("CREDIT") || normalized.StartsWith("DEPOSIT") || normalized.StartsWith("INCOME") || normalized.StartsWith("IN"))
                return "CREDIT";
            
            // Truncate to max 10 characters if it doesn't match known patterns
            return normalized.Length > 10 ? normalized.Substring(0, 10) : normalized;
        }

        private async Task AutoMatchStatementItemsAsync(string statementId, string userId) {
            var statement = await _context.BankStatements.Include(s => s.StatementItems).FirstOrDefaultAsync(s => s.Id == statementId);
            if (statement == null) return;
            foreach (var item in statement.StatementItems.Where(i => !i.IsMatched)) {
                var match = await _context.Payments.FirstOrDefaultAsync(p => p.BankAccountId == statement.BankAccountId && Math.Abs((decimal)(p.Amount - item.Amount)) < 0.01m);
                if (match != null) {
                    item.IsMatched = true; item.MatchedTransactionId = match.Id; item.MatchedTransactionType = "Payment";
                    item.MatchedAt = DateTime.UtcNow; item.MatchedBy = userId; item.UpdatedAt = DateTime.UtcNow;
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task CreateTransactionsFromUnmatchedItemsAsync(string statementId, string userId) {
            var statement = await _context.BankStatements.Include(s => s.StatementItems).FirstOrDefaultAsync(s => s.Id == statementId);
            if (statement == null) return;
            foreach (var item in statement.StatementItems.Where(i => !i.IsMatched)) {
                var res = await _bankAccountService.CreateTransactionAsync(new CreateBankTransactionDto {
                    BankAccountId = statement.BankAccountId, Amount = item.Amount, TransactionType = item.TransactionType,
                    Description = item.Description ?? "Statement Import", TransactionDate = item.TransactionDate,
                    Currency = "USD"
                }, userId);
                if (res.Success) {
                    item.IsMatched = true; item.MatchedTransactionId = res.Data.Id; item.MatchedTransactionType = "Payment";
                    item.MatchedAt = DateTime.UtcNow; item.MatchedBy = userId; item.UpdatedAt = DateTime.UtcNow;
                }
            }
            await _context.SaveChangesAsync();
        }

        private BankStatementUploadDto MapToUploadDto(BankStatementUpload u) => new BankStatementUploadDto {
            Id = u.Id, UserId = u.UserId, BankAccountId = u.BankAccountId, OriginalFileName = u.OriginalFileName,
            FileType = u.FileType, Status = u.Status, ErrorMessage = u.ErrorMessage,
            ProcessedBankStatementId = u.ProcessedBankStatementId, RetryCount = u.RetryCount,
            CreatedAt = u.CreatedAt, ProcessedAt = u.ProcessedAt, UpdatedAt = u.UpdatedAt
        };

        private StagingTransactionDto MapToStagingDto(StagingTransaction t) => new StagingTransactionDto {
            Id = t.Id, UploadId = t.UploadId, TransactionDate = t.TransactionDate, Amount = t.Amount,
            TransactionType = t.TransactionType, Description = t.Description, ReferenceNumber = t.ReferenceNumber,
            Merchant = t.Merchant, Category = t.Category, BalanceAfterTransaction = t.BalanceAfterTransaction
        };

        private BankStatementDto MapToBankStatementDto(BankStatement s) => new BankStatementDto {
            Id = s.Id, UserId = s.UserId, BankAccountId = s.BankAccountId, StatementName = s.StatementName,
            StatementStartDate = s.StatementStartDate, StatementEndDate = s.StatementEndDate, OpeningBalance = s.OpeningBalance,
            ClosingBalance = s.ClosingBalance, ImportFormat = s.ImportFormat, ImportSource = s.ImportSource,
            TotalTransactions = s.TotalTransactions, MatchedTransactions = s.MatchedTransactions,
            UnmatchedTransactions = s.UnmatchedTransactions, IsReconciled = s.IsReconciled,
            ReconciledAt = s.ReconciledAt, ReconciledBy = s.ReconciledBy, CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt,
            StatementItems = s.StatementItems?.Select(MapToBankStatementItemDto).ToList()
        };

        private BankStatementItemDto MapToBankStatementItemDto(BankStatementItem i) => new BankStatementItemDto {
            Id = i.Id, BankStatementId = i.BankStatementId, TransactionDate = i.TransactionDate, Amount = i.Amount,
            TransactionType = i.TransactionType, Description = i.Description, ReferenceNumber = i.ReferenceNumber,
            Merchant = i.Merchant, Category = i.Category, BalanceAfterTransaction = i.BalanceAfterTransaction,
            IsMatched = i.IsMatched, MatchedTransactionId = i.MatchedTransactionId,
            MatchedTransactionType = i.MatchedTransactionType, MatchedAt = i.MatchedAt, MatchedBy = i.MatchedBy,
            CreatedAt = i.CreatedAt, UpdatedAt = i.UpdatedAt
        };

        private ReconciliationDto MapToReconciliationDto(Reconciliation r) => new ReconciliationDto {
            Id = r.Id, UserId = r.UserId, BankAccountId = r.BankAccountId, BankStatementId = r.BankStatementId,
            ReconciliationName = r.ReconciliationName, ReconciliationDate = r.ReconciliationDate, BookBalance = r.BookBalance,
            StatementBalance = r.StatementBalance, Difference = r.Difference, TotalTransactions = r.TotalTransactions,
            MatchedTransactions = r.MatchedTransactions, UnmatchedTransactions = r.UnmatchedTransactions,
            PendingTransactions = r.PendingTransactions, Status = r.Status, Notes = r.Notes,
            CompletedAt = r.CompletedAt, CompletedBy = r.CompletedBy, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt,
            Matches = r.Matches?.Select(MapToReconciliationMatchDto).ToList()
        };

        private ReconciliationMatchDto MapToReconciliationMatchDto(ReconciliationMatch m) => new ReconciliationMatchDto {
            Id = m.Id, ReconciliationId = m.ReconciliationId, SystemTransactionId = m.SystemTransactionId,
            SystemTransactionType = m.SystemTransactionType, StatementItemId = m.StatementItemId,
            MatchType = m.MatchType, Amount = m.Amount, TransactionDate = m.TransactionDate,
            Description = m.Description, MatchStatus = m.MatchStatus, MatchNotes = m.MatchNotes,
            AmountDifference = m.AmountDifference, CreatedAt = m.CreatedAt, UpdatedAt = m.UpdatedAt, MatchedBy = m.MatchedBy
        };
    }
}
