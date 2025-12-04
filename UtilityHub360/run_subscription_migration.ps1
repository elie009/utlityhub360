# PowerShell script to run subscription tables migration
# This script executes the SQL migration to create subscription tables

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Subscription Tables Migration Script" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Get database connection details
$server = Read-Host "Enter SQL Server name (e.g., localhost\SQLEXPRESS or localhost)"
$database = Read-Host "Enter database name (e.g., UtilityHub360 or DBUTILS)"
$useWindowsAuth = Read-Host "Use Windows Authentication? (Y/N)"

if ($useWindowsAuth -eq "Y" -or $useWindowsAuth -eq "y") {
    $connectionString = "Server=$server;Database=$database;Trusted_Connection=true;TrustServerCertificate=true;"
} else {
    $username = Read-Host "Enter SQL Server username"
    $password = Read-Host "Enter SQL Server password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    $connectionString = "Server=$server;Database=$database;User Id=$username;Password=$plainPassword;TrustServerCertificate=true;"
}

$scriptPath = Join-Path $PSScriptRoot "Documentation\Database\Scripts\create_subscription_tables.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: SQL script not found at: $scriptPath" -ForegroundColor Red
    Write-Host "Please ensure you're running this from the UtilityHub360 project directory" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Running migration script..." -ForegroundColor Yellow
Write-Host "Script path: $scriptPath" -ForegroundColor Gray
Write-Host ""

try {
    # Check if sqlcmd is available
    $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
    
    if ($sqlcmdPath) {
        Write-Host "Using sqlcmd..." -ForegroundColor Green
        
        if ($useWindowsAuth -eq "Y" -or $useWindowsAuth -eq "y") {
            sqlcmd -S $server -d $database -i $scriptPath -E
        } else {
            sqlcmd -S $server -d $database -i $scriptPath -U $username -P $plainPassword
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "==========================================" -ForegroundColor Green
            Write-Host "Migration completed successfully!" -ForegroundColor Green
            Write-Host "==========================================" -ForegroundColor Green
            Write-Host ""
            Write-Host "Subscription tables have been created:" -ForegroundColor Cyan
            Write-Host "  - SubscriptionPlans" -ForegroundColor White
            Write-Host "  - UserSubscriptions" -ForegroundColor White
            Write-Host ""
            Write-Host "Default subscription plans have been seeded:" -ForegroundColor Cyan
            Write-Host "  - Starter (Free)" -ForegroundColor White
            Write-Host "  - Professional (Premium)" -ForegroundColor White
            Write-Host "  - Enterprise (Premium Plus)" -ForegroundColor White
        } else {
            Write-Host ""
            Write-Host "Migration failed. Please check the error messages above." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "sqlcmd not found. Please install SQL Server Command Line Utilities" -ForegroundColor Yellow
        Write-Host "Or use SQL Server Management Studio (SSMS) to run the script manually:" -ForegroundColor Yellow
        Write-Host "  $scriptPath" -ForegroundColor Cyan
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "Error running migration: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Use SQL Server Management Studio (SSMS) to run:" -ForegroundColor Yellow
    Write-Host "  $scriptPath" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "You can now restart your backend application." -ForegroundColor Green

