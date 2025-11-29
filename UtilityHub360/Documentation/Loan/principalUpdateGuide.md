# 💰 Principal Update Feature - Complete Guide

## ✅ Feature Implemented: Update Loan Principal Amount

You can now update the **Principal** (loan amount) along with other loan details. The backend automatically recalculates all financial values when the principal changes.

---

## 🎯 How It Works

### **Automatic Recalculation**

When you update the principal, the system:
1. ✅ Updates the principal amount
2. ✅ Recalculates monthly payment (unless you provide a manual value)
3. ✅ Recalculates total amount
4. ✅ Recalculates remaining balance (preserves payment history)

---

## 📋 **Request Examples**

### **Example 1: Update Principal Only (Auto-Calculate Everything)**

**SCENARIO:** Increase loan from $50,000 to $60,000

**REQUEST:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "principal": 60000
}
```

**BEFORE:**
```json
{
  "principal": 50000,
  "interestRate": 5.5,
  "term": 60,
  "monthlyPayment": 950.50,
  "totalAmount": 57030.00,
  "remainingBalance": 57030.00
}
```

**AFTER:**
```json
{
  "principal": 60000,              // ← UPDATED
  "interestRate": 5.5,             // ← UNCHANGED
  "term": 60,                      // ← UNCHANGED
  "monthlyPayment": 1140.60,       // ← RECALCULATED (60k at 5.5%)
  "totalAmount": 68436.00,         // ← RECALCULATED
  "remainingBalance": 68436.00     // ← RECALCULATED
}
```

**DEBUG LOGS:**
```
[UPDATE] Principal: 50000 -> 60000
[UPDATE] Monthly Payment (Calculated): 1140.60
[UPDATE] Total Amount: 68436.00
[UPDATE] Remaining Balance (No Payments): 68436.00
```

---

### **Example 2: Update Principal + Interest Rate**

**SCENARIO:** Change loan amount and interest rate together

**REQUEST:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "principal": 60000,
  "interestRate": 4.5
}
```

**RESPONSE:**
```json
{
  "principal": 60000,              // ← UPDATED
  "interestRate": 4.5,             // ← UPDATED
  "term": 60,
  "monthlyPayment": 1118.70,       // ← RECALCULATED (60k at 4.5%)
  "totalAmount": 67122.00,         // ← RECALCULATED
  "remainingBalance": 67122.00     // ← RECALCULATED
}
```

**DEBUG LOGS:**
```
[UPDATE] Principal: 50000 -> 60000
[UPDATE] Interest Rate: 4.5%
[UPDATE] Monthly Payment (Calculated): 1118.70
[UPDATE] Total Amount: 67122.00
[UPDATE] Remaining Balance (No Payments): 67122.00
```

---

### **Example 3: Update Principal with Manual Monthly Payment**

**SCENARIO:** Change principal but set custom monthly payment

**REQUEST:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "principal": 60000,
  "monthlyPayment": 1200
}
```

**RESPONSE:**
```json
{
  "principal": 60000,              // ← UPDATED
  "interestRate": 5.5,             // ← UNCHANGED
  "term": 60,
  "monthlyPayment": 1200.00,       // ← MANUAL (not calculated)
  "totalAmount": 72000.00,         // ← RECALCULATED (1200 × 60)
  "remainingBalance": 72000.00     // ← RECALCULATED
}
```

**DEBUG LOGS:**
```
[UPDATE] Principal: 50000 -> 60000
[UPDATE] Monthly Payment (Manual): 1200
[UPDATE] Total Amount: 72000.00
[UPDATE] Remaining Balance (No Payments): 72000.00
```

---

### **Example 4: Loan with Payments Already Made**

**SCENARIO:** Update principal on a loan that has active payments

**BEFORE:**
```json
{
  "principal": 50000,
  "interestRate": 5.5,
  "term": 60,
  "monthlyPayment": 950.50,
  "totalAmount": 57030.00,
  "remainingBalance": 47030.00     // ← $10,000 already paid
}
```

**REQUEST:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "principal": 60000
}
```

**RESPONSE:**
```json
{
  "principal": 60000,              // ← UPDATED
  "interestRate": 5.5,
  "term": 60,
  "monthlyPayment": 1140.60,       // ← RECALCULATED
  "totalAmount": 68436.00,         // ← RECALCULATED
  "remainingBalance": 58436.00     // ← RECALCULATED (preserved $10k paid)
}
```

