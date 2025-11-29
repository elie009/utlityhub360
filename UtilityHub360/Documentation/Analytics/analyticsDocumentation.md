# Financial Analytics & Reports Documentation

## Overview

The Financial Analytics & Reports system in UtilityHub360 provides comprehensive financial insights and trend analysis. This system enables frontend developers to build powerful financial dashboards, charts, and reporting interfaces.

**⚠️ Note:** This documentation only includes **fully implemented and working** endpoints. Some features are still in development.

## Table of Contents

1. [Quick Start Guide](#quick-start-guide)
2. [Authentication](#authentication)
3. [API Endpoints](#api-endpoints)
4. [Report Types](#report-types)
5. [Data Transfer Objects (DTOs)](#data-transfer-objects-dtos)
6. [Request/Response Examples](#requestresponse-examples)
7. [Frontend Integration Examples](#frontend-integration-examples)
8. [Chart Integration](#chart-integration)
9. [Error Handling](#error-handling)
10. [Best Practices](#best-practices)

## Quick Start Guide

### Base URLs
```
/api/Reports  (for Financial Reports)
/api/Analytics  (for Analytics endpoints)
```

### Authentication
All endpoints require JWT authentication via the `Authorization` header:
```
Authorization: Bearer <jwt_token>
```

### Common Use Cases

1. **Dashboard Summary**: Get quick financial overview ✅
2. **Full Report**: Complete financial analysis with all sections ✅
3. **Income Report**: Analyze income sources and trends ✅
4. **Expense Report**: Track spending patterns by category ✅
5. **Loan Report**: Analyze debt and repayment progress ✅
6. **Net Worth Report**: Overall financial health analysis ✅
7. **Monthly Cash Flow**: Get incoming and outgoing per month for a year ✅

## Authentication

All Analytics endpoints require authentication. The user ID is automatically extracted from the JWT token.

**Header Required:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## API Endpoints

### 1. Get Financial Summary (Dashboard) ✅

**Endpoint:** `GET /api/Reports/summary`

**Description:** Get a quick financial summary for dashboard widgets. Perfect for displaying key metrics.

**Query Parameters:**
- `date` (optional): Specific date for summary. Defaults to current month.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalIncome": 50000.00,
    "incomeChange": 10.5,
    "totalExpenses": 35000.00,
    "expenseChange": -5.2,
    "disposableIncome": 15000.00,
    "disposableIncomeChange": 25.0,
    "netWorth": 245000.00,
    "netWorthChange": 5.1,
    "savingsProgress": 50.0,
    "savingsGoal": 50000.00,
    "totalSavings": 25000.00
  },
  "errors": []
}
```

**Fields:**
- `totalIncome`: Total monthly income from all active income sources
- `incomeChange`: Percentage change from previous month
- `totalExpenses`: Total expenses (bills + variable expenses)
- `expenseChange`: Percentage change from previous month
- `disposableIncome`: Money left after expenses (income - expenses)
- `disposableIncomeChange`: Percentage change in disposable income
- `netWorth`: Total assets minus liabilities
- `netWorthChange`: Net worth change percentage
- `savingsProgress`: Percentage progress toward savings goal
- `savingsGoal`: Target savings amount from user profile
- `totalSavings`: Current total savings

**Use Case:** Dashboard summary cards, overview widgets

---

### 2. Get Full Financial Report ✅

**Endpoint:** `GET /api/Reports/full`

**Description:** Get complete financial report with all available sections (income, expenses, loans, net worth).

**Query Parameters:**
- `period` (optional): `MONTHLY`, `QUARTERLY`, `YEARLY`, or `CUSTOM` (default: `MONTHLY`)
- `startDate` (optional): Start date for custom period (ISO 8601)
- `endDate` (optional): End date for custom period (ISO 8601)
- `includeInsights` (optional): Include AI insights (currently not implemented)
- `includePredictions` (optional): Include predictions (currently not implemented)
- `includeTransactions` (optional): Include recent transactions (currently not implemented)

**Example Requests:**
```
GET /api/Reports/full?period=MONTHLY
GET /api/Reports/full?period=QUARTERLY
GET /api/Reports/full?period=CUSTOM&startDate=2025-01-01&endDate=2025-06-30
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Financial report generated successfully",
  "data": {
    "reportDate": "2025-10-29T12:00:00Z",
    "period": "MONTHLY",
    "summary": {
      "totalIncome": 50000.00,
      "incomeChange": 10.5,
      "totalExpenses": 35000.00,
      "expenseChange": -5.2,
      "disposableIncome": 15000.00,
      "disposableIncomeChange": 25.0,
      "netWorth": 245000.00,
      "netWorthChange": 5.1,
      "savingsProgress": 50.0,
      "savingsGoal": 50000.00,
      "totalSavings": 25000.00
    },
    "incomeReport": {
      "totalIncome": 50000.00,
      "monthlyAverage": 47500.00,
      "growthRate": 12.5,
      "incomeBySource": {
        "Salary": 45000.00,
        "Freelance": 5000.00
      },
      "incomeByCategory": {
        "PRIMARY": 45000.00,
        "SIDE_HUSTLE": 5000.00
      },
      "incomeTrend": [],
      "topIncomeSource": "Salary",
      "topIncomeAmount": 45000.00
    },
    "expenseReport": {
      "totalExpenses": 35000.00,
      "fixedExpenses": 8000.00,
      "variableExpenses": 27000.00,
      "expenseByCategory": {
        "Bills & Utilities": 8000.00,
        "GROCERIES": 6000.00,
        "TRANSPORTATION": 4000.00,
        "ENTERTAINMENT": 3000.00
      },
      "expensePercentage": {
        "Bills & Utilities": 22.86,
        "GROCERIES": 17.14,
        "TRANSPORTATION": 11.43,
        "ENTERTAINMENT": 8.57
      },
      "expenseTrend": [],
      "highestExpenseCategory": "Bills & Utilities",
      "highestExpenseAmount": 8000.00,
      "highestExpensePercentage": 22.86,
      "averageMonthlyExpense": 35000.00,
      "categoryComparison": []
    },
    "disposableIncomeReport": null,
    "billsReport": null,
    "loanReport": {
      "activeLoansCount": 2,
      "totalPrincipal": 50000.00,
      "totalRemainingBalance": 27500.00,
      "totalMonthlyPayment": 2000.00,
      "totalInterestPaid": 2500.00,
      "activeLoans": [
        {
          "loanId": "loan-id-123",
          "purpose": "Home Improvement",
          "principal": 25000.00,
          "remainingBalance": 13750.00,
          "monthlyPayment": 1000.00,
          "interestRate": 5.5,
          "repaymentProgress": 45
        }
      ],
      "repaymentTrend": [
        {
          "date": "2025-09-01T00:00:00Z",
          "label": "Sep 2025",
          "value": 2000.00
        },
        {
          "date": "2025-10-01T00:00:00Z",
          "label": "Oct 2025",
          "value": 2000.00
        }
      ],
      "projectedDebtFreeDate": "2028-06-15T00:00:00Z",
      "monthsUntilDebtFree": 14
    },
    "savingsReport": null,
    "netWorthReport": {
      "currentNetWorth": 245000.00,
      "netWorthChange": 5000.00,
      "netWorthChangePercentage": 5.1,
      "totalAssets": 295000.00,
      "totalLiabilities": 50000.00,
      "assetBreakdown": {
        "Bank Accounts": 270000.00,
        "Savings Accounts": 25000.00
      },
      "liabilityBreakdown": {
        "Home Improvement": 13750.00,
        "Car Loan": 13750.00
      },
      "netWorthTrend": [
        {
          "date": "2025-09-30T00:00:00Z",
          "label": "Sep 2025",
          "value": 240000.00
        },
        {
          "date": "2025-10-30T00:00:00Z",
          "label": "Oct 2025",
          "value": 245000.00
        }
      ],
      "trendDescription": "Great progress! Your net worth increased by 5.1% ($5,000). You're building wealth."
    },
    "insights": [],
    "predictions": [],
    "recentTransactions": []
  },
  "errors": []
}
```

**Note:** Some sections (`disposableIncomeReport`, `billsReport`, `savingsReport`) may be `null` as they are not yet implemented. `insights`, `predictions`, and `recentTransactions` will be empty arrays unless those features are implemented.

---

### 3. Get Income Report ✅

**Endpoint:** `GET /api/Reports/income`

**Description:** Detailed income analysis with trends and source breakdown.

**Query Parameters:**
- `period` (optional): `MONTHLY`, `QUARTERLY`, `YEARLY`, or `CUSTOM` (default: `MONTHLY`)
- `startDate` (optional): Start date for custom period
- `endDate` (optional): End date for custom period

**Example Requests:**
```
GET /api/Reports/income?period=MONTHLY
GET /api/Reports/income?period=QUARTERLY
GET /api/Reports/income?period=CUSTOM&startDate=2025-01-01&endDate=2025-06-30
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalIncome": 50000.00,
    "monthlyAverage": 47500.00,
    "growthRate": 12.5,
    "incomeBySource": {
      "Salary": 45000.00,
      "Freelance Web Development": 5000.00
    },
    "incomeByCategory": {
      "PRIMARY": 45000.00,
      "SIDE_HUSTLE": 5000.00
    },
    "incomeTrend": [],
    "topIncomeSource": "Salary",
    "topIncomeAmount": 45000.00
  },
  "errors": []
}
```

**Fields:**
- `totalIncome`: Total monthly income (all active income sources converted to monthly)
- `monthlyAverage`: Average monthly income
- `growthRate`: Growth percentage
- `incomeBySource`: Dictionary of income source names and amounts
- `incomeByCategory`: Dictionary of income categories (PRIMARY, SIDE_HUSTLE, etc.) and amounts
- `incomeTrend`: Array of trend data points (currently empty, will be populated with historical data)
- `topIncomeSource`: Name of the highest income source
- `topIncomeAmount`: Amount of the top income source

---

### 4. Get Expense Report ✅

**Endpoint:** `GET /api/Reports/expenses`

**Description:** Detailed expense analysis with category breakdown and trends.

**Query Parameters:**
- `period` (optional): `MONTHLY`, `QUARTERLY`, `YEARLY`, or `CUSTOM` (default: `MONTHLY`)
- `startDate` (optional): Start date for custom period
- `endDate` (optional): End date for custom period

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalExpenses": 35000.00,
    "fixedExpenses": 8000.00,
    "variableExpenses": 27000.00,
    "expenseByCategory": {
      "Bills & Utilities": 8000.00,
      "GROCERIES": 6000.00,
      "TRANSPORTATION": 4000.00,
      "ENTERTAINMENT": 3000.00,
      "HEALTHCARE": 2500.00,
      "OTHER": 11500.00
    },
    "expensePercentage": {
      "Bills & Utilities": 22.86,
      "GROCERIES": 17.14,
      "TRANSPORTATION": 11.43,
      "ENTERTAINMENT": 8.57,
      "HEALTHCARE": 7.14,
      "OTHER": 32.86
    },
    "expenseTrend": [],
    "highestExpenseCategory": "OTHER",
    "highestExpenseAmount": 11500.00,
    "highestExpensePercentage": 32.86,
    "averageMonthlyExpense": 35000.00,
    "categoryComparison": [
      {
        "category": "Bills & Utilities",
        "currentAmount": 8000.00,
        "previousAmount": 7500.00,
        "change": 500.00,
        "changePercentage": 6.67
      }
    ]
  },
  "errors": []
}
```

**Fields:**
- `totalExpenses`: Total expenses (bills + variable expenses)
- `fixedExpenses`: Recurring bills amount
- `variableExpenses`: Variable expenses amount
- `expenseByCategory`: Dictionary of category names and amounts
- `expensePercentage`: Dictionary of category percentages
- `expenseTrend`: Array of trend data points
- `highestExpenseCategory`: Category with highest spending
- `highestExpenseAmount`: Amount of highest category
- `highestExpensePercentage`: Percentage of highest category
- `averageMonthlyExpense`: Average monthly expenses
- `categoryComparison`: Comparison with previous period

---

### 5. Get Loan Report ✅

**Endpoint:** `GET /api/Reports/loans`

**Description:** Analysis of loans and debt progress.

**Query Parameters:**
- `period` (optional): `MONTHLY`, `QUARTERLY`, `YEARLY`, or `CUSTOM` (default: `MONTHLY`)
- `startDate` (optional): Start date for custom period
- `endDate` (optional): End date for custom period

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "activeLoansCount": 2,
    "totalPrincipal": 50000.00,
    "totalRemainingBalance": 27500.00,
    "totalMonthlyPayment": 2000.00,
    "totalInterestPaid": 2500.00,
    "activeLoans": [
      {
        "loanId": "loan-id-123",
        "purpose": "Home Improvement",
        "principal": 25000.00,
        "remainingBalance": 13750.00,
        "monthlyPayment": 1000.00,
        "interestRate": 5.5,
        "repaymentProgress": 45
      },
      {
        "loanId": "loan-id-456",
        "purpose": "Car Loan",
        "principal": 25000.00,
        "remainingBalance": 13750.00,
        "monthlyPayment": 1000.00,
        "interestRate": 4.5,
        "repaymentProgress": 45
      }
    ],
    "repaymentTrend": [
      {
        "date": "2025-09-01T00:00:00Z",
        "label": "Sep 2025",
        "value": 2000.00
      },
      {
        "date": "2025-10-01T00:00:00Z",
        "label": "Oct 2025",
        "value": 2000.00
      }
    ],
    "projectedDebtFreeDate": "2028-06-15T00:00:00Z",
    "monthsUntilDebtFree": 14
  },
  "errors": []
}
```

**Fields:**
- `activeLoansCount`: Number of active loans (excluding REJECTED and COMPLETED)
- `totalPrincipal`: Total principal amount of all loans
- `totalRemainingBalance`: Total remaining balance to pay
- `totalMonthlyPayment`: Sum of all monthly payments
- `totalInterestPaid`: Total interest paid across all loans
- `activeLoans`: Array of loan details with repayment progress
- `repaymentTrend`: Monthly repayment amounts over time
- `projectedDebtFreeDate`: Estimated date when all loans will be paid off
- `monthsUntilDebtFree`: Number of months until debt-free

---

### 6. Get Net Worth Report ✅

**Endpoint:** `GET /api/Reports/networth`

**Description:** Overall financial health analysis (assets vs liabilities).

**Query Parameters:**
- `period` (optional): `MONTHLY`, `QUARTERLY`, `YEARLY`, or `CUSTOM` (default: `MONTHLY`)
- `startDate` (optional): Start date for custom period
- `endDate` (optional): End date for custom period

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "currentNetWorth": 245000.00,
    "netWorthChange": 5000.00,
    "netWorthChangePercentage": 5.1,
    "totalAssets": 295000.00,
    "totalLiabilities": 50000.00,
    "assetBreakdown": {
      "Bank Accounts": 270000.00,
      "Savings Accounts": 25000.00
    },
    "liabilityBreakdown": {
      "Home Improvement": 13750.00,
      "Car Loan": 13750.00,
      "Personal Loan": 22500.00
    },
    "netWorthTrend": [
      {
        "date": "2025-09-30T00:00:00Z",
        "label": "Sep 2025",
        "value": 240000.00
      },
      {
        "date": "2025-10-30T00:00:00Z",
        "label": "Oct 2025",
        "value": 245000.00
      }
    ],
    "trendDescription": "Great progress! Your net worth increased by 5.1% ($5,000). You're building wealth."
  },
  "errors": []
}
```

**Fields:**
- `currentNetWorth`: Total assets minus total liabilities
- `netWorthChange`: Absolute change from previous period
- `netWorthChangePercentage`: Percentage change from previous period
- `totalAssets`: Sum of bank account balances + savings
- `totalLiabilities`: Sum of remaining loan balances
- `assetBreakdown`: Dictionary of asset types and amounts
- `liabilityBreakdown`: Dictionary of loan purposes and remaining balances
- `netWorthTrend`: Net worth over time (monthly data points)
- `trendDescription`: Human-readable description of the trend

---

## Report Types

### Period Options

| Period | Description | Use Case |
|--------|-------------|----------|
| `MONTHLY` | Current month data | Most common, dashboard views |
| `QUARTERLY` | Last 3 months | Trend analysis |
| `YEARLY` | Last 12 months | Long-term analysis |
| `CUSTOM` | User-specified date range | Specific analysis periods |

### Available Report Categories ✅

1. **Financial Summary**: Quick overview for dashboard
2. **Income Report**: Income sources, trends, growth
3. **Expense Report**: Spending categories, patterns
4. **Loan Report**: Debt progress and repayment
5. **Net Worth Report**: Overall financial health

### Not Yet Implemented ⚠️

- Disposable Income Report
- Bills Report
- Savings Report
- Financial Insights
- Financial Predictions
- Recent Transactions
- Period Comparison
- PDF/CSV Export

---

## Data Transfer Objects (DTOs)

### ReportQueryDto

```typescript
interface ReportQueryDto {
  period?: "MONTHLY" | "QUARTERLY" | "YEARLY" | "CUSTOM";
  startDate?: string;  // ISO 8601 date string
  endDate?: string;    // ISO 8601 date string
  includeInsights?: boolean;
  includePredictions?: boolean;
  includeTransactions?: boolean;
}
```

### ReportFinancialSummaryDto

```typescript
interface ReportFinancialSummaryDto {
  totalIncome: number;
  incomeChange: number;  // Percentage
  totalExpenses: number;
  expenseChange: number;  // Percentage
  disposableIncome: number;
  disposableIncomeChange: number;  // Percentage
  netWorth: number;
  netWorthChange: number;  // Percentage
  savingsProgress: number;  // Percentage
  savingsGoal: number;
  totalSavings: number;
}
```

### IncomeReportDto

```typescript
interface IncomeReportDto {
  totalIncome: number;
  monthlyAverage: number;
  growthRate: number;
  incomeBySource: Record<string, number>;
  incomeByCategory: Record<string, number>;
  incomeTrend: TrendDataPoint[];
  topIncomeSource: string;
  topIncomeAmount: number;
}
```

### ExpenseReportDto

```typescript
interface ExpenseReportDto {
  totalExpenses: number;
  fixedExpenses: number;
  variableExpenses: number;
  expenseByCategory: Record<string, number>;
  expensePercentage: Record<string, number>;
  expenseTrend: TrendDataPoint[];
  highestExpenseCategory: string;
  highestExpenseAmount: number;
  highestExpensePercentage: number;
  averageMonthlyExpense: number;
  categoryComparison: ExpenseComparisonDto[];
}
```

### LoanReportDto

```typescript
interface LoanReportDto {
  activeLoansCount: number;
  totalPrincipal: number;
  totalRemainingBalance: number;
  totalMonthlyPayment: number;
  totalInterestPaid: number;
  activeLoans: LoanDetailDto[];
  repaymentTrend: TrendDataPoint[];
  projectedDebtFreeDate?: string;  // ISO 8601 date
  monthsUntilDebtFree: number;
}
```

### NetWorthReportDto

```typescript
interface NetWorthReportDto {
  currentNetWorth: number;
  netWorthChange: number;
  netWorthChangePercentage: number;
  totalAssets: number;
  totalLiabilities: number;
  assetBreakdown: Record<string, number>;
  liabilityBreakdown: Record<string, number>;
  netWorthTrend: TrendDataPoint[];
  trendDescription: string;
}
```

### TrendDataPoint

```typescript
interface TrendDataPoint {
  date: string;  // ISO 8601 date
  label: string;  // Display label (e.g., "Oct 2025")
  value: number;  // Amount
}
```

### LoanDetailDto

```typescript
interface LoanDetailDto {
  loanId: string;
  purpose: string;
  principal: number;
  remainingBalance: number;
  monthlyPayment: number;
  interestRate: number;
  repaymentProgress: number;  // Percentage (0-100)
}
```

### ExpenseComparisonDto

```typescript
interface ExpenseComparisonDto {
  category: string;
  currentAmount: number;
  previousAmount: number;
  change: number;
  changePercentage: number;
}
```

### MonthlyCashFlowDto

```typescript
interface MonthlyCashFlowDto {
  year: number;
  monthlyData: MonthlyDataDto[];
  totalIncoming: number;
  totalOutgoing: number;
  netCashFlow: number;
}

interface MonthlyDataDto {
  month: number;  // 1-12
  monthName: string;  // "January", "February", etc.
  monthAbbreviation: string;  // "Jan", "Feb", etc.
  incoming: number;
  outgoing: number;
  net: number;  // incoming - outgoing
  transactionCount: number;
}
```

---

## Request/Response Examples

### Example 1: Get Dashboard Summary

**Request:**
```http
GET /api/Reports/summary
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalIncome": 50000.00,
    "totalExpenses": 35000.00,
    "disposableIncome": 15000.00,
    "netWorth": 245000.00
  }
}
```

---

### Example 2: Get Monthly Income Report

**Request:**
```http
GET /api/Reports/income?period=MONTHLY
Authorization: Bearer <token>
```

**Response:** See Income Report section above

---

### Example 3: Get Quarterly Expense Analysis

**Request:**
```http
GET /api/Reports/expenses?period=QUARTERLY
Authorization: Bearer <token>
```

**Response:** See Expense Report section above

---

### Example 4: Get Custom Date Range Report

**Request:**
```http
GET /api/Reports/full?period=CUSTOM&startDate=2025-01-01&endDate=2025-06-30
Authorization: Bearer <token>
```

**Response:** See Full Report section above

---

### Example 5: Get Loan Report

**Request:**
```http
GET /api/Reports/loans?period=MONTHLY
Authorization: Bearer <token>
```

**Response:** See Loan Report section above

---

### Example 6: Get Net Worth Report

**Request:**
```http
GET /api/Reports/networth?period=MONTHLY
Authorization: Bearer <token>
```

**Response:** See Net Worth Report section above

---

### Example 7: Get Monthly Cash Flow for Current Year

**Request:**
```http
GET /api/Analytics/monthly-cash-flow
Authorization: Bearer <token>
```

**Response:** See Monthly Cash Flow section above

---

### Example 8: Get Monthly Cash Flow for Specific Year

**Request:**
```http
GET /api/Analytics/monthly-cash-flow?year=2024
Authorization: Bearer <token>
```

**Response:** See Monthly Cash Flow section above

---

## Frontend Integration Examples

### React/TypeScript Example

```typescript
import axios from 'axios';

const API_BASE_URL = 'https://api.utilityhub360.com/api';

// Get Financial Summary
async function getFinancialSummary(token: string, date?: Date) {
  try {
    const params = date ? { date: date.toISOString() } : {};
    const response = await axios.get(
      `${API_BASE_URL}/Reports/summary`,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        params
      }
    );
    
    if (response.data.success) {
      return response.data.data;
    } else {
      throw new Error(response.data.message);
    }
  } catch (error) {
    console.error('Error fetching financial summary:', error);
    throw error;
  }
}

// Get Full Report
async function getFullReport(
  token: string, 
  period: 'MONTHLY' | 'QUARTERLY' | 'YEARLY' | 'CUSTOM',
  startDate?: Date,
  endDate?: Date
) {
  try {
    const params: any = { period };
    
    if (period === 'CUSTOM' && startDate && endDate) {
      params.startDate = startDate.toISOString();
      params.endDate = endDate.toISOString();
    }
    
    const response = await axios.get(
      `${API_BASE_URL}/Reports/full`,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        params
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching full report:', error);
    throw error;
  }
}

// Get Income Report
async function getIncomeReport(
  token: string,
  period: 'MONTHLY' | 'QUARTERLY' | 'YEARLY' | 'CUSTOM',
  startDate?: Date,
  endDate?: Date
) {
  try {
    const params: any = { period };
    
    if (period === 'CUSTOM' && startDate && endDate) {
      params.startDate = startDate.toISOString();
      params.endDate = endDate.toISOString();
    }
    
    const response = await axios.get(
      `${API_BASE_URL}/Reports/income`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        },
        params
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching income report:', error);
    throw error;
  }
}

// Get Expense Report
async function getExpenseReport(
  token: string,
  period: 'MONTHLY' | 'QUARTERLY' | 'YEARLY' | 'CUSTOM',
  startDate?: Date,
  endDate?: Date
) {
  try {
    const params: any = { period };
    
    if (period === 'CUSTOM' && startDate && endDate) {
      params.startDate = startDate.toISOString();
      params.endDate = endDate.toISOString();
    }
    
    const response = await axios.get(
      `${API_BASE_URL}/Reports/expenses`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        },
        params
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching expense report:', error);
    throw error;
  }
}

// Get Loan Report
async function getLoanReport(
  token: string,
  period: 'MONTHLY' | 'QUARTERLY' | 'YEARLY' | 'CUSTOM',
  startDate?: Date,
  endDate?: Date
) {
  try {
    const params: any = { period };
    
    if (period === 'CUSTOM' && startDate && endDate) {
      params.startDate = startDate.toISOString();
      params.endDate = endDate.toISOString();
    }
    
    const response = await axios.get(
      `${API_BASE_URL}/Reports/loans`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        },
        params
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching loan report:', error);
    throw error;
  }
}

// Get Monthly Cash Flow
async function getMonthlyCashFlow(
  token: string,
  year?: number
) {
  try {
    const params: any = {};
    if (year) {
      params.year = year;
    }
    
    const response = await axios.get(
      `${API_BASE_URL}/Analytics/monthly-cash-flow`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        },
        params
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching monthly cash flow:', error);
    throw error;
  }
}

// Get Net Worth Report
async function getNetWorthReport(
  token: string,
  period: 'MONTHLY' | 'QUARTERLY' | 'YEARLY' | 'CUSTOM',
  startDate?: Date,
  endDate?: Date
) {
  try {
    const params: any = { period };
    
    if (period === 'CUSTOM' && startDate && endDate) {
      params.startDate = startDate.toISOString();
      params.endDate = endDate.toISOString();
    }
    
    const response = await axios.get(
      `${API_BASE_URL}/Reports/networth`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        },
        params
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching net worth report:', error);
    throw error;
  }
}
```

### React Hook Example

```typescript
import { useState, useEffect } from 'react';

function useFinancialReports(token: string) {
  const [summary, setSummary] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchSummary() {
      try {
        setLoading(true);
        const data = await getFinancialSummary(token);
        setSummary(data);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load summary');
      } finally {
        setLoading(false);
      }
    }

    if (token) {
      fetchSummary();
    }
  }, [token]);

  return {
    summary,
    loading,
    error,
    refetch: () => {
      setLoading(true);
      getFinancialSummary(token)
        .then(setSummary)
        .catch((err) => setError(err.message))
        .finally(() => setLoading(false));
    }
  };
}
```

### React Component Example

```tsx
import React, { useState, useEffect } from 'react';
import { Line, Pie, Bar } from 'react-chartjs-2';

function FinancialDashboard({ token }: { token: string }) {
  const [summary, setSummary] = useState<any>(null);
  const [fullReport, setFullReport] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadData() {
      try {
        const [summaryData, reportData] = await Promise.all([
          getFinancialSummary(token),
          getFullReport(token, 'MONTHLY')
        ]);
        
        setSummary(summaryData);
        setFullReport(reportData);
      } catch (error) {
        console.error('Error loading dashboard:', error);
      } finally {
        setLoading(false);
      }
    }

    if (token) {
      loadData();
    }
  }, [token]);

  if (loading) return <div>Loading...</div>;
  if (!summary || !fullReport) return <div>No data available</div>;

  // Income Trend Chart
  const incomeChartData = {
    labels: fullReport.incomeReport.incomeTrend.map((t: any) => t.label),
    datasets: [{
      label: 'Income',
      data: fullReport.incomeReport.incomeTrend.map((t: any) => t.value),
      borderColor: 'rgb(75, 192, 192)',
      backgroundColor: 'rgba(75, 192, 192, 0.2)'
    }]
  };

  // Expense Category Pie Chart
  const expensePieData = {
    labels: Object.keys(fullReport.expenseReport.expenseByCategory),
    datasets: [{
      data: Object.values(fullReport.expenseReport.expenseByCategory),
      backgroundColor: [
        '#FF6384',
        '#36A2EB',
        '#FFCE56',
        '#4BC0C0',
        '#9966FF',
        '#FF9F40'
      ]
    }]
  };

  return (
    <div className="dashboard">
      {/* Summary Cards */}
      <div className="summary-cards">
        <Card 
          title="Total Income" 
          value={`$${summary.totalIncome.toLocaleString()}`}
          change={`${summary.incomeChange > 0 ? '+' : ''}${summary.incomeChange.toFixed(1)}%`}
          trend={summary.incomeChange >= 0 ? 'up' : 'down'}
        />
        <Card 
          title="Total Expenses" 
          value={`$${summary.totalExpenses.toLocaleString()}`}
          change={`${summary.expenseChange > 0 ? '+' : ''}${summary.expenseChange.toFixed(1)}%`}
          trend={summary.expenseChange >= 0 ? 'down' : 'up'}
        />
        <Card 
          title="Disposable Income" 
          value={`$${summary.disposableIncome.toLocaleString()}`}
          change={`${summary.disposableIncomeChange > 0 ? '+' : ''}${summary.disposableIncomeChange.toFixed(1)}%`}
          trend={summary.disposableIncomeChange >= 0 ? 'up' : 'down'}
        />
        <Card 
          title="Net Worth" 
          value={`$${summary.netWorth.toLocaleString()}`}
          change={`${summary.netWorthChange > 0 ? '+' : ''}${summary.netWorthChange.toFixed(1)}%`}
          trend={summary.netWorthChange >= 0 ? 'up' : 'down'}
        />
      </div>

      {/* Charts */}
      <div className="charts-grid">
        {fullReport.incomeReport.incomeTrend.length > 0 && (
          <div className="chart-card">
            <h3>Income Trend</h3>
            <Line data={incomeChartData} />
          </div>
        )}
        
        <div className="chart-card">
          <h3>Expense by Category</h3>
          <Pie data={expensePieData} />
        </div>

        {fullReport.netWorthReport.netWorthTrend.length > 0 && (
          <div className="chart-card">
            <h3>Net Worth Trend</h3>
            <Line data={{
              labels: fullReport.netWorthReport.netWorthTrend.map((t: any) => t.label),
              datasets: [{
                label: 'Net Worth',
                data: fullReport.netWorthReport.netWorthTrend.map((t: any) => t.value),
                borderColor: 'rgb(54, 162, 235)',
                backgroundColor: 'rgba(54, 162, 235, 0.2)'
              }]
            }} />
          </div>
        )}
      </div>

      {/* Loan Summary */}
      {fullReport.loanReport && fullReport.loanReport.activeLoansCount > 0 && (
        <div className="loan-summary">
          <h3>Active Loans</h3>
          <p>Total Remaining: ${fullReport.loanReport.totalRemainingBalance.toLocaleString()}</p>
          <p>Monthly Payment: ${fullReport.loanReport.totalMonthlyPayment.toLocaleString()}</p>
          <p>Debt-Free Date: {new Date(fullReport.loanReport.projectedDebtFreeDate).toLocaleDateString()}</p>
        </div>
      )}
    </div>
  );
}
```

---

## Chart Integration

### Chart.js Setup

```typescript
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import { Line, Pie, Bar } from 'react-chartjs-2';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  BarElement,
  Title,
  Tooltip,
  Legend
);
```

### Line Chart (Trends)

```typescript
function IncomeTrendChart({ trendData }: { trendData: TrendDataPoint[] }) {
  if (!trendData || trendData.length === 0) {
    return <div>No trend data available</div>;
  }

  const data = {
    labels: trendData.map(d => d.label),
    datasets: [{
      label: 'Income',
      data: trendData.map(d => d.value),
      borderColor: 'rgb(75, 192, 192)',
      backgroundColor: 'rgba(75, 192, 192, 0.2)',
      tension: 0.4
    }]
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: true,
        text: 'Income Trend'
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: function(value: any) {
            return '$' + value.toLocaleString();
          }
        }
      }
    }
  };

  return <Line data={data} options={options} />;
}
```

### Pie Chart (Distribution)

```typescript
function ExpenseCategoryChart({ expenseByCategory }: { expenseByCategory: Record<string, number> }) {
  const data = {
    labels: Object.keys(expenseByCategory),
    datasets: [{
      data: Object.values(expenseByCategory),
      backgroundColor: [
        '#FF6384',
        '#36A2EB',
        '#FFCE56',
        '#4BC0C0',
        '#9966FF',
        '#FF9F40'
      ]
    }]
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'right' as const,
      },
      tooltip: {
        callbacks: {
          label: function(context: any) {
            const label = context.label || '';
            const value = context.parsed || 0;
            const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
            const percentage = ((value / total) * 100).toFixed(1);
            return `${label}: $${value.toLocaleString()} (${percentage}%)`;
          }
        }
      }
    }
  };

  return <Pie data={data} options={options} />;
}
```

### Bar Chart (Comparison)

```typescript
function CategoryComparisonChart({ comparison }: { comparison: ExpenseComparisonDto[] }) {
  const data = {
    labels: comparison.map(c => c.category),
    datasets: [
      {
        label: 'Current',
        data: comparison.map(c => c.currentAmount),
        backgroundColor: 'rgba(75, 192, 192, 0.6)'
      },
      {
        label: 'Previous',
        data: comparison.map(c => c.previousAmount),
        backgroundColor: 'rgba(255, 99, 132, 0.6)'
      }
    ]
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  return <Bar data={data} options={options} />;
}
```

---

## Error Handling

### Common Error Responses

#### 400 Bad Request - Invalid Period
```json
{
  "success": false,
  "message": "Invalid period specified. Use MONTHLY, QUARTERLY, YEARLY, or CUSTOM.",
  "data": null,
  "errors": []
}
```

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "User not authenticated",
  "data": null,
  "errors": []
}
```

#### 400 Bad Request - Invalid Date Range
```json
{
  "success": false,
  "message": "Start date must be before end date",
  "data": null,
  "errors": []
}
```

### Error Handling Best Practices

```typescript
async function handleApiCall<T>(
  apiCall: () => Promise<T>
): Promise<{ data?: T; error?: string }> {
  try {
    const data = await apiCall();
    return { data };
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const errorMessage = error.response?.data?.message || error.message;
      
      // Handle specific error codes
      if (error.response?.status === 401) {
        // Redirect to login
        window.location.href = '/login';
        return { error: 'Session expired. Please login again.' };
      }
      
      return { error: errorMessage };
    }
    return { error: 'An unexpected error occurred' };
  }
}

// Usage
const { data, error } = await handleApiCall(() => 
  getFinancialSummary(token)
);

if (error) {
  // Show error toast/notification
  showError(error);
} else {
  // Process data
  setSummary(data);
}
```

---

## Best Practices

### 1. Always Include Authorization Header
```typescript
const headers = {
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
};
```

### 2. Cache Report Data
```typescript
// Cache reports for 5 minutes
const CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

let cachedSummary: { data: any; timestamp: number } | null = null;

async function getCachedSummary(token: string) {
  const now = Date.now();
  
  if (cachedSummary && (now - cachedSummary.timestamp) < CACHE_DURATION) {
    return cachedSummary.data;
  }
  
  const data = await getFinancialSummary(token);
  cachedSummary = { data, timestamp: now };
  
  return data;
}
```

### 3. Use Appropriate Periods
```typescript
// Dashboard: Use MONTHLY
const summary = await getFinancialSummary(token);

// Trends page: Use QUARTERLY
const trends = await getFullReport(token, 'QUARTERLY');

// Annual review: Use YEARLY
const annual = await getFullReport(token, 'YEARLY');
```

### 4. Format Currency Properly
```typescript
function formatCurrency(amount: number, currency: string = 'USD'): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency
  }).format(amount);
}

