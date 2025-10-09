# User Profile System Documentation

## Overview

The User Profile System allows users to manage their income information, financial goals, and employment details. This comprehensive system tracks various income sources including salary, passive income, investments, and side hustles, while also managing financial goals and tax information.

## Features

### 💰 **Income Management - Generalized System**

The income management system is now **completely flexible** and **user-defined**. Instead of hardcoded income types, users can create any income source they need with custom names, amounts, frequencies, and categories.

#### 🎯 **Key Features**

**✅ Completely Flexible**: Users can add any income source they want
- No more hardcoded fields like `salaryAmount` vs `sideHustleAmount`
- Users define their own income sources dynamically
- Supports any combination of income patterns

**✅ Individual Frequency Fields**: Each income source has its own frequency
- **WEEKLY**: Weekly income (e.g., side hustle, freelance work)
- **BI_WEEKLY**: Bi-weekly income (e.g., some salary structures)  
- **MONTHLY**: Monthly income (e.g., regular salary, rent)
- **QUARTERLY**: Quarterly income (e.g., bonuses, dividends)
- **ANNUALLY**: Annual income (e.g., yearly bonuses, tax refunds)

**✅ User-Defined Categories**: Flexible categorization system
- **PRIMARY**: Main job/salary
- **PASSIVE**: Investment returns, dividends, interest
- **BUSINESS**: Business profits and revenue
- **SIDE_HUSTLE**: Freelance and part-time work
- **INVESTMENT**: Investment returns
- **RENTAL**: Property rental earnings
- **DIVIDEND**: Dividend payments
- **INTEREST**: Interest earnings
- **OTHER**: Miscellaneous income sources

#### 📊 **Real-World Examples**

**Scenario 1: Traditional Employee**
```json
{
  "incomeSources": [
    {
      "name": "Software Engineer Salary",
      "amount": 5000,
      "frequency": "MONTHLY",
      "category": "PRIMARY",
      "company": "Tech Corp"
    }
  ]
}
```

**Scenario 2: Freelancer with Multiple Streams**
```json
{
  "incomeSources": [
    {
      "name": "Client A Project",
      "amount": 2000,
      "frequency": "MONTHLY",
      "category": "BUSINESS"
    },
    {
      "name": "Client B Project", 
      "amount": 800,
      "frequency": "WEEKLY",
      "category": "BUSINESS"
    },
    {
      "name": "Online Course Sales",
      "amount": 500,
      "frequency": "MONTHLY",
      "category": "PASSIVE"
    }
  ]
}
```

**Scenario 3: Mixed Income**
```json
{
  "incomeSources": [
    {
      "name": "Full-time Job",
      "amount": 4000,
      "frequency": "MONTHLY",
      "category": "PRIMARY"
    },
    {
      "name": "Uber Driving",
      "amount": 300,
      "frequency": "WEEKLY",
      "category": "SIDE_HUSTLE"
    },
    {
      "name": "Rental Property",
      "amount": 1200,
      "frequency": "MONTHLY",
      "category": "RENTAL"
    }
  ]
}
```

### 🎯 **Financial Goals**
- **Savings Goals**: Monthly savings targets
- **Investment Goals**: Monthly investment targets
- **Emergency Fund Goals**: Emergency fund contributions

### 💼 **Employment Information**
- **Job Details**: Title, company, employment type
- **Industry**: Professional industry classification
- **Location**: Work location information

### 📊 **Tax Management**
- **Tax Rate**: Personal tax rate percentage
- **Tax Deductions**: Monthly tax deductions

### 📈 **Analytics & Reporting**
- **Income Summary**: Total and net income calculations
- **Income Breakdown**: Detailed income source analysis
- **Goals Analysis**: Financial goal tracking
- **Savings Rate**: Percentage of income saved
- **Disposable Income**: Available income after goals

## Database Schema

### UserProfile Entity

