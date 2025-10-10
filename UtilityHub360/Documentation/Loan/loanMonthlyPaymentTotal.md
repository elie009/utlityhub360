# Loan Monthly Payment Total API Documentation

## Overview

The Loan Monthly Payment Total API provides an endpoint to retrieve the total monthly payment obligation across all active loans for a user. This helps users understand their total monthly loan commitments and plan their finances accordingly.

## Table of Contents
- [Purpose](#purpose)
- [Endpoint](#endpoint)
- [Authentication](#authentication)
- [Request & Response Examples](#request--response-examples)
- [Use Cases](#use-cases)
- [Frontend Integration](#frontend-integration)
- [Future Enhancements](#future-enhancements)

---

## Purpose

This API endpoint solves a critical user need:

**"As a user with multiple active loans, I want to see my total monthly loan payment obligation so I can budget accordingly."**

### Key Benefits:
- ✅ Aggregate view of all active loan payments
- ✅ Single source of truth for monthly loan obligations
- ✅ Helps with budgeting and financial planning
- ✅ Includes breakdown by individual loans

---

## Endpoint

### Get Total Monthly Payment Obligation

Returns the total monthly payment amount across all active loans for the authenticated user.

**Endpoint:**
```
GET /api/loans/monthly-payment-total
```

**Authentication:** Required (JWT Bearer Token)

**Query Parameters:** None

**Filters:**
- Only includes loans with `Status = "ACTIVE"`
- User-specific (authenticated user's loans only)

---

## Authentication

The endpoint requires authentication using JWT Bearer token.

**Header:**
```
Authorization: Bearer {your-jwt-token}
```

**Access Control:**
- Users can only view their own loan data
- UserId is extracted from the JWT token

---

## Request & Response Examples

### Example 1: User with Multiple Active Loans

**Request:**
```http
GET /api/loans/monthly-payment-total HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalMonthlyPayment": 1850.00,
    "totalRemainingBalance": 45000.00,
    "activeLoanCount": 3,
    "totalPayment": 72,
    "totalPaymentRemaining": 35,
    "totalMonthsRemaining": 35,
    "loans": [
      {
        "id": "loan-123",
        "purpose": "Home Renovation",
        "monthlyPayment": 800.00,
        "remainingBalance": 20000.00,
        "interestRate": 5.5,
        "totalInstallments": 30,
        "installmentsRemaining": 15,
        "monthsRemaining": 25
      },
      {
        "id": "loan-456",
        "purpose": "Car Purchase",
        "monthlyPayment": 650.00,
        "remainingBalance": 18000.00,
        "interestRate": 4.8,
        "totalInstallments": 24,
        "installmentsRemaining": 12,
        "monthsRemaining": 28
      },
      {
        "id": "loan-789",
        "purpose": "Education",
        "monthlyPayment": 400.00,
        "remainingBalance": 7000.00,
        "interestRate": 3.5,
        "totalInstallments": 18,
        "installmentsRemaining": 8,
        "monthsRemaining": 18
      }
    ]
  },
  "errors": null
}
```

---

### Example 2: User with Single Active Loan

**Request:**
```http
GET /api/loans/monthly-payment-total HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalMonthlyPayment": 500.00,
    "totalRemainingBalance": 10000.00,
    "activeLoanCount": 1,
    "totalPayment": 24,
    "totalPaymentRemaining": 10,
    "totalMonthsRemaining": 20,
    "loans": [
      {
        "id": "loan-999",
        "purpose": "Personal Loan",
        "monthlyPayment": 500.00,
        "remainingBalance": 10000.00,
        "interestRate": 6.0,
        "totalInstallments": 24,
        "installmentsRemaining": 10,
        "monthsRemaining": 20
      }
    ]
  },
  "errors": null
}
```

---

### Example 3: User with No Active Loans

**Request:**
```http
GET /api/loans/monthly-payment-total HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalMonthlyPayment": 0.00,
    "totalRemainingBalance": 0.00,
    "activeLoanCount": 0,
    "totalPayment": 0,
    "totalPaymentRemaining": 0,
    "totalMonthsRemaining": 0,
    "loans": []
  },
  "errors": null
}
```

---

## Response Schema

### Response Object Structure

```typescript
{
  success: boolean,
  message: string,
  data: {
    totalMonthlyPayment: number,      // Sum of all active loan monthly payments
    totalRemainingBalance: number,    // Sum of all active loan remaining balances
    activeLoanCount: number,          // Number of active loans
    totalPayment: number,             // Total number of installments (all loans combined)
    totalPaymentRemaining: number,    // Total number of installments remaining (pending/overdue)
    totalMonthsRemaining: number,     // Total months remaining (sum across all loans)
    loans: [
      {
        id: string,                   // Loan ID
        purpose: string,              // Loan purpose
        monthlyPayment: number,       // Monthly payment for this loan
        remainingBalance: number,     // Remaining balance for this loan
        interestRate: number,         // Interest rate percentage
        totalInstallments: number,    // Total number of installments for this loan
        installmentsRemaining: number,// Installments remaining for this loan
        monthsRemaining: number       // Months remaining for this loan
      }
    ]
  },
  errors: array | null
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `totalMonthlyPayment` | decimal | The sum of monthly payment amounts across all active loans |
| `totalRemainingBalance` | decimal | The sum of remaining balances across all active loans |
| `activeLoanCount` | integer | The count of active loans |
| `totalPayment` | integer | The total number of installments/payments (all loans combined) |
| `totalPaymentRemaining` | integer | The total number of installments remaining (PENDING or OVERDUE status) |
| `totalMonthsRemaining` | integer | The total months remaining (sum of months needed to pay off all loans) |
| `loans` | array | Array of individual loan details |
| `loans[].id` | string | Unique loan identifier |
| `loans[].purpose` | string | Purpose/reason for the loan |
| `loans[].monthlyPayment` | decimal | Monthly payment amount for this specific loan |
| `loans[].remainingBalance` | decimal | Remaining balance for this specific loan |
| `loans[].interestRate` | decimal | Annual interest rate percentage for this loan |
| `loans[].totalInstallments` | integer | Total number of installments for this loan |
| `loans[].installmentsRemaining` | integer | Number of installments remaining (PENDING or OVERDUE) |
| `loans[].monthsRemaining` | integer | Calculated months remaining (remainingBalance / monthlyPayment) |

---

## Error Handling

### Common Error Responses

#### 401 Unauthorized - Missing or Invalid Token

**Response:**
```json
{
  "success": false,
  "message": "User not authenticated",
  "data": null,
  "errors": null
}
```

**Cause:** JWT token is missing, expired, or invalid

---

#### 500 Internal Server Error

**Response:**
```json
{
  "success": false,
  "message": "Failed to get monthly payment total: [error details]",
  "data": null,
  "errors": null
}
```

**Cause:** Server-side error (database connection, unexpected exception, etc.)

---

## Important Notes

### Loan Status Filter

- **Only ACTIVE loans are included** in the calculation
- Loans with status `PENDING`, `APPROVED`, `REJECTED`, `COMPLETED`, or `DEFAULTED` are excluded
- This ensures users only see current payment obligations

### Currency

- All amounts are in the default currency (typically USD)
- Currency field is not currently included but can be added in future versions

### Calculation Method

The total monthly payment is calculated as:
```
TotalMonthlyPayment = Sum(MonthlyPayment) for all loans where Status = 'ACTIVE'
```

Currently, all loans in the system use **monthly payment frequency** (the `Term` field is in months).

---

## Use Cases

### 1. Dashboard Summary Widget

Display total monthly loan obligation prominently on the user's dashboard.

```javascript
async function displayLoanObligation() {
  const response = await fetch('/api/loans/monthly-payment-total', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const { data } = await response.json();
  
  document.getElementById('monthly-obligation').textContent = 
    `$${data.totalMonthlyPayment.toFixed(2)}`;
  document.getElementById('loan-count').textContent = 
    `${data.activeLoanCount} active loans`;
}
```

---

### 2. Budget Planning Calculator

Help users understand how much of their monthly income goes to loan payments.

```javascript
async function calculateLoanToIncomeRatio() {
  const monthlyIncome = 5000; // User's monthly income
  
  const response = await fetch('/api/loans/monthly-payment-total', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const { data } = await response.json();
  
  const loanToIncomeRatio = (data.totalMonthlyPayment / monthlyIncome * 100).toFixed(1);
  
  console.log(`Monthly Income: $${monthlyIncome}`);
  console.log(`Loan Payments: $${data.totalMonthlyPayment}`);
  console.log(`Loan-to-Income Ratio: ${loanToIncomeRatio}%`);
  
  if (loanToIncomeRatio > 40) {
    alert('⚠️ Your loan payments exceed 40% of your income. Consider debt consolidation.');
  }
}
```

---

### 3. Loan Breakdown Visualization

Create a pie chart showing the distribution of monthly payments across loans.

```javascript
async function createLoanBreakdownChart() {
  const response = await fetch('/api/loans/monthly-payment-total', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const { data } = await response.json();
  
  const chartData = {
    labels: data.loans.map(l => l.purpose),
    datasets: [{
      data: data.loans.map(l => l.monthlyPayment),
      backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0']
    }]
  };
  
  // Render pie chart with your preferred library (Chart.js, etc.)
}
```

---

### 4. Financial Health Check

Alert users when their loan payments are too high relative to income.

```javascript
async function performFinancialHealthCheck() {
  const monthlyIncome = getUserMonthlyIncome(); // From user profile
  
  const response = await fetch('/api/loans/monthly-payment-total', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const { data } = await response.json();
  
  const healthScore = {
    excellent: 20,  // < 20% of income
    good: 30,       // 20-30% of income
    fair: 40,       // 30-40% of income
    poor: 100       // > 40% of income
  };
  
  const ratio = (data.totalMonthlyPayment / monthlyIncome * 100);
  
  let status;
  if (ratio < healthScore.excellent) status = 'Excellent';
  else if (ratio < healthScore.good) status = 'Good';
  else if (ratio < healthScore.fair) status = 'Fair';
  else status = 'Needs Attention';
  
  return {
    status,
    ratio: ratio.toFixed(1),
    totalPayment: data.totalMonthlyPayment,
    recommendation: status === 'Needs Attention' 
      ? 'Consider debt consolidation or refinancing'
      : 'Your loan payments are within healthy limits'
  };
}
```

---

### 5. Loan Comparison Tool

Help users understand which loans have the highest monthly payments.

```javascript
async function analyzeLoanPriority() {
  const response = await fetch('/api/loans/monthly-payment-total', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const { data } = await response.json();
  
  // Sort loans by monthly payment (highest first)
  const sortedLoans = [...data.loans].sort((a, b) => 
    b.monthlyPayment - a.monthlyPayment
  );
  
  console.log('Loans prioritized by monthly payment:');
  sortedLoans.forEach((loan, index) => {
    const percentage = (loan.monthlyPayment / data.totalMonthlyPayment * 100).toFixed(1);
    console.log(`${index + 1}. ${loan.purpose}: $${loan.monthlyPayment} (${percentage}%)`);
  });
}
```

---

## Frontend Integration

### React Example

```jsx
import { useState, useEffect } from 'react';

function LoanPaymentSummary() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function fetchLoanPaymentTotal() {
      try {
        const token = localStorage.getItem('authToken');
        const response = await fetch('http://localhost:5000/api/loans/monthly-payment-total', {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
        
        if (!response.ok) {
          throw new Error('Failed to fetch loan payment total');
        }
        
        const result = await response.json();
        setData(result.data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    fetchLoanPaymentTotal();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;
  if (!data) return null;

  return (
    <div className="loan-payment-summary">
      <div className="summary-header">
        <h2>Monthly Loan Obligations</h2>
        <div className="total-amount">
          <span className="label">Total Monthly Payment:</span>
          <span className="amount">${data.totalMonthlyPayment.toFixed(2)}</span>
        </div>
        <div className="loan-stats">
          <span>{data.activeLoanCount} active loans</span>
          <span>Total remaining: ${data.totalRemainingBalance.toFixed(2)}</span>
        </div>
      </div>

      <div className="loan-breakdown">
        <h3>Loan Breakdown</h3>
        {data.loans.length === 0 ? (
          <p>No active loans</p>
        ) : (
          <ul>
            {data.loans.map((loan) => (
              <li key={loan.id} className="loan-item">
                <div className="loan-purpose">{loan.purpose}</div>
                <div className="loan-details">
                  <span className="monthly-payment">
                    ${loan.monthlyPayment.toFixed(2)}/month
                  </span>
                  <span className="remaining">
                    ${loan.remainingBalance.toFixed(2)} remaining
                  </span>
                  <span className="interest">
                    {loan.interestRate}% APR
                  </span>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

export default LoanPaymentSummary;
```

---

### Vue.js Example

```vue
<template>
  <div class="loan-payment-summary">
    <div v-if="loading">Loading...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <div v-else-if="data" class="content">
      <div class="summary-card">
        <h2>Monthly Loan Obligations</h2>
        <div class="total-amount">
          ${{ data.totalMonthlyPayment.toFixed(2) }}
        </div>
        <p class="subtitle">
          {{ data.activeLoanCount }} active loans • 
          ${{ data.totalRemainingBalance.toFixed(2) }} remaining
        </p>
      </div>

      <div class="loan-list">
        <h3>Individual Loans</h3>
        <div v-for="loan in data.loans" :key="loan.id" class="loan-card">
          <h4>{{ loan.purpose }}</h4>
          <div class="loan-stats">
            <span>${{ loan.monthlyPayment.toFixed(2) }}/month</span>
            <span>${{ loan.remainingBalance.toFixed(2) }} remaining</span>
            <span>{{ loan.interestRate }}% APR</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      data: null,
      loading: true,
      error: null
    };
  },
  async mounted() {
    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('http://localhost:5000/api/loans/monthly-payment-total', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      if (!response.ok) {
        throw new Error('Failed to fetch loan payment total');
      }
      
      const result = await response.json();
      this.data = result.data;
    } catch (err) {
      this.error = err.message;
    } finally {
      this.loading = false;
    }
  }
};
</script>
```

---

### Vanilla JavaScript Example

```javascript
async function getLoanMonthlyPaymentTotal() {
  const token = localStorage.getItem('authToken');
  
  try {
    const response = await fetch('http://localhost:5000/api/loans/monthly-payment-total', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to fetch loan payment total');
    }
    
    const result = await response.json();
    return result.data;
  } catch (error) {
    console.error('Error fetching loan payment total:', error);
    throw error;
  }
}

// Usage example
async function displayLoanSummary() {
  try {
    const data = await getLoanMonthlyPaymentTotal();
    
    console.log('=== LOAN PAYMENT SUMMARY ===');
    console.log(`Total Monthly Payment: $${data.totalMonthlyPayment.toFixed(2)}`);
    console.log(`Active Loans: ${data.activeLoanCount}`);
    console.log(`Total Remaining Balance: $${data.totalRemainingBalance.toFixed(2)}`);
    console.log('\nIndividual Loans:');
    
    data.loans.forEach((loan, index) => {
      console.log(`\n${index + 1}. ${loan.purpose}`);
      console.log(`   Monthly Payment: $${loan.monthlyPayment.toFixed(2)}`);
      console.log(`   Remaining: $${loan.remainingBalance.toFixed(2)}`);
      console.log(`   Interest Rate: ${loan.interestRate}%`);
    });
  } catch (error) {
    console.error('Failed to display loan summary:', error);
  }
}
```

---

## Future Enhancements

### Payment Frequency Support

Currently, all loans in the system use **monthly payment frequency**. Future versions may support:

| Frequency | Conversion to Monthly | Example |
|-----------|----------------------|---------|
| **Daily** | `dailyAmount × 30.44` | $10/day = $304.40/month |
| **Weekly** | `weeklyAmount × 4.33` | $100/week = $433/month |
| **Bi-weekly** | `biweeklyAmount × 2.17` | $200/biweekly = $434/month |
| **Monthly** | `monthlyAmount × 1` | $500/month = $500/month |
| **Quarterly** | `quarterlyAmount / 3` | $1,500/quarter = $500/month |
| **Semi-annual** | `semiAnnualAmount / 6` | $3,000/6mo = $500/month |
| **Annual** | `annualAmount / 12` | $6,000/year = $500/month |

**Implementation Suggestion:**

Add a `PaymentFrequency` field to the `Loan` entity:

```csharp
[StringLength(20)]
public string PaymentFrequency { get; set; } = "MONTHLY"; // DAILY, WEEKLY, BIWEEKLY, MONTHLY, etc.
```

Then update the calculation logic:

```csharp
private decimal ConvertToMonthly(decimal amount, string frequency)
{
    return frequency.ToUpper() switch
    {
        "DAILY" => amount * 30.44m,
        "WEEKLY" => amount * 4.33m,
        "BIWEEKLY" => amount * 2.17m,
        "MONTHLY" => amount,
        "QUARTERLY" => amount / 3m,
        "SEMIANNUAL" => amount / 6m,
        "ANNUAL" => amount / 12m,
        _ => amount
    };
}
```

---

## Testing with Swagger

1. Start your application
2. Navigate to `http://localhost:5000/swagger`
3. Locate the **Loans** section
4. Find the endpoint: `GET /api/loans/monthly-payment-total`
5. Click "Try it out"
6. Click "Authorize" and enter your JWT token
7. Execute the request

---

## Database Query Logic

The API uses the following logic to calculate the total:

```sql
-- Pseudo SQL representation
SELECT 
  SUM(MonthlyPayment) as TotalMonthlyPayment,
  SUM(RemainingBalance) as TotalRemainingBalance,
  COUNT(*) as ActiveLoanCount
FROM Loans
WHERE 
  UserId = @UserId
  AND Status = 'ACTIVE'
```

**Filters Applied:**
- ✅ User-specific (UserId matches authenticated user)
- ✅ Only active loans (Status = 'ACTIVE')

---

## Performance Considerations

- **Query Efficiency:** Uses a single database query with projection
- **Response Size:** Lightweight (~1-3KB typical)
- **Indexing:** Ensure indexes on `UserId` and `Status` columns
- **Caching:** Consider caching with short TTL (1-5 minutes) if needed

---

## Security

- ✅ Authentication required (JWT token)
- ✅ User data isolation (can only see own loans)
- ✅ SQL injection protection (uses Entity Framework with parameterized queries)
- ✅ No sensitive data exposure (only financial summary data)

---

## Related Endpoints

- `GET /api/loans` - Get all loans for user
- `GET /api/loans/{loanId}` - Get specific loan details
- `GET /api/loans/{loanId}/schedule` - Get loan repayment schedule
- `POST /api/loans/apply` - Apply for a new loan

---

## Support & Troubleshooting

### Common Issues

**Issue:** Getting 401 Unauthorized  
**Solution:** Ensure your JWT token is valid and not expired. Re-login if necessary.

**Issue:** Total is 0 but I have loans  
**Solution:** Check that your loans have Status = 'ACTIVE'. Completed, pending, or rejected loans are not included.

**Issue:** Missing loans in the breakdown  
**Solution:** Verify the loans are marked as 'ACTIVE' in the database. Check loan status.

---

## Changelog

### Version 1.0 - October 10, 2025
- Initial release
- Added GET /api/loans/monthly-payment-total endpoint
- Calculates total monthly payment obligation for active loans
- Includes individual loan breakdown
- Supports only monthly payment frequency (current system design)

---

## Last Updated
October 10, 2025

## API Version
v1.0

