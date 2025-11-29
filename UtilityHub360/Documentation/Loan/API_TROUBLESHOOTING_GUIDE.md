# 🔧 Loan Payment Schedule API - Troubleshooting Guide

## 🚨 **Common 404 Error - Duplicate /api/**

### **Problem:**
```
404 Not Found
URL: /api/api/Loans/da188a68-ebe3-4288-b56d-d9e0a922dc81/add-schedule
     ↑↑↑ Duplicate /api/
```

### **Solution:**
Remove one `/api/` from your URL:

❌ **WRONG:**
```
/api/api/Loans/da188a68-ebe3-4288-b56d-d9e0a922dc81/add-schedule
```

✅ **CORRECT:**
```
/api/Loans/da188a68-ebe3-4288-b56d-d9e0a922dc81/add-schedule
```

### **Where This Happens:**

#### **1. Frontend JavaScript/TypeScript**
```javascript
// ❌ WRONG
const response = await fetch('/api/api/Loans/loan-id/add-schedule', ...);

// ✅ CORRECT
const response = await fetch('/api/Loans/loan-id/add-schedule', ...);
```

#### **2. Axios Configuration**
```javascript
// ❌ WRONG - baseURL already has /api/
const api = axios.create({ baseURL: 'http://localhost:5000/api' });
api.post('/api/Loans/loan-id/add-schedule', ...); // Results in /api/api/

// ✅ CORRECT - Don't include /api/ in the path
const api = axios.create({ baseURL: 'http://localhost:5000/api' });
api.post('/Loans/loan-id/add-schedule', ...);

// OR

// ✅ CORRECT - No /api/ in baseURL
const api = axios.create({ baseURL: 'http://localhost:5000' });
api.post('/api/Loans/loan-id/add-schedule', ...);
```

#### **3. React/Angular HTTP Client**
```typescript
// ❌ WRONG
this.http.post('/api/api/Loans/loan-id/add-schedule', data);

// ✅ CORRECT
this.http.post('/api/Loans/loan-id/add-schedule', data);
```

---

## 🔍 **All Correct API Endpoints**

### **Base URLs:**
- Development: `http://localhost:5000`
- Production: `https://your-domain.com`

### **Payment Schedule Endpoints:**

| Action | Method | Correct URL |
|--------|--------|-------------|
| Add Custom Installments | POST | `/api/Loans/{loanId}/add-schedule` |
| Extend Loan Term | POST | `/api/Loans/{loanId}/extend-term` |
| Regenerate Schedule | POST | `/api/Loans/{loanId}/regenerate-schedule` |
| Update Schedule | PATCH | `/api/Loans/{loanId}/schedule/{installmentNumber}` |
| Mark as Paid | POST | `/api/Loans/{loanId}/schedule/{installmentNumber}/mark-paid` |
| Update Due Date | PUT | `/api/Loans/{loanId}/schedule/{installmentNumber}` |
| Delete Installment | DELETE | `/api/Loans/{loanId}/schedule/{installmentNumber}` |
| Get Schedule | GET | `/api/Loans/{loanId}/schedule` |

---

## 🚨 **Other Common Errors**

### **1. 401 Unauthorized**

**Problem:**
```json
{
  "success": false,
  "message": "User not authenticated"
}
```

**Solutions:**

#### A. Missing Authorization Header
```javascript
// ❌ WRONG
fetch('/api/Loans/loan-id/add-schedule', {
  method: 'POST',
  body: JSON.stringify(data)
});

// ✅ CORRECT
fetch('/api/Loans/loan-id/add-schedule', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(data)
});
```

#### B. Expired Token
```javascript
// Get a fresh token by logging in again
const loginResponse = await fetch('/api/Auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password })
});

const { token } = await loginResponse.json();
localStorage.setItem('token', token);
```

---

### **2. 403 Forbidden**

**Problem:**
```json
{
  "success": false,
  "message": "You can only update your own loans"
}
```

**Solution:**
- Ensure you're trying to access your own loan
- Check if you're an ADMIN user
- Verify the loan belongs to the authenticated user

---

### **3. 404 Not Found (Correct URL)**

**Problem:**
```
404 Not Found
URL: /api/Loans/invalid-loan-id/add-schedule
```

**Solutions:**

#### A. Invalid Loan ID
```javascript
// Verify loan exists first
const loanResponse = await fetch(`/api/Loans/${loanId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
});

if (loanResponse.ok) {
  // Loan exists, proceed with operation
} else {
  console.error('Loan not found');
}
```

#### B. Check Available Loans
```javascript
// Get all user loans
const loansResponse = await fetch(`/api/Loans/user/${userId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
});

const { data } = await loansResponse.json();
console.log('Available loans:', data);
```

---

### **4. 400 Bad Request - Validation Error**

**Problem:**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Amount must be greater than 0",
    "Starting installment number is required"
  ]
}
```

**Solutions:**

#### A. Check Required Fields
```javascript
// ❌ WRONG - Missing required fields
{
  "numberOfMonths": 3
}

