# Payment Delete Feature Documentation

## Overview

The Payment Delete feature allows users to delete their own payments under specific business rules and conditions. This feature provides a way to correct payment mistakes while maintaining data integrity and preventing abuse.

## API Endpoint

### Delete Payment
```http
DELETE /api/payments/{paymentId}
Authorization: Bearer {token}
```

**Parameters:**
- `paymentId` (string, required): The unique identifier of the payment to delete

**Response:**
```json
{
  "success": true,
  "data": true,
  "message": "Payment deleted successfully"
}
```

## Business Rules & Validation

### 1. **User Authorization**
- Only the payment owner can delete their own payments
- Users cannot delete payments belonging to other users
- Authentication is required (JWT token)

### 2. **Payment Status Validation**
- **Cannot delete COMPLETED payments**
  - Reason: Completed payments have been processed and should not be deleted
  - Error: "Cannot delete completed payments. Please contact support if this is an error."

### 3. **Loan Status Validation**
- **Cannot delete payments for non-active loans**
  - Only payments for ACTIVE loans can be deleted
  - Error: "Cannot delete payments for loans that are not active"

### 4. **Time-based Validation**
- **Cannot delete payments older than 24 hours**
  - Business rule to prevent deletion of old payments
  - Error: "Cannot delete payments older than 24 hours"

## What Happens When a Payment is Deleted

### 1. **Payment Removal**
- The payment record is removed from the database
- All payment data is permanently deleted

### 2. **Associated Transaction Cleanup**
- The corresponding transaction record is also removed
- Transaction type "PAYMENT" with matching reference is deleted

### 3. **Loan Balance Restoration**
- The payment amount is added back to the loan's `RemainingBalance`
- The loan balance is restored to its state before the payment

### 4. **Loan Status Reversion**
- If the loan was marked as "COMPLETED" due to this payment, it's reverted to "ACTIVE"
- The `CompletedAt` timestamp is cleared if the loan is no longer fully paid

## Implementation Details

### Service Layer (`PaymentService.DeletePaymentAsync`)

```csharp
public async Task<ApiResponse<bool>> DeletePaymentAsync(string paymentId, string userId)
{
    // 1. Find payment and verify ownership
    var payment = await _context.Payments
        .Include(p => p.Loan)
        .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

    // 2. Validate business rules
    if (payment.Status == "COMPLETED") { /* Error */ }
    if (payment.Loan.Status != "ACTIVE") { /* Error */ }
    if (hoursSinceCreation > 24) { /* Error */ }

    // 3. Remove associated transaction
    var transaction = await _context.Transactions
        .FirstOrDefaultAsync(t => t.LoanId == payment.LoanId && 
                                t.Type == "PAYMENT" && 
                                t.Reference == payment.Reference);
    if (transaction != null) {
        _context.Transactions.Remove(transaction);
    }

    // 4. Restore loan balance
    payment.Loan.RemainingBalance += payment.Amount;

    // 5. Revert loan status if needed
    if (payment.Loan.Status == "COMPLETED" && payment.Loan.RemainingBalance > 0) {
        payment.Loan.Status = "ACTIVE";
        payment.Loan.CompletedAt = null;
    }

    // 6. Remove payment
    _context.Payments.Remove(payment);
    await _context.SaveChangesAsync();
}
```

### Controller Layer (`PaymentsController.DeletePayment`)

```csharp
[HttpDelete("{paymentId}")]
public async Task<ActionResult<ApiResponse<bool>>> DeletePayment(string paymentId)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) {
        return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
    }

    var result = await _paymentService.DeletePaymentAsync(paymentId, userId);
    
    if (result.Success) {
        return Ok(result);
    }
    
    return BadRequest(result);
}
```

## Usage Examples

### 1. **Successful Payment Deletion**

```javascript
// Delete a payment
const response = await fetch('/api/payments/payment-123', {
    method: 'DELETE',
    headers: {
        'Authorization': `Bearer ${token}`
    }
});

const result = await response.json();
if (result.success) {
    console.log('Payment deleted successfully');
    // Loan balance has been restored
    // Associated transaction has been removed
}
```