// Usage
formatCurrency(50000.00); // "$50,000.00"
formatCurrency(50000.00, 'EUR'); // "€50,000.00"
```

### 5. Handle Loading States
```typescript
const [loading, setLoading] = useState(false);
const [data, setData] = useState(null);

async function loadReport() {
  setLoading(true);
  try {
    const report = await getFullReport(token, 'MONTHLY');
    setData(report);
  } catch (error) {
    console.error(error);
  } finally {
    setLoading(false);
  }
}

// In component
{loading ? (
  <Spinner />
) : (
  <ReportDisplay data={data} />
)}
```

### 6. Handle Empty Trend Data
```typescript
function TrendChart({ trendData }: { trendData: TrendDataPoint[] }) {
  if (!trendData || trendData.length === 0) {
    return (
      <div className="empty-chart">
        <p>No trend data available yet. Check back after more transactions.</p>
      </div>
    );
  }

  // Render chart with data
  return <Line data={chartData} />;
}
```

### 7. Display Percentage Changes
```typescript
function PercentageChange({ value, isPositive }: { value: number; isPositive: boolean }) {
  const color = isPositive ? 'green' : 'red';
  const icon = isPositive ? '↑' : '↓';
  
  return (
    <span style={{ color }}>
      {icon} {Math.abs(value).toFixed(1)}%
    </span>
  );
}
```

### 8. Optimize API Calls
```typescript
// Good: Fetch only what you need
const summary = await getFinancialSummary(token);

