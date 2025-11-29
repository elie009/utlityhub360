# UtilityHub360 - Complete API Endpoints Reference

## 🚀 Base Configuration
- **Base URL:** `http://localhost:5000/api`
- **Authentication:** JWT Bearer tokens
- **Content-Type:** `application/json`

---

## 🔑 Authentication Endpoints (`/api/auth`)

### 1. POST `/api/auth/register`
**Description:** Register a new user account
**Authentication:** Not required

**Request:**
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

### 2. POST `/api/auth/login`
**Description:** Authenticate user and get JWT token
**Authentication:** Not required

**Request:**
```json
{
  "email": "john@example.com",
  "password": "password123"
}
```

**Response:** Same as register response

### 3. POST `/api/auth/forgot-password` ⭐ **NEW**
**Description:** Request password reset
**Authentication:** Not required

**Request:**
```json
{
  "email": "john@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "If the email exists, a password reset link has been sent.",
  "data": {}
}
```

### 4. POST `/api/auth/reset-password` ⭐ **NEW**
**Description:** Reset password using token
**Authentication:** Not required

**Request:**
```json
{
  "token": "reset-token-from-email",
  "email": "john@example.com",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Password has been reset successfully.",
  "data": {}
}
```

### 5. GET `/api/auth/me`
**Description:** Get current authenticated user information
**Authentication:** Required
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

### 6. POST `/api/auth/refresh`
**Description:** Refresh JWT token using refresh token
**Authentication:** Not required

**Request:**
```json
{
  "refreshToken": "guid-refresh-token"
}
```

### 7. POST `/api/auth/logout`
**Description:** Logout current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 👥 User Management Endpoints (`/api/users`)

### 8. GET `/api/users/{userId}`
**Description:** Get user by ID
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Access:** Users can view their own profile, admins can view any profile

**Response:**
```json
{
  "success": true,
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

### 9. PUT `/api/users/{userId}`
**Description:** Update user information
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "name": "John Smith",
  "phone": "+1234567891",
  "role": "USER",
  "isActive": true
}
```

### 10. GET `/api/users`
**Description:** Get all users (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)
- `role`: Filter by role (optional)
- `isActive`: Filter by active status (optional)

---

## 💰 Loan Management Endpoints (`/api/loans`)

### 11. POST `/api/loans/apply`
**Description:** Apply for a new loan
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
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

### 12. GET `/api/loans/{loanId}`
**Description:** Get loan details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 13. GET `/api/loans/user/{userId}`
**Description:** Get all loans for a user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by loan status (optional)
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

### 14. GET `/api/loans/{loanId}/status`
**Description:** Get loan status and outstanding balance
**Authentication:** Required
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

### 15. GET `/api/loans/{loanId}/schedule`
**Description:** Get repayment schedule for a loan
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 16. GET `/api/loans/{loanId}/transactions`
**Description:** Get transaction history for a loan
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 💳 Bill Management Endpoints (`/api/bills`)

### 17. POST `/api/bills`
**Description:** Create a new bill
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "billName": "Electricity Bill",
  "billType": "UTILITY",
  "provider": "Electric Company",
  "amount": 150.00,
  "dueDate": "2024-01-15T00:00:00Z",
  "frequency": "monthly",
  "autoGenerateNext": true,
  "notes": "Monthly electricity bill"
}
```

### 18. GET `/api/bills`
**Description:** Get all bills for current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by status (PENDING, PAID, OVERDUE)
- `billType`: Filter by bill type
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

### 19. GET `/api/bills/{billId}`
**Description:** Get bill details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 20. PUT `/api/bills/{billId}`
**Description:** Update bill
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 21. DELETE `/api/bills/{billId}`
**Description:** Delete bill
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 22. PUT `/api/bills/{billId}/pay`
**Description:** Mark bill as paid
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "paidAt": "2024-01-15T10:30:00Z",
  "notes": "Paid via online banking"
}
```

### 23. GET `/api/bills/admin/all` ⭐ **Admin Only**
**Description:** Get all bills for all users
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

---

## 🏦 Bank Account Endpoints (`/api/bankaccounts`)

