# 🔧 Apply Investment Tracking Tables Migration

## ❌ Error You're Seeing

```
Invalid object name 'Investments'
```

**Cause:** The investment tracking tables haven't been created in your database yet.

---

## ✅ Solution: Apply the Migration

### Step 1: Stop Your Running App (if needed)
If your application is running, you may need to stop it temporarily to avoid database locks.

### Step 2: Run the SQL Script

**Option A: Using SQL Server Management Studio (SSMS) or Azure Data Studio**

1. Open your database connection
2. Open the SQL script: `Documentation/Database/Scripts/create_investment_tables.sql`
3. Execute the script

**Option B: Using Command Line (sqlcmd)**

```powershell
sqlcmd -S your_server -d your_database -i "Documentation/Database/Scripts/create_investment_tables.sql"
```

**Option C: Using Entity Framework (if app is stopped)**

```powershell
cd UtilityHub360
dotnet ef database update
```

---

## 📋 What This Migration Creates

### 1. **Investments** Table
- Stores investment accounts (brokerage, 401(k), IRA, etc.)
- Links to Users
- Tracks account details, values, gains/losses, and returns

### 2. **InvestmentPositions** Table
- Stores individual positions/holdings within investment accounts
- Links to Investments
- Tracks symbols, quantities, cost basis, current values, and performance

### 3. **InvestmentTransactions** Table
- Stores investment transactions (buy, sell, dividend, etc.)
- Links to Investments and optional Positions
- Tracks transaction details, dates, fees, and taxes

---

## 🧪 Verify the Migration

After running the script, verify the tables were created:

```sql
-- Check if tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Investments', 'InvestmentPositions', 'InvestmentTransactions');
```

You should see all three tables listed.

---

## 🚀 After Migration

Once the migration is complete:
1. Restart your application
2. The Investment Tracking feature should work correctly
3. Enterprise users can now add and manage investment accounts

---

## 📝 Notes

- This migration is safe to run multiple times (uses IF NOT EXISTS)
- The tables support soft delete functionality
- All foreign keys are properly configured with cascade deletes where appropriate

