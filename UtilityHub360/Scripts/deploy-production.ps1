# Production Deployment Script for UtilityHub360
Write-Host "Starting Production Deployment..." -ForegroundColor Green

# Set environment to Production
$env:ASPNETCORE_ENVIRONMENT = "Production"
Write-Host "Environment set to: $env:ASPNETCORE_ENVIRONMENT" -ForegroundColor Yellow

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean

# Build in Release mode
Write-Host "Building in Release mode..." -ForegroundColor Yellow
dotnet build --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Green
    
    # Publish for production
    Write-Host "Publishing for production..." -ForegroundColor Yellow
    dotnet publish --configuration Release --output ./publish
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Production build completed successfully!" -ForegroundColor Green
        Write-Host "Files are ready in ./publish folder" -ForegroundColor Green
        Write-Host "Upload these files to your production server at: https://wh1479740.ispot.cc" -ForegroundColor Cyan
        Write-Host "Make sure to set ASPNETCORE_ENVIRONMENT=Production on your server" -ForegroundColor Cyan
        
        # Show publish folder contents
        Write-Host ""
        Write-Host "Published files:" -ForegroundColor Yellow
        Get-ChildItem ./publish | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
    } else {
        Write-Host "Publish failed!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Upload ./publish folder contents to your server" -ForegroundColor White
Write-Host "2. Set ASPNETCORE_ENVIRONMENT=Production on your server" -ForegroundColor White
Write-Host "3. Configure IIS to host your application" -ForegroundColor White
Write-Host "4. Test: https://wh1479740.ispot.cc/swagger" -ForegroundColor White