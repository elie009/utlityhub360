# 🚀 Financial Dashboard — Quick Start Guide

## Get Your Dashboard Running in 10 Minutes!

This guide will help you integrate the Financial Dashboard and Disposable Amount features into your frontend application quickly.

---

## 📋 Prerequisites

- JWT Authentication token
- Base API URL configured
- HTTP client library (axios, fetch, etc.)

---

## ⚡ Quick Setup

### Step 1: Configure API Client (2 minutes)

```javascript
// api/config.js
export const API_BASE_URL = 'https://your-domain.com/api';

export const getAuthHeaders = () => {
  const token = localStorage.getItem('authToken');
  return {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  };
};
```

### Step 2: Create API Service (3 minutes)

```javascript
// api/dashboardService.js
import axios from 'axios';
import { API_BASE_URL, getAuthHeaders } from './config';

const dashboardAPI = axios.create({
  baseURL: `${API_BASE_URL}/Dashboard`,
});

// Add auth header to all requests
dashboardAPI.interceptors.request.use(config => {
  Object.assign(config.headers, getAuthHeaders());
  return config;
});

export const dashboardService = {
  // Get current month disposable amount
  getCurrentDisposableAmount: async (targetSavings, investmentAllocation) => {
    const { data } = await dashboardAPI.get('/disposable-amount/current', {
      params: { targetSavings, investmentAllocation }
    });
    return data.data;
  },

  // Get full financial summary
  getFinancialSummary: async () => {
    const { data } = await dashboardAPI.get('/financial-summary');
    return data.data;
  },

  // Get specific month
  getMonthlyDisposableAmount: async (year, month, targetSavings) => {
    const { data } = await dashboardAPI.get('/disposable-amount/monthly', {
      params: { year, month, targetSavings }
    });
    return data.data;
  }
};
```

```javascript
// api/expenseService.js
import axios from 'axios';
import { API_BASE_URL, getAuthHeaders } from './config';

const expenseAPI = axios.create({
  baseURL: `${API_BASE_URL}/VariableExpenses`,
});

expenseAPI.interceptors.request.use(config => {
  Object.assign(config.headers, getAuthHeaders());
  return config;
});

export const expenseService = {
  // Get all expenses
  getExpenses: async (startDate, endDate, category) => {
    const { data } = await expenseAPI.get('/', {
      params: { startDate, endDate, category }
    });
    return data.data;
  },

  // Create expense
  createExpense: async (expense) => {
    const { data } = await expenseAPI.post('/', expense);
    return data.data;
  },

  // Update expense
  updateExpense: async (id, expense) => {
    const { data } = await expenseAPI.put(`/${id}`, expense);
    return data.data;
  },

  // Delete expense
  deleteExpense: async (id) => {
    const { data } = await expenseAPI.delete(`/${id}`);
    return data.data;
  },

  // Get statistics
  getStatistics: async (startDate, endDate) => {
    const { data } = await expenseAPI.get('/statistics/by-category', {
      params: { startDate, endDate }
    });
    return data.data;
  }
};
```

### Step 3: Create Dashboard Component (5 minutes)

```jsx
// components/Dashboard.jsx
import React, { useState, useEffect } from 'react';
import { dashboardService } from '../api/dashboardService';
import DisposableAmountCard from './DisposableAmountCard';
import IncomeVsExpensesChart from './IncomeVsExpensesChart';
import ExpenseBreakdownChart from './ExpenseBreakdownChart';
import InsightsPanel from './InsightsPanel';

const Dashboard = () => {
  const [loading, setLoading] = useState(true);
  const [financialData, setFinancialData] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const data = await dashboardService.getFinancialSummary();
      setFinancialData(data);
      setError(null);
    } catch (err) {
      setError('Failed to load dashboard data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="loading">Loading dashboard...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!financialData) return null;

  return (
    <div className="dashboard">
      <h1>Financial Dashboard</h1>
      
      {/* Hero Card */}
      <DisposableAmountCard 
        amount={financialData.currentMonth.disposableAmount}
        percentage={financialData.currentMonth.savingsRate}
        trend={financialData.currentMonth.disposableAmount > financialData.previousMonth.disposableAmount ? 'up' : 'down'}
      />

      {/* Charts Section */}
      <div className="charts-grid">
        <IncomeVsExpensesChart 
          income={financialData.currentMonth.totalIncome}
          fixed={financialData.currentMonth.fixedExpenses}
          variable={financialData.currentMonth.variableExpenses}
          disposable={financialData.currentMonth.disposableAmount}
        />
        
        <ExpenseBreakdownChart 
          data={financialData.variableExpensesBreakdown}
        />
      </div>

      {/* Quick Stats */}
      <div className="quick-stats">
        <StatCard 
          title="Avg Monthly Income" 
          value={financialData.stats.averageMonthlyIncome}
        />
        <StatCard 
          title="Avg Expenses" 
          value={financialData.stats.averageMonthlyExpenses}
        />
        <StatCard 
          title="Active Loans" 
          value={financialData.stats.activeLoans}
        />
        <StatCard 
          title="Pending Bills" 
          value={financialData.stats.pendingBills}
        />
      </div>
    </div>
  );
};

export default Dashboard;
```

