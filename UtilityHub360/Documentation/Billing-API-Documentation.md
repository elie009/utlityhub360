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
