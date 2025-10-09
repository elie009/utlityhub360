# Database Schema

## 📊 Entity Relationship Diagram

```
Users (1) ←→ (N) Loans (1) ←→ (N) Transactions
    ↓              ↓              ↓
    ↓              ↓              ↓
Notifications  RepaymentSchedules  Payments
```

## 🗄️ Tables Overview

### Users Table
Primary user account information.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique user identifier |
| Name | string | User's full name |
| Email | string (Unique) | User's email address |
| Phone | string | User's phone number |
| Role | string | User role (USER, ADMIN) |
| IsActive | boolean | Account status |
| CreatedAt | datetime | Account creation date |
| UpdatedAt | datetime | Last update timestamp |

### Loans Table
Loan application and management data.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique loan identifier |
| UserId | string (FK) | Reference to Users table |
| Principal | decimal | Loan amount requested |
| InterestRate | decimal | Annual interest rate |
| Term | int | Loan term in months |
| Purpose | string | Loan purpose description |
| Status | string | Current loan status |
| MonthlyPayment | decimal | Calculated monthly payment |
| TotalAmount | decimal | Total amount to be repaid |
| RemainingBalance | decimal | Outstanding balance |
| AppliedAt | datetime | Application date |
| ApprovedAt | datetime | Approval date (nullable) |
| DisbursedAt | datetime | Disbursement date (nullable) |
| CompletedAt | datetime | Completion date (nullable) |
| AdditionalInfo | string | Additional loan information |

### LoanApplications Table
Loan application tracking.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique application ID |
| UserId | string (FK) | Reference to Users table |
| Principal | decimal | Requested loan amount |
| Term | int | Requested loan term |
| Purpose | string | Loan purpose |
| Status | string | Application status |
| AppliedAt | datetime | Application date |
| AdditionalInfo | string | Additional information |

### Transactions Table
Financial transaction records.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique transaction ID |
| LoanId | string (FK) | Reference to Loans table |
| UserId | string (FK) | Reference to Users table |
| Amount | decimal | Transaction amount |
| Type | string | Transaction type |
| Status | string | Transaction status |
| Description | string | Transaction description |
| CreatedAt | datetime | Transaction date |

### Payments Table
Payment processing records.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique payment ID |
| LoanId | string (FK) | Reference to Loans table |
| UserId | string (FK) | Reference to Users table |
| Amount | decimal | Payment amount |
| PaymentMethod | string | Payment method used |
| Status | string | Payment status |
| Reference | string | External reference |
| ProcessedAt | datetime | Processing date |

### RepaymentSchedules Table
Loan repayment schedule.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique schedule ID |
| LoanId | string (FK) | Reference to Loans table |
| PaymentNumber | int | Payment sequence number |
| DueDate | datetime | Payment due date |
| Amount | decimal | Payment amount |
| Status | string | Payment status |
| PaidAt | datetime | Payment date (nullable) |

### Notifications Table
User notification system.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique notification ID |
| UserId | string (FK) | Reference to Users table |
| Title | string | Notification title |
| Message | string | Notification content |
| Type | string | Notification type |
| IsRead | boolean | Read status |
| CreatedAt | datetime | Creation date |

## 🔗 Relationships

### Primary Relationships
- **Users → Loans**: One-to-Many (One user can have multiple loans)
- **Loans → Transactions**: One-to-Many (One loan can have multiple transactions)
- **Loans → Payments**: One-to-Many (One loan can have multiple payments)
- **Loans → RepaymentSchedules**: One-to-Many (One loan has multiple payment schedules)
- **Users → Notifications**: One-to-Many (One user can have multiple notifications)

### Foreign Key Constraints
- All foreign keys have proper referential integrity
- Cascade delete policies configured appropriately
- Indexes created on foreign key columns for performance

## 📈 Indexes

### Performance Indexes
- `IX_Users_Email`: Unique index on email for fast login
- `IX_Loans_UserId`: Index on UserId for user loan queries
- `IX_Loans_Status`: Index on Status for loan filtering
- `IX_Transactions_LoanId`: Index on LoanId for transaction queries
- `IX_Payments_LoanId`: Index on LoanId for payment queries
- `IX_Notifications_UserId`: Index on UserId for notification queries

## 🔒 Data Types

### String Types
- **Id fields**: nvarchar(450) - GUID strings
- **Email**: nvarchar(255) - Email addresses
- **Names**: nvarchar(255) - User and entity names
- **Descriptions**: nvarchar(max) - Long text content

### Numeric Types
- **Monetary**: decimal(18,2) - Currency amounts
- **Percentages**: decimal(5,2) - Interest rates
- **Counts**: int - Term months, payment numbers

### Date Types
- **Timestamps**: datetime2(7) - Precise timestamps
- **Dates**: date - Date-only values

## 🌱 Seed Data

### Default Admin User
```sql
INSERT INTO Users (Id, Name, Email, Phone, Role, IsActive, CreatedAt, UpdatedAt)
VALUES ('admin-id', 'System Administrator', 'admin@utilityhub360.com', '+1234567890', 'ADMIN', 1, GETDATE(), GETDATE())
```

### Loan Statuses
- PENDING: Initial application status
- APPROVED: Loan approved by admin
- REJECTED: Loan rejected by admin
- ACTIVE: Loan is active and being repaid
- COMPLETED: Loan fully repaid
- CANCELLED: Loan cancelled

### Transaction Types
- LOAN_DISBURSEMENT: Initial loan disbursement
- PAYMENT: Regular loan payment
- LATE_FEE: Late payment fee
- PENALTY: Penalty charges

## 🔧 Migration Commands

### Create Migration
```bash
dotnet ef migrations add MigrationName
```

### Update Database
```bash
dotnet ef database update
```

### Remove Last Migration
```bash
dotnet ef migrations remove
```

## 📊 Sample Queries

### Get User Loans
```sql
SELECT l.*, u.Name as UserName 
FROM Loans l 
JOIN Users u ON l.UserId = u.Id 
WHERE u.Id = @userId
```

### Get Loan Transactions
```sql
SELECT t.* 
FROM Transactions t 
WHERE t.LoanId = @loanId 
ORDER BY t.CreatedAt DESC
```

### Get Overdue Payments
```sql
SELECT rs.*, l.Principal, u.Name as UserName
FROM RepaymentSchedules rs
JOIN Loans l ON rs.LoanId = l.Id
JOIN Users u ON l.UserId = u.Id
WHERE rs.DueDate < GETDATE() 
AND rs.Status = 'PENDING'
```
