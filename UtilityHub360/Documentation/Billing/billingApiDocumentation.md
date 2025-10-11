# Billing Management API Documentation

## 📋 Overview

The Billing Management API allows users to manage recurring bills including utilities, subscriptions, loans, and other recurring expenses. The system provides comprehensive CRUD operations, analytics, and bill management features.

## 🔐 Authentication

All endpoints require JWT Bearer token authentication:

```
Authorization: Bearer <jwt_token>
```

## 📊 Bill Types & Statuses

### Bill Types
- `utility` - Electricity, Water, Gas, Internet, etc.
- `subscription` - Netflix, Spotify, Gym membership, etc.
- `loan` - Loan repayments
- `others` - Any other recurring bills

### Bill Statuses
- `PENDING` - Bill created, not yet paid
- `PAID` - Bill has been paid
- `OVERDUE` - Bill past due date (automatically calculated)

### Frequencies
- `monthly` - Monthly bills
- `quarterly` - Every 3 months
- `yearly` - Annual bills

---

## 🚀 API Endpoints

### 1. Create Bill

**Endpoint:** `POST /api/bills`

**Description:** Create a new recurring bill

**Request Body:**
```json
{
  "billName": "Electricity Bill",
  "billType": "utility",
  "amount": 150.00,
  "dueDate": "2025-10-15T00:00:00Z",
  "frequency": "monthly",
  "notes": "Monthly electricity bill",
  "provider": "Electric Company",
  "referenceNumber": "ACC123456"
}
```

**Request Body Validation:**
| Field | Type | Required | Validation Rules | Example |
|-------|------|----------|------------------|---------|
| billName | string | ✅ | 1-255 characters | "Electricity Bill" |
| billType | string | ✅ | Must be: utility, subscription, loan, others | "utility" |
| amount | decimal | ✅ | Must be > 0, max 2 decimal places | 150.00 |
| dueDate | datetime | ✅ | Must be valid ISO 8601 datetime | "2025-10-15T00:00:00Z" |
| frequency | string | ✅ | Must be: monthly, quarterly, yearly | "monthly" |
| notes | string | ❌ | Max 500 characters | "Monthly bill" |
| provider | string | ❌ | Max 100 characters | "Electric Company" |
| referenceNumber | string | ❌ | Max 100 characters | "ACC123456" |

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Bill created successfully",
  "data": {
    "id": "bill-123",
    "userId": "user-456",
    "billName": "Electricity Bill",
    "billType": "utility",
    "amount": 150.00,
    "dueDate": "2025-10-15T00:00:00Z",
    "frequency": "monthly",
    "status": "PENDING",
    "createdAt": "2025-09-24T20:35:00Z",
    "updatedAt": "2025-09-24T20:35:00Z",
    "paidAt": null,
    "notes": "Monthly electricity bill",
    "provider": "Electric Company",
    "referenceNumber": "ACC123456"
  },
  "errors": []
}
```

**Response (Error - 400):**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Amount must be greater than 0",
    "Bill type must be one of: utility, subscription, loan, others"
  ]
}
```

---

### 2. Get Bill

**Endpoint:** `GET /api/bills/{billId}`

