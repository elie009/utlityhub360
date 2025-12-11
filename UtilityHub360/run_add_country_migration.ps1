# PowerShell script to add Country column to Users table
# This script uses the connection string from appsettings.json
# This fixes the "Invalid column name 'Country'" error

Write-Host "Adding Country column to Users table..." -ForegroundColor Yellow

# Read connection string from appsettings.json
$appsettingsPath = "appsettings.json"
if (-not (Test-Path $appsettingsPath)) {
    Write-Host "ERROR: appsettings.json not found!" -ForegroundColor Red
    exit 1
}

$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.DefaultConnection

Write-Host "Connection String: $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray

# Get the SQL script file path
$scriptPath = Join-Path $PSScriptRoot "Scripts\add_country_column_to_users.sql"
if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: SQL script not found at: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "`nExecuting SQL script from: $scriptPath" -ForegroundColor Yellow

try {
    # Extract connection details from connection string
    $server = ($connectionString -split 'Server=')[1] -split ';' | Select-Object -First 1
    $database = ($connectionString -split 'Database=')[1] -split ';' | Select-Object -First 1
    $userId = ($connectionString -split 'User Id=')[1] -split ';' | Select-Object -First 1
    $password = ($connectionString -split 'Password=')[1] -split ';' | Select-Object -First 1
    
    Write-Host "Server: $server" -ForegroundColor Gray
    Write-Host "Database: $database" -ForegroundColor Gray
    Write-Host "User: $userId" -ForegroundColor Gray
    Write-Host ""
    
    $useDotNet = $false
    
    # Try using sqlcmd first
    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        Write-Host "Using sqlcmd to execute script..." -ForegroundColor Yellow
        $output = sqlcmd -S $server -d $database -U $userId -P $password -i $scriptPath -W -b 2>&1
        
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-Host "`n✅ SUCCESS: Country column has been added!" -ForegroundColor Green
            if ($output) {
                Write-Host $output
            }
        } else {
            Write-Host "`n⚠️ sqlcmd failed, trying .NET SqlClient method..." -ForegroundColor Yellow
            $useDotNet = $true
        }
    } else {
        Write-Host "sqlcmd not found, using .NET SqlClient method..." -ForegroundColor Yellow
        $useDotNet = $true
    }
    
    # Fall back to .NET SqlClient if sqlcmd failed or is not available
    if ($useDotNet) {
        Write-Host "Using .NET SqlClient to execute script..." -ForegroundColor Yellow
        
        Add-Type -AssemblyName System.Data
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        try {
            $sqlScript = Get-Content $scriptPath -Raw
            
            # Remove GO statements as they're not needed for .NET SqlClient
            $sqlScript = $sqlScript -replace '(?m)^GO\s*$', ''
            
            $command = $connection.CreateCommand()
            $command.CommandText = $sqlScript
            $command.CommandTimeout = 30
            
            $result = $command.ExecuteNonQuery()
            
            Write-Host "`n✅ SUCCESS: Country column has been added!" -ForegroundColor Green
        } finally {
            $connection.Close()
        }
    }
} catch {
    Write-Host "`n❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nPlease run the SQL script manually:" -ForegroundColor Yellow
    Write-Host "1. Open SQL Server Management Studio or Azure Data Studio" -ForegroundColor Cyan
    Write-Host "2. Connect to your database" -ForegroundColor Cyan
    Write-Host "3. Open the file: $scriptPath" -ForegroundColor Cyan
    Write-Host "4. Execute the script" -ForegroundColor Cyan
    exit 1
}

Write-Host "`nScript completed." -ForegroundColor Yellow

