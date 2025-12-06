-- ==========================================
-- ADD STRIPE PRICE ID COLUMNS TO SUBSCRIPTION PLANS
-- ==========================================
-- This script adds Stripe integration fields to the SubscriptionPlans table
-- Run this script to enable Stripe payment processing for subscriptions
-- ==========================================

-- Add StripeMonthlyPriceId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'StripeMonthlyPriceId')
BEGIN
    ALTER TABLE [dbo].[SubscriptionPlans]
    ADD [StripeMonthlyPriceId] NVARCHAR(255) NULL;
    
    PRINT 'Added StripeMonthlyPriceId column to SubscriptionPlans table';
END
ELSE
BEGIN
    PRINT 'StripeMonthlyPriceId column already exists';
END
GO

-- Add StripeYearlyPriceId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'StripeYearlyPriceId')
BEGIN
    ALTER TABLE [dbo].[SubscriptionPlans]
    ADD [StripeYearlyPriceId] NVARCHAR(255) NULL;
    
    PRINT 'Added StripeYearlyPriceId column to SubscriptionPlans table';
END
ELSE
BEGIN
    PRINT 'StripeYearlyPriceId column already exists';
END
GO

-- ==========================================
-- NOTES
-- ==========================================
-- After running this script, you need to:
-- 1. Create products and prices in your Stripe Dashboard
-- 2. Update the SubscriptionPlans table with the Stripe Price IDs:
--    UPDATE SubscriptionPlans 
--    SET StripeMonthlyPriceId = 'price_xxxxx', 
--        StripeYearlyPriceId = 'price_yyyyy'
--    WHERE Name = 'PROFESSIONAL';
--
-- 3. Repeat for all subscription plans (STARTER, PROFESSIONAL, ENTERPRISE)
-- ==========================================

PRINT 'Stripe price ID columns migration completed successfully!';
PRINT 'Remember to update the SubscriptionPlans table with actual Stripe Price IDs from your Stripe Dashboard.';

