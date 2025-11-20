# PowerShell script to add PreferredCurrency column to UserProfiles table
# This script uses the connection string from appsettings.json

Write-Host "Adding PreferredCurrency column to UserProfiles table..." -ForegroundColor Yellow

# Read connection string from appsettings.json
$appsettingsPath = "appsettings.json"
$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.DefaultConnection

Write-Host "Connection String: $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray

# SQL Script
$sqlScript = @"
-- Add PreferredCurrency column to UserProfiles table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserProfiles' 
    AND COLUMN_NAME = 'PreferredCurrency'
)
BEGIN
    -- Add the column with a default value of 'USD'
    ALTER TABLE [UserProfiles]
    ADD [PreferredCurrency] NVARCHAR(10) NOT NULL DEFAULT 'USD';
    
    PRINT 'PreferredCurrency column added successfully to UserProfiles table.';
END
ELSE
BEGIN
    PRINT 'PreferredCurrency column already exists in UserProfiles table.';
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
        Write-Host "`n✅ SUCCESS: PreferredCurrency column has been added!" -ForegroundColor Green
        Write-Host $result
    } else {
        Write-Host "`n❌ ERROR: Failed to execute SQL script" -ForegroundColor Red
        Write-Host $result
        Write-Host "`nPlease run the SQL script manually using SQL Server Management Studio or Azure Data Studio" -ForegroundColor Yellow
    }
} catch {
    Write-Host "`n❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nPlease run the SQL script manually:" -ForegroundColor Yellow
    Write-Host "1. Open SQL Server Management Studio or Azure Data Studio" -ForegroundColor Cyan
    Write-Host "2. Connect to your database" -ForegroundColor Cyan
    Write-Host "3. Open the file: add_preferred_currency_column.sql" -ForegroundColor Cyan
    Write-Host "4. Execute the script" -ForegroundColor Cyan
}

# Clean up temp file
Remove-Item $tempSqlFile -ErrorAction SilentlyContinue

Write-Host "`nScript completed." -ForegroundColor Yellow


