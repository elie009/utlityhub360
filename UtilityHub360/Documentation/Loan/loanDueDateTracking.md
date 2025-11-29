# Loan Due Date Tracking System

## Overview

The loan due date tracking system helps users stay on top of their loan payments by tracking payment schedules, sending reminders, and identifying overdue payments. The system automatically handles calendar variations (28/30/31 days per month).

## Architecture

### Database Structure

The system uses the existing `RepaymentSchedule` table to track payment due dates:

```sql
RepaymentSchedule
├── Id (PK)
├── LoanId (FK)
├── InstallmentNumber
├── DueDate          -- When payment is due
├── PrincipalAmount
├── InterestAmount
├── TotalAmount
├── Status           -- PENDING, PAID, OVERDUE
└── PaidAt
```

### Why This Approach?

Instead of storing `NextDueDate` directly on the `Loan` table, we calculate it dynamically from `RepaymentSchedule`. This provides:

✅ **Flexibility**: Supports any payment frequency (daily, weekly, monthly)  
✅ **Accuracy**: Always shows the actual next unpaid installment  
✅ **Tracking**: Complete payment history per installment  
✅ **Calendar-Safe**: C# DateTime handles month variations automatically  

## API Endpoints

### 1. Get User Loans with Due Dates

```http
GET /api/Loans/user/{userId}?status=ACTIVE&page=1&limit=10
```

**Response:**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": "loan-123",
        "userId": "user-456",
        "principal": 10000,
        "interestRate": 5.5,
        "term": 12,
        "purpose": "Business expansion",
        "status": "ACTIVE",
        "monthlyPayment": 858.33,
        "totalAmount": 10300,
        "remainingBalance": 8000,
        "appliedAt": "2025-01-09T00:00:00Z",
        "approvedAt": "2025-01-10T00:00:00Z",
        "disbursedAt": "2025-01-11T00:00:00Z",
        "completedAt": null,
        "additionalInfo": null,
        "nextDueDate": "2025-11-09T00:00:00Z"
      }
    ],
    "page": 1,
    "limit": 10,
    "totalCount": 1
  }
}
```

### 2. Get Upcoming Payments

```http
GET /api/Loans/upcoming-payments?days=30
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "loanId": "loan-123",
      "dueDate": "2025-11-09T00:00:00Z",
      "amount": 858.33,
      "installmentNumber": 3,
      "daysUntilDue": 15,
      "loanPurpose": "Business expansion"
    },
    {
      "loanId": "loan-789",
      "dueDate": "2025-11-20T00:00:00Z",
      "amount": 450.00,
      "installmentNumber": 2,
      "daysUntilDue": 26,
      "loanPurpose": "Car purchase"
    }
  ]
}
```

### 3. Get Overdue Payments

```http
GET /api/Loans/overdue-payments
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "loanId": "loan-456",
      "dueDate": "2025-10-01T00:00:00Z",
      "amount": 500.00,
      "installmentNumber": 1,
      "daysOverdue": 8,
      "loanPurpose": "Medical expenses"
    }
  ]
}
```

### 4. Get Next Due Date for Specific Loan

```http
GET /api/Loans/{loanId}/next-due-date
```

**Response:**
```json
{
  "success": true,
  "data": "2025-11-09T00:00:00Z"
}
```

### 5. Update Payment Due Date

```http
PUT /api/Loans/{loanId}/schedule/{installmentNumber}
```

**Request:**
```json
{
  "newDueDate": "2025-11-15T00:00:00Z"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Repayment schedule updated successfully",
  "data": {
    "id": "schedule-123",
    "loanId": "loan-456",
    "installmentNumber": 3,
    "dueDate": "2025-11-15T00:00:00Z",
    "principalAmount": 800.00,
    "interestAmount": 58.33,
    "totalAmount": 858.33,
    "status": "PENDING",
    "paidAt": null
  }
}
```

**Rules:**
- ✅ Can only update PENDING payments (not PAID ones)
- ✅ Both loan owner and admin can update
- ✅ Cannot update past payment dates to future (business logic can be added)

### 6. Get Full Repayment Schedule

```http
GET /api/Loans/{loanId}/schedule
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "schedule-1",
      "loanId": "loan-123",
      "installmentNumber": 1,
      "dueDate": "2025-02-09T00:00:00Z",
      "principalAmount": 800.00,
      "interestAmount": 58.33,
      "totalAmount": 858.33,
      "status": "PAID",
      "paidAt": "2025-02-08T10:30:00Z"
    },
    {
      "id": "schedule-2",
      "loanId": "loan-123",
      "installmentNumber": 2,
      "dueDate": "2025-03-09T00:00:00Z",
      "principalAmount": 805.12,
      "interestAmount": 53.21,
      "totalAmount": 858.33,
      "status": "PAID",
      "paidAt": "2025-03-07T14:20:00Z"
    },
    {
      "id": "schedule-3",
      "loanId": "loan-123",
      "installmentNumber": 3,
      "dueDate": "2025-04-09T00:00:00Z",
      "principalAmount": 810.35,
      "interestAmount": 47.98,
      "totalAmount": 858.33,
      "status": "PENDING",
      "paidAt": null
    }
  ]
}
```

## How NextDueDate is Calculated

The `NextDueDate` shown in loan responses is **dynamically calculated**:

```csharp
// Get next due date from RepaymentSchedule
var nextDueDate = await _context.RepaymentSchedules
    .Where(rs => rs.LoanId == loanId && rs.Status == "PENDING")
    .OrderBy(rs => rs.DueDate)
    .Select(rs => rs.DueDate)
    .FirstOrDefaultAsync();