**Description:** Retrieve a specific bill by ID

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billId | string | ✅ | Unique bill identifier |

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": "bill-123",
    "userId": "user-456",
    "billName": "Electricity Bill",
    "billType": "utility",
    "amount": 150.00,
    "dueDate": "2025-10-15T00:00:00Z",
    "frequency": "monthly",
    "status": "PENDING",
    "createdAt": "2025-09-24T20:35:00Z",
    "updatedAt": "2025-09-24T20:35:00Z",
    "paidAt": null,
    "notes": "Monthly electricity bill",
    "provider": "Electric Company",
    "referenceNumber": "ACC123456"
  },
  "errors": []
}
```

**Response (Error - 404):**
```json
{
  "success": false,
  "message": "Bill not found",
  "data": null,
  "errors": []
}
```

---

### 3. Get User Bills

**Endpoint:** `GET /api/bills`

**Description:** Retrieve all bills for the authenticated user with optional filtering and pagination

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| status | string | ❌ | null | Filter by status (PENDING, PAID, OVERDUE) |
| billType | string | ❌ | null | Filter by bill type |
| page | integer | ❌ | 1 | Page number for pagination |
| limit | integer | ❌ | 10 | Items per page (max 100) |

**Example Request:**
```
GET /api/bills?status=PENDING&billType=utility&page=1&limit=10
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "data": [
      {
        "id": "bill-123",
        "userId": "user-456",
        "billName": "Electricity Bill",
        "billType": "utility",
        "amount": 150.00,
        "dueDate": "2025-10-15T00:00:00Z",
        "frequency": "monthly",
        "status": "PENDING",
        "createdAt": "2025-09-24T20:35:00Z",
        "updatedAt": "2025-09-24T20:35:00Z",
        "paidAt": null,
        "notes": "Monthly electricity bill",
        "provider": "Electric Company",
        "referenceNumber": "ACC123456"
      }
    ],
    "page": 1,
    "limit": 10,
    "totalCount": 1
  },
  "errors": []
}
```

---

### 4. Update Bill

**Endpoint:** `PUT /api/bills/{billId}`

**Description:** Update an existing bill

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billId | string | ✅ | Unique bill identifier |

**Request Body:**
```json
{
  "billName": "Updated Electricity Bill",
  "amount": 175.00,
  "dueDate": "2025-11-15T00:00:00Z",
  "status": "PAID",
  "notes": "Updated amount due to increased usage"
}
```

**Request Body Validation:**
| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| billName | string | ❌ | 1-255 characters |
| billType | string | ❌ | Must be: utility, subscription, loan, others |
| amount | decimal | ❌ | Must be > 0, max 2 decimal places |
| dueDate | datetime | ❌ | Must be valid ISO 8601 datetime |
| frequency | string | ❌ | Must be: monthly, quarterly, yearly |
| status | string | ❌ | Must be: PENDING, PAID, OVERDUE |
| notes | string | ❌ | Max 500 characters |
| provider | string | ❌ | Max 100 characters |
| referenceNumber | string | ❌ | Max 100 characters |

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Bill updated successfully",
  "data": {
    "id": "bill-123",
    "userId": "user-456",
    "billName": "Updated Electricity Bill",
    "billType": "utility",
    "amount": 175.00,
    "dueDate": "2025-11-15T00:00:00Z",
    "frequency": "monthly",
    "status": "PAID",
    "createdAt": "2025-09-24T20:35:00Z",
    "updatedAt": "2025-09-24T21:00:00Z",
    "paidAt": "2025-09-24T21:00:00Z",
    "notes": "Updated amount due to increased usage",
    "provider": "Electric Company",
    "referenceNumber": "ACC123456"
  },
  "errors": []
}
```

---

### 5. Delete Bill

**Endpoint:** `DELETE /api/bills/{billId}`

**Description:** Delete a bill

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billId | string | ✅ | Unique bill identifier |

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Bill deleted successfully",
  "data": true,
  "errors": []
}
```

---

## 📊 Analytics Endpoints

### 6. Get Total Pending Amount

**Endpoint:** `GET /api/bills/analytics/total-pending`

**Description:** Get total amount of all pending bills

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": 1250.50,
  "errors": []
}
```

---

### 7. Get Total Paid Amount

**Endpoint:** `GET /api/bills/analytics/total-paid`

**Description:** Get total amount paid in a specific period

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| period | string | ❌ | month | Period: week, month, quarter, year |

**Example Request:**
```
GET /api/bills/analytics/total-paid?period=month
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "amount": 850.00,
    "count": 5,
    "period": "MONTH",
    "startDate": "2025-08-24T00:00:00Z",
    "endDate": "2025-09-24T00:00:00Z"
  },
  "errors": []
}
```

---

### 8. Get Total Overdue Amount

**Endpoint:** `GET /api/bills/analytics/total-overdue`

**Description:** Get total amount of all overdue bills

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": 300.00,
  "errors": []
}
```

---

### 9. Get Bill Analytics Summary

**Endpoint:** `GET /api/bills/analytics/summary`

**Description:** Get comprehensive analytics summary

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "totalPendingAmount": 1250.50,
    "totalPaidAmount": 2100.00,
    "totalOverdueAmount": 300.00,
    "totalPendingBills": 8,
    "totalPaidBills": 15,
    "totalOverdueBills": 2,
    "generatedAt": "2025-09-24T21:00:00Z"
  },
  "errors": []
}
```

---

## 🔧 Management Endpoints

### 10. Mark Bill as Paid

**Endpoint:** `PUT /api/bills/{billId}/mark-paid`

**Description:** Mark a bill as paid with optional notes

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billId | string | ✅ | Unique bill identifier |

**Request Body:**
```json
"Payment completed via bank transfer"
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Bill marked as paid",
  "data": {
    "id": "bill-123",
    "status": "PAID",
    "paidAt": "2025-09-24T21:00:00Z",
    "notes": "Payment completed via bank transfer"
  },
  "errors": []
}
```

