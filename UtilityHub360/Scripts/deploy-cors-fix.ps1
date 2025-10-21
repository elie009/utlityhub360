# Deploy CORS Fix to Production
# This script helps deploy the latest CORS fixes to resolve the 403 Forbidden error

Write-Host "=== DEPLOYING CORS FIX TO PRODUCTION ===" -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Cyan

# Check if publish directory exists
if (-not (Test-Path ".\publish")) {
    Write-Host "ERROR: Publish directory not found. Please run 'dotnet publish' first." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Publish directory found" -ForegroundColor Green

Write-Host ""
Write-Host "=== CORS FIXES INCLUDED ===" -ForegroundColor Yellow
Write-Host "1. ✅ Fixed CORS Policy - Allow specific origins with credentials" -ForegroundColor Green
Write-Host "2. ✅ Updated All Controllers - All controllers use correct CORS policy" -ForegroundColor Green
Write-Host "3. ✅ Removed Conflicting Middleware - Clean CORS implementation" -ForegroundColor Green
Write-Host "4. ✅ Added Debug Logging - Better error tracking" -ForegroundColor Green

Write-Host ""
Write-Host "=== PRODUCTION DEPLOYMENT STEPS ===" -ForegroundColor Yellow
Write-Host "1. STOP your current application on the production server" -ForegroundColor Red
Write-Host "2. BACKUP your current application files" -ForegroundColor Yellow
Write-Host "3. COPY the contents of the 'publish' folder to your server" -ForegroundColor White
Write-Host "4. RESTART your application" -ForegroundColor White

Write-Host ""
Write-Host "=== CRITICAL: BEFORE DEPLOYMENT ===" -ForegroundColor Red
Write-Host "Make sure your production appsettings.Production.json contains:" -ForegroundColor Yellow
Write-Host "- Correct database connection string" -ForegroundColor White
Write-Host "- Correct JWT settings" -ForegroundColor White
Write-Host "- Correct SMTP settings" -ForegroundColor White

Write-Host ""
Write-Host "=== TESTING AFTER DEPLOYMENT ===" -ForegroundColor Yellow
Write-Host "1. Test CORS: GET https://api.utilityhub360.com/test-cors" -ForegroundColor Cyan
Write-Host "2. Test DELETE: DELETE https://api.utilityhub360.com/api/Loans/{loanId}" -ForegroundColor Cyan
Write-Host "3. Check logs for any errors" -ForegroundColor Cyan

Write-Host ""
Write-Host "=== EXPECTED RESULT ===" -ForegroundColor Green
Write-Host "After deployment, your DELETE request should work without CORS errors!" -ForegroundColor White
Write-Host "The 403 Forbidden error should be resolved." -ForegroundColor White

Write-Host ""
Write-Host "=== FILES TO DEPLOY ===" -ForegroundColor Yellow
$files = Get-ChildItem ".\publish" -Name | Select-Object -First 10
foreach ($file in $files) {
    Write-Host "  - $file" -ForegroundColor White
}
if ((Get-ChildItem ".\publish").Count -gt 10) {
    Write-Host "  ... and $(((Get-ChildItem ".\publish").Count - 10)) more files" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== READY FOR DEPLOYMENT ===" -ForegroundColor Green
Write-Host "Copy the publish folder contents to your production server now!" -ForegroundColor White
