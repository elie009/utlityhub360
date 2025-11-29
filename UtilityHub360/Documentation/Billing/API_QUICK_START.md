# Variable Monthly Billing - API Quick Start Guide

## 🚀 Quick Start for Frontend Developers

This guide provides a fast-track introduction to integrating the Variable Monthly Billing API.

---

## 📋 Prerequisites

- JWT authentication token
- Base URL: `http://localhost:5000/api` (Development)
- All requests require `Authorization: Bearer {token}` header

---

## 🎯 Common Use Cases

### 1. Add a New Bill

```javascript
const response = await fetch('http://localhost:5000/api/bills', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    billName: 'Electricity Bill - October 2025',
    billType: 'utility',
    provider: 'Meralco',
    amount: 3050.00,
    dueDate: '2025-10-10T00:00:00Z',
    frequency: 'monthly',
    notes: 'October electricity usage'
  })
});

const result = await response.json();
console.log(result.data.id); // Save this bill ID
```

---

### 2. Get Bill History with Analytics

```javascript
const provider = 'Meralco';
const billType = 'utility';
const months = 6;

const response = await fetch(
  `http://localhost:5000/api/bills/analytics/history?provider=${provider}&billType=${billType}&months=${months}`,
  {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  }
);

const data = await response.json();

// Access the data
console.log(data.bills); // Array of bills
console.log(data.analytics.averageSimple); // Simple average
console.log(data.analytics.averageWeighted); // Weighted average
console.log(data.analytics.trend); // "increasing", "decreasing", or "stable"
console.log(data.forecast.estimatedAmount); // Next month's forecast
```

---

### 3. Create a Budget

```javascript
const response = await fetch('http://localhost:5000/api/bills/budgets', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    provider: 'Meralco',
    billType: 'utility',
    monthlyBudget: 3000.00,
    enableAlerts: true,
    alertThreshold: 90 // Alert at 90% of budget
  })
});

const budget = await response.json();
console.log(budget.data.id); // Budget ID
```

---

### 4. Check Budget Status

```javascript
const provider = 'Meralco';
const billType = 'utility';

const response = await fetch(
  `http://localhost:5000/api/bills/budgets/status?provider=${provider}&billType=${billType}`,
  {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  }
);

const status = await response.json();

console.log(status.data.monthlyBudget); // Budget amount
console.log(status.data.currentBill); // Current spending
console.log(status.data.remaining); // Amount remaining
console.log(status.data.percentageUsed); // Percentage used
console.log(status.data.status); // "on_track", "approaching_limit", or "over_budget"
console.log(status.data.message); // User-friendly message
```

---

### 5. Get Forecast

```javascript
const provider = 'Meralco';
const billType = 'utility';
const method = 'weighted'; // or 'simple', 'seasonal'

const response = await fetch(
  `http://localhost:5000/api/bills/analytics/forecast?provider=${provider}&billType=${billType}&method=${method}`,
  {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  }
);

const forecast = await response.json();

console.log(forecast.data.estimatedAmount); // Estimated next month
console.log(forecast.data.confidence); // "high", "medium", or "low"
console.log(forecast.data.recommendation); // Advice for user
```

---

### 6. Calculate Variance for a Bill

```javascript
const billId = 'bill-123';

const response = await fetch(
  `http://localhost:5000/api/bills/${billId}/variance`,
  {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  }
);

const variance = await response.json();

