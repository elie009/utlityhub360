# 🔌 Disposable Amount & Financial Dashboard — API Documentation

## 📖 Complete API Reference

**Base URL:** `https://your-domain.com/api`  
**Authentication:** Bearer JWT Token (Required for all endpoints)  
**Version:** 1.0.0

---

## 📑 Table of Contents

1. [Dashboard Endpoints](#dashboard-endpoints)
2. [Variable Expenses Endpoints](#variable-expenses-endpoints)
3. [Common Response Structures](#common-response-structures)
4. [Error Handling](#error-handling)
5. [Code Examples](#code-examples)

---

## 🎯 Dashboard Endpoints

### 1. Get Current Month Disposable Amount

**Endpoint:** `GET /api/Dashboard/disposable-amount/current`

**Description:** Calculate disposable amount for the current month with optional savings goals.

**Authorization:** Required (User or Admin)

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `targetSavings` | decimal | No | Monthly savings goal amount |
| `investmentAllocation` | decimal | No | Monthly investment amount |

**Example Request:**
```http
GET /api/Dashboard/disposable-amount/current?targetSavings=5000&investmentAllocation=3000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Disposable amount calculated successfully",
  "data": {
    "userId": "user123",
    "period": "MONTHLY",
    "startDate": "2025-10-01T00:00:00Z",
    "endDate": "2025-10-31T23:59:59Z",
    
    "totalIncome": 69510.00,
    "incomeBreakdown": [
      {
        "sourceName": "Monthly Salary",
        "category": "PRIMARY",
        "amount": 45000.00,
        "monthlyAmount": 45000.00,
        "frequency": "MONTHLY"
      },
      {
        "sourceName": "Freelance Work",
        "category": "SIDE_HUSTLE",
        "amount": 5000.00,
        "monthlyAmount": 10850.00,
        "frequency": "BI_WEEKLY"
      }
    ],
    
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
    
    "totalVariableExpenses": 19000.00,
    "variableExpensesBreakdown": [
      {
        "category": "GROCERIES",
        "totalAmount": 8500.00,
        "count": 12,
        "percentage": 44.74
      },
      {
        "category": "TRANSPORTATION",
        "totalAmount": 4200.00,
        "count": 8,
        "percentage": 22.11
      }
    ],
    
    "disposableAmount": 19511.00,
    "disposablePercentage": 28.07,
    
    "targetSavings": 5000.00,
    "investmentAllocation": 3000.00,
    "netDisposableAmount": 11511.00,
    
    "insights": [
      "Your disposable income increased by 12.5% compared to the previous period.",
      "Your highest spending category is GROCERIES at $8,500 (44.7% of variable expenses).",
      "Consider saving at least $3,900 per month (20% of your disposable income) to build your financial cushion.",
      "Reducing your variable expenses by 15% ($2,850) can increase your savings by 24.8%."
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

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token
- `400 Bad Request` - Invalid parameters

---

### 2. Get Monthly Disposable Amount

**Endpoint:** `GET /api/Dashboard/disposable-amount/monthly`

**Description:** Calculate disposable amount for a specific month and year.

**Authorization:** Required (User or Admin)

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `year` | int | Yes | Year (e.g., 2025) |
| `month` | int | Yes | Month (1-12) |
| `targetSavings` | decimal | No | Monthly savings goal |
| `investmentAllocation` | decimal | No | Monthly investment amount |

**Example Request:**
```http
GET /api/Dashboard/disposable-amount/monthly?year=2025&month=9&targetSavings=5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Disposable amount calculated successfully",
  "data": {
    "userId": "user123",
    "period": "MONTHLY",
    "startDate": "2025-09-01T00:00:00Z",
    "endDate": "2025-09-30T23:59:59Z",
    "totalIncome": 65000.00,
    "totalFixedExpenses": 29500.00,
    "totalVariableExpenses": 18150.00,
    "disposableAmount": 17350.00,
    "disposablePercentage": 26.69
    // ... (similar structure to current month)
  }
}
```

**Validation:**
- Year must be between 2000 and 2100
- Month must be between 1 and 12

**Status Codes:**
- `200 OK` - Success
- `400 Bad Request` - Invalid year or month
- `401 Unauthorized` - Missing or invalid token

---

### 3. Get Custom Date Range Disposable Amount

**Endpoint:** `GET /api/Dashboard/disposable-amount/custom`

**Description:** Calculate disposable amount for a custom date range.

**Authorization:** Required (User or Admin)

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `startDate` | datetime | Yes | Start date (ISO 8601) |
| `endDate` | datetime | Yes | End date (ISO 8601) |
| `targetSavings` | decimal | No | Savings goal for period |
| `investmentAllocation` | decimal | No | Investment amount for period |

**Example Request:**
```http
GET /api/Dashboard/disposable-amount/custom?startDate=2025-07-01&endDate=2025-09-30&targetSavings=15000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Disposable amount calculated successfully",
  "data": {
    "userId": "user123",
    "period": "CUSTOM",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-09-30T23:59:59Z",
    "totalIncome": 198000.00,
    "totalFixedExpenses": 88500.00,
    "totalVariableExpenses": 54200.00,
    "disposableAmount": 55300.00,
    "disposablePercentage": 27.93
    // ... (similar structure)
  }
}
```

**Validation:**
- `startDate` must be before `endDate`
- Both dates must be valid ISO 8601 format

**Status Codes:**
- `200 OK` - Success
- `400 Bad Request` - Invalid date range
- `401 Unauthorized` - Missing or invalid token

---

### 4. Get Financial Summary

**Endpoint:** `GET /api/Dashboard/financial-summary`

**Description:** Get comprehensive financial dashboard with current month, previous month, and year-to-date data.

**Authorization:** Required (User or Admin)

**Query Parameters:** None

**Example Request:**
```http
GET /api/Dashboard/financial-summary
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Financial summary retrieved successfully",
  "data": {
    "userId": "user123",
    "generatedAt": "2025-10-11T15:30:00Z",
    
    "currentMonth": {
      "month": 10,
      "year": 2025,
      "totalIncome": 69510.00,
      "totalExpenses": 49999.00,
      "fixedExpenses": 30999.00,
      "variableExpenses": 19000.00,
      "disposableAmount": 19511.00,
      "savingsAmount": 19511.00,
      "savingsRate": 28.07
    },
    
    "previousMonth": {
      "month": 9,
      "year": 2025,
      "totalIncome": 65000.00,
      "totalExpenses": 47650.00,
      "fixedExpenses": 29500.00,
      "variableExpenses": 18150.00,
      "disposableAmount": 17350.00,
      "savingsAmount": 17350.00,
      "savingsRate": 26.69
    },
    
    "yearToDate": {
      "year": 2025,
      "totalIncome": 673540.00,
      "totalExpenses": 485230.00,
      "totalDisposable": 188310.00,
      "averageMonthlyDisposable": 18831.00,
      "totalSavings": 188310.00,
      "monthlyBreakdown": [
        {
          "month": 1,
          "monthName": "January",
          "income": 65000.00,
          "expenses": 48500.00,
          "disposable": 16500.00
        },
        // ... months 2-10
      ]
    },
    
    "stats": {
      "averageMonthlyIncome": 67354.00,
      "averageMonthlyExpenses": 48523.00,
      "averageDisposable": 18831.00,
      "topExpenseCategory": "GROCERIES",
      "topExpenseCategoryAmount": 8500.00,
      "activeLoans": 2,
      "totalLoanBalance": 145000.00,
      "pendingBills": 4,
      "pendingBillsAmount": 18499.00
    }
  }
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token

---

### 5. Get User Disposable Amount (Admin Only)

**Endpoint:** `GET /api/Dashboard/disposable-amount/user/{userId}`

**Description:** Admin endpoint to view any user's disposable amount.

**Authorization:** Required (Admin only)

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | string | Yes | User ID to query |

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `startDate` | datetime | No | Start date (defaults to current month start) |
| `endDate` | datetime | No | End date (defaults to current month end) |

**Example Request:**
```http
GET /api/Dashboard/disposable-amount/user/user456?startDate=2025-10-01&endDate=2025-10-31
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Disposable amount calculated successfully",
  "data": {
    "userId": "user456",
    "period": "MONTHLY",
    // ... (same structure as regular disposable amount)
  }
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - User does not have admin role
- `404 Not Found` - User not found

---

### 6. Get User Financial Summary (Admin Only)

**Endpoint:** `GET /api/Dashboard/financial-summary/user/{userId}`

**Description:** Admin endpoint to view any user's financial summary.

**Authorization:** Required (Admin only)

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | string | Yes | User ID to query |

**Example Request:**
```http
GET /api/Dashboard/financial-summary/user/user456
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Financial summary retrieved successfully",
  "data": {
    "userId": "user456",
    // ... (same structure as regular financial summary)
  }
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - User does not have admin role
- `404 Not Found` - User not found

---

## 💸 Variable Expenses Endpoints

### 1. Get All Variable Expenses

**Endpoint:** `GET /api/VariableExpenses`

**Description:** Get all variable expenses for the authenticated user with optional filters.

**Authorization:** Required (User or Admin)

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `startDate` | datetime | No | Filter by start date |
| `endDate` | datetime | No | Filter by end date |
| `category` | string | No | Filter by category |

**Example Request:**
```http
GET /api/VariableExpenses?startDate=2025-10-01&endDate=2025-10-31&category=GROCERIES
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Variable expenses retrieved successfully",
  "data": [
    {
      "id": "exp123",
      "userId": "user123",
      "description": "SM Supermarket",
      "amount": 3200.00,
      "category": "GROCERIES",
      "currency": "USD",
      "expenseDate": "2025-10-05T00:00:00Z",
      "notes": "Weekly grocery shopping",
      "merchant": "SM Supermarket",
      "paymentMethod": "CARD",
      "isRecurring": true,
      "createdAt": "2025-10-05T14:30:00Z",
      "updatedAt": "2025-10-05T14:30:00Z"
    },
    {
      "id": "exp124",
      "userId": "user123",
      "description": "Puregold groceries",
      "amount": 2800.00,
      "category": "GROCERIES",
      "currency": "USD",
      "expenseDate": "2025-10-12T00:00:00Z",
      "notes": null,
      "merchant": "Puregold",
      "paymentMethod": "CASH",
      "isRecurring": true,
      "createdAt": "2025-10-12T16:45:00Z",
      "updatedAt": "2025-10-12T16:45:00Z"
    }
  ]
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token

---

### 2. Get Variable Expense by ID

**Endpoint:** `GET /api/VariableExpenses/{id}`

**Description:** Get a specific variable expense by ID.

**Authorization:** Required (User or Admin)

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Expense ID |

**Example Request:**
```http
GET /api/VariableExpenses/exp123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Variable expense retrieved successfully",
  "data": {
    "id": "exp123",
    "userId": "user123",
    "description": "SM Supermarket",
    "amount": 3200.00,
    "category": "GROCERIES",
    "currency": "USD",
    "expenseDate": "2025-10-05T00:00:00Z",
    "notes": "Weekly grocery shopping",
    "merchant": "SM Supermarket",
    "paymentMethod": "CARD",
    "isRecurring": true,
    "createdAt": "2025-10-05T14:30:00Z",
    "updatedAt": "2025-10-05T14:30:00Z"
  }
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Expense not found

---

### 3. Create Variable Expense

**Endpoint:** `POST /api/VariableExpenses`

**Description:** Create a new variable expense.

**Authorization:** Required (User or Admin)

**Request Body:**
```json
{
  "description": "Jollibee lunch",
  "amount": 250.00,
  "category": "FOOD",
  "currency": "USD",
  "expenseDate": "2025-10-11T12:30:00Z",
  "notes": "Quick lunch during work",
  "merchant": "Jollibee",
  "paymentMethod": "CARD",
  "isRecurring": false
}
```

**Validation Rules:**
| Field | Required | Rules |
|-------|----------|-------|
| `description` | Yes | Max 200 characters |
| `amount` | Yes | Must be > 0, decimal(18,2) |
| `category` | Yes | Valid category value |
| `currency` | No | Default: "USD", max 10 characters |
| `expenseDate` | Yes | Valid datetime |
| `notes` | No | Max 500 characters |
| `merchant` | No | Max 200 characters |
| `paymentMethod` | No | Max 50 characters |
| `isRecurring` | No | Default: false |

**Available Categories:**
```
FOOD, GROCERIES, RESTAURANTS, COFFEE, FAST_FOOD,
TRANSPORTATION, GAS, PUBLIC_TRANSPORT, TAXI, RIDESHARE, PARKING, CAR_MAINTENANCE,
ENTERTAINMENT, MOVIES, GAMES, SPORTS, HOBBIES,
SHOPPING, CLOTHING, ELECTRONICS, BOOKS, PERSONAL_CARE,
HEALTHCARE, MEDICINE, FITNESS, DOCTOR,
EDUCATION, COURSES, BOOKS_EDUCATION,
TRAVEL, HOTEL, FLIGHTS, VACATION,
OTHER, GIFTS, DONATIONS, FEES, SUBSCRIPTIONS
```

**Example Response:**
```json
{
  "success": true,
  "message": "Variable expense created successfully",
  "data": {
    "id": "exp125",
    "userId": "user123",
    "description": "Jollibee lunch",
    "amount": 250.00,
    "category": "FOOD",
    "currency": "USD",
    "expenseDate": "2025-10-11T12:30:00Z",
    "notes": "Quick lunch during work",
    "merchant": "Jollibee",
    "paymentMethod": "CARD",
    "isRecurring": false,
    "createdAt": "2025-10-11T12:35:00Z",
    "updatedAt": "2025-10-11T12:35:00Z"
  }
}
```

**Status Codes:**
- `201 Created` - Success
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Missing or invalid token

---

### 4. Update Variable Expense

**Endpoint:** `PUT /api/VariableExpenses/{id}`

**Description:** Update an existing variable expense.

**Authorization:** Required (User or Admin)

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Expense ID |

**Request Body:**
```json
{
  "description": "Jollibee lunch - updated",
  "amount": 280.00,
  "category": "FOOD",
  "currency": "USD",
  "expenseDate": "2025-10-11T12:30:00Z",
  "notes": "Added dessert",
  "merchant": "Jollibee",
  "paymentMethod": "CARD",
  "isRecurring": false
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Variable expense updated successfully",
  "data": {
    "id": "exp125",
    "userId": "user123",
    "description": "Jollibee lunch - updated",
    "amount": 280.00,
    "category": "FOOD",
    "currency": "USD",
    "expenseDate": "2025-10-11T12:30:00Z",
    "notes": "Added dessert",
    "merchant": "Jollibee",
    "paymentMethod": "CARD",
    "isRecurring": false,
    "createdAt": "2025-10-11T12:35:00Z",
    "updatedAt": "2025-10-11T14:20:00Z"
  }
}
```

**Status Codes:**
- `200 OK` - Success
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Expense not found

---

### 5. Delete Variable Expense

**Endpoint:** `DELETE /api/VariableExpenses/{id}`

**Description:** Delete a variable expense.

**Authorization:** Required (User or Admin)

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Expense ID |

**Example Request:**
```http
DELETE /api/VariableExpenses/exp125
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Variable expense deleted successfully",
  "data": {
    "message": "Variable expense deleted successfully"
  }
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - Expense not found

---

### 6. Get Expense Statistics by Category

**Endpoint:** `GET /api/VariableExpenses/statistics/by-category`

**Description:** Get aggregated statistics of variable expenses grouped by category.

**Authorization:** Required (User or Admin)

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `startDate` | datetime | No | Filter start date (defaults to current month start) |
| `endDate` | datetime | No | Filter end date (defaults to current date) |

**Example Request:**
```http
GET /api/VariableExpenses/statistics/by-category?startDate=2025-10-01&endDate=2025-10-31
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Example Response:**
```json
{
  "success": true,
  "message": "Expense statistics retrieved successfully",
  "data": [
    {
      "category": "GROCERIES",
      "totalAmount": 8500.00,
      "count": 12,
      "averageAmount": 708.33,
      "percentage": 44.74
    },
    {
      "category": "TRANSPORTATION",
      "totalAmount": 4200.00,
      "count": 8,
      "averageAmount": 525.00,
      "percentage": 22.11
    },
    {
      "category": "FOOD",
      "totalAmount": 3800.00,
      "count": 15,
      "averageAmount": 253.33,
      "percentage": 20.00
    },
    {
      "category": "ENTERTAINMENT",
      "totalAmount": 1500.00,
      "count": 5,
      "averageAmount": 300.00,
      "percentage": 7.89
    },
    {
      "category": "SHOPPING",
      "totalAmount": 1000.00,
      "count": 3,
      "averageAmount": 333.33,
      "percentage": 5.26
    }
  ]
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid token

---

## 📋 Common Response Structures

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* ... response data ... */ }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "errors": [
    "Detailed error message 1",
    "Detailed error message 2"
  ]
}
```

### Validation Error Response
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "amount": ["Amount must be greater than 0"],
    "category": ["Category is required"]
  }
}
```

---

## ⚠️ Error Handling

### Common HTTP Status Codes

| Code | Description | Common Causes |
|------|-------------|---------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid parameters, validation errors |
| 401 | Unauthorized | Missing or invalid JWT token |
| 403 | Forbidden | Insufficient permissions (e.g., not admin) |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Server-side error |

### Error Response Examples

**400 Bad Request:**
```json
{
  "success": false,
  "message": "Start date must be before end date",
  "errors": ["Start date: 2025-10-31, End date: 2025-10-01"]
}
```

**401 Unauthorized:**
```json
{
  "success": false,
  "message": "User not authenticated",
  "errors": ["Authorization header missing or invalid"]
}
```

**403 Forbidden:**
```json
{
  "success": false,
  "message": "Forbidden",
  "errors": ["This action requires admin privileges"]
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "Variable expense not found",
  "errors": ["No expense found with ID: exp999"]
}
```

---

## 💻 Code Examples

### JavaScript/TypeScript (Axios)

```typescript
import axios from 'axios';

const API_BASE_URL = 'https://your-domain.com/api';
const token = 'your-jwt-token';

// Create axios instance with auth
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

// Get current month disposable amount
async function getCurrentDisposableAmount(targetSavings?: number, investmentAllocation?: number) {
  try {
    const response = await api.get('/Dashboard/disposable-amount/current', {
      params: { targetSavings, investmentAllocation }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching disposable amount:', error);
    throw error;
  }
}

// Get financial summary
async function getFinancialSummary() {
  try {
    const response = await api.get('/Dashboard/financial-summary');
    return response.data;
  } catch (error) {
    console.error('Error fetching financial summary:', error);
    throw error;
  }
}

// Create variable expense
async function createExpense(expense: VariableExpenseDto) {
  try {
    const response = await api.post('/VariableExpenses', expense);
    return response.data;
  } catch (error) {
    console.error('Error creating expense:', error);
    throw error;
  }
}

// Get expense statistics
async function getExpenseStatistics(startDate?: Date, endDate?: Date) {
  try {
    const response = await api.get('/VariableExpenses/statistics/by-category', {
      params: { 
        startDate: startDate?.toISOString(),
        endDate: endDate?.toISOString()
      }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching statistics:', error);
    throw error;
  }
}

// Usage Examples
const disposable = await getCurrentDisposableAmount(5000, 3000);
console.log('Disposable Amount:', disposable.data.disposableAmount);

const summary = await getFinancialSummary();
console.log('Monthly Income:', summary.data.currentMonth.totalIncome);

const newExpense = await createExpense({
  description: 'Grocery shopping',
  amount: 2500,
  category: 'GROCERIES',
  expenseDate: new Date().toISOString()
});
console.log('Created expense:', newExpense.data.id);
```

### C# (.NET)

```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

public class FinancialDashboardClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://your-domain.com/api";

    public FinancialDashboardClient(string jwtToken)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", jwtToken);
    }

    public async Task<DisposableAmountDto> GetCurrentDisposableAmountAsync(
        decimal? targetSavings = null, 
        decimal? investmentAllocation = null)
    {
        var queryString = $"?targetSavings={targetSavings}&investmentAllocation={investmentAllocation}";
        var response = await _httpClient.GetAsync($"/Dashboard/disposable-amount/current{queryString}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<DisposableAmountDto>>(content);
        return result.Data;
    }

    public async Task<FinancialSummaryDto> GetFinancialSummaryAsync()
    {
        var response = await _httpClient.GetAsync("/Dashboard/financial-summary");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<FinancialSummaryDto>>(content);
        return result.Data;
    }

    public async Task<VariableExpenseDto> CreateVariableExpenseAsync(VariableExpenseDto expense)
    {
        var json = JsonSerializer.Serialize(expense);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/VariableExpenses", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<VariableExpenseDto>>(responseContent);
        return result.Data;
    }
}

// Usage
var client = new FinancialDashboardClient("your-jwt-token");

var disposable = await client.GetCurrentDisposableAmountAsync(5000, 3000);
Console.WriteLine($"Disposable Amount: {disposable.DisposableAmount:C}");

var summary = await client.GetFinancialSummaryAsync();
Console.WriteLine($"Monthly Income: {summary.CurrentMonth.TotalIncome:C}");
```

### Python (requests)

```python
import requests
from datetime import datetime
from typing import Optional, Dict, Any

class FinancialDashboardClient:
    def __init__(self, base_url: str, jwt_token: str):
        self.base_url = base_url
        self.headers = {
            'Authorization': f'Bearer {jwt_token}',
            'Content-Type': 'application/json'
        }
    
    def get_current_disposable_amount(
        self, 
        target_savings: Optional[float] = None,
        investment_allocation: Optional[float] = None
    ) -> Dict[str, Any]:
        params = {}
        if target_savings:
            params['targetSavings'] = target_savings
        if investment_allocation:
            params['investmentAllocation'] = investment_allocation
        
        response = requests.get(
            f'{self.base_url}/Dashboard/disposable-amount/current',
            headers=self.headers,
            params=params
        )
        response.raise_for_status()
        return response.json()['data']
    
    def get_financial_summary(self) -> Dict[str, Any]:
        response = requests.get(
            f'{self.base_url}/Dashboard/financial-summary',
            headers=self.headers
        )
        response.raise_for_status()
        return response.json()['data']
    
    def create_variable_expense(self, expense: Dict[str, Any]) -> Dict[str, Any]:
        response = requests.post(
            f'{self.base_url}/VariableExpenses',
            headers=self.headers,
            json=expense
        )
        response.raise_for_status()
        return response.json()['data']
    
    def get_expense_statistics(
        self,
        start_date: Optional[datetime] = None,
        end_date: Optional[datetime] = None
    ) -> list:
        params = {}
        if start_date:
            params['startDate'] = start_date.isoformat()
        if end_date:
            params['endDate'] = end_date.isoformat()
        
        response = requests.get(
            f'{self.base_url}/VariableExpenses/statistics/by-category',
            headers=self.headers,
            params=params
        )
        response.raise_for_status()
        return response.json()['data']

# Usage
client = FinancialDashboardClient(
    'https://your-domain.com/api',
    'your-jwt-token'
)

disposable = client.get_current_disposable_amount(5000, 3000)
print(f"Disposable Amount: ${disposable['disposableAmount']:,.2f}")

summary = client.get_financial_summary()
print(f"Monthly Income: ${summary['currentMonth']['totalIncome']:,.2f}")

new_expense = client.create_variable_expense({
    'description': 'Grocery shopping',
    'amount': 2500.00,
    'category': 'GROCERIES',
    'expenseDate': datetime.now().isoformat()
})
print(f"Created expense ID: {new_expense['id']}")
```

---

## 🔐 Authentication

All endpoints require a valid JWT token in the Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Getting a Token

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "your-password"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "user123",
    "name": "John Doe",
    "role": "USER",
    "expiresAt": "2025-10-12T15:30:00Z"
  }
}
```

---

## 📊 Rate Limiting

Currently no rate limiting is implemented, but it's recommended for production:

**Suggested Limits:**
- Dashboard endpoints: 60 requests/minute
- Variable expense CRUD: 120 requests/minute
- Statistics endpoints: 30 requests/minute

---

## 🌐 CORS

The API supports CORS with the following allowed origins:
- `http://localhost:3000`
- `https://localhost:3000`
- `http://localhost:5000`
- `https://localhost:5000`
- Production domain (configure in appsettings)

---

## 📝 Notes

- All monetary amounts are in decimal(18,2) format
- All dates are in ISO 8601 format (UTC)
- Currency is stored but currently only used for display
- User can only access their own data unless admin
- All responses include `success`, `message`, and `data` fields
- Validation errors include detailed field-level messages

---

**API Version:** 1.0.0  
**Last Updated:** October 11, 2025  
**Status:** Production Ready ✅

