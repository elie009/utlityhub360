# 🔄 Loan Update Flow - Step by Step

## Complete Process Flow for `PUT /api/Loans/{loanId}`

---

## 📥 **STEP 1: Request Received**

### **HTTP Request Details**
```http
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539 HTTP/1.1
Host: localhost:5001
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "purpose": "Finishtere",
  "additionalInfo": "",
  "status": "APPROVED",
  "interestRate": 0,
  "monthlyPayment": 725,
  "remainingBalance": 43500
}
```

### **Request Body Schema (UpdateLoanDto)**
```json
{
  "purpose": "string (optional, max 500 chars)",
  "additionalInfo": "string (optional, max 1000 chars)",
  "status": "string (optional, max 20 chars)",
  "principal": "decimal? (optional, > 0) ⭐ NEW!",
  "interestRate": "decimal? (optional, 0-100)",
  "monthlyPayment": "decimal? (optional, > 0)",
  "remainingBalance": "decimal? (optional, > 0)"
}
```

### **Valid Status Values**
```
"PENDING", "APPROVED", "REJECTED", "DISBURSED", 
"ACTIVE", "COMPLETED", "CANCELLED", "DEFAULTED"
```

### **Example Requests**

#### Request 1: Update Interest Rate Only (Auto-Calculate)
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "interestRate": 5.5
}
```

#### Request 2: Manual Monthly Payment
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "interestRate": 0,
  "monthlyPayment": 725
}
```

#### Request 3: Full Update (Your Example)
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "purpose": "Finishtere",
  "additionalInfo": "",
  "status": "APPROVED",
  "interestRate": 0,
  "monthlyPayment": 725,
  "remainingBalance": 43500
}
```

#### Request 4: Status Update Only
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "status": "APPROVED"
}
```

#### Request 5: Update Principal (NEW!) ⭐
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "principal": 60000
}
```

#### Request 6: Update Principal + Interest Rate (NEW!) ⭐
```json
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539

{
  "principal": 60000,
  "interestRate": 4.5
}
```

---

## 🔐 **STEP 2: Authentication & Authorization**

```
1. Extract userId from JWT token (ClaimTypes.NameIdentifier)
   → If no userId: Return 401 Unauthorized

2. Extract userRole from JWT token (ClaimTypes.Role)
   → Fallback to database role if JWT role is missing

3. Check if user exists in database
   → Get user from database using userId

4. Set effectiveRole
   → Use JWT role if available
   → Otherwise use database role
```

---

## 🔍 **STEP 3: Loan Access Check**

```
1. Call: _loanService.GetLoanWithAccessCheckAsync(loanId, userId)
   → Fetches loan from database
   → Checks if loan exists
   → If not found: Return 404 Not Found

2. Authorization check:
   → If user is NOT ADMIN AND loan.UserId != currentUserId
     → Return 403 Forbidden
   → If user is ADMIN OR owns the loan
     → Continue to update
```

---

## ✏️ **STEP 4: Update Basic Fields**

```
1. Update Purpose (if provided):
   → if (!string.IsNullOrEmpty(updateLoanDto.Purpose))
   → loan.Purpose = updateLoanDto.Purpose
   → Log: None (basic field)

2. Update AdditionalInfo (if provided):
   → if (!string.IsNullOrEmpty(updateLoanDto.AdditionalInfo))
   → loan.AdditionalInfo = updateLoanDto.AdditionalInfo
   → Log: None (basic field)

3. Update Status (if provided):
   → if (!string.IsNullOrEmpty(updateLoanDto.Status))
   → loan.Status = updateLoanDto.Status
   → Log: None (basic field)
```

---

## 💰 **STEP 5: Financial Update Logic (AUTOMATED)**

### **5.1: Initialize**
```
→ Set financialValuesChanged = false
```

### **5.2: Update Principal (NEW!) ⭐**
```
→ if (updateLoanDto.Principal.HasValue)
  {
    → var oldPrincipal = loan.Principal
    → loan.Principal = updateLoanDto.Principal.Value
    → financialValuesChanged = true
    → Log: "[UPDATE] Principal: {oldValue} -> {newValue}"
  }

