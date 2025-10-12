# 🎨 Financial Dashboard Widgets — Complete UI/UX Guide

## Build Beautiful Dashboard Components

This guide provides complete UI/UX specifications and implementation examples for all dashboard widgets.

---

## 📐 Design System

### Color Palette

```css
/* Primary Colors */
--primary-blue: #3b82f6;
--primary-green: #22c55e;
--primary-amber: #f59e0b;
--primary-red: #ef4444;

/* Status Colors */
--status-healthy: #22c55e;
--status-warning: #f59e0b;
--status-critical: #ef4444;

/* Neutral Colors */
--gray-50: #f9fafb;
--gray-100: #f3f4f6;
--gray-200: #e5e7eb;
--gray-300: #d1d5db;
--gray-400: #9ca3af;
--gray-500: #6b7280;
--gray-600: #4b5563;
--gray-700: #374151;
--gray-800: #1f2937;
--gray-900: #111827;

/* Background Colors */
--bg-primary: #ffffff;
--bg-secondary: #f9fafb;
--bg-tertiary: #f3f4f6;
```

### Typography

```css
/* Font Sizes */
--text-xs: 0.75rem;      /* 12px */
--text-sm: 0.875rem;     /* 14px */
--text-base: 1rem;       /* 16px */
--text-lg: 1.125rem;     /* 18px */
--text-xl: 1.25rem;      /* 20px */
--text-2xl: 1.5rem;      /* 24px */
--text-3xl: 1.875rem;    /* 30px */
--text-4xl: 2.25rem;     /* 36px */
--text-5xl: 3rem;        /* 48px */

/* Font Weights */
--font-normal: 400;
--font-medium: 500;
--font-semibold: 600;
--font-bold: 700;
```

### Spacing

```css
/* Spacing Scale */
--space-1: 0.25rem;   /* 4px */
--space-2: 0.5rem;    /* 8px */
--space-3: 0.75rem;   /* 12px */
--space-4: 1rem;      /* 16px */
--space-5: 1.25rem;   /* 20px */
--space-6: 1.5rem;    /* 24px */
--space-8: 2rem;      /* 32px */
--space-10: 2.5rem;   /* 40px */
--space-12: 3rem;     /* 48px */
```

---

## 🏗️ Widget Architecture

### Base Widget Component

```jsx
// components/widgets/BaseWidget.jsx
import React from 'react';
import './BaseWidget.css';

const BaseWidget = ({ 
  title, 
  icon, 
  children, 
  className = '',
  headerAction = null 
}) => {
  return (
    <div className={`widget ${className}`}>
      <div className="widget-header">
        <div className="widget-title">
          {icon && <span className="widget-icon">{icon}</span>}
          <h3>{title}</h3>
        </div>
        {headerAction && (
          <div className="widget-action">
            {headerAction}
          </div>
        )}
      </div>
      <div className="widget-body">
        {children}
      </div>
    </div>
  );
};

export default BaseWidget;
```

```css
/* BaseWidget.css */
.widget {
  background: var(--bg-primary);
  border-radius: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);
  padding: var(--space-6);
  transition: all 0.3s ease;
}

.widget:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06);
  transform: translateY(-2px);
}

.widget-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-4);
}

.widget-title {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.widget-title h3 {
  font-size: var(--text-lg);
  font-weight: var(--font-semibold);
  color: var(--gray-800);
  margin: 0;
}

.widget-icon {
  font-size: var(--text-xl);
}

.widget-body {
  color: var(--gray-700);
}
```

---

## 💰 Widget 1: Disposable Amount Hero Card

### Design Specifications

**Dimensions:** Full-width, minimum height 200px  
**Animation:** Smooth number counting on load  
**Responsive:** Stacks vertically on mobile

### Complete Implementation

