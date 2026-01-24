using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using UtilityHub360.Data;
using Microsoft.Reporting.NETCore;

namespace UtilityHub360.Services
{
    public interface IRdlcReportService
    {
        Task<byte[]> GenerateIncomeStatementReportAsync(string userId, DateTime startDate, DateTime endDate, string format = "PDF");
        Task<byte[]> GenerateBalanceSheetReportAsync(string userId, DateTime asOfDate, DateTime? startDate, DateTime? endDate, string format = "PDF");
        Task<byte[]> GenerateCashFlowReportAsync(string userId, DateTime startDate, DateTime endDate, string format = "PDF");
    }

    public class RdlcReportService : IRdlcReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RdlcReportService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RdlcReportService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<RdlcReportService> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<byte[]> GenerateIncomeStatementReportAsync(
            string userId, 
            DateTime startDate, 
            DateTime endDate, 
            string format = "PDF")
        {
            try
            {
                _logger.LogInformation($"Generating Income Statement Report for user {userId} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Get data from stored procedure
                var (transactionData, summaryData) = await GetIncomeStatementDataAsync(userId, startDate, endDate);

                // Load RDLC report
                var reportPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "IncomeStatement.rdlc");
                
                if (!File.Exists(reportPath))
                {
                    throw new FileNotFoundException($"RDLC report file not found at: {reportPath}");
                }

                // Initialize encoding provider
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // Create LocalReport instance
                using var localReport = new LocalReport();
                localReport.ReportPath = reportPath;

                // Add data sources
                localReport.DataSources.Add(new ReportDataSource("IncomeStatementData", transactionData));
                localReport.DataSources.Add(new ReportDataSource("SummaryData", summaryData));

                // Determine the render format
                string renderFormat = format.ToUpper() switch
                {
                    "PDF" => "PDF",
                    "EXCEL" => "EXCELOPENXML",
                    "WORD" => "WORDOPENXML",
                    _ => "PDF"
                };

                // Render the report
                string mimeType;
                string encoding;
                string fileNameExtension;
                Warning[] warnings;
                string[] streams;

                var renderedBytes = localReport.Render(
                    renderFormat,
                    null,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings
                );

                _logger.LogInformation($"Report generated successfully. Size: {renderedBytes.Length} bytes");

                return renderedBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating Income Statement Report: {ex.Message}");
                throw;
            }
        }

        private async Task<(DataTable transactionData, DataTable summaryData)> GetIncomeStatementDataAsync(
            string userId, 
            DateTime startDate, 
            DateTime endDate)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Create DataTables
            var transactionData = new DataTable("IncomeStatementData");
            var summaryData = new DataTable("SummaryData");

            using (var command = new SqlCommand("SP_GetIncomeStatementReport", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                command.CommandTimeout = 120; // 2 minutes timeout

                using var adapter = new SqlDataAdapter(command);
                var dataSet = new DataSet();
                await Task.Run(() => adapter.Fill(dataSet));

                if (dataSet.Tables.Count >= 1)
                {
                    transactionData = dataSet.Tables[0];
                    transactionData.TableName = "IncomeStatementData";
                }

                if (dataSet.Tables.Count >= 2)
                {
                    summaryData = dataSet.Tables[1];
                    summaryData.TableName = "SummaryData";
                }
            }

            _logger.LogInformation($"Retrieved {transactionData.Rows.Count} transaction rows and {summaryData.Rows.Count} summary rows");

            return (transactionData, summaryData);
        }

