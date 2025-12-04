# Simple script to run subscription tables migration
# This uses the connection string from appsettings.Production.json

$scriptPath = Join-Path $PSScriptRoot "Documentation\Database\Scripts\create_subscription_tables.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: SQL script not found at: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Subscription Tables Migration" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Production database connection (from appsettings.Production.json)
$server = "174.138.185.18"
$database = "DBUTILS"
$username = "sa01"
$password = "iSTc0#T3tw~noz2r"

Write-Host "Connecting to: $server" -ForegroundColor Yellow
Write-Host "Database: $database" -ForegroundColor Yellow
Write-Host ""

# Check if sqlcmd is available
$sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue

if ($sqlcmdPath) {
    Write-Host "Executing migration script..." -ForegroundColor Green
    Write-Host ""
    
    $result = sqlcmd -S $server -d $database -U $username -P $password -i $scriptPath -b
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host "✓ Migration completed successfully!" -ForegroundColor Green
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Subscription tables created:" -ForegroundColor Cyan
        Write-Host "  - SubscriptionPlans" -ForegroundColor White
        Write-Host "  - UserSubscriptions" -ForegroundColor White
        Write-Host ""
        Write-Host "Default plans seeded:" -ForegroundColor Cyan
        Write-Host "  - Starter (Free)" -ForegroundColor White
        Write-Host "  - Professional (Premium)" -ForegroundColor White
        Write-Host "  - Enterprise (Premium Plus)" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host "Migration failed. Exit code: $LASTEXITCODE" -ForegroundColor Red
        Write-Host "Output: $result" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "sqlcmd not found. Please install SQL Server Command Line Utilities" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternative: Use SQL Server Management Studio (SSMS):" -ForegroundColor Yellow
    Write-Host "  1. Open SSMS" -ForegroundColor Cyan
    Write-Host "  2. Connect to: $server" -ForegroundColor Cyan
    Write-Host "  3. Open and execute: $scriptPath" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "You can now restart your backend application." -ForegroundColor Green

