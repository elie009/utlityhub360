# PowerShell script to switch between development and production environments
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("dev", "development", "prod", "production")]
    [string]$Environment
)

Write-Host "🔄 Switching to $Environment environment..." -ForegroundColor Cyan

switch ($Environment.ToLower()) {
    "dev" { 
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        Write-Host "✅ Switched to Development environment" -ForegroundColor Green
        Write-Host "📍 Database: localhost\SQLEXPRESS" -ForegroundColor Yellow
    }
    "development" { 
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        Write-Host "✅ Switched to Development environment" -ForegroundColor Green
        Write-Host "📍 Database: localhost\SQLEXPRESS" -ForegroundColor Yellow
    }
    "prod" { 
        $env:ASPNETCORE_ENVIRONMENT = "Production"
        Write-Host "✅ Switched to Production environment" -ForegroundColor Green
        Write-Host "📍 Database: 174.138.185.18" -ForegroundColor Yellow
    }
    "production" { 
        $env:ASPNETCORE_ENVIRONMENT = "Production"
        Write-Host "✅ Switched to Production environment" -ForegroundColor Green
        Write-Host "📍 Database: 174.138.185.18" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Current Environment: $env:ASPNETCORE_ENVIRONMENT" -ForegroundColor Magenta
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Cyan
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "To run database migrations:" -ForegroundColor Cyan
Write-Host "  dotnet ef database update" -ForegroundColor White
