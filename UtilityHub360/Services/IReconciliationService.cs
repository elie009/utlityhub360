using UtilityHub360.DTOs;
using UtilityHub360.Models;
using Microsoft.AspNetCore.Http;

namespace UtilityHub360.Services
{
    public interface IReconciliationService
    {
        // Bank Statement Operations
        Task<ApiResponse<BankStatementDto>> ImportBankStatementAsync(ImportBankStatementDto importDto, string userId);
        Task<ApiResponse<BankStatementDto>> GetBankStatementAsync(string statementId, string userId);
        Task<ApiResponse<List<BankStatementDto>>> GetBankStatementsAsync(string bankAccountId, string userId);
        Task<ApiResponse<bool>> DeleteBankStatementAsync(string statementId, string userId);

        // Bank Statement Upload Operations
        Task<ApiResponse<BankStatementUploadDto>> UploadBankStatementAsync(IFormFile file, string bankAccountId, string userId);
        Task<ApiResponse<List<BankStatementUploadDto>>> GetPendingUploadsAsync(int limit);
        Task<ApiResponse<List<BankStatementUploadDto>>> GetUserUploadsAsync(string bankAccountId, string userId);
        Task<ApiResponse<(Stream fileStream, string fileName, string contentType)>> GetUploadFileAsync(string uploadId);
        Task<ApiResponse<bool>> ProcessExtractedTextAsync(ProcessExtractedTextDto processDto);
        Task<ApiResponse<List<StagingTransactionDto>>> GetStagingTransactionsAsync(string uploadId, string userId);
        Task<ApiResponse<bool>> SaveStagingTransactionsAsync(string uploadId, ConfirmBankStatementUploadDto saveDto, string userId);
        Task<ApiResponse<BankStatementDto>> ConfirmUploadAsync(string uploadId, ConfirmBankStatementUploadDto confirmDto, string userId);
        Task<ApiResponse<BankStatementUploadDto>> GetUploadStatusAsync(string uploadId, string userId);
        Task<ApiResponse<bool>> CancelUploadAsync(string uploadId, string userId);
        Task<ApiResponse<bool>> UpdateUploadErrorAsync(string uploadId, string errorMessage);
        
        // Bank Statement Extraction Operations
        Task<ApiResponse<ExtractBankStatementResponseDto>> ExtractBankStatementFromFileAsync(Stream fileStream, string fileName, string bankAccountId, string userId);
        Task<ApiResponse<ExtractBankStatementResponseDto>> AnalyzePDFWithAIAsync(Stream fileStream, string fileName, string bankAccountId, string userId);

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
