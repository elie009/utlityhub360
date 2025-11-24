using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class ReconciliationService : IReconciliationService
    {
        private readonly ApplicationDbContext _context;

        public ReconciliationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== BANK STATEMENT OPERATIONS ====================

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

