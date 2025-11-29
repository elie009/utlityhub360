# Payment Schedule Management for Loans

This document describes the enhanced payment schedule management features for the loan system, allowing users to add additional months, extend loan terms, and manage payment schedules dynamically.

## Overview

The system provides comprehensive monthly payment schedule management with the following capabilities:

1. **Automatic Schedule Generation** - Payment schedules are automatically created when loans are approved
2. **Extend Loan Terms** - Add additional months to existing loans
3. **Add Custom Payment Schedules** - Insert specific payment installments
4. **Regenerate Schedules** - Completely rebuild payment schedules with new terms
5. **Delete Installments** - Remove specific unpaid payment installments

## API Endpoints

### 1. Extend Loan Term
**POST** `/api/loans/{loanId}/extend-term`

Extends a loan by adding additional months with the same monthly payment amount.

**Request Body:**
```json
{
  "additionalMonths": 6,
  "reason": "Financial hardship - need extended payment period"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "schedule": [...],
    "totalInstallments": 18,
    "totalAmount": 18000.00,
    "firstDueDate": "2024-01-15T00:00:00Z",
    "lastDueDate": "2025-06-15T00:00:00Z",
    "message": "Loan term extended by 6 months. 6 new installments added."
  },
  "message": "Loan term extended successfully"
}
```

### 2. Add Payment Schedule (Auto Installment Number) ⭐ **SIMPLIFIED**
**POST** `/api/loans/{loanId}/add-schedule`

Adds payment installments to a loan with **automatic installment numbering**. Just provide due date and amount!

**Request Body:**
```json
{
  "firstDueDate": "2024-07-15T00:00:00Z",
  "monthlyPayment": 1200.00,
  "numberOfMonths": 3,
  "reason": "Additional payment periods"
}
```

**Note:** `startingInstallmentNumber` is **AUTO-GENERATED** - no need to provide it!

**Response:**
```json
{
  "success": true,
  "message": "Payment schedules added successfully",
  "data": {
    "schedule": [...],
    "message": "3 new payment installment(s) added automatically starting from installment #13"
  }
}
```

### 2b. Add Payment Schedule (Manual Installment Number)
**POST** `/api/loans/{loanId}/add-schedule-manual`

Advanced option if you need to specify exact installment numbers.

**Request Body:**
```json
{
  "startingInstallmentNumber": 13,
  "numberOfMonths": 3,
  "firstDueDate": "2024-07-15T00:00:00Z",
  "monthlyPayment": 1200.00,
  "reason": "Additional payment periods to reduce monthly burden"
}
```

### 3. Regenerate Payment Schedule
**POST** `/api/loans/{loanId}/regenerate-schedule`

Completely rebuilds the payment schedule with new terms (removes all unpaid installments).

**Request Body:**
```json
{
  "newMonthlyPayment": 800.00,
  "newTerm": 24,
  "startDate": "2024-01-01T00:00:00Z",
  "reason": "Restructuring loan due to income change"
}
```

### 4. Delete Payment Installment
**DELETE** `/api/loans/{loanId}/schedule/{installmentNumber}`

Removes a specific unpaid payment installment from the schedule.

**Response:**
```json
{
  "success": true,
  "data": true,
  "message": "Payment installment deleted successfully"
}
```

### 5. Mark Installment as Paid ⭐ **NEW**
**POST** `/api/loans/{loanId}/schedule/{installmentNumber}/mark-paid`

Marks a specific payment schedule installment as paid with a specific amount.

**Request Body:**
```json
{
  "amount": 825.00,
  "method": "CASH",
  "reference": "PAY-2024-001",
  "paymentDate": "2024-12-01T10:30:00Z",
  "notes": "Paid in cash at office"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Installment #3 marked as fully paid successfully",
  "data": {
    "id": "schedule-123",
    "loanId": "loan-456",
    "installmentNumber": 3,
    "dueDate": "2024-12-15T00:00:00Z",
    "principalAmount": 750.00,
    "interestAmount": 75.00,
    "totalAmount": 825.00,
    "status": "PAID",
    "paidAt": "2024-12-01T10:30:00Z"
  }
}
```

