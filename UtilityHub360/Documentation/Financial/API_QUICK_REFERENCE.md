# 🚀 Disposable Amount API — Quick Reference

## One Simple Endpoint for Everything!

### Base Endpoint
```
GET /api/Dashboard/disposable-amount
```

---

## 📊 Usage Scenarios

### 1. Get Current Month (Default)
**No parameters needed!**

```http
GET /api/Dashboard/disposable-amount
Authorization: Bearer {your-jwt-token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "userId": "user123",
    "period": "MONTHLY",
    "startDate": "2025-10-01T00:00:00Z",
    "endDate": "2025-10-31T23:59:59Z",
    
    "totalIncome": 69510.00,
    "totalFixedExpenses": 30999.00,
    "totalVariableExpenses": 19000.00,
    "disposableAmount": 19511.00,
    "disposablePercentage": 28.07,
    
    "incomeBreakdown": [...],
    "billsBreakdown": [...],
    "loansBreakdown": [...],
    "variableExpensesBreakdown": [...],
    
    "insights": [
      "Your disposable income increased by 12.5% compared to the previous period.",
      "Your highest spending category is GROCERIES at ₱8,500 (44.7%)"
    ],
    
    "comparison": {
      "previousPeriodDisposableAmount": 17350.00,
      "changeAmount": 2161.00,
      "changePercentage": 12.46,
      "trend": "UP"
    }
  }
}
```

---

### 2. Get Specific Month

```http
GET /api/Dashboard/disposable-amount?year=2025&month=9
Authorization: Bearer {your-jwt-token}
```

**Parameters:**
- `year` (int): Year (e.g., 2025)
- `month` (int): Month (1-12)

---

### 3. Get Custom Date Range

```http
GET /api/Dashboard/disposable-amount?startDate=2025-07-01&endDate=2025-09-30
Authorization: Bearer {your-jwt-token}
```

**Parameters:**
- `startDate` (datetime): Start date (ISO 8601)
- `endDate` (datetime): End date (ISO 8601)

---

### 4. Add Savings Goals

```http
GET /api/Dashboard/disposable-amount?targetSavings=5000&investmentAllocation=3000
Authorization: Bearer {your-jwt-token}
```

**Additional Parameters:**
- `targetSavings` (decimal): Monthly savings target
- `investmentAllocation` (decimal): Monthly investment amount

**Response includes:**
```json
{
  "data": {
    "disposableAmount": 19511.00,
    "targetSavings": 5000.00,
    "investmentAllocation": 3000.00,
    "netDisposableAmount": 11511.00  // After savings/investments
  }
}
```

---

## 🔐 Admin Endpoint

### Get Any User's Disposable Amount

```http
GET /api/Dashboard/disposable-amount/{userId}
Authorization: Bearer {admin-jwt-token}
```

Same parameters as above (year, month, startDate, endDate, etc.)

**Example:**
```http
GET /api/Dashboard/disposable-amount/user456?year=2025&month=10
```

---

## 💡 Complete Response Structure

```json
{
  "success": true,
  "message": "Disposable amount calculated successfully",
  "data": {
    // PERIOD INFO
    "userId": "user123",
    "period": "MONTHLY",
    "startDate": "2025-10-01T00:00:00Z",
    "endDate": "2025-10-31T23:59:59Z",
    
    // INCOME DETAILS
    "totalIncome": 69510.00,
    "incomeBreakdown": [
      {
        "sourceName": "Monthly Salary",
        "category": "PRIMARY",
        "amount": 45000.00,
        "monthlyAmount": 45000.00,
        "frequency": "MONTHLY"
      }
    ],
    
    // FIXED EXPENSES
    "totalFixedExpenses": 30999.00,
    "totalBills": 18499.00,
    "billsBreakdown": [
      {
        "id": "bill123",
        "name": "Meralco Electricity",
        "type": "utility",
        "amount": 2500.00,
        "status": "PENDING",
        "dueDate": "2025-10-15T00:00:00Z"
      }
    ],
    "totalLoans": 12500.00,
    "loansBreakdown": [
      {
        "id": "loan123",
        "name": "Personal Loan",
        "type": "LOAN",
        "amount": 4000.00,
        "status": "ACTIVE"
      }
    ],
    
    // VARIABLE EXPENSES
    "totalVariableExpenses": 19000.00,
    "variableExpensesBreakdown": [
      {
        "category": "GROCERIES",
        "totalAmount": 8500.00,
        "count": 12,
        "percentage": 44.74
      }
    ],
    
    // DISPOSABLE AMOUNT
    "disposableAmount": 19511.00,
    "disposablePercentage": 28.07,
    
    // OPTIONAL: SAVINGS GOALS
    "targetSavings": 5000.00,
    "investmentAllocation": 3000.00,
    "netDisposableAmount": 11511.00,
    
    // INSIGHTS
    "insights": [
      "Your disposable income increased by 12.5% compared to the previous period.",
      "Your highest spending category is GROCERIES at ₱8,500 (44.7% of variable expenses).",
      "Consider saving at least ₱3,900 per month (20% of your disposable income).",
      "Reducing your variable expenses by 15% (₱2,850) can increase your savings by 24.8%."
    ],
    
    // COMPARISON
    "comparison": {
      "previousPeriodDisposableAmount": 17350.00,
      "changeAmount": 2161.00,
      "changePercentage": 12.46,
      "trend": "UP"
    }
  }
}
```

