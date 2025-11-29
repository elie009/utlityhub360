# Chat AI Assistant API - Complete Documentation

## 📋 Overview

The Chat AI Assistant is an intelligent chatbot powered by OpenAI that helps users with:
1. **Application Questions** - Answers questions about UtilityHub360 features, APIs, and functionality by searching documentation
2. **Financial Advice** - Provides personalized financial guidance based on user's actual data (bills, loans, savings)

---

## 🔑 Authentication

All endpoints require JWT Bearer Token authentication.

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

Get your token from `/api/auth/login` endpoint.

---

## 📡 API Endpoints

### 1. Send Message to AI Assistant

Send a message and get an AI-powered response with optional financial context.

**Endpoint:** `POST /api/chat/message`

**Request Body:**
```json
{
  "message": "What bills do I have coming up?",
  "conversationId": null,
  "includeTransactionContext": true,
  "reportFormat": null
}
```

**Parameters:**
- `message` (string, required): Your question or message (1-4000 characters)
- `conversationId` (string, optional): Conversation ID for continuing chat, null for new
- `includeTransactionContext` (boolean, optional): Include financial data in context (default: true)
- `reportFormat` (string, optional): "pdf" or "excel" for report requests

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Message processed successfully",
  "data": {
    "message": "Based on your financial data and documentation...",
    "conversationId": "abc123-conversation-id",
    "suggestedActions": ["View upcoming bills", "Review budget"],
    "generatedReportPath": null,
    "reportFormat": null,
    "tokensUsed": 256,
    "timestamp": "2025-10-28T14:30:45.123Z",
    "isReportGenerated": false
  }
}
```

**Example Questions:**

**Application Questions:**
```http
POST /api/chat/message

{
  "message": "How do I create a recurring bill?",
  "includeTransactionContext": false
}
```

**Financial Questions:**
```http
POST /api/chat/message

{
  "message": "What's my disposable income this month?",
  "includeTransactionContext": true
}
```

---

### 2. Search Documentation (NEW!)

Directly search application documentation without AI processing.

**Endpoint:** `GET /api/chat/search-documentation`

**Query Parameters:**
- `query` (string, required): Search query

**Request:**
```http
GET /api/chat/search-documentation?query=loan API endpoints
Authorization: Bearer YOUR_TOKEN
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Documentation retrieved successfully",
  "data": "=== RELEVANT DOCUMENTATION ===\n\n📄 From: API/Loan_API.md (Section: Create Loan)\n[Documentation content here]\n---"
}
```

---

### 3. Create New Conversation

**Endpoint:** `POST /api/chat/conversations`

**Request Body:**
```json
{
  "title": "Financial Planning Discussion"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "conversation-123",
    "title": "Financial Planning Discussion",
    "startedAt": "2025-10-28T14:30:00Z",
    "lastMessageAt": "2025-10-28T14:30:00Z",
    "isActive": true,
    "totalMessages": 0,
    "totalTokensUsed": 0,
    "messages": []
  }
}
```

---

### 4. Get All Conversations

**Endpoint:** `GET /api/chat/conversations`

**Query Parameters:**
- `page` (int, optional): Page number (default: 1)
- `limit` (int, optional): Items per page (default: 10)

**Request:**
```http
GET /api/chat/conversations?page=1&limit=10
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": "conv-123",
        "title": "New Conversation",
        "startedAt": "2025-10-28T10:00:00Z",
        "lastMessageAt": "2025-10-28T10:30:00Z",
        "isActive": true,
        "totalMessages": 6,
        "totalTokensUsed": 1250
      }
    ],
    "page": 1,
    "limit": 10,
    "totalCount": 1
  }
}
```

---

### 5. Get Conversation History

**Endpoint:** `GET /api/chat/conversations/{conversationId}/messages`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "conv-123",
    "title": "Financial Discussion",
    "messages": [
      {
        "id": "msg-1",
        "role": "user",
        "content": "What bills do I have?",
        "timestamp": "2025-10-28T10:00:00Z",
        "tokensUsed": 0
      },
      {
        "id": "msg-2",
        "role": "assistant",
        "content": "You have 3 bills coming up...",
        "timestamp": "2025-10-28T10:00:05Z",
        "tokensUsed": 150
      }
    ]
  }
}
```

