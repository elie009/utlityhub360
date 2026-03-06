# Plaid Integration Implementation

## Overview
This document describes the Plaid MVP integration that has been implemented for bank feed connectivity in UtilityHub360.

## What Was Implemented

### Backend

1. **Plaid SDK Integration**
   - Added `Plaid` NuGet package (v18.0.0) to the project
   - Created `PlaidService` class that wraps Plaid API calls
   - Implemented methods for:
     - Creating Link tokens
     - Exchanging public tokens for access tokens
     - Fetching accounts
     - Fetching transactions
     - Removing items (disconnecting accounts)

2. **BankFeedService Updates**
   - Updated `BankFeedService` to use `PlaidService` instead of mock data
   - Implemented `StorePlaidAccessTokenAsync` method to store access tokens after Link flow
   - Updated `FetchTransactionsAsync` to fetch real transactions from Plaid
   - Updated `DisconnectAccountAsync` to remove items from Plaid

3. **API Endpoints**
   - `POST /api/plaid/link-token` - Creates a Link token for Plaid Link initialization
   - `POST /api/plaid/exchange-token` - Exchanges public token for access token and connects account

4. **Configuration**
   - Added `PlaidSettings` model
   - Added Plaid configuration section to `appsettings.Production.json`

### Frontend

1. **Plaid Link Integration**
   - Added `react-plaid-link` package (v3.4.0)
   - Created `PlaidLinkDialog` component for the Plaid Link flow
   - Updated `BankAccounts` page to use Plaid Link when connecting accounts
   - Added API service methods for Plaid operations

2. **User Experience**
   - Users can click "Connect" on a bank account card
   - Plaid Link dialog opens with secure connection flow
   - After successful connection, account is automatically synced

## Configuration

### Backend Configuration

Add the following to your `appsettings.json` or `appsettings.Production.json`:

```json
{
  "Plaid": {
    "ClientId": "YOUR_PLAID_CLIENT_ID",
    "Secret": "YOUR_PLAID_SECRET",
    "Environment": "sandbox"
  }
}
```

**Environments:**
- `sandbox` - For testing (default)
- `development` - For development
- `production` - For production

### Getting Plaid Credentials

1. Sign up for a Plaid account at https://plaid.com
2. Navigate to the Dashboard
3. Get your Client ID and Secret from the API Keys section
4. For sandbox testing, use the test credentials provided by Plaid

## How It Works

### Connection Flow

1. User clicks "Connect" on a bank account
2. Frontend requests a Link token from `/api/plaid/link-token`
3. Plaid Link dialog opens with the token
4. User authenticates with their bank through Plaid
5. Plaid returns a public token
6. Frontend exchanges public token for access token via `/api/plaid/exchange-token`
7. Backend stores access token in `BankAccount.ConnectionId`
8. Account is marked as connected

### Transaction Sync Flow

1. Background service (`BankAccountSyncBackgroundService`) runs periodically
2. For each connected account, it calls `BankFeedService.FetchTransactionsAsync`
3. `BankFeedService` uses `PlaidService` to fetch transactions from Plaid
4. Transactions are mapped to `BankFeedTransactionDto` format
5. Transactions can then be imported into the system

## Features

- ✅ Secure bank connection via Plaid Link
- ✅ Automatic transaction fetching
- ✅ Account balance updates
- ✅ Support for US and Canadian banks
- ✅ Premium feature gating (requires `BANK_FEED` feature access)

## Testing

### Sandbox Testing

Plaid provides sandbox credentials for testing:
- Use test credentials from Plaid Dashboard
- Test with Plaid's test institutions (e.g., "First Platypus Bank")
- Use test credentials: `user_good` / `pass_good`

### Production

1. Update `Environment` to `production` in appsettings
2. Use production Client ID and Secret
3. Ensure webhook URL is configured if using webhooks

## Next Steps

1. **Configure Plaid Credentials**: Add your Plaid Client ID and Secret to appsettings
2. **Test Connection**: Use sandbox environment to test the flow
3. **Set Up Webhooks** (Optional): Configure webhook URL for real-time updates
4. **Production Deployment**: Switch to production environment when ready

## Notes

- Access tokens are stored in `BankAccount.ConnectionId` field
- The integration supports both manual and automatic syncing
- Premium subscription is required to access bank feed features
- Transactions are fetched for the last 30 days by default

## Troubleshooting

### Common Issues

1. **"Failed to create Link token"**
   - Check Plaid credentials in appsettings
   - Verify environment is correct
   - Check user has `BANK_FEED` feature access

2. **"Failed to exchange token"**
   - Ensure public token is valid
   - Check Plaid API status
   - Verify account exists

3. **"No transactions found"**
   - Check account is connected
   - Verify date range
   - Check Plaid account has transactions

## API Documentation

See Swagger UI at `/swagger` for detailed API documentation.

