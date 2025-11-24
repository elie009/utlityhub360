# 🚀 Run Savings Account Migration - Quick Guide

## The Problem
You're getting these errors:
```
Msg 207, Level 16, State 1, Line 26
Invalid column name 'AccountType'.

Msg 207, Level 16, State 1, Line 38
Invalid column name 'NextInterestCalculationDate'.
```

This means the new columns haven't been added to your database yet.

---

## ✅ Solution: Run the Migration Script

### Option 1: Using SQL Server Management Studio (SSMS) - RECOMMENDED

1. **Open SQL Server Management Studio**
2. **Connect to your database** (the one specified in your connection string)
3. **Open the migration script:**
   - File: `add_savings_account_type_and_interest_SAFE.sql`
   - Location: `Utilityhub360-backend/UtilityHub360/add_savings_account_type_and_interest_SAFE.sql`
4. **Execute the script** (Press F5 or click Execute)
5. **Verify success** - You should see messages like:
   ```
   AccountType column added successfully.
   InterestRate column added successfully.
   ...
   Migration completed successfully.
   ```

### Option 2: Using Azure Data Studio

1. **Open Azure Data Studio**
2. **Connect to your database**
3. **Open the migration script** (`add_savings_account_type_and_interest_SAFE.sql`)
4. **Run the script** (Press F5)

### Option 3: Using Command Line (sqlcmd)

**For Windows:**
```powershell
# Navigate to the project directory
cd D:\PROJECT\REPOSITORY\UtilityHub360\Utilityhub360-backend\UtilityHub360

# Run the script (replace with your connection details)
sqlcmd -S localhost -d DBUTILS -U sa01 -P "your_password" -i add_savings_account_type_and_interest_SAFE.sql
```

**For Linux/Mac:**
```bash
sqlcmd -S localhost -d DBUTILS -U sa01 -P "your_password" -i add_savings_account_type_and_interest_SAFE.sql
```

---

## 🔍 Verify Migration Success

After running the script, verify the columns exist:

```sql
-- Check if columns exist
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

You should see all 5 columns listed.

---

## 🎯 After Migration

1. **Restart your application** - The new columns will now be recognized
2. **Test creating a savings account** with account type and interest rate:
   ```json
   POST /api/savings
   {
     "accountName": "High-Yield Savings",
     "savingsType": "EMERGENCY",
     "accountType": "HIGH_YIELD",
     "interestRate": 0.045,
     "interestCompoundingFrequency": "MONTHLY",
     "targetAmount": 10000,
     "targetDate": "2025-12-31"
   }
   ```

---

## ⚠️ Important Notes

- **The SAFE script** checks if columns exist before adding them, so it's safe to run multiple times
- **Existing data is preserved** - All existing savings accounts will have `AccountType = 'REGULAR'` by default
- **No data loss** - All existing savings accounts and transactions remain intact

---

## 🆘 If You Still Get Errors

1. **Check your connection string** - Make sure you're connected to the correct database
2. **Verify table name** - Ensure your table is named `SavingsAccounts` (case-sensitive in some databases)
3. **Check permissions** - Make sure your database user has ALTER TABLE permissions
4. **Run the verification query** above to see what columns exist

---

## 📋 What This Migration Adds

- ✅ `AccountType` - Type of savings account (HIGH_YIELD, CD, MONEY_MARKET, REGULAR)
- ✅ `InterestRate` - Annual interest rate (e.g., 0.045 for 4.5%)
- ✅ `InterestCompoundingFrequency` - How often interest compounds (DAILY, MONTHLY, QUARTERLY, ANNUALLY)
- ✅ `LastInterestCalculationDate` - When interest was last calculated
- ✅ `NextInterestCalculationDate` - When interest should be calculated next
- ✅ Indexes for performance

---

**After running the migration, your application should work without errors!** ✅

