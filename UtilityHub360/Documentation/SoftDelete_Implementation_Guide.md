# Soft Delete Implementation Guide for UtilityHub360

## Overview
This document outlines the soft delete implementation for financial transactions in UtilityHub360, following industry best practices for financial data management.

## Changes Made

### 1. Entity Updates
Added soft delete properties to all transaction entities:

**Entities Updated:**
- `Bill.cs`
- `Payment.cs`
- `BankTransaction.cs`
- `IncomeSource.cs`
- `VariableExpense.cs`
- `SavingsTransaction.cs`

**New Properties Added:**
```csharp
public bool IsDeleted { get; set; } = false;
public DateTime? DeletedAt { get; set; }
public string? DeletedBy { get; set; } // User ID who deleted it
public string? DeleteReason { get; set; } // Reason for deletion
```

### 2. Database Migration
Created migration: `20251028160148_AddSoftDeleteToTransactions.cs`

**Tables Updated:**
- Bills
- Payments
- BankTransactions
- IncomeSources
- VariableExpenses
- SavingsTransactions

**To Apply Migration:**
```bash
cd UtilityHub360
dotnet ef database update
```

## Financial Management Rules

### Delete Policies by Transaction Type

#### 1. **Bills**
- **Pending Bills (< 24 hours old)**: Hard delete allowed
- **Pending Bills (> 24 hours old)**: Soft delete (void)
- **Paid Bills**: Soft delete only (void) - maintains audit trail
- **Overdue Bills**: Soft delete only (void)

**Endpoints:**
```
DELETE /api/Bills/{id}         (Only if PENDING and < 24 hours)
PUT /api/Bills/{id}/void       (All other cases)
PUT /api/Bills/{id}/restore    (Restore voided bills)
```

#### 2. **Payments**
- **Pending Payments**: Hard delete allowed
- **Completed Payments**: Soft delete only (cancel) - financial integrity
- **Failed Payments**: Hard delete allowed

**Endpoints:**
```
DELETE /api/Payments/{id}       (Only if PENDING or FAILED)
PUT /api/Payments/{id}/cancel   (COMPLETED payments)
PUT /api/Payments/{id}/restore  (Restore cancelled payments)
```

#### 3. **Bank Transactions**
- **Manual Entries (< 24 hours)**: Hard delete allowed
- **Manual Entries (> 24 hours)**: Soft delete only
- **Synced from Bank**: Cannot be deleted (read-only)

**Endpoints:**
```
DELETE /api/BankTransactions/{id}     (Manual, < 24 hours)
PUT /api/BankTransactions/{id}/hide   (All others)
PUT /api/BankTransactions/{id}/restore (Restore hidden)
```

#### 4. **Income Sources**
- **Never Used in Calculations**: Hard delete allowed
- **Used in Profile/Reports**: Soft delete (deactivate)

**Endpoints:**
```
DELETE /api/IncomeSource/{id}          (If no dependencies)
PUT /api/IncomeSource/{id}/deactivate  (Preferred method)
PUT /api/IncomeSource/{id}/activate    (Restore)
```

#### 5. **Variable Expenses**
- **Same Day Entries**: Hard delete allowed
- **Historical Entries**: Soft delete (hide from view)

**Endpoints:**
```
DELETE /api/VariableExpenses/{id}     (Same day only)
PUT /api/VariableExpenses/{id}/hide   (Historical)
PUT /api/VariableExpenses/{id}/restore (Restore hidden)
```

#### 6. **Savings Transactions**
- **Pending Transactions**: Hard delete allowed
- **Processed Transactions**: Soft delete only - affects account balance history

**Endpoints:**
```
DELETE /api/SavingsTransactions/{id}     (PENDING only)
PUT /api/SavingsTransactions/{id}/void   (Processed)
PUT /api/SavingsTransactions/{id}/restore (Restore voided)
```

## Implementation Guidelines

### Service Layer Methods

Each service should implement:

```csharp
// Hard Delete (with validation)
Task<ApiResponse<bool>> DeleteAsync(string id, string userId);

// Soft Delete
Task<ApiResponse<bool>> SoftDeleteAsync(string id, string userId, string reason);

// Restore
Task<ApiResponse<TDto>> RestoreAsync(string id, string userId);

// Get including deleted
Task<ApiResponse<List<TDto>>> GetAllAsync(string userId, bool includeDeleted = false);
```

