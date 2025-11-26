# Financial Reports & Analytics System - Complete Documentation

## 📚 Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Report Types](#report-types)
4. [API Endpoints](#api-endpoints)
5. [Request & Response Examples](#request--response-examples)
6. [Data Structures](#data-structures)
7. [Analytics & Calculations](#analytics--calculations)
8. [Insights & Predictions](#insights--predictions)
9. [Frontend Integration](#frontend-integration)
10. [Export Functionality](#export-functionality)
11. [Security & Authorization](#security--authorization)
12. [Performance Considerations](#performance-considerations)
13. [Testing Guide](#testing-guide)

---

## Overview

### Purpose
The Financial Reports & Analytics System provides comprehensive, data-driven insights into user finances, enabling them to:
- 📊 Understand spending patterns
- 📈 Track income growth
- 🎯 Monitor savings goals
- 💰 Manage debt effectively
- 🔮 Predict future finances
- 💡 Receive actionable insights

### Key Features
- **7 Report Categories**: Income, Expenses, Disposable Income, Bills, Loans, Savings, Net Worth
- **Real-time Analytics**: Trend analysis, growth rates, comparisons
- **AI-Generated Insights**: Alerts, tips, and forecasts
- **Predictive Analytics**: Next month forecasts using weighted averages
- **Visual Data**: Chart-ready data for graphs
- **Export Options**: PDF and CSV formats
- **100% Non-Invasive**: No database changes required

### Technology Stack
- **Backend**: ASP.NET Core 8.0, Entity Framework Core
- **Database**: Reads from existing SQL Server tables
- **Authentication**: JWT Bearer tokens
- **Authorization**: Role-based (User, Admin)

---

## System Architecture

### Component Structure
```
┌─────────────────────────────────────────────────┐
│           Frontend Application                   │
│  (React/Angular/Vue with Chart.js/Recharts)     │
└──────────────────┬──────────────────────────────┘
                   │ HTTP/REST API
                   ↓
┌─────────────────────────────────────────────────┐
│        FinancialReportsController                │
│  - Full Report Generation                        │
│  - Individual Report Endpoints                   │
│  - Insights & Predictions                        │
│  - Export Functionality                          │
└──────────────────┬──────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────┐
│       FinancialReportService                     │
│  - Report Generation Logic                       │
│  - Analytics Calculations                        │
│  - Trend Analysis                                │
│  - Prediction Algorithms                         │
│  - Insight Generation                            │
└──────────────────┬──────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────┐
│         ApplicationDbContext                     │
│  READ ONLY - Queries existing tables:            │
│  - IncomeSources                                 │
│  - Bills, Payments                               │
│  - BankAccounts, BankTransactions                │
│  - Loans, VariableExpenses                       │
│  - SavingsAccounts, UserProfiles                 │
└─────────────────────────────────────────────────┘
```

### Data Flow
```
User Request → API Controller → Service Layer → Database Query
                                       ↓
                            In-Memory Calculations
                                       ↓
                              Analytics Engine
                                       ↓
                            Insights Generation
                                       ↓
                              JSON Response → Frontend Charts
```

---

## Report Types

### 1. 💰 Income Report

**Purpose**: Analyze all income streams and track growth trends.

**Metrics Provided**:
- Total income (monthly/quarterly/yearly)
- Income by source (salary, freelance, rental, etc.)
- Income by category (PRIMARY, PASSIVE, BUSINESS, etc.)
- Month-over-month growth rate
- Top income source
- Income trend chart (6-12 months)

**Use Cases**:
- Track salary increases
- Monitor side hustle income
- Identify primary income sources
- Plan for income stability

**Sample Insight**:
> "Your income increased by 12% since last month. Your freelance work contributed $5,000 (10%) to total income."

---

### 2. 💸 Expense Report

**Purpose**: Understand spending habits and identify cost-saving opportunities.

**Metrics Provided**:
- Total expenses (fixed + variable)
- Expense breakdown by category
- Expense percentage distribution
- Highest expense category
- Month-over-month comparison
- Average monthly expense
- Expense trend chart

**Categories Tracked**:
- Bills & Utilities
- Groceries
- Transportation
- Entertainment
- Healthcare
- Education
- Shopping
- Dining Out
- Others

**Use Cases**:
- Identify overspending categories
- Compare expenses month-to-month
- Find savings opportunities
- Budget planning

**Sample Insight**:
> "Groceries made up 25% of total expenses this month. You're spending $2,000 more than your 3-month average. Consider meal planning to reduce costs."

---

### 3. 🪙 Disposable Income Report

**Purpose**: Show available money after all expenses.

**Metrics Provided**:
- Current disposable income
- 6-month disposable income trend
- Average disposable income
- Recommended savings allocation (30%)
- Comparison with previous periods

**Formula**:
```
Disposable Income = Total Income - Total Expenses
```

**Use Cases**:
- Plan discretionary spending
- Determine savings capacity
- Evaluate financial health
- Set realistic financial goals

**Sample Insight**:
> "You have $15,000 disposable income this month (+8% from last month). Allocate $4,500 (30%) to savings to reach your goal 2 months earlier."

---

### 4. 🏦 Bills & Utilities Report

**Purpose**: Track recurring bills and predict future costs.

**Metrics Provided**:
- Total monthly bills
- Average bill amount (last 6 months)
- Bills by type (electricity, water, internet, etc.)
- Bills by provider
- Monthly comparison (increases/decreases)
- Predicted next month total
- Unpaid bills count
- Overdue bills count
- Upcoming bills (next 30 days)

**Use Cases**:
- Monitor utility usage
- Detect unusual increases
- Plan for bill payments
- Budget for recurring costs

**Sample Insight**:
> "Your electricity bill increased by $300 (+15%) compared to last month. Next month's estimated total bills: $9,850. You have 2 unpaid bills due in 3 days."

---

### 5. 💳 Loan & Debt Report

**Purpose**: Track loan repayment progress and plan debt elimination.

**Metrics Provided**:
- Active loans count
- Total principal amount
- Total remaining balance
- Total monthly payment
- Total interest paid
- Individual loan details
- Repayment progress (%)
- Projected debt-free date

**Loan Details Include**:
- Loan purpose
- Principal amount
- Remaining balance
- Monthly payment
- Interest rate
- Repayment progress

**Use Cases**:
- Monitor debt reduction
- Plan early repayment
- Track interest costs
- Motivate debt elimination

**Sample Insight**:
> "You'll be debt-free by August 2026 if you maintain current payments. Increasing monthly payment by $2,000 would make you debt-free 8 months earlier."

---

### 6. 💎 Savings & Goal Progress Report

**Purpose**: Visualize savings growth and track financial goals.

**Metrics Provided**:
- Total savings balance
- Monthly savings amount
- Savings goal target
- Goal progress percentage
- Savings rate (% of income saved)
- Savings trend (6-12 months)
- Projected goal achievement date
- Months until goal

**Goals Tracked**:
- Emergency fund
- Investment fund
- Vacation fund
- House down payment
- Education fund
- Custom goals

**Use Cases**:
- Track savings progress
- Motivate consistent saving
- Adjust savings strategy
- Plan for large purchases

**Sample Insight**:
> "You've saved $25,000 (50% of your $50,000 goal). At current rate, you'll reach your goal in 5 months. Increase monthly savings by $1,000 to achieve it 2 months earlier."

---

### 7. 📊 Net Worth Report

**Purpose**: Provide overall financial health snapshot.

**Metrics Provided**:
- Current net worth
- Net worth change (amount & %)
- Total assets (savings + accounts)
- Total liabilities (loans)
- Asset breakdown
- Liability breakdown
- 12-month net worth trend
- Trend description

**Formula**:
```
Net Worth = (Total Savings + Bank Accounts) - (Total Loans)
```

**Use Cases**:
- Track wealth accumulation
- Monitor financial progress
- Set long-term goals
- Evaluate financial health

**Sample Insight**:
> "Your current net worth: $245,000 (+5% from last month). Your net worth has grown steadily for 6 months, increasing by $35,000 total."

---

## API Endpoints

### Base URL
```
https://api.utilityhub360.com
http://localhost:5000 (Development)
```

### Authentication
All endpoints require JWT Bearer token:
```
Authorization: Bearer {your_jwt_token}
```

---

### 📊 Main Report Endpoints

#### 1. Get Full Financial Report
```http
GET /api/Reports/full
```

**Query Parameters**:
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| period | string | No | MONTHLY | Report period: MONTHLY, QUARTERLY, YEARLY, CUSTOM |
| startDate | datetime | No | Start of current month | Custom start date |
| endDate | datetime | No | End of current month | Custom end date |
| includeComparison | boolean | No | true | Include period comparison |
| includeInsights | boolean | No | true | Include AI insights |
| includePredictions | boolean | No | true | Include predictions |
| includeTransactions | boolean | No | true | Include recent transactions |

**Example Request**:
```http
GET /api/Reports/full?period=MONTHLY&includeInsights=true
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Financial report generated successfully",
  "data": {
    "reportDate": "2025-10-28T20:00:00Z",
    "period": "MONTHLY",
    "summary": { /* Financial summary */ },
    "incomeReport": { /* Income details */ },
    "expenseReport": { /* Expense details */ },
    "disposableIncomeReport": { /* Disposable income */ },
    "billsReport": { /* Bills details */ },
    "loanReport": { /* Loan details */ },
    "savingsReport": { /* Savings details */ },
    "netWorthReport": { /* Net worth details */ },
    "insights": [ /* AI insights */ ],
    "predictions": [ /* Predictions */ ],
    "recentTransactions": [ /* Last 20 transactions */ ]
  }
}
```

---

#### 2. Get Financial Summary (Dashboard)
```http
GET /api/Reports/summary
```

**Query Parameters**:
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| date | datetime | No | Current date | Target date for summary |

**Example Request**:
```http
GET /api/Reports/summary?date=2025-10-28
Authorization: Bearer {token}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "totalIncome": 50000.00,
    "incomeChange": 10.5,
    "totalExpenses": 35000.00,
    "expenseChange": -5.2,
    "disposableIncome": 15000.00,
    "disposableChange": 8.3,
    "totalSavings": 25000.00,
    "savingsGoal": 50000.00,
    "savingsProgress": 50.0,
    "netWorth": 245000.00,
    "netWorthChange": 5.1
  }
}
```

---

#### 3. Get Income Report
```http
GET /api/Reports/income
```

**Query Parameters**: Same as full report

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "totalIncome": 50000.00,
    "monthlyAverage": 48500.00,
    "growthRate": 12.5,
    "incomeBySource": {
      "Company Salary": 45000.00,
      "Freelance Work": 5000.00
    },
    "incomeByCategory": {
      "PRIMARY": 45000.00,
      "SIDE_HUSTLE": 5000.00
    },
    "incomeTrend": [
      {
        "date": "2025-01-01T00:00:00Z",
        "label": "Jan 2025",
        "value": 45000.00
      },
      {
        "date": "2025-02-01T00:00:00Z",
        "label": "Feb 2025",
        "value": 47000.00
      }
    ],
    "topIncomeSource": "Company Salary",
    "topIncomeAmount": 45000.00
  }
}
```

---

#### 4. Get Expense Report
```http
GET /api/Reports/expenses
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "totalExpenses": 35000.00,
    "fixedExpenses": 25000.00,
    "variableExpenses": 10000.00,
    "expenseByCategory": {
      "Bills & Utilities": 8000.00,
      "GROCERIES": 6000.00,
      "TRANSPORTATION": 4000.00,
      "ENTERTAINMENT": 3000.00,
      "Others": 14000.00
    },
    "expensePercentage": {
      "Bills & Utilities": 22.86,
      "GROCERIES": 17.14,
      "TRANSPORTATION": 11.43,
      "ENTERTAINMENT": 8.57,
      "Others": 40.00
    },
    "expenseTrend": [ /* Trend data */ ],
    "highestExpenseCategory": "Bills & Utilities",
    "highestExpenseAmount": 8000.00,
    "highestExpensePercentage": 22.86,
    "averageMonthlyExpense": 34200.00,
    "categoryComparison": [
      {
        "category": "GROCERIES",
        "currentAmount": 6000.00,
        "previousAmount": 5500.00,
        "change": 500.00,
        "changePercentage": 9.09
      }
    ]
  }
}
```

---

#### 5. Get Disposable Income Report
```http
GET /api/Reports/disposable-income
```

#### 6. Get Bills Report
```http
GET /api/Reports/bills
```

#### 7. Get Loan Report
```http
GET /api/Reports/loans
```

#### 8. Get Savings Report
```http
GET /api/Reports/savings
```

#### 9. Get Net Worth Report
```http
GET /api/Reports/networth
```

---

### 💡 Insights & Predictions

#### 10. Get Financial Insights
```http
GET /api/Reports/insights
```

**Query Parameters**:
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| date | datetime | No | Current date | Target date |

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "type": "ALERT",
      "title": "Utility Bill Increase",
      "message": "Your electricity bill increased by $400 (+15%) this month.",
      "category": "BILLS",
      "amount": 400.00,
      "percentage": 15.0,
      "severity": "WARNING",
      "icon": "⚠️"
    },
    {
      "type": "TIP",
      "title": "Savings Opportunity",
      "message": "Reduce entertainment expenses by $1,000 to save $12,000/year.",
      "category": "ENTERTAINMENT",
      "amount": 1000.00,
      "severity": "INFO",
      "icon": "💡"
    },
    {
      "type": "FORECAST",
      "title": "Next Month Projection",
      "message": "Projected disposable income: $16,200",
      "category": "FORECAST",
      "amount": 16200.00,
      "severity": "INFO",
      "icon": "🔮"
    }
  ]
}
```

---

#### 11. Get Financial Predictions
```http
GET /api/Reports/predictions
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "type": "EXPENSE",
      "description": "Predicted total expenses for next month",
      "predictedAmount": 35800.00,
      "predictionDate": "2025-11-01T00:00:00Z",
      "confidence": 85.0
    },
    {
      "type": "INCOME",
      "description": "Predicted total income for next month",
      "predictedAmount": 51000.00,
      "predictionDate": "2025-11-01T00:00:00Z",
      "confidence": 90.0
    }
  ]
}
```

---

### 📄 Transaction Logs

#### 12. Get Recent Transactions
```http
GET /api/Reports/transactions/recent
```

**Query Parameters**:
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| limit | integer | No | 20 | Number of transactions |

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "date": "2025-10-28T10:30:00Z",
      "category": "Salary",
      "description": "Company Payroll",
      "type": "CREDIT",
      "amount": 45000.00,
      "balanceAfter": 45000.00
    },
    {
      "date": "2025-10-27T15:20:00Z",
      "category": "Utilities",
      "description": "Meralco Electricity Bill",
      "type": "DEBIT",
      "amount": 2500.00,
      "balanceAfter": 42500.00
    }
  ]
}
```

---

### 🔄 Comparisons

#### 13. Compare Periods
```http
GET /api/Reports/compare
```

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| period1Start | datetime | Yes | First period start |
| period1End | datetime | Yes | First period end |
| period2Start | datetime | Yes | Second period start |
| period2End | datetime | Yes | Second period end |

**Example Request**:
```http
GET /api/Reports/compare?period1Start=2025-01-01&period1End=2025-01-31&period2Start=2025-02-01&period2End=2025-02-28
Authorization: Bearer {token}
```

---

### 📥 Export Endpoints

#### 14. Export Report as PDF
```http
POST /api/Reports/export/pdf
```

**Request Body**:
```json
{
  "format": "PDF",
  "reportType": "FULL",
  "startDate": "2025-10-01",
  "endDate": "2025-10-31"
}
```

**Response**: Binary PDF file

---

#### 15. Export Report as CSV
```http
POST /api/Reports/export/csv
```

**Request Body**:
```json
{
  "format": "CSV",
  "reportType": "EXPENSE",
  "startDate": "2025-10-01",
  "endDate": "2025-10-31"
}
```

**Response**: CSV file

---

## Analytics & Calculations

### Growth Rate Calculation
```
Growth Rate = ((Current Value - Previous Value) / Previous Value) × 100
```

**Example**:
```
Current Income: $50,000
Previous Income: $45,000
Growth Rate = ((50,000 - 45,000) / 45,000) × 100 = 11.11%
```

### Disposable Income
```
Disposable Income = Total Income - Total Expenses
```

### Savings Rate
```
Savings Rate = (Monthly Savings / Total Income) × 100
```

### Net Worth
```
Net Worth = (Total Assets) - (Total Liabilities)
Assets = Savings + Bank Account Balances
Liabilities = Total Loan Balances
```

### Weighted Average (Predictions)
```
Prediction = (M1 × 0.1) + (M2 × 0.2) + (M3 × 0.3) + (M4 × 0.4)
Where M1-M4 are the last 4 months, with recent months weighted more
```

---

## Insights & Predictions

### Insight Types

#### 1. ALERT (⚠️)
**Triggers**:
- Expense increase > 15%
- Bill increase > 10%
- Savings goal off track
- Overdue bills detected
- Low disposable income (<10% of income)

**Example**:
```json
{
  "type": "ALERT",
  "title": "High Expense Increase",
  "message": "Your expenses increased by 18% this month",
  "severity": "WARNING",
  "icon": "⚠️"
}
```

#### 2. TIP (💡)
**Triggers**:
- Spending above category average
- Savings opportunity detected
- Bill reduction possible
- Goal optimization available

**Example**:
```json
{
  "type": "TIP",
  "title": "Reduce Entertainment Cost",
  "message": "You're spending 30% more than average on entertainment",
  "severity": "INFO",
  "icon": "💡"
}
```

#### 3. FORECAST (🔮)
**Triggers**:
- Next month projections
- Goal achievement dates
- Debt-free projections

**Example**:
```json
{
  "type": "FORECAST",
  "title": "Next Month Income",
  "message": "Projected income: $51,000 (90% confidence)",
  "severity": "INFO",
  "icon": "🔮"
}
```

---

## Frontend Integration

### Chart Libraries
**Recommended**:
- Chart.js - Simple and flexible
- Recharts - React-specific
- ApexCharts - Advanced features
- D3.js - Maximum customization

### Sample Implementation (React + Chart.js)

#### Line Chart - Income vs Expenses
```javascript
import { Line } from 'react-chartjs-2';

const IncomeTrendChart = ({ reportData }) => {
  const chartData = {
    labels: reportData.incomeTrend.map(t => t.label),
    datasets: [
      {
        label: 'Income',
        data: reportData.incomeTrend.map(t => t.value),
        borderColor: 'rgb(75, 192, 192)',
        backgroundColor: 'rgba(75, 192, 192, 0.2)',
        tension: 0.1
      },
      {
        label: 'Expenses',
        data: reportData.expenseTrend.map(t => t.value),
        borderColor: 'rgb(255, 99, 132)',
        backgroundColor: 'rgba(255, 99, 132, 0.2)',
        tension: 0.1
      }
    ]
  };

  const options = {
    responsive: true,
    plugins: {
      legend: { position: 'top' },
      title: { display: true, text: 'Income vs Expenses Trend' }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => `$${value.toLocaleString()}`
        }
      }
    }
  };

  return <Line data={chartData} options={options} />;
};
```

#### Pie Chart - Expense Distribution
```javascript
import { Pie } from 'react-chartjs-2';

const ExpenseDistributionChart = ({ expenseData }) => {
  const categories = Object.keys(expenseData.expenseByCategory);
  const amounts = Object.values(expenseData.expenseByCategory);

  const chartData = {
    labels: categories,
    datasets: [{
      data: amounts,
      backgroundColor: [
        '#FF6384', '#36A2EB', '#FFCE56', 
        '#4BC0C0', '#9966FF', '#FF9F40'
      ]
    }]
  };

  const options = {
    responsive: true,
    plugins: {
      legend: { position: 'right' },
      title: { display: true, text: 'Expense Distribution' },
      tooltip: {
        callbacks: {
          label: (context) => {
            const label = context.label || '';
            const value = context.parsed;
            const percentage = expenseData.expensePercentage[label];
            return `${label}: $${value.toLocaleString()} (${percentage.toFixed(1)}%)`;
          }
        }
      }
    }
  };

  return <Pie data={chartData} options={options} />;
};
```

#### Progress Bar - Savings Goal
```javascript
const SavingsProgress = ({ savingsData }) => {
  const progress = savingsData.goalProgress;
  
  return (
    <div className="savings-progress">
      <div className="progress-header">
        <span>Savings Goal Progress</span>
        <span>{progress.toFixed(1)}%</span>
      </div>
      <div className="progress-bar">
        <div 
          className="progress-fill" 
          style={{ width: `${progress}%` }}
        >
        </div>
      </div>
      <div className="progress-details">
        <span>${savingsData.totalSavings.toLocaleString()}</span>
        <span>${savingsData.savingsGoal.toLocaleString()}</span>
      </div>
    </div>
  );
};
```

---

## Security & Authorization

### Authentication Requirements
All endpoints require valid JWT token:
```http
Authorization: Bearer {jwt_token}
```

### User Isolation
All reports automatically filter by authenticated user:
```csharp
var data = await _context.IncomeSources
    .Where(i => i.UserId == userId)
    .ToListAsync();
```

### Authorization Levels

| Endpoint | User | Admin |
|----------|------|-------|
| Personal Reports | ✅ | ✅ |
| Export Reports | ✅ | ✅ |
| All Users Reports | ❌ | ✅ |

### Rate Limiting
Recommended limits:
- Personal Reports: 60 requests/hour
- Export Operations: 10 requests/hour

---

## Performance Considerations

### Caching Strategy
```csharp
// Cache reports for 5 minutes
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "period", "startDate", "endDate" })]
public async Task<IActionResult> GetFullReport([FromQuery] ReportQueryDto query)
```

### Database Optimization
- Add indexes on date fields
- Use `AsNoTracking()` for read-only queries
- Implement pagination for large datasets

### Query Optimization
```csharp
// Efficient query - single database hit
var data = await _context.Bills
    .Where(b => b.UserId == userId && 
                b.DueDate >= startDate && 
                b.DueDate <= endDate)
    .Select(b => new { b.Amount, b.BillType })
    .ToListAsync();