**CALCULATION BREAKDOWN:**
```
Old Situation:
  Total Amount: $57,030
  Remaining: $47,030
  Paid: $10,000

New Situation:
  Total Amount: $68,436 (new calculation)
  Paid Amount: $10,000 (preserved!)
  New Remaining: $68,436 - $10,000 = $58,436
```

**DEBUG LOGS:**
```
[UPDATE] Principal: 50000 -> 60000
[UPDATE] Monthly Payment (Calculated): 1140.60
[UPDATE] Total Amount: 68436.00
[UPDATE] Remaining Balance (After 10000 paid): 58436.00
```

---

### **Example 5: Full Update (Everything at Once)**

**SCENARIO:** Update all financial values together

**REQUEST:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "principal": 60000,
  "interestRate": 4.0,
  "monthlyPayment": 1100,
  "remainingBalance": 66000,
  "status": "APPROVED"
}
```

**RESPONSE:**
```json
{
  "principal": 60000,              // ← UPDATED (manual)
  "interestRate": 4.0,             // ← UPDATED (manual)
  "term": 60,
  "purpose": "Home Renovation",
  "status": "APPROVED",            // ← UPDATED (manual)
  "monthlyPayment": 1100.00,       // ← UPDATED (manual)
  "totalAmount": 66000.00,         // ← CALCULATED (1100 × 60)
  "remainingBalance": 66000.00     // ← UPDATED (manual)
}
```

**DEBUG LOGS:**
```
[UPDATE] Principal: 50000 -> 60000
[UPDATE] Interest Rate: 4.0%
[UPDATE] Monthly Payment (Manual): 1100
[UPDATE] Total Amount: 66000.00
[UPDATE] Remaining Balance (Manual): 66000.00
```

---

## 🎨 **Frontend Implementation**

### **Simple Update (Auto-Calculate)**
```javascript
// User changes principal, backend calculates everything else
const updateLoanPrincipal = async (loanId, newPrincipal) => {
  const response = await axios.put(`/api/Loans/${loanId}`, {
    principal: newPrincipal
  });
  
  if (response.data.success) {
    const updatedLoan = response.data.data;
    console.log('New monthly payment:', updatedLoan.monthlyPayment);
    console.log('New total amount:', updatedLoan.totalAmount);
    return updatedLoan;
  }
};

// Usage
await updateLoanPrincipal('loan-id', 60000);
```

### **Update with Manual Values**
```javascript
// User sets principal and custom monthly payment
const updateLoanCustom = async (loanId, principal, monthlyPayment) => {
  const response = await axios.put(`/api/Loans/${loanId}`, {
    principal: principal,
    monthlyPayment: monthlyPayment
  });
  
  return response.data.data;
};

