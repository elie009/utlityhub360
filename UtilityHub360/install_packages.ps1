# PowerShell script to install NuGet packages
Write-Host "Installing Entity Framework and Swashbuckle packages..." -ForegroundColor Green

# Create packages directory if it doesn't exist
$packagesDir = "..\packages"
if (!(Test-Path $packagesDir)) {
    New-Item -ItemType Directory -Path $packagesDir -Force
}

# Download Entity Framework 6.4.4
Write-Host "Downloading Entity Framework 6.4.4..." -ForegroundColor Yellow
$efUrl = "https://www.nuget.org/api/v2/package/EntityFramework/6.4.4"
$efPath = "$packagesDir\EntityFramework.6.4.4"
if (!(Test-Path $efPath)) {
    New-Item -ItemType Directory -Path $efPath -Force
    Invoke-WebRequest -Uri $efUrl -OutFile "$efPath\EntityFramework.6.4.4.nupkg"
    Expand-Archive -Path "$efPath\EntityFramework.6.4.4.nupkg" -DestinationPath $efPath -Force
}

# Download Swashbuckle.Core 5.6.0
Write-Host "Downloading Swashbuckle.Core 5.6.0..." -ForegroundColor Yellow
$swaggerUrl = "https://www.nuget.org/api/v2/package/Swashbuckle.Core/5.6.0"
$swaggerPath = "$packagesDir\Swashbuckle.Core.5.6.0"
if (!(Test-Path $swaggerPath)) {
    New-Item -ItemType Directory -Path $swaggerPath -Force
    Invoke-WebRequest -Uri $swaggerUrl -OutFile "$swaggerPath\Swashbuckle.Core.5.6.0.nupkg"
    Expand-Archive -Path "$swaggerPath\Swashbuckle.Core.5.6.0.nupkg" -DestinationPath $swaggerPath -Force
}

Write-Host "Package installation completed!" -ForegroundColor Green
Write-Host "Please rebuild your project in Visual Studio." -ForegroundColor Cyan
