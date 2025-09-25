# API Endpoints Reference

## 🔐 Authentication Endpoints

### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "phone": "+1234567890"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "id": "user-id",
      "name": "John Doe",
      "email": "john@example.com",
      "role": "USER"
    }
  }
}
```

### POST /api/auth/login
Authenticate user and get JWT token.

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

### GET /api/auth/me
Get current user information (requires authentication).

### POST /api/auth/refresh
Refresh JWT token using refresh token.

### POST /api/auth/logout
Logout user (requires authentication).

---

## 💰 Loan Management Endpoints

### POST /api/loans/apply
Apply for a new loan (requires authentication).

**Request Body:**
```json
{
  "principal": 50000,
  "term": 24,
  "purpose": "Home improvement",
  "additionalInfo": "Renovating kitchen"
}
```

### GET /api/loans/{loanId}
Get specific loan details (requires authentication).

**Parameters:**
- `loanId`: Loan ID (string)

### GET /api/loans/user/{userId}
Get all loans for a specific user (requires authentication).

**Parameters:**
- `userId`: User ID (string)
- `status`: Optional filter by status
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

### GET /api/loans
Get all loans (Admin only).

**Query Parameters:**
- `status`: Optional filter by status
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

### PUT /api/loans/{loanId}
Update loan information (requires authentication).

**Request Body:**
```json
{
  "purpose": "Updated purpose",
  "additionalInfo": "Updated information",
  "status": "APPROVED"
}
```

**Note:** 
- All users can update: `purpose`, `additionalInfo`, `status`
- Only admins can update: `interestRate`, `monthlyPayment`, `remainingBalance`

### GET /api/loans/{loanId}/status
Get loan status (requires authentication).

### GET /api/loans/{loanId}/repayment-schedule
Get repayment schedule for a loan (requires authentication).

### GET /api/loans/{loanId}/transactions
Get transaction history for a loan (requires authentication).

---

## 👥 User Management Endpoints

### GET /api/users/{userId}
Get user profile (requires authentication).

### GET /api/users/{userId}/loans
Get all loans for a user (requires authentication).

---

## 💳 Payment Endpoints

### POST /api/payments
Process a payment (requires authentication).

**Request Body:**
```json
{
  "loanId": "loan-id",
  "amount": 1500,
  "paymentMethod": "BANK_TRANSFER",
  "reference": "TXN123456"
}
```

### GET /api/payments/loan/{loanId}
Get payment history for a loan (requires authentication).

---

## 🔔 Notification Endpoints

### GET /api/notifications
Get user notifications (requires authentication).

### PUT /api/notifications/{notificationId}/read
Mark notification as read (requires authentication).

---

## 👨‍💼 Admin Endpoints

### GET /api/admin/loans
Get all loans with admin view (Admin only).

### POST /api/admin/loans/{loanId}/approve
Approve a loan (Admin only).

### POST /api/admin/loans/{loanId}/reject
Reject a loan (Admin only).

### POST /api/admin/loans/{loanId}/disburse
Disburse loan funds (Admin only).

### POST /api/admin/loans/{loanId}/close
Close a loan (Admin only).

---

## 📊 Response Format

All endpoints return responses in the following format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ },
  "errors": []
}
```

### Error Response Example:
```json
{
  "success": false,
  "message": "Loan not found",
  "data": null,
  "errors": ["Invalid loan ID"]
}
```

---

## 🔑 Authentication

Most endpoints require authentication via JWT Bearer token:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📝 Status Codes

- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `500` - Internal Server Error

---

## 🧪 Testing

Use Swagger UI at `/swagger` for interactive API testing in development mode.
