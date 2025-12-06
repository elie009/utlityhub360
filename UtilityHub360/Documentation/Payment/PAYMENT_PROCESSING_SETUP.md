# Payment Processing Integration Setup Guide

## Overview

This guide explains how to set up and configure the Stripe payment processing integration for subscription payments in UtilityHub360.

## Prerequisites

1. Stripe account (sign up at https://stripe.com)
2. Stripe API keys (Secret Key and Publishable Key)
3. Database access to run migration scripts

## Setup Steps

### 1. Install Stripe NuGet Package

The Stripe.net package has been added to the project. If you need to restore packages:

```bash
dotnet restore
```

### 2. Configure Stripe API Keys

Update `appsettings.Production.json` (or your environment-specific appsettings file) with your Stripe keys:

```json
{
  "Stripe": {
    "SecretKey": "sk_live_your_secret_key_here",
    "PublishableKey": "pk_live_your_publishable_key_here",
    "WebhookSecret": "whsec_your_webhook_secret_here"
  }
}
```

**Important:**
- Use `sk_test_...` and `pk_test_...` for development/testing
- Use `sk_live_...` and `pk_live_...` for production
- Never commit real API keys to version control

### 3. Run Database Migration

Execute the migration script to add Stripe Price ID columns:

```sql
-- Run this script
\Documentation\Database\Scripts\add_stripe_price_ids_to_subscription_plans.sql
```

### 4. Create Products and Prices in Stripe Dashboard

1. Log in to your Stripe Dashboard
2. Navigate to **Products** → **Add Product**
3. Create products for each subscription plan:
   - **Starter Plan** (Free)
   - **Professional Plan** (Premium)
   - **Enterprise Plan** (Premium Plus)

4. For each product, create two prices:
   - **Monthly Price**: Recurring, monthly billing
   - **Yearly Price**: Recurring, yearly billing

5. Copy the Price IDs (they start with `price_...`)

### 5. Update Subscription Plans with Stripe Price IDs

Update your database with the Stripe Price IDs:

```sql
-- Example: Update Professional Plan
UPDATE SubscriptionPlans 
SET StripeMonthlyPriceId = 'price_xxxxx', 
    StripeYearlyPriceId = 'price_yyyyy'
WHERE Name = 'PROFESSIONAL';

-- Update Enterprise Plan
UPDATE SubscriptionPlans 
SET StripeMonthlyPriceId = 'price_aaaaa', 
    StripeYearlyPriceId = 'price_bbbbb'
WHERE Name = 'ENTERPRISE';
```

### 6. Configure Webhook Endpoint

1. In Stripe Dashboard, go to **Developers** → **Webhooks**
2. Click **Add endpoint**
3. Set the endpoint URL to: `https://your-domain.com/api/subscriptionpayment/webhook`
4. Select the following events to listen for:
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
   - `payment_method.attached`

5. Copy the **Signing secret** (starts with `whsec_...`) and add it to your appsettings

## API Endpoints

### Create Stripe Customer
```
POST /api/subscriptionpayment/create-customer
Authorization: Bearer {token}
```

Creates a Stripe customer for the authenticated user.

### Attach Payment Method
```
POST /api/subscriptionpayment/attach-payment-method
Authorization: Bearer {token}
Body: {
  "paymentMethodId": "pm_xxxxx"
}
```

Attaches a payment method to the user's Stripe customer.

### Create Subscription
```
POST /api/subscriptionpayment/create-subscription
Authorization: Bearer {token}
Body: {
  "planId": "plan_id",
  "billingCycle": "MONTHLY", // or "YEARLY"
  "paymentMethodId": "pm_xxxxx"
}
```

Creates a subscription for the user.

### Cancel Subscription
```
POST /api/subscriptionpayment/cancel-subscription
Authorization: Bearer {token}
```

Cancels the user's active subscription.

### Webhook Endpoint
```
POST /api/subscriptionpayment/webhook
```

Handles Stripe webhook events. This endpoint is public (no authentication required) but is secured by Stripe signature verification.

## Frontend Integration

### 1. Install Stripe.js

```bash
npm install @stripe/stripe-js
```

### 2. Initialize Stripe

```typescript
import { loadStripe } from '@stripe/stripe-js';

const stripe = await loadStripe('pk_test_your_publishable_key');
```

### 3. Create Payment Method

```typescript
const { paymentMethod, error } = await stripe.createPaymentMethod({
  type: 'card',
  card: cardElement,
});
```

### 4. Create Subscription Flow

1. Create customer (if not exists)
2. Attach payment method
3. Create subscription
4. Handle subscription confirmation using `clientSecret` from response

## Testing

### Test Mode

1. Use Stripe test API keys
2. Use test card numbers from Stripe documentation:
   - Success: `4242 4242 4242 4242`
   - Decline: `4000 0000 0000 0002`
   - 3D Secure: `4000 0025 0000 3155`

### Webhook Testing

Use Stripe CLI to test webhooks locally:

```bash
stripe listen --forward-to localhost:5000/api/subscriptionpayment/webhook
```

## Security Considerations

1. **Never expose secret keys** in client-side code
2. **Always verify webhook signatures** (already implemented)
3. **Use HTTPS** in production for webhook endpoints
4. **Store API keys** securely (use environment variables or secure vault)
5. **Implement rate limiting** on payment endpoints
6. **Log all payment transactions** for audit purposes

## Troubleshooting

### Common Issues

1. **"Stripe price ID not configured"**
   - Ensure you've updated SubscriptionPlans with Stripe Price IDs
   - Verify the Price IDs are correct in Stripe Dashboard

2. **"Stripe customer not found"**
   - User must call `/create-customer` endpoint first
   - Check if StripeCustomerId is set in UserSubscriptions table

3. **Webhook not receiving events**
   - Verify webhook endpoint URL is correct
   - Check webhook secret matches in appsettings
   - Ensure endpoint is accessible (not behind firewall)

4. **Payment method attachment fails**
   - Verify payment method ID is valid
   - Check customer exists in Stripe
   - Ensure payment method hasn't been used elsewhere

## Support

For Stripe-specific issues, refer to:
- [Stripe Documentation](https://stripe.com/docs)
- [Stripe API Reference](https://stripe.com/docs/api)
- [Stripe Support](https://support.stripe.com)

For application-specific issues, check the application logs and error messages.

