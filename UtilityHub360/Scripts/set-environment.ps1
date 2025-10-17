# ================================================
# ENVIRONMENT SWITCHER FOR UTILITYHUB360
# ================================================
# Change this value to switch environments:
$Environment = "Development"  # Options: Development, Staging, Production
# ================================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Environment Switcher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Switching to: $Environment" -ForegroundColor Yellow
Write-Host ""

# Update launchSettings.json
$launchSettingsPath = ".\Properties\launchSettings.json"
if (Test-Path $launchSettingsPath) {
    $launchSettings = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json

    # Update all profiles
    $launchSettings.profiles.PSObject.Properties | ForEach-Object {
        if ($_.Value.environmentVariables) {
            $_.Value.environmentVariables.ASPNETCORE_ENVIRONMENT = $Environment
        }
    }

    # Save back
    $launchSettings | ConvertTo-Json -Depth 10 | Set-Content $launchSettingsPath

    Write-Host "[OK] Updated launchSettings.json" -ForegroundColor Green
} else {
    Write-Host "[!!] launchSettings.json not found" -ForegroundColor Red
}

# Update web.config
$webConfigPath = ".\web.config"
if (Test-Path $webConfigPath) {
    [xml]$webConfig = Get-Content $webConfigPath
    $envVar = $webConfig.configuration.location.'system.webServer'.aspNetCore.environmentVariables.environmentVariable | Where-Object { $_.name -eq "ASPNETCORE_ENVIRONMENT" }
    if ($envVar) {
        $envVar.value = $Environment
        $webConfig.Save((Resolve-Path $webConfigPath))
        Write-Host "[OK] Updated web.config" -ForegroundColor Green
    }
} else {
    Write-Host "[!!] web.config not found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Environment: $Environment" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Show database connection info
Write-Host "Database Connection:" -ForegroundColor White
switch ($Environment) {
    "Development" { 
        Write-Host "  Server:   localhost\SQLEXPRESS" -ForegroundColor Green
        Write-Host "  Database: DBUTILS" -ForegroundColor Green
        Write-Host "  Type:     Local Development DB" -ForegroundColor Green
    }
    "Staging" { 
        Write-Host "  Server:   174.138.185.18" -ForegroundColor Yellow
        Write-Host "  Database: DBUTILS" -ForegroundColor Yellow
        Write-Host "  Type:     Remote Test/Staging DB" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "  WARNING: Currently points to LIVE database!" -ForegroundColor Red
        Write-Host "  Consider creating a separate test database." -ForegroundColor Red
    }
    "Production" { 
        Write-Host "  Server:   174.138.185.18" -ForegroundColor Red
        Write-Host "  Database: DBUTILS" -ForegroundColor Red
        Write-Host "  Type:     LIVE PRODUCTION DB" -ForegroundColor Red
        Write-Host ""
        Write-Host "  CAUTION: You are using LIVE PRODUCTION data!" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ready! Press F5 in Visual Studio" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