```jsx
// components/widgets/DisposableAmountWidget.jsx
import React, { useEffect, useState } from 'react';
import BaseWidget from './BaseWidget';
import './DisposableAmountWidget.css';

const DisposableAmountWidget = ({ 
  amount, 
  percentage, 
  trend,
  previousAmount 
}) => {
  const [animatedAmount, setAnimatedAmount] = useState(0);

  useEffect(() => {
    // Animate number counting
    const duration = 1500;
    const steps = 60;
    const increment = amount / steps;
    let current = 0;
    let step = 0;

    const timer = setInterval(() => {
      step++;
      current = Math.min(amount, current + increment);
      setAnimatedAmount(current);

      if (step >= steps) {
        setAnimatedAmount(amount);
        clearInterval(timer);
      }
    }, duration / steps);

    return () => clearInterval(timer);
  }, [amount]);

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-PH', {
      style: 'currency',
      currency: 'PHP',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  };

  const getStatusClass = () => {
    if (percentage > 20) return 'healthy';
    if (percentage >= 10) return 'warning';
    return 'critical';
  };

  const getStatusText = () => {
    if (percentage > 20) return '✅ Healthy';
    if (percentage >= 10) return '⚠️ Monitor';
    return '🚨 Critical';
  };

  const getTrendData = () => {
    if (!previousAmount) return null;
    const change = amount - previousAmount;
    const changePercent = ((change / previousAmount) * 100).toFixed(1);
    return {
      direction: change > 0 ? 'up' : 'down',
      change: Math.abs(change),
      percent: Math.abs(changePercent)
    };
  };

  const trendData = getTrendData();

  return (
    <div className={`disposable-widget status-${getStatusClass()}`}>
      <div className="disposable-header">
        <div className="header-left">
          <span className="icon">💰</span>
          <h2>Disposable Amount</h2>
        </div>
        {trendData && (
          <div className={`trend-badge trend-${trendData.direction}`}>
            <span className="trend-arrow">
              {trendData.direction === 'up' ? '↑' : '↓'}
            </span>
            <span className="trend-value">
              {trendData.percent}%
            </span>
          </div>
        )}
      </div>

      <div className="disposable-body">
        <div className="amount-display">
          {formatCurrency(animatedAmount)}
        </div>
        
        <div className="percentage-bar">
          <div className="bar-background">
            <div 
              className={`bar-fill fill-${getStatusClass()}`}
              style={{ width: `${Math.min(percentage, 100)}%` }}
            />
          </div>
          <span className="percentage-label">
            {percentage.toFixed(1)}% of income
          </span>
        </div>

        <div className="disposable-footer">
          <div className={`status-badge badge-${getStatusClass()}`}>
            {getStatusText()}
          </div>
          {trendData && (
            <div className="change-amount">
              {trendData.direction === 'up' ? '+' : '-'}
              {formatCurrency(trendData.change)} vs last month
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default DisposableAmountWidget;
```

```css
/* DisposableAmountWidget.css */
.disposable-widget {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 20px;
  padding: var(--space-8);
  color: white;
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.3);
  transition: all 0.3s ease;
  margin-bottom: var(--space-6);
}

.disposable-widget:hover {
  transform: translateY(-4px);
  box-shadow: 0 15px 35px rgba(102, 126, 234, 0.4);
}

.disposable-widget.status-healthy {
  background: linear-gradient(135deg, #22c55e 0%, #10b981 100%);
}

.disposable-widget.status-warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.disposable-widget.status-critical {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
}

.disposable-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-6);
}

.header-left {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.header-left .icon {
  font-size: 2rem;
}

.header-left h2 {
  margin: 0;
  font-size: var(--text-xl);
  font-weight: var(--font-semibold);
}

.trend-badge {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-2) var(--space-4);
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  font-weight: var(--font-semibold);
}

.trend-arrow {
  font-size: var(--text-lg);
}

.amount-display {
  font-size: 3.5rem;
  font-weight: var(--font-bold);
  margin: var(--space-4) 0;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.percentage-bar {
  margin: var(--space-4) 0;
}

.bar-background {
  height: 12px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 6px;
  overflow: hidden;
  margin-bottom: var(--space-2);
}

.bar-fill {
  height: 100%;
  background: rgba(255, 255, 255, 0.9);
  border-radius: 6px;
  transition: width 1s ease-out;
}

.percentage-label {
  font-size: var(--text-sm);
  opacity: 0.9;
}

.disposable-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: var(--space-6);
  padding-top: var(--space-6);
  border-top: 1px solid rgba(255, 255, 255, 0.2);
}

.status-badge {
  padding: var(--space-2) var(--space-4);
  border-radius: 12px;
  font-size: var(--text-sm);
  font-weight: var(--font-semibold);
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
}

.change-amount {
  font-size: var(--text-sm);
  opacity: 0.9;
}

/* Mobile Responsive */
@media (max-width: 768px) {
  .disposable-widget {
    padding: var(--space-6);
  }

  .amount-display {
    font-size: 2.5rem;
  }

  .disposable-footer {
    flex-direction: column;
    gap: var(--space-3);
    align-items: flex-start;
  }
}
```

---

## 📊 Widget 2: Income vs Expenses Chart

### Bar Chart Implementation