---

### 6. Delete Conversation

**Endpoint:** `DELETE /api/chat/conversations/{conversationId}`

**Response:**
```json
{
  "success": true,
  "message": "Conversation deleted successfully",
  "data": true
}
```

---

### 7. Get Financial Context

Get user's financial data that AI uses for context.

**Endpoint:** `GET /api/chat/financial-context`

**Response:**
```json
{
  "success": true,
  "data": {
    "financialSummary": {
      "totalIncome": 5000,
      "totalExpenses": 2000,
      "disposableAmount": 3000,
      "totalSavings": 10000,
      "totalDebt": 5000,
      "netWorth": 8000
    },
    "upcomingBills": [...],
    "recentTransactions": [...],
    "activeLoans": [...],
    "savingsAccounts": [...],
    "recentExpenses": [...]
  }
}
```

---

### 8. Get Bill Reminders

**Endpoint:** `GET /api/chat/bill-reminders`

**Response:**
```json
{
  "success": true,
  "data": [
    "Bill 'Electric' of $125.50 is due on Nov 01, 2025",
    "Bill 'Internet' of $79.99 is due on Nov 03, 2025"
  ]
}
```

---

### 9. Get Budget Suggestions

**Endpoint:** `GET /api/chat/budget-suggestions`

**Response:**
```json
{
  "success": true,
  "data": [
    "Your expenses exceed your income. Consider reducing variable expenses.",
    "You have 3 bills totaling $250.49 due in the next 7 days."
  ]
}
```

---

### 10. Generate Report (Placeholder)

**Endpoint:** `POST /api/chat/generate-report`

**Request Body:**
```json
{
  "reportType": "financial_summary",
  "format": "pdf"
}
```

---

## 🎯 Use Cases

### Use Case 1: Ask About Application Features

```http
POST /api/chat/message
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "message": "How do I add a recurring bill?",
  "includeTransactionContext": false
}
```

**AI Response:**
> "According to the documentation, to add a recurring bill, you can use the POST /api/bills endpoint with the frequency parameter set to 'monthly', 'weekly', or 'yearly'..."

---

### Use Case 2: Get Financial Advice

```http
POST /api/chat/message

{
  "message": "Should I pay off my loan early or save the money?",
  "includeTransactionContext": true
}
```

**AI Response:**
> "Based on your financial data, you have $3,000 in disposable income and an active loan with 5% interest. Here's my advice..."

---

### Use Case 3: Check Bill Status

```http
POST /api/chat/message

{
  "message": "What bills are due this week?",
  "includeTransactionContext": true
}
```

---

### Use Case 4: Search Documentation Only

```http
GET /api/chat/search-documentation?query=soft delete implementation
```

---

## 🔧 Configuration

Configuration in `appsettings.json`:

```json
{
  "OpenAISettings": {
    "ApiKey": "sk-...",
    "Model": "gpt-4-turbo-preview",
    "MaxTokens": 1000,
    "Temperature": 0.7,
    "SystemPrompt": "You are a helpful assistant...",
    "MaxConversationHistory": 10,
    "RateLimitPerMinute": 10
  }
}
```

**Settings Explained:**
- `ApiKey`: Your OpenAI API key
- `Model`: GPT model to use (gpt-4-turbo-preview, gpt-3.5-turbo)
- `MaxTokens`: Maximum response length
- `Temperature`: Response creativity (0.0-1.0)
- `MaxConversationHistory`: How many previous messages to include
- `RateLimitPerMinute`: Max messages per minute per user

---

## 🎨 Features

### ✅ What's Implemented

1. **Documentation Search**
   - Automatically indexes all `.md` files in Documentation folder
   - Searches by relevance scoring (section headers, file names, content)
   - Caches index for 24 hours for fast searches
   - Provides top 3 most relevant documentation chunks