### 24. POST `/api/bankaccounts`
**Description:** Create a new bank account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "accountName": "Main Checking",
  "accountNumber": "1234567890",
  "bankName": "Bank of America",
  "accountType": "CHECKING",
  "balance": 5000.00,
  "currency": "USD"
}
```

### 25. GET `/api/bankaccounts`
**Description:** Get all bank accounts for current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 26. GET `/api/bankaccounts/{accountId}`
**Description:** Get bank account details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 27. PUT `/api/bankaccounts/{accountId}`
**Description:** Update bank account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 28. DELETE `/api/bankaccounts/{accountId}`
**Description:** Delete bank account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 29. GET `/api/bankaccounts/{accountId}/transactions`
**Description:** Get bank account transactions
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 💰 Savings Endpoints (`/api/savings`)

### 30. POST `/api/savings`
**Description:** Create a new savings account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "accountName": "Emergency Fund",
  "accountType": "SAVINGS",
  "initialBalance": 1000.00,
  "interestRate": 2.5,
  "goalAmount": 10000.00,
  "goalDate": "2024-12-31T00:00:00Z"
}
```

## 💰 Savings Endpoints (`/api/Savings`)

### 31. GET `/api/Savings/accounts`
**Description:** Get all savings accounts for current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "savings-account-id",
      "accountName": "Emergency Fund",
      "savingsType": "EMERGENCY",
      "targetAmount": 10000.00,
      "currentBalance": 2500.00,
      "currency": "USD",
      "progressPercentage": 25.0,
      "remainingAmount": 7500.00,
      "daysRemaining": 180,
      "monthlyTarget": 416.67
    }
  ]
}
```

### 32. POST `/api/Savings/accounts`
**Description:** Create a new savings account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "accountName": "Emergency Fund",
  "savingsType": "EMERGENCY",
  "targetAmount": 10000.00,
  "description": "6 months emergency savings",
  "goal": "Build emergency fund for unexpected expenses",
  "targetDate": "2024-12-31T00:00:00Z",
  "currency": "USD"
}
```

### 33. GET `/api/Savings/accounts/{savingsAccountId}`
**Description:** Get savings account details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 34. PUT `/api/Savings/accounts/{savingsAccountId}`
**Description:** Update savings account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 35. DELETE `/api/Savings/accounts/{savingsAccountId}`
**Description:** Delete savings account (only if no transactions exist)
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 36. POST `/api/Savings/transactions`
**Description:** Create a savings transaction (deposit or withdrawal)
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "savingsAccountId": "savings-account-id",
  "sourceBankAccountId": "bank-account-id",
  "amount": 500.00,
  "transactionType": "DEPOSIT",
  "description": "Monthly savings contribution",
  "category": "MONTHLY_SAVINGS",
  "notes": "20% of salary saved",
  "transactionDate": "2024-01-15T00:00:00Z",
  "currency": "USD",
  "isRecurring": true,
  "recurringFrequency": "MONTHLY"
}
```

**Note:** `transactionType` must be either `"DEPOSIT"` (transfer from bank to savings) or `"WITHDRAWAL"` (transfer from savings to bank).

### 37. GET `/api/Savings/summary`
**Description:** Get savings summary with total savings balance across all accounts
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "success": true,
  "data": {
    "totalSavingsAccounts": 3,
    "totalSavingsBalance": 5000.00,
    "totalTargetAmount": 15000.00,
    "overallProgressPercentage": 33.33,
    "activeGoals": 2,
    "completedGoals": 1,
    "monthlySavingsTarget": 1250.00,
    "thisMonthSaved": 500.00,
    "recentAccounts": [...]
  }
}
```

### 38. GET `/api/Savings/analytics?period=month`
**Description:** Get savings analytics for a specific period
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `period`: Period type (week/month/quarter/year, default: month)

### 39. GET `/api/Savings/by-type`
**Description:** Get savings grouped by type (EMERGENCY, VACATION, etc.)
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 40. POST `/api/Savings/transfer/bank-to-savings`
**Description:** Transfer money from bank account to savings account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "bankAccountId": "bank-account-id",
  "savingsAccountId": "savings-account-id",
  "amount": 1000.00,
  "description": "Monthly savings transfer"
}
```

### 41. POST `/api/Savings/transfer/savings-to-bank`
**Description:** Transfer money from savings account to bank account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "savingsAccountId": "savings-account-id",
  "bankAccountId": "bank-account-id",
  "amount": 500.00,
  "description": "Emergency withdrawal"
}
```

---

## 💵 Payment Endpoints (`/api/payments`)

