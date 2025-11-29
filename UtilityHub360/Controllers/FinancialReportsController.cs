using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/Reports")]
    [Authorize]
    public class FinancialReportsController : ControllerBase
    {
        private readonly IFinancialReportService _reportService;

        public FinancialReportsController(IFinancialReportService reportService)
        {
            _reportService = reportService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("User not authenticated.");
        }

        /// <summary>
        /// Get complete financial report with all sections
        /// </summary>
        [HttpGet("full")]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> GetFullReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                Console.WriteLine($"[CONTROLLER DEBUG] GetFullReport called - Authenticated UserId: {userId}");
                var result = await _reportService.GenerateFullReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<FinancialReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<FinancialReportDto>.ErrorResult($"Failed to generate report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get financial summary (dashboard)
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<ReportFinancialSummaryDto>>> GetSummary([FromQuery] DateTime? date = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetFinancialSummaryAsync(userId, date);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<ReportFinancialSummaryDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReportFinancialSummaryDto>.ErrorResult($"Failed to get summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get income report
        /// </summary>
        [HttpGet("income")]
        public async Task<ActionResult<ApiResponse<IncomeReportDto>>> GetIncomeReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetIncomeReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<IncomeReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IncomeReportDto>.ErrorResult($"Failed to get income report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get expense report
        /// </summary>
        [HttpGet("expenses")]
        public async Task<ActionResult<ApiResponse<ExpenseReportDto>>> GetExpenseReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetExpenseReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<ExpenseReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseReportDto>.ErrorResult($"Failed to get expense report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get disposable income report
        /// </summary>
        [HttpGet("disposable-income")]
        public async Task<ActionResult<ApiResponse<DisposableIncomeReportDto>>> GetDisposableIncomeReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetDisposableIncomeReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<DisposableIncomeReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<DisposableIncomeReportDto>.ErrorResult($"Failed to get disposable income report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get bills and utilities report
        /// </summary>
        [HttpGet("bills")]
        public async Task<ActionResult<ApiResponse<BillsReportDto>>> GetBillsReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetBillsReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<BillsReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BillsReportDto>.ErrorResult($"Failed to get bills report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get loan and debt report
        /// </summary>
        [HttpGet("loans")]
        public async Task<ActionResult<ApiResponse<LoanReportDto>>> GetLoanReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetLoanReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<LoanReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoanReportDto>.ErrorResult($"Failed to get loan report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get savings and goal progress report
        /// </summary>
        [HttpGet("savings")]
        public async Task<ActionResult<ApiResponse<SavingsReportDto>>> GetSavingsReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetSavingsReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<SavingsReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SavingsReportDto>.ErrorResult($"Failed to get savings report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get net worth report
        /// </summary>
        [HttpGet("networth")]
        public async Task<ActionResult<ApiResponse<NetWorthReportDto>>> GetNetWorthReport([FromQuery] ReportQueryDto query)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetNetWorthReportAsync(userId, query);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<NetWorthReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NetWorthReportDto>.ErrorResult($"Failed to get net worth report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get balance sheet (Assets, Liabilities, Equity)
        /// </summary>
        [HttpGet("balance-sheet")]
        public async Task<ActionResult<ApiResponse<BalanceSheetDto>>> GetBalanceSheet([FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetBalanceSheetAsync(userId, asOfDate);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<BalanceSheetDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BalanceSheetDto>.ErrorResult($"Failed to get balance sheet: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get income statement (Revenue, Expenses, Net Income)
        /// </summary>
        [HttpGet("income-statement")]
        public async Task<ActionResult<ApiResponse<IncomeStatementDto>>> GetIncomeStatement(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string period = "MONTHLY",
            [FromQuery] bool includeComparison = false)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetIncomeStatementAsync(userId, startDate, endDate, period, includeComparison);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<IncomeStatementDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IncomeStatementDto>.ErrorResult($"Failed to get income statement: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get financial insights (AI-generated alerts, tips, forecasts)
        /// </summary>
        [HttpGet("insights")]
        public async Task<ActionResult<ApiResponse<List<FinancialInsightDto>>>> GetInsights([FromQuery] DateTime? date = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetFinancialInsightsAsync(userId, date);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<FinancialInsightDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<FinancialInsightDto>>.ErrorResult($"Failed to get insights: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get financial predictions (next month forecasts)
        /// </summary>
        [HttpGet("predictions")]
        public async Task<ActionResult<ApiResponse<List<FinancialPredictionDto>>>> GetPredictions()
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetFinancialPredictionsAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<FinancialPredictionDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<FinancialPredictionDto>>.ErrorResult($"Failed to get predictions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get cash flow statement (Operating, Investing, Financing activities)
        /// </summary>
        [HttpGet("cashflow-statement")]
        public async Task<ActionResult<ApiResponse<CashFlowStatementDto>>> GetCashFlowStatement(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string period = "MONTHLY")
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetCashFlowStatementAsync(userId, startDate, endDate, period);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<CashFlowStatementDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CashFlowStatementDto>.ErrorResult($"Failed to get cash flow statement: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get cash flow projection for the next N months
        /// </summary>
        [HttpGet("cashflow-projection")]
        public async Task<ActionResult<ApiResponse<CashFlowProjectionDto>>> GetCashFlowProjection([FromQuery] int monthsAhead = 6)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetCashFlowProjectionAsync(userId, monthsAhead);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<CashFlowProjectionDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CashFlowProjectionDto>.ErrorResult($"Failed to get cash flow projection: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get financial ratios (liquidity, debt, profitability, efficiency)
        /// </summary>
        [HttpGet("financial-ratios")]
        public async Task<ActionResult<ApiResponse<FinancialRatiosDto>>> GetFinancialRatios([FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetFinancialRatiosAsync(userId, asOfDate);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<FinancialRatiosDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<FinancialRatiosDto>.ErrorResult($"Failed to get financial ratios: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get tax report for a specific tax year
        /// </summary>
        [HttpGet("tax-report")]
        public async Task<ActionResult<ApiResponse<TaxReportDto>>> GetTaxReport(
            [FromQuery] int taxYear,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetTaxReportAsync(userId, taxYear, startDate, endDate);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<TaxReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TaxReportDto>.ErrorResult($"Failed to get tax report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get recent transaction logs
        /// </summary>
        [HttpGet("transactions/recent")]
        public async Task<ActionResult<ApiResponse<List<TransactionLogDto>>>> GetRecentTransactions([FromQuery] int limit = 20)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetTransactionLogsAsync(userId, limit);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<TransactionLogDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionLogDto>>.ErrorResult($"Failed to get transactions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Compare two time periods
        /// </summary>
        [HttpGet("compare")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> ComparePeriods(
            [FromQuery] DateTime period1Start,
            [FromQuery] DateTime period1End,
            [FromQuery] DateTime period2Start,
            [FromQuery] DateTime period2End)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.ComparePeriodsAsync(userId, period1Start, period1End, period2Start, period2End);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<Dictionary<string, object>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult($"Failed to compare periods: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export report as PDF
        /// </summary>
        [HttpPost("export/pdf")]
        public async Task<IActionResult> ExportPdf([FromBody] ExportReportDto exportDto)
        {
            try
            {
                var userId = GetUserId();
                var pdfBytes = await _reportService.ExportReportToPdfAsync(userId, exportDto);

                return File(pdfBytes, "application/pdf", $"Financial_Report_{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to export PDF: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export report as CSV
        /// </summary>
        [HttpPost("export/csv")]
        public async Task<IActionResult> ExportCsv([FromBody] ExportReportDto exportDto)
        {
            try
            {
                var userId = GetUserId();
                var csvBytes = await _reportService.ExportReportToCsvAsync(userId, exportDto);

                return File(csvBytes, "text/csv", $"Financial_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to export CSV: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export report as Excel
        /// </summary>
        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportReportDto exportDto)
        {
            try
            {
                var userId = GetUserId();
                // Note: Excel export would need EPPlus or ClosedXML library
                // For now, return CSV format with .xlsx extension
                var csvBytes = await _reportService.ExportReportToCsvAsync(userId, exportDto);
                return File(csvBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Financial_Report_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to export Excel: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get Budget vs Actual report
        /// </summary>
        [HttpGet("budget-vs-actual")]
        public async Task<ActionResult<ApiResponse<BudgetVsActualReportDto>>> GetBudgetVsActualReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string period = "MONTHLY")
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetBudgetVsActualReportAsync(userId, startDate, endDate, period);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<BudgetVsActualReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BudgetVsActualReportDto>.ErrorResult($"Failed to get budget vs actual report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Generate custom report
        /// </summary>
        [HttpPost("custom")]
        public async Task<ActionResult<ApiResponse<CustomReportDto>>> GenerateCustomReport([FromBody] CustomReportRequestDto request)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GenerateCustomReportAsync(userId, request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<CustomReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CustomReportDto>.ErrorResult($"Failed to generate custom report: {ex.Message}"));
            }
        }

        /// <summary>
        /// Save custom report template
        /// </summary>
        [HttpPost("custom/templates")]
        public async Task<ActionResult<ApiResponse<CustomReportTemplateDto>>> SaveCustomReportTemplate([FromBody] SaveCustomReportTemplateDto template)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.SaveCustomReportTemplateAsync(userId, template);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<CustomReportTemplateDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CustomReportTemplateDto>.ErrorResult($"Failed to save template: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all custom report templates
        /// </summary>
        [HttpGet("custom/templates")]
        public async Task<ActionResult<ApiResponse<List<CustomReportTemplateDto>>>> GetCustomReportTemplates()
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetCustomReportTemplatesAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<List<CustomReportTemplateDto>>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<CustomReportTemplateDto>>.ErrorResult($"Failed to get templates: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get custom report template by ID
        /// </summary>
        [HttpGet("custom/templates/{templateId}")]
        public async Task<ActionResult<ApiResponse<CustomReportTemplateDto>>> GetCustomReportTemplate(string templateId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.GetCustomReportTemplateAsync(userId, templateId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<CustomReportTemplateDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CustomReportTemplateDto>.ErrorResult($"Failed to get template: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete custom report template
        /// </summary>
        [HttpDelete("custom/templates/{templateId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCustomReportTemplate(string templateId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reportService.DeleteCustomReportTemplateAsync(userId, templateId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete template: {ex.Message}"));
            }
        }
    }
}

