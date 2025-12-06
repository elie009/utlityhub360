# Stripe Price ID Setup Guide

## Problem
If you're getting the error: **"Stripe price ID not configured for MONTHLY billing cycle"**, it means your subscription plans in the database don't have Stripe Price IDs configured.

## Solution

### Step 1: Create Products and Prices in Stripe Dashboard

1. **Go to Stripe Dashboard**: https://dashboard.stripe.com/test/products
2. **Create a Product** for each subscription plan:
   - Click "Add product"
   - Enter product name (e.g., "Starter Plan", "Professional Plan", "Enterprise Plan")
   - Enter description
   - Click "Save product"

3. **Add Prices** to each product:
   - For each product, click "Add price"
   - **Monthly Price**:
     - Pricing model: Standard pricing
     - Price: Enter your monthly price (e.g., $9.99)
     - Billing period: Monthly
     - Click "Add price"
   - **Yearly Price**:
     - Pricing model: Standard pricing
     - Price: Enter your yearly price (e.g., $99.99)
     - Billing period: Yearly
     - Click "Add price"

4. **Copy the Price IDs**:
   - Each price will have a Price ID that starts with `price_...`
   - Copy both the monthly and yearly Price IDs for each plan

### Step 2: Update Your Database

#### Option A: Using SQL Script (Recommended)

1. Run the check script to see which plans need updating:
   ```sql
   -- Run: check_and_update_stripe_price_ids.sql
   ```

2. Update your plans with the Price IDs:
   ```sql
   -- Example for STARTER plan
   UPDATE SubscriptionPlans 
   SET StripeMonthlyPriceId = 'price_xxxxxxxxxxxxxxxxxxxxx',  -- Your monthly price ID
       StripeYearlyPriceId = 'price_yyyyyyyyyyyyyyyyyyyyy'     -- Your yearly price ID
   WHERE Name = 'STARTER';

   -- Example for PROFESSIONAL plan
   UPDATE SubscriptionPlans 
   SET StripeMonthlyPriceId = 'price_xxxxxxxxxxxxxxxxxxxxx',
       StripeYearlyPriceId = 'price_yyyyyyyyyyyyyyyyyyyyy'
   WHERE Name = 'PROFESSIONAL';

   -- Example for ENTERPRISE plan
   UPDATE SubscriptionPlans 
   SET StripeMonthlyPriceId = 'price_xxxxxxxxxxxxxxxxxxxxx',
       StripeYearlyPriceId = 'price_yyyyyyyyyyyyyyyyyyyyy'
   WHERE Name = 'ENTERPRISE';
   ```

#### Option B: Using Entity Framework / Application Code

If you have a way to update plans through your application, you can update them there.

### Step 3: Verify the Update

Run this query to verify all plans have Price IDs configured:

```sql
SELECT 
    Name,
    DisplayName,
    StripeMonthlyPriceId,
    StripeYearlyPriceId,
    CASE 
        WHEN StripeMonthlyPriceId IS NULL OR StripeMonthlyPriceId = '' THEN 'MISSING'
        ELSE 'OK'
    END AS MonthlyStatus,
    CASE 
        WHEN StripeYearlyPriceId IS NULL OR StripeYearlyPriceId = '' THEN 'MISSING'
        ELSE 'OK'
    END AS YearlyStatus
FROM SubscriptionPlans;
```

All plans should show "OK" for both MonthlyStatus and YearlyStatus.

## Important Notes

1. **Test Mode vs Live Mode**: 
   - Make sure you're using test mode Price IDs if you're in development
   - Test mode Price IDs start with `price_` and are found in the test dashboard
   - Live mode Price IDs are found in the live dashboard

2. **Price ID Format**:
   - Price IDs always start with `price_`
   - Example: `price_1ABC123def456GHI789jkl012`

3. **Free Plans**:
   - If a plan is free (e.g., STARTER with $0), you still need to create a $0 price in Stripe
   - Or you can handle free plans differently in your code

4. **After Updating**:
   - Restart your backend server if needed
   - The error should be resolved once Price IDs are configured

## Troubleshooting

### Error persists after updating
- Verify the Price IDs are correct (copy-paste from Stripe Dashboard)
- Check for extra spaces or quotes in the database
- Ensure you're using the correct plan name (case-sensitive: 'STARTER', 'PROFESSIONAL', 'ENTERPRISE')
- Restart your backend server

### Can't find Price IDs in Stripe
- Make sure you're in the correct mode (Test vs Live)
- Check that prices are created and active
- Verify you're looking at the correct product

## Quick Reference

- **Stripe Dashboard (Test)**: https://dashboard.stripe.com/test/products
- **Stripe Dashboard (Live)**: https://dashboard.stripe.com/products
- **Check Script**: `Documentation/Database/Scripts/check_and_update_stripe_price_ids.sql`
- **Migration Script**: `Documentation/Database/Scripts/add_stripe_price_ids_to_subscription_plans.sql`

