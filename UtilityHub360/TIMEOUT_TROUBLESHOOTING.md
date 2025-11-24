# 🔍 Timeout Troubleshooting Guide

## ❌ Error: "Execution Timeout Expired"

This indicates the database query is taking longer than the timeout period (default 30 seconds).

---

## ✅ Fixes Applied

### 1. **Increased Timeouts**
- Connection Timeout: 60 seconds (in connection string)
- Command Timeout: 120 seconds (in DbContext configuration)

### 2. **Simplified Query**
- Changed from complex projection to simple entity loading + in-memory mapping
- More reliable for remote database connections

### 3. **Better Error Handling**
- Specific error messages for table not found
- Timeout-specific error messages

---

## 🔍 Diagnostic Steps

### Step 1: Verify Table Exists

Run this SQL query on your database:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'BankStatements';
```

**Expected:** Should return 1 row  
**If 0 rows:** Table doesn't exist - run `run_migration_direct_FIXED.sql`

---

### Step 2: Check Indexes

Run this SQL query:

```sql
SELECT 
    i.name AS IndexName,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columns
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('dbo.BankStatements')
    AND i.type > 0
GROUP BY i.name
ORDER BY i.name;
```

**Expected:** Should see indexes on `BankAccountId` and `UserId`  
**If missing:** Run `add_performance_index.sql`

---

### Step 3: Test Simple Query

Run this directly in SQL:

```sql
SELECT TOP 10 
    Id, StatementName, StatementEndDate, TotalTransactions
FROM BankStatements
WHERE BankAccountId = 'cac74e59-47a4-49bd-b0f2-78e3d8771bf7'
ORDER BY StatementEndDate DESC;
```

**If this times out:**
- Check network connectivity to `174.138.185.18`
- Check if database server is responsive
- Check for table locks

---

### Step 4: Check Network Latency

Test connection to database server:

```powershell
Test-NetConnection -ComputerName 174.138.185.18 -Port 1433
```

**If connection fails:**
- Firewall blocking port 1433
- Database server not accessible
- Network issues

---

## 🚨 Most Likely Causes

### 1. **Table Doesn't Exist** (Most Common)
**Solution:** Run `run_migration_direct_FIXED.sql`

### 2. **Missing Indexes**
**Solution:** Run `add_performance_index.sql`

### 3. **Network Latency**
**Solution:** 
- Check connection to `174.138.185.18`
- Consider using local database for development

### 4. **Table Locks**
**Solution:** 
- Check for long-running transactions
- Restart database if needed

---

## ✅ Quick Fix Checklist

- [ ] Table `BankStatements` exists (run diagnostic SQL)
- [ ] Indexes exist on `BankAccountId` and `UserId`
- [ ] Network connection to database server works
- [ ] No table locks blocking the query
- [ ] Connection string has timeout settings
- [ ] Application restarted after code changes

---

## 🧪 Test After Fixes

1. **Restart your application**
2. **Test the endpoint:**
   ```
   GET /api/reconciliation/statements/account/cac74e59-47a4-49bd-b0f2-78e3d8771bf7
   ```
3. **Check response time** - Should be < 1 second

---

## 📝 Connection String Updated

The connection string now includes:
```
Connection Timeout=60;Command Timeout=120;
```

This gives more time for slow network connections.

---

**Run the diagnostic script:** `check_database_status.sql` to get detailed information about your database state.