---

### 11. Update Bill Status

**Endpoint:** `PUT /api/bills/{billId}/status`

**Description:** Update bill status

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billId | string | ✅ | Unique bill identifier |

**Request Body:**
```json
"PAID"
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Bill status updated successfully",
  "data": true,
  "errors": []
}
```

---

### 12. Get Overdue Bills

**Endpoint:** `GET /api/bills/overdue`

**Description:** Get all overdue bills

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": "bill-456",
      "billName": "Water Bill",
      "amount": 150.00,
      "dueDate": "2025-09-20T00:00:00Z",
      "status": "PENDING",
      "provider": "Water Company"
    }
  ],
  "errors": []
}
```

---

### 13. Get Upcoming Bills

**Endpoint:** `GET /api/bills/upcoming`

**Description:** Get bills due within specified days

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| days | integer | ❌ | 7 | Number of days to look ahead |

**Example Request:**
```
GET /api/bills/upcoming?days=14
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": "bill-789",
      "billName": "Internet Bill",
      "amount": 75.00,
      "dueDate": "2025-10-01T00:00:00Z",
      "status": "PENDING",
      "provider": "ISP Company"
    }
  ],
  "errors": []
}
```

---

## 👨‍💼 Admin Endpoints

### 14. Get All Bills (Admin Only)

**Endpoint:** `GET /api/bills/admin/all`

**Description:** Get all bills across all users (Admin access required)

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| status | string | ❌ | null | Filter by status |
| billType | string | ❌ | null | Filter by bill type |
| page | integer | ❌ | 1 | Page number |
| limit | integer | ❌ | 10 | Items per page |

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "data": [
      {
        "id": "bill-123",
        "userId": "user-456",
        "billName": "Electricity Bill",
        "billType": "utility",
        "amount": 150.00,
        "dueDate": "2025-10-15T00:00:00Z",
        "frequency": "monthly",
        "status": "PENDING",
        "createdAt": "2025-09-24T20:35:00Z",
        "updatedAt": "2025-09-24T20:35:00Z",
        "paidAt": null,
        "notes": "Monthly electricity bill",
        "provider": "Electric Company",
        "referenceNumber": "ACC123456"
      }
    ],
    "page": 1,
    "limit": 10,
    "totalCount": 25
  },
  "errors": []
}
```

---

## 🚨 Error Responses

### Common Error Codes

**401 Unauthorized:**
```json
{
  "success": false,
  "message": "User not authenticated",
  "data": null,
  "errors": []
}
```

**403 Forbidden:**
```json
{
  "success": false,
  "message": "Access denied. Admin role required.",
  "data": null,
  "errors": []
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "Bill not found",
  "data": null,
  "errors": []
}
```

**500 Internal Server Error:**
```json
{
  "success": false,
  "message": "An unexpected error occurred",
  "data": null,
  "errors": []
}
```

---

## 📝 Frontend Implementation Notes

### Form Validation Rules

1. **Bill Name**: Required, 1-255 characters
2. **Bill Type**: Required, must be one of the predefined values
3. **Amount**: Required, must be positive number with max 2 decimal places
4. **Due Date**: Required, must be future date
5. **Frequency**: Required, must be one of the predefined values
6. **Notes**: Optional, max 500 characters
7. **Provider**: Optional, max 100 characters
8. **Reference Number**: Optional, max 100 characters

### Date Format
All dates should be in ISO 8601 format: `YYYY-MM-DDTHH:mm:ssZ`

### Pagination
Use page and limit parameters for paginated endpoints. Maximum limit is 100.

### Error Handling
Always check the `success` field in responses and handle errors appropriately. Display user-friendly error messages from the `errors` array.

### Authentication
Include the JWT token in the Authorization header for all requests:
```
Authorization: Bearer <token>
```

---

## 🔄 Typical User Flows

### 1. Create New Bill
1. User fills out bill creation form
2. Frontend validates form data
3. Send POST request to `/api/bills`
4. Display success message and redirect to bills list

### 2. View Bills Dashboard
1. Load analytics summary from `/api/bills/analytics/summary`
2. Display pending, paid, and overdue amounts
3. Load bills list from `/api/bills`
4. Show bills in table/card format with status indicators

### 3. Mark Bill as Paid
1. User clicks "Mark as Paid" button
2. Optionally show payment notes input
3. Send PUT request to `/api/bills/{billId}/mark-paid`
4. Update UI to show paid status

### 4. Filter Bills
1. User selects filters (status, bill type)
2. Update query parameters
3. Send GET request to `/api/bills` with filters
4. Update bills display

