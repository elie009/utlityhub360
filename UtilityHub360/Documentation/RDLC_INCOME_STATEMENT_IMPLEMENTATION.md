# RDLC Income Statement Implementation Guide

## Overview
This document describes the implementation of RDLC (Report Definition Language Client-side) reports for the Income Statement feature in UtilityHub360.

## Components

### 1. SQL Stored Procedure
**File**: `Database/StoredProcedures/SP_GetIncomeStatementReport.sql`

**Purpose**: Retrieves all income statement data from the database

**Parameters**:
- `@UserId` (NVARCHAR(450)) - User identifier
- `@StartDate` (DATETIME2) - Report period start date
- `@EndDate` (DATETIME2) - Report period end date

**Returns**: Two result sets
1. **Transaction Data**: All revenue and expense transactions with details
   - SectionType (REVENUE/EXPENSE)
   - CategoryName
   - CategoryCode
   - Description
   - Amount
   - TransactionDate
   - PaymentMethod
   - AccountName
   - AccountType

2. **Summary Data**: Aggregated totals
   - PeriodStart
   - PeriodEnd
   - TotalRevenue
   - TotalExpenses
   - NetIncome
   - Status (Profit/Loss)

**Data Sources**:
- Payments table (income and expenses)
- BankTransactions table (bank account transactions)
- Bills table (paid bills as expenses)

### 2. RDLC Report Definition
**File**: `Reports/IncomeStatement.rdlc`

**Format**: XML-based report definition

**Features**:
- Professional header with title and period
- Revenue section with detailed transactions
- Expenses section with detailed transactions
- Summary section with totals
- Color-coded sections (green for revenue, red for expenses)
- Dynamic Net Income coloring (green for profit, red for loss)
- Page numbers and generation timestamp
- Responsive layout (8.5" x 11" page)

**Data Sets**:
1. `IncomeStatementData` - Transaction details
2. `SummaryData` - Summary totals

### 3. RDLC Report Service
**File**: `Services/RdlcReportService.cs`

**Interface**: `IRdlcReportService`

**Methods**:
```csharp
Task<byte[]> GenerateIncomeStatementReportAsync(
    string userId, 
    DateTime startDate, 
    DateTime endDate, 
    string format = "PDF")
```

**Supported Formats**:
- PDF (default)
- Excel
- Word
- Image

**Process**:
1. Executes stored procedure to get data
2. Loads RDLC report definition
3. Binds data to report data sources
4. Renders report in specified format
5. Returns byte array of generated report

### 4. NuGet Packages Added
**File**: `UtilityHub360.csproj`

```xml
<PackageReference Include="Microsoft.ReportingServices.ReportViewerControl.Winforms" Version="150.1620.0" />
<PackageReference Include="Microsoft.SqlServer.Types" Version="160.1000.6" />
```

**Embedded Resources**:
```xml
<EmbeddedResource Include="Reports\**\*.rdlc" />
```

## Installation Steps

### Step 1: Run SQL Script
Execute the stored procedure script in your SQL Server database:

```bash
# Using SQL Server Management Studio (SSMS)
# Open: Database/StoredProcedures/SP_GetIncomeStatementReport.sql
# Execute against your UtilityHub360 database

# Or using sqlcmd
sqlcmd -S your_server -d UtilityHub360 -i Database/StoredProcedures/SP_GetIncomeStatementReport.sql
```

### Step 2: Restore NuGet Packages
```bash
cd utlityhub360-backend/UtilityHub360
dotnet restore
```

### Step 3: Register Service
Add to `Program.cs` or `Startup.cs`:

```csharp
// Register RDLC Report Service
builder.Services.AddScoped<IRdlcReportService, RdlcReportService>();
```

### Step 4: Add API Endpoint
Create controller method:

```csharp
[HttpGet("income-statement/rdlc")]
[Authorize]
public async Task<IActionResult> GetIncomeStatementRdlcReport(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] string format = "PDF")
{
    try
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reportBytes = await _rdlcReportService.GenerateIncomeStatementReportAsync(
            userId, 
            startDate, 
            endDate, 
            format.ToUpper());

        var contentType = format.ToUpper() switch
        {
            "PDF" => "application/pdf",
            "EXCEL" => "application/vnd.ms-excel",
            "WORD" => "application/msword",
            "IMAGE" => "image/png",
            _ => "application/pdf"
        };

        var fileName = $"IncomeStatement_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format.ToLower()}";

        return File(reportBytes, contentType, fileName);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generating RDLC report");
        return StatusCode(500, new { message = "Error generating report", error = ex.Message });
    }
}
```

