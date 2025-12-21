# Database Schema

## 📊 Entity Relationship Diagram

```
Users (1) ←→ (N) Loans (1) ←→ (N) Transactions
    ↓              ↓              ↓
    ↓              ↓              ↓
Notifications  RepaymentSchedules  Payments

Users (1) ←→ (N) Investments (1) ←→ (N) InvestmentPositions
    ↓                                  ↓
    ↓                                  ↓
    └────────→ (N) InvestmentTransactions
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

### Investments Table
Investment account tracking (brokerage, 401(k), IRA, etc.).

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique investment account identifier |
| UserId | string (FK) | Reference to Users table |
| AccountName | string | Name of the investment account |
| InvestmentType | string | Type: STOCK, BOND, MUTUAL_FUND, ETF, CRYPTO, REAL_ESTATE, OTHER |
| AccountType | string | Account type: BROKERAGE, RETIREMENT_401K, RETIREMENT_IRA, TAXABLE, etc. |
| BrokerName | string | Broker name (Fidelity, Vanguard, etc.) |
| AccountNumber | string | Masked account number |
| InitialInvestment | decimal | Initial investment amount |
| CurrentValue | decimal | Current account value |
| TotalCostBasis | decimal | Total amount invested |
| UnrealizedGainLoss | decimal | Current value - Cost basis |
| RealizedGainLoss | decimal | Gains/losses from sold positions |
| TotalReturnPercentage | decimal | ((Current Value - Cost Basis) / Cost Basis) * 100 |
| Currency | string | Currency code (default: USD) |
| Description | string | Account description |
| IsActive | boolean | Account active status |
| CreatedAt | datetime | Account creation date |
| UpdatedAt | datetime | Last update timestamp |
| IsDeleted | boolean | Soft delete flag |
| DeletedAt | datetime | Deletion timestamp (nullable) |
| DeletedBy | string | User who deleted (nullable) |
| DeleteReason | string | Reason for deletion (nullable) |

### InvestmentPositions Table
Individual positions/holdings within investment accounts.

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique position identifier |
| InvestmentId | string (FK) | Reference to Investments table |
| Symbol | string | Stock ticker, fund symbol, etc. |
| Name | string | Company name, fund name, etc. |
| AssetType | string | Type: STOCK, BOND, MUTUAL_FUND, ETF, CRYPTO, REAL_ESTATE, OTHER |
| Quantity | decimal | Number of shares/units |
| AverageCostBasis | decimal | Average price per share/unit |
| TotalCostBasis | decimal | Quantity * AverageCostBasis |
| CurrentPrice | decimal | Current market price per share/unit |
| CurrentValue | decimal | Quantity * CurrentPrice |
| UnrealizedGainLoss | decimal | CurrentValue - TotalCostBasis |
| GainLossPercentage | decimal | (UnrealizedGainLoss / TotalCostBasis) * 100 |
| DividendsReceived | decimal | Total dividends received for this position |
| InterestReceived | decimal | Total interest received (for bonds) |
| LastPriceUpdate | datetime | Last price update timestamp (nullable) |
| CreatedAt | datetime | Position creation date |
| UpdatedAt | datetime | Last update timestamp |

### InvestmentTransactions Table
Investment transactions (buy, sell, dividend, etc.).

| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique transaction identifier |
| InvestmentId | string (FK) | Reference to Investments table |
| PositionId | string (FK) | Reference to InvestmentPositions table (nullable) |
| TransactionType | string | Type: BUY, SELL, DIVIDEND, INTEREST, DEPOSIT, WITHDRAWAL, FEE, SPLIT, MERGER |
| Symbol | string | Stock ticker, fund symbol, etc. |
| Name | string | Company name, fund name, etc. (nullable) |
| Quantity | decimal | Number of shares/units (nullable) |
| PricePerShare | decimal | Price per share/unit (nullable) |
| Amount | decimal | Total transaction amount |
| Fees | decimal | Transaction fees (nullable) |
| Taxes | decimal | Taxes on transaction (nullable) |
| Currency | string | Currency code (default: USD) |
| Description | string | Transaction description (nullable) |
| Reference | string | External transaction reference (nullable) |
| TransactionDate | datetime | Transaction date |
| CreatedAt | datetime | Record creation date |
| IsDeleted | boolean | Soft delete flag |
| DeletedAt | datetime | Deletion timestamp (nullable) |
| DeletedBy | string | User who deleted (nullable) |
| DeleteReason | string | Reason for deletion (nullable) |

## 🔗 Relationships

### Primary Relationships
- **Users → Loans**: One-to-Many (One user can have multiple loans)
- **Loans → Transactions**: One-to-Many (One loan can have multiple transactions)
- **Loans → Payments**: One-to-Many (One loan can have multiple payments)
- **Loans → RepaymentSchedules**: One-to-Many (One loan has multiple payment schedules)
- **Users → Notifications**: One-to-Many (One user can have multiple notifications)
- **Users → Investments**: One-to-Many (One user can have multiple investment accounts)
- **Investments → InvestmentPositions**: One-to-Many (One investment account can have multiple positions)
- **Investments → InvestmentTransactions**: One-to-Many (One investment account can have multiple transactions)
- **InvestmentPositions → InvestmentTransactions**: One-to-Many (One position can have multiple transactions, optional)

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
- `IX_Investments_UserId`: Index on UserId for user investment queries
- `IX_Investments_IsActive`: Index on IsActive for filtering active accounts
- `IX_InvestmentPositions_InvestmentId`: Index on InvestmentId for position queries
- `IX_InvestmentPositions_Symbol`: Index on Symbol for symbol-based queries
- `IX_InvestmentTransactions_InvestmentId`: Index on InvestmentId for transaction queries
- `IX_InvestmentTransactions_TransactionDate`: Index on TransactionDate for date-based queries
- `IX_InvestmentTransactions_TransactionType`: Index on TransactionType for type filtering

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

### Get User Investment Accounts
```sql
SELECT i.*, u.Name as UserName 
FROM Investments i 
JOIN Users u ON i.UserId = u.Id 
WHERE i.UserId = @userId 
AND i.IsDeleted = 0
ORDER BY i.CreatedAt DESC
```

### Get Investment Positions
```sql
SELECT ip.*, i.AccountName 
FROM InvestmentPositions ip 
JOIN Investments i ON ip.InvestmentId = i.Id 
WHERE ip.InvestmentId = @investmentId
ORDER BY ip.CurrentValue DESC
```

### Get Investment Transactions
```sql
SELECT it.*, i.AccountName 
FROM InvestmentTransactions it 
JOIN Investments i ON it.InvestmentId = i.Id 
WHERE it.InvestmentId = @investmentId 
AND it.IsDeleted = 0
ORDER BY it.TransactionDate DESC
```

### Get Investment Performance Summary
```sql
SELECT 
    i.AccountName,
    i.CurrentValue,
    i.TotalCostBasis,
    i.UnrealizedGainLoss,
    i.RealizedGainLoss,
    i.TotalReturnPercentage,
    COUNT(DISTINCT ip.Id) as PositionCount,
    COUNT(DISTINCT it.Id) as TransactionCount
FROM Investments i
LEFT JOIN InvestmentPositions ip ON i.Id = ip.InvestmentId
LEFT JOIN InvestmentTransactions it ON i.Id = it.InvestmentId AND it.IsDeleted = 0
WHERE i.UserId = @userId 
AND i.IsDeleted = 0
GROUP BY i.Id, i.AccountName, i.CurrentValue, i.TotalCostBasis, 
         i.UnrealizedGainLoss, i.RealizedGainLoss, i.TotalReturnPercentage
```
