# Steps to Enable StartDate Updates

## ⚠️ IMPORTANT: Run Migration First!

Before making code changes, you **MUST** run the SQL migration script first to add the `StartDate` column to your database.

## Step 1: Run Migration Script

Execute this SQL script on your database:

**File:** `UtilityHub360/add_startdate_to_savings_accounts.sql`

The script will:
- Add the `StartDate` column to `SavingsAccounts` table
- Set existing records to use `CreatedAt` as `StartDate`

## Step 2: Remove Ignore in ApplicationDbContext

After confirming the migration ran successfully, remove these lines from `UtilityHub360/Data/ApplicationDbContext.cs` (around line 227):

```csharp
// Remove these lines:
entity.Ignore(e => e.StartDate);
```

## Step 3: Update Mapping Code

### In SavingsService.cs

**Line ~861 in `MapToSavingsAccountDto` method:**
Change from:
```csharp
var startDate = savingsAccount.CreatedAt;
```
To:
```csharp
var startDate = savingsAccount.StartDate ?? savingsAccount.CreatedAt;
```

### In FinancialReportService.cs

**Line ~539 in `GetSavingsReportAsync` method:**
Change from:
```csharp
var startDate = sa.CreatedAt;
```
To:
```csharp
var startDate = sa.StartDate ?? sa.CreatedAt;
```

## Step 4: Restart Application

After making all changes, restart your application.

## Verification

After completing these steps, when you update a savings account with:
```json
{
  "startDate": "2026-07-22"
}
```

The response should show:
```json
{
  "startDate": "2026-07-22T00:00:00"
}
```

## ⚠️ If You Get "Invalid column name 'StartDate'" Error

This means the migration hasn't been run yet. Go back to Step 1 and run the migration script first.