---

## 🎯 Testing Endpoints

Use Swagger UI at `http://localhost:5000/swagger` for interactive testing, or use tools like Postman with the provided examples above.

---

# 📊 Variable Monthly Billing - Advanced Features

## Overview

The Variable Monthly Billing system provides advanced analytics, forecasting, budgeting, and alerting capabilities for bills with variable amounts (like electricity, water, etc.). This helps users predict future expenses, track spending trends, and stay within budget.

---

## 🔮 Analytics & Forecasting Endpoints

### 15. Get Bill History with Analytics

**Endpoint:** `GET /api/bills/analytics/history`

**Description:** Retrieve bill history along with comprehensive analytics and forecast data

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| provider | string | ❌ | null | Filter by provider name |
| billType | string | ❌ | null | Filter by bill type |
| months | integer | ❌ | 6 | Number of months to analyze |

**Example Request:**
```
GET /api/bills/analytics/history?provider=Meralco&billType=utility&months=6
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "bills": [
      {
        "id": "bill-123",
        "billName": "Electricity Bill - October",
        "amount": 3050.00,
        "dueDate": "2025-10-10T00:00:00Z",
        "status": "PENDING",
        "provider": "Meralco",
        "createdAt": "2025-10-01T00:00:00Z"
      }
    ],
    "analytics": {
      "averageSimple": 2903.33,
      "averageWeighted": 2989.00,
      "averageSeasonal": 2903.33,
      "totalSpent": 17190.00,
      "highestBill": 3200.00,
      "lowestBill": 2450.00,
      "trend": "increasing",
      "billCount": 6,
      "firstBillDate": "2025-05-01T00:00:00Z",
      "lastBillDate": "2025-10-01T00:00:00Z"
    },
    "forecast": {
      "estimatedAmount": 2989.00,
      "calculationMethod": "weighted",
      "confidence": "medium",
      "estimatedForMonth": "2025-11-01T00:00:00Z",
      "recommendation": "Based on historical patterns, expect around ₱2,989.00 for next month."
    },
    "totalCount": 6
  },
  "errors": []
}
```

---

### 16. Get Analytics Calculations

**Endpoint:** `GET /api/bills/analytics/calculations`

**Description:** Get detailed analytics calculations without bill history

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| provider | string | ❌ | null | Filter by provider name |
| billType | string | ❌ | null | Filter by bill type |
| months | integer | ❌ | 6 | Number of months to analyze |

**Response (Success - 200):**
```json
{
  "success": true,
  "data": {
    "averageSimple": 2903.33,
    "averageWeighted": 2989.00,
    "averageSeasonal": 2903.33,
    "totalSpent": 17190.00,
    "highestBill": 3200.00,
    "lowestBill": 2450.00,
    "trend": "increasing",
    "billCount": 6,
    "firstBillDate": "2025-05-01T00:00:00Z",
    "lastBillDate": "2025-10-01T00:00:00Z"
  },
  "errors": []
}
```

---

### 17. Get Bill Forecast

**Endpoint:** `GET /api/bills/analytics/forecast`

**Description:** Get forecast for next month's bill based on historical data

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| provider | string | ✅ | - | Provider name |
| billType | string | ✅ | - | Bill type |
| method | string | ❌ | weighted | Calculation method: simple, weighted, seasonal |

**Example Request:**
```
GET /api/bills/analytics/forecast?provider=Meralco&billType=utility&method=weighted
```

**Response (Success - 200):**
```json
{
  "success": true,
  "data": {
    "estimatedAmount": 2989.00,
    "calculationMethod": "weighted",
    "confidence": "medium",
    "estimatedForMonth": "2025-11-01T00:00:00Z",
    "recommendation": "Based on historical patterns, expect around ₱2,989.00 for next month."
  },
  "errors": []
}
```

**Calculation Methods:**
- **simple**: Average of last N months (equal weight)
- **weighted**: Recent months weighted more (50%, 30%, 20%)
- **seasonal**: Average of same month from previous years

**Confidence Levels:**
- **high**: 12+ months of data
- **medium**: 6-11 months of data
- **low**: Less than 6 months of data

---

### 18. Calculate Bill Variance

**Endpoint:** `GET /api/bills/{billId}/variance`

**Description:** Calculate variance between actual bill and estimated amount

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billId | string | ✅ | Unique bill identifier |

**Example Request:**
```
GET /api/bills/bill-123/variance
```