---

## 🎨 Ready-to-Use Component Examples

### Disposable Amount Hero Card

```jsx
// components/DisposableAmountCard.jsx
import React from 'react';
import './DisposableAmountCard.css';

const DisposableAmountCard = ({ amount, percentage, trend }) => {
  const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'PHP',
      minimumFractionDigits: 2
    }).format(value);
  };

  const getStatusClass = () => {
    if (percentage > 20) return 'healthy';
    if (percentage > 10) return 'warning';
    return 'critical';
  };

  const getTrendIcon = () => {
    return trend === 'up' ? '↑' : '↓';
  };

  return (
    <div className={`disposable-card ${getStatusClass()}`}>
      <div className="card-header">
        <h2>💰 Disposable Amount</h2>
        <span className={`trend trend-${trend}`}>
          {getTrendIcon()} {trend === 'up' ? '+' : ''}{percentage.toFixed(1)}%
        </span>
      </div>
      
      <div className="card-body">
        <div className="amount-display">
          {formatCurrency(amount)}
        </div>
        
        <div className="percentage-display">
          {percentage.toFixed(1)}% of income
        </div>
      </div>
      
      <div className="card-footer">
        <span className={`status-badge ${getStatusClass()}`}>
          {getStatusClass().toUpperCase()}
        </span>
      </div>
    </div>
  );
};

export default DisposableAmountCard;
```

```css
/* DisposableAmountCard.css */
.disposable-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  margin-bottom: 24px;
}

.disposable-card.healthy {
  border-left: 4px solid #22c55e;
}

.disposable-card.warning {
  border-left: 4px solid #f59e0b;
}

.disposable-card.critical {
  border-left: 4px solid #ef4444;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.amount-display {
  font-size: 3rem;
  font-weight: bold;
  color: #1f2937;
  margin: 16px 0;
}

.percentage-display {
  font-size: 1.125rem;
  color: #6b7280;
}

.trend {
  font-size: 1rem;
  font-weight: 600;
  padding: 4px 12px;
  border-radius: 12px;
}

.trend-up {
  color: #22c55e;
  background: rgba(34, 197, 94, 0.1);
}

.trend-down {
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
}

.status-badge {
  display: inline-block;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: uppercase;
}

.status-badge.healthy {
  background: rgba(34, 197, 94, 0.1);
  color: #22c55e;
}

.status-badge.warning {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

.status-badge.critical {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
}
```

### Quick Expense Entry Form

```jsx
// components/QuickExpenseForm.jsx
import React, { useState } from 'react';
import { expenseService } from '../api/expenseService';

const QuickExpenseForm = ({ onExpenseAdded }) => {
  const [formData, setFormData] = useState({
    description: '',
    amount: '',
    category: 'OTHER',
    expenseDate: new Date().toISOString().split('T')[0]
  });
  const [loading, setLoading] = useState(false);

  const categories = [
    'FOOD', 'GROCERIES', 'TRANSPORTATION', 'ENTERTAINMENT',
    'SHOPPING', 'HEALTHCARE', 'EDUCATION', 'TRAVEL', 'OTHER'
  ];

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const expense = {
        ...formData,
        amount: parseFloat(formData.amount),
        expenseDate: new Date(formData.expenseDate).toISOString()
      };
      
      await expenseService.createExpense(expense);
      
      // Reset form
      setFormData({
        description: '',
        amount: '',
        category: 'OTHER',
        expenseDate: new Date().toISOString().split('T')[0]
      });
      
      // Callback to refresh data
      if (onExpenseAdded) onExpenseAdded();
      
      alert('Expense added successfully!');
    } catch (error) {
      console.error('Failed to add expense:', error);
      alert('Failed to add expense');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="quick-expense-form">
      <h3>Quick Add Expense</h3>
      
      <input
        type="text"
        placeholder="Description (e.g., Lunch at Jollibee)"
        value={formData.description}
        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
        required
      />
      
      <input
        type="number"
        step="0.01"
        placeholder="Amount"
        value={formData.amount}
        onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
        required
      />
      
      <select
        value={formData.category}
        onChange={(e) => setFormData({ ...formData, category: e.target.value })}
      >
        {categories.map(cat => (
          <option key={cat} value={cat}>{cat}</option>
        ))}
      </select>
      
      <input
        type="date"
        value={formData.expenseDate}
        onChange={(e) => setFormData({ ...formData, expenseDate: e.target.value })}
        required
      />
      
      <button type="submit" disabled={loading}>
        {loading ? 'Adding...' : 'Add Expense'}
      </button>
    </form>
  );
};

export default QuickExpenseForm;
```

