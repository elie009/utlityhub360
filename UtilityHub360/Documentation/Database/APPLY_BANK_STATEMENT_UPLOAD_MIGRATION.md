# 🔧 Fix: Apply Bank Statement Upload Tables Migration

## ❌ Error You're Seeing

```
Invalid object name 'BankStatementUploads'
```

**Cause:** The `BankStatementUploads` and `StagingTransactions` tables haven't been created in your database yet.

---

## ✅ Solution: Apply the Migration

### Step 1: Stop Your Running App (if needed)
If your application is running, you may need to stop it temporarily to avoid database locks.

### Step 2: Run the SQL Script

**Option A: Using SQL Server Management Studio (SSMS) or Azure Data Studio**

1. Open your database connection
2. **IMPORTANT:** Make sure you're connected to the correct database
3. Open the SQL script: `Documentation/Database/Scripts/create_bank_statement_upload_tables.sql`
4. Execute the script (no need to modify anything - the script will work with your current database)

**Option B: Using Command Line (sqlcmd)**

```powershell
sqlcmd -S your_server -d your_database -i "Documentation/Database/Scripts/create_bank_statement_upload_tables.sql"
```

**Option C: Using Entity Framework (if app is stopped)**

```powershell
cd UtilityHub360
dotnet ef migrations add AddBankStatementUploadTables
dotnet ef database update
```

---

## 📋 What This Migration Creates

### 1. **BankStatementUploads** Table
- Stores uploaded bank statement files (PDF, CSV)
- Links to Users and BankAccounts
- Tracks upload status (PENDING, PROCESSING, COMPLETED, FAILED)
- Links to processed BankStatements

**Key Fields:**
- `Id` - Unique upload identifier
- `UserId` - Reference to Users table
- `BankAccountId` - Reference to BankAccounts table
- `FilePath` - Server file path
- `OriginalFileName` - Original file name
- `FileType` - PDF or CSV
- `Status` - Upload processing status
- `ProcessedBankStatementId` - Link to BankStatement after processing

### 2. **StagingTransactions** Table
- Stores temporary transactions extracted from bank statements
- Waiting for user confirmation before being finalized
- Links to BankStatementUploads

**Key Fields:**
- `Id` - Unique transaction identifier
- `UploadId` - Reference to BankStatementUploads table
- `TransactionDate` - Transaction date
- `Amount` - Transaction amount
- `TransactionType` - DEBIT or CREDIT
- `Description` - Transaction description
- `BalanceAfterTransaction` - Balance after this transaction

---

## 🧪 Verify the Migration

After running the script, verify the tables were created:

```sql
-- Check if tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('BankStatementUploads', 'StagingTransactions');

-- Should return 2 rows
```

---

## 🎯 Next Steps

1. **Restart your application** (if you stopped it)
2. **Test the endpoint**:
   ```
   GET /api/reconciliation/statements/uploads/account/{bankAccountId}
   ```
3. **The error should be resolved!**

---

## 📝 Notes

- The script is idempotent - it checks if tables exist before creating them
- Foreign key constraints ensure data integrity
- Indexes are created for performance
- The script will create foreign keys only if the referenced tables exist

---

## ⚠️ Important

- **Make sure you're connected to the correct database** before running the script
- The script will automatically check if referenced tables (`Users`, `BankAccounts`, `BankStatements`) exist before creating foreign keys
- If foreign keys can't be created (tables don't exist), the script will continue and print warnings
- The composite index on `BankAccountId_UserId` was removed due to SQL Server's 1700-byte key length limit (nvarchar(450) * 2 = 1800 bytes)
- Individual indexes on `UserId` and `BankAccountId` provide good query performance
- If the table already exists but is missing foreign keys, the script will attempt to add them

---

**Created:** 2025  
**Status:** ✅ Ready to apply

