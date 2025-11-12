# 🚨 QUICK FIX: Database Error

## The Error You're Seeing:
```
Failed to get user loans: Invalid column name 'ActualFinancedAmount'...
```

## ✅ Solution: Run SQL Script (Takes 30 seconds)

### Step 1: Open SQL Server Management Studio (SSMS)
- Or use Azure Data Studio, or any SQL tool

### Step 2: Connect to Your Database
- Use the same connection string as your application

### Step 3: Copy and Paste This SQL:

```sql
ALTER TABLE [dbo].[Loans]
ADD [InterestComputationMethod] [nvarchar](20) NOT NULL DEFAULT 'AMORTIZED',
    [TotalInterest] [decimal](18,2) NOT NULL DEFAULT 0,
    [DownPayment] [decimal](18,2) NOT NULL DEFAULT 0,
    [ProcessingFee] [decimal](18,2) NOT NULL DEFAULT 0,
    [ActualFinancedAmount] [decimal](18,2) NOT NULL DEFAULT 0,
    [PaymentFrequency] [nvarchar](20) NOT NULL DEFAULT 'MONTHLY',
    [StartDate] [datetime2](7) NULL;

UPDATE [dbo].[Loans]
SET [ActualFinancedAmount] = [Principal]
WHERE [ActualFinancedAmount] = 0 AND [Principal] > 0;
```

### Step 4: Execute the Script (F5 or Execute button)

### Step 5: Restart Your Application

## ✅ Done! Error Should Be Fixed

---

## Alternative: Run the SQL File

1. Open `FIX_DATABASE_ERROR.sql` (in project root)
2. Copy the SQL
3. Paste into SSMS
4. Execute

---

## Still Having Issues?

1. **Make sure you're connected to the correct database**
2. **Check your table name** - It should be `Loans` (case-sensitive in some setups)
3. **Verify permissions** - Your SQL user needs ALTER TABLE permission

---

**That's it! This will fix the error immediately.**


