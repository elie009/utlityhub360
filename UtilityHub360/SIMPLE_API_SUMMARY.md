# ✅ Simple Disposable Amount API - Ready to Use!

## 🎯 You Now Have ONE Simple API!

### Main Endpoint
```
GET /api/Dashboard/summary
```

---

## 💰 What It Does

**Simple calculation from your database:**

```
Remaining Amount = Total Income - Total Expenses - Total Savings

Where:
├── Total Income       = Sum of all active income sources (monthly)
├── Total Bills        = Bills due this month
├── Total Loans        = Monthly loan payments
├── Total Savings      = Savings deposits this month
└── Total Expenses     = Bills + Loans
```

---

## 🚀 How to Use

### 1. Current Month (No parameters!)
```http
GET /api/Dashboard/summary
Authorization: Bearer {your-token}
```

### 2. Specific Month
```http
GET /api/Dashboard/summary?year=2025&month=9
Authorization: Bearer {your-token}
```

---

## 📊 Response Example

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
    
    // TOTAL EXPENSES
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

## 💡 Calculation Example

```
Your Data:
├─ Total Income (from IncomeSources):     ₱45,000
├─ Total Bills (from Bills table):       -₱15,000
├─ Total Loans (monthly payments):        -₱8,000
├─ Total Savings (deposits this month):   -₱5,000
└─ ═══════════════════════════════════════════════
   REMAINING AMOUNT:                       ₱17,000

   Remaining % of Income:                   37.78%
   Status:                                  HEALTHY ✅
```

---

## 🎯 Financial Status

| Status | Remaining % | Meaning |
|--------|-------------|---------|
| **HEALTHY** ✅ | ≥ 20% | Good financial health |
| **WARNING** ⚠️ | 10-20% | Monitor your budget |
| **CRITICAL** 🚨 | 0-10% | Very tight - reduce spending |
| **DEFICIT** ❌ | < 0% | Spending more than earning! |

---

## 💻 Code Examples

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
      <span className="badge">{summary.financialStatus}</span>
    </div>
  );
};
```

---

## 📋 What's in the Database

### Income Sources (`IncomeSources` table)
- All your income streams
- Automatically converted to monthly
- Only active sources counted

### Bills (`Bills` table)
- Only bills due in the specified month
- PENDING or PAID status
- Real amounts from DB

### Loans (`Loans` table)
- Active and approved loans only
- Monthly payment amount
- Real data from DB

### Savings (`SavingsTransactions` table)
- Only DEPOSIT transactions
- For the specified month
- Real savings from DB

---

## 🧪 Test It Now!

1. **Stop your running app** (if any)
2. **Restart the app:**
   ```powershell
   dotnet run
   ```
3. **Open Swagger:**
   ```
   http://localhost:5000/swagger
   ```
4. **Find the endpoint:**
   - Dashboard → GET /api/Dashboard/summary
5. **Authorize** with your JWT token
6. **Try it out!**

---

## ✨ Features

✅ **Direct DB Queries** - No complex calculations  
✅ **Real Data** - From your actual database  
✅ **Simple Formula** - Easy to understand  
✅ **Current & Historical** - Any month you want  
✅ **Financial Status** - Automatic health check  
✅ **Fast Response** - Lightweight queries  

---

## 📝 Parameters

| Parameter | Type | Required | Default | Example |
|-----------|------|----------|---------|---------|
| `year` | int | No | Current year | 2025 |
| `month` | int | No | Current month | 10 |

---

## 🎨 Simple Display Component

```jsx
<div className="financial-card">
  <h3>💰 Available Money</h3>
  
  <div className="breakdown">
    <div className="line">
      <span>Income:</span>
      <span>₱{totalIncome}</span>
    </div>
    <div className="line expenses">
      <span>Bills & Loans:</span>
      <span>-₱{totalExpenses}</span>
    </div>
    <div className="line savings">
      <span>Savings:</span>
      <span>-₱{totalSavings}</span>
    </div>
    <div className="line total">
      <span>Remaining:</span>
      <span className={statusClass}>
        ₱{remainingAmount}
      </span>
    </div>
  </div>
  
  <div className="status-badge">
    {financialStatus}
  </div>
</div>
```

---

## 📞 Support

- Check the response in Swagger
- Verify data exists in your database tables
- Ensure JWT token is valid

---

**Created:** October 11, 2025  
**Status:** ✅ Ready to Use  
**Complexity:** ⭐ Super Simple!