**Response (Success - 200):**
```json
{
  "success": true,
  "data": {
    "billId": "bill-123",
    "actualAmount": 3050.00,
    "estimatedAmount": 2989.00,
    "variance": 61.00,
    "variancePercentage": 2.04,
    "status": "slightly_over",
    "message": "Your bill is slightly higher than expected (+2.04%)",
    "recommendation": "Your bill is slightly above average. Monitor usage to keep costs down."
  },
  "errors": []
}
```

**Variance Status Values:**
- `over_budget`: Variance >= 5%
- `slightly_over`: Variance > 1% and < 5%
- `on_target`: Variance between -1% and +1%
- `under_budget`: Variance < -1%

---

### 19. Get Monthly Trend

**Endpoint:** `GET /api/bills/analytics/trend`

**Description:** Get monthly spending trend data for visualization

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| provider | string | ❌ | null | Filter by provider name |
| billType | string | ❌ | null | Filter by bill type |
| months | integer | ❌ | 12 | Number of months to retrieve |

**Response (Success - 200):**
```json
{
  "success": true,
  "data": [
    {
      "year": 2025,
      "month": 5,
      "monthName": "May 2025",
      "totalAmount": 2450.00,
      "billCount": 1,
      "averageAmount": 2450.00,
      "status": "paid"
    },
    {
      "year": 2025,
      "month": 6,
      "monthName": "Jun 2025",
      "totalAmount": 2980.00,
      "billCount": 1,
      "averageAmount": 2980.00,
      "status": "paid"
    }
  ],
  "errors": []
}
```

---

## 💰 Budget Management Endpoints

### 20. Create Budget

**Endpoint:** `POST /api/bills/budgets`

**Description:** Create a monthly budget for a specific provider and bill type

**Request Body:**
```json
{
  "provider": "Meralco",
  "billType": "utility",
  "monthlyBudget": 3000.00,
  "enableAlerts": true,
  "alertThreshold": 90
}
```

**Request Body Validation:**
| Field | Type | Required | Validation Rules | Description |
|-------|------|----------|------------------|-------------|
| provider | string | ✅ | Max 100 characters | Provider name |
| billType | string | ✅ | Must be: utility, subscription, loan, others | Bill type |
| monthlyBudget | decimal | ✅ | Must be > 0 | Monthly budget amount |
| enableAlerts | boolean | ❌ | Default: true | Enable budget alerts |
| alertThreshold | integer | ❌ | 1-100, Default: 90 | Alert when % of budget reached |

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Budget created successfully",
  "data": {
    "id": "budget-123",
    "userId": "user-456",
    "provider": "Meralco",
    "billType": "utility",
    "monthlyBudget": 3000.00,
    "enableAlerts": true,
    "alertThreshold": 90,
    "createdAt": "2025-10-11T00:00:00Z",
    "updatedAt": "2025-10-11T00:00:00Z"
  },
  "errors": []
}
```

---

### 21. Update Budget

**Endpoint:** `PUT /api/bills/budgets/{budgetId}`

**Description:** Update an existing budget

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| budgetId | string | ✅ | Budget identifier |

**Request Body:** Same as Create Budget

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Budget updated successfully",
  "data": { /* Budget object */ },
  "errors": []
}
```

---

### 22. Delete Budget

**Endpoint:** `DELETE /api/bills/budgets/{budgetId}`

**Description:** Delete a budget

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Budget deleted successfully",
  "data": true,
  "errors": []
}
```

---

### 23. Get Budget

**Endpoint:** `GET /api/bills/budgets/{budgetId}`

**Description:** Get a specific budget by ID

**Response (Success - 200):**
```json
{
  "success": true,
  "data": {
    "id": "budget-123",
    "userId": "user-456",
    "provider": "Meralco",
    "billType": "utility",
    "monthlyBudget": 3000.00,
    "enableAlerts": true,
    "alertThreshold": 90
  },
  "errors": []
}
```

---

### 24. Get All User Budgets

**Endpoint:** `GET /api/bills/budgets`

**Description:** Get all budgets for the authenticated user

**Response (Success - 200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "budget-123",
      "provider": "Meralco",
      "billType": "utility",
      "monthlyBudget": 3000.00
    },
    {
      "id": "budget-456",
      "provider": "Globe",
      "billType": "utility",
      "monthlyBudget": 1500.00
    }
  ],
  "errors": []
}
```

---

### 25. Get Budget Status

**Endpoint:** `GET /api/bills/budgets/status`

**Description:** Get current budget status for a provider/bill type

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| provider | string | ✅ | Provider name |
| billType | string | ✅ | Bill type |

