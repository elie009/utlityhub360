# Database & Environment Configuration

## Table of Contents
- [Environment Configurations](#environment-configurations)
- [Database Schema](#database-schema)
- [Entity Relationships](#entity-relationships)

---

## Environment Configurations

### Overview
The application supports three environments: **Development**, **Staging**, and **Production**. Each environment is configured through `appsettings.{Environment}.json` files.

### Environment Details

#### 1. Development Environment
- **Environment Name:** `Development`
- **Launch Profile:** "Local DB"
- **Configuration File:** `appsettings.Development.json`
- **Database Connection:**
  ```
  Server: localhost\SQLEXPRESS
  Database: DBUTILS
  User: myadmin
  Password: p@$$w0rdBAH
  ```
- **Purpose:** Local development and testing
- **Entity Framework Logging:** Information level
- **Application URL:** `https://localhost:5001` / `http://localhost:5000`

#### 2. Staging Environment
- **Environment Name:** `Staging`
- **Launch Profile:** "Test DB"
- **Configuration File:** `appsettings.Staging.json`
- **Database Connection:**
  ```
  Server: 174.138.185.18
  Database: DBUTILS
  User: sa01
  Password: iSTc0#T3tw~noz2r
  TrustServerCertificate: true
  Encrypt: true
  ```
- **⚠️ WARNING:** Currently points to the SAME database as Production
- **Purpose:** Testing before production deployment
- **Entity Framework Logging:** Information level
- **Application URL:** `https://localhost:5001` / `http://localhost:5000`

#### 3. Production Environment
- **Environment Name:** `Production`
- **Launch Profiles:** "Live DB", "http", "https", "IIS Express"
- **Configuration File:** `appsettings.Production.json`
- **Database Connection:**
  ```
  Server: 174.138.185.18
  Database: DBUTILS
  User: sa01
  Password: iSTc0#T3tw~noz2r
  TrustServerCertificate: true
  Encrypt: true
  ```
- **Purpose:** Live production environment
- **Entity Framework Logging:** Warning level
- **Application URL:** `https://localhost:5001` / `http://localhost:5000`

### Environment Selection

To switch environments, use one of these methods:

1. **Visual Studio/Rider:** Select the launch profile from the dropdown
2. **Command Line:**
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT="Development"
   dotnet run
   ```
3. **Using the switch-env script:**
   ```powershell
   .\switch-env.ps1 -Environment Development
   ```

### 🚨 Important Security Notice

**Current Configuration Issue:**
- Staging and Production environments use the SAME database (`DBUTILS` on `174.138.185.18`)
- This means testing in Staging will affect live production data
- **Recommendation:** Create a separate test database (e.g., `DBUTILS_TEST`) for Staging environment

---

## Database Schema

### Core Tables

#### 1. **Users**
Stores user account information and authentication data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique user identifier (GUID) |
| Name | string(100) | Required | User's full name |
| Email | string(255) | Required, Unique | User's email address |
| Phone | string(20) | Nullable | User's phone number |
| Role | string(20) | Required | User role (USER, ADMIN) |
| IsActive | bool | Required | Account status |
| CreatedAt | DateTime | Required | Account creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- One-to-Many with Loans
- One-to-Many with Payments
- One-to-Many with Notifications
- One-to-Many with LoanApplications
- One-to-One with UserProfile
- One-to-Many with BankAccounts
- One-to-Many with Bills
- One-to-Many with SavingsAccounts
- One-to-Many with IncomeSources

---

#### 2. **UserProfiles**
Stores detailed user profile information including employment and financial goals.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique profile identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| JobTitle | string(100) | Nullable | Current job title |
| Company | string(200) | Nullable | Employer name |
| EmploymentType | string(50) | Nullable | FULL_TIME, PART_TIME, CONTRACT, FREELANCE, SELF_EMPLOYED |
| TaxRate | decimal(5,2) | Nullable | Tax rate percentage |
| MonthlyTaxDeductions | decimal(18,2) | Nullable | Monthly tax deductions |
| MonthlySavingsGoal | decimal(18,2) | Nullable | Monthly savings target |
| MonthlyInvestmentGoal | decimal(18,2) | Nullable | Monthly investment target |
| MonthlyEmergencyFundGoal | decimal(18,2) | Nullable | Monthly emergency fund target |
| Notes | string(500) | Nullable | Additional notes |
| Industry | string(100) | Nullable | Industry sector |
| Location | string(100) | Nullable | User location |
| IsActive | bool | Required | Profile status |
| CreatedAt | DateTime | Required | Profile creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- Many-to-One with Users
- One-to-Many with IncomeSources

**Computed Properties (Not Stored):**
- TotalMonthlyIncome (calculated from IncomeSources)
- NetMonthlyIncome (TotalMonthlyIncome - MonthlyTaxDeductions)
- TotalMonthlyGoals (sum of all goal fields)

---

#### 3. **IncomeSources**
Tracks multiple income sources for users.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique income source identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| Name | string(100) | Required | Income source name |
| Amount | decimal(18,2) | Required | Income amount |
| Frequency | string(50) | Required | WEEKLY, BI_WEEKLY, MONTHLY, QUARTERLY, ANNUALLY |
| Category | string(50) | Required | PRIMARY, PASSIVE, BUSINESS, SIDE_HUSTLE, INVESTMENT, RENTAL, OTHER |
| Currency | string(10) | Required | Currency code (default: USD) |
| IsActive | bool | Required | Income source status |
| Description | string(500) | Nullable | Additional description |
| Company | string(200) | Nullable | Company name (for salary/business) |
| CreatedAt | DateTime | Required | Creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- Many-to-One with Users

**Computed Properties (Not Stored):**
- MonthlyAmount (converts any frequency to monthly equivalent)

---

#### 4. **BankAccounts**
Manages user bank accounts and financial institution connections.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique account identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| AccountName | string(255) | Required | Account display name |
| AccountType | string(50) | Required | Bank, Wallet, Credit Card, Cash, Investment |
| InitialBalance | decimal(18,2) | Required | Starting balance |
| CurrentBalance | decimal(18,2) | Required | Current balance |
| Currency | string(10) | Required | Currency code (default: USD) |
| Description | string(500) | Nullable | Account description |
| FinancialInstitution | string(100) | Nullable | Bank/institution name |
| AccountNumber | string(255) | Nullable | Masked account number |
| RoutingNumber | string(100) | Nullable | Bank routing number |
| SyncFrequency | string(50) | Required | MANUAL, DAILY, WEEKLY, MONTHLY |
| IsConnected | bool | Required | API connection status |
| ConnectionId | string(500) | Nullable | External system connection ID |
| LastSyncedAt | DateTime | Nullable | Last sync timestamp |
| Iban | string(100) | Nullable | International bank account number |
| SwiftCode | string(100) | Nullable | SWIFT/BIC code |
| IsActive | bool | Required | Account status |
| CreatedAt | DateTime | Required | Creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- Many-to-One with Users
- One-to-Many with BankTransactions
- One-to-Many with Payments

---

#### 5. **BankTransactions**
Records individual bank account transactions.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique transaction identifier (GUID) |
| BankAccountId | string(450) | Required, FK | Reference to BankAccounts.Id |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| Amount | decimal(18,2) | Required | Transaction amount |
| TransactionType | string(20) | Required | CREDIT, DEBIT |
| Description | string(255) | Required | Transaction description |
| Category | string(100) | Nullable | FOOD, TRANSPORTATION, ENTERTAINMENT, etc. |
| ReferenceNumber | string(100) | Nullable | Transaction reference |
| ExternalTransactionId | string(100) | Nullable | External system transaction ID |
| TransactionDate | DateTime | Required | Date of transaction |
| Merchant | string(100) | Nullable | Merchant/vendor name |
| Location | string(100) | Nullable | Transaction location |
| IsRecurring | bool | Required | Recurring transaction flag |
| RecurringFrequency | string(50) | Nullable | Frequency if recurring |
| Currency | string(10) | Required | Currency code (default: USD) |
| BalanceAfterTransaction | decimal(18,2) | Required | Account balance after transaction |
| Notes | string(500) | Nullable | Additional notes |
| CreatedAt | DateTime | Required | Creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- Many-to-One with BankAccounts
- Many-to-One with Users

---

#### 6. **Bills**
Manages recurring and one-time bills.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique bill identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| BillName | string(255) | Required | Bill name/description |
| BillType | string(50) | Required | utility, subscription, loan, others |
| Amount | decimal(18,2) | Required | Bill amount |
| DueDate | DateTime | Required | Payment due date |
| Frequency | string(20) | Required | monthly, quarterly, yearly |
| Status | string(20) | Required | PENDING, PAID, OVERDUE |
| Provider | string(100) | Nullable | Service provider name |
| ReferenceNumber | string(100) | Nullable | Account/subscription number |
| Notes | string(500) | Nullable | Additional notes |
| PaidAt | DateTime | Nullable | Payment timestamp |
| CreatedAt | DateTime | Required | Creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- Many-to-One with Users
- One-to-Many with Payments

---

#### 7. **Loans**
Stores loan information and tracking.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique loan identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| Principal | decimal(18,2) | Required | Original loan amount |
| InterestRate | decimal(5,2) | Required | Annual interest rate percentage |
| Term | int | Required | Loan term in months |
| Purpose | string(500) | Required | Loan purpose |
| Status | string(20) | Required | PENDING, APPROVED, REJECTED, ACTIVE, COMPLETED, DEFAULTED |
| MonthlyPayment | decimal(18,2) | Required | Monthly payment amount |
| TotalAmount | decimal(18,2) | Required | Total amount to be repaid |
| RemainingBalance | decimal(18,2) | Required | Current outstanding balance |
| NextDueDate | DateTime2 | Nullable | Next payment due date |
| FinalDueDate | DateTime2 | Nullable | Final payment due date |
| AdditionalInfo | string(1000) | Nullable | Additional information |
| AppliedAt | DateTime | Required | Application timestamp |
| ApprovedAt | DateTime | Nullable | Approval timestamp |
| DisbursedAt | DateTime | Nullable | Disbursement timestamp |
| CompletedAt | DateTime | Nullable | Completion timestamp |

**Relationships:**
- Many-to-One with Users
- One-to-Many with RepaymentSchedules
- One-to-Many with Payments

**Note:** `NextDueDate` and `FinalDueDate` were recently added to the production database.

---

#### 8. **LoanApplications**
Tracks loan application requests.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique application identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| Principal | decimal(18,2) | Required | Requested loan amount |
| Purpose | string(500) | Required | Loan purpose |
| Term | int | Required | Requested term in months |
| MonthlyIncome | decimal(18,2) | Required | Applicant's monthly income |
| EmploymentStatus | string(20) | Required | employed, self-employed, unemployed, retired, student |
| AdditionalInfo | string(1000) | Nullable | Additional information |
| Status | string(20) | Required | PENDING, APPROVED, REJECTED |
| AppliedAt | DateTime | Required | Application timestamp |
| ReviewedAt | DateTime | Nullable | Review timestamp |
| ReviewedBy | string(450) | Nullable | Admin user ID who reviewed |
| RejectionReason | string(500) | Nullable | Reason for rejection |

**Relationships:**
- Many-to-One with Users

---

#### 9. **RepaymentSchedules**
Manages loan repayment installment schedules.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique schedule identifier (GUID) |
| LoanId | string(450) | Required, FK | Reference to Loans.Id |
| InstallmentNumber | int | Required | Installment sequence number |
| DueDate | DateTime | Required | Payment due date |
| PrincipalAmount | decimal(18,2) | Required | Principal portion of payment |
| InterestAmount | decimal(18,2) | Required | Interest portion of payment |
| TotalAmount | decimal(18,2) | Required | Total installment amount |
| Status | string(20) | Required | PENDING, PAID, OVERDUE |
| PaidAt | DateTime | Nullable | Payment timestamp |

**Relationships:**
- Many-to-One with Loans

---

#### 10. **Payments**
Universal payment tracking table (supports loans, bills, bank transactions, and savings).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique payment identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| LoanId | string(450) | Nullable, FK | Reference to Loans.Id |
| BillId | string(450) | Nullable, FK | Reference to Bills.Id |
| BankAccountId | string(450) | Nullable, FK | Reference to BankAccounts.Id |
| SavingsAccountId | string(450) | Nullable, FK | Reference to SavingsAccounts.Id |
| Amount | decimal(18,2) | Required | Payment amount |
| Method | string(20) | Required | BANK_TRANSFER, CARD, WALLET, CASH |
| Reference | string(50) | Required | Payment reference number |
| Status | string(20) | Required | PENDING, COMPLETED, FAILED |
| IsBankTransaction | bool | Required | Bank transaction flag |
| TransactionType | string(20) | Nullable | CREDIT, DEBIT |
| Description | string(255) | Nullable | Payment description |
| Category | string(100) | Nullable | Payment category |
| ExternalTransactionId | string(100) | Nullable | External system transaction ID |
| Merchant | string(100) | Nullable | Merchant name |
| Location | string(100) | Nullable | Transaction location |
| IsRecurring | bool | Required | Recurring payment flag |
| RecurringFrequency | string(50) | Nullable | Frequency if recurring |
| Currency | string(10) | Required | Currency code (default: USD) |
| BalanceAfterTransaction | decimal(18,2) | Nullable | Balance after transaction |
| TransactionDate | DateTime | Nullable | Transaction date |
| Notes | string(500) | Nullable | Additional notes |
| ProcessedAt | DateTime | Required | Processing timestamp |
| CreatedAt | DateTime | Required | Creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- Many-to-One with Users
- Many-to-One with Loans (nullable)
- Many-to-One with Bills (nullable)
- Many-to-One with BankAccounts (nullable)
- Many-to-One with SavingsAccounts (nullable)

**Note:** This is a polymorphic table that handles multiple payment types based on which FK is populated.

---

#### 11. **SavingsAccounts**
Manages savings goals and accounts.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique savings account identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| AccountName | string(100) | Required | Account name |
| SavingsType | string(50) | Required | EMERGENCY, VACATION, INVESTMENT, etc. |
| TargetAmount | decimal(18,2) | Required | Target savings amount |
| CurrentBalance | decimal(18,2) | Required | Current balance |
| Currency | string(10) | Required | Currency code (default: USD) |
| Description | string(500) | Nullable | Account description |
| Goal | string(100) | Nullable | Savings goal description |
| TargetDate | DateTime | Required | Target completion date |
| IsActive | bool | Required | Account status |
| CreatedAt | DateTime | Required | Creation timestamp |
| UpdatedAt | DateTime | Required | Last update timestamp |

**Relationships:**
- Many-to-One with Users
- One-to-Many with SavingsTransactions
- One-to-Many with Payments

---

#### 12. **SavingsTransactions**
Tracks deposits and withdrawals from savings accounts.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique transaction identifier (GUID) |
| SavingsAccountId | string(450) | Required, FK | Reference to SavingsAccounts.Id |
| SourceBankAccountId | string(450) | Required, FK | Reference to BankAccounts.Id |
| Amount | decimal(18,2) | Required | Transaction amount |
| TransactionType | string(50) | Required | DEPOSIT, WITHDRAWAL, TRANSFER |
| Description | string(255) | Required | Transaction description |
| Category | string(100) | Nullable | MONTHLY_SAVINGS, BONUS, TAX_REFUND, etc. |
| Notes | string(500) | Nullable | Additional notes |
| TransactionDate | DateTime | Required | Transaction date |
| Currency | string(10) | Required | Currency code (default: USD) |
| IsRecurring | bool | Required | Recurring transaction flag |
| RecurringFrequency | string(50) | Nullable | Frequency if recurring |
| CreatedAt | DateTime | Required | Creation timestamp |

**Relationships:**
- Many-to-One with SavingsAccounts
- Many-to-One with BankAccounts (source)

---

#### 13. **Notifications**
Manages user notifications and alerts.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | string | PK | Unique notification identifier (GUID) |
| UserId | string(450) | Required, FK | Reference to Users.Id |
| Type | string(20) | Required | PAYMENT_DUE, PAYMENT_RECEIVED, LOAN_APPROVED, LOAN_REJECTED, GENERAL |
| Title | string(200) | Required | Notification title |
| Message | string(1000) | Required | Notification message |
| IsRead | bool | Required | Read status |
| CreatedAt | DateTime | Required | Creation timestamp |
| ReadAt | DateTime | Nullable | Read timestamp |

**Relationships:**
- Many-to-One with Users

---

## Entity Relationships

### Relationship Diagram

```
Users (1) ─────── (Many) UserProfiles
  │
  ├─ (Many) BankAccounts
  │      └─ (Many) BankTransactions
  │
  ├─ (Many) Bills
  │      └─ (Many) Payments
  │
  ├─ (Many) Loans
  │      ├─ (Many) RepaymentSchedules
  │      └─ (Many) Payments
  │
  ├─ (Many) LoanApplications
  │
  ├─ (Many) SavingsAccounts
  │      ├─ (Many) SavingsTransactions
  │      └─ (Many) Payments
  │
  ├─ (Many) IncomeSources
  │
  ├─ (Many) Notifications
  │
  └─ (Many) Payments
```

### Key Relationships Explained

1. **Users → UserProfiles** (One-to-One)
   - Each user has one detailed profile
   - Profile stores employment and financial goals

2. **Users → BankAccounts** (One-to-Many)
   - Each user can have multiple bank accounts
   - Accounts track balances and transactions

3. **BankAccounts → BankTransactions** (One-to-Many)
   - Each account has multiple transactions
   - Transactions update account balances

4. **Users → Loans** (One-to-Many)
   - Each user can have multiple loans
   - Tracks loan lifecycle from application to completion

5. **Loans → RepaymentSchedules** (One-to-Many)
   - Each loan has multiple installments
   - Tracks payment schedule and status

6. **Users → SavingsAccounts** (One-to-Many)
   - Each user can have multiple savings goals
   - Tracks progress toward financial targets

7. **SavingsAccounts → SavingsTransactions** (One-to-Many)
   - Each savings account has multiple transactions
   - Links to source bank accounts

8. **Payments (Polymorphic Relationships)**
   - Can relate to Loans, Bills, BankAccounts, or SavingsAccounts
   - Universal payment tracking across all modules

---

## Migration History

### Applied Migrations (in order):
1. `20250923091412_InitialCreate` - Initial database schema
2. `20250924203529_AddBillsTable` - Added Bills table
3. `20250925163640_AddBankAccountsAndTransactions` - Added bank account management
4. `20250926110333_AddSavingsSystem` - Added savings accounts
5. `20250926122822_AddUserProfileSystem` - Added user profiles
6. `20250926132932_UpdateUserProfileIncomeFields` - Updated income fields
7. `20250926134441_CreateGeneralizedIncomeSourceSystem` - Generalized income tracking
8. `20250926145159_AddIncomeSourcesTable` - Added income sources table
9. `20250927083225_MergeBankTransactionsToPayments` - Merged transaction tables
10. `20250927092117_DropTransactionTable` - Removed old transaction table
11. `20250928185255_AddBillPaymentsToPaymentTable` - Added bill payment support
12. `20250928202555_AddSavingsAccountIdToPaymentTable` - Added savings payment support

### Pending/Manual Migrations:
- `20251009000000_AddLoanDueDateColumns` - Added NextDueDate and FinalDueDate to Loans table
  - **Status:** Applied directly to production database via SQL script
  - **Date Applied:** October 9, 2025

---

## Connection String Format

### Standard Format:
```
Server={server_address};
Database={database_name};
User Id={username};
Password={password};
TrustServerCertificate=true;
MultipleActiveResultSets=true;
Encrypt=true;
```

### Local Development:
```
Server=localhost\SQLEXPRESS;
Database=DBUTILS;
User Id=myadmin;
Password=p@$$w0rdBAH;
TrustServerCertificate=true;
MultipleActiveResultSets=true
```

### Remote (Staging/Production):
```
Server=174.138.185.18;
Database=DBUTILS;
User Id=sa01;
Password=iSTc0#T3tw~noz2r;
TrustServerCertificate=true;
MultipleActiveResultSets=true;
Encrypt=true;
```

---

## Database Maintenance

### Applying Migrations

**Development:**
```powershell
dotnet ef database update
```

**Staging:**
```powershell
$env:ASPNETCORE_ENVIRONMENT="Staging"
dotnet ef database update
```

**Production:**
```powershell
dotnet ef database update --connection "Server=174.138.185.18;Database=DBUTILS;User Id=sa01;Password=iSTc0#T3tw~noz2r;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=true;"
```

### Creating New Migrations

```powershell
dotnet ef migrations add MigrationName
```

### Removing Last Migration (if not applied)

```powershell
dotnet ef migrations remove
```

---

## Security Recommendations

1. **Separate Staging Database**
   - Create `DBUTILS_TEST` database for staging environment
   - Update `appsettings.Staging.json` to use test database

2. **Connection String Security**
   - Move production credentials to Azure Key Vault or similar
   - Use environment variables for sensitive data
   - Never commit credentials to source control

3. **User Roles**
   - Implement proper role-based access control
   - Separate admin and user permissions
   - Regular security audits

4. **Database Backups**
   - Schedule regular automated backups
   - Test restore procedures
   - Maintain backup retention policy

---

## Last Updated
October 9, 2025

## Version
1.0

