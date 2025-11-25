# Run Notification Migration Script
# This script adds missing columns to the Notifications table

Write-Host "=== RUNNING NOTIFICATION MIGRATION ===" -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Cyan

# Database connection parameters
$server = "174.138.185.18"
$database = "DBUTILS"
$username = "sa01"
$password = "iSTc0#T3tw~noz2r"

# SQL script path (relative to Scripts folder)
$scriptPath = "..\add_notification_columns.sql"

# Check if SQL script exists
if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: SQL script not found at: $scriptPath" -ForegroundColor Red
    Write-Host "Please ensure the script exists in the UtilityHub360 folder" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ SQL script found: $scriptPath" -ForegroundColor Green

# Check if sqlcmd is available
$sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
if (-not $sqlcmdPath) {
    Write-Host "ERROR: sqlcmd not found. Please install SQL Server Command Line Utilities." -ForegroundColor Red
    Write-Host "Alternatively, you can run the SQL script manually in SQL Server Management Studio" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ sqlcmd found" -ForegroundColor Green
Write-Host ""
Write-Host "Connecting to database: $database on server: $server" -ForegroundColor Cyan
Write-Host ""

# Run the SQL script
try {
    $sqlcmdArgs = @(
        "-S", $server
        "-d", $database
        "-U", $username
        "-P", $password
        "-i", $scriptPath
        "-b"  # Stop on error
    )
    
    & sqlcmd @sqlcmdArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host "Migration completed successfully!" -ForegroundColor Green
        Write-Host "==========================================" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "ERROR: Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to run migration script" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Run the SQL script manually in SQL Server Management Studio" -ForegroundColor Yellow
    Write-Host "Script location: $scriptPath" -ForegroundColor Yellow
    exit 1
}