```csharp
public class UserProfile
{
    public string Id { get; set; }
    public string UserId { get; set; }
    
    // Employment Information (kept for general info)
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? EmploymentType { get; set; }
    
    // Income Sources (flexible, user-defined)
    public virtual ICollection<IncomeSource> IncomeSources { get; set; }
    
    // Financial Goals
    public decimal? MonthlySavingsGoal { get; set; }
    public decimal? MonthlyInvestmentGoal { get; set; }
    public decimal? MonthlyEmergencyFundGoal { get; set; }
    
    // Tax Information
    public decimal? TaxRate { get; set; }
    public decimal? MonthlyTaxDeductions { get; set; }
    
    // Additional Information
    public string? Notes { get; set; }
    public string? Industry { get; set; }
    public string? Location { get; set; }
    
    // Computed Properties (calculated from IncomeSources)
    public decimal TotalMonthlyIncome { get; }
    public decimal NetMonthlyIncome { get; }
    public decimal TotalMonthlyGoals { get; }
}

### IncomeSource Entity

```csharp
public class IncomeSource
{
    public string Id { get; set; }
    public string UserId { get; set; }
    
    // Income Source Details
    public string Name { get; set; } // "Salary", "Side Hustle", "Freelance", etc.
    public decimal Amount { get; set; }
    public string Frequency { get; set; } // WEEKLY, MONTHLY, etc.
    public string Category { get; set; } // PRIMARY, PASSIVE, BUSINESS, etc.
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    
    // Additional Information
    public string? Description { get; set; }
    public string? Company { get; set; } // For salary or business income
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; }
    
    // Computed Properties
    public decimal MonthlyAmount { get; set; } // Auto-converted to monthly
}
```

## API Endpoints

### Income Source Management

#### Create Income Source
```http
POST /api/incomesource
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Software Engineer Salary",
  "amount": 5000,
  "frequency": "MONTHLY",
  "category": "PRIMARY",
  "currency": "USD",
  "description": "Full-time software engineering position",
  "company": "Tech Corp"
}
```

#### Get All Income Sources
```http
GET /api/incomesource?activeOnly=true
Authorization: Bearer {token}
```

#### Get Income Source by ID
```http
GET /api/incomesource/{incomeSourceId}
Authorization: Bearer {token}
```

#### Update Income Source
```http
PUT /api/incomesource/{incomeSourceId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "amount": 5500,
  "frequency": "MONTHLY"
}
```

#### Delete Income Source
```http
DELETE /api/incomesource/{incomeSourceId}
Authorization: Bearer {token}
```

#### Toggle Income Source Status
```http
PATCH /api/incomesource/{incomeSourceId}/toggle-status
Authorization: Bearer {token}
```

#### Create Multiple Income Sources
```http
POST /api/incomesource/bulk
Authorization: Bearer {token}
Content-Type: application/json

