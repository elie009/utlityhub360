# Savings System Documentation

## Overview

The Savings System is a comprehensive feature that allows users to create dedicated savings accounts with specific goals, track their progress, and manage savings transactions. It integrates seamlessly with the existing Bank Account system to provide a complete financial management solution.

## Key Features

### 1. **Dedicated Savings Accounts**
- Create multiple savings accounts for different goals
- Set target amounts and target dates
- Track progress with visual indicators
- Categorize by savings type (Emergency, Vacation, Investment, etc.)

### 2. **Savings Transactions**
- Transfer money from bank accounts to savings
- Withdraw from savings to bank accounts
- Track all savings-related transactions
- Support for recurring savings

### 3. **Goal Tracking**
- Progress percentage calculation
- Remaining amount and days
- Monthly savings targets
- Goal completion status

### 4. **Analytics & Reporting**
- Savings summary across all accounts
- Analytics by period (week, month, quarter, year)
- Savings by type and category
- Monthly savings trends

### 5. **Bank Account Integration**
- Seamless transfers between bank and savings accounts
- Automatic balance updates
- Transaction history tracking

## Database Schema

### SavingsAccount Entity
```csharp
public class SavingsAccount
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string AccountName { get; set; }
    public string SavingsType { get; set; } // EMERGENCY, VACATION, etc.
    public decimal TargetAmount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; }
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public DateTime TargetDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### SavingsTransaction Entity
```csharp
public class SavingsTransaction
{
    public string Id { get; set; }
    public string SavingsAccountId { get; set; }
    public string SourceBankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } // DEPOSIT, WITHDRAWAL, TRANSFER
    public string Description { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Currency { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurringFrequency { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## API Endpoints

### Savings Account Management

#### Create Savings Account
```http
POST /api/Savings/accounts
Authorization: Bearer {token}
Content-Type: application/json

{
  "accountName": "Emergency Fund",
  "savingsType": "EMERGENCY",
  "targetAmount": 10000.00,
  "description": "6 months emergency savings",
  "goal": "Build emergency fund for unexpected expenses",
  "targetDate": "2024-12-31T00:00:00Z",
  "currency": "USD"
}
```

#### Get User Savings Accounts
```http
GET /api/Savings/accounts
Authorization: Bearer {token}
```

#### Get Specific Savings Account
```http
GET /api/Savings/accounts/{savingsAccountId}
Authorization: Bearer {token}
```

#### Update Savings Account
```http
PUT /api/Savings/accounts/{savingsAccountId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "accountName": "Updated Emergency Fund",
  "savingsType": "EMERGENCY",
  "targetAmount": 15000.00,
  "description": "Updated emergency savings goal",
  "goal": "Build larger emergency fund",
  "targetDate": "2025-06-30T00:00:00Z",
  "currency": "USD"
}
```

#### Delete Savings Account
```http
DELETE /api/Savings/accounts/{savingsAccountId}
Authorization: Bearer {token}
```

### Savings Transactions

#### Create Savings Transaction
```http
POST /api/Savings/transactions
Authorization: Bearer {token}
Content-Type: application/json

{
  "savingsAccountId": "savings-account-id",
  "sourceBankAccountId": "bank-account-id",
  "amount": 500.00,
  "transactionType": "DEPOSIT",
  "description": "Monthly savings contribution",
  "category": "MONTHLY_SAVINGS",
  "notes": "20% of salary saved",
  "transactionDate": "2024-01-15T00:00:00Z",
  "currency": "USD",
  "isRecurring": true,
  "recurringFrequency": "MONTHLY"
}
```

#### Get Savings Transactions
```http
GET /api/Savings/accounts/{savingsAccountId}/transactions?page=1&limit=50
Authorization: Bearer {token}
```

#### Get Specific Transaction
```http
GET /api/Savings/transactions/{transactionId}
Authorization: Bearer {token}
```

### Analytics & Summary

#### Get Savings Summary
```http
GET /api/Savings/summary
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalSavingsAccounts": 3,
    "totalSavingsBalance": 2500.00,
    "totalTargetAmount": 15000.00,
    "overallProgressPercentage": 16.67,
    "activeGoals": 2,
    "completedGoals": 1,
    "monthlySavingsTarget": 1250.00,
    "thisMonthSaved": 500.00,
    "recentAccounts": [...]
  },
  "message": "Savings summary retrieved successfully"
}
```

**Alternative: Get Total Savings via Financial Reports**

You can also get your total savings amount as part of a comprehensive financial summary:

```http
GET /api/Reports/summary
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalIncome": 8000.00,
    "totalExpenses": 4500.00,
    "disposableIncome": 3500.00,
    "totalSavings": 5000.00,
    "savingsGoal": 10000.00,
    "savingsProgress": 50.0,
    "netWorth": 25000.00,
    ...
  }
}
```

**When to use which endpoint:**
- Use `GET /api/Savings/summary` when you need detailed savings-specific information (accounts, goals, progress, etc.)
- Use `GET /api/Reports/summary` when you need total savings along with other financial metrics (income, expenses, net worth) in a single call

#### Get Savings Analytics
```http
GET /api/Savings/analytics?period=month
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalSaved": 1000.00,
    "totalWithdrawn": 200.00,
    "netSavings": 800.00,
    "totalTransactions": 5,
    "periodStart": "2024-01-01T00:00:00Z",
    "periodEnd": "2024-01-31T23:59:59Z",
    "savingsByType": {
      "EMERGENCY": 600.00,
      "VACATION": 400.00
    },
    "savingsByCategory": {
      "MONTHLY_SAVINGS": 800.00,
      "BONUS": 200.00
    },
    "recentTransactions": [...],
    "averageMonthlySavings": 1000.00,
    "averageTransactionAmount": 200.00
  },
  "message": "Savings analytics retrieved successfully"
}
```

#### Get Savings by Type
```http
GET /api/Savings/by-type
Authorization: Bearer {token}
```

### Goal Management

#### Update Savings Goal
```http
PUT /api/Savings/accounts/{savingsAccountId}/goal
Authorization: Bearer {token}
Content-Type: application/json

{
  "targetAmount": 20000.00,
  "targetDate": "2025-12-31T00:00:00Z"
}
```

#### Get Savings Goals by Type
```http
GET /api/Savings/goals/{savingsType}
Authorization: Bearer {token}
```

#### Get Total Savings Progress
```http
GET /api/Savings/progress
Authorization: Bearer {token}
```

### Bank Account Integration

#### Transfer from Bank to Savings
```http
POST /api/Savings/transfer/bank-to-savings
Authorization: Bearer {token}
Content-Type: application/json

{
  "bankAccountId": "bank-account-id",
  "savingsAccountId": "savings-account-id",
  "amount": 1000.00,
  "description": "Monthly savings transfer"
}
```

#### Transfer from Savings to Bank
```http
POST /api/Savings/transfer/savings-to-bank
Authorization: Bearer {token}
Content-Type: application/json

{
  "savingsAccountId": "savings-account-id",
  "bankAccountId": "bank-account-id",
  "amount": 500.00,
  "description": "Emergency withdrawal"
}
```

## Data Transfer Objects (DTOs)

### CreateSavingsAccountDto
```csharp
public class CreateSavingsAccountDto
{
    [Required]
    [StringLength(100)]
    public string AccountName { get; set; }
    
    [Required]
    [StringLength(50)]
    public string SavingsType { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TargetAmount { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? Goal { get; set; }
    
    [Required]
    public DateTime TargetDate { get; set; }
    
    [StringLength(10)]
    public string Currency { get; set; } = "USD";
}
```

### SavingsAccountDto
```csharp
public class SavingsAccountDto
{
    public string Id { get; set; }
    public string AccountName { get; set; }
    public string SavingsType { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; }
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public DateTime TargetDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal ProgressPercentage { get; set; }
    public decimal RemainingAmount { get; set; }
    public int DaysRemaining { get; set; }
    public decimal MonthlyTarget { get; set; }
}
```

### CreateSavingsTransactionDto
```csharp
public class CreateSavingsTransactionDto
{
    [Required]
    [StringLength(450)]
    public string SavingsAccountId { get; set; }
    
    [Required]
    [StringLength(450)]
    public string SourceBankAccountId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Description { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(10)]
    public string Currency { get; set; } = "USD";
    
    public bool IsRecurring { get; set; } = false;
    
    [StringLength(50)]
    public string? RecurringFrequency { get; set; }
}
```

## Enums

### SavingsType
```csharp
public enum SavingsType
{
    EMERGENCY,           // Emergency fund
    VACATION,            // Vacation savings
    INVESTMENT,          // Investment savings
    RETIREMENT,          // Retirement fund
    EDUCATION,           // Education fund
    HOME_DOWN_PAYMENT,   // Home down payment
    CAR_PURCHASE,        // Car purchase fund
    WEDDING,             // Wedding fund
    TRAVEL,              // Travel fund
    BUSINESS,            // Business investment
    HEALTH,              // Health expenses
    TAX_SAVINGS,         // Tax savings
    GENERAL              // General savings
}
```

### SavingsTransactionType
```csharp
public enum SavingsTransactionType
{
    DEPOSIT,     // Money going into savings
    WITHDRAWAL,  // Money coming out of savings
    TRANSFER,    // Transfer between accounts
    INTEREST,    // Interest earned
    BONUS        // Bonus or windfall
}
```

### SavingsCategory
```csharp
public enum SavingsCategory
{
    MONTHLY_SAVINGS,      // Regular monthly savings
    BONUS,                // Bonus or windfall
    TAX_REFUND,           // Tax refund
    GIFT,                 // Gift money
    SIDE_INCOME,          // Side income
    INVESTMENT_RETURN,    // Investment returns
    EMERGENCY_WITHDRAWAL, // Emergency withdrawal
    PLANNED_EXPENSE,      // Planned expense
    TRANSFER,             // Account transfer
    OTHER                 // Other category
}
```

## Business Logic

### 1. **Account Creation**
- Each user can create multiple savings accounts
- Account names must be unique per user
- Target amounts must be greater than 0
- Target dates must be in the future

### 2. **Transaction Processing**
- **Deposits**: Money moves from bank account to savings account
- **Withdrawals**: Money moves from savings account to bank account
- **Balance Updates**: Both accounts are updated atomically
- **Validation**: Sufficient balance checks before transactions

### 3. **Progress Calculation**
- **Progress Percentage**: (Current Balance / Target Amount) × 100
- **Remaining Amount**: Target Amount - Current Balance
- **Days Remaining**: Target Date - Current Date
- **Monthly Target**: Remaining Amount / (Days Remaining / 30)

### 4. **Goal Management**
- Goals can be updated (target amount, target date)
- Progress is recalculated automatically
- Completed goals are identified when Current Balance ≥ Target Amount

## Integration with Bank Accounts

### How Savings Affects Bank Accounts

1. **When Creating a Savings Account**:
   - No immediate impact on bank accounts
   - Savings account starts with $0 balance

2. **When Making a Deposit to Savings**:
   - Bank account balance decreases by deposit amount
   - Savings account balance increases by deposit amount
   - Transaction is recorded in both systems

3. **When Making a Withdrawal from Savings**:
   - Savings account balance decreases by withdrawal amount
   - Bank account balance increases by withdrawal amount
   - Transaction is recorded in both systems

### Example Flow

```javascript
// 1. User has $5000 in checking account
const checkingBalance = 5000.00;

// 2. User creates emergency fund savings account
const emergencyFund = await createSavingsAccount({
  accountName: "Emergency Fund",
  savingsType: "EMERGENCY",
  targetAmount: 10000.00,
  targetDate: "2024-12-31"
});

// 3. User transfers $1000 from checking to emergency fund
await transferToSavings({
  bankAccountId: "checking-account-id",
  savingsAccountId: emergencyFund.id,
  amount: 1000.00,
  description: "Initial emergency fund deposit"
});

// 4. Result:
// - Checking account balance: $4000.00
// - Emergency fund balance: $1000.00
// - Progress: 10% (1000/10000)
```

## Usage Examples

### 1. **Setting Up Multiple Savings Goals**

```javascript
// Create emergency fund
const emergencyFund = await fetch('/api/Savings/accounts', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    accountName: "Emergency Fund",
    savingsType: "EMERGENCY",
    targetAmount: 10000.00,
    description: "6 months emergency savings",
    goal: "Build emergency fund for unexpected expenses",
    targetDate: "2024-12-31T00:00:00Z"
  })
});

// Create vacation fund
const vacationFund = await fetch('/api/Savings/accounts', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    accountName: "Summer Vacation",
    savingsType: "VACATION",
    targetAmount: 3000.00,
    description: "Summer vacation to Europe",
    goal: "Save for 2-week European vacation",
    targetDate: "2024-06-30T00:00:00Z"
  })
});

// Create investment fund
const investmentFund = await fetch('/api/Savings/accounts', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    accountName: "Investment Fund",
    savingsType: "INVESTMENT",
    targetAmount: 5000.00,
    description: "Long-term investment savings",
    goal: "Build investment portfolio",
    targetDate: "2025-12-31T00:00:00Z"
  })
});
```

### 2. **Monthly Savings Routine**

```javascript
// Monthly savings allocation
const monthlySavings = async (salaryAmount) => {
  const emergencyAmount = salaryAmount * 0.10; // 10% to emergency
  const vacationAmount = salaryAmount * 0.05;  // 5% to vacation
  const investmentAmount = salaryAmount * 0.15; // 15% to investment

  // Transfer to emergency fund
  await fetch('/api/Savings/transfer/bank-to-savings', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      bankAccountId: "main-checking-id",
      savingsAccountId: emergencyFundId,
      amount: emergencyAmount,
      description: "Monthly emergency fund contribution"
    })
  });

  // Transfer to vacation fund
  await fetch('/api/Savings/transfer/bank-to-savings', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      bankAccountId: "main-checking-id",
      savingsAccountId: vacationFundId,
      amount: vacationAmount,
      description: "Monthly vacation fund contribution"
    })
  });

  // Transfer to investment fund
  await fetch('/api/Savings/transfer/bank-to-savings', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      bankAccountId: "main-checking-id",
      savingsAccountId: investmentFundId,
      amount: investmentAmount,
      description: "Monthly investment contribution"
    })
  });
};
```

### 3. **Tracking Progress**

```javascript
// Get savings summary
const getSavingsProgress = async () => {
  const response = await fetch('/api/Savings/summary', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const data = await response.json();
  
  console.log(`Total Savings: $${data.data.totalSavingsBalance}`);
  console.log(`Overall Progress: ${data.data.overallProgressPercentage.toFixed(1)}%`);
  console.log(`Active Goals: ${data.data.activeGoals}`);
  console.log(`Completed Goals: ${data.data.completedGoals}`);
  console.log(`This Month Saved: $${data.data.thisMonthSaved}`);
};

// Get individual account progress
const getAccountProgress = async (accountId) => {
  const response = await fetch(`/api/Savings/accounts/${accountId}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const data = await response.json();
  
  const account = data.data;
  console.log(`${account.accountName}:`);
  console.log(`  Progress: ${account.progressPercentage.toFixed(1)}%`);
  console.log(`  Remaining: $${account.remainingAmount}`);
  console.log(`  Days Left: ${account.daysRemaining}`);
  console.log(`  Monthly Target: $${account.monthlyTarget.toFixed(2)}`);
};
```

### 4. **Emergency Withdrawal**

```javascript
// Emergency withdrawal from savings
const emergencyWithdrawal = async (amount, reason) => {
  const response = await fetch('/api/Savings/transfer/savings-to-bank', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      savingsAccountId: emergencyFundId,
      bankAccountId: "main-checking-id",
      amount: amount,
      description: `Emergency withdrawal: ${reason}`
    })
  });
  
  if (response.ok) {
    console.log(`Emergency withdrawal of $${amount} completed`);
  }
};
```

## Benefits of the Savings System

### 1. **Goal-Oriented Saving**
- Clear targets and deadlines
- Visual progress tracking
- Motivation through progress indicators

### 2. **Financial Organization**
- Separate accounts for different purposes
- Categorized savings types
- Clear transaction history

### 3. **Automatic Calculations**
- Progress percentages
- Monthly savings targets
- Days remaining calculations

### 4. **Integration Benefits**
- Seamless bank account integration
- Unified transaction history
- Consistent balance management

### 5. **Analytics & Insights**
- Savings trends over time
- Category-based analysis
- Monthly savings patterns

## Security & Validation

### 1. **User Authorization**
- All endpoints require authentication
- Users can only access their own savings accounts
- JWT token validation on every request

### 2. **Data Validation**
- Required field validation
- Amount range validation (must be > 0)
- Date validation (target dates must be future)
- String length limitations

### 3. **Business Logic Validation**
- Sufficient balance checks
- Account ownership verification
- Transaction type validation
- Currency consistency

### 4. **Error Handling**
- Comprehensive error messages
- Graceful failure handling
- Transaction rollback on errors

## Future Enhancements

### 1. **Auto-Save Features**
- Automatic monthly transfers
- Percentage-based savings
- Round-up savings from transactions

### 2. **Savings Challenges**
- 52-week savings challenge
- Monthly savings goals
- Achievement badges

### 3. **Investment Integration**
- Link to investment accounts
- Automatic investment transfers
- Portfolio tracking

### 4. **Notifications**
- Goal completion alerts
- Monthly savings reminders
- Progress milestone notifications

### 5. **Advanced Analytics**
- Savings rate calculations
- Goal achievement predictions
- Spending vs. saving analysis

This comprehensive savings system provides users with powerful tools to manage their financial goals while maintaining seamless integration with their existing bank accounts and transaction history.
