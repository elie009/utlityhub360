using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IReconciliationService
    {
        // Bank Statement Operations
        Task<ApiResponse<BankStatementDto>> ImportBankStatementAsync(ImportBankStatementDto importDto, string userId);
        Task<ApiResponse<ExtractBankStatementResponseDto>> ExtractBankStatementFromFileAsync(Stream fileStream, string fileName, string bankAccountId, string userId);
        Task<ApiResponse<ExtractBankStatementResponseDto>> AnalyzePDFWithAIAsync(Stream pdfStream, string fileName, string bankAccountId, string userId);
        Task<ApiResponse<BankStatementDto>> GetBankStatementAsync(string statementId, string userId);
        Task<ApiResponse<List<BankStatementDto>>> GetBankStatementsAsync(string bankAccountId, string userId);
        Task<ApiResponse<bool>> DeleteBankStatementAsync(string statementId, string userId);

        // Reconciliation Operations
        Task<ApiResponse<ReconciliationDto>> CreateReconciliationAsync(CreateReconciliationDto createDto, string userId);
        Task<ApiResponse<ReconciliationDto>> GetReconciliationAsync(string reconciliationId, string userId);
        Task<ApiResponse<List<ReconciliationDto>>> GetReconciliationsAsync(string bankAccountId, string userId);
        Task<ApiResponse<ReconciliationDto>> AutoMatchTransactionsAsync(string reconciliationId, string userId);
        Task<ApiResponse<ReconciliationMatchDto>> MatchTransactionAsync(MatchTransactionDto matchDto, string userId);
        Task<ApiResponse<bool>> UnmatchTransactionAsync(UnmatchTransactionDto unmatchDto, string userId);
        Task<ApiResponse<ReconciliationDto>> CompleteReconciliationAsync(CompleteReconciliationDto completeDto, string userId);
        Task<ApiResponse<List<TransactionMatchSuggestionDto>>> GetMatchSuggestionsAsync(string reconciliationId, string userId);
        Task<ApiResponse<ReconciliationSummaryDto>> GetReconciliationSummaryAsync(string bankAccountId, DateTime? reconciliationDate, string userId);
    }
}