**Example Request:**
```
GET /api/bills/budgets/status?provider=Meralco&billType=utility
```

**Response (Success - 200):**
```json
{
  "success": true,
  "data": {
    "budgetId": "budget-123",
    "provider": "Meralco",
    "billType": "utility",
    "monthlyBudget": 3000.00,
    "currentBill": 3050.00,
    "remaining": -50.00,
    "percentageUsed": 101.7,
    "status": "over_budget",
    "alert": true,
    "message": "You exceeded your budget by ₱50"
  },
  "errors": []
}
```

**Budget Status Values:**
- `on_track`: Usage below alert threshold
- `approaching_limit`: Usage >= alert threshold
- `over_budget`: Usage > 100% of budget

---

## 🔔 Alert Management Endpoints

### 26. Get User Alerts

**Endpoint:** `GET /api/bills/alerts`

**Description:** Get alerts for the authenticated user

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| isRead | boolean | ❌ | null | Filter by read status |
| limit | integer | ❌ | 50 | Maximum number of alerts |

**Example Request:**
```
GET /api/bills/alerts?isRead=false&limit=10
```

**Response (Success - 200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "alert-123",
      "alertType": "due_date",
      "severity": "warning",
      "title": "Payment Reminder",
      "message": "Your Meralco bill of ₱3,050 is due in 3 days (Oct 10)",
      "billId": "bill-123",
      "provider": "Meralco",
      "amount": 3050.00,
      "createdAt": "2025-10-07T00:00:00Z",
      "isRead": false,
      "actionLink": "/bills/bill-123"
    },
    {
      "id": "alert-456",
      "alertType": "budget_exceeded",
      "severity": "error",
      "title": "Budget Alert",
      "message": "You exceeded your budget by ₱50",
      "billId": "bill-123",
      "provider": "Meralco",
      "amount": 3050.00,
      "createdAt": "2025-10-10T00:00:00Z",
      "isRead": false,
      "actionLink": "/bills/bill-123"
    }
  ],
  "errors": []
}
```

**Alert Types:**
- `due_date`: Bill due in 3 days
- `overdue`: Bill past due date
- `budget_exceeded`: Bill exceeds budget
- `trend_increase`: Bills trending upward
- `unusual_spike`: Bill significantly higher than average
- `savings`: Bill significantly lower than average

**Severity Levels:**
- `info`: Informational
- `warning`: Warning
- `error`: Critical issue
- `success`: Positive outcome

---

### 27. Mark Alert as Read

**Endpoint:** `PUT /api/bills/alerts/{alertId}/read`

**Description:** Mark a specific alert as read

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| alertId | string | ✅ | Alert identifier |

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Alert marked as read",
  "data": true,
  "errors": []
}
```

---

### 28. Generate Alerts

**Endpoint:** `POST /api/bills/alerts/generate`

**Description:** Manually trigger alert generation for the authenticated user

**Response (Success - 200):**
```json
{
  "success": true,
  "data": [
    { /* Alert object */ },
    { /* Alert object */ }
  ],
  "errors": []
}
```

**Note:** Alerts are automatically generated every 6 hours by the background service.

---

## 📈 Provider Analytics Endpoints

### 29. Get Provider Analytics

**Endpoint:** `GET /api/bills/analytics/providers`

**Description:** Get analytics for all providers

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| months | integer | ❌ | 6 | Number of months to analyze |

**Response (Success - 200):**
```json
{
  "success": true,
  "data": [
    {
      "provider": "Meralco",
      "billType": "utility",
      "totalSpent": 17190.00,
      "averageMonthly": 2865.00,
      "billCount": 6,
      "highestBill": 3200.00,
      "lowestBill": 2450.00,
      "lastBillDate": "2025-10-01T00:00:00Z",
      "currentBudget": 3000.00,
      "monthlySummary": [
        {
          "year": 2025,
          "month": 5,
          "monthName": "May 2025",
          "totalAmount": 2450.00,
          "billCount": 1,
          "averageAmount": 2450.00,
          "status": "paid"
        }
      ]
    }
  ],
  "errors": []
}
```

---

### 30. Get Provider Analytics by Provider

**Endpoint:** `GET /api/bills/analytics/providers/{provider}`

**Description:** Get detailed analytics for a specific provider

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| provider | string | ✅ | Provider name |

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billType | string | ✅ | Bill type |
| months | integer | ❌ | Number of months (default: 6) |

**Example Request:**
```
GET /api/bills/analytics/providers/Meralco?billType=utility&months=6
```

**Response:** Same as Get Provider Analytics but for single provider

