# 🔧 Troubleshoot Migration Errors

## The Error You're Seeing

```
Msg 207, Level 16, State 1, Line 26
Invalid column name 'AccountType'.
```

This means the ALTER TABLE statements are failing, so the columns aren't being created.

---

## 🔍 Diagnostic Steps

### Step 1: Check if Table Exists

Run this query first:

```sql
SELECT 
    TABLE_NAME,
    TABLE_SCHEMA
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%Savings%'
ORDER BY TABLE_NAME;
```

**Expected:** You should see `SavingsAccounts` listed.

**If not found:** Your table might have a different name or be in a different schema.

---

### Step 2: Check Current Columns

```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SavingsAccounts'
ORDER BY ORDINAL_POSITION;
```

This shows what columns currently exist.

---

### Step 3: Check Permissions

```sql
-- Check if you have ALTER TABLE permission
SELECT HAS_PERMS_BY_NAME('SavingsAccounts', 'OBJECT', 'ALTER') AS HasAlterPermission;
```

**Expected:** Should return `1` (true)

**If 0:** You need ALTER TABLE permissions. Contact your DBA.

---

## ✅ Solutions

### Solution 1: Use the SIMPLE Script (RECOMMENDED)

Run this file: `add_savings_columns_SIMPLE.sql`

This script:
- ✅ Adds columns one at a time
- ✅ Checks if each column exists before adding
- ✅ Safe to run multiple times
- ✅ No GO statements needed

### Solution 2: Use the WORKING Script

Run this file: `add_savings_account_type_and_interest_WORKING.sql`

This script:
- ✅ Uses GO statements to separate batches
- ✅ Checks table existence first
- ✅ Proper error handling

### Solution 3: Manual Column Addition

If scripts still fail, add columns manually one by one:

```sql
-- Run these one at a time, checking for errors after each

-- 1. Add AccountType
ALTER TABLE SavingsAccounts ADD AccountType NVARCHAR(50) NULL;
GO

-- 2. Add InterestRate
ALTER TABLE SavingsAccounts ADD InterestRate DECIMAL(5,4) NULL;
GO

-- 3. Add InterestCompoundingFrequency
ALTER TABLE SavingsAccounts ADD InterestCompoundingFrequency NVARCHAR(50) NULL;
GO

-- 4. Add LastInterestCalculationDate
ALTER TABLE SavingsAccounts ADD LastInterestCalculationDate DATETIME2 NULL;
GO

-- 5. Add NextInterestCalculationDate
ALTER TABLE SavingsAccounts ADD NextInterestCalculationDate DATETIME2 NULL;
GO

-- 6. Set defaults
UPDATE SavingsAccounts SET AccountType = 'REGULAR' WHERE AccountType IS NULL;
GO

-- 7. Create indexes
CREATE INDEX IX_SavingsAccounts_NextInterestCalculationDate 
ON SavingsAccounts(NextInterestCalculationDate) 
WHERE NextInterestCalculationDate IS NOT NULL;
GO

CREATE INDEX IX_SavingsAccounts_AccountType 
ON SavingsAccounts(AccountType) 
WHERE AccountType IS NOT NULL;
GO
```

---

## 🆘 Common Issues

### Issue 1: Table Name Case Sensitivity

**Problem:** Table might be `savingsAccounts` or `SAVINGSACCOUNTS`

**Solution:** Check exact table name:
```sql
SELECT name FROM sys.tables WHERE name LIKE '%saving%';
```

### Issue 2: Schema Issue

**Problem:** Table might be in a different schema (e.g., `dbo.SavingsAccounts`)

**Solution:** Use fully qualified name:
```sql
ALTER TABLE dbo.SavingsAccounts ADD AccountType NVARCHAR(50) NULL;
```

### Issue 3: Transaction Lock

**Problem:** Another process has the table locked

**Solution:** 
1. Check for active connections:
```sql
SELECT * FROM sys.dm_exec_sessions WHERE database_id = DB_ID();
```
2. Kill blocking sessions if needed
3. Retry the migration

### Issue 4: Database Compatibility Level

**Problem:** Database compatibility level too low

**Solution:** Check and update:
```sql
-- Check compatibility
SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME();

-- Update if needed (SQL Server 2019+)
ALTER DATABASE [YourDatabaseName] SET COMPATIBILITY_LEVEL = 150;
```

---

## ✅ Verification After Migration

After running the migration, verify success:

```sql
-- Should return 5 rows
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
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

**Expected Result:** All 5 columns should be listed.

---

## 📞 Still Having Issues?

If none of these solutions work:

1. **Check SQL Server Error Log:**
   ```sql
   EXEC xp_readerrorlog;
   ```

2. **Try with a different user** (with DBO permissions)

3. **Check if table has constraints** that might prevent ALTER:
   ```sql
   SELECT * FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('SavingsAccounts');
   ```

4. **Contact your DBA** with the error messages

---

**The SIMPLE script should work in most cases!** ✅