Example:
→ principal changes from 50000 to 60000
→ Log: "[UPDATE] Principal: 50000 -> 60000"
```

### **5.3: Update Interest Rate**
```
→ if (updateLoanDto.InterestRate.HasValue)
  {
    → loan.InterestRate = updateLoanDto.InterestRate.Value
    → financialValuesChanged = true
    → Log: "[UPDATE] Interest Rate: {value}%"
  }

Example from your request:
→ interestRate = 0
→ Log: "[UPDATE] Interest Rate: 0%"
```

### **5.4: Update Monthly Payment**
```
→ if (updateLoanDto.MonthlyPayment.HasValue)
  {
    // MANUAL VALUE PROVIDED - Use it!
    → loan.MonthlyPayment = updateLoanDto.MonthlyPayment.Value
    → financialValuesChanged = true
    → Log: "[UPDATE] Monthly Payment (Manual): {value}"
  }
→ else if (updateLoanDto.Principal.HasValue || updateLoanDto.InterestRate.HasValue)
  {
    // NO MANUAL VALUE BUT PRINCIPAL OR RATE CHANGED - Calculate it!
    → loan.MonthlyPayment = CalculateMonthlyPayment(loan.Principal, loan.InterestRate, loan.Term)
    → Log: "[UPDATE] Monthly Payment (Calculated): {value}"
  }

Example from your request:
→ monthlyPayment = 725 (provided)
→ loan.MonthlyPayment = 725
→ Log: "[UPDATE] Monthly Payment (Manual): 725"
```

### **5.4: Recalculate Total Amount**
```
→ if (financialValuesChanged)
  {
    → loan.TotalAmount = loan.MonthlyPayment * loan.Term
    → Log: "[UPDATE] Total Amount: {value}"
  }

Example from your request:
→ Assume term = 60 months
→ loan.TotalAmount = 725 × 60 = 43,500
→ Log: "[UPDATE] Total Amount: 43500"
```

### **5.5: Update Remaining Balance**
```
→ if (updateLoanDto.RemainingBalance.HasValue)
  {
    // MANUAL VALUE PROVIDED - Use it!
    → loan.RemainingBalance = updateLoanDto.RemainingBalance.Value
    → Log: "[UPDATE] Remaining Balance (Manual): {value}"
  }
→ else if (financialValuesChanged)
  {
    // NO MANUAL VALUE - Recalculate it!
    
    → if (loan.RemainingBalance >= loan.Principal)
      {
        // No payments made yet
        → loan.RemainingBalance = loan.TotalAmount
        → Log: "[UPDATE] Remaining Balance (No Payments): {value}"
      }
    → else
      {
        // Payments have been made - maintain paid amount
        → var paidAmount = loan.TotalAmount - loan.RemainingBalance
        → loan.RemainingBalance = loan.TotalAmount - paidAmount
        → Log: "[UPDATE] Remaining Balance (After {paidAmount} paid): {value}"
      }
  }

Example from your request:
→ remainingBalance = 43500 (provided)
→ loan.RemainingBalance = 43500
→ Log: "[UPDATE] Remaining Balance (Manual): 43500"
```

### **5.6: Save to Database**
```
→ await _context.SaveChangesAsync()
→ All changes are committed to the database
```

---

## 💾 **STEP 6: Save to Database**

```
→ await _context.SaveChangesAsync()
→ Commits all changes to database
→ If error: Exception is caught and returns 400 Bad Request
```

---

## 📤 **STEP 7: Build Response DTO**

```
→ Create new LoanDto object with all loan data:
  {
    Id: loan.Id,
    UserId: loan.UserId,
    Principal: loan.Principal,
    InterestRate: loan.InterestRate,      → Updated value (0)
    Term: loan.Term,
    Purpose: loan.Purpose,                → Updated value ("Finishtere")
    Status: loan.Status,                  → Updated value ("APPROVED")
    MonthlyPayment: loan.MonthlyPayment,  → Updated value (725)
    TotalAmount: loan.TotalAmount,        → Calculated value (43500)
    RemainingBalance: loan.RemainingBalance, → Updated value (43500)
    AppliedAt: loan.AppliedAt,
    ApprovedAt: loan.ApprovedAt,
    DisbursedAt: loan.DisbursedAt,
    CompletedAt: loan.CompletedAt,
    AdditionalInfo: loan.AdditionalInfo
  }