// Better: Fetch multiple reports in parallel (if needed)
const [summary, incomeReport, expenseReport] = await Promise.all([
  getFinancialSummary(token),
  getIncomeReport(token, 'MONTHLY'),
  getExpenseReport(token, 'MONTHLY')
]);

// Avoid: Fetching full report when you only need summary
const fullReport = await getFullReport(token, 'MONTHLY'); // Only if needed
```

### 9. Handle Empty States
```tsx
function ReportsDisplay({ report }: { report: any }) {
  if (!report || !report.summary) {
    return (
      <div className="empty-state">
        <p>No financial data available yet.</p>
        <p>Start by adding income sources, bills, and transactions.</p>
      </div>
    );
  }

  // Render report data
  return <ReportContent report={report} />;
}
```

### 10. Validate Period Parameters
```typescript
function validatePeriod(period: string): boolean {
  const validPeriods = ['MONTHLY', 'QUARTERLY', 'YEARLY', 'CUSTOM'];
  return validPeriods.includes(period.toUpperCase());
}

async function getReportWithValidation(
  token: string,
  period: string,
  startDate?: Date,
  endDate?: Date
) {
  if (!validatePeriod(period)) {
    throw new Error('Invalid period. Use MONTHLY, QUARTERLY, YEARLY, or CUSTOM');
  }

  if (period === 'CUSTOM' && (!startDate || !endDate)) {
    throw new Error('startDate and endDate are required for CUSTOM period');
  }

  return getFullReport(token, period as any, startDate, endDate);
}
```

---

---

### 7. Get Monthly Cash Flow (Incoming & Outgoing) ✅

**Endpoint:** `GET /api/Analytics/monthly-cash-flow`

**Description:** Get total incoming and outgoing amounts per month for a given year (January to December). This endpoint provides a complete 12-month breakdown of cash flow, making it perfect for annual financial charts and analysis.

**Query Parameters:**
- `year` (optional): Year to retrieve data for (defaults to current year). Example: `2025`

**Example Requests:**
```
GET /api/Analytics/monthly-cash-flow
GET /api/Analytics/monthly-cash-flow?year=2024
GET /api/Analytics/monthly-cash-flow?year=2025
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Monthly cash flow data retrieved successfully for 2025",
  "data": {
    "year": 2025,
    "monthlyData": [
      {
        "month": 1,
        "monthName": "January",
        "monthAbbreviation": "Jan",
        "incoming": 50000.00,
        "outgoing": 35000.00,
        "net": 15000.00,
        "transactionCount": 45
      },
      {
        "month": 2,
        "monthName": "February",
        "monthAbbreviation": "Feb",
        "incoming": 48000.00,
        "outgoing": 32000.00,
        "net": 16000.00,
        "transactionCount": 38
      },
      {
        "month": 3,
        "monthName": "March",
        "monthAbbreviation": "Mar",
        "incoming": 52000.00,
        "outgoing": 37000.00,
        "net": 15000.00,
        "transactionCount": 42
      }
      // ... (continues for all 12 months)
    ],
    "totalIncoming": 600000.00,
    "totalOutgoing": 420000.00,
    "netCashFlow": 180000.00
  },
  "errors": []
}
```

**Fields:**
- `year`: The year for which data is retrieved
- `monthlyData`: Array of 12 objects (one for each month)
  - `month`: Month number (1-12)
  - `monthName`: Full month name (January, February, etc.)
  - `monthAbbreviation`: Short month name (Jan, Feb, etc.)
  - `incoming`: Total incoming amount (CREDIT transactions) for the month
  - `outgoing`: Total outgoing amount (DEBIT transactions) for the month
  - `net`: Net cash flow (incoming - outgoing) for the month
  - `transactionCount`: Number of transactions in the month
- `totalIncoming`: Sum of all incoming amounts for the year
- `totalOutgoing`: Sum of all outgoing amounts for the year
- `netCashFlow`: Total net cash flow for the year (totalIncoming - totalOutgoing)

**Use Case:** 
- Annual financial charts showing monthly trends
- Year-end financial summaries
- Monthly cash flow analysis
- Budget vs actual spending by month
- Income and expense trends visualization

**Notes:**
- Only includes bank transactions (where `IsBankTransaction = true`)
- Only includes transactions with `TransactionType` of "CREDIT" (incoming) or "DEBIT" (outgoing)
- All months are included in the response, even if there are no transactions (amounts will be 0)
- Amounts are calculated based on the `TransactionDate` field

---

## Summary

This documentation covers all **fully implemented** Financial Analytics & Reports API endpoints:

✅ **Working Endpoints (7):**
1. Financial Summary
2. Full Financial Report
3. Income Report
4. Expense Report
5. Loan Report
6. Net Worth Report
7. **Monthly Cash Flow (NEW)** - Get incoming/outgoing per month

⚠️ **Not Yet Implemented:**
- Disposable Income Report
- Bills Report
- Savings Report
- Financial Insights
- Financial Predictions
- Recent Transactions
- Period Comparison
- PDF/CSV Export

All endpoints in this documentation are **fully functional** and ready for frontend integration. For additional help or questions, refer to the main API documentation or contact the development team.
