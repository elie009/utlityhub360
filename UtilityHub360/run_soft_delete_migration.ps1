# PowerShell script to run soft delete migration
# This script will execute the SQL migration against your database

$sqlFile = "run_all_soft_delete_migrations.sql"
$server = "174.138.185.18"
$database = "DBUTILS"
$username = "sa01"
$password = "iSTc0#T3tw~noz2r"

Write-Host "Running soft delete migration..." -ForegroundColor Green
Write-Host "Server: $server" -ForegroundColor Yellow
Write-Host "Database: $database" -ForegroundColor Yellow

# Run the SQL script using sqlcmd
sqlcmd -S $server -d $database -U $username -P $password -i $sqlFile -b

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Migration failed. Please check the error messages above." -ForegroundColor Red
}

