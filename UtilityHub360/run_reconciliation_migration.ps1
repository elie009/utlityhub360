# PowerShell script to apply reconciliation tables migration
# This script will help you run the SQL migration script

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Reconciliation Tables Migration Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Get connection details
Write-Host "Please provide your database connection details:" -ForegroundColor Yellow
$server = Read-Host "SQL Server name (e.g., localhost, localhost\SQLEXPRESS)"
$database = Read-Host "Database name"
$useWindowsAuth = Read-Host "Use Windows Authentication? (Y/N)"

$sqlScriptPath = Join-Path $PSScriptRoot "Documentation\Database\Scripts\create_reconciliation_tables.sql"

if (-not (Test-Path $sqlScriptPath)) {
    Write-Host "ERROR: SQL script not found at: $sqlScriptPath" -ForegroundColor Red
    Write-Host "Please ensure the script exists." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Reading SQL script..." -ForegroundColor Green
$sqlScript = Get-Content $sqlScriptPath -Raw

if ($useWindowsAuth -eq "Y" -or $useWindowsAuth -eq "y") {
    Write-Host "Connecting with Windows Authentication..." -ForegroundColor Green
    $connectionString = "Server=$server;Database=$database;Integrated Security=True;TrustServerCertificate=True;"
} else {
    $username = Read-Host "Username"
    $password = Read-Host "Password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    $connectionString = "Server=$server;Database=$database;User Id=$username;Password=$plainPassword;TrustServerCertificate=True;"
}

Write-Host ""
Write-Host "Executing migration script..." -ForegroundColor Green
Write-Host "This will create 4 tables:" -ForegroundColor Yellow
Write-Host "  - BankStatements" -ForegroundColor Yellow
Write-Host "  - BankStatementItems" -ForegroundColor Yellow
Write-Host "  - Reconciliations" -ForegroundColor Yellow
Write-Host "  - ReconciliationMatches" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Continue? (Y/N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Migration cancelled." -ForegroundColor Yellow
    exit 0
}

try {
    # Try using sqlcmd if available
    $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($sqlcmdPath) {
        Write-Host "Using sqlcmd..." -ForegroundColor Green
        if ($useWindowsAuth -eq "Y" -or $useWindowsAuth -eq "y") {
            sqlcmd -S $server -d $database -E -i $sqlScriptPath -b
        } else {
            sqlcmd -S $server -d $database -U $username -P $plainPassword -i $sqlScriptPath -b
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "=========================================" -ForegroundColor Green
            Write-Host "Migration completed successfully!" -ForegroundColor Green
            Write-Host "=========================================" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "ERROR: Migration failed. Check the error messages above." -ForegroundColor Red
            exit 1
        }
    } else {
        # Fallback: Use .NET SQL Client
        Write-Host "sqlcmd not found. Using .NET SQL Client..." -ForegroundColor Yellow
        Write-Host "Please run the SQL script manually using SQL Server Management Studio or Azure Data Studio." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Script location: $sqlScriptPath" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Or install sqlcmd:" -ForegroundColor Yellow
        Write-Host "  Download: https://learn.microsoft.com/en-us/sql/tools/sqlcmd-utility" -ForegroundColor Cyan
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run the SQL script manually:" -ForegroundColor Yellow
    Write-Host "  Location: $sqlScriptPath" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Green
Write-Host "1. Restart your application" -ForegroundColor White
Write-Host "2. Test the endpoint: GET /api/reconciliation/account/{bankAccountId}" -ForegroundColor White