        public async Task<byte[]> GenerateBalanceSheetReportAsync(
            string userId,
            DateTime asOfDate,
            DateTime? startDate,
            DateTime? endDate,
            string format = "PDF")
        {
            try
            {
                _logger.LogInformation($"Generating Balance Sheet Report for user {userId} as of {asOfDate:yyyy-MM-dd}");

                // Get data from stored procedure
                var (balanceSheetData, summaryData) = await GetBalanceSheetDataAsync(userId, asOfDate, startDate, endDate);

                // Load RDLC report
                var reportPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "BalanceSheet.rdlc");

                if (!File.Exists(reportPath))
                {
                    throw new FileNotFoundException($"RDLC report file not found at: {reportPath}");
                }

                // Initialize encoding provider
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // Create LocalReport instance
                using var localReport = new LocalReport();
                localReport.ReportPath = reportPath;

                // Add data sources
                localReport.DataSources.Add(new ReportDataSource("BalanceSheetData", balanceSheetData));
                localReport.DataSources.Add(new ReportDataSource("SummaryData", summaryData));

                // Determine the render format
                string renderFormat = format.ToUpper() switch
                {
                    "PDF" => "PDF",
                    "EXCEL" => "EXCELOPENXML",
                    "WORD" => "WORDOPENXML",
                    _ => "PDF"
                };

                // Render the report
                string mimeType;
                string encoding;
                string fileNameExtension;
                Warning[] warnings;
                string[] streams;

                var renderedBytes = localReport.Render(
                    renderFormat,
                    null,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings
                );

                _logger.LogInformation($"Balance Sheet report generated successfully. Size: {renderedBytes.Length} bytes");

                return renderedBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating Balance Sheet Report: {ex.Message}");
                throw;
            }
        }

        private async Task<(DataTable balanceSheetData, DataTable summaryData)> GetBalanceSheetDataAsync(
            string userId,
            DateTime asOfDate,
            DateTime? startDate,
            DateTime? endDate)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Create DataTables
            var balanceSheetData = new DataTable("BalanceSheetData");
            var summaryData = new DataTable("SummaryData");

            using (var command = new SqlCommand("SP_GetBalanceSheetReport", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@AsOfDate", asOfDate);
                command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = startDate.HasValue ? (object)startDate.Value : DBNull.Value;
                command.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value = endDate.HasValue ? (object)endDate.Value : DBNull.Value;
                command.CommandTimeout = 120; // 2 minutes timeout

                using var adapter = new SqlDataAdapter(command);
                var dataSet = new DataSet();
                await Task.Run(() => adapter.Fill(dataSet));

                if (dataSet.Tables.Count >= 1)
                {
                    balanceSheetData = dataSet.Tables[0];
                    balanceSheetData.TableName = "BalanceSheetData";
                    
                    // Log all data before filtering for debugging
                    _logger.LogInformation($"Before filtering: {balanceSheetData.Rows.Count} rows");
                    
                    // Filter out rows where AccountType or AccountName contains "Bank Account"
                    var rowsToRemove = new List<DataRow>();
                    foreach (DataRow row in balanceSheetData.Rows)
                    {
                        var accountType = row["AccountType"] != DBNull.Value ? row["AccountType"].ToString()?.Trim() : "";
                        var accountName = row["AccountName"] != DBNull.Value ? row["AccountName"].ToString()?.Trim() : "";
                        var category = row["Category"] != DBNull.Value ? row["Category"].ToString()?.Trim() : "";
                        
                        // Log each row for debugging
                        _logger.LogDebug($"Row - AccountType: '{accountType}', AccountName: '{accountName}', Category: '{category}'");
                        
                        // Remove if AccountType or AccountName or Category equals "Bank Account" (but keep "Bank Statement")
                        if ((accountType.Equals("Bank Account", StringComparison.OrdinalIgnoreCase)) ||
                            (accountName.Equals("Bank Account", StringComparison.OrdinalIgnoreCase)) ||
                            (category.Equals("Bank Account", StringComparison.OrdinalIgnoreCase)) ||
                            (accountType.Contains("Bank Account", StringComparison.OrdinalIgnoreCase) && !accountType.Contains("Statement", StringComparison.OrdinalIgnoreCase)) ||
                            (accountName.Contains("Bank Account", StringComparison.OrdinalIgnoreCase) && !accountName.Contains("Statement", StringComparison.OrdinalIgnoreCase)) ||
                            (category.Contains("Bank Account", StringComparison.OrdinalIgnoreCase) && !category.Contains("Statement", StringComparison.OrdinalIgnoreCase)))
                        {
                            _logger.LogInformation($"Removing row - AccountType: '{accountType}', AccountName: '{accountName}', Category: '{category}'");
                            rowsToRemove.Add(row);
                        }
                    }
                    
                    foreach (var row in rowsToRemove)
                    {
                        balanceSheetData.Rows.Remove(row);
                    }
                    
                    _logger.LogInformation($"Filtered out {rowsToRemove.Count} rows with 'Bank Account' (keeping 'Bank Statement'). Remaining: {balanceSheetData.Rows.Count} rows");
                }

                if (dataSet.Tables.Count >= 2)
                {
                    summaryData = dataSet.Tables[1];
                    summaryData.TableName = "SummaryData";
                }
            }

            _logger.LogInformation($"Retrieved {balanceSheetData.Rows.Count} balance sheet rows and {summaryData.Rows.Count} summary rows");

            return (balanceSheetData, summaryData);
        }

        public async Task<byte[]> GenerateCashFlowReportAsync(
            string userId,
            DateTime startDate,
            DateTime endDate,
            string format = "PDF")
        {
            try
            {
                _logger.LogInformation($"Generating Cash Flow Report for user {userId} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var (cashFlowData, summaryData) = await GetCashFlowDataAsync(userId, startDate, endDate);

                var reportPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "CashFlowStatement.rdlc");

                if (!File.Exists(reportPath))
                {
                    throw new FileNotFoundException($"RDLC report file not found at: {reportPath}");
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var localReport = new LocalReport();
                localReport.ReportPath = reportPath;

                localReport.DataSources.Add(new ReportDataSource("CashFlowData", cashFlowData));
                localReport.DataSources.Add(new ReportDataSource("SummaryData", summaryData));

                string renderFormat = format.ToUpper() switch
                {
                    "PDF" => "PDF",
                    "EXCEL" => "EXCELOPENXML",
                    "WORD" => "WORDOPENXML",
                    _ => "PDF"
                };

                string mimeType;
                string encoding;
                string fileNameExtension;
                Warning[] warnings;
                string[] streams;

                var renderedBytes = localReport.Render(
                    renderFormat,
                    null,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings
                );

                _logger.LogInformation($"Cash Flow report generated successfully. Size: {renderedBytes.Length} bytes");

                return renderedBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating Cash Flow Report: {ex.Message}");
                throw;
            }
        }

        private async Task<(DataTable cashFlowData, DataTable summaryData)> GetCashFlowDataAsync(
            string userId,
            DateTime startDate,
            DateTime endDate)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var cashFlowData = new DataTable("CashFlowData");
            var summaryData = new DataTable("SummaryData");

            using (var command = new SqlCommand("SP_GetCashFlowReport", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                command.CommandTimeout = 120;

                using var adapter = new SqlDataAdapter(command);
                var dataSet = new DataSet();
                await Task.Run(() => adapter.Fill(dataSet));

                if (dataSet.Tables.Count >= 1)
                {
                    cashFlowData = dataSet.Tables[0];
                    cashFlowData.TableName = "CashFlowData";
                }

                if (dataSet.Tables.Count >= 2)
                {
                    summaryData = dataSet.Tables[1];
                    summaryData.TableName = "SummaryData";
                }
            }

            _logger.LogInformation($"Retrieved {cashFlowData.Rows.Count} cash flow rows and {summaryData.Rows.Count} summary rows");

            return (cashFlowData, summaryData);
        }
    }
}
