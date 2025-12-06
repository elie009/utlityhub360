-- ==========================================
-- ADD TIER-BASED FEATURE FLAGS
-- ==========================================
-- This script adds new feature flags for tier-based restrictions:
-- - HasFinancialHealthScore
-- - HasBillForecasting
-- - HasDebtOptimizer
-- ==========================================

-- Add new columns to SubscriptionPlans table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'HasFinancialHealthScore')
BEGIN
    ALTER TABLE [dbo].[SubscriptionPlans]
    ADD [HasFinancialHealthScore] BIT NOT NULL DEFAULT 0;
    PRINT 'Added HasFinancialHealthScore column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'HasBillForecasting')
BEGIN
    ALTER TABLE [dbo].[SubscriptionPlans]
    ADD [HasBillForecasting] BIT NOT NULL DEFAULT 0;
    PRINT 'Added HasBillForecasting column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'HasDebtOptimizer')
BEGIN
    ALTER TABLE [dbo].[SubscriptionPlans]
    ADD [HasDebtOptimizer] BIT NOT NULL DEFAULT 0;
    PRINT 'Added HasDebtOptimizer column';
END
GO

-- Update existing plans with correct feature flags
-- Free (STARTER) - No premium features
UPDATE [dbo].[SubscriptionPlans]
SET 
    [HasFinancialHealthScore] = 0,
    [HasBillForecasting] = 0,
    [HasDebtOptimizer] = 0
WHERE [Name] = 'STARTER';
PRINT 'Updated STARTER plan features';

-- Premium (PROFESSIONAL) - Has Financial Health Score, Bill Forecasting, Debt Optimizer
UPDATE [dbo].[SubscriptionPlans]
SET 
    [HasFinancialHealthScore] = 1,
    [HasBillForecasting] = 1,
    [HasDebtOptimizer] = 1
WHERE [Name] = 'PROFESSIONAL';
PRINT 'Updated PROFESSIONAL plan features';

-- Premium Plus (ENTERPRISE) - Has all features
UPDATE [dbo].[SubscriptionPlans]
SET 
    [HasFinancialHealthScore] = 1,
    [HasBillForecasting] = 1,
    [HasDebtOptimizer] = 1
WHERE [Name] = 'ENTERPRISE';
PRINT 'Updated ENTERPRISE plan features';

PRINT '=========================================';
PRINT 'Tier-based feature flags migration complete!';
PRINT '=========================================';