console.log(variance.data.actualAmount); // Actual bill amount
console.log(variance.data.estimatedAmount); // What was estimated
console.log(variance.data.variance); // Difference
console.log(variance.data.variancePercentage); // Percentage difference
console.log(variance.data.status); // "over_budget", "slightly_over", "on_target", "under_budget"
console.log(variance.data.message); // User-friendly message
console.log(variance.data.recommendation); // Advice
```

---

### 7. Get Dashboard Data

```javascript
const response = await fetch('http://localhost:5000/api/bills/dashboard', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

const dashboard = await response.json();

// Access all dashboard data
console.log(dashboard.data.currentBills); // Bills this month
console.log(dashboard.data.upcomingBills); // Bills due soon
console.log(dashboard.data.overdueBills); // Overdue bills
console.log(dashboard.data.providerAnalytics); // Provider breakdowns
console.log(dashboard.data.budgetStatuses); // All budget statuses
console.log(dashboard.data.alerts); // Recent alerts
console.log(dashboard.data.summary); // Overall summary
```

---

### 8. Get Alerts

```javascript
const response = await fetch(
  'http://localhost:5000/api/bills/alerts?isRead=false&limit=10',
  {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  }
);

const alerts = await response.json();

alerts.data.forEach(alert => {
  console.log(alert.title); // Alert title
  console.log(alert.message); // Alert message
  console.log(alert.severity); // "info", "warning", "error", "success"
  console.log(alert.alertType); // "due_date", "overdue", "budget_exceeded", etc.
  console.log(alert.actionLink); // Link to take action
});
```

---

### 9. Mark Alert as Read

```javascript
const alertId = 'alert-123';

const response = await fetch(
  `http://localhost:5000/api/bills/alerts/${alertId}/read`,
  {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  }
);

const result = await response.json();
console.log(result.message); // "Alert marked as read"
```

---

### 10. Get Monthly Trend

```javascript
const provider = 'Meralco';
const billType = 'utility';
const months = 12;

const response = await fetch(
  `http://localhost:5000/api/bills/analytics/trend?provider=${provider}&billType=${billType}&months=${months}`,
  {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  }
);

const trend = await response.json();

// Perfect for charts!
trend.data.forEach(month => {
  console.log(month.monthName); // "May 2025"
  console.log(month.totalAmount); // Total spent that month
  console.log(month.billCount); // Number of bills
  console.log(month.status); // "paid", "pending", "overdue"
});
```

---

## 🎨 React Example Components

### Bill History Component

```jsx
import React, { useEffect, useState } from 'react';

function BillHistory({ provider, billType, authToken }) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchData() {
      const response = await fetch(
        `http://localhost:5000/api/bills/analytics/history?provider=${provider}&billType=${billType}&months=6`,
        {
          headers: {
            'Authorization': `Bearer ${authToken}`,
            'Content-Type': 'application/json'
          }
        }
      );
      const result = await response.json();
      setData(result.data);
      setLoading(false);
    }
    fetchData();
  }, [provider, billType, authToken]);

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      <h2>{provider} - Bill Analytics</h2>
      
      {/* Display Analytics */}
      <div className="analytics">
        <p>Average (Simple): ${data.analytics.averageSimple.toFixed(2)}</p>
        <p>Average (Weighted): ${data.analytics.averageWeighted.toFixed(2)}</p>
        <p>Trend: {data.analytics.trend}</p>
        <p>Total Spent: ${data.analytics.totalSpent.toFixed(2)}</p>
      </div>

      {/* Display Forecast */}
      <div className="forecast">
        <h3>Next Month Forecast</h3>
        <p>Estimated: ${data.forecast.estimatedAmount.toFixed(2)}</p>
        <p>Confidence: {data.forecast.confidence}</p>
        <p>{data.forecast.recommendation}</p>
      </div>

      {/* Display Bills */}
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Amount</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {data.bills.map(bill => (
            <tr key={bill.id}>
              <td>{new Date(bill.createdAt).toLocaleDateString()}</td>
              <td>${bill.amount.toFixed(2)}</td>
              <td>{bill.status}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default BillHistory;
```

---

### Budget Status Component

```jsx
import React, { useEffect, useState } from 'react';

function BudgetStatus({ provider, billType, authToken }) {
  const [status, setStatus] = useState(null);

  useEffect(() => {
    async function fetchStatus() {
      const response = await fetch(
        `http://localhost:5000/api/bills/budgets/status?provider=${provider}&billType=${billType}`,
        {
          headers: {
            'Authorization': `Bearer ${authToken}`,
            'Content-Type': 'application/json'
          }
        }
      );
      const result = await response.json();
      setStatus(result.data);
    }
    fetchStatus();
  }, [provider, billType, authToken]);

  if (!status) return <div>Loading...</div>;

  const progressColor = status.status === 'over_budget' ? 'red' : 
                       status.status === 'approaching_limit' ? 'orange' : 'green';

  return (
    <div className="budget-status">
      <h3>{provider} Budget</h3>
      <div className="progress-bar">
        <div 
          style={{
            width: `${Math.min(status.percentageUsed, 100)}%`,
            backgroundColor: progressColor
          }}
        />
      </div>
      <p>${status.currentBill.toFixed(2)} / ${status.monthlyBudget.toFixed(2)}</p>
      <p className={`status ${status.status}`}>{status.message}</p>
    </div>
  );
}

export default BudgetStatus;
```

---

## 📱 Testing in Swagger

1. Navigate to: `http://localhost:5000/swagger`
2. Click **"Authorize"** button (top right)
3. Enter: `Bearer {your_token}`
4. Try any endpoint with the **"Try it out"** button

---

## 🔗 Additional Resources

- **Full API Documentation:** [billingApiDocumentation.md](./billingApiDocumentation.md)
- **User Guide:** [variableMonthlyBillingFlow.md](./variableMonthlyBillingFlow.md)
- **Implementation Guide:** [variableMonthlyBillingImplementation.md](./variableMonthlyBillingImplementation.md)
- **Swagger UI:** `http://localhost:5000/swagger`

---

## 🐛 Common Issues

### 401 Unauthorized
- Check your JWT token is valid
- Ensure you're including the `Bearer ` prefix
- Token may have expired - get a new one from `/api/auth/login`

### 404 Not Found
- Check the endpoint URL is correct
- Verify the bill ID or budget ID exists
- Check query parameters are properly formatted

### 400 Bad Request
- Validate your request body matches the required format
- Check required fields are present
- Verify data types (numbers, strings, dates)

---

**Last Updated:** October 11, 2025  
**Version:** 2.0.0  
**Status:** ✅ Production Ready