2. **Financial Context**
   - Last 30 days of transactions
   - Upcoming bills (next 7 days)
   - Active loans
   - Savings accounts
   - Recent variable expenses
   - Cached for 5 minutes per user

3. **Conversation Management**
   - Create, list, view, and delete conversations
   - Maintains conversation history
   - Token usage tracking
   - Pagination support

4. **Rate Limiting**
   - 10 messages per minute per user (configurable)
   - Prevents API abuse

5. **Suggested Actions**
   - AI analyzes response and suggests next steps
   - Examples: "View upcoming bills", "Review budget", "Check loan status"

---

## 🧪 Testing with cURL

### Test Basic Chat
```bash
curl -X POST http://localhost:5000/api/chat/message \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "message": "What bills do I have?",
    "includeTransactionContext": true
  }'
```

### Test Documentation Search
```bash
curl -X GET "http://localhost:5000/api/chat/search-documentation?query=loan%20API" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test Bill Reminders
```bash
curl -X GET http://localhost:5000/api/chat/bill-reminders \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 🚨 Error Responses

### 401 Unauthorized
```json
{
  "success": false,
  "message": "User not authenticated"
}
```

### 400 Bad Request - Rate Limited
```json
{
  "success": false,
  "message": "Rate limit exceeded. Please wait before sending another message."
}
```

### 400 Bad Request - OpenAI Error
```json
{
  "success": false,
  "message": "OpenAI API key not configured. Please contact administrator."
}
```

---

## 💡 Tips for Best Results

### For Application Questions
- Be specific: "How do I delete a bill?" instead of "bills"
- Ask about features: "What endpoints are available for loans?"
- Inquire about implementation: "How does soft delete work?"

### For Financial Questions
- Set `includeTransactionContext: true`
- Be specific about timeframes: "this month", "next week"
- Ask for advice: "Should I...", "How can I..."

---

## 📊 Response Time

- **With Financial Context**: 2-5 seconds
- **Without Context**: 1-3 seconds
- **Documentation Search Only**: < 1 second (cached)

---

## 🔒 Security

- All endpoints require JWT authentication
- User data is isolated (users only see their own data)
- OpenAI API key stored securely in configuration
- Rate limiting prevents abuse
- CORS enabled for approved origins

---

## 🎓 Examples

### Complete Workflow Example

```javascript
// 1. Login
const loginResponse = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password123'
  })
});
const { data: { token } } = await loginResponse.json();

// 2. Start Conversation
const chatResponse = await fetch('http://localhost:5000/api/chat/message', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    message: 'What bills do I have coming up?',
    includeTransactionContext: true
  })
});

// 3. Continue Conversation
const { data: { conversationId } } = await chatResponse.json();
const followUpResponse = await fetch('http://localhost:5000/api/chat/message', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    message: 'How can I reduce my expenses?',
    conversationId: conversationId,
    includeTransactionContext: true
  })
});
```

---

## 📚 Related Documentation

- [API Endpoints Reference](./API/API_ENDPOINTS_REFERENCE.md)
- [Authentication Guide](./General/authenticationGuide.md)
- [Bill Management](./Billing/billingApiDocumentation.md)
- [Loan Management](./Loan/README.md)

---

## 🆘 Troubleshooting

### Issue: "OpenAI API key not configured"
**Solution:** Set valid OpenAI API key in `appsettings.json`

### Issue: "Rate limit exceeded"
**Solution:** Wait 1 minute or increase `RateLimitPerMinute` in settings

### Issue: "No relevant documentation found"
**Solution:** Check if documentation files exist in `Documentation/` folder

### Issue: Empty or unhelpful responses
**Solution:** 
- Ensure `includeTransactionContext: true` for financial questions
- Be more specific in your questions
- Check if user has financial data in the system

---

## 📝 Notes

- Documentation index is cached for 24 hours and rebuilt automatically
- Financial context is cached for 5 minutes per user
- Token usage is tracked per conversation
- The AI uses GPT-4 Turbo for best results
- All timestamps are in UTC

---

**Last Updated:** October 28, 2025
**Version:** 1.0
**Status:** ✅ Production Ready

