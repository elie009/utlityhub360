# Subscription Access Restrictions - Complete Documentation

## 📋 Table of Contents

1. [Overview](#overview)
2. [Feature Matrix](#feature-matrix)
3. [Quick Start](#quick-start)
4. [Backend Implementation](#backend-implementation)
5. [Frontend Implementation](#frontend-implementation)
6. [API Endpoints & Restrictions](#api-endpoints--restrictions)
7. [Usage Examples](#usage-examples)
8. [Testing Guide](#testing-guide)
9. [Migration Guide](#migration-guide)
10. [Troubleshooting](#troubleshooting)
11. [Best Practices](#best-practices)

---

## Overview

UtilityHub360 implements tier-based access restrictions across three subscription tiers:

- **Free (STARTER)**: Basic features with limited usage
- **Premium (PROFESSIONAL)**: Advanced features with unlimited usage
- **Premium Plus (ENTERPRISE)**: All features including enterprise capabilities

### Key Concepts

- **Feature Flags**: Boolean flags in subscription plans that control feature access
- **Usage Limits**: Numeric limits (e.g., 10 AI queries/month for Free tier)
- **Feature Checks**: Backend and frontend validation before allowing feature access

---

## Feature Matrix

| Feature | Free | Premium | Premium Plus |
|---------|------|---------|--------------|
| Dashboard | ✅ | ✅ | ✅ |
| Bank Accounts | 3 max | Unlimited | Unlimited |
| Transactions | Unlimited | Unlimited | Unlimited |
| Bills | 5 max | Unlimited | Unlimited |
| Loans | 5 max | Unlimited | Unlimited |
| Savings Goals | 5 max | Unlimited | Unlimited |
| Basic Reports | ✅ | ✅ | ✅ |
| Advanced Reports | ❌ | ✅ | ✅ |
| AI Assistant | 10 queries/mo | Unlimited | Unlimited |
| Bank Feeds | ❌ | ✅ | ✅ |
| Receipt OCR | ❌ | ✅ | ✅ |
| Financial Health Score | ❌ | ✅ | ✅ |
| Bill Forecasting | ❌ | ✅ | ✅ |
| Debt Optimizer | ❌ | ✅ | ✅ |
| Investment Tracking | ❌ | ❌ | ✅ |
| Tax Optimization | ❌ | ❌ | ✅ |
| Multi-User Support | ❌ | ❌ | ✅ |
| API Access | ❌ | ❌ | ✅ |
| White-Label | ❌ | ❌ | ✅ |
| Support | Email (48h) | Priority (24h) | Dedicated (4h) |

---

## Quick Start

### Backend Feature Check
```csharp
var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "FEATURE_NAME");
if (!featureCheck.Success || !featureCheck.Data)
{
    return BadRequest(ApiResponse<Dto>.ErrorResult(
        "Feature is a Premium feature. Please upgrade to Premium."));
}
```

### Frontend Feature Check
```typescript
const { hasAccess, loading } = usePremiumFeature('FEATURE_NAME');
if (!hasAccess) return <UpgradePrompt />;
```

### Feature Names Reference

| Feature | Feature Name Constant | Tier |
|---------|---------------------|------|
| AI Assistant | `AI_ASSISTANT` | Premium+ |
| Bank Feed Integration | `BANK_FEED` | Premium+ |
| Receipt OCR | `RECEIPT_OCR` | Premium+ |
| Advanced Reports | `ADVANCED_REPORTS` | Premium+ |
| Financial Health Score | `FINANCIAL_HEALTH_SCORE` | Premium+ |
| Bill Forecasting | `BILL_FORECASTING` | Premium+ |
| Debt Optimizer | `DEBT_OPTIMIZER` | Premium+ |
| Investment Tracking | `INVESTMENT_TRACKING` | Enterprise |
| Tax Optimization | `TAX_OPTIMIZATION` | Enterprise |
| Multi-User Support | `MULTI_USER` | Enterprise |
| API Access | `API_ACCESS` | Enterprise |
| White-Label | `WHITE_LABEL` | Enterprise |

### Limit Types Reference

| Limit | Limit Type Constant | Free Limit |
|-------|-------------------|------------|
| Bank Accounts | `BANK_ACCOUNTS` | 3 |
| Bills per Month | `BILLS` | 5 |
| Loans | `LOANS` | 5 |
| Savings Goals | `SAVINGS_GOALS` | 5 |
| AI Queries per Month | `AI_QUERIES` | 10 |
| Receipt OCR per Month | `RECEIPT_OCR` | 0 |
| API Calls per Month | `API_CALLS` | 0 |
| Transactions per Month | `TRANSACTIONS` | 1000 |

---

## Backend Implementation

### 1. Database Schema

#### Subscription Plans Table

```sql
-- Feature flags in SubscriptionPlans table
HasAiAssistant BIT NOT NULL DEFAULT 0
HasBankFeedIntegration BIT NOT NULL DEFAULT 0
HasReceiptOcr BIT NOT NULL DEFAULT 0
HasAdvancedReports BIT NOT NULL DEFAULT 0
HasFinancialHealthScore BIT NOT NULL DEFAULT 0
HasBillForecasting BIT NOT NULL DEFAULT 0
HasDebtOptimizer BIT NOT NULL DEFAULT 0
HasInvestmentTracking BIT NOT NULL DEFAULT 0
HasTaxOptimization BIT NOT NULL DEFAULT 0
HasMultiUserSupport BIT NOT NULL DEFAULT 0
HasApiAccess BIT NOT NULL DEFAULT 0
HasWhiteLabelOptions BIT NOT NULL DEFAULT 0

-- Usage limits
MaxAiQueriesPerMonth INT NULL  -- NULL = unlimited
MaxBankAccounts INT NULL
MaxBillsPerMonth INT NULL
MaxLoans INT NULL
MaxSavingsGoals INT NULL
```

**Migration Script**: `Documentation/Database/Scripts/add_tier_feature_flags.sql`

### 2. Feature Check Service

#### SubscriptionService.CheckFeatureAccessAsync

**Location**: `Services/SubscriptionService.cs`

**Method Signature**:
```csharp
public async Task<ApiResponse<bool>> CheckFeatureAccessAsync(string userId, string feature)
```

**Supported Feature Names**:
- `AI_ASSISTANT`
- `BANK_FEED`
- `RECEIPT_OCR`
- `ADVANCED_REPORTS`
- `FINANCIAL_HEALTH_SCORE`
- `BILL_FORECASTING`
- `DEBT_OPTIMIZER`
- `INVESTMENT_TRACKING`
- `TAX_OPTIMIZATION`
- `MULTI_USER`
- `API_ACCESS`
- `WHITE_LABEL`

**Usage Example**:
```csharp
var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "BILL_FORECASTING");
if (!featureCheck.Success || !featureCheck.Data)
{
    return BadRequest(ApiResponse<BillForecastDto>.ErrorResult(
        "Bill Forecasting is a Premium feature. Please upgrade to Premium to access this feature."));
}
```

### 3. Usage Limit Check Service

#### SubscriptionService.CheckLimitAsync

**Location**: `Services/SubscriptionService.cs`

**Method Signature**:
```csharp
public async Task<ApiResponse<bool>> CheckLimitAsync(string userId, string limitType, int currentCount)
```

**Supported Limit Types**:
- `BANK_ACCOUNTS`
- `TRANSACTIONS`
- `BILLS`
- `LOANS`
- `SAVINGS_GOALS`
- `RECEIPT_OCR`
- `AI_QUERIES`
- `API_CALLS`
- `USERS`

**Usage Example**:
```csharp
var limitCheck = await _subscriptionService.CheckLimitAsync(userId, "BANK_ACCOUNTS", currentBankAccountCount);
if (!limitCheck.Success || !limitCheck.Data)
{
    return BadRequest(ApiResponse<BankAccountDto>.ErrorResult(
        "You have reached your bank account limit. Please upgrade to Premium for unlimited bank accounts."));
}
```

### 4. Controller Implementation Pattern

#### Standard Feature Check Pattern

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<FeatureDto>>> CreateFeature([FromBody] CreateFeatureDto dto)
{
    try
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<FeatureDto>.ErrorResult("User not authenticated"));
        }

        // 1. Check feature access
        var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "FEATURE_NAME");
        if (!featureCheck.Success || !featureCheck.Data)
        {
            return BadRequest(ApiResponse<FeatureDto>.ErrorResult(
                "Feature Name is a Premium feature. Please upgrade to Premium to access this feature."));
        }

        // 2. Check usage limits (if applicable)
        var currentCount = await GetCurrentFeatureCountAsync(userId);
        var limitCheck = await _subscriptionService.CheckLimitAsync(userId, "FEATURE_TYPE", currentCount);
        if (!limitCheck.Success || !limitCheck.Data)
        {
            return BadRequest(ApiResponse<FeatureDto>.ErrorResult(
                "You have reached your feature limit. Please upgrade to Premium for unlimited access."));
        }

        // 3. Proceed with feature implementation
        var result = await _featureService.CreateFeatureAsync(dto, userId);
        return Ok(result);
    }
    catch (Exception ex)
    {
        return BadRequest(ApiResponse<FeatureDto>.ErrorResult($"Failed to create feature: {ex.Message}"));
    }
}
```

### 5. AI Query Limit Check (Special Case)

For AI Assistant with monthly query limits:

```csharp
// Check if user has access to AI Assistant feature
var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "AI_ASSISTANT");
if (!featureCheck.Success || !featureCheck.Data)
{
    return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult(
        "AI Assistant is a Premium feature. Please upgrade to Premium to access this feature."));
}

// Check AI query limit for Free tier (10 queries/month)
var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
if (subscription?.Success == true && subscription.Data != null)
{
    var userSubscription = subscription.Data;
    var planResult = await _subscriptionService.GetSubscriptionPlanAsync(userSubscription.SubscriptionPlanId);
    if (planResult?.Success == true && planResult.Data != null)
    {
        var plan = planResult.Data;
        
        // If plan has a limit on AI queries, check it
        if (plan.MaxAiQueriesPerMonth.HasValue)
        {
            if (userSubscription.AiQueriesThisMonth >= plan.MaxAiQueriesPerMonth.Value)
            {
                return BadRequest(ApiResponse<ChatResponseDto>.ErrorResult(
                    $"You have reached your monthly AI query limit ({plan.MaxAiQueriesPerMonth.Value} queries/month). Please upgrade to Premium for unlimited AI queries."));
            }
        }
    }
}
```

### 6. Subscription Plan Configuration

#### Free (STARTER)
- `MaxAiQueriesPerMonth`: 10
- `MaxBankAccounts`: 3
- `MaxBillsPerMonth`: 5
- `MaxLoans`: 5
- `MaxSavingsGoals`: 5
- All premium features: `false`

#### Premium (PROFESSIONAL)
- All limits: `NULL` (unlimited)
- `MaxReceiptOcrPerMonth`: 50
- `HasAiAssistant`: `true`
- `HasBankFeedIntegration`: `true`
- `HasReceiptOcr`: `true`
- `HasAdvancedReports`: `true`
- `HasFinancialHealthScore`: `true`
- `HasBillForecasting`: `true`
- `HasDebtOptimizer`: `true`
- `HasPrioritySupport`: `true`
- Enterprise features: `false`

#### Premium Plus (ENTERPRISE)
- All limits: `NULL` (unlimited)
- `MaxApiCallsPerMonth`: 1000
- `MaxUsers`: 5
- All features: `true`

---

## Frontend Implementation

### 1. Feature Check Hook

#### usePremiumFeature Hook

**Location**: `src/hooks/usePremiumFeature.ts`

**Usage**:
```typescript
import { usePremiumFeature } from '../hooks/usePremiumFeature';

const MyComponent: React.FC = () => {
  const { hasAccess, loading, error } = usePremiumFeature('BILL_FORECASTING');
  
  if (loading) return <Loading />;
  if (!hasAccess) {
    return <UpgradePrompt featureName="Bill Forecasting" />;
  }
  
  return <BillForecastingComponent />;
};
```

**Supported Feature Names**: Same as backend (see Feature Names Reference above)

### 2. Subscription Hook

#### useSubscription Hook

**Location**: `src/hooks/usePremiumFeature.ts`

**Usage**:
```typescript
import { useSubscription } from '../hooks/usePremiumFeature';

const MyComponent: React.FC = () => {
  const { subscription, isPremium, isEnterprise, isFree, loading } = useSubscription();
  
  if (loading) return <Loading />;
  
  if (isFree) {
    return <FreeTierComponent />;
  }
  
  if (isPremium && !isEnterprise) {
    return <PremiumTierComponent />;
  }
  
  if (isEnterprise) {
    return <EnterpriseTierComponent />;
  }
};
```

**Return Values**:
- `subscription`: User's subscription object
- `isPremium`: `true` if user has Premium or Enterprise
- `isEnterprise`: `true` if user has Enterprise
- `isFree`: `true` if user has Free tier or no subscription
- `loading`: Loading state
- `error`: Error state

### 3. Component Implementation Patterns

#### Pattern 1: Conditional Rendering

```typescript
import { usePremiumFeature } from '../hooks/usePremiumFeature';
import UpgradePrompt from '../components/Subscription/UpgradePrompt';

const FeatureComponent: React.FC = () => {
  const { hasAccess, loading } = usePremiumFeature('FEATURE_NAME');
  const [showUpgrade, setShowUpgrade] = useState(false);

  if (loading) return <Loading />;
  
  if (!hasAccess) {
    return (
      <>
        <UpgradePrompt
          open={showUpgrade}
          onClose={() => setShowUpgrade(false)}
          featureName="Feature Name"
          featureDescription="Description of the feature"
          premiumFeatures={[
            'Feature Name',
            'Other Premium Features'
          ]}
        />
        <Box>
          <Alert severity="info">
            This feature requires Premium subscription.
            <Button onClick={() => setShowUpgrade(true)}>Upgrade Now</Button>
          </Alert>
        </Box>
      </>
    );
  }

  // Render feature UI
  return <FeatureUI />;
};
```

#### Pattern 2: Hide Feature

```typescript
const Sidebar: React.FC = () => {
  const { hasAccess: hasForecast } = usePremiumFeature('BILL_FORECASTING');
  const { hasAccess: hasInvestment } = usePremiumFeature('INVESTMENT_TRACKING');
  
  return (
    <List>
      <MenuItem>Dashboard</MenuItem>
      {hasForecast && <MenuItem>Bill Forecasting</MenuItem>}
      {hasInvestment && <MenuItem>Investment Tracking</MenuItem>}
    </List>
  );
};
```

#### Pattern 3: Disable Feature

```typescript
const MyComponent: React.FC = () => {
  const { hasAccess } = usePremiumFeature('ADVANCED_REPORTS');
  const [showUpgrade, setShowUpgrade] = useState(false);
  
  return (
    <>
      <Button
        disabled={!hasAccess}
        onClick={() => {
          if (!hasAccess) {
            setShowUpgrade(true);
          } else {
            handleAdvancedReport();
          }
        }}
      >
        Generate Advanced Report
      </Button>
      <UpgradePrompt
        open={showUpgrade}
        onClose={() => setShowUpgrade(false)}
        featureName="Advanced Reports"
      />
    </>
  );
};
```

#### Pattern 4: Menu Item Filtering

```typescript
const menuItems = [
  { text: 'Dashboard', path: '/', tier: 'all' },
  { text: 'Bill Forecasting', path: '/forecast', tier: 'premium' },
  { text: 'Investment Tracking', path: '/investments', tier: 'enterprise' },
];

const Sidebar: React.FC = () => {
  const { isPremium, isEnterprise } = useSubscription();
  
  const visibleItems = menuItems.filter(item => {
    if (item.tier === 'enterprise' && !isEnterprise) return false;
    if (item.tier === 'premium' && !isPremium) return false;
    return true;
  });
  
  return (
    <List>
      {visibleItems.map(item => (
        <MenuItem key={item.path}>{item.text}</MenuItem>
      ))}
    </List>
  );
};
```

---

## API Endpoints & Restrictions

### Restricted Endpoints

| Endpoint | Method | Feature Check | Limit Check |
|----------|--------|---------------|-------------|
| `/api/chat/message` | POST | `AI_ASSISTANT` | `AI_QUERIES` (10/month for Free) |
| `/api/chat/generate-report` | POST | `ADVANCED_REPORTS` | - |
| `/api/receipts/upload` | POST | `RECEIPT_OCR` | `RECEIPT_OCR` |
| `/api/receipts/{id}/process-ocr` | POST | `RECEIPT_OCR` | `RECEIPT_OCR` |
| `/api/reconciliation/statements/extract` | POST | `BANK_FEED` | - |
| `/api/reconciliation/statements/analyze-pdf` | POST | `BANK_FEED` | - |
| `/api/bills/analytics/forecast` | GET | `BILL_FORECASTING` | - |
| `/api/bills` | POST | - | `BILLS` (5/month for Free) |
| `/api/bankaccounts` | POST | - | `BANK_ACCOUNTS` (3 max for Free) |
| `/api/loans` | POST | - | `LOANS` (5 max for Free) |
| `/api/savings` | POST | - | `SAVINGS_GOALS` (5 max for Free) |

### Error Responses

All restricted endpoints return consistent error responses:

```json
{
  "success": false,
  "message": "Feature Name is a Premium feature. Please upgrade to Premium to access this feature.",
  "data": null
}
```

**HTTP Status Codes**:
- `400 Bad Request`: Feature not available or limit reached
- `401 Unauthorized`: User not authenticated
- `403 Forbidden`: Feature access denied (rare, usually returns 400)

### Error Message Templates

- **Feature Not Available**: `"{Feature} is a {Tier} feature. Please upgrade to {Tier} to access this feature."`
- **Limit Reached**: `"You have reached your {limit} limit. Please upgrade to Premium for unlimited {feature}."`
- **AI Query Limit**: `"You have reached your monthly AI query limit ({limit} queries/month). Please upgrade to Premium for unlimited AI queries."`

---

## Usage Examples

### Example 1: Bill Forecasting Feature

#### Backend Controller

```csharp
[HttpGet("analytics/forecast")]
public async Task<ActionResult<ApiResponse<BillForecastDto>>> GetForecast(
    [FromQuery] string provider,
    [FromQuery] string billType)
{
    var userId = GetUserId();
    
    // Check feature access
    var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "BILL_FORECASTING");
    if (!featureCheck.Success || !featureCheck.Data)
    {
        return BadRequest(ApiResponse<BillForecastDto>.ErrorResult(
            "Bill Forecasting is a Premium feature. Please upgrade to Premium to access this feature."));
    }
    
    var result = await _billAnalyticsService.GetForecastAsync(userId, provider, billType);
    return Ok(result);
}
```

#### Frontend Component

```typescript
import { usePremiumFeature } from '../hooks/usePremiumFeature';
import UpgradePrompt from '../components/Subscription/UpgradePrompt';

const BillForecastPage: React.FC = () => {
  const { hasAccess, loading } = usePremiumFeature('BILL_FORECASTING');
  const [showUpgrade, setShowUpgrade] = useState(false);
  
  if (loading) return <Loading />;
  
  if (!hasAccess) {
    return (
      <>
        <UpgradePrompt
          open={showUpgrade}
          onClose={() => setShowUpgrade(false)}
          featureName="Bill Forecasting"
          featureDescription="Predict future bill amounts using advanced analytics"
        />
        <Alert severity="info">
          Bill Forecasting is a Premium feature.
          <Button onClick={() => setShowUpgrade(true)}>Upgrade to Premium</Button>
        </Alert>
      </>
    );
  }
  
  return <BillForecastComponent />;
};
```

### Example 2: Bank Account Limit Check

#### Backend Controller

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<BankAccountDto>>> CreateBankAccount([FromBody] CreateBankAccountDto dto)
{
    var userId = GetUserId();
    
    // Check limit
    var currentCount = await _context.BankAccounts
        .CountAsync(ba => ba.UserId == userId && ba.IsActive);
    
    var limitCheck = await _subscriptionService.CheckLimitAsync(userId, "BANK_ACCOUNTS", currentCount);
    if (!limitCheck.Success || !limitCheck.Data)
    {
        return BadRequest(ApiResponse<BankAccountDto>.ErrorResult(
            "You have reached your bank account limit. Please upgrade to Premium for unlimited bank accounts."));
    }
    
    var result = await _bankAccountService.CreateBankAccountAsync(dto, userId);
    return Ok(result);
}
```

### Example 3: Enterprise-Only Feature

#### Backend Controller

```csharp
[HttpPost("investments")]
public async Task<ActionResult<ApiResponse<InvestmentDto>>> CreateInvestment([FromBody] CreateInvestmentDto dto)
{
    var userId = GetUserId();
    
    // Check enterprise feature
    var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "INVESTMENT_TRACKING");
    if (!featureCheck.Success || !featureCheck.Data)
    {
        return BadRequest(ApiResponse<InvestmentDto>.ErrorResult(
            "Investment Tracking is an Enterprise feature. Please upgrade to Enterprise to access this feature."));
    }
    
    var result = await _investmentService.CreateInvestmentAsync(dto, userId);
    return Ok(result);
}
```

---

## Testing Guide

### 1. Backend Testing

#### Test Feature Access Check

```csharp
[Fact]
public async Task CheckFeatureAccess_FreeUser_ShouldReturnFalse()
{
    // Arrange
    var userId = "free-user-id";
    var feature = "BILL_FORECASTING";
    
    // Act
    var result = await _subscriptionService.CheckFeatureAccessAsync(userId, feature);
    
    // Assert
    Assert.True(result.Success);
    Assert.False(result.Data); // Free users don't have access
}
```

#### Test Usage Limit Check

```csharp
[Fact]
public async Task CheckLimit_FreeUserAtLimit_ShouldReturnFalse()
{
    // Arrange
    var userId = "free-user-id";
    var limitType = "BANK_ACCOUNTS";
    var currentCount = 3; // Free tier limit
    
    // Act
    var result = await _subscriptionService.CheckLimitAsync(userId, limitType, currentCount);
    
    // Assert
    Assert.True(result.Success);
    Assert.False(result.Data); // At limit
}
```

### 2. Frontend Testing

#### Test Feature Hook

```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { usePremiumFeature } from '../hooks/usePremiumFeature';

test('usePremiumFeature returns false for free user', async () => {
  const { result } = renderHook(() => usePremiumFeature('BILL_FORECASTING'));
  
  await waitFor(() => {
    expect(result.current.loading).toBe(false);
    expect(result.current.hasAccess).toBe(false);
  });
});
```

### 3. Integration Testing

#### Test API Endpoint Restriction

```http
### Test Free User - Should Fail
POST /api/bills/analytics/forecast?provider=Meralco&billType=utility
Authorization: Bearer {free-user-token}

### Expected Response
HTTP 400 Bad Request
{
  "success": false,
  "message": "Bill Forecasting is a Premium feature. Please upgrade to Premium to access this feature."
}

### Test Premium User - Should Succeed
POST /api/bills/analytics/forecast?provider=Meralco&billType=utility
Authorization: Bearer {premium-user-token}

### Expected Response
HTTP 200 OK
{
  "success": true,
  "data": { ... }
}
```

### 4. Testing Checklist

- [ ] Free user cannot access premium features
- [ ] Free user cannot access enterprise features
- [ ] Premium user can access premium features
- [ ] Premium user cannot access enterprise features
- [ ] Enterprise user can access all features
- [ ] Limits are enforced correctly
- [ ] Error messages are user-friendly
- [ ] Upgrade prompts appear correctly
- [ ] Menu items are hidden for unavailable features

---

## Migration Guide

### 1. Database Migration

Run the migration script to add new feature flags:

```sql
-- File: Documentation/Database/Scripts/add_tier_feature_flags.sql
-- This adds HasFinancialHealthScore, HasBillForecasting, HasDebtOptimizer columns
```

**Steps**:
1. Connect to your database
2. Run the migration script: `add_tier_feature_flags.sql`
3. Verify columns were added successfully
4. Verify subscription plans were updated with correct feature flags

### 2. Update Existing Subscriptions

Existing users will automatically:
- Default to Free tier restrictions if no subscription exists
- Maintain their current tier's features
- Have new features enabled/disabled based on their tier

### 3. Frontend Updates

1. **Update Components**: Add feature checks to components using restricted features
2. **Update Navigation**: Hide menu items for unavailable features
3. **Add Upgrade Prompts**: Show upgrade prompts when users try to access restricted features

### 4. Code Updates Required

#### Backend
- ✅ Entity updates (SubscriptionPlan.cs)
- ✅ DTO updates (SubscriptionDto.cs)
- ✅ Service updates (SubscriptionService.cs)
- ✅ Controller updates (ChatController, BillsController, etc.)

#### Frontend
- ✅ Type updates (subscription.ts)
- ✅ Hook updates (usePremiumFeature.ts)
- ⚠️ Component updates (add feature checks as needed)
- ⚠️ Navigation updates (hide restricted menu items)

---

## Troubleshooting

### Common Issues

#### 1. Feature Check Always Returns False

**Problem**: Feature check returns false even for premium users

**Solution**:
- Verify subscription status is "ACTIVE"
- Check that subscription plan has the feature flag set to `true`
- Ensure user has an active subscription record
- Check database to verify subscription plan configuration

#### 2. Limits Not Enforcing

**Problem**: Users can exceed their tier limits

**Solution**:
- Verify limit checks are called before creating resources
- Check that `CheckLimitAsync` is using correct limit type
- Ensure subscription plan has limits set (not NULL for free tier)
- Verify current count calculation is correct

#### 3. Frontend Hook Not Working

**Problem**: `usePremiumFeature` always returns loading or false

**Solution**:
- Check that user is authenticated
- Verify API endpoint `/api/Subscription/check-feature` is accessible
- Check browser console for errors
- Verify subscription data is loaded correctly
- Check network tab for API call failures

#### 4. AI Query Limit Not Working

**Problem**: Free users can exceed 10 queries/month

**Solution**:
- Verify `AiQueriesThisMonth` is being incremented after each query
- Check that limit check is performed before processing the query
- Verify `MaxAiQueriesPerMonth` is set to 10 for Free tier
- Check that usage is reset monthly

---

## Best Practices

1. **Always Check Features**: Never assume a user has access to a feature
2. **User-Friendly Messages**: Provide clear error messages with upgrade options
3. **Graceful Degradation**: Hide features rather than showing error messages when possible
4. **Consistent Naming**: Use consistent feature names across backend and frontend
5. **Test All Tiers**: Test features with Free, Premium, and Enterprise users
6. **Monitor Usage**: Track feature usage to understand user behavior
7. **Error Handling**: Always handle errors gracefully in both backend and frontend
8. **Loading States**: Show loading indicators while checking feature access
9. **Cache Strategically**: Cache subscription data but refresh when needed
10. **Document Changes**: Update documentation when adding new features or restrictions

---

## Related Files

### Backend
- **Service**: `Services/SubscriptionService.cs`
- **Entity**: `Entities/SubscriptionPlan.cs`
- **DTOs**: `DTOs/SubscriptionDto.cs`
- **Controllers**: 
  - `Controllers/ChatController.cs`
  - `Controllers/BillsController.cs`
  - `Controllers/ReceiptsController.cs`
  - `Controllers/ReconciliationController.cs`
- **Database Scripts**:
  - `Documentation/Database/Scripts/create_subscription_tables.sql`
  - `Documentation/Database/Scripts/add_tier_feature_flags.sql`

### Frontend
- **Hook**: `src/hooks/usePremiumFeature.ts`
- **Types**: `src/types/subscription.ts`
- **Component**: `src/components/Subscription/UpgradePrompt.tsx`
- **Modal**: `src/components/Subscription/SubscriptionModal.tsx`

---

## Support

For questions or issues with access restrictions:

1. Check this documentation
2. Review the implementation in `Services/SubscriptionService.cs`
3. Check controller implementations for examples
4. Review frontend hooks in `src/hooks/usePremiumFeature.ts`
5. Check database subscription plan configurations

---

**Last Updated**: 2025-01-XX  
**Version**: 1.0.0
