# PowerShell script to verify and fix missing BankTransactions columns
# This script checks if the columns exist and adds them if they don't

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BankTransactions Column Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get connection string from appsettings.json
$appsettingsPath = "appsettings.json"
if (-not (Test-Path $appsettingsPath)) {
    $appsettingsPath = "appsettings.Development.json"
}

if (-not (Test-Path $appsettingsPath)) {
    Write-Host "ERROR: Could not find appsettings.json or appsettings.Development.json" -ForegroundColor Red
    exit 1
}

Write-Host "Reading connection string from: $appsettingsPath" -ForegroundColor Yellow
$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.DefaultConnection

if (-not $connectionString) {
    Write-Host "ERROR: Could not find DefaultConnection in appsettings" -ForegroundColor Red
    exit 1
}

Write-Host "Connection String: $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray
Write-Host ""

# Extract database name from connection string
$dbName = ""
if ($connectionString -match "Database=([^;]+)") {
    $dbName = $matches[1]
    Write-Host "Database Name: $dbName" -ForegroundColor Yellow
} else {
    Write-Host "WARNING: Could not extract database name from connection string" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SQL Script Location:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$scriptPath = "Documentation\Database\Scripts\add_transaction_linking_fields.sql"
$fullScriptPath = Join-Path $PSScriptRoot "..\$scriptPath" | Resolve-Path -ErrorAction SilentlyContinue

if ($fullScriptPath) {
    Write-Host "Script found at: $fullScriptPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "To run this script:" -ForegroundColor Yellow
    Write-Host "1. Open SQL Server Management Studio (SSMS)" -ForegroundColor White
    Write-Host "2. Connect to your database server" -ForegroundColor White
    Write-Host "3. Select database: $dbName" -ForegroundColor White
    Write-Host "4. Open file: $fullScriptPath" -ForegroundColor White
    Write-Host "5. Execute the script (F5)" -ForegroundColor White
} else {
    Write-Host "Script not found at: $scriptPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run this SQL manually:" -ForegroundColor Yellow
    Write-Host @"
-- Add missing columns to BankTransactions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'BillId')
    ALTER TABLE [dbo].[BankTransactions] ADD [BillId] NVARCHAR(450) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'LoanId')
    ALTER TABLE [dbo].[BankTransactions] ADD [LoanId] NVARCHAR(450) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'SavingsAccountId')
    ALTER TABLE [dbo].[BankTransactions] ADD [SavingsAccountId] NVARCHAR(450) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BankTransactions]') AND name = 'TransactionPurpose')
    ALTER TABLE [dbo].[BankTransactions] ADD [TransactionPurpose] NVARCHAR(50) NULL;
"@ -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verification Query" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "After running the script, verify with:" -ForegroundColor Yellow
Write-Host @"
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'BankTransactions' 
AND COLUMN_NAME IN ('BillId', 'LoanId', 'SavingsAccountId', 'TransactionPurpose');
"@ -ForegroundColor White
Write-Host ""
Write-Host "You should see 4 rows returned." -ForegroundColor Green