---

## 📊 Dashboard Endpoint

### 31. Get Dashboard Data

**Endpoint:** `GET /api/bills/dashboard`

**Description:** Get comprehensive dashboard data including current bills, upcoming bills, analytics, budgets, and alerts

**Response (Success - 200):**
```json
{
  "success": true,
  "data": {
    "currentBills": [
      { /* Bill object */ }
    ],
    "upcomingBills": [
      { /* Bill object */ }
    ],
    "overdueBills": [
      { /* Bill object */ }
    ],
    "providerAnalytics": [
      { /* Provider analytics object */ }
    ],
    "budgetStatuses": [
      { /* Budget status object */ }
    ],
    "alerts": [
      { /* Alert object */ }
    ],
    "summary": {
      "totalPendingAmount": 1250.50,
      "totalPaidAmount": 2100.00,
      "totalOverdueAmount": 300.00,
      "totalPendingBills": 8,
      "totalPaidBills": 15,
      "totalOverdueBills": 2
    }
  },
  "errors": []
}
```

---

## 🎯 Variable Monthly Billing Use Cases

### Use Case 1: Track Electricity Bills

**Scenario:** User wants to track variable electricity bills and get forecasts

**Steps:**
1. Create bills each month with actual amounts
2. Get analytics to see averages and trends
3. Get forecast for next month's bill
4. Set a budget and receive alerts

**Example Flow:**
```http
# 1. Create monthly bills
POST /api/bills
{
  "billName": "Electricity - October",
  "billType": "utility",
  "provider": "Meralco",
  "amount": 3050.00,
  "dueDate": "2025-10-10T00:00:00Z",
  "frequency": "monthly"
}

# 2. Get analytics
GET /api/bills/analytics/history?provider=Meralco&billType=utility&months=6

# 3. Get forecast
GET /api/bills/analytics/forecast?provider=Meralco&billType=utility&method=weighted

# 4. Set budget
POST /api/bills/budgets
{
  "provider": "Meralco",
  "billType": "utility",
  "monthlyBudget": 3000.00,
  "enableAlerts": true
}

# 5. Check budget status
GET /api/bills/budgets/status?provider=Meralco&billType=utility
```

---

### Use Case 2: Monitor All Utility Bills

**Scenario:** User wants to see overall utility spending across all providers

**Steps:**
```http
# 1. Get dashboard with all data
GET /api/bills/dashboard

# 2. Get provider analytics
GET /api/bills/analytics/providers?months=6

# 3. Get monthly trends
GET /api/bills/analytics/trend?billType=utility&months=12

# 4. Check alerts
GET /api/bills/alerts?isRead=false
```

---

### Use Case 3: Budget Management

**Scenario:** User wants to set budgets and track spending

**Steps:**
```http
# 1. Create budgets for different providers
POST /api/bills/budgets
{
  "provider": "Meralco",
  "billType": "utility",
  "monthlyBudget": 3000.00
}

POST /api/bills/budgets
{
  "provider": "Globe",
  "billType": "utility",
  "monthlyBudget": 1500.00
}

# 2. Get all budget statuses
GET /api/bills/budgets

# 3. Check specific budget status
GET /api/bills/budgets/status?provider=Meralco&billType=utility

# 4. View budget alerts
GET /api/bills/alerts?isRead=false
```

---

## 🔄 Background Services

### Bill Reminder Background Service

**Description:** Automated service that runs every 6 hours to generate alerts

**Features:**
- Generates due date reminders (3 days before)
- Creates overdue alerts
- Checks budget violations
- Detects unusual spending spikes

**Schedule:** Every 6 hours

**Manual Trigger:** Use `POST /api/bills/alerts/generate` to manually trigger alert generation

---

## 📝 Best Practices

### 1. Bill Entry
- Enter bills consistently each month
- Use consistent provider names
- Include accurate due dates

### 2. Budget Setting
- Set realistic budgets based on historical data
- Use analytics to determine appropriate budget amounts
- Enable alerts to stay informed

### 3. Alert Management
- Review alerts regularly
- Mark alerts as read after taking action
- Adjust budgets based on alert patterns

### 4. Forecasting
- Wait for at least 3 months of data for reliable forecasts
- Use weighted average for variable bills
- Use seasonal average for bills with yearly patterns

---

## 🚀 12-Month Auto-Generation (NEW in v2.2.0)

### GET /api/bills/monthly

**Description:** Retrieve all bills for a specific month

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| year | integer | ✅ | Year (2000-2100) |
| month | integer | ✅ | Month (1-12) |
| provider | string | ❌ | Filter by provider name |
| billType | string | ❌ | Filter by bill type |

