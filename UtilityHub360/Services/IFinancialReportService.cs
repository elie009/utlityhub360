using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IFinancialReportService
    {
        // Main Report Generation
        Task<ApiResponse<FinancialReportDto>> GenerateFullReportAsync(string userId, ReportQueryDto query);
        
        // Individual Report Sections
        Task<ApiResponse<IncomeReportDto>> GetIncomeReportAsync(string userId, ReportQueryDto query);
        Task<ApiResponse<ExpenseReportDto>> GetExpenseReportAsync(string userId, ReportQueryDto query);
        Task<ApiResponse<DisposableIncomeReportDto>> GetDisposableIncomeReportAsync(string userId, ReportQueryDto query);
        Task<ApiResponse<BillsReportDto>> GetBillsReportAsync(string userId, ReportQueryDto query);
        Task<ApiResponse<LoanReportDto>> GetLoanReportAsync(string userId, ReportQueryDto query);
        Task<ApiResponse<SavingsReportDto>> GetSavingsReportAsync(string userId, ReportQueryDto query);
        Task<ApiResponse<NetWorthReportDto>> GetNetWorthReportAsync(string userId, ReportQueryDto query);
        Task<ApiResponse<BalanceSheetDto>> GetBalanceSheetAsync(string userId, DateTime? asOfDate = null);
        Task<ApiResponse<CashFlowStatementDto>> GetCashFlowStatementAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, string period = "MONTHLY");
        Task<ApiResponse<IncomeStatementDto>> GetIncomeStatementAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, string period = "MONTHLY", bool includeComparison = false);
        
        // Summary & Dashboard
        Task<ApiResponse<ReportFinancialSummaryDto>> GetFinancialSummaryAsync(string userId, DateTime? date = null);
        
        // Insights & Predictions
        Task<ApiResponse<List<FinancialInsightDto>>> GetFinancialInsightsAsync(string userId, DateTime? date = null);
        Task<ApiResponse<List<FinancialPredictionDto>>> GetFinancialPredictionsAsync(string userId);
        Task<ApiResponse<CashFlowProjectionDto>> GetCashFlowProjectionAsync(string userId, int monthsAhead = 6);
        
        // Full Report
        Task<ApiResponse<FullFinancialReportDto>> GetFullFinancialReportAsync(string userId, string period = "MONTHLY", bool includeComparison = false, bool includeInsights = false, bool includePredictions = false, bool includeTransactions = false);
        
        // Transaction Logs
        Task<ApiResponse<List<TransactionLogDto>>> GetTransactionLogsAsync(string userId, int limit = 20);
        
        // Comparisons
        Task<ApiResponse<Dictionary<string, object>>> ComparePeriodsAsync(string userId, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End);
        
        // Export Functionality
        Task<byte[]> ExportReportToPdfAsync(string userId, ExportReportDto exportDto);
        Task<byte[]> ExportReportToCsvAsync(string userId, ExportReportDto exportDto);
        
        // Financial Ratios
        Task<ApiResponse<FinancialRatiosDto>> GetFinancialRatiosAsync(string userId, DateTime? asOfDate = null);
        
        // Tax Reporting
        Task<ApiResponse<TaxReportDto>> GetTaxReportAsync(string userId, int taxYear, DateTime? startDate = null, DateTime? endDate = null);
        
        // Budget vs Actual
        Task<ApiResponse<BudgetVsActualReportDto>> GetBudgetVsActualReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, string period = "MONTHLY");
        
        // Custom Report Builder
        Task<ApiResponse<CustomReportDto>> GenerateCustomReportAsync(string userId, CustomReportRequestDto request);
        Task<ApiResponse<CustomReportTemplateDto>> SaveCustomReportTemplateAsync(string userId, SaveCustomReportTemplateDto template);
        Task<ApiResponse<List<CustomReportTemplateDto>>> GetCustomReportTemplatesAsync(string userId);
        Task<ApiResponse<CustomReportTemplateDto>> GetCustomReportTemplateAsync(string userId, string templateId);
        Task<ApiResponse<bool>> DeleteCustomReportTemplateAsync(string userId, string templateId);
    }
}

