# UtilityHub360 API Documentation

## Overview
This is a comprehensive loan management system API built with ASP.NET Core 8.0 and Entity Framework Core. The API provides complete functionality for user management, loan processing, payment handling, and administrative operations.

## Base Configuration
- **Base URL:** `https://localhost:7000/api` (Development)
- **Authentication:** JWT Bearer tokens
- **Content-Type:** `application/json`
- **Framework:** ASP.NET Core 8.0
- **Database:** SQL Server LocalDB

## Authentication & Authorization

### JWT Configuration
- **Secret Key:** Configured in `appsettings.json`
- **Issuer:** UtilityHub360
- **Audience:** UtilityHub360Users
- **Expiration:** 60 minutes (configurable)

### User Roles
- **USER:** Regular users who can apply for loans and make payments
- **ADMIN:** Administrators who can approve/reject loans and manage the system

## API Endpoints

### 1. Authentication Endpoints (`/api/auth`)

#### POST `/api/auth/register`
**Description:** Register a new user account
**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "password": "password123",
  "confirmPassword": "password123"
}
```
**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "guid-refresh-token",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": "user-id",
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "+1234567890",
      "role": "USER",
      "isActive": true,
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    }
  }
}
```

#### POST `/api/auth/login`
**Description:** Authenticate user and get JWT token
**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "password123"
}
```
**Response:** Same as register response

#### GET `/api/auth/me`
**Description:** Get current authenticated user information
**Headers:** `Authorization: Bearer <token>`
**Response:**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "user-id",
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890",
    "role": "USER",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T10:00:00Z"
  }
}
```

#### POST `/api/auth/refresh`
**Description:** Refresh JWT token using refresh token
**Request Body:**
```json
{
  "refreshToken": "guid-refresh-token"
}
```

#### POST `/api/auth/logout`
**Description:** Logout current user
**Headers:** `Authorization: Bearer <token>`

### 2. User Management Endpoints (`/api/users`)

#### GET `/api/users/{userId}`
**Description:** Get user by ID
**Headers:** `Authorization: Bearer <token>`
**Access:** Users can view their own profile, admins can view any profile

#### PUT `/api/users/{userId}`
**Description:** Update user information
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "name": "John Smith",
  "phone": "+1234567891",
  "role": "USER",
  "isActive": true
}
```

#### GET `/api/users`
**Description:** Get all users (Admin only)
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)
- `role`: Filter by role (optional)
- `isActive`: Filter by active status (optional)

### 3. Loan Management Endpoints (`/api/loans`)

#### POST `/api/loans/apply`
**Description:** Apply for a new loan
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "principal": 50000,
  "purpose": "Home improvement",
  "term": 24,
  "monthlyIncome": 8000,
  "employmentStatus": "employed",
  "additionalInfo": "Additional details about the loan purpose"
}
```

#### GET `/api/loans/{loanId}`
**Description:** Get loan details
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/loans/user/{userId}`
**Description:** Get all loans for a user
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by loan status (optional)
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### GET `/api/loans/{loanId}/status`
**Description:** Get loan status and outstanding balance
**Headers:** `Authorization: Bearer <token>`
**Response:**
```json
{
  "success": true,
  "data": {
    "status": "ACTIVE",
    "outstandingBalance": 45000
  }
}
```

#### GET `/api/loans/{loanId}/schedule`
**Description:** Get repayment schedule for a loan
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/loans/{loanId}/transactions`
**Description:** Get transaction history for a loan
**Headers:** `Authorization: Bearer <token>`

### 4. Admin Endpoints (`/api/admin`)

#### PUT `/api/admin/loans/{loanId}/approve`
**Description:** Approve a loan (Admin only)
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "approvedBy": "admin-user-id",
  "notes": "Approval notes"
}
```

#### PUT `/api/admin/loans/{loanId}/reject`
**Description:** Reject a loan (Admin only)
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "reason": "Insufficient income",
  "rejectedBy": "admin-user-id",
  "notes": "Rejection details"
}
```

#### POST `/api/admin/transactions/disburse`
**Description:** Disburse a loan (Admin only)
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "loanId": "loan-id",
  "disbursedBy": "admin-user-id",
  "disbursementMethod": "BANK_TRANSFER",
  "reference": "DISB-001"
}
```

#### PUT `/api/admin/loans/{loanId}/close`
**Description:** Close a loan (Admin only)
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "closedBy": "admin-user-id",
  "notes": "Loan completed successfully"
}
```