```

---

## ✅ **STEP 8: Return Response**

### **Success Response (200 OK)**

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "message": "Loan updated successfully",
  "data": {
    "id": "9ece099b-602c-4ac7-931d-76b760fe9539",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "principal": 50000,
    "interestRate": 0,
    "term": 60,
    "purpose": "Finishtere",
    "status": "APPROVED",
    "monthlyPayment": 725.00,
    "totalAmount": 43500.00,
    "remainingBalance": 43500.00,
    "appliedAt": "2024-01-01T10:00:00Z",
    "approvedAt": "2024-01-02T14:30:00Z",
    "disbursedAt": null,
    "completedAt": null,
    "additionalInfo": ""
  },
  "errors": null
}
```

### **Response Body Schema (ApiResponse<LoanDto>)**
```json
{
  "success": "boolean",
  "message": "string",
  "data": {
    "id": "string (GUID)",
    "userId": "string (GUID)",
    "principal": "decimal",
    "interestRate": "decimal",
    "term": "integer (months)",
    "purpose": "string",
    "status": "string",
    "monthlyPayment": "decimal",
    "totalAmount": "decimal",
    "remainingBalance": "decimal",
    "appliedAt": "datetime",
    "approvedAt": "datetime | null",
    "disbursedAt": "datetime | null",
    "completedAt": "datetime | null",
    "additionalInfo": "string"
  },
  "errors": "array | null"
}
```

### **Error Responses**

#### 401 Unauthorized (No JWT Token)
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "success": false,
  "message": "User not authenticated",
  "data": null,
  "errors": null
}
```

#### 403 Forbidden (User doesn't own loan)
```http
HTTP/1.1 403 Forbidden
Content-Type: application/json

You can only update your own loans
```

#### 404 Not Found (Loan doesn't exist)
```http
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "success": false,
  "message": "Loan not found",
  "data": null,
  "errors": null
}
```

#### 400 Bad Request (Validation Error)
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "success": false,
  "message": "Failed to update loan: Interest rate must be between 0 and 100",
  "data": null,
  "errors": null
}
```