### Validation Rules

**Before Hard Delete:**
1. Check age of record (< 24 hours for most)
2. Check status (PENDING, FAILED, etc.)
3. Verify no dependencies
4. Verify ownership (userId matches)

**Before Soft Delete:**
1. Check if already deleted
2. Verify ownership
3. Record who deleted and when
4. Optional: require reason

**Before Restore:**
1. Check if deleted
2. Verify ownership
3. Check if restore is allowed by business rules
4. Clear deleted flags

### Query Filtering

**Default Behavior:**
All queries should exclude soft-deleted records by default:

```csharp
var query = _context.Bills
    .Where(b => b.UserId == userId && !b.IsDeleted);
```

**Include Deleted (Admin/Audit):**
```csharp
var query = _context.Bills
    .Where(b => b.UserId == userId);
// Don't filter IsDeleted
```

## Controller Implementation

### Example: Bills Controller

```csharp
/// <summary>
/// Delete or void a bill based on business rules
/// </summary>
[HttpDelete("{billId}")]
public async Task<ActionResult<ApiResponse<bool>>> DeleteBill(string billId)
{
    var userId = GetUserId();
    var result = await _billService.DeleteBillAsync(billId, userId);
    
    if (!result.Success)
        return BadRequest(result);
    
    return Ok(result);
}

/// <summary>
/// Void a paid or overdue bill
/// </summary>
[HttpPut("{billId}/void")]
public async Task<ActionResult<ApiResponse<BillDto>>> VoidBill(
    string billId,
    [FromBody] VoidBillDto voidDto)
{
    var userId = GetUserId();
    var result = await _billService.VoidBillAsync(billId, userId, voidDto.Reason);
    
    if (!result.Success)
        return BadRequest(result);
    
    return Ok(result);
}

/// <summary>
/// Restore a voided bill
/// </summary>
[HttpPut("{billId}/restore")]
public async Task<ActionResult<ApiResponse<BillDto>>> RestoreBill(string billId)
{
    var userId = GetUserId();
    var result = await _billService.RestoreBillAsync(billId, userId);
    
    if (!result.Success)
        return BadRequest(result);
    
    return Ok(result);
}
```

## DTOs

### VoidBillDto
```csharp
public class VoidBillDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
```

### QueryOptions
```csharp
public class QueryOptionsDto
{
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;
}
```

## Benefits of Soft Delete

1. **Audit Trail**: Complete history of all transactions
2. **Compliance**: Meets financial regulations
3. **Data Integrity**: Calculations remain accurate
4. **Recovery**: Accidental deletions can be restored
5. **Analytics**: Historical data preserved for reports
6. **Trust**: Users trust the system won't lose data

## Testing Checklist

- [ ] Hard delete works for eligible records
- [ ] Hard delete is blocked for ineligible records
- [ ] Soft delete marks records correctly
- [ ] Deleted records don't appear in default queries
- [ ] Restore functionality works
- [ ] Audit fields (DeletedBy, DeletedAt, DeleteReason) are set
- [ ] Financial calculations exclude soft-deleted records
- [ ] Reports can optionally include deleted records
- [ ] Admin can view all deleted records

## Database Cleanup (Optional)

For long-term maintenance, soft-deleted records older than X years can be permanently removed:

```sql
-- Archive and delete records older than 7 years
DELETE FROM Bills 
WHERE IsDeleted = 1 
AND DeletedAt < DATEADD(YEAR, -7, GETUTCDATE());
```

## Next Steps

1. ✅ Update entities with soft delete properties
2. ✅ Create database migration
3. ⏳ Apply migration to database
4. ⏳ Update all service methods to filter IsDeleted
5. ⏳ Implement soft delete methods in services
6. ⏳ Add delete/void/restore endpoints to controllers
7. ⏳ Update DTOs for void/restore operations
8. ⏳ Test implementation
9. ⏳ Update API documentation

## Migration Application

**Important:** After stopping your application, run:

```bash
cd UtilityHub360
dotnet ef database update
```

This will add the soft delete columns to all transaction tables.