#### POST `/api/admin/notifications/send`
**Description:** Send notification to user (Admin only)
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "userId": "user-id",
  "type": "PAYMENT_DUE",
  "title": "Payment Due",
  "message": "Your monthly payment is due on 2024-01-15"
}
```

### 5. Payment Endpoints (`/api/payments`)

#### POST `/api/payments`
**Description:** Make a payment
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "loanId": "loan-id",
  "amount": 2500,
  "method": "BANK_TRANSFER",
  "reference": "PAY-001"
}
```

#### GET `/api/payments/{paymentId}`
**Description:** Get payment details
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/payments/loan/{loanId}`
**Description:** Get all payments for a loan
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### PUT `/api/payments/{paymentId}/status`
**Description:** Update payment status (Admin only)
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "status": "COMPLETED"
}
```

### 6. Notification Endpoints (`/api/notifications`)

#### GET `/api/notifications/user/{userId}`
**Description:** Get user notifications
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by read status (unread/read)
- `type`: Filter by notification type
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### PUT `/api/notifications/{notificationId}/read`
**Description:** Mark notification as read
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/notifications/user/{userId}/unread-count`
**Description:** Get unread notification count
**Headers:** `Authorization: Bearer <token>`

### 7. Report Endpoints (`/api/reports`)

#### GET `/api/reports/user/{userId}`
**Description:** Get user financial report
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `period`: Report period (year/month)
- `startDate`: Start date (ISO format)
- `endDate`: End date (ISO format)

#### GET `/api/reports/loan/{loanId}`
**Description:** Get detailed loan report
**Headers:** `Authorization: Bearer <token>`

## Data Models

### Enums
- **UserRole:** USER, ADMIN
- **LoanStatus:** PENDING, APPROVED, REJECTED, ACTIVE, COMPLETED, DEFAULTED
- **PaymentMethod:** BANK_TRANSFER, CARD, WALLET, CASH
- **PaymentStatus:** PENDING, COMPLETED, FAILED
- **RepaymentStatus:** PENDING, PAID, OVERDUE
- **TransactionType:** DISBURSEMENT, PAYMENT, INTEREST, PENALTY
- **NotificationType:** PAYMENT_DUE, PAYMENT_RECEIVED, LOAN_APPROVED, LOAN_REJECTED, GENERAL
- **EmploymentStatus:** employed, self_employed, unemployed, retired, student

### Response Format
All API responses follow this format:
```json
{
  "success": boolean,
  "message": string,
  "data": any,
  "errors": string[]
}
```

### Pagination
Paginated responses include:
```json
{
  "data": [...],
  "page": 1,
  "limit": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## Error Handling

### HTTP Status Codes
- **200 OK:** Successful request
- **201 Created:** Resource created successfully
- **400 Bad Request:** Invalid request data
- **401 Unauthorized:** Authentication required
- **403 Forbidden:** Insufficient permissions
- **404 Not Found:** Resource not found
- **500 Internal Server Error:** Server error

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

## Security Features

1. **JWT Authentication:** Secure token-based authentication
2. **Role-based Authorization:** Different access levels for users and admins
3. **Input Validation:** Comprehensive validation on all inputs
4. **SQL Injection Protection:** Entity Framework parameterized queries
5. **CORS Configuration:** Configurable cross-origin resource sharing

## Database Schema

The API uses the following main entities:
- **Users:** User accounts and profiles
- **Loans:** Loan applications and details
- **RepaymentSchedules:** Installment schedules
- **Payments:** Payment records
- **Transactions:** Financial transaction history
- **Notifications:** User notifications
- **LoanApplications:** Loan application workflow

## Getting Started

1. **Install Dependencies:**
   ```bash
   dotnet restore
   ```

2. **Update Database:**
   ```bash
   dotnet ef database update
   ```

3. **Run the Application:**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI:**
   Navigate to `https://localhost:7000/swagger` for interactive API documentation

## Configuration

Update `appsettings.json` for:
- Database connection string
- JWT settings
- Logging configuration
- CORS policies

## Testing

The API includes comprehensive error handling and validation. Test endpoints using:
- Swagger UI (interactive documentation)
- Postman collections
- Unit tests (recommended to add)
- Integration tests (recommended to add)

## Production Considerations

1. **Security:**
   - Use strong JWT secret keys
   - Implement HTTPS
   - Add rate limiting
   - Implement proper logging and monitoring

2. **Performance:**
   - Add caching for frequently accessed data
   - Implement database indexing
   - Use connection pooling

3. **Scalability:**
   - Consider microservices architecture
   - Implement message queues for async operations
   - Add load balancing

This API provides a complete foundation for a loan management system with all the essential features for user management, loan processing, payment handling, and administrative operations.

