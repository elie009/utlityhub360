# Deploy CORS and DELETE endpoint fixes to production
Write-Host "=== DEPLOYING CORS AND DELETE ENDPOINT FIXES ===" -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Cyan

# Check if publish directory exists
if (-not (Test-Path ".\publish")) {
    Write-Host "ERROR: Publish directory not found. Please run 'dotnet publish' first." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Publish directory found" -ForegroundColor Green

Write-Host ""
Write-Host "=== FIXES INCLUDED IN THIS DEPLOYMENT ===" -ForegroundColor Yellow
Write-Host "1. ✅ CORS Configuration - Allow all origins with credentials" -ForegroundColor Green
Write-Host "2. ✅ DELETE Endpoint - Fixed loan deletion functionality" -ForegroundColor Green
Write-Host "3. ✅ Debug Endpoint - Added /api/Loans/{id}/debug for troubleshooting" -ForegroundColor Green
Write-Host "4. ✅ Route Conflicts - Resolved ambiguous route issues" -ForegroundColor Green
Write-Host "5. ✅ Null Reference Warnings - Fixed compiler warnings" -ForegroundColor Green

Write-Host ""
Write-Host "=== DEPLOYMENT INSTRUCTIONS ===" -ForegroundColor Yellow
Write-Host "1. Stop the current application on your server" -ForegroundColor White
Write-Host "2. Copy the contents of the 'publish' folder to your server" -ForegroundColor White
Write-Host "3. Make sure appsettings.Production.json is configured correctly" -ForegroundColor White
Write-Host "4. Start the application" -ForegroundColor White

Write-Host ""
Write-Host "=== TESTING AFTER DEPLOYMENT ===" -ForegroundColor Yellow
Write-Host "1. Test CORS: GET https://api.utilityhub360.com/test-cors" -ForegroundColor Cyan
Write-Host "2. Test DELETE: DELETE https://api.utilityhub360.com/api/Loans/{loanId}" -ForegroundColor Cyan
Write-Host "3. Debug loan: GET https://api.utilityhub360.com/api/Loans/{loanId}/debug" -ForegroundColor Cyan

Write-Host ""
Write-Host "=== READY FOR DEPLOYMENT ===" -ForegroundColor Green
Write-Host "The publish folder contains all necessary files for deployment." -ForegroundColor White

Write-Host ""
Write-Host "Deployment preparation completed" -ForegroundColor Green