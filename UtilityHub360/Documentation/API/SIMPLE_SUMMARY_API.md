# 💰 Simple Financial Summary API

## The Simplest Way to Get Remaining Amount!

### One Simple Endpoint
```
GET /api/Dashboard/summary
```

---

## 💡 1. What It Does

Pulls data directly from your database and calculates your **Remaining Amount** - the money left after all expenses and savings.

---

## ⚙️ 2. Formula

```
Remaining Amount = Total Income - Total Expenses - Total Savings

Where:
├── Total Income       = Sum of all active income sources (monthly)
├── Total Expenses     = Bills + Loans  
├── Total Savings      = Savings deposits this month
└── Remaining Amount   = What's left for discretionary use
```

### 📊 Data Sources

| Component | Database Table | Description |
|-----------|---------------|-------------|
| **Income** | `IncomeSources` | All active income (auto-converted to monthly) |
| **Bills** | `Bills` | Bills due this month |
| **Loans** | `Loans` | Monthly loan payments |
| **Savings** | `SavingsTransactions` | Deposits made this month |

---

## 🧮 3. Step-by-Step Calculation Example

### Your Data from Database:

| Item | Source | Amount (₱) |
|------|--------|-----------|
| Total Income | IncomeSources table | 45,000 |
| Total Bills | Bills table (this month) | 15,000 |
| Total Loans | Loans table (monthly payment) | 8,000 |
| Total Savings | SavingsTransactions (deposits) | 5,000 |

### Calculation:

```
Step 1: Total Income                 = ₱45,000
Step 2: Total Expenses (Bills+Loans) = ₱15,000 + ₱8,000 = ₱23,000
Step 3: Total Savings                = ₱5,000

Remaining Amount = 45,000 - 23,000 - 5,000 = ₱17,000
```

```
╔═══════════════════════════════════════╗
║  CALCULATION BREAKDOWN                ║
╠═══════════════════════════════════════╣
║  Total Income:            ₱45,000    ║
║                                       ║
║  Total Bills:            -₱15,000    ║
║  Total Loans:             -₱8,000    ║
║  ─────────────────────────────────    ║
║  Total Expenses:         -₱23,000    ║
║                                       ║
║  Total Savings:           -₱5,000    ║
║                                       ║
║  ═══════════════════════════════════  ║
║  💰 REMAINING AMOUNT:     ₱17,000    ║
║  ═══════════════════════════════════  ║
║                                       ║
║  As % of Income:            37.78%   ║
║  Status:                  ✅ HEALTHY  ║
╚═══════════════════════════════════════╝
```

**✅ Your Remaining Amount = ₱17,000**

---

## 🚀 4. How to Use

### Current Month (No parameters!)
```http
GET /api/Dashboard/summary
Authorization: Bearer {your-token}
```

### Specific Month
```http
GET /api/Dashboard/summary?year=2025&month=9
Authorization: Bearer {your-token}
```

---

## 📋 5. API Response

```json
{
  "success": true,
  "data": {
    "userId": "user123",
    "month": 10,
    "year": 2025,
    
    // INCOME FROM DB
    "totalIncome": 45000.00,
    "incomeSourcesCount": 2,
    
    // BILLS FROM DB
    "totalBills": 15000.00,
    "billsCount": 5,
    
    // LOANS FROM DB
    "totalLoans": 8000.00,
    "activeLoansCount": 2,
    
    // TOTAL EXPENSES (Bills + Loans)
    "totalExpenses": 23000.00,
    
    // SAVINGS FROM DB
    "totalSavings": 5000.00,
    "savingsAccountsCount": 1,
    
    // THE ANSWER!
    "remainingAmount": 17000.00,
    "remainingPercentage": 37.78,
    
    // STATUS
    "financialStatus": "HEALTHY"
  }
}
```

---

## 🎯 6. Financial Status

| Status | Remaining % | Meaning |
|--------|-------------|---------|
| **HEALTHY** ✅ | ≥ 20% | Good financial health |
| **WARNING** ⚠️ | 10-20% | Monitor your budget |
| **CRITICAL** 🚨 | 0-10% | Very tight - reduce spending |
| **DEFICIT** ❌ | < 0% | Spending more than earning! |

---

## 💻 7. Code Examples

### JavaScript (Fetch)
```javascript
const getRemaining = async () => {
  const response = await fetch('/api/Dashboard/summary', {
    headers: {
      'Authorization': `Bearer ${yourToken}`
    }
  });
  const { data } = await response.json();
  
  console.log(`Remaining: ₱${data.remainingAmount}`);
  console.log(`Status: ${data.financialStatus}`);
};
```

### JavaScript (Axios)
```javascript
const { data } = await axios.get('/api/Dashboard/summary', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

console.log('Remaining:', data.data.remainingAmount);
console.log('Status:', data.data.financialStatus);
```

### React Component
```jsx
const FinancialSummary = () => {
  const [summary, setSummary] = useState(null);

  useEffect(() => {
    fetch('/api/Dashboard/summary', {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => setSummary(data.data));
  }, []);

  if (!summary) return <div>Loading...</div>;

  return (
    <div className={`status-${summary.financialStatus.toLowerCase()}`}>
      <h2>Remaining Amount</h2>
      <h1>₱{summary.remainingAmount.toLocaleString()}</h1>
      <p>{summary.remainingPercentage.toFixed(1)}% of income</p>
      
      <div className="breakdown">
        <div>Income: ₱{summary.totalIncome.toLocaleString()}</div>
        <div>Expenses: -₱{summary.totalExpenses.toLocaleString()}</div>
        <div>Savings: -₱{summary.totalSavings.toLocaleString()}</div>
      </div>
      
      <span className="badge">{summary.financialStatus}</span>
    </div>
  );
};
```

---

## 🧠 8. System Formula Summary

### Pseudocode
```javascript
// Get total income (monthly)
total_income = sum(income_sources.monthlyAmount);

// Get expenses
total_bills = sum(bills.amount);  // This month's bills
total_loans = sum(loans.monthlyPayment);  // Active loans
total_expenses = total_bills + total_loans;

// Get savings
total_savings = sum(savings_transactions.amount);  // This month's deposits

// Calculate remaining
remaining_amount = total_income - total_expenses - total_savings;
```

### C# Implementation
```csharp
// Actual implementation in DisposableAmountService.cs
decimal totalIncome = incomeSources.Sum(i => i.MonthlyAmount);
decimal totalBills = bills.Sum(b => b.Amount);
decimal totalLoans = loans.Sum(l => l.MonthlyPayment);
decimal totalExpenses = totalBills + totalLoans;
decimal totalSavings = savingsTransactions.Sum(st => st.Amount);

decimal remainingAmount = totalIncome - totalExpenses - totalSavings;
```