### Expense Statistics Chart

```jsx
// components/ExpenseBreakdownChart.jsx
import React from 'react';
import { Pie } from 'react-chartjs-2';

const ExpenseBreakdownChart = ({ data }) => {
  if (!data || data.length === 0) {
    return <div>No expense data available</div>;
  }

  const chartData = {
    labels: data.map(item => item.category),
    datasets: [{
      data: data.map(item => item.totalAmount),
      backgroundColor: [
        '#3b82f6', // blue
        '#10b981', // green
        '#f59e0b', // amber
        '#ef4444', // red
        '#8b5cf6', // purple
        '#ec4899', // pink
        '#14b8a6', // teal
        '#f97316', // orange
      ],
      borderWidth: 2,
      borderColor: '#fff'
    }]
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'right',
      },
      tooltip: {
        callbacks: {
          label: function(context) {
            const label = context.label || '';
            const value = context.parsed || 0;
            const percentage = data[context.dataIndex].percentage;
            return `${label}: ₱${value.toFixed(2)} (${percentage.toFixed(1)}%)`;
          }
        }
      }
    }
  };

  return (
    <div className="chart-container">
      <h3>Variable Expenses Breakdown</h3>
      <Pie data={chartData} options={options} />
    </div>
  );
};

export default ExpenseBreakdownChart;
```

---

## 📱 Mobile-Friendly Example

```jsx
// components/MobileDashboard.jsx
import React, { useState, useEffect } from 'react';
import { dashboardService } from '../api/dashboardService';

const MobileDashboard = () => {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadSummary();
  }, []);

  const loadSummary = async () => {
    try {
      const data = await dashboardService.getCurrentDisposableAmount();
      setSummary(data);
    } catch (error) {
      console.error('Failed to load summary:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="mobile-loading">Loading...</div>;
  }

  return (
    <div className="mobile-dashboard">
      {/* Swipeable Cards */}
      <div className="card-carousel">
        {/* Disposable Amount Card */}
        <div className="mobile-card">
          <div className="card-icon">💰</div>
          <div className="card-title">Disposable Amount</div>
          <div className="card-value">₱{summary.disposableAmount.toLocaleString()}</div>
          <div className="card-subtitle">
            {summary.disposablePercentage.toFixed(1)}% of income
          </div>
        </div>

        {/* Income Card */}
        <div className="mobile-card">
          <div className="card-icon">📈</div>
          <div className="card-title">Total Income</div>
          <div className="card-value">₱{summary.totalIncome.toLocaleString()}</div>
          <div className="card-subtitle">This month</div>
        </div>

        {/* Expenses Card */}
        <div className="mobile-card">
          <div className="card-icon">💳</div>
          <div className="card-title">Total Expenses</div>
          <div className="card-value">
            ₱{(summary.totalFixedExpenses + summary.totalVariableExpenses).toLocaleString()}
          </div>
          <div className="card-subtitle">
            Fixed + Variable
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="quick-actions">
        <button className="action-btn" onClick={() => {/* navigate to add expense */}}>
          <span className="icon">➕</span>
          Add Expense
        </button>
        <button className="action-btn" onClick={() => {/* navigate to view expenses */}}>
          <span className="icon">📊</span>
          View Expenses
        </button>
      </div>

      {/* Insights */}
      <div className="mobile-insights">
        <h3>💡 Insights</h3>
        {summary.insights.map((insight, index) => (
          <div key={index} className="insight-item">
            {insight}
          </div>
        ))}
      </div>
    </div>
  );
};

export default MobileDashboard;
```