```jsx
// components/widgets/IncomeVsExpensesWidget.jsx
import React from 'react';
import { Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import BaseWidget from './BaseWidget';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

const IncomeVsExpensesWidget = ({ 
  income, 
  fixedExpenses, 
  variableExpenses, 
  disposable 
}) => {
  const data = {
    labels: ['Financial Overview'],
    datasets: [
      {
        label: 'Income',
        data: [income],
        backgroundColor: 'rgba(34, 197, 94, 0.8)',
        borderColor: 'rgba(34, 197, 94, 1)',
        borderWidth: 2,
      },
      {
        label: 'Fixed Expenses',
        data: [fixedExpenses],
        backgroundColor: 'rgba(239, 68, 68, 0.8)',
        borderColor: 'rgba(239, 68, 68, 1)',
        borderWidth: 2,
      },
      {
        label: 'Variable Expenses',
        data: [variableExpenses],
        backgroundColor: 'rgba(245, 158, 11, 0.8)',
        borderColor: 'rgba(245, 158, 11, 1)',
        borderWidth: 2,
      },
      {
        label: 'Disposable',
        data: [disposable],
        backgroundColor: 'rgba(59, 130, 246, 0.8)',
        borderColor: 'rgba(59, 130, 246, 1)',
        borderWidth: 2,
      }
    ]
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          usePointStyle: true,
          padding: 15,
          font: {
            size: 12
          }
        }
      },
      tooltip: {
        callbacks: {
          label: function(context) {
            return `${context.dataset.label}: ₱${context.parsed.y.toLocaleString()}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: function(value) {
            return '₱' + value.toLocaleString();
          }
        }
      }
    }
  };

  return (
    <BaseWidget 
      title="Income vs Expenses" 
      icon="📊"
      className="income-vs-expenses-widget"
    >
      <div style={{ height: '300px' }}>
        <Bar data={data} options={options} />
      </div>
      
      <div className="summary-stats">
        <div className="stat-item">
          <span className="stat-label">Total Expenses:</span>
          <span className="stat-value">
            ₱{(fixedExpenses + variableExpenses).toLocaleString()}
          </span>
        </div>
        <div className="stat-item">
          <span className="stat-label">Expense Ratio:</span>
          <span className="stat-value">
            {((fixedExpenses + variableExpenses) / income * 100).toFixed(1)}%
          </span>
        </div>
      </div>
    </BaseWidget>
  );
};

export default IncomeVsExpensesWidget;
```

---

## 🥧 Widget 3: Expense Breakdown Pie Chart

```jsx
// components/widgets/ExpenseBreakdownWidget.jsx
import React from 'react';
import { Doughnut } from 'react-chartjs-2';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
import BaseWidget from './BaseWidget';
import './ExpenseBreakdownWidget.css';

ChartJS.register(ArcElement, Tooltip, Legend);

const ExpenseBreakdownWidget = ({ expenses }) => {
  const colors = [
    '#3b82f6', // blue
    '#22c55e', // green
    '#f59e0b', // amber
    '#ef4444', // red
    '#8b5cf6', // purple
    '#ec4899', // pink
    '#14b8a6', // teal
    '#f97316', // orange
  ];

  const data = {
    labels: expenses.map(e => e.category),
    datasets: [{
      data: expenses.map(e => e.totalAmount),
      backgroundColor: colors,
      borderWidth: 3,
      borderColor: '#fff',
      hoverBorderWidth: 4,
    }]
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'right',
        labels: {
          usePointStyle: true,
          padding: 15,
          generateLabels: (chart) => {
            const data = chart.data;
            return data.labels.map((label, i) => ({
              text: `${label} (${expenses[i].percentage.toFixed(1)}%)`,
              fillStyle: colors[i],
              hidden: false,
              index: i
            }));
          }
        }
      },
      tooltip: {
        callbacks: {
          label: function(context) {
            const expense = expenses[context.dataIndex];
            return [
              `${context.label}`,
              `Amount: ₱${expense.totalAmount.toLocaleString()}`,
              `Count: ${expense.count} transactions`,
              `Percentage: ${expense.percentage.toFixed(1)}%`
            ];
          }
        }
      }
    }
  };

  return (
    <BaseWidget 
      title="Expense Breakdown" 
      icon="🥧"
      className="expense-breakdown-widget"
    >
      <div style={{ height: '300px' }}>
        <Doughnut data={data} options={options} />
      </div>
      
      <div className="breakdown-details">
        {expenses.slice(0, 3).map((expense, index) => (
          <div key={index} className="detail-item">
            <div className="detail-header">
              <span className="category-dot" style={{ backgroundColor: colors[index] }} />
              <span className="category-name">{expense.category}</span>
            </div>
            <div className="detail-values">
              <span className="amount">₱{expense.totalAmount.toLocaleString()}</span>
              <span className="percentage">{expense.percentage.toFixed(1)}%</span>
            </div>
          </div>
        ))}
      </div>
    </BaseWidget>
  );
};