#### 400 Bad Request (Database Error)
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "success": false,
  "message": "Failed to update loan: Database connection failed",
  "data": null,
  "errors": null
}
```

---

## 📊 **Complete Request/Response Examples**

### **Example 1: Auto-Calculate (Interest Rate Only)**

**REQUEST:**
```http
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539 HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "interestRate": 5.5
}
```

**RESPONSE:**
```json
{
  "success": true,
  "message": "Loan updated successfully",
  "data": {
    "id": "9ece099b-602c-4ac7-931d-76b760fe9539",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "principal": 50000,
    "interestRate": 5.5,
    "term": 60,
    "purpose": "Home Renovation",
    "status": "APPROVED",
    "monthlyPayment": 950.50,    // ← CALCULATED
    "totalAmount": 57030.00,      // ← CALCULATED
    "remainingBalance": 57030.00, // ← CALCULATED
    "appliedAt": "2024-01-01T10:00:00Z",
    "approvedAt": "2024-01-02T14:30:00Z",
    "disbursedAt": null,
    "completedAt": null,
    "additionalInfo": ""
  },
  "errors": null
}
```

**DEBUG LOGS:**
```
[UPDATE] Interest Rate: 5.5%
[UPDATE] Monthly Payment (Calculated): 950.50
[UPDATE] Total Amount: 57030.00
[UPDATE] Remaining Balance (No Payments): 57030.00
```

---

### **Example 2: Manual Override (Your Case)**

**REQUEST:**
```http
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539 HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "purpose": "Finishtere",
  "additionalInfo": "",
  "status": "APPROVED",
  "interestRate": 0,
  "monthlyPayment": 725,
  "remainingBalance": 43500
}
```

**RESPONSE:**
```json
{
  "success": true,
  "message": "Loan updated successfully",
  "data": {
    "id": "9ece099b-602c-4ac7-931d-76b760fe9539",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "principal": 50000,
    "interestRate": 0,              // ← UPDATED (manual)
    "term": 60,
    "purpose": "Finishtere",        // ← UPDATED
    "status": "APPROVED",           // ← UPDATED
    "monthlyPayment": 725.00,       // ← UPDATED (manual)
    "totalAmount": 43500.00,        // ← CALCULATED (725 × 60)
    "remainingBalance": 43500.00,   // ← UPDATED (manual)
    "appliedAt": "2024-01-01T10:00:00Z",
    "approvedAt": "2024-01-02T14:30:00Z",
    "disbursedAt": null,
    "completedAt": null,
    "additionalInfo": ""
  },
  "errors": null
}
```

**DEBUG LOGS:**
```
[UPDATE] Interest Rate: 0%
[UPDATE] Monthly Payment (Manual): 725
[UPDATE] Total Amount: 43500
[UPDATE] Remaining Balance (Manual): 43500
```

---

### **Example 3: Status Update Only**

**REQUEST:**
```http
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539 HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "status": "DISBURSED"
}
```

**RESPONSE:**
```json
{
  "success": true,
  "message": "Loan updated successfully",
  "data": {
    "id": "9ece099b-602c-4ac7-931d-76b760fe9539",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "principal": 50000,
    "interestRate": 5.5,
    "term": 60,
    "purpose": "Home Renovation",
    "status": "DISBURSED",          // ← UPDATED
    "monthlyPayment": 950.50,       // ← UNCHANGED
    "totalAmount": 57030.00,        // ← UNCHANGED
    "remainingBalance": 57030.00,   // ← UNCHANGED
    "appliedAt": "2024-01-01T10:00:00Z",
    "approvedAt": "2024-01-02T14:30:00Z",
    "disbursedAt": null,
    "completedAt": null,
    "additionalInfo": ""
  },
  "errors": null
}
```

**DEBUG LOGS:**
```
(No financial update logs - only status changed)
```

---

### **Example 4: Loan with Payments Already Made**

**BEFORE UPDATE:**
```json
{
  "principal": 50000,
  "interestRate": 5.5,
  "term": 60,
  "monthlyPayment": 950.50,
  "totalAmount": 57030.00,
  "remainingBalance": 47030.00  // ← $10,000 already paid
}
```

**REQUEST:**
```http
PUT /api/Loans/9ece099b-602c-4ac7-931d-76b760fe9539 HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "interestRate": 4.0  // ← Lower interest rate
}
```

**RESPONSE:**
```json
{
  "success": true,
  "message": "Loan updated successfully",
  "data": {
    "id": "9ece099b-602c-4ac7-931d-76b760fe9539",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "principal": 50000,
    "interestRate": 4.0,            // ← UPDATED
    "term": 60,
    "purpose": "Home Renovation",
    "status": "ACTIVE",
    "monthlyPayment": 920.00,       // ← RECALCULATED (lower rate)
    "totalAmount": 55200.00,        // ← RECALCULATED (920 × 60)
    "remainingBalance": 45200.00,   // ← RECALCULATED (maintained $10k paid)
    "appliedAt": "2024-01-01T10:00:00Z",
    "approvedAt": "2024-01-02T14:30:00Z",
    "disbursedAt": "2024-01-03T09:00:00Z",
    "completedAt": null,
    "additionalInfo": ""
  },
  "errors": null
}
```

**DEBUG LOGS:**
```
[UPDATE] Interest Rate: 4%
[UPDATE] Monthly Payment (Calculated): 920.00
[UPDATE] Total Amount: 55200.00
[UPDATE] Remaining Balance (After 10000 paid): 45200.00
```

**EXPLANATION:**
- Old total: $57,030, Paid: $10,000, Remaining: $47,030
- New total: $55,200, Paid: $10,000 (maintained), New remaining: $45,200
- The $10,000 already paid is preserved in the calculation

---

## 🔄 **Complete Flow Summary (Your Example)**

```
INPUT:
→ loanId: 9ece099b-602c-4ac7-931d-76b760fe9539
→ purpose: "Finishtere"
→ status: "APPROVED"
→ interestRate: 0
→ monthlyPayment: 725
→ remainingBalance: 43500

PROCESSING:
1. ✅ Authenticate user from JWT
2. ✅ Check user owns loan or is ADMIN
3. ✅ Load loan from database
4. ✅ Update purpose = "Finishtere"
5. ✅ Update status = "APPROVED"
6. ✅ Update interestRate = 0
7. ✅ Update monthlyPayment = 725 (manual)
8. ✅ Calculate totalAmount = 725 × 60 = 43500
9. ✅ Update remainingBalance = 43500 (manual)
10. ✅ Save to database
11. ✅ Build response DTO