// ✅ CORRECT - All required fields
{
  "startingInstallmentNumber": 13,
  "numberOfMonths": 3,
  "firstDueDate": "2024-07-15T00:00:00Z",
  "monthlyPayment": 1200.00
}
```

#### B. Validate Data Types
```javascript
// ❌ WRONG - String instead of number
{
  "monthlyPayment": "1200.00"  // String
}

// ✅ CORRECT - Number
{
  "monthlyPayment": 1200.00  // Number
}
```

---

### **5. 500 Internal Server Error**

**Problem:**
```json
{
  "success": false,
  "message": "Failed to add payment schedule: Database connection failed"
}
```

**Solutions:**
- Check database connection
- Verify appsettings.json configuration
- Check server logs for detailed error
- Ensure database migrations are applied

---

## 🧪 **Testing Your API**

### **Method 1: Using .http File (VS Code)**

1. Open `LOAN_PAYMENT_SCHEDULE_APIs.http`
2. Replace `YOUR_JWT_TOKEN_HERE` with your actual token
3. Replace `da188a68-ebe3-4288-b56d-d9e0a922dc81` with your loan ID
4. Click "Send Request" button

### **Method 2: Using Swagger**

1. Navigate to `http://localhost:5000/swagger`
2. Click "Authorize" button
3. Enter: `Bearer YOUR_TOKEN`
4. Try any endpoint under "Loans"

### **Method 3: Using cURL**

```bash
# Get your token first
curl -X POST "http://localhost:5000/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "your@email.com", "password": "yourpassword"}'

# Use the token
curl -X POST "http://localhost:5000/api/Loans/loan-id/add-schedule" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startingInstallmentNumber": 13,
    "numberOfMonths": 3,
    "firstDueDate": "2024-07-15T00:00:00Z",
    "monthlyPayment": 1200.00
  }'
```

### **Method 4: Using Postman**

1. Create new POST request
2. URL: `http://localhost:5000/api/Loans/loan-id/add-schedule`
3. Headers:
   - `Authorization`: `Bearer YOUR_TOKEN`
   - `Content-Type`: `application/json`
4. Body (raw JSON):
```json
{
  "startingInstallmentNumber": 13,
  "numberOfMonths": 3,
  "firstDueDate": "2024-07-15T00:00:00Z",
  "monthlyPayment": 1200.00
}
```

---

## 📋 **Request/Response Validation Checklist**

### **Before Making Request:**
- [ ] URL has single `/api/` (not `/api/api/`)
- [ ] Authorization header included
- [ ] Token is valid (not expired)
- [ ] Content-Type header set to `application/json`
- [ ] Request body is valid JSON
- [ ] All required fields provided
- [ ] Data types are correct (numbers, not strings)
- [ ] Loan ID is valid and exists
- [ ] Loan belongs to authenticated user

### **After Receiving Error:**
- [ ] Check HTTP status code
- [ ] Read error message carefully
- [ ] Verify all request parameters
- [ ] Check network tab in browser dev tools
- [ ] Review API documentation
- [ ] Test with Swagger UI
- [ ] Check server logs if 500 error

---

## 🎯 **Quick Fix Commands**

### **Check if API is running:**
```bash
curl http://localhost:5000/health
```

### **Check if endpoint exists:**
```bash
# Should see Swagger UI
curl http://localhost:5000/swagger
```

### **Test authentication:**
```bash
curl -X POST "http://localhost:5000/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

### **Verify loan exists:**
```bash
curl -X GET "http://localhost:5000/api/Loans/YOUR_LOAN_ID" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 📞 **Still Having Issues?**

1. **Check Server Logs:**
   - Open Visual Studio Output window
   - Look for error messages
   - Check console for exceptions

2. **Use Swagger UI:**
   - Navigate to `/swagger`
   - Test endpoints directly
   - See request/response examples

3. **Review Documentation:**
   - `PaymentScheduleManagement.md`
   - `SIMPLE_SCHEDULE_UPDATE_API.md`
   - `MARK_INSTALLMENT_PAID_API.md`

4. **Common Fixes:**
   - Restart API server
   - Clear browser cache
   - Get fresh JWT token
   - Verify database connection

---

## ✅ **Success Indicators**

You know it's working when you see:
```json
{
  "success": true,
  "message": "Payment schedules added successfully",
  "data": {
    "schedule": [...],
    "totalInstallments": 15,
    "message": "3 new payment installments added..."
  }
}
```

---

**Last Updated:** October 12, 2025  
**API Version:** 2.2.0

