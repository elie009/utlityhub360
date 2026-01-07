# PowerShell script to add DateOfInvestment column to Investments table
# This script uses the connection string from appsettings.json

Write-Host "Adding DateOfInvestment column to Investments table..." -ForegroundColor Yellow

# Read connection string from appsettings.json
$appsettingsPath = "appsettings.json"
if (-not (Test-Path $appsettingsPath)) {
    Write-Host "ERROR: appsettings.json not found in current directory." -ForegroundColor Red
    Write-Host "Please run this script from the UtilityHub360 project directory." -ForegroundColor Yellow
    exit 1
}

$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.DefaultConnection

if (-not $connectionString) {
    Write-Host "ERROR: Connection string not found in appsettings.json" -ForegroundColor Red
    exit 1
}

Write-Host "Connection String: $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray

# SQL Script
$sqlScript = @"
-- Add DateOfInvestment column to Investments table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Investments' 
    AND COLUMN_NAME = 'DateOfInvestment'
)
BEGIN
    -- Add the column as nullable DateTime
    ALTER TABLE [Investments]
    ADD [DateOfInvestment] DATETIME2 NULL;
    
    PRINT 'DateOfInvestment column added successfully to Investments table.';
END
ELSE
BEGIN
    PRINT 'DateOfInvestment column already exists in Investments table.';
END
"@

# Save SQL to temp file
$tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
$sqlScript | Out-File -FilePath $tempSqlFile -Encoding UTF8

Write-Host "`nExecuting SQL script..." -ForegroundColor Yellow

try {
    # Use sqlcmd to execute the script
    $server = ($connectionString -split 'Server=')[1] -split ';' | Select-Object -First 1
    $database = ($connectionString -split 'Database=')[1] -split ';' | Select-Object -First 1
    $userId = ($connectionString -split 'User Id=')[1] -split ';' | Select-Object -First 1
    $password = ($connectionString -split 'Password=')[1] -split ';' | Select-Object -First 1
    
    Write-Host "Server: $server" -ForegroundColor Gray
    Write-Host "Database: $database" -ForegroundColor Gray
    Write-Host "User: $userId" -ForegroundColor Gray
    
    # Execute using sqlcmd
    $result = sqlcmd -S $server -d $database -U $userId -P $password -i $tempSqlFile -W
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ SUCCESS: DateOfInvestment column has been added!" -ForegroundColor Green
        Write-Host $result
    } else {
        Write-Host "`n❌ ERROR: Failed to execute SQL script" -ForegroundColor Red
        Write-Host $result
        Write-Host "`nPlease run the SQL script manually using SQL Server Management Studio or Azure Data Studio" -ForegroundColor Yellow
        Write-Host "SQL script location: $tempSqlFile" -ForegroundColor Cyan
    }
} catch {
    Write-Host "`n❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nPlease run the SQL script manually:" -ForegroundColor Yellow
    Write-Host "1. Open SQL Server Management Studio or Azure Data Studio" -ForegroundColor Cyan
    Write-Host "2. Connect to your database" -ForegroundColor Cyan
    Write-Host "3. Open the file: Documentation/Database/add_date_of_investment_to_investments.sql" -ForegroundColor Cyan
    Write-Host "4. Execute the script" -ForegroundColor Cyan
}

# Clean up temp file
Remove-Item $tempSqlFile -ErrorAction SilentlyContinue

Write-Host "`nScript completed." -ForegroundColor Yellow