### 2. **Error Handling**

```javascript
try {
    const response = await fetch('/api/payments/payment-123', {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    const result = await response.json();
    
    if (!result.success) {
        switch (result.message) {
            case 'Cannot delete completed payments. Please contact support if this is an error.':
                // Handle completed payment error
                break;
            case 'Cannot delete payments for loans that are not active':
                // Handle inactive loan error
                break;
            case 'Cannot delete payments older than 24 hours':
                // Handle time limit error
                break;
            case 'Payment not found or you don\'t have permission to delete it':
                // Handle not found/unauthorized error
                break;
        }
    }
} catch (error) {
    console.error('Network error:', error);
}
```

## Error Responses

### 1. **Unauthorized Access**
```json
{
  "success": false,
  "message": "User not authenticated"
}
```

### 2. **Payment Not Found**
```json
{
  "success": false,
  "message": "Payment not found or you don't have permission to delete it"
}
```

### 3. **Completed Payment**
```json
{
  "success": false,
  "message": "Cannot delete completed payments. Please contact support if this is an error."
}
```

### 4. **Inactive Loan**
```json
{
  "success": false,
  "message": "Cannot delete payments for loans that are not active"
}
```

### 5. **Time Limit Exceeded**
```json
{
  "success": false,
  "message": "Cannot delete payments older than 24 hours"
}
```

## Security Considerations

### 1. **Authorization**
- JWT token validation ensures only authenticated users can delete payments
- User ID verification ensures users can only delete their own payments

### 2. **Business Rules**
- Multiple validation layers prevent unauthorized or inappropriate deletions
- Time-based restrictions prevent deletion of old payments
- Status-based restrictions prevent deletion of processed payments

### 3. **Data Integrity**
- Atomic operations ensure database consistency
- Loan balance restoration maintains accurate financial records
- Transaction cleanup prevents orphaned records

## Database Impact

### Tables Affected:
1. **Payments** - Payment record is deleted
2. **Transactions** - Associated transaction record is deleted
3. **Loans** - RemainingBalance is restored, status may be reverted

### Cascade Effects:
- No cascade deletes are implemented
- Manual cleanup of related records ensures data integrity
- Loan status and balance are properly restored

## Testing Scenarios

### 1. **Valid Deletion**
- User deletes their own payment within 24 hours
- Payment status is PENDING or FAILED
- Loan status is ACTIVE
- Expected: Success, loan balance restored

### 2. **Invalid Deletions**
- Delete completed payment → Error
- Delete payment for inactive loan → Error
- Delete payment older than 24 hours → Error
- Delete another user's payment → Error

### 3. **Edge Cases**
- Delete payment that completed the loan → Loan status reverted to ACTIVE
- Delete payment with no associated transaction → Still succeeds
- Delete payment with multiple transactions → Only matching transaction deleted

## Integration with Existing Features

### 1. **Loan Management**
- Deleted payments restore loan balances
- Loan completion status is properly managed
- Loan history remains accurate

### 2. **Transaction History**
- Associated transactions are cleaned up
- No orphaned transaction records
- Financial records remain consistent

### 3. **User Experience**
- Clear error messages for different scenarios
- Proper HTTP status codes
- Consistent API response format

## Future Enhancements

### 1. **Audit Trail**
- Log payment deletions for audit purposes
- Track who deleted what and when
- Maintain deletion history

### 2. **Soft Delete**
- Mark payments as deleted instead of removing them
- Allow recovery of accidentally deleted payments
- Maintain complete payment history

### 3. **Admin Override**
- Allow administrators to delete any payment
- Bypass business rules for administrative purposes
- Special admin-only deletion endpoint

### 4. **Bulk Deletion**
- Allow deletion of multiple payments at once
- Batch processing for efficiency
- Transaction rollback on any failure

This payment delete feature provides a robust and secure way to manage payment corrections while maintaining data integrity and preventing abuse through comprehensive business rules and validation.
