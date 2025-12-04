# ⚡ QUICK FIX: Create Subscription Tables

## The Problem
You're getting this error:
```
Invalid object name 'UserSubscriptions'
```

This means the subscription tables haven't been created in your database yet.

## ✅ Solution: Run This SQL Script

### Step 1: Open SQL Server Management Studio (SSMS)

### Step 2: Connect to Database
- **Server:** `174.138.185.18`
- **Database:** `DBUTILS`
- **Username:** `sa01`
- **Password:** `iSTc0#T3tw~noz2r`

### Step 3: Copy and Paste This SQL

```sql
-- Create SubscriptionPlans Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SubscriptionPlans] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL,
        [DisplayName] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [MonthlyPrice] DECIMAL(18,2) NOT NULL,
        [YearlyPrice] DECIMAL(18,2) NULL,
        [MaxBankAccounts] INT NULL,
        [MaxTransactionsPerMonth] INT NULL,
        [MaxBillsPerMonth] INT NULL,
        [MaxLoans] INT NULL,
        [MaxSavingsGoals] INT NULL,
        [MaxReceiptOcrPerMonth] INT NULL,
        [MaxAiQueriesPerMonth] INT NULL,
        [MaxApiCallsPerMonth] INT NULL,
        [MaxUsers] INT NULL,
        [TransactionHistoryMonths] INT NULL,
        [HasAiAssistant] BIT NOT NULL DEFAULT 0,
        [HasBankFeedIntegration] BIT NOT NULL DEFAULT 0,
        [HasReceiptOcr] BIT NOT NULL DEFAULT 0,
        [HasAdvancedReports] BIT NOT NULL DEFAULT 0,
        [HasPrioritySupport] BIT NOT NULL DEFAULT 0,
        [HasApiAccess] BIT NOT NULL DEFAULT 0,
        [HasInvestmentTracking] BIT NOT NULL DEFAULT 0,
        [HasTaxOptimization] BIT NOT NULL DEFAULT 0,
        [HasMultiUserSupport] BIT NOT NULL DEFAULT 0,
        [HasWhiteLabelOptions] BIT NOT NULL DEFAULT 0,
        [HasCustomIntegrations] BIT NOT NULL DEFAULT 0,
        [HasDedicatedSupport] BIT NOT NULL DEFAULT 0,
        [HasAccountManager] BIT NOT NULL DEFAULT 0,
        [HasCustomReporting] BIT NOT NULL DEFAULT 0,
        [HasAdvancedSecurity] BIT NOT NULL DEFAULT 0,
        [HasComplianceReports] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [UQ_SubscriptionPlans_Name] UNIQUE ([Name])
    );
    CREATE INDEX [IX_SubscriptionPlans_IsActive] ON [dbo].[SubscriptionPlans] ([IsActive]);
    CREATE INDEX [IX_SubscriptionPlans_DisplayOrder] ON [dbo].[SubscriptionPlans] ([DisplayOrder]);
END
GO

-- Create UserSubscriptions Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserSubscriptions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserSubscriptions] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [SubscriptionPlanId] NVARCHAR(450) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
        [BillingCycle] NVARCHAR(20) NOT NULL DEFAULT 'MONTHLY',
        [CurrentPrice] DECIMAL(18,2) NOT NULL,
        [StartDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [EndDate] DATETIME2 NULL,
        [NextBillingDate] DATETIME2 NULL,
        [CancelledAt] DATETIME2 NULL,
        [TrialEndDate] DATETIME2 NULL,
        [StripeSubscriptionId] NVARCHAR(255) NULL,
        [StripeCustomerId] NVARCHAR(255) NULL,
        [PaymentMethodId] NVARCHAR(255) NULL,
        [TransactionsThisMonth] INT NOT NULL DEFAULT 0,
        [BillsThisMonth] INT NOT NULL DEFAULT 0,
        [ReceiptOcrThisMonth] INT NOT NULL DEFAULT 0,
        [AiQueriesThisMonth] INT NOT NULL DEFAULT 0,
        [ApiCallsThisMonth] INT NOT NULL DEFAULT 0,
        [LastUsageResetDate] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_UserSubscriptions_SubscriptionPlans] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [dbo].[SubscriptionPlans] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_UserSubscriptions_UserId] ON [dbo].[UserSubscriptions] ([UserId]);
    CREATE INDEX [IX_UserSubscriptions_SubscriptionPlanId] ON [dbo].[UserSubscriptions] ([SubscriptionPlanId]);
    CREATE INDEX [IX_UserSubscriptions_Status] ON [dbo].[UserSubscriptions] ([Status]);
    CREATE INDEX [IX_UserSubscriptions_BillingCycle] ON [dbo].[UserSubscriptions] ([BillingCycle]);
    CREATE INDEX [IX_UserSubscriptions_NextBillingDate] ON [dbo].[UserSubscriptions] ([NextBillingDate]);
    CREATE INDEX [IX_UserSubscriptions_UserId_Status] ON [dbo].[UserSubscriptions] ([UserId], [Status]);
    
    -- Add foreign key to Users table if it exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserSubscriptions_Users')
        BEGIN
            ALTER TABLE [dbo].[UserSubscriptions]
            ADD CONSTRAINT [FK_UserSubscriptions_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
        END
    END
END
GO

-- Seed Default Plans
IF NOT EXISTS (SELECT * FROM [dbo].[SubscriptionPlans] WHERE [Name] = 'STARTER')
BEGIN
    INSERT INTO [dbo].[SubscriptionPlans] (
        [Id], [Name], [DisplayName], [Description], [MonthlyPrice], [YearlyPrice],
        [MaxBankAccounts], [MaxTransactionsPerMonth], [MaxBillsPerMonth], [MaxLoans], [MaxSavingsGoals],
        [MaxReceiptOcrPerMonth], [MaxAiQueriesPerMonth], [MaxApiCallsPerMonth], [MaxUsers],
        [TransactionHistoryMonths],
        [HasAiAssistant], [HasBankFeedIntegration], [HasReceiptOcr], [HasAdvancedReports],
        [HasPrioritySupport], [HasApiAccess], [HasInvestmentTracking], [HasTaxOptimization],
        [HasMultiUserSupport], [HasWhiteLabelOptions], [HasCustomIntegrations], [HasDedicatedSupport],
        [HasAccountManager], [HasCustomReporting], [HasAdvancedSecurity], [HasComplianceReports],
        [IsActive], [DisplayOrder], [CreatedAt], [UpdatedAt]
    ) VALUES (
        NEWID(), 'STARTER', 'Free Plan - Starter', 'Perfect for getting started with financial tracking', 0.00, 0.00,
        3, 1000, 5, 5, 5,
        0, 10, 0, 1,
        12,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        1, 1, GETUTCDATE(), GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[SubscriptionPlans] WHERE [Name] = 'PROFESSIONAL')
BEGIN
    INSERT INTO [dbo].[SubscriptionPlans] (
        [Id], [Name], [DisplayName], [Description], [MonthlyPrice], [YearlyPrice],
        [MaxBankAccounts], [MaxTransactionsPerMonth], [MaxBillsPerMonth], [MaxLoans], [MaxSavingsGoals],
        [MaxReceiptOcrPerMonth], [MaxAiQueriesPerMonth], [MaxApiCallsPerMonth], [MaxUsers],
        [TransactionHistoryMonths],
        [HasAiAssistant], [HasBankFeedIntegration], [HasReceiptOcr], [HasAdvancedReports],
        [HasPrioritySupport], [HasApiAccess], [HasInvestmentTracking], [HasTaxOptimization],
        [HasMultiUserSupport], [HasWhiteLabelOptions], [HasCustomIntegrations], [HasDedicatedSupport],
        [HasAccountManager], [HasCustomReporting], [HasAdvancedSecurity], [HasComplianceReports],
        [IsActive], [DisplayOrder], [CreatedAt], [UpdatedAt]
    ) VALUES (
        NEWID(), 'PROFESSIONAL', 'Premium Plan - Professional', 'Unlock AI-powered insights and automation for smarter financial decisions', 9.99, 99.00,
        NULL, NULL, NULL, NULL, NULL,
        50, NULL, NULL, 1,
        NULL,
        1, 1, 1, 1,
        1, 0, 0, 0,
        0, 0, 0, 0,
        0, 1, 0, 0,
        1, 2, GETUTCDATE(), GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[SubscriptionPlans] WHERE [Name] = 'ENTERPRISE')
BEGIN
    INSERT INTO [dbo].[SubscriptionPlans] (
        [Id], [Name], [DisplayName], [Description], [MonthlyPrice], [YearlyPrice],
        [MaxBankAccounts], [MaxTransactionsPerMonth], [MaxBillsPerMonth], [MaxLoans], [MaxSavingsGoals],
        [MaxReceiptOcrPerMonth], [MaxAiQueriesPerMonth], [MaxApiCallsPerMonth], [MaxUsers],
        [TransactionHistoryMonths],
        [HasAiAssistant], [HasBankFeedIntegration], [HasReceiptOcr], [HasAdvancedReports],
        [HasPrioritySupport], [HasApiAccess], [HasInvestmentTracking], [HasTaxOptimization],
        [HasMultiUserSupport], [HasWhiteLabelOptions], [HasCustomIntegrations], [HasDedicatedSupport],
        [HasAccountManager], [HasCustomReporting], [HasAdvancedSecurity], [HasComplianceReports],
        [IsActive], [DisplayOrder], [CreatedAt], [UpdatedAt]
    ) VALUES (
        NEWID(), 'ENTERPRISE', 'Premium Plus Plan - Enterprise', 'Enterprise-grade financial intelligence for serious users and families', 29.99, 299.00,
        NULL, NULL, NULL, NULL, NULL,
        NULL, NULL, 1000, 5,
        NULL,
        1, 1, 1, 1,
        1, 1, 1, 1,
        1, 1, 1, 1,
        1, 1, 1, 1,
        1, 3, GETUTCDATE(), GETUTCDATE()
    );
END
GO

PRINT 'Subscription tables created successfully!';
PRINT 'Default subscription plans have been seeded.';
```

### Step 4: Execute (Press F5)

### Step 5: Verify
Run this query to verify:
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('SubscriptionPlans', 'UserSubscriptions');
```

You should see both tables listed.

### Step 6: Restart Backend
Stop and restart your backend application.

---

## Alternative: Use the SQL File

1. Open SSMS
2. Connect to database (same credentials as above)
3. File → Open → File
4. Navigate to: `utlityhub360-backend\UtilityHub360\Documentation\Database\Scripts\create_subscription_tables.sql`
5. Execute (F5)

---

## Still Having Issues?

If you get errors, check:
1. Are you connected to the correct database? (`DBUTILS`)
2. Do you have permission to create tables?
3. Does the `Users` table exist? (The script checks for this automatically)

