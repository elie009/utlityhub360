# 🔧 Fix Migration Error - Step by Step

## The Problem

You're getting these errors when running the migration:
```
Msg 207, Level 16, State 1, Line 26
Invalid column name 'AccountType'.

Msg 207, Level 16, State 1, Line 38
Invalid column name 'NextInterestCalculationDate'.

Msg 207, Level 16, State 1, Line 43
Invalid column name 'AccountType'.
```

**Why this happens:** The script tries to use columns (in UPDATE and CREATE INDEX statements) before confirming they were created successfully.

---

## ✅ Solution: Use the FIXED Script

### Step 1: Use the Fixed Migration Script

**Run this file instead:**
```
add_savings_account_type_and_interest_FIXED.sql
```

This fixed version:
- ✅ Checks if columns exist before adding them
- ✅ Only runs UPDATE statements if columns exist
- ✅ Only creates indexes if columns exist
- ✅ Uses transactions for safety (rolls back on error)
- ✅ Provides clear error messages

### Step 2: Run the Script

**Option A: SQL Server Management Studio**
1. Open SSMS
2. Connect to your database
3. Open `add_savings_account_type_and_interest_FIXED.sql`
4. Execute (F5)

**Option B: Command Line**
```powershell
sqlcmd -S your_server -d your_database -U your_username -P your_password -i add_savings_account_type_and_interest_FIXED.sql
```

---

## 🔍 Verify the Fix

After running the fixed script, verify columns exist:

```sql
-- Check all columns
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SavingsAccounts'
AND COLUMN_NAME IN (
    'AccountType',
    'InterestRate',
    'InterestCompoundingFrequency',
    'LastInterestCalculationDate',
    'NextInterestCalculationDate'
)
ORDER BY COLUMN_NAME;
```

**Expected Result:** You should see all 5 columns listed.

---

## 🆘 If You Still Get Errors

### Check 1: Table Name
Make sure your table is named exactly `SavingsAccounts` (case-sensitive in some databases):

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Savings%';
```

### Check 2: Permissions
Make sure your database user has ALTER TABLE permissions:

```sql
-- Check your permissions
SELECT 
    pr.name AS principal_name,
    pr.type_desc AS principal_type,
    pe.permission_name,
    pe.state_desc
FROM sys.database_permissions pe
INNER JOIN sys.database_principals pr ON pe.grantee_principal_id = pr.principal_id
WHERE pr.name = USER_NAME()
AND pe.major_id = OBJECT_ID('SavingsAccounts');
```

### Check 3: Manual Column Addition
If the script still fails, add columns one by one:

```sql
-- Add columns manually
ALTER TABLE SavingsAccounts ADD AccountType NVARCHAR(50) NULL;
ALTER TABLE SavingsAccounts ADD InterestRate DECIMAL(5,4) NULL;
ALTER TABLE SavingsAccounts ADD InterestCompoundingFrequency NVARCHAR(50) NULL;
ALTER TABLE SavingsAccounts ADD LastInterestCalculationDate DATETIME2 NULL;
ALTER TABLE SavingsAccounts ADD NextInterestCalculationDate DATETIME2 NULL;

-- Set defaults
UPDATE SavingsAccounts SET AccountType = 'REGULAR' WHERE AccountType IS NULL;

-- Create indexes
CREATE INDEX IX_SavingsAccounts_NextInterestCalculationDate 
ON SavingsAccounts(NextInterestCalculationDate) 
WHERE NextInterestCalculationDate IS NOT NULL;

CREATE INDEX IX_SavingsAccounts_AccountType 
ON SavingsAccounts(AccountType) 
WHERE AccountType IS NOT NULL;
```

---

## ✅ After Successful Migration

1. **Restart your application** - The new columns will be recognized
2. **Test the API:**
   ```json
   POST /api/savings
   {
     "accountName": "Test High-Yield",
     "savingsType": "EMERGENCY",
     "accountType": "HIGH_YIELD",
     "interestRate": 0.045,
     "interestCompoundingFrequency": "MONTHLY",
     "targetAmount": 10000,
     "targetDate": "2025-12-31"
   }
   ```

---

## 📋 What Changed in the Fixed Script

1. **Added transaction wrapper** - Ensures all-or-nothing execution
2. **Added column existence checks** - Before every ALTER TABLE
3. **Added column checks before UPDATE** - Only updates if column exists
4. **Added column checks before CREATE INDEX** - Only creates index if column exists
5. **Added error handling** - Catches and reports errors clearly
6. **Added WAITFOR DELAY** - Ensures columns are committed before use

---

**The fixed script should work without errors!** ✅

