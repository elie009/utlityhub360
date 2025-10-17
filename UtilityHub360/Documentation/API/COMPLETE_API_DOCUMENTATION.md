# UtilityHub360 - Complete API Documentation

## 🚀 Overview
This is a comprehensive financial management system API built with ASP.NET Core 8.0 and Entity Framework Core. The API provides complete functionality for user management, loan processing, bill management, bank accounts, savings, and administrative operations.

## 📋 Base Configuration
- **Base URL:** `http://localhost:5000/api` (Development)
- **Authentication:** JWT Bearer tokens
- **Content-Type:** `application/json`
- **Framework:** ASP.NET Core 8.0
- **Database:** SQL Server

## 🔐 Authentication & Authorization

### JWT Configuration
- **Secret Key:** Configured in `appsettings.json`
- **Issuer:** UtilityHub360
- **Audience:** UtilityHub360Users
- **Expiration:** 60 minutes (configurable)

### User Roles
- **USER:** Regular users who can manage their financial data
- **ADMIN:** Administrators who can manage all users and system data

---

## 📚 API Endpoints

### 1. 🔑 Authentication Endpoints (`/api/auth`)

#### POST `/api/auth/register`
**Description:** Register a new user account
**Authentication:** Not required
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
**Authentication:** Not required
**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "password123"
}
```
**Response:** Same as register response

#### POST `/api/auth/forgot-password` ⭐ **NEW**
**Description:** Request password reset
**Authentication:** Not required
**Request Body:**
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

#### POST `/api/auth/reset-password` ⭐ **NEW**
**Description:** Reset password using token
**Authentication:** Not required
**Request Body:**
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

#### GET `/api/auth/me`
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

#### POST `/api/auth/refresh`
**Description:** Refresh JWT token using refresh token
**Authentication:** Not required
**Request Body:**
```json
{
  "refreshToken": "guid-refresh-token"
}
```

#### POST `/api/auth/logout`
**Description:** Logout current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 2. 👥 User Management Endpoints (`/api/users`)

#### GET `/api/users/{userId}`
**Description:** Get user by ID
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Access:** Users can view their own profile, admins can view any profile

#### PUT `/api/users/{userId}`
**Description:** Update user information
**Authentication:** Required
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
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)
- `role`: Filter by role (optional)
- `isActive`: Filter by active status (optional)

---

### 3. 💰 Loan Management Endpoints (`/api/loans`)

#### POST `/api/loans/apply`
**Description:** Apply for a new loan
**Authentication:** Required
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
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/loans/user/{userId}`
**Description:** Get all loans for a user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by loan status (optional)
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### GET `/api/loans/{loanId}/status`
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

#### GET `/api/loans/{loanId}/schedule`
**Description:** Get repayment schedule for a loan
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/loans/{loanId}/transactions`
**Description:** Get transaction history for a loan
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 4. 💳 Bill Management Endpoints (`/api/bills`)

#### POST `/api/bills`
**Description:** Create a new bill
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
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

#### GET `/api/bills`
**Description:** Get all bills for current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by status (PENDING, PAID, OVERDUE)
- `billType`: Filter by bill type
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### GET `/api/bills/{billId}`
**Description:** Get bill details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### PUT `/api/bills/{billId}`
**Description:** Update bill
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### DELETE `/api/bills/{billId}`
**Description:** Delete bill
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### PUT `/api/bills/{billId}/pay`
**Description:** Mark bill as paid
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "paidAt": "2024-01-15T10:30:00Z",
  "notes": "Paid via online banking"
}
```

#### GET `/api/bills/admin/all` ⭐ **Admin Only**
**Description:** Get all bills for all users
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

---

### 5. 🏦 Bank Account Endpoints (`/api/bankaccounts`)

#### POST `/api/bankaccounts`
**Description:** Create a new bank account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
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

#### GET `/api/bankaccounts`
**Description:** Get all bank accounts for current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/bankaccounts/{accountId}`
**Description:** Get bank account details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### PUT `/api/bankaccounts/{accountId}`
**Description:** Update bank account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### DELETE `/api/bankaccounts/{accountId}`
**Description:** Delete bank account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/bankaccounts/{accountId}/transactions`
**Description:** Get bank account transactions
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 6. 💰 Savings Endpoints (`/api/savings`)

