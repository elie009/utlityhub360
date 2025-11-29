# Deploy JWT Fix Script
# This script deploys the updated application with consistent JWT settings

Write-Host "=== Deploying JWT Fix to Production ===" -ForegroundColor Green

# Check if publish folder exists
if (Test-Path "publish") {
    Write-Host "✅ Publish folder found" -ForegroundColor Green
    
    # List the files to be deployed
    Write-Host "`n📁 Files to be deployed:" -ForegroundColor Yellow
    Get-ChildItem "publish" | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
    
    Write-Host "`n🚀 Deployment Instructions:" -ForegroundColor Cyan
    Write-Host "1. Copy all files from the 'publish' folder to your production server"
    Write-Host "2. Replace the existing application files"
    Write-Host "3. Restart your application in Plesk"
    Write-Host "4. Test the API endpoints"
    
    Write-Host "`n🔧 Key Changes in this deployment:" -ForegroundColor Yellow
    Write-Host "- JWT settings now match between development and production"
    Write-Host "- CORS configuration is updated"
    Write-Host "- Debug endpoints are available"
    Write-Host "- Authentication should work consistently"
    
    Write-Host "`n📋 Next Steps:" -ForegroundColor Magenta
    Write-Host "1. Deploy the files to your production server"
    Write-Host "2. Restart the application in Plesk"
    Write-Host "3. Test: GET https://api.utilityhub360.com/"
    Write-Host "4. Test: GET https://api.utilityhub360.com/swagger"
    Write-Host "5. Get a fresh JWT token from your frontend login"
    Write-Host "6. Test API endpoints with the new token"
    
} else {
    Write-Host "❌ Publish folder not found. Please run 'dotnet publish' first." -ForegroundColor Red
}

Write-Host "`n=== Deployment Script Complete ===" -ForegroundColor Green
