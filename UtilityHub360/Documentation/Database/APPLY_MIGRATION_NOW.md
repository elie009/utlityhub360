# 🔧 Fix: Apply VariableExpenses Migration

## ❌ Error You're Seeing

```
Invalid object name 'VariableExpenses'
```

**Cause:** The VariableExpenses table hasn't been created in your database yet.

---

## ✅ Solution: Apply the Migration

### Step 1: Stop Your Running App
Press `Ctrl+C` in the terminal where your app is running.

### Step 2: Apply the Migration

**Option A: Using Entity Framework (Recommended)**
```powershell
cd UtilityHub360
dotnet ef database update
```

**Option B: Run the SQL Script Manually**

If Option A doesn't work, execute this SQL script in your database:

```sql
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
        FOREIGN KEY ([UserId]) 
        REFERENCES [Users] ([Id]) 
        ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX [IX_VariableExpenses_UserId] 
    ON [dbo].[VariableExpenses] ([UserId]);
    
CREATE INDEX [IX_VariableExpenses_Category] 
    ON [dbo].[VariableExpenses] ([Category]);
    
CREATE INDEX [IX_VariableExpenses_ExpenseDate] 
    ON [dbo].[VariableExpenses] ([ExpenseDate]);
    
CREATE INDEX [IX_VariableExpenses_UserId_ExpenseDate_Category] 
    ON [dbo].[VariableExpenses] ([UserId], [ExpenseDate], [Category]);

GO

-- Mark migration as applied
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251011192439_AddVariableExpensesTable', N'8.0.0');

PRINT 'VariableExpenses table created successfully!';
```

### Step 3: Restart Your App
```powershell
dotnet run
```

### Step 4: Test Again
```http
GET /api/Dashboard/disposable-amount?year=2025&month=10
```

---

## ✅ It Should Work Now!

After applying the migration, the API will work and return your disposable amount data.

---

## 💡 Alternative: Use the Simple API Instead

If you don't need variable expense tracking yet, use the simpler endpoint:

```http
GET /api/Dashboard/summary?year=2025&month=10
```

**This endpoint works right now** because it only uses:
- IncomeSources ✅ (exists)
- Bills ✅ (exists)
- Loans ✅ (exists)
- SavingsTransactions ✅ (exists)

No VariableExpenses table needed!

**Response:**
```json
{
  "success": true,
  "data": {
    "totalIncome": 45000.00,
    "totalBills": 15000.00,
    "totalLoans": 8000.00,
    "totalExpenses": 23000.00,
    "totalSavings": 5000.00,
    "remainingAmount": 17000.00,
    "financialStatus": "HEALTHY"
  }
}
```

---

**Quick Fix:** Stop app → Run migration → Restart app → Test! 🚀