### Step 5: Update Frontend
Add button to download RDLC report:

```typescript
const downloadRdlcReport = async (format: 'PDF' | 'EXCEL' | 'WORD' = 'PDF') => {
  try {
    const response = await apiService.get(
      `/api/financial-reports/income-statement/rdlc`,
      {
        params: {
          startDate: localStartDate,
          endDate: localEndDate,
          format: format
        },
        responseType: 'blob'
      }
    );

    const blob = new Blob([response.data]);
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `IncomeStatement_${localStartDate}_${localEndDate}.${format.toLowerCase()}`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  } catch (error) {
    console.error('Error downloading RDLC report:', error);
  }
};
```

## Usage

### API Call Example
```http
GET /api/financial-reports/income-statement/rdlc?startDate=2024-01-01&endDate=2024-01-31&format=PDF
Authorization: Bearer {token}
```

### Response
- Content-Type: application/pdf (or appropriate type)
- Binary data of generated report

## Features

### Report Sections
1. **Header**
   - Report title
   - Period range

2. **Revenue Section**
   - Grouped by category
   - Individual transaction details
   - Total revenue (green highlight)

3. **Expenses Section**
   - Grouped by category
   - Individual transaction details
   - Total expenses (red highlight)

4. **Net Income**
   - Calculated total
   - Color-coded based on profit/loss
   - Double border for emphasis

5. **Footer**
   - Generation timestamp
   - Page numbers

### Styling
- Professional business report layout
- Color-coded sections for easy reading
- Clear typography (Arial font family)
- Proper spacing and borders
- Print-friendly design

## Troubleshooting

### Common Issues

1. **Report file not found**
   - Ensure `IncomeStatement.rdlc` is in `Reports` folder
   - Check that file is set as Embedded Resource
   - Verify build action in project file

2. **Stored procedure not found**
   - Execute SQL script in correct database
   - Check connection string
   - Verify user permissions

3. **No data in report**
   - Check date range parameters
   - Verify user has transactions in period
   - Check stored procedure execution manually

4. **Rendering errors**
   - Ensure all NuGet packages are installed
   - Check .NET version compatibility
   - Verify RDLC file XML structure

### Debugging

Enable detailed logging:
```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

## Performance Considerations

1. **Database**
   - Stored procedure uses efficient queries
   - Indexes on TransactionDate, UserId recommended
   - Consider pagination for large datasets

2. **Report Generation**
   - Report rendering is CPU-intensive
   - Consider caching for frequently requested reports
   - Use async methods to avoid blocking

3. **File Size**
   - PDF files are typically 50-500 KB
   - Excel files may be larger with many transactions
   - Consider compression for large reports

## Security

1. **Authorization**
   - Always verify user identity
   - Ensure user can only access their own data
   - Use [Authorize] attribute on endpoints

2. **SQL Injection**
   - Stored procedure uses parameterized queries
   - No dynamic SQL construction
   - Safe from injection attacks

3. **Data Privacy**
   - Reports contain sensitive financial data
   - Use HTTPS for transmission
   - Consider encryption at rest

## Future Enhancements

1. **Additional Reports**
   - Balance Sheet RDLC
   - Cash Flow Statement RDLC
   - Custom report templates

2. **Features**
   - Email report delivery
   - Scheduled report generation
   - Report templates customization
   - Multi-currency support

3. **Performance**
   - Report caching
   - Async generation with notifications
   - Batch report generation

## Support

For issues or questions:
1. Check logs in `Logs` folder
2. Review stored procedure execution
3. Verify RDLC file structure
4. Test with sample data

## References

- [Microsoft Reporting Services Documentation](https://docs.microsoft.com/en-us/sql/reporting-services/)
- [RDLC Report Designer](https://docs.microsoft.com/en-us/visualstudio/reporting-services/tools/design-reporting-services-paginated-reports-with-report-designer-ssrs)
- [ReportViewer Control](https://docs.microsoft.com/en-us/dotnet/api/microsoft.reporting.winforms.reportviewer)
