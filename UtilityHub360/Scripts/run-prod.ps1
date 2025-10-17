# PowerShell script to run application in Production mode by default
Write-Host "🚀 Starting UtilityHub360 in Production mode..." -ForegroundColor Cyan

# Set production environment
$env:ASPNETCORE_ENVIRONMENT = "Production"

Write-Host "✅ Environment: Production" -ForegroundColor Green
Write-Host "📍 Database: 174.138.185.18/DBUTILS" -ForegroundColor Yellow
Write-Host "🌐 URLs: http://174.138.185.18:5000, https://174.138.185.18:5001" -ForegroundColor Yellow
Write-Host ""

# Run the application
Write-Host "Starting application..." -ForegroundColor Cyan
dotnet run