export default ExpenseBreakdownWidget;
```

```css
/* ExpenseBreakdownWidget.css */
.expense-breakdown-widget {
  min-height: 450px;
}

.breakdown-details {
  margin-top: var(--space-6);
  padding-top: var(--space-6);
  border-top: 1px solid var(--gray-200);
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-3) 0;
}

.detail-header {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.category-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.category-name {
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  color: var(--gray-700);
}

.detail-values {
  display: flex;
  gap: var(--space-4);
  align-items: center;
}

.amount {
  font-size: var(--text-base);
  font-weight: var(--font-semibold);
  color: var(--gray-800);
}

.percentage {
  font-size: var(--text-sm);
  color: var(--gray-500);
  background: var(--gray-100);
  padding: var(--space-1) var(--space-3);
  border-radius: 12px;
}
```

---

## 📈 Widget 4: Monthly Trend Line Chart

```jsx
// components/widgets/MonthlyTrendWidget.jsx
import React from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';
import BaseWidget from './BaseWidget';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

const MonthlyTrendWidget = ({ monthlyData }) => {
  const data = {
    labels: monthlyData.map(m => m.monthName.substring(0, 3)),
    datasets: [
      {
        label: 'Income',
        data: monthlyData.map(m => m.income),
        borderColor: 'rgba(34, 197, 94, 1)',
        backgroundColor: 'rgba(34, 197, 94, 0.1)',
        tension: 0.4,
        fill: true,
      },
      {
        label: 'Expenses',
        data: monthlyData.map(m => m.expenses),
        borderColor: 'rgba(239, 68, 68, 1)',
        backgroundColor: 'rgba(239, 68, 68, 0.1)',
        tension: 0.4,
        fill: true,
      },
      {
        label: 'Disposable',
        data: monthlyData.map(m => m.disposable),
        borderColor: 'rgba(59, 130, 246, 1)',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        tension: 0.4,
        fill: true,
        borderWidth: 3,
      }
    ]
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
      mode: 'index',
      intersect: false,
    },
    plugins: {
      legend: {
        position: 'top',
      },
      tooltip: {
        callbacks: {
          label: function(context) {
            return `${context.dataset.label}: ₱${context.parsed.y.toLocaleString()}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: function(value) {
            return '₱' + (value / 1000) + 'k';
          }
        }
      }
    }
  };

  return (
    <BaseWidget 
      title="6-Month Trend" 
      icon="📈"
      className="monthly-trend-widget"
    >
      <div style={{ height: '300px' }}>
        <Line data={data} options={options} />
      </div>
    </BaseWidget>
  );
};

export default MonthlyTrendWidget;
```

---

## 💡 Widget 5: Insights Panel

```jsx
// components/widgets/InsightsWidget.jsx
import React from 'react';
import BaseWidget from './BaseWidget';
import './InsightsWidget.css';

const InsightsWidget = ({ insights }) => {
  const getInsightIcon = (insight) => {
    if (insight.includes('increased')) return '📈';
    if (insight.includes('decreased')) return '📉';
    if (insight.includes('spending')) return '💳';
    if (insight.includes('saving') || insight.includes('save')) return '💰';
    if (insight.includes('highest')) return '⚠️';
    return '💡';
  };

  const getInsightType = (insight) => {
    if (insight.includes('increased')) return 'positive';
    if (insight.includes('decreased') || insight.includes('high')) return 'warning';
    if (insight.includes('save') || insight.includes('Consider')) return 'info';
    return 'neutral';
  };

  return (
    <BaseWidget 
      title="Smart Insights" 
      icon="💡"
      className="insights-widget"
    >
      <div className="insights-list">
        {insights.map((insight, index) => (
          <div key={index} className={`insight-item type-${getInsightType(insight)}`}>
            <div className="insight-icon">
              {getInsightIcon(insight)}
            </div>
            <div className="insight-text">
              {insight}
            </div>
          </div>
        ))}
      </div>
    </BaseWidget>
  );
};

export default InsightsWidget;
```

```css
/* InsightsWidget.css */
.insights-widget {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.insights-widget .widget-title h3 {
  color: white;
}

.insights-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.insight-item {
  display: flex;
  gap: var(--space-3);
  padding: var(--space-4);
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: 12px;
  border-left: 4px solid rgba(255, 255, 255, 0.3);
  transition: all 0.3s ease;
}

.insight-item:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: translateX(4px);
}

.insight-item.type-positive {
  border-left-color: rgba(34, 197, 94, 0.8);
}

.insight-item.type-warning {
  border-left-color: rgba(245, 158, 11, 0.8);
}

.insight-item.type-info {
  border-left-color: rgba(59, 130, 246, 0.8);
}

.insight-icon {
  font-size: 1.5rem;
  flex-shrink: 0;
}

.insight-text {
  font-size: var(--text-sm);
  line-height: 1.6;
}
```

---

## 📊 Widget 6: Quick Stats Grid

```jsx
// components/widgets/QuickStatsWidget.jsx
import React from 'react';
import './QuickStatsWidget.css';

const QuickStatsWidget = ({ stats }) => {
  const statItems = [
    {
      label: 'Avg Monthly Income',
      value: stats.averageMonthlyIncome,
      icon: '💵',
      color: '#22c55e',
      format: 'currency'
    },
    {
      label: 'Avg Expenses',
      value: stats.averageMonthlyExpenses,
      icon: '💳',
      color: '#ef4444',
      format: 'currency'
    },
    {
      label: 'Avg Disposable',
      value: stats.averageDisposable,
      icon: '💰',
      color: '#3b82f6',
      format: 'currency'
    },
    {
      label: 'Active Loans',
      value: stats.activeLoans,
      icon: '📊',
      color: '#f59e0b',
      format: 'number'
    },
    {
      label: 'Pending Bills',
      value: stats.pendingBills,
      icon: '📄',
      color: '#8b5cf6',
      format: 'number'
    },
    {
      label: 'Top Category',
      value: stats.topExpenseCategory,
      icon: '🎯',
      color: '#ec4899',
      format: 'text'
    }
  ];

  const formatValue = (value, format) => {
    if (format === 'currency') {
      return '₱' + Math.round(value).toLocaleString();
    }
    if (format === 'number') {
      return value;
    }
    return value;
  };

  return (
    <div className="quick-stats-grid">
      {statItems.map((stat, index) => (
        <div key={index} className="stat-card" style={{ '--card-color': stat.color }}>
          <div className="stat-icon">{stat.icon}</div>
          <div className="stat-content">
            <div className="stat-label">{stat.label}</div>
            <div className="stat-value">{formatValue(stat.value, stat.format)}</div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default QuickStatsWidget;
```

```css
/* QuickStatsWidget.css */
.quick-stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-4);
  margin: var(--space-6) 0;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: var(--space-5);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: var(--space-4);
  border-left: 4px solid var(--card-color);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  font-size: 2rem;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--card-color), var(--card-color));
  opacity: 0.1;
  border-radius: 12px;
}

.stat-content {
  flex: 1;
}

.stat-label {
  font-size: var(--text-xs);
  color: var(--gray-500);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: var(--space-1);
}

.stat-value {
  font-size: var(--text-2xl);
  font-weight: var(--font-bold);
  color: var(--gray-800);
}

@media (max-width: 768px) {
  .quick-stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 480px) {
  .quick-stats-grid {
    grid-template-columns: 1fr;
  }
}
```

---

## 📱 Complete Dashboard Layout

```jsx
// pages/Dashboard.jsx
import React, { useState, useEffect } from 'react';
import { dashboardService } from '../api/dashboardService';
import DisposableAmountWidget from '../components/widgets/DisposableAmountWidget';
import IncomeVsExpensesWidget from '../components/widgets/IncomeVsExpensesWidget';
import ExpenseBreakdownWidget from '../components/widgets/ExpenseBreakdownWidget';
import MonthlyTrendWidget from '../components/widgets/MonthlyTrendWidget';
import InsightsWidget from '../components/widgets/InsightsWidget';
import QuickStatsWidget from '../components/widgets/QuickStatsWidget';
import './Dashboard.css';

const Dashboard = () => {
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState(null);
  const [disposableData, setDisposableData] = useState(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const [summary, disposable] = await Promise.all([
        dashboardService.getFinancialSummary(),
        dashboardService.getCurrentDisposableAmount(5000, 3000)
      ]);
      setData(summary);
      setDisposableData(disposable);
    } catch (error) {
      console.error('Failed to load dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="dashboard-loading">Loading dashboard...</div>;
  }

  if (!data || !disposableData) {
    return <div className="dashboard-error">Failed to load dashboard</div>;
  }

  return (
    <div className="dashboard-container">
      <div className="dashboard-header">
        <h1>Financial Dashboard</h1>
        <button onClick={loadDashboard} className="refresh-btn">
          🔄 Refresh
        </button>
      </div>

      {/* Hero Section */}
      <DisposableAmountWidget
        amount={disposableData.disposableAmount}
        percentage={disposableData.disposablePercentage}
        trend={disposableData.comparison.trend.toLowerCase()}
        previousAmount={disposableData.comparison.previousPeriodDisposableAmount}
      />

      {/* Quick Stats */}
      <QuickStatsWidget stats={data.stats} />

      {/* Charts Grid */}
      <div className="dashboard-grid">
        <div className="grid-item span-2">
          <IncomeVsExpensesWidget
            income={data.currentMonth.totalIncome}
            fixedExpenses={data.currentMonth.fixedExpenses}
            variableExpenses={data.currentMonth.variableExpenses}
            disposable={data.currentMonth.disposableAmount}
          />
        </div>

        <div className="grid-item">
          <ExpenseBreakdownWidget expenses={disposableData.variableExpensesBreakdown} />
        </div>

        <div className="grid-item span-3">
          <MonthlyTrendWidget monthlyData={data.yearToDate.monthlyBreakdown} />
        </div>

        <div className="grid-item span-3">
          <InsightsWidget insights={disposableData.insights} />
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
```

```css
/* Dashboard.css */
.dashboard-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: var(--space-6);
  background: var(--bg-secondary);
  min-height: 100vh;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-8);
}

.dashboard-header h1 {
  font-size: var(--text-3xl);
  font-weight: var(--font-bold);
  color: var(--gray-800);
  margin: 0;
}

.refresh-btn {
  padding: var(--space-3) var(--space-6);
  background: white;
  border: 1px solid var(--gray-200);
  border-radius: 8px;
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  cursor: pointer;
  transition: all 0.2s ease;
}

.refresh-btn:hover {
  background: var(--gray-50);
  border-color: var(--gray-300);
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-6);
}

.grid-item {
  grid-column: span 1;
}

.grid-item.span-2 {
  grid-column: span 2;
}

.grid-item.span-3 {
  grid-column: span 3;
}

/* Responsive Design */
@media (max-width: 1200px) {
  .dashboard-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .grid-item.span-2,
  .grid-item.span-3 {
    grid-column: span 2;
  }
}

@media (max-width: 768px) {
  .dashboard-container {
    padding: var(--space-4);
  }

  .dashboard-grid {
    grid-template-columns: 1fr;
  }
  
  .grid-item,
  .grid-item.span-2,
  .grid-item.span-3 {
    grid-column: span 1;
  }
}

.dashboard-loading,
.dashboard-error {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  font-size: var(--text-lg);
  color: var(--gray-600);
}
```

---

## ✨ Animation Effects

### Fade In Animation

```css
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.widget {
  animation: fadeIn 0.5s ease-out;
}

.grid-item:nth-child(1) { animation-delay: 0.1s; }
.grid-item:nth-child(2) { animation-delay: 0.2s; }
.grid-item:nth-child(3) { animation-delay: 0.3s; }
```

### Number Counter Animation

```javascript
const animateNumber = (element, start, end, duration) => {
  const range = end - start;
  const increment = range / (duration / 16);
  let current = start;

  const timer = setInterval(() => {
    current += increment;
    if ((increment > 0 && current >= end) || (increment < 0 && current <= end)) {
      current = end;
      clearInterval(timer);
    }
    element.textContent = Math.floor(current).toLocaleString();
  }, 16);
};
```

---

## 📚 Complete Widget Library

All widgets are now ready to use! Simply import and pass the required props:

```jsx
import DisposableAmountWidget from './components/widgets/DisposableAmountWidget';
import IncomeVsExpensesWidget from './components/widgets/IncomeVsExpensesWidget';
import ExpenseBreakdownWidget from './components/widgets/ExpenseBreakdownWidget';
import MonthlyTrendWidget from './components/widgets/MonthlyTrendWidget';
import InsightsWidget from './components/widgets/InsightsWidget';
import QuickStatsWidget from './components/widgets/QuickStatsWidget';
```

---

**Guide Version:** 1.0.0  
**Last Updated:** October 11, 2025  
**Status:** Production Ready ✅

