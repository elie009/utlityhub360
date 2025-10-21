# Test CORS configuration for UtilityHub360
# This script tests the CORS headers from the production API

Write-Host "Testing CORS configuration..." -ForegroundColor Green

# Test URLs
$apiUrl = "https://api.utilityhub360.com"
$testEndpoints = @(
    "/test-cors",
    "/health",
    "/api/Loans"
)

# Test origins
$testOrigins = @(
    "https://www.utilityhub360.com",
    "https://utilityhub360.com",
    "https://api.utilityhub360.com"
)

foreach ($origin in $testOrigins) {
    Write-Host "`nTesting with origin: $origin" -ForegroundColor Yellow
    
    foreach ($endpoint in $testEndpoints) {
        $url = "$apiUrl$endpoint"
        Write-Host "  Testing: $url" -ForegroundColor Cyan
        
        try {
            # Test OPTIONS request (preflight)
            $optionsHeaders = @{
                'Origin' = $origin
                'Access-Control-Request-Method' = 'GET'
                'Access-Control-Request-Headers' = 'Authorization, Content-Type'
            }
            
            $response = Invoke-WebRequest -Uri $url -Method OPTIONS -Headers $optionsHeaders -UseBasicParsing
            
            Write-Host "    OPTIONS Status: $($response.StatusCode)" -ForegroundColor Green
            
            # Check CORS headers
            $corsHeaders = @{
                'Access-Control-Allow-Origin' = $response.Headers['Access-Control-Allow-Origin']
                'Access-Control-Allow-Methods' = $response.Headers['Access-Control-Allow-Methods']
                'Access-Control-Allow-Headers' = $response.Headers['Access-Control-Allow-Headers']
                'Access-Control-Allow-Credentials' = $response.Headers['Access-Control-Allow-Credentials']
            }
            
            Write-Host "    CORS Headers:" -ForegroundColor White
            foreach ($header in $corsHeaders.GetEnumerator()) {
                if ($header.Value) {
                    Write-Host "      $($header.Key): $($header.Value)" -ForegroundColor Green
                } else {
                    Write-Host "      $($header.Key): MISSING" -ForegroundColor Red
                }
            }
            
        } catch {
            Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`nCORS test completed!" -ForegroundColor Green
Write-Host "If you see 'MISSING' for any CORS headers, the configuration needs to be fixed." -ForegroundColor Yellow
