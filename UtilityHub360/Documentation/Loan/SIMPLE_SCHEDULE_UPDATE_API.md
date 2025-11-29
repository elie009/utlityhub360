# 🔄 Simple Schedule Update API - Complete Guide

## ✅ **Implementation Complete!**

You requested an API to **update amount, status, due date, and paid date for specific loan payment schedules** - **DONE!** ✅

## 📡 **API Endpoint**

### **Simple Schedule Update**
```http
PATCH /api/Loans/{loanId}/schedule/{installmentNumber}
```

**Updates any combination of:**
- ✅ **Amount** - Change installment amount
- ✅ **Status** - PENDING ↔ PAID ↔ OVERDUE  
- ✅ **Due Date** - Change when payment is due
- ✅ **Paid Date** - Set when payment was made

## 🚀 **Usage Examples**

### **1. Update Amount Only**
```http
PATCH /api/Loans/loan-123/schedule/3
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "amount": 900.00
}
```

### **2. Mark as Paid with Payment Details**
```http
PATCH /api/Loans/loan-123/schedule/3
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "status": "PAID",
  "paidDate": "2024-12-01T10:30:00Z",
  "paymentMethod": "CASH",
  "paymentReference": "CASH-001",
  "notes": "Paid in cash at office"
}
```

### **3. Change Due Date**
```http
PATCH /api/Loans/loan-123/schedule/3
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "dueDate": "2024-12-20T00:00:00Z"
}
```

### **4. Complete Update - All Fields**
```http
PATCH /api/Loans/loan-123/schedule/3
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "amount": 850.00,
  "status": "PAID",
  "dueDate": "2024-12-15T00:00:00Z",
  "paidDate": "2024-12-01T10:30:00Z",
  "paymentMethod": "BANK_TRANSFER",
  "paymentReference": "TXN-123456",
  "notes": "Updated amount and marked as paid"
}
```

### **5. Mark as Overdue**
```http
PATCH /api/Loans/loan-123/schedule/3
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "status": "OVERDUE"
}
```

### **6. Revert from Paid to Pending**
```http
PATCH /api/Loans/loan-123/schedule/3
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "status": "PENDING"
}
```

## 📋 **Request Body Fields**

### **SimpleScheduleUpdateDto**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | decimal? | ❌ Optional | New installment amount |
| `status` | string? | ❌ Optional | PENDING, PAID, or OVERDUE |
| `dueDate` | DateTime? | ❌ Optional | New due date |
| `paidDate` | DateTime? | ❌ Optional | Date when payment was made |
| `paymentMethod` | string? | ❌ Optional | Payment method for audit trail |
| `paymentReference` | string? | ❌ Optional | Payment reference for tracking |
| `notes` | string? | ❌ Optional | Additional notes |

**All fields are optional - only update what you need!**

## 📤 **Response Example**

```json
{
  "success": true,
  "message": "Payment schedule updated successfully",
  "data": {
    "id": "schedule-123",
    "loanId": "loan-456",
    "installmentNumber": 3,
    "dueDate": "2024-12-20T00:00:00Z",
    "principalAmount": 775.00,
    "interestAmount": 75.00,
    "totalAmount": 850.00,
    "status": "PAID",
    "paidAt": "2024-12-01T10:30:00Z"
  }
}
```

## 🔧 **What Happens Automatically**

### **When You Change Amount:**
- ✅ **Principal/Interest Split** - Maintains proportional ratio
- ✅ **Loan Total Update** - Adjusts total loan amount
- ✅ **Balance Adjustment** - Updates remaining balance if unpaid

### **When You Mark as PAID:**
- ✅ **Payment Record Created** - Audit trail with method/reference
- ✅ **Loan Balance Updated** - Reduces remaining balance
- ✅ **Auto-Completion** - Completes loan when all installments paid
- ✅ **Paid Timestamp** - Sets PaidAt automatically or uses your date

### **When You Mark as PENDING:**
- ✅ **Balance Restoration** - Adds amount back to remaining balance
- ✅ **Loan Reopening** - Reopens completed loan if needed
- ✅ **Timestamp Clearing** - Removes PaidAt timestamp

### **When You Change Due Date:**
- ✅ **Calendar Update** - Updates payment schedule timeline
- ✅ **No Financial Impact** - Doesn't affect amounts or balances

## 🎯 **Smart Business Logic**

### **Amount Changes:**
```json
// Original: $825 installment
// Update to: $900
{
  "amount": 900.00
}

// Result:
// - Principal: $775 → $825 (proportional increase)
// - Interest: $50 → $75 (proportional increase) 
// - Total: $825 → $900
// - Loan total increases by $75
// - If unpaid: remaining balance increases by $75
```

### **Status Changes with Automatic Payment Records:**
```json
// Mark as PAID with details
{
  "status": "PAID",
  "paymentMethod": "CASH",
  "paymentReference": "CASH-001"
}

// Automatically creates:
// Payment {
//   Amount: $825,
//   Method: "CASH", 
//   Reference: "CASH-001",
//   TransactionType: "SCHEDULE_UPDATE"
// }
```

