# PowerShell Script to Delete 2026-2031 Auto-Generated Bills
# Run this from the UtilityHub360 directory

param(
    [string]$Email = "your-email@example.com",
    [string]$Password = "your-password",
    [string]$BaseUrl = "https://localhost:7299"
)

Write-Host "🚨 EMERGENCY DELETE: Removing 2026-2031 Auto-Generated Bills" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Yellow

# Step 1: Login and get token
Write-Host "Step 1: Logging in..." -ForegroundColor Cyan
$loginBody = @{
    email = $Email
    password = $Password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -SkipCertificateCheck
    $token = $loginResponse.data.token
    Write-Host "✅ Login successful!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Login failed: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Check current count
Write-Host "`nStep 2: Checking current bill count..." -ForegroundColor Cyan
$headers = @{ Authorization = "Bearer $token" }

try {
    $countResponse = Invoke-RestMethod -Uri "$BaseUrl/api/bills/cleanup/date-range/count?startDate=2026-01-01&endDate=2031-12-31" -Method GET -Headers $headers -SkipCertificateCheck
    Write-Host "📊 Found $($countResponse.data.count) auto-generated bills in 2026-2031 range" -ForegroundColor Yellow
    
    if ($countResponse.data.count -eq 0) {
        Write-Host "✅ No bills to delete. You're all set!" -ForegroundColor Green
        exit 0
    }
}
catch {
    Write-Host "⚠️ Could not check count: $_" -ForegroundColor Yellow
}

# Step 3: Emergency delete
Write-Host "`nStep 3: Performing emergency deletion..." -ForegroundColor Cyan
Write-Host "⚠️ THIS WILL PERMANENTLY DELETE THE BILLS!" -ForegroundColor Red

$confirm = Read-Host "Type 'YES' to confirm deletion"
if ($confirm -ne "YES") {
    Write-Host "❌ Deletion cancelled by user" -ForegroundColor Yellow
    exit 0
}

try {
    $deleteResponse = Invoke-RestMethod -Uri "$BaseUrl/api/bills/emergency-delete-2026-2031" -Method POST -Headers $headers -SkipCertificateCheck
    Write-Host "✅ $($deleteResponse.data)" -ForegroundColor Green
}
catch {
    Write-Host "❌ Deletion failed: $_" -ForegroundColor Red
    exit 1
}

# Step 4: Verify deletion
Write-Host "`nStep 4: Verifying deletion..." -ForegroundColor Cyan
try {
    $verifyResponse = Invoke-RestMethod -Uri "$BaseUrl/api/bills/cleanup/date-range/count?startDate=2026-01-01&endDate=2031-12-31" -Method GET -Headers $headers -SkipCertificateCheck
    if ($verifyResponse.data.count -eq 0) {
        Write-Host "✅ SUCCESS: All 2026-2031 auto-generated bills have been deleted!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ WARNING: $($verifyResponse.data.count) bills still remain" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️ Could not verify deletion: $_" -ForegroundColor Yellow
}

Write-Host "`n🎉 EMERGENCY DELETE COMPLETED!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Yellow
