# 🔧 QUICK FIX: Create Reconciliation Tables

## ❌ Current Error
```
Invalid object name 'Reconciliations'
```

## ✅ Solution: Run SQL Script

### **Option 1: Use PowerShell Script (Easiest)**

1. Open PowerShell in the project directory:
   ```powershell
   cd UtilityHub360
   ```

2. Run the migration script:
   ```powershell
   .\run_reconciliation_migration.ps1
   ```

3. Follow the prompts to enter your database connection details.

---

### **Option 2: Manual SQL Execution**

1. **Open SQL Server Management Studio (SSMS)** or **Azure Data Studio**

2. **Connect to your database**

3. **Open the SQL file:**
   ```
   Documentation/Database/Scripts/create_reconciliation_tables.sql
   ```

4. **Execute the script** (Press F5 or click Execute)

---

### **Option 3: Command Line (sqlcmd)**

If you have `sqlcmd` installed:

```powershell
sqlcmd -S localhost -d YourDatabaseName -E -i "Documentation/Database/Scripts/create_reconciliation_tables.sql"
```

**For SQL Server Authentication:**
```powershell
sqlcmd -S localhost -d YourDatabaseName -U YourUsername -P YourPassword -i "Documentation/Database/Scripts/create_reconciliation_tables.sql"
```

---

## 📋 What Gets Created

The script creates 4 tables:

1. **BankStatements** - Stores imported bank statements
2. **BankStatementItems** - Stores individual statement line items  
3. **Reconciliations** - Stores reconciliation sessions
4. **ReconciliationMatches** - Stores transaction matches

---

## ✅ Verify It Worked

After running the script, test your endpoint:

```
GET http://localhost:5000/api/reconciliation/account/cac74e59-47a4-49bd-b0f2-78e3d8771bf7
```

**Expected:** Should return `200 OK` with an empty array `[]` (if no reconciliations exist yet)

---

## 🚨 Still Getting Error?

1. **Check table names in database:**
   ```sql
   SELECT TABLE_NAME 
   FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME LIKE '%Reconciliation%'
   ```

2. **Verify connection string** in `appsettings.json` points to the correct database

3. **Check SQL script executed successfully** - Look for "Reconciliation tables created successfully!" message

---

**Need Help?** Check the detailed guide: `Documentation/Database/APPLY_RECONCILIATION_MIGRATION.md`