### 35. POST `/api/payments`
**Description:** Make a payment
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "loanId": "loan-id",
  "amount": 2500,
  "method": "BANK_TRANSFER",
  "reference": "PAY-001"
}
```

### 36. GET `/api/payments/{paymentId}`
**Description:** Get payment details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 37. GET `/api/payments/loan/{loanId}`
**Description:** Get all payments for a loan
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

### 38. PUT `/api/payments/{paymentId}/status`
**Description:** Update payment status (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "status": "COMPLETED"
}
```

---

## 📊 Dashboard Endpoints (`/api/dashboard`)

### 39. GET `/api/dashboard/financial-summary`
**Description:** Get user's financial summary
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "success": true,
  "data": {
    "totalIncome": 8000.00,
    "totalExpenses": 4500.00,
    "totalSavings": 2000.00,
    "disposableAmount": 1500.00,
    "monthlyBills": 1200.00,
    "loanPayments": 1800.00
  }
}
```

### 40. GET `/api/dashboard/disposable-amount`
**Description:** Get user's disposable amount
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 41. GET `/api/dashboard/disposable-amount/user/{userId}` ⭐ **Admin Only**
**Description:** Get any user's disposable amount
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

### 42. GET `/api/dashboard/financial-summary/user/{userId}` ⭐ **Admin Only**
**Description:** Get any user's financial summary
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

---

## 📈 Income Source Endpoints (`/api/incomesources`)

### 43. POST `/api/incomesources`
**Description:** Add income source
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "sourceName": "Salary",
  "amount": 5000.00,
  "frequency": "monthly",
  "startDate": "2024-01-01T00:00:00Z",
  "isActive": true
}
```

### 44. GET `/api/incomesources`
**Description:** Get all income sources
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 45. PUT `/api/incomesources/{sourceId}`
**Description:** Update income source
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 46. DELETE `/api/incomesources/{sourceId}`
**Description:** Delete income source
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 💸 Variable Expenses Endpoints (`/api/variableexpenses`)

### 47. POST `/api/variableexpenses`
**Description:** Add variable expense
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "expenseName": "Groceries",
  "amount": 300.00,
  "category": "FOOD",
  "date": "2024-01-15T00:00:00Z",
  "notes": "Weekly grocery shopping"
}
```

### 48. GET `/api/variableexpenses`
**Description:** Get all variable expenses
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `startDate`: Start date filter
- `endDate`: End date filter
- `category`: Category filter
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

### 49. PUT `/api/variableexpenses/{expenseId}`
**Description:** Update variable expense
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 50. DELETE `/api/variableexpenses/{expenseId}`
**Description:** Delete variable expense
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 👤 User Profile Endpoints (`/api/userprofile`)

### 51. GET `/api/userprofile`
**Description:** Get user profile
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 52. PUT `/api/userprofile`
**Description:** Update user profile
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "address": "123 Main St",
  "city": "New York",
  "state": "NY",
  "zipCode": "10001",
  "country": "USA"
}
```

### 53. POST `/api/userprofile/employment`
**Description:** Update employment information
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "jobTitle": "Software Engineer",
  "company": "Tech Corp",
  "employmentType": "FULL_TIME",
  "industry": "Technology",
  "location": "New York, NY"
}
```

---

## 🚀 Onboarding Endpoints (`/api/onboarding`)

### 54. GET `/api/onboarding/status`
**Description:** Get onboarding status
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 55. POST `/api/onboarding/step/{stepNumber}`
**Description:** Complete onboarding step
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "stepData": {
    "monthlyIncome": 5000.00,
    "monthlyExpenses": 3000.00,
    "savingsGoal": 10000.00
  }
}
```

### 56. POST `/api/onboarding/complete`
**Description:** Complete onboarding process
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 🔔 Notification Endpoints (`/api/notifications`)

### 57. GET `/api/notifications/user/{userId}`
**Description:** Get user notifications
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by read status (unread/read)
- `type`: Filter by notification type
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

### 58. PUT `/api/notifications/{notificationId}/read`
**Description:** Mark notification as read
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

### 59. GET `/api/notifications/user/{userId}/unread-count`
**Description:** Get unread notification count
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 📊 Reports Endpoints (`/api/Reports`)

### 60. GET `/api/Reports/summary`
**Description:** Get financial summary including total savings, income, expenses, and net worth
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `date`: Optional date for summary (default: current date)