---

## 🧪 Quick Test Examples

### JavaScript/Fetch

```javascript
// Current month
const response = await fetch('https://your-domain.com/api/Dashboard/disposable-amount', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
const data = await response.json();
console.log('Disposable Amount:', data.data.disposableAmount);
```

### JavaScript/Axios

```javascript
const { data } = await axios.get('/api/Dashboard/disposable-amount', {
  params: {
    year: 2025,
    month: 10,
    targetSavings: 5000
  },
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
console.log('Disposable:', data.data.disposableAmount);
```

### C#

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

var response = await client.GetAsync(
    "https://your-domain.com/api/Dashboard/disposable-amount?year=2025&month=10"
);
var data = await response.Content.ReadAsStringAsync();
```

### Python

```python
import requests

headers = {'Authorization': f'Bearer {token}'}
params = {'year': 2025, 'month': 10}

response = requests.get(
    'https://your-domain.com/api/Dashboard/disposable-amount',
    headers=headers,
    params=params
)
data = response.json()
print(f"Disposable: {data['data']['disposableAmount']}")
```

### cURL

```bash
# Current month
curl -X GET "https://your-domain.com/api/Dashboard/disposable-amount" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Specific month
curl -X GET "https://your-domain.com/api/Dashboard/disposable-amount?year=2025&month=9" \
  -H "Authorization: Bearer YOUR_TOKEN"

# With savings goals
curl -X GET "https://your-domain.com/api/Dashboard/disposable-amount?targetSavings=5000&investmentAllocation=3000" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ⚠️ Error Responses

### 401 Unauthorized
```json
{
  "success": false,
  "message": "User not authenticated"
}
```

### 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid year or month"
}
```

---

## 📝 Query Parameter Summary

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `year` | int | No | Specific year | 2025 |
| `month` | int | No | Specific month (1-12) | 10 |
| `startDate` | datetime | No | Custom range start | 2025-07-01 |
| `endDate` | datetime | No | Custom range end | 2025-09-30 |
| `targetSavings` | decimal | No | Monthly savings goal | 5000.00 |
| `investmentAllocation` | decimal | No | Monthly investment | 3000.00 |

**Priority:**
1. If `startDate` and `endDate` provided → Custom range
2. If `year` and `month` provided → Specific month
3. Otherwise → Current month

---

## 🎯 What You Get

✅ **Total Income** - All income sources converted to monthly  
✅ **Fixed Expenses** - Bills + Loan payments  
✅ **Variable Expenses** - All discretionary spending by category  
✅ **Disposable Amount** - Money left after all expenses  
✅ **Percentage** - Disposable as % of income  
✅ **Complete Breakdown** - Every income source and expense  
✅ **Smart Insights** - AI-generated recommendations  
✅ **Trend Comparison** - vs previous period  
✅ **Savings Goals** - Optional target tracking

---

## 🚀 Try It Now!

1. Open Swagger: `http://localhost:5000/swagger`
2. Click "Authorize" and enter your JWT token
3. Find `Dashboard` → `GET /api/Dashboard/disposable-amount`
4. Click "Try it out"
5. Leave parameters empty for current month
6. Click "Execute"

**You'll get all your financial data in ONE call!**

---

**Last Updated:** October 11, 2025  
**Version:** 2.0 (Simplified - One Endpoint)  
**Status:** ✅ Production Ready

