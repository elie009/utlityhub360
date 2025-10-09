# 🎯 Frontend Guide: Update Loan API

## Endpoint
```
PUT /api/Loans/{loanId}
```

## 🚀 How It Works (Automated)

The backend **automatically calculates** related financial values for you. Just send what you want to update!

---

## 📋 Request Body Schema

```json
{
  "purpose": "string (optional)",
  "additionalInfo": "string (optional)",
  "status": "string (optional)",
  "interestRate": "number (optional)",
  "monthlyPayment": "number (optional)",
  "remainingBalance": "number (optional)"
}
```

---

## 💡 Frontend Implementation Examples

### **Example 1: User Changes Interest Rate Only**
**Frontend sends:**
```json
{
  "interestRate": 5.5
}
```

**Backend automatically:**
- ✅ Sets `interestRate = 5.5`
- ✅ Recalculates `monthlyPayment` based on formula
- ✅ Recalculates `totalAmount = monthlyPayment × term`
- ✅ Recalculates `remainingBalance` (maintains paid amount)

**Result:** Everything updates automatically! 🎉

---

### **Example 2: User Manually Sets Monthly Payment**
**Frontend sends:**
```json
{
  "interestRate": 5.5,
  "monthlyPayment": 725
}
```

**Backend automatically:**
- ✅ Sets `interestRate = 5.5`
- ✅ Uses manual `monthlyPayment = 725` (ignores calculated value)
- ✅ Recalculates `totalAmount = 725 × term`
- ✅ Recalculates `remainingBalance`

**Result:** Uses your custom monthly payment! 💪

---

### **Example 3: Update Status Only**
**Frontend sends:**
```json
{
  "status": "APPROVED"
}
```

**Backend automatically:**
- ✅ Sets `status = "APPROVED"`
- ✅ Leaves all financial values unchanged

**Result:** Simple status update! ✨

---

### **Example 4: Manual Override Everything**
**Frontend sends:**
```json
{
  "interestRate": 0,
  "monthlyPayment": 725,
  "remainingBalance": 43500
}
```

**Backend automatically:**
- ✅ Sets `interestRate = 0`
- ✅ Uses manual `monthlyPayment = 725`
- ✅ Uses manual `remainingBalance = 43500`
- ✅ Recalculates `totalAmount = 725 × term`

**Result:** Full manual control! 🎯

---

## 🎨 Frontend UI/UX Recommendations

### **Scenario A: Auto-Calculate Mode (Recommended)**
```javascript
// When user changes interest rate, clear manual overrides
const handleInterestRateChange = (newRate) => {
  setInterestRate(newRate);
  // Don't send monthlyPayment or remainingBalance
  // Backend will calculate them automatically
};

// API Call
const updateLoan = async () => {
  await axios.put(`/api/Loans/${loanId}`, {
    interestRate: interestRate
    // Backend calculates monthlyPayment and remainingBalance
  });
};
```

**UI Flow:**
```
User changes interest rate input
       ↓
Frontend sends ONLY interestRate
       ↓
Backend calculates everything
       ↓
Fetch updated loan data
       ↓
Display new values to user
```

---

### **Scenario B: Manual Override Mode (Advanced Users)**
```javascript
const [manualMode, setManualMode] = useState(false);

// API Call with manual values
const updateLoan = async () => {
  const payload = {
    interestRate: interestRate
  };
  
  if (manualMode) {
    payload.monthlyPayment = monthlyPayment;
    payload.remainingBalance = remainingBalance;
  }
  
  await axios.put(`/api/Loans/${loanId}`, payload);
};
```

**UI Example:**
```
[ ] Enable Manual Mode (Checkbox)

Interest Rate: [___5.5___] %

If Manual Mode OFF:
  Monthly Payment: $725.00 (Auto-calculated)
  Remaining Balance: $43,500.00 (Auto-calculated)

If Manual Mode ON:
  Monthly Payment: [___725___] (Editable)
  Remaining Balance: [___43500___] (Editable)
```

---

## 📊 Complete Frontend Example (React)

```jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

const LoanUpdateForm = ({ loanId, currentLoan }) => {
  const [interestRate, setInterestRate] = useState(currentLoan.interestRate);
  const [monthlyPayment, setMonthlyPayment] = useState(currentLoan.monthlyPayment);
  const [remainingBalance, setRemainingBalance] = useState(currentLoan.remainingBalance);
  const [manualMode, setManualMode] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      // Build payload - only include what changed
      const payload = {};

      if (interestRate !== currentLoan.interestRate) {
        payload.interestRate = interestRate;
      }

      // Only include manual values if manual mode is ON
      if (manualMode) {
        if (monthlyPayment !== currentLoan.monthlyPayment) {
          payload.monthlyPayment = monthlyPayment;
        }
        if (remainingBalance !== currentLoan.remainingBalance) {
          payload.remainingBalance = remainingBalance;
        }
      }

      // Send update request
      const response = await axios.put(`/api/Loans/${loanId}`, payload);

      if (response.data.success) {
        // Update UI with new values from backend
        const updatedLoan = response.data.data;
        setInterestRate(updatedLoan.interestRate);
        setMonthlyPayment(updatedLoan.monthlyPayment);
        setRemainingBalance(updatedLoan.remainingBalance);
        
        alert('Loan updated successfully!');
      }
    } catch (error) {
      console.error('Update failed:', error);
      alert('Failed to update loan');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Interest Rate (%)</label>
        <input
          type="number"
          step="0.01"
          value={interestRate}
          onChange={(e) => setInterestRate(parseFloat(e.target.value))}
        />
      </div>

      <div>
        <label>
          <input
            type="checkbox"
            checked={manualMode}
            onChange={(e) => setManualMode(e.target.checked)}
          />
          Manual Override Mode
        </label>
      </div>

      <div>
        <label>Monthly Payment</label>
        <input
          type="number"
          step="0.01"
          value={monthlyPayment}
          onChange={(e) => setMonthlyPayment(parseFloat(e.target.value))}
          disabled={!manualMode}
          placeholder={manualMode ? "Enter amount" : "Auto-calculated"}
        />
        {!manualMode && <small>Automatically calculated by backend</small>}
      </div>

      <div>
        <label>Remaining Balance</label>
        <input
          type="number"
          step="0.01"
          value={remainingBalance}
          onChange={(e) => setRemainingBalance(parseFloat(e.target.value))}
          disabled={!manualMode}
          placeholder={manualMode ? "Enter amount" : "Auto-calculated"}
        />
        {!manualMode && <small>Automatically calculated by backend</small>}
      </div>

      <button type="submit" disabled={loading}>
        {loading ? 'Updating...' : 'Update Loan'}
      </button>
    </form>
  );
};

export default LoanUpdateForm;
```

---

## 🔄 Recommended Workflow

### **Simple Workflow (Most Users)**
1. User changes interest rate in the form
2. Frontend sends **only** `interestRate` to backend
3. Backend calculates `monthlyPayment`, `totalAmount`, and `remainingBalance`
4. Frontend fetches updated loan and displays new values
5. ✅ Done!

### **Advanced Workflow (Power Users)**
1. User enables "Manual Mode" checkbox
2. User changes interest rate AND manually sets monthly payment
3. Frontend sends `interestRate` + `monthlyPayment` to backend
4. Backend uses manual values and recalculates `totalAmount` and `remainingBalance`
5. Frontend displays updated values
6. ✅ Done!

---

## ⚠️ Important Notes

1. **Don't overthink it!** Just send what you want to update
2. **Backend is smart** - it automatically handles all calculations
3. **Manual values override** - if you send `monthlyPayment`, backend uses it instead of calculating
4. **No manual value?** - backend calculates it for you
5. **Always fetch updated data** - after update, get the loan again to show accurate values

---

## 🐛 Debugging

If values aren't updating:
1. Check Visual Studio Output window for `[UPDATE]` logs
2. Verify you're sending the correct field names (camelCase)
3. Make sure values are numbers, not strings
4. Check if you're fetching the updated loan data after the PUT request

---

## 📝 Valid Status Values

```javascript
const LOAN_STATUSES = {
  PENDING: 'PENDING',
  APPROVED: 'APPROVED',
  REJECTED: 'REJECTED',
  DISBURSED: 'DISBURSED',
  ACTIVE: 'ACTIVE',
  COMPLETED: 'COMPLETED',
  CANCELLED: 'CANCELLED',
  DEFAULTED: 'DEFAULTED'
};
```

---

## ✅ Quick Reference

| What You Send | What Backend Does |
|--------------|-------------------|
| `interestRate` only | Calculates `monthlyPayment`, `totalAmount`, `remainingBalance` |
| `interestRate` + `monthlyPayment` | Uses your `monthlyPayment`, calculates `totalAmount`, `remainingBalance` |
| `interestRate` + `monthlyPayment` + `remainingBalance` | Uses all your values, calculates only `totalAmount` |
| `status` only | Updates status, leaves financials unchanged |
| Nothing | Returns error (at least one field required) |

---

## 🎯 Summary for Frontend Team

**TL;DR:**
- Send what you want to update
- Backend automatically calculates everything else
- For most users: Just send `interestRate`
- For power users: Send `interestRate` + manual values
- Always fetch updated loan after update
- Check debug logs if something seems wrong

**Need help?** Check the Visual Studio Output window for `[UPDATE]` logs! 🚀