{
  "incomeSources": [
    {
      "name": "Full-time Job",
      "amount": 4000,
      "frequency": "MONTHLY",
      "category": "PRIMARY"
    },
    {
      "name": "Uber Driving",
      "amount": 300,
      "frequency": "WEEKLY",
      "category": "SIDE_HUSTLE"
    }
  ]
}
```

#### Get Income Summary
```http
GET /api/incomesource/summary
Authorization: Bearer {token}
```

#### Get Income Analytics
```http
GET /api/incomesource/analytics?period=month
Authorization: Bearer {token}
```

#### Get Income by Category
```http
GET /api/incomesource/by-category
Authorization: Bearer {token}
```

#### Get Income by Frequency
```http
GET /api/incomesource/by-frequency
Authorization: Bearer {token}
```

#### Get Total Monthly Income
```http
GET /api/incomesource/total-monthly
Authorization: Bearer {token}
```

#### Get Net Monthly Income
```http
GET /api/incomesource/net-monthly
Authorization: Bearer {token}
```

#### Get Available Categories
```http
GET /api/incomesource/categories
Authorization: Bearer {token}
```

#### Get Available Frequencies
```http
GET /api/incomesource/frequencies
Authorization: Bearer {token}
```

#### Get Income Sources by Category
```http
GET /api/incomesource/category/{category}
Authorization: Bearer {token}
```

#### Get Income Sources by Frequency
```http
GET /api/incomesource/frequency/{frequency}
Authorization: Bearer {token}
```

### Profile Management

#### Create User Profile
```http
POST /api/userprofile
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "monthlySalary": 5000.00,
  "salaryCurrency": "USD",
  "jobTitle": "Software Engineer",
  "company": "Tech Corp",
  "employmentType": "FULL_TIME",
  "monthlyPassiveIncome": 500.00,
  "monthlyInvestmentIncome": 200.00,
  "monthlySavingsGoal": 1000.00,
  "monthlyInvestmentGoal": 500.00,
  "taxRate": 25.0,
  "industry": "Technology",
  "location": "San Francisco, CA"
}
```

#### Get User Profile
```http
GET /api/userprofile
Authorization: Bearer {jwt_token}
```

#### Update User Profile
```http
PUT /api/userprofile
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "monthlySalary": 5500.00,
  "monthlySavingsGoal": 1200.00
}
```

#### Delete User Profile
```http
DELETE /api/userprofile
Authorization: Bearer {jwt_token}
```

### Income Management

#### Update Salary
```http
PUT /api/userprofile/salary
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "monthlySalary": 6000.00,
  "salaryCurrency": "USD"
}
```

#### Update Passive Income
```http
PUT /api/userprofile/passive-income
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 750.00
}
```

#### Update Investment Income
```http
PUT /api/userprofile/investment-income
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 300.00
}
```

#### Update Rental Income
```http
PUT /api/userprofile/rental-income
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 1200.00
}
```

#### Update Business Income
```http
PUT /api/userprofile/business-income
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 2000.00
}
```

#### Update Side Hustle Income
```http
PUT /api/userprofile/side-hustle-income
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 800.00
}
```

### Financial Goals

#### Update Savings Goal
```http
PUT /api/userprofile/savings-goal
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 1500.00
}
```

#### Update Investment Goal
```http
PUT /api/userprofile/investment-goal
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 800.00
}
```

#### Update Emergency Fund Goal
```http
PUT /api/userprofile/emergency-fund-goal
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "amount": 500.00
}
```

### Tax Management

#### Update Tax Information
```http
PUT /api/userprofile/tax-information
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "taxRate": 28.0,
  "monthlyTaxDeductions": 1200.00
}
```

### Employment Information

#### Update Employment Info
```http
PUT /api/userprofile/employment
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "jobTitle": "Senior Software Engineer",
  "company": "Tech Corp Inc",
  "employmentType": "FULL_TIME",
  "industry": "Technology",
  "location": "San Francisco, CA"
}
```

### Analytics & Reporting

#### Get Income Summary
```http
GET /api/userprofile/income-summary
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalMonthlyIncome": 8500.00,
    "netMonthlyIncome": 7300.00,
    "totalMonthlyGoals": 2800.00,
    "disposableIncome": 4500.00,
    "savingsRate": 32.94,
    "incomeBreakdown": {
      "Salary": 5000.00,
      "Passive Income": 500.00,
      "Investment Income": 200.00,
      "Rental Income": 1200.00,
      "Business Income": 2000.00,
      "Side Hustle Income": 800.00,
      "Other Income": 0.00
    },
    "goalsBreakdown": {
      "Savings Goal": 1500.00,
      "Investment Goal": 800.00,
      "Emergency Fund Goal": 500.00
    }
  }
}
```

#### Get Income Analytics
```http
GET /api/userprofile/income-analytics?period=year
Authorization: Bearer {jwt_token}
```

#### Get Income Breakdown
```http
GET /api/userprofile/income-breakdown
Authorization: Bearer {jwt_token}
```

#### Get Goals Breakdown
```http
GET /api/userprofile/goals-breakdown
Authorization: Bearer {jwt_token}
```

#### Validate Income Goals
```http
GET /api/userprofile/validate-goals
Authorization: Bearer {jwt_token}
```

#### Calculate Savings Rate
```http
GET /api/userprofile/savings-rate
Authorization: Bearer {jwt_token}
```

#### Calculate Disposable Income
```http
GET /api/userprofile/disposable-income
Authorization: Bearer {jwt_token}
```

### Admin Endpoints

#### Get All User Profiles (Admin Only)
```http
GET /api/userprofile/admin/all?page=1&limit=50
Authorization: Bearer {admin_jwt_token}
```

#### Get Profiles by Income Range (Admin Only)
```http
GET /api/userprofile/admin/by-income-range?minIncome=3000&maxIncome=10000&page=1&limit=50
Authorization: Bearer {admin_jwt_token}
```

#### Get Profiles by Employment Type (Admin Only)
```http
GET /api/userprofile/admin/by-employment-type?employmentType=FULL_TIME&page=1&limit=50
Authorization: Bearer {admin_jwt_token}
```

## Data Transfer Objects (DTOs)

### CreateUserProfileDto
```csharp
public class CreateUserProfileDto
{
    public decimal? MonthlySalary { get; set; }
    public string? SalaryCurrency { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? MonthlyPassiveIncome { get; set; }
    public decimal? MonthlyInvestmentIncome { get; set; }
    public decimal? MonthlyRentalIncome { get; set; }
    public decimal? MonthlyDividendIncome { get; set; }
    public decimal? MonthlyInterestIncome { get; set; }
    public decimal? MonthlyBusinessIncome { get; set; }
    public decimal? MonthlySideHustleIncome { get; set; }
    public decimal? MonthlyOtherIncome { get; set; }
    public decimal? MonthlySavingsGoal { get; set; }
    public decimal? MonthlyInvestmentGoal { get; set; }
    public decimal? MonthlyEmergencyFundGoal { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? MonthlyTaxDeductions { get; set; }
    public string? Notes { get; set; }
    public string? Industry { get; set; }
    public string? Location { get; set; }
}
```

### IncomeSummaryDto
```csharp
public class IncomeSummaryDto
{
    public decimal TotalMonthlyIncome { get; set; }
    public decimal NetMonthlyIncome { get; set; }
    public decimal TotalMonthlyGoals { get; set; }
    public decimal DisposableIncome { get; set; }
    public decimal SavingsRate { get; set; }
    public Dictionary<string, decimal> IncomeBreakdown { get; set; }
    public Dictionary<string, decimal> GoalsBreakdown { get; set; }
}
```

### IncomeAnalyticsDto
```csharp
public class IncomeAnalyticsDto
{
    public decimal AverageMonthlyIncome { get; set; }
    public decimal HighestIncomeMonth { get; set; }
    public decimal LowestIncomeMonth { get; set; }
    public decimal IncomeGrowthRate { get; set; }
    public Dictionary<string, decimal> IncomeTrends { get; set; }
    public List<string> TopIncomeSources { get; set; }
    public decimal IncomeStability { get; set; }
}
```

## Business Rules

### Income Validation
- All income amounts must be positive numbers
- Currency must be a valid 3-letter code (default: USD)
- Tax rate must be between 0 and 100 percent

### Goal Validation
- Financial goals cannot exceed net income
- Goals are validated when requested via `/validate-goals` endpoint
- System provides warnings for unrealistic goals

### Profile Management
- Each user can have only one active profile
- Profile creation is optional but recommended
- Profile updates are incremental (only provided fields are updated)

### Computed Properties
- **Total Monthly Income**: Sum of all income sources
- **Net Monthly Income**: Total income minus tax deductions
- **Total Monthly Goals**: Sum of all financial goals
- **Disposable Income**: Net income minus total goals
- **Savings Rate**: Percentage of total income allocated to savings and investments

## Employment Types

Supported employment types:
- `FULL_TIME`: Full-time employment
- `PART_TIME`: Part-time employment
- `CONTRACT`: Contract work
- `FREELANCE`: Freelance work
- `SELF_EMPLOYED`: Self-employment

## Income Frequencies

Supported income frequencies:
- `WEEKLY`: Weekly income
- `BI_WEEKLY`: Bi-weekly income
- `MONTHLY`: Monthly income (default)
- `QUARTERLY`: Quarterly income
- `ANNUALLY`: Annual income

## Error Handling

### Common Error Responses

#### Profile Not Found
```json
{
  "success": false,
  "message": "User profile not found",
  "data": null
}
```

#### Validation Error
```json
{
  "success": false,
  "message": "Monthly salary must be a positive number",
  "data": null
}
```

#### Unauthorized Access
```json
{
  "success": false,
  "message": "User not authenticated",
  "data": null
}
```

#### Profile Already Exists
```json
{
  "success": false,
  "message": "User profile already exists. Use update instead.",
  "data": null
}
```

## Usage Examples

### Complete Profile Setup
```javascript
// 1. Create initial profile
const createProfile = await fetch('/api/userprofile', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    salaryAmount: 5000,
    salaryFrequency: 'MONTHLY',
    salaryCurrency: 'USD',
    jobTitle: 'Software Engineer',
    company: 'Tech Corp',
    employmentType: 'FULL_TIME',
    sideHustleAmount: 800,
    sideHustleFrequency: 'WEEKLY',
    businessIncomeAmount: 2000,
    businessIncomeFrequency: 'MONTHLY',
    rentalIncomeAmount: 1200,
    rentalIncomeFrequency: 'MONTHLY',
    investmentIncomeAmount: 200,
    investmentIncomeFrequency: 'MONTHLY',
    monthlySavingsGoal: 1000,
    monthlyInvestmentGoal: 500,
    taxRate: 25,
    industry: 'Technology',
    location: 'San Francisco, CA'
  })
});

