# 🧪 Loan Update API - Test Cases

## Quick Test Guide for `PUT /api/Loans/{loanId}`

---

## Test 1: Auto-Calculate (Interest Rate Only) ✅

**Scenario:** User changes interest rate, backend calculates monthly payment and remaining balance

**Request:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "interestRate": 5.5
}
```

**Expected Behavior:**
- ✅ `interestRate` = 5.5
- ✅ `monthlyPayment` = calculated automatically
- ✅ `totalAmount` = calculated automatically
- ✅ `remainingBalance` = calculated automatically

**Debug Output:**
```
[UPDATE] Interest Rate: 5.5%
[UPDATE] Monthly Payment (Calculated): XXX
[UPDATE] Total Amount: XXX
[UPDATE] Remaining Balance (No Payments): XXX
```

---

## Test 2: Manual Monthly Payment Override ✅

**Scenario:** User sets custom monthly payment

**Request:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "interestRate": 0,
  "monthlyPayment": 725
}
```

**Expected Behavior:**
- ✅ `interestRate` = 0
- ✅ `monthlyPayment` = 725 (manual value)
- ✅ `totalAmount` = 725 × term
- ✅ `remainingBalance` = calculated based on payments made

**Debug Output:**
```
[UPDATE] Interest Rate: 0%
[UPDATE] Monthly Payment (Manual): 725
[UPDATE] Total Amount: 43500
[UPDATE] Remaining Balance (No Payments): 43500
```

---

## Test 3: Manual Remaining Balance Override ✅

**Scenario:** User manually sets remaining balance

**Request:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "remainingBalance": 40000
}
```

**Expected Behavior:**
- ✅ `remainingBalance` = 40000 (manual value)
- ✅ All other values unchanged

**Debug Output:**
```
[UPDATE] Remaining Balance (Manual): 40000
```

---

## Test 4: Full Manual Override ✅

**Scenario:** User manually sets all financial values

**Request:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "interestRate": 0,
  "monthlyPayment": 725,
  "remainingBalance": 43500
}
```

**Expected Behavior:**
- ✅ `interestRate` = 0
- ✅ `monthlyPayment` = 725 (manual)
- ✅ `remainingBalance` = 43500 (manual)
- ✅ `totalAmount` = 725 × term (calculated)

**Debug Output:**
```
[UPDATE] Interest Rate: 0%
[UPDATE] Monthly Payment (Manual): 725
[UPDATE] Total Amount: 43500
[UPDATE] Remaining Balance (Manual): 43500
```

---

## Test 5: Status Update Only ✅

**Scenario:** User only updates loan status

**Request:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "status": "APPROVED"
}
```

**Expected Behavior:**
- ✅ `status` = "APPROVED"
- ✅ All financial values unchanged

**Debug Output:**
```
(No financial update logs)
```

---

## Test 6: Combined Status and Financial Update ✅

**Scenario:** Update both status and financial details

**Request:**
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "status": "APPROVED",
  "interestRate": 5.5,
  "monthlyPayment": 800
}
```

**Expected Behavior:**
- ✅ `status` = "APPROVED"
- ✅ `interestRate` = 5.5
- ✅ `monthlyPayment` = 800 (manual)
- ✅ `totalAmount` = calculated
- ✅ `remainingBalance` = calculated

---

## 🔍 How to Test

### Using Swagger UI:
1. Navigate to `https://localhost:5001/swagger`
2. Find `PUT /api/Loans/{loanId}`
3. Click "Try it out"
4. Enter loan ID
5. Copy/paste test JSON
6. Click "Execute"
7. Check response

### Using Postman:
1. Create new request: `PUT https://localhost:5001/api/Loans/{loanId}`
2. Headers: `Authorization: Bearer YOUR_JWT_TOKEN`
3. Body: Select "raw" → "JSON"
4. Paste test JSON
5. Click "Send"
6. Check response

### Using Frontend:
1. Open browser console (F12)
2. Make the API call using axios/fetch
3. Check Visual Studio Output window for debug logs
4. Verify response data

---

## 📊 Expected Response Format

```json
{
  "success": true,
  "message": "Loan updated successfully",
  "data": {
    "id": "9ece099b-602c-4ac7-931d-76b760fe9539",
    "userId": "user-id",
    "principal": 50000,
    "interestRate": 5.5,
    "term": 60,
    "purpose": "Home Renovation",
    "status": "APPROVED",
    "monthlyPayment": 950.12,
    "totalAmount": 57007.20,
    "remainingBalance": 57007.20,
    "appliedAt": "2024-01-01T00:00:00Z",
    "approvedAt": "2024-01-02T00:00:00Z",
    "disbursedAt": null,
    "completedAt": null,
    "additionalInfo": ""
  },
  "errors": null
}
```

---

## ⚠️ Common Issues & Solutions

### Issue 1: Values Not Updating
**Problem:** Sent request but values didn't change

**Solutions:**
- ✅ Check if you're fetching updated loan after PUT request
- ✅ Verify JSON field names are camelCase (not PascalCase)
- ✅ Check Visual Studio Output for `[UPDATE]` logs
- ✅ Make sure app is restarted after code changes

### Issue 2: Calculation Seems Wrong
**Problem:** Monthly payment doesn't match expected value

**Solutions:**
- ✅ Check if interest rate is annual (e.g., 5.5 means 5.5% per year)
- ✅ Verify term is in months (not years)
- ✅ Check if manual value is overriding calculation
- ✅ Review debug logs to see which calculation path was taken

### Issue 3: Manual Values Not Working
**Problem:** Sent manual values but backend calculated anyway

**Solutions:**
- ✅ Make sure you're sending numbers, not strings: `5.5` not `"5.5"`
- ✅ Check if value is `null` or `undefined` in request
- ✅ Verify field names match DTO exactly
- ✅ Check Visual Studio Output for what values were received

---

## 🐛 Debug Checklist

When testing, check these in Visual Studio Output window:

- [ ] `[UPDATE] Interest Rate: X%` - confirms interest rate received
- [ ] `[UPDATE] Monthly Payment (Manual): X` - confirms manual payment used
- [ ] `[UPDATE] Monthly Payment (Calculated): X` - confirms auto-calculation
- [ ] `[UPDATE] Total Amount: X` - confirms total recalculated
- [ ] `[UPDATE] Remaining Balance (Manual): X` - confirms manual balance used
- [ ] `[UPDATE] Remaining Balance (No Payments): X` - confirms auto-calculation for new loan
- [ ] `[UPDATE] Remaining Balance (After X paid): X` - confirms auto-calculation with payments

---

## ✅ Success Criteria

A successful update should:
1. Return `success: true` in response
2. Return updated loan data with all fields
3. Show `[UPDATE]` logs in Visual Studio Output
4. Have correct calculated values based on sent data
5. Persist changes (verify by fetching loan again)

---

## 📞 Need Help?

If tests fail:
1. Check Visual Studio Output window for errors
2. Verify authentication token is valid
3. Confirm loan ID exists and belongs to user
4. Review the debug logs to see what backend received
5. Compare request JSON with examples above

**Remember:** The backend is designed to be smart and handle calculations automatically. If something seems wrong, check the debug logs first! 🚀

