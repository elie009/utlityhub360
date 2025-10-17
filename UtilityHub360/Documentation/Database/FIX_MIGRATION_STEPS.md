# 🔧 Fix VariableExpenses Error - Quick Steps

## The Problem
```
Invalid object name 'VariableExpenses'
```

The table doesn't exist in your database yet.

---

## ✅ Solution (2 Minutes)

### Step 1: Stop Your Running App
- If running in terminal: Press `Ctrl + C`
- If running in Visual Studio: Stop debugging

### Step 2: Apply Migration

Open PowerShell in the project directory and run:

```powershell
cd D:\PROJECT\REPOSITORY\WEBMVC\UtilityHub360\UtilityHub360
dotnet ef database update
```

You should see:
```
Applying migration '20251011192439_AddVariableExpensesTable'
Done.
```

### Step 3: Restart Your App

```powershell
dotnet run
```

### Step 4: Test the API Again

```http
GET /api/Dashboard/disposable-amount?year=2025&month=10
```

**It will work now!** ✅

---

## 🆘 If Migration Still Doesn't Work

Run this SQL script directly in your database:

```sql
USE [your_database_name];
GO

-- Create VariableExpenses table
CREATE TABLE [dbo].[VariableExpenses] (
    [Id] NVARCHAR(450) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [Description] NVARCHAR(200) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Category] NVARCHAR(50) NOT NULL,
    [Currency] NVARCHAR(10) NOT NULL,
    [ExpenseDate] DATETIME2 NOT NULL,
    [Notes] NVARCHAR(500) NULL,
    [Merchant] NVARCHAR(200) NULL,
    [PaymentMethod] NVARCHAR(50) NULL,
    [IsRecurring] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    CONSTRAINT [PK_VariableExpenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VariableExpenses_Users_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_VariableExpenses_UserId] ON [dbo].[VariableExpenses] ([UserId]);
CREATE INDEX [IX_VariableExpenses_Category] ON [dbo].[VariableExpenses] ([Category]);
CREATE INDEX [IX_VariableExpenses_ExpenseDate] ON [dbo].[VariableExpenses] ([ExpenseDate]);
CREATE INDEX [IX_VariableExpenses_UserId_ExpenseDate_Category] ON [dbo].[VariableExpenses] ([UserId], [ExpenseDate], [Category]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251011192439_AddVariableExpensesTable', N'8.0.0');

GO
```

Then restart your app.

---

## 💡 Alternative: Use Simple API (Works NOW!)

**Don't want to deal with migration?**

Use this endpoint instead - it works without the VariableExpenses table:

```http
GET /api/Dashboard/summary?year=2025&month=10
```

This gives you:
- ✅ Total Income
- ✅ Total Bills  
- ✅ Total Loans
- ✅ Total Savings
- ✅ **Remaining Amount**
- ✅ Financial Status

**No migration needed!**

---

**Quick fix:** Stop app → Run `dotnet ef database update` → Restart → Test! 🚀