#### POST `/api/savings`
**Description:** Create a new savings account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
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

#### GET `/api/savings`
**Description:** Get all savings accounts for current user
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/savings/{accountId}`
**Description:** Get savings account details
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### POST `/api/savings/{accountId}/deposit`
**Description:** Make a deposit to savings account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "amount": 500.00,
  "notes": "Monthly savings deposit"
}
```

#### POST `/api/savings/{accountId}/withdraw`
**Description:** Make a withdrawal from savings account
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "amount": 200.00,
  "notes": "Emergency withdrawal"
}
```

---

### 7. 💵 Payment Endpoints (`/api/payments`)

#### POST `/api/payments`
**Description:** Make a payment
**Authentication:** Required
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
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/payments/loan/{loanId}`
**Description:** Get all payments for a loan
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### PUT `/api/payments/{paymentId}/status`
**Description:** Update payment status (Admin only)
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "status": "COMPLETED"
}
```

---

### 8. 📊 Dashboard Endpoints (`/api/dashboard`)

#### GET `/api/dashboard/financial-summary`
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

#### GET `/api/dashboard/disposable-amount`
**Description:** Get user's disposable amount
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/dashboard/disposable-amount/user/{userId}` ⭐ **Admin Only**
**Description:** Get any user's disposable amount
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/dashboard/financial-summary/user/{userId}` ⭐ **Admin Only**
**Description:** Get any user's financial summary
**Authentication:** Required (Admin)
**Headers:** `Authorization: Bearer <token>`

---

### 9. 📈 Income Source Endpoints (`/api/incomesources`)

#### POST `/api/incomesources`
**Description:** Add income source
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "sourceName": "Salary",
  "amount": 5000.00,
  "frequency": "monthly",
  "startDate": "2024-01-01T00:00:00Z",
  "isActive": true
}
```

#### GET `/api/incomesources`
**Description:** Get all income sources
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### PUT `/api/incomesources/{sourceId}`
**Description:** Update income source
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### DELETE `/api/incomesources/{sourceId}`
**Description:** Delete income source
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 10. 💸 Variable Expenses Endpoints (`/api/variableexpenses`)

#### POST `/api/variableexpenses`
**Description:** Add variable expense
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "expenseName": "Groceries",
  "amount": 300.00,
  "category": "FOOD",
  "date": "2024-01-15T00:00:00Z",
  "notes": "Weekly grocery shopping"
}
```

#### GET `/api/variableexpenses`
**Description:** Get all variable expenses
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `startDate`: Start date filter
- `endDate`: End date filter
- `category`: Category filter
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### PUT `/api/variableexpenses/{expenseId}`
**Description:** Update variable expense
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### DELETE `/api/variableexpenses/{expenseId}`
**Description:** Delete variable expense
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 11. 👤 User Profile Endpoints (`/api/userprofile`)

#### GET `/api/userprofile`
**Description:** Get user profile
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### PUT `/api/userprofile`
**Description:** Update user profile
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
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

#### POST `/api/userprofile/employment`
**Description:** Update employment information
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
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

### 12. 🚀 Onboarding Endpoints (`/api/onboarding`)

#### GET `/api/onboarding/status`
**Description:** Get onboarding status
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### POST `/api/onboarding/step/{stepNumber}`
**Description:** Complete onboarding step
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Request Body:**
```json
{
  "stepData": {
    "monthlyIncome": 5000.00,
    "monthlyExpenses": 3000.00,
    "savingsGoal": 10000.00
  }
}
```

#### POST `/api/onboarding/complete`
**Description:** Complete onboarding process
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 13. 🔔 Notification Endpoints (`/api/notifications`)

#### GET `/api/notifications/user/{userId}`
**Description:** Get user notifications
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `status`: Filter by read status (unread/read)
- `type`: Filter by notification type
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)

#### PUT `/api/notifications/{notificationId}/read`
**Description:** Mark notification as read
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

#### GET `/api/notifications/user/{userId}/unread-count`
**Description:** Get unread notification count
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 14. 📊 Reports Endpoints (`/api/reports`)

#### GET `/api/reports/user/{userId}`
**Description:** Get user financial report
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`
**Query Parameters:**
- `period`: Report period (year/month)
- `startDate`: Start date (ISO format)
- `endDate`: End date (ISO format)

