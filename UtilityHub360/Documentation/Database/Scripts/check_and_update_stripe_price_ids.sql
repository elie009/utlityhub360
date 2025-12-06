-- ==========================================
-- CHECK AND UPDATE STRIPE PRICE IDS
-- ==========================================
-- This script helps you check which subscription plans are missing Stripe Price IDs
-- and provides a template to update them
-- ==========================================

-- Step 1: Check current status of all subscription plans
PRINT '========================================';
PRINT 'Current Subscription Plans Status:';
PRINT '========================================';

SELECT 
    Id,
    Name,
    DisplayName,
    MonthlyPrice,
    YearlyPrice,
    CASE 
        WHEN StripeMonthlyPriceId IS NULL OR StripeMonthlyPriceId = '' THEN 'MISSING'
        ELSE 'CONFIGURED'
    END AS MonthlyPriceIdStatus,
    CASE 
        WHEN StripeYearlyPriceId IS NULL OR StripeYearlyPriceId = '' THEN 'MISSING'
        ELSE 'CONFIGURED'
    END AS YearlyPriceIdStatus,
    StripeMonthlyPriceId,
    StripeYearlyPriceId,
    IsActive
FROM SubscriptionPlans
ORDER BY DisplayOrder, Name;

PRINT '';
PRINT '========================================';
PRINT 'Plans Missing Stripe Price IDs:';
PRINT '========================================';

SELECT 
    Name,
    DisplayName,
    CASE 
        WHEN StripeMonthlyPriceId IS NULL OR StripeMonthlyPriceId = '' THEN 'Monthly Price ID Missing'
        ELSE ''
    END AS MonthlyIssue,
    CASE 
        WHEN StripeYearlyPriceId IS NULL OR StripeYearlyPriceId = '' THEN 'Yearly Price ID Missing'
        ELSE ''
    END AS YearlyIssue
FROM SubscriptionPlans
WHERE (StripeMonthlyPriceId IS NULL OR StripeMonthlyPriceId = '')
   OR (StripeYearlyPriceId IS NULL OR StripeYearlyPriceId = '')
ORDER BY Name;

PRINT '';
PRINT '========================================';
PRINT 'UPDATE TEMPLATE:';
PRINT '========================================';
PRINT 'Use the following template to update your plans with Stripe Price IDs.';
PRINT 'Replace the price IDs with your actual Stripe Price IDs from your Stripe Dashboard.';
PRINT '';
PRINT 'To get your Stripe Price IDs:';
PRINT '1. Go to https://dashboard.stripe.com/test/products';
PRINT '2. Click on a product';
PRINT '3. Copy the Price ID (starts with price_...)';
PRINT '';

-- ==========================================
-- UPDATE TEMPLATE - Uncomment and fill in your Price IDs
-- ==========================================

/*
-- Example: Update STARTER plan
UPDATE SubscriptionPlans 
SET StripeMonthlyPriceId = 'price_xxxxxxxxxxxxxxxxxxxxx',  -- Replace with your monthly price ID
    StripeYearlyPriceId = 'price_yyyyyyyyyyyyyyyyyyyyy'     -- Replace with your yearly price ID
WHERE Name = 'STARTER';

-- Example: Update PROFESSIONAL plan
UPDATE SubscriptionPlans 
SET StripeMonthlyPriceId = 'price_xxxxxxxxxxxxxxxxxxxxx',  -- Replace with your monthly price ID
    StripeYearlyPriceId = 'price_yyyyyyyyyyyyyyyyyyyyy'     -- Replace with your yearly price ID
WHERE Name = 'PROFESSIONAL';

-- Example: Update ENTERPRISE plan
UPDATE SubscriptionPlans 
SET StripeMonthlyPriceId = 'price_xxxxxxxxxxxxxxxxxxxxx',  -- Replace with your monthly price ID
    StripeYearlyPriceId = 'price_yyyyyyyyyyyyyyyyyyyyy'     -- Replace with your yearly price ID
WHERE Name = 'ENTERPRISE';
*/

PRINT '';
PRINT '========================================';
PRINT 'After updating, run the first SELECT query again to verify.';
PRINT '========================================';

