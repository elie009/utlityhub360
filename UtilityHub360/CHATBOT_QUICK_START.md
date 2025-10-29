# 🤖 AI Chatbot - Quick Start Guide

## ⚡ 5-Minute Setup

### 1. Verify OpenAI API Key

Check `UtilityHub360/appsettings.json`:
```json
{
  "OpenAISettings": {
    "ApiKey": "sk-proj-..."  // ✅ Already configured
  }
}
```

### 2. Run the Application

```bash
cd UtilityHub360
dotnet run
```

You should see:
```
Now listening on: http://localhost:5000
```

### 3. Get JWT Token

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"your@email.com","password":"YourPassword123!"}'
```

Copy the `token` from the response.

### 4. Test the Chatbot!

**Ask about the application:**
```bash
curl -X POST http://localhost:5000/api/chat/message \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "message": "How do I create a recurring bill?",
    "includeTransactionContext": false
  }'
```

**Ask about your finances:**
```bash
curl -X POST http://localhost:5000/api/chat/message \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "message": "What bills do I have coming up?",
    "includeTransactionContext": true
  }'
```

---

## 🎯 What Can I Ask?

### Application Questions ❓
- "How do I create a loan?"
- "What API endpoints are available for bills?"
- "How does soft delete work?"
- "What's the deployment process?"
- "How do I add a recurring bill?"

### Financial Questions 💰
- "What bills are due this week?"
- "How much disposable income do I have?"
- "Should I pay off my loan early?"
- "What's my total debt?"
- "How can I save more money?"

---

## 📡 All Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/chat/message` | POST | Send message to AI |
| `/api/chat/search-documentation` | GET | Search docs only |
| `/api/chat/conversations` | GET | List conversations |
| `/api/chat/conversations` | POST | Create conversation |
| `/api/chat/conversations/{id}/messages` | GET | Get history |
| `/api/chat/conversations/{id}` | DELETE | Delete conversation |
| `/api/chat/financial-context` | GET | Get financial data |
| `/api/chat/bill-reminders` | GET | Get bill reminders |
| `/api/chat/budget-suggestions` | GET | Get suggestions |

---

## 🧪 Test in VS Code

1. Open `Tests/chat-api-tests.http`
2. Install "REST Client" extension
3. Update `@token` variable with your JWT
4. Click "Send Request" on any test

---

## 📚 Full Documentation

- **Complete API Docs:** `Documentation/Chat_API_Documentation.md`
- **Implementation Details:** `Documentation/CHAT_AI_ENHANCEMENT_SUMMARY.md`
- **Test Collection:** `Tests/chat-api-tests.http`

---

## 🚀 You're Ready!

Your AI chatbot can now:
✅ Answer questions from 160+ documentation files  
✅ Provide personalized financial advice  
✅ Search documentation directly  
✅ Maintain conversation history  
✅ Track token usage  

**Try it now!** 🎉