```

---

## Testing Guide

### Test Data Setup
```sql
-- Create test income sources
INSERT INTO IncomeSources (Id, UserId, Name, Amount, Frequency, Category)
VALUES 
  (NEWID(), 'test-user-id', 'Test Salary', 45000, 'MONTHLY', 'PRIMARY'),
  (NEWID(), 'test-user-id', 'Freelance', 5000, 'MONTHLY', 'SIDE_HUSTLE');

-- Create test bills
INSERT INTO Bills (Id, UserId, BillName, Amount, DueDate, BillType, Status)
VALUES 
  (NEWID(), 'test-user-id', 'Electricity', 2500, GETUTCDATE(), 'UTILITY', 'PENDING');
```

### HTTP Test Files
Create `UtilityHub360/Tests/FinancialReports.http`:
```http
### 1. Get Full Report
GET http://localhost:5000/api/Reports/full?period=MONTHLY
Authorization: Bearer {{token}}

### 2. Get Summary
GET http://localhost:5000/api/Reports/summary
Authorization: Bearer {{token}}

### 3. Get Income Report
GET http://localhost:5000/api/Reports/income?period=MONTHLY
Authorization: Bearer {{token}}

### 4. Get Insights
GET http://localhost:5000/api/Reports/insights
Authorization: Bearer {{token}}