### 6. Simple Schedule Update ⭐ **NEW**
**PATCH** `/api/loans/{loanId}/schedule/{installmentNumber}`

Updates amount, status, due date, and paid date for a specific installment.

**Request Body (all fields optional):**
```json
{
  "amount": 900.00,
  "status": "PAID",
  "dueDate": "2024-12-20T00:00:00Z",
  "paidDate": "2024-12-01T10:30:00Z",
  "paymentMethod": "BANK_TRANSFER",
  "paymentReference": "TXN-123456",
  "notes": "Updated amount and marked as paid"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Payment schedule updated successfully",
  "data": {
    "id": "schedule-123",
    "loanId": "loan-456",
    "installmentNumber": 3,
    "dueDate": "2024-12-20T00:00:00Z",
    "principalAmount": 825.00,
    "interestAmount": 75.00,
    "totalAmount": 900.00,
    "status": "PAID",
    "paidAt": "2024-12-01T10:30:00Z"
  }
}
```

## Existing Endpoints (Already Available)

### Get Payment Schedule
**GET** `/api/loans/{loanId}/schedule`

Returns the complete payment schedule for a loan.

### Update Due Date
**PUT** `/api/loans/{loanId}/schedule/{installmentNumber}`

Updates the due date for a specific installment.

### Get Upcoming Payments
**GET** `/api/loans/upcoming-payments?days=30`

Returns upcoming payments across all active loans.

### Get Overdue Payments
**GET** `/api/loans/overdue-payments`

Returns all overdue payments for the user.

## Data Models

### RepaymentSchedule Entity
```csharp
public class RepaymentSchedule
{
    public string Id { get; set; }
    public string LoanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } // PENDING, PAID, OVERDUE
    public DateTime? PaidAt { get; set; }
}
```

### ExtendLoanTermDto
```csharp
public class ExtendLoanTermDto
{
    [Required]
    [Range(1, 60)]
    public int AdditionalMonths { get; set; }
    public string? Reason { get; set; }
}
```

### AddPaymentScheduleDto
```csharp
public class AddPaymentScheduleDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int StartingInstallmentNumber { get; set; }
    
    [Required]
    [Range(1, 60)]
    public int NumberOfMonths { get; set; }
    
    [Required]
    public DateTime FirstDueDate { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MonthlyPayment { get; set; }
    
    public string? Reason { get; set; }
}
```

### RegenerateScheduleDto
```csharp
public class RegenerateScheduleDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal NewMonthlyPayment { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int NewTerm { get; set; }
    
    public DateTime? StartDate { get; set; }
    public string? Reason { get; set; }
}
```

### MarkInstallmentPaidDto ⭐ **NEW**
```csharp
public class MarkInstallmentPaidDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string Method { get; set; } = string.Empty; // CASH, BANK_TRANSFER, CREDIT_CARD, etc.

    [StringLength(100)]
    public string? Reference { get; set; }
    
    public DateTime? PaymentDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
```

### SimpleScheduleUpdateDto ⭐ **NEW**
```csharp
public class SimpleScheduleUpdateDto
{
    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }
    
    [StringLength(20)]
    public string? Status { get; set; } // PENDING, PAID, OVERDUE
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    [StringLength(50)]
    public string? PaymentMethod { get; set; } // For when marking as PAID
    
    [StringLength(100)]
    public string? PaymentReference { get; set; } // For when marking as PAID
    
    [StringLength(500)]
    public string? Notes { get; set; }
}
```

## Usage Examples

### Example 1: Extend Loan by 3 Months
```bash
curl -X POST "https://api.utilityhub360.com/api/loans/loan-123/extend-term" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "additionalMonths": 3,
    "reason": "Need more time due to temporary income reduction"
  }'
```

