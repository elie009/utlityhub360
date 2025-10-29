# 🚨 Chat API Troubleshooting Guide

## Issue: POST /api/chat/message Returns "Canceled" Status

### ⚠️ Common Causes

1. **OpenAI API Timeout** - Request takes >30 seconds
2. **Invalid/Expired OpenAI API Key**
3. **Network Issues**
4. **Documentation Index Building** (first request only)
5. **Large Financial Context Data**

---

## ✅ Quick Fixes

### Fix 1: Verify OpenAI API Key

**Check if your API key is valid:**

1. Go to https://platform.openai.com/api-keys
2. Verify your key is active and has credits
3. If expired, generate new key
4. Update in `appsettings.json` or `appsettings.Development.json`

```json
{
  "OpenAISettings": {
    "ApiKey": "sk-proj-YOUR_NEW_KEY_HERE"
  }
}
```

---

### Fix 2: Increase Request Timeout

The default timeout might be too short. Add this to `Program.cs`:

```csharp
// After builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueCountLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

// Add this after var app = builder.Build();
app.UseRequestTimeouts(new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutOptions
{
    DefaultTimeout = TimeSpan.FromSeconds(120) // 2 minutes
});
```

---

### Fix 3: Test Without Financial Context First

Disable financial context to test if documentation search works:

```http
POST http://localhost:5000/api/chat/message
Authorization: Bearer YOUR_TOKEN

{
  "message": "Hello, are you working?",
  "includeTransactionContext": false
}
```

This should be faster since it skips financial data queries.

---

### Fix 4: Check OpenAI Model Availability

The model `gpt-4-turbo-preview` might not be available for your API key. Try changing to:

```json
{
  "OpenAISettings": {
    "Model": "gpt-3.5-turbo"  // Faster and always available
  }
}
```

---

### Fix 5: Test OpenAI Connection Manually

Create a simple test endpoint to verify OpenAI works:

```csharp
// Add to ChatController.cs
[HttpGet("test-openai")]
public async Task<IActionResult> TestOpenAI()
{
    try
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAISettings.ApiKey}");
        client.Timeout = TimeSpan.FromSeconds(10);
        
        var response = await client.GetAsync("https://api.openai.com/v1/models");
        var content = await response.Content.ReadAsStringAsync();
        
        return Ok(new { 
            status = response.StatusCode, 
            working = response.IsSuccessStatusCode,
            message = response.IsSuccessStatusCode ? "OpenAI API is working!" : "OpenAI API error",
            details = content.Substring(0, Math.Min(500, content.Length))
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

Test with: `GET http://localhost:5000/api/chat/test-openai`

---

### Fix 6: Test Documentation Search Only

Test if documentation search works independently:

```http
GET http://localhost:5000/api/chat/search-documentation?query=bill
Authorization: Bearer YOUR_TOKEN
```

This should return instantly (no OpenAI call).

---

### Fix 7: Use Simpler Test Message

```http
POST http://localhost:5000/api/chat/message
Authorization: Bearer YOUR_TOKEN

{
  "message": "Hi",
  "conversationId": null,
  "includeTransactionContext": false
}
```

Short message = faster response = less likely to timeout.

---

## 🔍 Debugging Steps

### Step 1: Check Application Logs

Look for errors in console output when request is made. You should see:
```
Indexing XXX documentation files...
Successfully indexed XXX documentation chunks
```

### Step 2: Check Network Tab in Browser

If using browser/Postman:
1. Open Developer Tools → Network tab
2. Send request
3. Check how long it takes
4. Look for timeout errors

### Step 3: Test Each Component Separately

**A. Test Auth (should work):**
```http
POST http://localhost:5000/api/auth/login
{
  "email": "test@example.com",
  "password": "Password123!"
}
```

**B. Test Documentation Search (should work):**
```http
GET http://localhost:5000/api/chat/search-documentation?query=test
Authorization: Bearer TOKEN
```

**C. Test Financial Context (should work):**
```http
GET http://localhost:5000/api/chat/financial-context
Authorization: Bearer TOKEN
```

**D. Test Full Chat (might timeout):**
```http
POST http://localhost:5000/api/chat/message
Authorization: Bearer TOKEN
{
  "message": "Hi",
  "includeTransactionContext": false
}
```

---

## 💡 Quick Temporary Solution

### Use Faster Model

Change `appsettings.json`:

```json
{
  "OpenAISettings": {
    "Model": "gpt-3.5-turbo",  // Much faster
    "MaxTokens": 500,           // Shorter responses
    "Temperature": 0.5
  }
}
```

---

## 🎯 Expected Behavior

### First Request (Cold Start)
- **Time:** 3-7 seconds
- **Reason:** Building documentation index

### Subsequent Requests
- **Without Context:** 1-3 seconds
- **With Context:** 2-5 seconds

If taking longer than 10 seconds, there's a problem.

---

## 🔧 Advanced Fixes

### Option 1: Add HttpClient Timeout Configuration

In `DocumentationSearchService.cs`:

```csharp
var client = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60) // 1 minute timeout
};
```

### Option 2: Add Retry Logic

Wrap OpenAI calls with retry:

```csharp
int maxRetries = 3;
for (int i = 0; i < maxRetries; i++)
{
    try
    {
        var response = await client.PostAsync(...);
        break; // Success
    }
    catch (TaskCanceledException)
    {
        if (i == maxRetries - 1) throw;
        await Task.Delay(1000); // Wait 1 sec before retry
    }
}
```

### Option 3: Disable Documentation Search Temporarily

Comment out in `ChatService.cs` line 558-563:

```csharp
// Temporarily disable for testing
// var documentationContext = await _documentationSearchService.GetDocumentationContextAsync(userMessage);
// if (!string.IsNullOrEmpty(documentationContext))
// {
//     messages.Add(new { role = "system", content = documentationContext });
// }
```

---

## 📝 Checklist

Before asking for help, verify:

- [ ] Application is running (`dotnet run`)
- [ ] OpenAI API key is valid
- [ ] OpenAI API key has credits
- [ ] You can access OpenAI from your network
- [ ] JWT token is valid (not expired)
- [ ] Documentation folder exists (`UtilityHub360/Documentation/`)
- [ ] `.md` files exist in Documentation folder
- [ ] Request timeout is at least 60 seconds
- [ ] Using correct URL (`http://localhost:5000`)
- [ ] Using correct HTTP method (`POST`)
- [ ] Content-Type header is set (`application/json`)

---

## 🆘 Still Not Working?

### Test with cURL

```bash
curl -X POST http://localhost:5000/api/chat/message \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"message":"Hi","includeTransactionContext":false}' \
  --max-time 60 \
  --verbose
```

Look for:
- Connection errors
- Timeout messages
- HTTP status codes
- Response time

---

## 📊 Performance Optimization

If requests are slow but working:

1. **Reduce MaxTokens:** `500` instead of `1000`
2. **Use gpt-3.5-turbo:** Faster than gpt-4
3. **Disable context:** `includeTransactionContext: false`
4. **Cache more:** Increase cache duration
5. **Reduce documentation:** Index fewer files

---

## ✅ Success Indicators

You'll know it's working when:
- Response time < 5 seconds
- No timeout errors
- AI responds with actual content
- Console shows no errors
- Documentation is cited in responses

---

**Last Updated:** October 28, 2025