### **Loan Completion Logic:**
- ✅ **Auto-Complete** - When all installments marked as PAID
- ✅ **Auto-Reopen** - When PAID installments changed to PENDING
- ✅ **Balance Tracking** - Always keeps loan balance accurate

## 🔐 **Security & Validation**

### **Authentication:**
- ✅ JWT token required
- ✅ Users can only update their own loans
- ✅ Admins can update any loan

### **Validation Rules:**
- ✅ Amount must be positive if provided
- ✅ Status must be PENDING, PAID, or OVERDUE
- ✅ Cannot set negative loan balances
- ✅ All fields are optional

### **Business Rules:**
- ✅ Proportional amount updates maintain principal/interest ratio
- ✅ Payment records created for audit when marking as PAID
- ✅ Loan totals automatically updated
- ✅ Loan status managed automatically (ACTIVE ↔ COMPLETED)

## 🚨 **Error Handling**

### **Common Errors:**

```json
// Invalid amount
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Amount must be greater than 0"]
}

// Loan not found
{
  "success": false,
  "message": "Loan not found"
}

// Installment not found
{
  "success": false,
  "message": "Payment installment not found"
}

// Unauthorized access
{
  "success": false,
  "message": "User not authenticated"
}
```

## 💡 **Use Cases**

### **1. Accounting Corrections**
```bash
# Fix incorrect amount
PATCH /api/Loans/loan-123/schedule/3
{ "amount": 825.00 }
```

### **2. Manual Payment Recording**
```bash
# Record cash payment
PATCH /api/Loans/loan-123/schedule/3
{
  "status": "PAID",
  "paidDate": "2024-12-01T15:30:00Z",
  "paymentMethod": "CASH",
  "notes": "Customer paid at office"
}
```

### **3. Payment Plan Adjustments**
```bash
# Extend due date for customer
PATCH /api/Loans/loan-123/schedule/3
{ "dueDate": "2024-12-30T00:00:00Z" }
```

### **4. Overdue Management**
```bash
# Mark as overdue
PATCH /api/Loans/loan-123/schedule/3
{ "status": "OVERDUE" }
```

### **5. Payment Reversals**
```bash
# Reverse incorrect payment
PATCH /api/Loans/loan-123/schedule/3
{
  "status": "PENDING",
  "notes": "Reversed incorrect payment - check was bounced"
}
```

## 🔄 **Integration with Other APIs**

### **Works Seamlessly With:**
- ✅ **GET** `/api/loans/{loanId}/schedule` - View updated schedule
- ✅ **POST** `/api/loans/{loanId}/schedule/{installmentNumber}/mark-paid` - Alternative payment method
- ✅ **GET** `/api/loans/{loanId}` - View updated loan totals
- ✅ **GET** `/api/loans/upcoming-payments` - Updated due dates reflected
- ✅ **GET** `/api/loans/overdue-payments` - Overdue status changes reflected

## 📊 **Comparison with Mark as Paid API**

| Feature | Simple Update (PATCH) | Mark as Paid (POST) |
|---------|----------------------|-------------------|
| **Purpose** | Flexible updates | Specific payment recording |
| **Fields** | Amount, Status, Dates | Amount, Method, Reference |
| **Use Case** | Administrative changes | Payment processing |
| **Payment Record** | Optional (when marking PAID) | Always created |
| **Flexibility** | High - any field combination | Medium - payment-focused |
| **Validation** | Lighter | Stricter payment validation |

## ✅ **Ready to Use!**

Your Simple Schedule Update API is **fully implemented and production-ready**:

1. ✅ **Complete Implementation** - All code added and tested
2. ✅ **Flexible Updates** - Any combination of fields
3. ✅ **Smart Business Logic** - Automatic calculations and updates
4. ✅ **Comprehensive Validation** - Error handling and security
5. ✅ **Audit Trail** - Payment records for PAID status changes
6. ✅ **Documentation Complete** - Full API guide and examples

## 🧪 **Test It Now**

```bash
# Quick test - change amount
curl -X PATCH "http://localhost:5001/api/Loans/your-loan-id/schedule/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"amount": 900.00}'

# Quick test - mark as paid
curl -X PATCH "http://localhost:5001/api/Loans/your-loan-id/schedule/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status": "PAID", "paymentMethod": "CASH"}'
```

## 🎉 **Success!**

You now have the **exact functionality you requested**:
- ✅ **Payment schedule updates** for specific loans
- ✅ **Amount updates** with smart calculations
- ✅ **Status management** (PENDING/PAID/OVERDUE)
- ✅ **Due date flexibility** 
- ✅ **Paid date tracking**
- ✅ **Complete audit trail**

**Your loan payment schedule management is now complete and powerful!** 🚀

---

**Implementation Date**: October 12, 2025  
**Status**: ✅ **COMPLETE & READY**  
**API Endpoint**: `PATCH /api/Loans/{loanId}/schedule/{installmentNumber}`

