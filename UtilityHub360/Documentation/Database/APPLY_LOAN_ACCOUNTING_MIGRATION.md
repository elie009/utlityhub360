# 🚀 Apply Loan Accounting Migration

## ⚠️ **URGENT: Fix Database Error**

You're getting this error because the new Loan accounting fields haven't been added to the database yet:

```
Invalid column name 'ActualFinancedAmount'
Invalid column name 'DownPayment'
Invalid column name 'InterestComputationMethod'
Invalid column name 'PaymentFrequency'
Invalid column name 'ProcessingFee'
Invalid column name 'StartDate'
Invalid column name 'TotalInterest'
```

---

## ✅ **Solution: Run SQL Script**

### **Option 1: Run SQL Script Directly (RECOMMENDED - Fastest)**

1. **Stop your application** (if running)
2. Open SQL Server Management Studio (SSMS) or your SQL tool
3. Connect to your database
4. Run this SQL script:

```sql
-- Add Loan Accounting Fields
ALTER TABLE [dbo].[Loans]
ADD [InterestComputationMethod] [nvarchar](20) NOT NULL DEFAULT 'AMORTIZED',
    [TotalInterest] [decimal](18,2) NOT NULL DEFAULT 0,
    [DownPayment] [decimal](18,2) NOT NULL DEFAULT 0,
    [ProcessingFee] [decimal](18,2) NOT NULL DEFAULT 0,
    [ActualFinancedAmount] [decimal](18,2) NOT NULL DEFAULT 0,
    [PaymentFrequency] [nvarchar](20) NOT NULL DEFAULT 'MONTHLY',
    [StartDate] [datetime2](7) NULL;

-- Update existing loans
UPDATE [dbo].[Loans]
SET [ActualFinancedAmount] = [Principal]
WHERE [ActualFinancedAmount] = 0 AND [Principal] > 0;
```

**Or use the complete script:**
- Location: `Documentation/Database/Scripts/add_loan_accounting_fields.sql`
- This script includes error checking and will skip columns that already exist

### **Option 2: Create Journal Entry Tables**

Run this script to create the journal entry tables:

```sql
-- Create JournalEntries table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JournalEntries] (
        [Id] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [LoanId] [nvarchar](450) NULL,
        [EntryType] [nvarchar](50) NOT NULL,
        [EntryDate] [datetime2](7) NOT NULL,
        [Description] [nvarchar](500) NOT NULL,
        [Reference] [nvarchar](100) NULL,
        [TotalDebit] [decimal](18,2) NOT NULL,
        [TotalCredit] [decimal](18,2) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_JournalEntries_LoanId] ON [dbo].[JournalEntries] ([LoanId]);
    CREATE INDEX [IX_JournalEntries_UserId] ON [dbo].[JournalEntries] ([UserId]);
    CREATE INDEX [IX_JournalEntries_EntryType] ON [dbo].[JournalEntries] ([EntryType]);
    CREATE INDEX [IX_JournalEntries_EntryDate] ON [dbo].[JournalEntries] ([EntryDate]);
    
    ALTER TABLE [dbo].[JournalEntries]
    ADD CONSTRAINT [FK_JournalEntries_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[JournalEntries]
    ADD CONSTRAINT [FK_JournalEntries_Loans_LoanId] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([Id]) ON DELETE CASCADE;
END
GO

-- Create JournalEntryLines table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntryLines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[JournalEntryLines] (
        [Id] [nvarchar](450) NOT NULL,
        [JournalEntryId] [nvarchar](450) NOT NULL,
        [AccountName] [nvarchar](100) NOT NULL,
        [AccountType] [nvarchar](50) NOT NULL,
        [EntrySide] [nvarchar](10) NOT NULL,
        [Amount] [decimal](18,2) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_JournalEntryLines_JournalEntryId] ON [dbo].[JournalEntryLines] ([JournalEntryId]);
    CREATE INDEX [IX_JournalEntryLines_AccountName] ON [dbo].[JournalEntryLines] ([AccountName]);
    CREATE INDEX [IX_JournalEntryLines_AccountType] ON [dbo].[JournalEntryLines] ([AccountType]);
    
    ALTER TABLE [dbo].[JournalEntryLines]
    ADD CONSTRAINT [FK_JournalEntryLines_JournalEntries_JournalEntryId] 
    FOREIGN KEY ([JournalEntryId]) REFERENCES [dbo].[JournalEntries] ([Id]) ON DELETE CASCADE;
END
GO
```

**Or use the complete script:**
- Location: `Documentation/Database/Scripts/create_journal_entry_tables.sql`

---

## 📋 **Quick Fix Steps**

### **Step 1: Stop the Application**
Close/stop your running application to unlock the database.

### **Step 2: Run SQL Scripts**
Execute both SQL scripts in order:
1. `add_loan_accounting_fields.sql` - Adds new columns to Loans table
2. `create_journal_entry_tables.sql` - Creates journal entry tables

### **Step 3: Restart Application**
Start your application again. The error should be resolved!

---

## 🔧 **Alternative: Using Entity Framework Migrations**

If you prefer using EF Core migrations:

1. **Stop the application**
2. Open terminal in the project root
3. Run:
   ```bash
   cd UtilityHub360
   dotnet ef migrations add AddLoanAccountingFields --output-dir Data/Migrations
   dotnet ef database update
   ```

**Note:** This requires the application to be stopped and the build to succeed.

---

## ✅ **Verification**

After running the scripts, verify the columns exist:

```sql
-- Check if columns exist
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Loans'
AND COLUMN_NAME IN (
    'InterestComputationMethod',
    'TotalInterest',
    'DownPayment',
    'ProcessingFee',
    'ActualFinancedAmount',
    'PaymentFrequency',
    'StartDate'
);

-- Check if tables exist
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('JournalEntries', 'JournalEntryLines');
```

---

## 🚨 **If You Still Get Errors**

1. **Verify connection string** - Make sure you're connected to the correct database
2. **Check permissions** - Ensure your SQL user has ALTER TABLE permissions
3. **Verify table name** - Check if your Loans table is named `Loans` or `loans` (case-sensitive in some setups)
4. **Check for existing columns** - The script includes IF NOT EXISTS checks, but verify manually

---

## 📝 **What These Changes Do**

### **New Loan Columns:**
- `InterestComputationMethod` - Stores "FLAT_RATE" or "AMORTIZED"
- `TotalInterest` - Total interest over loan term
- `DownPayment` - Optional down payment amount
- `ProcessingFee` - Optional processing fee
- `ActualFinancedAmount` - Principal minus down payment
- `PaymentFrequency` - Payment frequency (MONTHLY, WEEKLY, etc.)
- `StartDate` - Loan start date (when disbursed)

### **New Tables:**
- `JournalEntries` - Header for accounting entries
- `JournalEntryLines` - Individual debit/credit lines

---

## ✅ **After Migration**

Once the migration is complete:
1. ✅ All existing loans will have default values for new fields
2. ✅ New loans will automatically use the accounting system
3. ✅ Journal entries will be created for all loan transactions
4. ✅ The error will be resolved

---

**Last Updated**: December 2024  
**Status**: Ready to Apply