```css
/* MobileDashboard.css */
.mobile-dashboard {
  padding: 16px;
  background: #f9fafb;
  min-height: 100vh;
}

.card-carousel {
  display: flex;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  gap: 16px;
  padding: 16px 0;
  margin-bottom: 24px;
}

.mobile-card {
  flex: 0 0 280px;
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  scroll-snap-align: start;
}

.card-icon {
  font-size: 2.5rem;
  margin-bottom: 12px;
}

.card-title {
  font-size: 0.875rem;
  color: #6b7280;
  margin-bottom: 8px;
}

.card-value {
  font-size: 2rem;
  font-weight: bold;
  color: #1f2937;
  margin-bottom: 4px;
}

.card-subtitle {
  font-size: 0.875rem;
  color: #9ca3af;
}

.quick-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 24px;
}

.action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 16px;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
}

.mobile-insights {
  background: white;
  border-radius: 16px;
  padding: 20px;
}

.insight-item {
  padding: 12px;
  background: #f9fafb;
  border-radius: 8px;
  margin-bottom: 8px;
  font-size: 0.875rem;
  color: #374151;
}
```

---

## 🧪 Testing Your Integration

### 1. Test API Connection

```javascript
// test/apiTest.js
import { dashboardService } from '../api/dashboardService';

const testAPI = async () => {
  try {
    console.log('Testing API connection...');
    
    const summary = await dashboardService.getFinancialSummary();
    console.log('✅ Financial Summary:', summary);
    
    const disposable = await dashboardService.getCurrentDisposableAmount(5000, 3000);
    console.log('✅ Disposable Amount:', disposable);
    
    console.log('All tests passed!');
  } catch (error) {
    console.error('❌ API Test Failed:', error);
  }
};

testAPI();
```

### 2. Test Component Rendering

```jsx
// test/Dashboard.test.jsx
import { render, screen, waitFor } from '@testing-library/react';
import Dashboard from '../components/Dashboard';
import { dashboardService } from '../api/dashboardService';

jest.mock('../api/dashboardService');

test('renders dashboard with data', async () => {
  const mockData = {
    currentMonth: {
      disposableAmount: 19511,
      savingsRate: 28.07,
      totalIncome: 69510
    },
    stats: {
      averageMonthlyIncome: 67354
    }
  };

  dashboardService.getFinancialSummary.mockResolvedValue(mockData);

  render(<Dashboard />);

  await waitFor(() => {
    expect(screen.getByText(/Financial Dashboard/i)).toBeInTheDocument();
    expect(screen.getByText(/19511/)).toBeInTheDocument();
  });
});
```

---

## 🐛 Common Issues & Solutions

### Issue 1: 401 Unauthorized

**Problem:** API returns 401 error

**Solution:**
```javascript
// Check if token is valid
const token = localStorage.getItem('authToken');
if (!token) {
  // Redirect to login
  window.location.href = '/login';
}

// Check token expiration
const payload = JSON.parse(atob(token.split('.')[1]));
if (payload.exp * 1000 < Date.now()) {
  // Token expired, refresh or redirect to login
  localStorage.removeItem('authToken');
  window.location.href = '/login';
}
```

### Issue 2: CORS Errors

**Problem:** Request blocked by CORS policy

**Solution:** Ensure your backend allows your frontend origin:
```csharp
// In Program.cs (already configured)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### Issue 3: Data Not Updating

**Problem:** Dashboard shows stale data

**Solution:**
```javascript
// Add refresh function
const refreshDashboard = async () => {
  setLoading(true);
  const data = await dashboardService.getFinancialSummary();
  setFinancialData(data);
  setLoading(false);
};

// Auto-refresh every 5 minutes
useEffect(() => {
  const interval = setInterval(refreshDashboard, 5 * 60 * 1000);
  return () => clearInterval(interval);
}, []);
```

---

## 📚 Next Steps

1. ✅ Implement the basic dashboard
2. 📊 Add charts using Chart.js or Recharts
3. 💾 Add state management (Redux/Context)
4. 📱 Make it responsive
5. 🔔 Add push notifications
6. 🎨 Customize styling to match your brand
7. 🧪 Add comprehensive tests

---

## 🔗 Additional Resources

- [Full API Documentation](./disposableAmountApiDocumentation.md)
- [Complete Flow Guide](./disposableAmountFlow.md)
- [Dashboard Widgets Guide](./dashboardWidgetsGuide.md)
- [Implementation Details](../DISPOSABLE_AMOUNT_FEATURE_IMPLEMENTATION.md)

---

**Guide Version:** 1.0.0  
**Last Updated:** October 11, 2025  
**Status:** Ready to Use ✅

**Need Help?** Check the API documentation or contact the development team.

