# Database Documentation

This directory contains database migration scripts and setup documentation.

## Structure

- **Scripts/** - SQL migration scripts for various features
- **Migrations/** - Entity Framework Core migration files
- **README_Database_Setup.md** - Database setup instructions

## Important Migration Scripts

### Month Closing Feature
- `create_closed_months_table_final.sql` - Creates the ClosedMonths table for month closing functionality

### Other Scripts
All other SQL scripts in the Scripts/ directory are historical migration scripts. Refer to the Migrations/ directory for current EF Core migrations.

## Usage

1. For new features, use Entity Framework Core migrations: `dotnet ef migrations add <MigrationName>`
2. For manual SQL scripts, review and test in a development environment first
3. Always backup your database before running migration scripts

---

## Month Closing Feature Documentation

### Overview

The Month Closing feature allows users to lock specific months for bank accounts, preventing any new transactions or modifications to transactions in closed months. This ensures data integrity and maintains accurate financial records for completed accounting periods.

### Database Schema

#### ClosedMonths Table

**Table Name:** `ClosedMonths`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | NVARCHAR(450) | PRIMARY KEY | Unique identifier for the closed month record |
| `BankAccountId` | NVARCHAR(450) | NOT NULL | Reference to the bank account |
| `Year` | INT | NOT NULL, CHECK (2000-2100) | Year of the closed month |
| `Month` | INT | NOT NULL, CHECK (1-12) | Month number (1=January, 12=December) |
| `ClosedBy` | NVARCHAR(450) | NOT NULL | User ID who closed the month |
| `ClosedAt` | DATETIME2 | NOT NULL | Timestamp when the month was closed |
| `Notes` | NVARCHAR(500) | NULL | Optional notes about the closure |
| `CreatedAt` | DATETIME2 | NOT NULL | Record creation timestamp |

#### Indexes

1. **IX_ClosedMonths_BankAccountId_Year_Month** (UNIQUE)
   - Columns: `BankAccountId`, `Year`, `Month`
   - Ensures only one closed month record per bank account per month/year combination

2. **IX_ClosedMonths_BankAccountId**
   - Column: `BankAccountId`
   - Optimizes queries filtering by bank account

3. **IX_ClosedMonths_Year_Month**
   - Columns: `Year`, `Month`
   - Optimizes queries filtering by date range

4. **IX_ClosedMonths_ClosedAt**
   - Column: `ClosedAt`
   - Optimizes queries sorting by closure date

#### Constraints

- **CK_ClosedMonths_Month_Range**: Ensures month is between 1 and 12
- **CK_ClosedMonths_Year_Range**: Ensures year is between 2000 and 2100
- **PK_ClosedMonths**: Primary key on `Id` column

### Entity Framework Model

**Entity:** `ClosedMonth` (namespace: `UtilityHub360.Entities`)

```csharp
public class ClosedMonth
{
    public string Id { get; set; }
    public string BankAccountId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
    public string ClosedBy { get; set; } // User ID
    public DateTime ClosedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual BankAccount BankAccount { get; set; }
    public virtual User User { get; set; }
}
```

### API Endpoints

#### 1. Close a Month

**Endpoint:** `POST /api/BankAccounts/{bankAccountId}/close-month`

**Request Body:**
```json
{
  "year": 2024,
  "month": 10,
  "notes": "October 2024 closed after reconciliation"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Month closed successfully",
  "data": {
    "id": "guid",
    "bankAccountId": "guid",
    "bankAccountName": "Checking Account",
    "year": 2024,
    "month": 10,
    "monthName": "October",
    "closedBy": "user-id",
    "closedByName": "John Doe",
    "closedAt": "2024-11-01T10:30:00Z",
    "notes": "October 2024 closed after reconciliation",
    "createdAt": "2024-11-01T10:30:00Z"
  }
}
```

#### 2. Get Closed Months

**Endpoint:** `GET /api/BankAccounts/{bankAccountId}/closed-months`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "bankAccountId": "guid",
      "bankAccountName": "Checking Account",
      "year": 2024,
      "month": 10,
      "monthName": "October",
      "closedBy": "user-id",
      "closedByName": "John Doe",
      "closedAt": "2024-11-01T10:30:00Z",
      "notes": "October 2024 closed after reconciliation",
      "createdAt": "2024-11-01T10:30:00Z"
    }
  ]
}
```

#### 3. Check if Month is Closed

**Service Method:** `IsMonthClosedAsync(string bankAccountId, int year, int month, string userId)`

Returns a boolean indicating whether the specified month is closed for the bank account.

### Business Rules

1. **Uniqueness**: Only one closed month record can exist per bank account per month/year combination
2. **Future Month Prevention**: Cannot close a future month (month/year must be <= current date)
3. **Transaction Blocking**: Once a month is closed, the system prevents:
   - Creating new bank transactions in that month
   - Modifying existing transactions in that month
   - Updating transaction dates to fall within a closed month
4. **User Authorization**: Only the owner of the bank account can close months for that account
5. **Validation**: Month must be between 1-12, Year must be between 2000-2100

### Implementation Details

#### Transaction Protection

The system checks for closed months in the following scenarios:

1. **Creating Bank Transactions** (`BankAccountService.CreateTransactionAsync`)
   - Validates that the transaction date's month/year is not closed
   - Returns error: "Cannot create transaction. The month {MonthName} {Year} is closed for this account."

2. **Updating Bank Transactions** (`BankAccountService.UpdateTransactionAsync`)
   - Validates both old and new transaction dates
   - Prevents moving transactions into closed months
   - Prevents modifying transactions in closed months

3. **Payment Processing** (`PaymentService`)
   - Checks month closure status before processing payments

#### Service Methods

**BankAccountService:**
- `CloseMonthAsync(bankAccountId, closeMonthDto, userId)` - Closes a month for a bank account
- `GetClosedMonthsAsync(bankAccountId, userId)` - Retrieves all closed months for a bank account
- `IsMonthClosedAsync(bankAccountId, year, month, userId)` - Checks if a specific month is closed

### Usage Scenarios

1. **Monthly Reconciliation**: Close months after completing bank reconciliation to prevent accidental changes
2. **Period-End Locking**: Lock completed accounting periods to maintain financial integrity
3. **Audit Trail**: Maintain a record of when months were closed and by whom
4. **Compliance**: Ensure financial records remain unchanged for regulatory compliance

### Error Handling

Common error scenarios:

- **Month Already Closed**: Returns error if attempting to close an already closed month
- **Future Month**: Returns error if attempting to close a future month
- **Invalid Month/Year**: Returns error if month is not 1-12 or year is not 2000-2100
- **Bank Account Not Found**: Returns error if bank account doesn't exist or doesn't belong to user
- **Transaction in Closed Month**: Returns error when attempting to create/modify transactions in closed months

### Database Migration

To apply the ClosedMonths table to your database:

1. Run the SQL script: `create_closed_months_table_final.sql`
2. Or use Entity Framework Core migrations (if migration exists)

**Note:** The SQL script creates the table and indexes without foreign keys. Foreign keys are optional and can be added later if needed for referential integrity.

### Related Files

- **Entity**: `UtilityHub360/Entities/ClosedMonth.cs`
- **DTO**: `UtilityHub360/DTOs/ClosedMonthDto.cs`
- **Service**: `UtilityHub360/Services/BankAccountService.cs` (methods: CloseMonthAsync, GetClosedMonthsAsync, IsMonthClosedAsync)
- **Controller**: `UtilityHub360/Controllers/BankAccountsController.cs` (endpoints: CloseMonth, GetClosedMonths)
- **SQL Script**: `Documentation/Database/create_closed_months_table_final.sql`