### Example 2: Add Payment Months (Auto Installment Number) ⭐ **SIMPLIFIED**
```bash
curl -X POST "https://api.utilityhub360.com/api/loans/loan-123/add-schedule" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "numberOfMonths": 2,
    "firstDueDate": "2024-07-15T00:00:00Z",
    "monthlyPayment": 1500.00,
    "reason": "Adding catch-up payments"
  }'
```
**Note:** No `startingInstallmentNumber` needed - it's AUTO-GENERATED!

### Example 3: Completely Restructure Loan
```bash
curl -X POST "https://api.utilityhub360.com/api/loans/loan-123/regenerate-schedule" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "newMonthlyPayment": 750.00,
    "newTerm": 20,
    "startDate": "2024-02-01T00:00:00Z",
    "reason": "Loan restructuring due to financial hardship"
  }'
```

### Example 4: Mark Installment as Paid ⭐ **NEW**
```bash
curl -X POST "https://api.utilityhub360.com/api/loans/loan-123/schedule/3/mark-paid" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 825.00,
    "method": "CASH",
    "reference": "CASH-PAY-001",
    "paymentDate": "2024-12-01T10:30:00Z",
    "notes": "Payment received in cash at office"
  }'
```

### Example 5: Simple Schedule Update ⭐ **NEW**
```bash
curl -X PATCH "https://api.utilityhub360.com/api/loans/loan-123/schedule/3" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 900.00,
    "status": "PAID",
    "dueDate": "2024-12-20T00:00:00Z",
    "paidDate": "2024-12-01T10:30:00Z",
    "paymentMethod": "BANK_TRANSFER",
    "paymentReference": "TXN-123456",
    "notes": "Updated amount and marked as paid"
  }'
```

## Business Rules

### Extend Loan Term
- Can only extend ACTIVE or APPROVED loans
- Adds months using the existing monthly payment amount
- Updates loan total amount and term
- Maintains the same interest rate calculation

### Add Payment Schedule
- Can only add to ACTIVE or APPROVED loans
- Cannot create installments with conflicting numbers
- Allows custom payment amounts
- Updates loan total amount

### Regenerate Schedule
- Can only regenerate for ACTIVE or APPROVED loans
- Cannot regenerate if there are already paid installments
- Completely removes existing unpaid installments
- Updates loan parameters (monthly payment, term, total amount)

### Delete Installment
- Can only delete unpaid installments (PENDING status)
- Updates loan totals when installment is removed
- Adjusts loan term count

### Mark Installment as Paid ⭐ **NEW**
- Can only mark PENDING installments as PAID
- Cannot mark already PAID installments again
- Creates a Payment record for audit trail
- Updates loan remaining balance automatically
- Supports full and partial payments
- Automatically completes loan when all installments are paid
- Generates unique reference if not provided
- Supports multiple payment methods (CASH, BANK_TRANSFER, CREDIT_CARD, etc.)

### Simple Schedule Update ⭐ **NEW**
- Can update any combination of amount, status, due date, and paid date
- Updates amounts proportionally to maintain principal/interest ratio
- Creates Payment record when marking as PAID
- Automatically updates loan balance and totals
- Can reopen completed loans if installments become pending
- Supports status changes: PENDING ↔ PAID ↔ OVERDUE
- All fields are optional - only updates what you provide
- Validates amount changes and prevents negative balances

## Security

- All endpoints require user authentication via JWT token
- Users can only manage their own loan payment schedules
- Admin users have access to all loans
- Input validation prevents invalid data

## Interest Calculation

The system properly handles interest calculations for added months:

- **0% Interest Loans**: All payment amounts go to principal
- **Interest-bearing Loans**: Each installment is split between principal and interest based on remaining balance
- Interest is calculated monthly using the annual interest rate divided by 12

## Integration Notes

- Payment schedules automatically integrate with the notification system for due date reminders
- The dashboard shows updated payment counts and amounts
- Monthly payment totals are recalculated when schedules change
- Existing loan tracking and analytics are automatically updated

## Error Handling

The API provides detailed error messages for common scenarios:

- Loan not found
- Insufficient permissions
- Invalid loan status
- Conflicting installment numbers
- Validation errors
- Existing paid installments (for regeneration)

All endpoints return consistent API response format with success/error status and detailed messages.