// 2. Get income summary
const summary = await fetch('/api/userprofile/income-summary', {
  headers: { 'Authorization': 'Bearer ' + token }
});

// 3. Validate goals
const validation = await fetch('/api/userprofile/validate-goals', {
  headers: { 'Authorization': 'Bearer ' + token }
});
```

### Update Specific Income Sources
```javascript
// Update salary
await fetch('/api/userprofile/salary', {
  method: 'PUT',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    monthlySalary: 5500,
    salaryCurrency: 'USD'
  })
});

// Update side hustle income
await fetch('/api/userprofile/side-hustle-income', {
  method: 'PUT',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    amount: 800
  })
});
```

### Financial Planning
```javascript
// Get comprehensive financial overview
const [summary, analytics, breakdown] = await Promise.all([
  fetch('/api/userprofile/income-summary', { headers: { 'Authorization': 'Bearer ' + token } }),
  fetch('/api/userprofile/income-analytics', { headers: { 'Authorization': 'Bearer ' + token } }),
  fetch('/api/userprofile/income-breakdown', { headers: { 'Authorization': 'Bearer ' + token } })
]);

const summaryData = await summary.json();
const analyticsData = await analytics.json();
const breakdownData = await breakdown.json();

console.log('Total Monthly Income:', summaryData.data.totalMonthlyIncome);
console.log('Savings Rate:', summaryData.data.savingsRate + '%');
console.log('Disposable Income:', summaryData.data.disposableIncome);
```

## Integration with Other Systems

### Bank Account Integration
- User profiles can be used to set up automatic savings transfers
- Income goals can trigger notifications when bank account balances reach targets
- Expense tracking can be compared against income goals

### Loan Management Integration
- Income information can be used for loan eligibility calculations
- Debt-to-income ratios can be calculated using profile data
- Loan payments can be factored into financial goal planning

### Savings System Integration
- Profile savings goals can automatically create savings accounts
- Investment goals can trigger investment account setups
- Emergency fund goals can be tracked in the savings system

## Security Considerations

### Data Protection
- All profile data is encrypted in transit and at rest
- User profiles are isolated by user ID
- Sensitive financial information is protected with proper access controls

### Access Control
- Users can only access and modify their own profiles
- Admin endpoints require ADMIN role
- All endpoints require valid JWT authentication

### Audit Trail
- All profile changes are logged with timestamps
- User activity is tracked for security monitoring
- Profile access is logged for compliance

## Performance Considerations

### Database Optimization
- User profiles are indexed by UserId for fast lookups
- Computed properties are calculated on-demand
- Profile data is cached for frequently accessed information

### API Performance
- Endpoints are optimized for minimal database queries
- Response data is paginated for admin endpoints
- Computed calculations are performed efficiently

## Future Enhancements

### Planned Features
- **Historical Income Tracking**: Track income changes over time
- **Goal Progress Tracking**: Monitor progress toward financial goals
- **Income Forecasting**: Predict future income based on trends
- **Tax Optimization**: Suggest tax-saving strategies
- **Investment Recommendations**: AI-powered investment suggestions
- **Expense Budgeting**: Create budgets based on income and goals

### Integration Opportunities
- **Payroll Integration**: Automatic salary updates from payroll systems
- **Bank Integration**: Real-time income verification from bank accounts
- **Tax Software Integration**: Export data to tax preparation software
- **Investment Platform Integration**: Sync with investment accounts

## Conclusion

The User Profile System provides a comprehensive foundation for personal financial management within UtilityHub360. It enables users to track multiple income sources, set and monitor financial goals, and gain insights into their financial health through detailed analytics and reporting.

The system is designed to be flexible, secure, and scalable, supporting various employment types and income sources while providing valuable insights for financial planning and decision-making.