```

### Logic:
1. Find all PENDING (unpaid) installments for the loan
2. Sort by due date (earliest first)
3. Return the first one's due date
4. If no pending installments → returns `null` (loan is completed)

## Handling Different Payment Frequencies

### Monthly Payments (Default)
```csharp
DueDate = DateTime.UtcNow.AddMonths(installmentNumber)
```
- Automatically handles 28/30/31 day months
- If loan starts on Jan 31, Feb payment will be Feb 28 (or 29 in leap years)

### Weekly Payments
```csharp
DueDate = DateTime.UtcNow.AddDays(7 * installmentNumber)
```

### Daily Payments
```csharp
DueDate = DateTime.UtcNow.AddDays(installmentNumber)
```

## Notification System

### Automatic Reminders

The `LoanDueDateService` can send automated notifications:

```csharp
// Send reminders for payments due in next 3 days
await dueDateService.SendUpcomingPaymentRemindersAsync(daysInAdvance: 3);

// Mark overdue payments and notify users
await dueDateService.UpdateOverduePaymentsAsync();
```

### Setting Up Scheduled Jobs

You can use a background service or scheduled task to run these methods:

**Option 1: Windows Task Scheduler**
- Create a console app that calls these methods
- Schedule to run daily at 9 AM

**Option 2: Background Service in ASP.NET**
```csharp
public class LoanDueDateBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Run every day at 9 AM
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1).AddHours(9);
            var delay = nextRun - now;
            
            await Task.Delay(delay, stoppingToken);
            
            // Run the due date checks
            await dueDateService.UpdateOverduePaymentsAsync();
            await dueDateService.SendUpcomingPaymentRemindersAsync(3);
        }
    }
}
```

**Option 3: Azure Functions / AWS Lambda**
- Create serverless function
- Schedule using cron: `0 9 * * *` (daily at 9 AM)

## Best Practices

### 1. **Don't Modify Paid Installments**
```csharp
if (schedule.Status == "PAID")
{
    return BadRequest("Cannot update a paid installment");
}
```

### 2. **Validate New Due Date**
```csharp
// Ensure new date is not in the past
if (updateDto.NewDueDate < DateTime.UtcNow.Date)
{
    return BadRequest("Due date cannot be in the past");
}

// Ensure chronological order if needed
var previousInstallment = await GetPreviousInstallment(loanId, installmentNumber);
if (previousInstallment != null && updateDto.NewDueDate <= previousInstallment.DueDate)
{
    return BadRequest("Due date must be after previous installment");
}
```

### 3. **Log Due Date Changes**
```csharp
// Create audit log entry
_logger.LogInformation(
    "Due date updated for Loan {LoanId}, Installment {InstallmentNumber}: {OldDate} -> {NewDate}",
    loanId, installmentNumber, oldDueDate, newDueDate
);
```

### 4. **Send Notification on Change**
```csharp
await _notificationService.SendNotificationAsync(new CreateNotificationDto
{
    UserId = loan.UserId,
    Title = "Payment Due Date Updated",
    Message = $"Your payment #{installmentNumber} due date has been changed to {newDueDate:MMM dd, yyyy}.",
    Type = "INFO"
});
```

## Frontend Integration

### Display Next Due Date

```typescript
// In your loan card/list component
interface Loan {
  id: string;
  principal: number;
  status: string;
  remainingBalance: number;
  nextDueDate: string | null; // ISO date string
}

