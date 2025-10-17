# Test Authentication API
Write-Host "Testing Authentication API..." -ForegroundColor Green

# Test Registration
Write-Host "`n1. Testing Registration..." -ForegroundColor Yellow
$registerData = @{
    name = "Test User"
    email = "test@example.com"
    phone = "1234567890"
    password = "TestPassword123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "Registration SUCCESS: $($registerResponse.data.user.email)" -ForegroundColor Green
} catch {
    Write-Host "Registration FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Login with Correct Password
Write-Host "`n2. Testing Login with Correct Password..." -ForegroundColor Yellow
$loginData = @{
    email = "test@example.com"
    password = "TestPassword123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    Write-Host "Login SUCCESS: Token received" -ForegroundColor Green
} catch {
    Write-Host "Login FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Login with Wrong Password
Write-Host "`n3. Testing Login with Wrong Password..." -ForegroundColor Yellow
$wrongLoginData = @{
    email = "test@example.com"
    password = "WrongPassword123!"
} | ConvertTo-Json

try {
    $wrongLoginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $wrongLoginData -ContentType "application/json"
    Write-Host "Login with wrong password SUCCESS (THIS IS BAD!): Token received" -ForegroundColor Red
} catch {
    Write-Host "Login with wrong password FAILED (THIS IS GOOD!): $($_.Exception.Message)" -ForegroundColor Green
}

Write-Host "`nAuthentication test completed!" -ForegroundColor Cyan