**Request Example:**
```http
GET /api/bills/monthly?year=2025&month=11&provider=Meralco&billType=utility
Authorization: Bearer <jwt_token>
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Found 3 bill(s) for November 2025",
  "data": [
    {
      "id": "bill-001",
      "billName": "Electricity Bill",
      "provider": "Meralco",
      "billType": "utility",
      "amount": 3050.00,
      "dueDate": "2025-11-10T00:00:00Z",
      "status": "PENDING",
      "isAutoGenerated": true,
      "notes": "Auto-generated for November 2025"
    },
    {
      "id": "bill-002",
      "billName": "Water Bill",
      "provider": "Manila Water",
      "billType": "utility",
      "amount": 450.00,
      "dueDate": "2025-11-15T00:00:00Z",
      "status": "PENDING",
      "isAutoGenerated": true
    }
  ]
}
```

**Use Cases:**
- Get all bills for a specific month for review
- Filter bills by provider or type for monthly reporting
- Check auto-generated bills before confirmation
- Plan monthly budget by reviewing all upcoming bills

---

### PUT /api/bills/{billId}/monthly

**Description:** Quick update for a specific month's bill (amount, notes, status)

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| billId | string | ✅ | ID of the bill to update |

**Request Body:**
```json
{
  "amount": 3200.00,
  "notes": "Updated to actual November amount",
  "status": "PENDING"
}
```

**Request Body Validation:**
| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| amount | decimal | ✅ | Must be > 0, max 2 decimal places |
| notes | string | ❌ | Max 500 characters |
| status | string | ❌ | Must be: PENDING, PAID |

**Request Example:**
```http
PUT /api/bills/bill-001/monthly
Authorization: Bearer <jwt_token>
Content-Type: application/json

{
  "amount": 3200.00,
  "notes": "Updated to actual November amount based on bill received",
  "status": "PENDING"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "Bill for November 2025 updated successfully",
  "data": {
    "id": "bill-001",
    "billName": "Electricity Bill",
    "provider": "Meralco",
    "billType": "utility",
    "amount": 3200.00,
    "dueDate": "2025-11-10T00:00:00Z",
    "status": "PENDING",
    "notes": "Updated to actual November amount based on bill received",
    "updatedAt": "2025-11-05T10:30:00Z"
  }
}
```

**Use Cases:**
- Update monthly bill amount when actual bill arrives
- Add notes about specific month's usage
- Mark bill as paid after payment
- Adjust estimated amounts from auto-generation

---

## 📅 12-Month Auto-Generation Workflow

### Creating Bills for Entire Year

**Scenario:** User wants to create bills for all 12 months at once

**Step 1: Enable Auto-Generation**
```http
POST /api/bills
Authorization: Bearer <jwt_token>
Content-Type: application/json

{
  "billName": "Electricity Bill",
  "billType": "utility",
  "provider": "Meralco",
  "amount": 3050.00,
  "dueDate": "2025-10-10T00:00:00Z",
  "frequency": "monthly",
  "referenceNumber": "ACC123456",
  "autoGenerateNext": true
}
```

**Result:** System creates 12 bills (one for each month) automatically!
- October 2025: ₱3,050 (original)
- November 2025: ₱3,050 (auto-generated)
- December 2025: ₱3,050 (auto-generated)
- ... all months through September 2026

**Step 2: Review Monthly Bills**
```http
GET /api/bills/monthly?year=2025&month=11
```

**Step 3: Update Individual Months**
```http
PUT /api/bills/{billId}/monthly
{
  "amount": 3200.00,
  "notes": "Actual November amount"
}
```

**Benefits:**
- ✅ **Instant Planning** - See entire year of bills immediately
- ✅ **95% Time Saved** - 2 minutes vs 24 minutes manual entry
- ✅ **No Missed Bills** - All 12 months created at once
- ✅ **Flexible Updates** - Modify any month independently

---

## 🎯 Updated Testing Endpoints

Use Swagger UI at `http://localhost:5000/swagger` for interactive testing.

**Recommended Testing Flow:**
1. Create a bill with `autoGenerateNext: true` to test 12-month generation
2. Use `GET /api/bills/monthly` to retrieve bills by month
3. Update individual months with `PUT /api/bills/{billId}/monthly`
4. Create several bills for the same provider (different months)
5. Test analytics endpoints to verify calculations
6. Create a budget and verify status calculations
7. Test alert generation
8. Use dashboard endpoint to see integrated data