#### GET `/api/reports/loan/{loanId}`
**Description:** Get detailed loan report
**Authentication:** Required
**Headers:** `Authorization: Bearer <token>`

---

### 15. 👨‍💼 Admin Endpoints (`/api/admin`)

#### PUT `/api/admin/loans/{loanId}/approve`
**Description:** Approve a loan (Admin only)
**Authentication:** Required (Admin)
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
**Authentication:** Required (Admin)
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
**Authentication:** Required (Admin)
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
**Authentication:** Required (Admin)
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
**Authentication:** Required (Admin)
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

---

## 📋 Data Models

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

---

## ⚠️ Error Handling

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

---

## 🔒 Security Features

1. **JWT Authentication:** Secure token-based authentication
2. **Role-based Authorization:** Different access levels for users and admins
3. **Input Validation:** Comprehensive validation on all inputs
4. **SQL Injection Protection:** Entity Framework parameterized queries
5. **CORS Configuration:** Configurable cross-origin resource sharing
6. **Password Reset Security:** Secure token-based password reset with expiration
7. **Email Verification:** Password reset links sent via email

---

## 🗄️ Database Schema

The API uses the following main entities:
- **Users:** User accounts and profiles
- **Loans:** Loan applications and details
- **RepaymentSchedules:** Installment schedules
- **Payments:** Payment records
- **Transactions:** Financial transaction history
- **Notifications:** User notifications
- **LoanApplications:** Loan application workflow
- **Bills:** Bill management and tracking
- **BankAccounts:** Bank account management
- **BankTransactions:** Bank transaction history
- **SavingsAccounts:** Savings account management
- **SavingsTransactions:** Savings transaction history
- **UserProfiles:** Extended user profile information
- **IncomeSources:** Income source tracking
- **VariableExpenses:** Variable expense tracking
- **UserOnboardings:** User onboarding process
- **PasswordResets:** Password reset token management ⭐ **NEW**

---

## 🚀 Getting Started

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
   Navigate to `http://localhost:5000/swagger` for interactive API documentation

---

## ⚙️ Configuration

Update `appsettings.json` for:
- Database connection string
- JWT settings
- SMTP settings for email functionality ⭐ **NEW**
- Logging configuration
- CORS policies

### Email Configuration Example:
```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "UtilityHub360"
  },
  "AppSettings": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

---

## 🧪 Testing

The API includes comprehensive error handling and validation. Test endpoints using:
- Swagger UI (interactive documentation)
- Postman collections
- HTTP test files (included in project)
- Unit tests (recommended to add)
- Integration tests (recommended to add)

### Test Files Available:
- `test_auth.http` - Authentication testing
- `test_forgot_password.http` - Password reset testing ⭐ **NEW**
- Various other test files for different endpoints

---

## 🏭 Production Considerations

1. **Security:**
   - Use strong JWT secret keys
   - Implement HTTPS
   - Add rate limiting
   - Implement proper logging and monitoring
   - Configure secure SMTP settings

2. **Performance:**
   - Add caching for frequently accessed data
   - Implement database indexing
   - Use connection pooling

3. **Scalability:**
   - Consider microservices architecture
   - Implement message queues for async operations
   - Add load balancing

---

## 📝 Recent Updates

### ⭐ **New Features Added:**
1. **Forgot Password Functionality:**
   - Secure password reset with email verification
   - Token-based reset system with expiration
   - Professional HTML email templates
   - Complete security implementation

2. **Enhanced Security:**
   - Password reset tokens with 1-hour expiration
   - One-time use tokens
   - Secure email delivery system

This API provides a complete foundation for a comprehensive financial management system with all essential features for user management, loan processing, bill management, bank account tracking, savings management, and administrative operations.

---

## 📞 Support

For API support and documentation updates, refer to the project documentation or contact the development team.

**Total Endpoints:** 50+ endpoints across 15+ controllers
**Authentication Methods:** JWT Bearer tokens
**Database:** SQL Server with Entity Framework Core
**Framework:** ASP.NET Core 8.0