### 5. Export PDF
POST http://localhost:5000/api/Reports/export/pdf
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "format": "PDF",
  "reportType": "FULL",
  "startDate": "2025-10-01",
  "endDate": "2025-10-31"
}
```

---

## Database Impact

### ❌ No Database Changes Required

**Read-Only Operations**:
- ✅ Queries existing tables
- ✅ Performs in-memory calculations
- ✅ Generates reports on-demand
- ❌ No new tables created
- ❌ No schema modifications
- ❌ No data writes

**Tables Accessed** (Read-Only):
- Users
- IncomeSources
- Bills
- Payments
- BankAccounts
- BankTransactions
- Loans
- VariableExpenses
- SavingsAccounts
- SavingsTransactions
- UserProfiles

---

## Deployment Checklist

### Backend Setup
- [ ] Service registered in `Program.cs`
- [ ] Controller added and routes configured
- [ ] Authentication middleware enabled
- [ ] CORS policy configured
- [ ] Environment variables set

### Database
- [ ] No migrations needed
- [ ] Existing data verified
- [ ] Database indexes added (optional)

### Frontend
- [ ] API client configured
- [ ] Chart library installed
- [ ] Authentication token handling
- [ ] Error handling implemented
- [ ] Loading states added

### Testing
- [ ] All endpoints tested
- [ ] Sample data verified
- [ ] Chart rendering verified
- [ ] Export functionality tested
- [ ] Performance tested

---

## Support & Troubleshooting

### Common Issues

**Issue**: "User not authenticated"  
**Solution**: Ensure valid JWT token in Authorization header

**Issue**: "No data available"  
**Solution**: Verify user has income sources, bills, or expenses

**Issue**: "Calculation errors"  
**Solution**: Check date ranges and ensure data exists for the period

**Issue**: "Slow performance"  
**Solution**: Implement caching, add database indexes, optimize queries

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-10-28 | Initial release |

---

## Contact & Support

For technical support or feature requests:
- **Email**: support@utilityhub360.com
- **Documentation**: https://docs.utilityhub360.com
- **API Status**: https://status.utilityhub360.com

---

**Last Updated**: October 28, 2025  
**Document Version**: 1.0.0