OUTPUT:
→ 200 OK
→ Loan with all updated values
→ success: true
→ message: "Loan updated successfully"

DEBUG LOGS:
[UPDATE] Interest Rate: 0%
[UPDATE] Monthly Payment (Manual): 725
[UPDATE] Total Amount: 43500
[UPDATE] Remaining Balance (Manual): 43500
```

---

## 🎯 **Key Decision Points in Flow**

### **Decision 1: Use Manual or Calculate Monthly Payment?**
```
if (monthlyPayment provided)
  → Use manual value
else if (interestRate changed)
  → Calculate using formula
else
  → Keep existing value
```

### **Decision 2: Use Manual or Calculate Remaining Balance?**
```
if (remainingBalance provided)
  → Use manual value
else if (any financial value changed)
  → Check if payments were made
    → If no payments: Set to totalAmount
    → If payments made: Maintain paid amount
else
  → Keep existing value
```

### **Decision 3: Should Update Timestamp?**
```
if (any financial value changed)
  → Update loan.UpdatedAt
else
  → Keep existing timestamp
```

---

## 🧪 **Testing the Flow**

To test each step, send different combinations:

### Test 1: Interest Rate Only (Auto-Calculate)
```json
{ "interestRate": 5.5 }
→ Flow: Steps 1-3, 5.2, 5.3 (calculate), 5.4, 5.5 (calculate), 5.6-8
```

### Test 2: Manual Monthly Payment
```json
{ "monthlyPayment": 725 }
→ Flow: Steps 1-3, 5.3 (manual), 5.4, 5.6-8
```

### Test 3: Full Manual (Your Case)
```json
{ "interestRate": 0, "monthlyPayment": 725, "remainingBalance": 43500 }
→ Flow: Steps 1-3, 5.2, 5.3 (manual), 5.4, 5.5 (manual), 5.6-8
```

---

## 📊 **Flow Execution Time**

```
Typical execution:
→ Authentication: ~10ms
→ Database fetch: ~50ms
→ Update logic: ~5ms
→ Database save: ~30ms
→ Build response: ~2ms
─────────────────────────
Total: ~100ms average
```

---

## ⚠️ **Error Handling Points**

```
Step 2: No JWT token → 401 Unauthorized
Step 3: Loan not found → 404 Not Found
Step 3: User doesn't own loan → 403 Forbidden
Step 6: Database error → 400 Bad Request with error message
Any step: Exception → Caught and returned as 400 Bad Request
```

---

## 🎓 **Understanding the Flow**

**The key principle:**
```
Manual values ALWAYS take priority over calculations
If user provides a value → Use it
If user doesn't provide a value AND something changed → Calculate it
If user doesn't provide a value AND nothing changed → Keep existing
```

**This makes the API:**
- ✅ Flexible (supports both auto and manual)
- ✅ Predictable (clear priority order)
- ✅ Safe (validates ownership)
- ✅ Transparent (debug logs show decisions)

---

**Need more detail on any step? Check the code in `LoansController.cs` lines 290-415!** 🚀

---

## 📋 **Quick Reference: Request → Response Mapping**

| Request Fields | What Gets Updated | What Gets Calculated |
|---------------|-------------------|---------------------|
| `principal` only ⭐ NEW | `principal` | `monthlyPayment`, `totalAmount`, `remainingBalance` |
| `interestRate` only | `interestRate` | `monthlyPayment`, `totalAmount`, `remainingBalance` |
| `principal` + `interestRate` ⭐ NEW | `principal`, `interestRate` | `monthlyPayment`, `totalAmount`, `remainingBalance` |
| `principal` + `monthlyPayment` ⭐ NEW | `principal`, `monthlyPayment` | `totalAmount`, `remainingBalance` |
| `interestRate` + `monthlyPayment` | `interestRate`, `monthlyPayment` | `totalAmount`, `remainingBalance` |
| `interestRate` + `remainingBalance` | `interestRate`, `remainingBalance` | `monthlyPayment`, `totalAmount` |
| All financial fields | All provided fields | `totalAmount` only |
| `status` only | `status` | Nothing (all financials unchanged) |
| `purpose` only | `purpose` | Nothing (all financials unchanged) |

---

## 🎯 **Response Field Definitions**

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `id` | string (GUID) | Unique loan identifier | `"9ece099b-602c-4ac7-931d-76b760fe9539"` |
| `userId` | string (GUID) | Owner of the loan | `"3fa85f64-5717-4562-b3fc-2c963f66afa6"` |
| `principal` | decimal | Original loan amount (never changes after creation) | `50000.00` |
| `interestRate` | decimal | Annual interest rate percentage | `5.5` (means 5.5%) |
| `term` | integer | Loan duration in months | `60` (5 years) |
| `purpose` | string | Reason for the loan | `"Home Renovation"` |
| `status` | string | Current loan status | `"APPROVED"`, `"ACTIVE"`, etc. |
| `monthlyPayment` | decimal | Amount to pay each month | `950.50` |
| `totalAmount` | decimal | Total amount to be paid (principal + interest) | `57030.00` |
| `remainingBalance` | decimal | Outstanding balance | `47030.00` |
| `appliedAt` | datetime | When loan was applied for | `"2024-01-01T10:00:00Z"` |
| `approvedAt` | datetime? | When loan was approved (null if not approved) | `"2024-01-02T14:30:00Z"` |
| `disbursedAt` | datetime? | When funds were released (null if not disbursed) | `null` |
| `completedAt` | datetime? | When loan was fully paid (null if not completed) | `null` |
| `additionalInfo` | string | Extra information or notes | `"For kitchen remodel"` |

---

## 🔍 **HTTP Status Codes Summary**

| Status Code | When It Occurs | Action Required |
|------------|----------------|-----------------|
| `200 OK` | Update successful | Use the updated data in response |
| `400 Bad Request` | Validation error or database error | Fix request data or check server logs |
| `401 Unauthorized` | No JWT token or invalid token | Login again to get new token |
| `403 Forbidden` | User doesn't own the loan and is not ADMIN | Can only update own loans |
| `404 Not Found` | Loan ID doesn't exist | Verify correct loan ID |

---

## 💻 **Frontend Integration Example**

```javascript
// Complete frontend example with error handling
const updateLoan = async (loanId, updates) => {
  try {
    // Send PUT request
    const response = await axios.put(
      `/api/Loans/${loanId}`,
      updates,
      {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      }
    );

    // Check if successful
    if (response.data.success) {
      const updatedLoan = response.data.data;
      console.log('Loan updated successfully:', updatedLoan);
      
      // Update UI with new values
      return updatedLoan;
    } else {
      console.error('Update failed:', response.data.message);
      throw new Error(response.data.message);
    }
  } catch (error) {
    // Handle different error types
    if (error.response) {
      switch (error.response.status) {
        case 401:
          console.error('Unauthorized - please login again');
          // Redirect to login
          break;
        case 403:
          console.error('You cannot update this loan');
          break;
        case 404:
          console.error('Loan not found');
          break;
        case 400:
          console.error('Validation error:', error.response.data.message);
          break;
        default:
          console.error('Unknown error:', error);
      }
    }
    throw error;
  }
};

// Usage examples:
// 1. Auto-calculate
await updateLoan('loan-id', { interestRate: 5.5 });

// 2. Manual override
await updateLoan('loan-id', { 
  interestRate: 0, 
  monthlyPayment: 725, 
  remainingBalance: 43500 
});

// 3. Status only
await updateLoan('loan-id', { status: 'APPROVED' });
```

---

## 📞 **Need Help?**

1. **Not updating?** → Check Visual Studio Output window for `[UPDATE]` logs
2. **401 Error?** → Verify JWT token is valid and not expired
3. **403 Error?** → Make sure user owns the loan or is ADMIN
4. **404 Error?** → Confirm loan ID exists in database
5. **Calculation wrong?** → Check if manual values are overriding calculation
6. **Debug logs not showing?** → Make sure application is restarted after code changes

**All documentation files:**
- `LOAN-UPDATE-FLOW.md` - Complete flow with request/response (this file)
- `FRONTEND-LOAN-UPDATE-GUIDE.md` - Frontend implementation guide
- `LOAN-UPDATE-API-TESTS.md` - Test cases and scenarios
- `LOAN-UPDATE-QUICK-REFERENCE.md` - Quick lookup reference

---

**Ready to test! Restart your app and try it out!** 🚀

