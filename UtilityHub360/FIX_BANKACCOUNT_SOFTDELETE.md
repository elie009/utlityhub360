# Fix Soft Delete Columns Error

## Problem
The error "Invalid column name 'DeleteReason', 'DeletedAt', 'DeletedBy', 'IsDeleted'" occurs because several entities have soft delete properties defined, but the database tables don't have these columns yet.

**Affected Entities:**
- `BankAccount`
- `BankTransaction`
- `Payment`
- `SavingsAccount`
- `SavingsTransaction`
- `IncomeSource`
- `VariableExpense`

## Temporary Fix Applied
I've updated `ApplicationDbContext.cs` to ignore these properties for all affected entities temporarily. This will prevent the error from occurring.

## Permanent Fix
To permanently fix this issue, you need to run the SQL migration script to add the soft delete columns to the database:

### Option 1: Run the SQL Script Directly
1. Connect to your SQL Server database
2. Run the script: `apply_soft_delete_migration.sql`

**This script will add soft delete columns to multiple tables:**
- BankAccounts
- BankTransactions
- Payments
- Bills
- IncomeSources
- VariableExpenses
- SavingsTransactions

For each table, it adds:
- `IsDeleted` (bit NOT NULL DEFAULT 0)
- `DeletedAt` (datetime2 NULL)
- `DeletedBy` (nvarchar(450) NULL)
- `DeleteReason` (nvarchar(500) NULL)

### Option 2: Using SQL Server Management Studio (SSMS)
1. Open SSMS
2. Connect to your database server: `174.138.185.18`
3. Database: `DBUTILS`
4. Open the file `apply_soft_delete_migration.sql`
5. Execute the script

### After Running the Migration
Once you've added the columns to the database:
1. Remove all the `Ignore()` calls from `ApplicationDbContext.cs` for the following entities:
   - BankAccount (lines 157-160)
   - BankTransaction (lines 179-184)
   - Payment (lines 115-120)
   - SavingsAccount (lines 204-209)
   - SavingsTransaction (lines 227-232)
   - IncomeSource (lines 262-267)
   - VariableExpense (lines 283-288)
2. The soft delete functionality will then work properly

## Verification
After running the migration, verify the columns exist for each table:
```sql
-- Check BankAccounts
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'BankAccounts'
AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')

-- Check other tables
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('BankTransactions', 'Payments', 'SavingsAccounts', 'SavingsTransactions', 'IncomeSources', 'VariableExpenses')
AND COLUMN_NAME IN ('IsDeleted', 'DeletedAt', 'DeletedBy', 'DeleteReason')
ORDER BY TABLE_NAME, COLUMN_NAME
```

You should see all 4 columns listed for each affected table.

