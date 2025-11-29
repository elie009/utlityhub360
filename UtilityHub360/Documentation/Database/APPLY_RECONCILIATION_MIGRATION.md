# 🔧 Fix: Apply Reconciliation Tables Migration

## ❌ Error You're Seeing

```
Invalid object name 'Reconciliations'
```

**Cause:** The reconciliation tables haven't been created in your database yet.

---

## ✅ Solution: Apply the Migration

### Step 1: Stop Your Running App (if needed)
If your application is running, you may need to stop it temporarily to avoid database locks.

### Step 2: Run the SQL Script

**Option A: Using SQL Server Management Studio (SSMS) or Azure Data Studio**

1. Open your database connection
2. Open the SQL script: `Documentation/Database/Scripts/create_reconciliation_tables.sql`
3. Execute the script

**Option B: Using Command Line (sqlcmd)**

```powershell
sqlcmd -S your_server -d your_database -i "Documentation/Database/Scripts/create_reconciliation_tables.sql"
```

**Option C: Using Entity Framework (if app is stopped)**

```powershell
cd UtilityHub360
dotnet ef database update
```

---

## 📋 What This Migration Creates

### 1. **BankStatements** Table
- Stores imported bank statements
- Links to Users and BankAccounts
- Tracks statement period, balances, and reconciliation status

### 2. **BankStatementItems** Table
- Stores individual transaction line items from statements
- Links to BankStatements
- Tracks transaction details, matching status

### 3. **Reconciliations** Table
- Stores reconciliation sessions
- Links to Users, BankAccounts, and optional BankStatements
- Tracks reconciliation status, balances, and transaction counts

### 4. **ReconciliationMatches** Table
- Stores matches between system transactions and statement items
- Links to Reconciliations and BankStatementItems
- Tracks match type, status, and notes

---

## 🧪 Verify the Migration

After running the script, verify the tables were created:

```sql
-- Check if tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('BankStatements', 'BankStatementItems', 'Reconciliations', 'ReconciliationMatches');

-- Should return 4 rows
```

---

## 🎯 Next Steps

1. **Restart your application** (if you stopped it)
2. **Test the endpoint**:
   ```
   GET /api/reconciliation/account/{bankAccountId}
   ```
3. **The error should be resolved!**

---

## 📝 Notes

- The script is idempotent - it checks if tables exist before creating them
- Foreign key constraints ensure data integrity
- Indexes are created for performance
- The migration is marked in `__EFMigrationsHistory` if that table exists

---

**Created:** 2024  
**Status:** ✅ Ready to apply