// Usage
await updateLoanCustom('loan-id', 60000, 1200);
```

### **React Component Example**
```jsx
const LoanPrincipalEditor = ({ loan, onUpdate }) => {
  const [principal, setPrincipal] = useState(loan.principal);
  const [autoCalculate, setAutoCalculate] = useState(true);
  const [monthlyPayment, setMonthlyPayment] = useState(loan.monthlyPayment);
  const [loading, setLoading] = useState(false);

  const handleUpdate = async () => {
    setLoading(true);
    try {
      const payload = { principal };
      
      // Only include monthly payment if NOT auto-calculating
      if (!autoCalculate) {
        payload.monthlyPayment = monthlyPayment;
      }

      const response = await axios.put(`/api/Loans/${loan.id}`, payload);
      
      if (response.data.success) {
        onUpdate(response.data.data);
        alert('Loan updated successfully!');
      }
    } catch (error) {
      alert('Failed to update loan: ' + error.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h3>Update Loan Amount</h3>
      
      <label>Principal Amount</label>
      <input
        type="number"
        value={principal}
        onChange={(e) => setPrincipal(parseFloat(e.target.value))}
        min="0.01"
        step="100"
      />
      
      <label>
        <input
          type="checkbox"
          checked={autoCalculate}
          onChange={(e) => setAutoCalculate(e.target.checked)}
        />
        Auto-calculate monthly payment
      </label>
      
      {!autoCalculate && (
        <>
          <label>Monthly Payment</label>
          <input
            type="number"
            value={monthlyPayment}
            onChange={(e) => setMonthlyPayment(parseFloat(e.target.value))}
            min="0"
            step="10"
          />
        </>
      )}
      
      <button onClick={handleUpdate} disabled={loading}>
        {loading ? 'Updating...' : 'Update Loan'}
      </button>
      
      {autoCalculate && (
        <p className="help-text">
          Monthly payment will be automatically calculated based on the new principal amount, 
          current interest rate ({loan.interestRate}%), and term ({loan.term} months).
        </p>
      )}
    </div>
  );
};
```

---

## ⚠️ **Important Considerations**

### **1. Payment History Preservation**
When you update the principal on a loan with existing payments:
- ✅ The system **preserves** the amount already paid
- ✅ Recalculates remaining balance accordingly
- ✅ Maintains accurate payment tracking

### **2. Validation**
- ❌ Principal must be greater than 0
- ❌ Principal cannot be negative
- ✅ Any positive decimal value is accepted

### **3. Impact on Existing Payments**
- Updates **do not** affect historical payment records
- Historical payments remain in the database
- Only the **remaining balance** is recalculated

### **4. Status Considerations**
- Works with any loan status (PENDING, APPROVED, ACTIVE, etc.)
- Consider business rules: Should principal be updatable once DISBURSED?
- You may want to add status restrictions based on your business needs

---

## 🔄 **Complete Update Flow**

```
1. User changes principal in frontend
   ↓
2. Frontend sends PUT request with new principal
   ↓
3. Backend receives request
   ↓
4. Authentication check (valid JWT token)
   ↓
5. Authorization check (user owns loan or is ADMIN)
   ↓
6. Update principal value
   ↓
7. Check if manual monthly payment provided
   ├─ YES → Use manual value
   └─ NO → Calculate using formula
   ↓
8. Recalculate total amount (monthlyPayment × term)
   ↓
9. Check if manual remaining balance provided
   ├─ YES → Use manual value
   └─ NO → Recalculate (preserve paid amount)
   ↓
10. Update timestamp
   ↓
11. Save to database
   ↓
12. Return updated loan data to frontend
   ↓
13. Frontend updates UI with new values
```

---

## 📊 **Calculation Formula**

### **Monthly Payment Calculation:**
```
If interest rate = 0:
  monthlyPayment = principal / term

If interest rate > 0:
  monthlyInterestRate = (interestRate / 100) / 12
  monthlyPayment = principal × [monthlyInterestRate × (1 + monthlyInterestRate)^term] 
                               / [(1 + monthlyInterestRate)^term - 1]
```

### **Total Amount:**
```
totalAmount = monthlyPayment × term
```

### **Remaining Balance (with payments):**
```
paidAmount = oldTotalAmount - oldRemainingBalance
newRemainingBalance = newTotalAmount - paidAmount
```

---

## 🧪 **Testing Scenarios**

### Test 1: Basic Principal Update
```bash
PUT /api/Loans/{loanId}
{ "principal": 60000 }

Expected: All values recalculated automatically
```

### Test 2: Principal + Interest Rate
```bash
PUT /api/Loans/{loanId}
{ "principal": 60000, "interestRate": 4.5 }

Expected: Both updated, monthly payment recalculated
```

### Test 3: Principal + Manual Payment
```bash
PUT /api/Loans/{loanId}
{ "principal": 60000, "monthlyPayment": 1200 }

Expected: Manual payment used, not calculated
```

### Test 4: Loan with Payments
```bash
# Loan has $10k already paid
PUT /api/Loans/{loanId}
{ "principal": 60000 }

Expected: $10k paid amount preserved in calculation
```

### Test 5: Validation Error
```bash
PUT /api/Loans/{loanId}
{ "principal": -5000 }

Expected: 400 Bad Request (validation error)
```

---

## 🎯 **Quick Reference**

| You Send | Backend Does |
|----------|-------------|
| `principal` only | Updates principal, recalculates monthly payment, total, remaining balance |
| `principal` + `interestRate` | Updates both, recalculates monthly payment, total, remaining balance |
| `principal` + `monthlyPayment` | Updates both, uses manual payment, recalculates total and remaining |
| `principal` + all values | Updates all manually, only recalculates total amount |

---

## ✅ **Summary**

- ✅ Principal can now be updated via PUT `/api/Loans/{loanId}`
- ✅ Automatic recalculation of all dependent values
- ✅ Manual override support for custom values
- ✅ Payment history is preserved
- ✅ Comprehensive debug logging
- ✅ Works with existing payment tracking

**Ready to use! Restart your application and test it!** 🚀