// Format for display
const formatDueDate = (dateStr: string | null) => {
  if (!dateStr) return 'No upcoming payment';
  
  const dueDate = new Date(dateStr);
  const today = new Date();
  const daysUntil = Math.floor((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
  
  if (daysUntil < 0) return `Overdue by ${Math.abs(daysUntil)} days`;
  if (daysUntil === 0) return 'Due TODAY';
  if (daysUntil === 1) return 'Due tomorrow';
  return `Due in ${daysUntil} days`;
};
```

### Payment Reminder Dashboard

```typescript
// Fetch upcoming payments
const response = await fetch('/api/Loans/upcoming-payments?days=30');
const { data: upcomingPayments } = await response.json();

// Group by urgency
const today = upcomingPayments.filter(p => p.daysUntilDue === 0);
const thisWeek = upcomingPayments.filter(p => p.daysUntilDue > 0 && p.daysUntilDue <= 7);
const thisMonth = upcomingPayments.filter(p => p.daysUntilDue > 7);
```

### Overdue Payment Alert

```typescript
// Fetch overdue payments
const response = await fetch('/api/Loans/overdue-payments');
const { data: overduePayments } = await response.json();

if (overduePayments.length > 0) {
  // Show alert banner
  showAlert(`You have ${overduePayments.length} overdue payment(s)`, 'danger');
}
```

## Testing

### Test Cases

**1. Get loans with next due date**
```bash
curl -X GET "https://api.example.com/api/Loans/user/{userId}" \
  -H "Authorization: Bearer {token}"
```

**2. Update payment due date**
```bash
curl -X PUT "https://api.example.com/api/Loans/{loanId}/schedule/3" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"newDueDate": "2025-11-15T00:00:00Z"}'
```

**3. Get upcoming payments**
```bash
curl -X GET "https://api.example.com/api/Loans/upcoming-payments?days=30" \
  -H "Authorization: Bearer {token}"
```

**4. Get overdue payments**
```bash
curl -X GET "https://api.example.com/api/Loans/overdue-payments" \
  -H "Authorization: Bearer {token}"
```

## Common Scenarios

### Scenario 1: User needs to postpone payment
1. Call `GET /api/Loans/{loanId}/schedule` to get all installments
2. Identify the next pending installment
3. Call `PUT /api/Loans/{loanId}/schedule/{installmentNumber}` with new date
4. The `NextDueDate` will automatically update

### Scenario 2: Multiple loans with different due dates
1. Call `GET /api/Loans/upcoming-payments?days=30`
2. Display all upcoming payments sorted by date
3. Group by loan or by week for better UX

### Scenario 3: Grace period handling
Add business logic in the service to only mark as OVERDUE after grace period:

```csharp
var gracePeriodDays = 5;
var overduePayments = await _context.RepaymentSchedules
    .Where(rs => rs.Status == "PENDING" && 
                 rs.DueDate.Date.AddDays(gracePeriodDays) < today)
    .ToListAsync();
```

## Future Enhancements

### 1. **Payment Frequency Configuration**
Add a `PaymentFrequency` field to Loan entity:
- DAILY
- WEEKLY  
- BIWEEKLY
- MONTHLY
- QUARTERLY

### 2. **Auto-Payment**
Allow users to set up automatic payments before due date:
```json
{
  "enableAutoPayment": true,
  "autoPaymentDaysBefore": 1,
  "paymentMethod": "BANK_TRANSFER",
  "bankAccountId": "account-123"
}
```

### 3. **Payment Reminders Configuration**
Let users configure when they want reminders:
```json
{
  "reminderDays": [7, 3, 1, 0],  // 7 days before, 3 days, 1 day, day of
  "reminderChannels": ["EMAIL", "SMS", "PUSH"]
}
```

### 4. **Calendar Integration**
Export payment schedule to iCal/Google Calendar format

### 5. **Smart Rescheduling**
If user misses a payment, allow rescheduling entire remaining schedule:
```http
POST /api/Loans/{loanId}/reschedule
{
  "startDate": "2025-12-01",
  "redistributePayments": true
}
```

## Troubleshooting

### NextDueDate is null
**Possible causes:**
1. No repayment schedule created (loan not approved/disbursed)
2. All installments are PAID (loan completed)
3. Loan status is COMPLETED, REJECTED, or CANCELLED

**Solution:** Check the repayment schedule:
```http
GET /api/Loans/{loanId}/schedule
```

### Due dates seem wrong
**Check:**
1. Timezone: All dates stored in UTC, convert to user's timezone in frontend
2. Calendar: DateTime.AddMonths() handles month variations correctly
3. Start date: Due dates calculated from `DisbursedAt`, not `AppliedAt`

### Performance with many loans
Current implementation uses batch query:
```csharp
// Efficient: One query for all loans' next due dates
var nextDueDates = await _context.RepaymentSchedules
    .Where(rs => loanIds.Contains(rs.LoanId) && rs.Status == "PENDING")
    .GroupBy(rs => rs.LoanId)
    .Select(g => new { LoanId = g.Key, NextDueDate = g.Min(rs => rs.DueDate) })
    .ToListAsync();
```

## Related Documentation

- [loanUpdateFlow.md](./loanUpdateFlow.md) - How to update loan details
- [loanUpdateQuickReference.md](./loanUpdateQuickReference.md) - Quick API reference
- [principalUpdateGuide.md](./principalUpdateGuide.md) - Updating loan principal

## Support

For questions or issues, contact the development team or create an issue in the repository.