**Response:**
```json
{
  "success": true,
  "data": {
    "totalIncome": 8000.00,
    "incomeChange": 5.2,
    "totalExpenses": 4500.00,
    "expenseChange": -2.1,
    "disposableIncome": 3500.00,
    "disposableChange": 8.5,
    "totalSavings": 5000.00,
    "savingsGoal": 10000.00,
    "savingsProgress": 50.0,
    "netWorth": 25000.00,
    "netWorthChange": 12.5
  }
}
```

**Note:** This endpoint provides `totalSavings` which is an alternative to `GET /api/Savings/summary` for getting your total savings amount. Use this endpoint when you need a comprehensive financial summary that includes savings along with income, expenses, and net worth.

### 61. GET `/api/Reports/user/{userId}`
**Description:** Get user financial report
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `period`: Report period (year/month)
- `startDate`: Start date (ISO format)
- `endDate`: End date (ISO format)

### 62. GET `/api/Reports/loan/{loanId}`
**Description:** Get detailed loan report
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

## 👨‍💼 Admin Endpoints (`/api/admin`)

### 63. PUT `/api/admin/loans/{loanId}/approve`
**Description:** Approve a loan (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "approvedBy": "admin-user-id",
  "notes": "Approval notes"
}
```

### 64. PUT `/api/admin/loans/{loanId}/reject`
**Description:** Reject a loan (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "reason": "Insufficient income",
  "rejectedBy": "admin-user-id",
  "notes": "Rejection details"
}
```

### 64. POST `/api/admin/transactions/disburse`
**Description:** Disburse a loan (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "loanId": "loan-id",
  "disbursedBy": "admin-user-id",
  "disbursementMethod": "BANK_TRANSFER",
  "reference": "DISB-001"
}
```

### 65. PUT `/api/admin/loans/{loanId}/close`
**Description:** Close a loan (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "closedBy": "admin-user-id",
  "notes": "Loan completed successfully"
}
```

### 66. POST `/api/admin/notifications/send`
**Description:** Send notification to user (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "userId": "user-id",
  "type": "PAYMENT_DUE",
  "title": "Payment Due",
  "message": "Your monthly payment is due on 2024-01-15"
}
```

---

## 📋 Data Models & Enums

### Enums
- **UserRole:** USER, ADMIN
- **LoanStatus:** PENDING, APPROVED, REJECTED, ACTIVE, COMPLETED, DEFAULTED
- **PaymentMethod:** BANK_TRANSFER, CARD, WALLET, CASH
- **PaymentStatus:** PENDING, COMPLETED, FAILED
- **RepaymentStatus:** PENDING, PAID, OVERDUE
- **TransactionType:** DISBURSEMENT, PAYMENT, INTEREST, PENALTY
- **NotificationType:** PAYMENT_DUE, PAYMENT_RECEIVED, LOAN_APPROVED, LOAN_REJECTED, GENERAL
- **EmploymentStatus:** employed, self_employed, unemployed, retired, student
- **BillType:** UTILITY, INSURANCE, SUBSCRIPTION, LOAN, OTHER
- **BillStatus:** PENDING, PAID, OVERDUE
- **AccountType:** CHECKING, SAVINGS, INVESTMENT
- **Frequency:** monthly, weekly, yearly, one_time

### Standard Response Format
```json
{
  "success": boolean,
  "message": string,
  "data": any,
  "errors": string[]
}
```

### Pagination Format
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

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

---

## ⚠️ HTTP Status Codes
- **200 OK:** Successful request
- **201 Created:** Resource created successfully
- **400 Bad Request:** Invalid request data
- **401 Unauthorized:** Authentication required
- **403 Forbidden:** Insufficient permissions
- **404 Not Found:** Resource not found
- **500 Internal Server Error:** Server error

---

## 🔒 Authentication
All endpoints marked as "Authentication: Required" need the following header:
```
Authorization: Bearer <your-jwt-token>
```

## 📊 Summary
- **Total Endpoints:** 66 endpoints
- **Controllers:** 15+ controllers
- **Authentication Methods:** JWT Bearer tokens
- **Database:** SQL Server with Entity Framework Core
- **Framework:** ASP.NET Core 8.0

## ⭐ New Features
- **Forgot Password:** Complete password reset functionality with email verification
- **Email Service:** Professional HTML email templates for password reset
- **Enhanced Security:** Token-based password reset with expiration and one-time use

---

**Base URL:** `http://localhost:5000/api`
**Swagger UI:** `http://localhost:5000/swagger`
