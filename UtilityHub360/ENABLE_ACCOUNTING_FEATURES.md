# 🔄 How to Enable Loan Accounting Features (After Migration)

## Prerequisites

1. ✅ You've run the SQL migration script
2. ✅ Database has the new columns and tables
3. ✅ Application is running without errors

---

## Step-by-Step: Uncomment Code

### Step 1: Enable Journal Entry Tables in DbContext

**File**: `UtilityHub360/Data/ApplicationDbContext.cs`

#### A. Uncomment DbSet declarations (around line 38):
```csharp
// Change FROM:
// public DbSet<JournalEntry> JournalEntries { get; set; }
// public DbSet<JournalEntryLine> JournalEntryLines { get; set; }

// Change TO:
public DbSet<JournalEntry> JournalEntries { get; set; }
public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
```

#### B. Remove Ignore() calls for Loan entity (around line 63):
```csharp
// DELETE these lines:
entity.Ignore(e => e.InterestComputationMethod);
entity.Ignore(e => e.TotalInterest);
entity.Ignore(e => e.DownPayment);
entity.Ignore(e => e.ProcessingFee);
entity.Ignore(e => e.ActualFinancedAmount);
entity.Ignore(e => e.PaymentFrequency);
entity.Ignore(e => e.StartDate);
entity.Ignore(e => e.JournalEntries);
```

#### C. Uncomment JournalEntry configuration (around line 73):
```csharp
// Change FROM:
// // TEMPORARY: Commented out until database migration is applied
// // JournalEntry configuration
// // modelBuilder.Entity<JournalEntry>(entity =>

// Change TO:
// JournalEntry configuration
modelBuilder.Entity<JournalEntry>(entity =>
{
    entity.HasOne(d => d.User)
        .WithMany()
        .HasForeignKey(d => d.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(d => d.Loan)
        .WithMany(p => p.JournalEntries)
        .HasForeignKey(d => d.LoanId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasIndex(e => e.LoanId);
    entity.HasIndex(e => e.UserId);
    entity.HasIndex(e => e.EntryType);
    entity.HasIndex(e => e.EntryDate);
});
```

#### D. Uncomment JournalEntryLine configuration (around line 93):
```csharp
// Similar - uncomment the entire block
modelBuilder.Entity<JournalEntryLine>(entity =>
{
    // ... entire configuration
});
```

---

### Step 2: Enable Loan Field Assignments in LoanService

**File**: `UtilityHub360/Services/LoanService.cs`

#### A. Uncomment loan creation fields (around line 103):
```csharp
// In ApplyForLoanAsync method, change FROM:
// // TEMPORARY: Commented out until database migration is applied
// // InterestComputationMethod = interestMethod,
// // TotalInterest = totalInterest,
// // ActualFinancedAmount = actualFinancedAmount,
// // PaymentFrequency = "MONTHLY",
// // StartDate = null

// Change TO:
InterestComputationMethod = interestMethod,
TotalInterest = totalInterest,
ActualFinancedAmount = actualFinancedAmount,
PaymentFrequency = "MONTHLY",
StartDate = null
```

#### B. Uncomment disbursement code (around line 461):
```csharp
// Change FROM:
// // TEMPORARY: Commented out until database migration is applied
// // loan.StartDate = DateTime.UtcNow;
// // Handle processing fee if applicable...
// // Handle down payment if applicable...
// // Create journal entry for loan disbursement...

// Change TO:
loan.StartDate = DateTime.UtcNow;

// Handle processing fee if applicable (record before disbursement)
if (loan.ProcessingFee > 0)
{
    await _accountingService.CreateProcessingFeeEntryAsync(
        loanId,
        loan.UserId,
        loan.ProcessingFee,
        reference != null ? $"{reference}-FEE" : null);
}

// Handle down payment if applicable (reduces principal before disbursement)
if (loan.DownPayment > 0)
{
    await _accountingService.CreateDownPaymentEntryAsync(
        loanId,
        loan.UserId,
        loan.DownPayment,
        reference != null ? $"{reference}-DOWN" : null);
    
    loan.RemainingBalance -= loan.DownPayment;
}

// Create journal entry for loan disbursement
var disbursementAmount = loan.ActualFinancedAmount > 0 ? loan.ActualFinancedAmount : loan.Principal;
await _accountingService.CreateLoanDisbursementEntryAsync(
    loanId,
    loan.UserId,
    disbursementAmount,
    reference);
```

#### C. Uncomment payment journal entry (around line 706):
```csharp
// Change FROM:
// // TEMPORARY: Commented out until database migration is applied
// // Create journal entry for loan payment...
// // await _accountingService.CreateLoanPaymentEntryAsync(...)

// Change TO:
// Create journal entry for loan payment (Debit Loan Payable, Debit Interest Expense, Credit Cash)
await _accountingService.CreateLoanPaymentEntryAsync(
    loanId,
    userId,
    principalAmount,
    interestAmount,
    payment.Amount,
    payment.Reference,
    $"Loan payment via {payment.Method}");
```

---

### Step 3: Test the Application

1. **Build the project**: `dotnet build`
2. **Run the application**: `dotnet run`
3. **Test loan operations**:
   - Create a loan
   - Disburse a loan
   - Make a payment
   - Verify journal entries are created

---

## Verification Checklist

After uncommenting, verify:

- [ ] Application builds without errors
- [ ] Application runs without errors
- [ ] Loans can be created
- [ ] Loans can be disbursed
- [ ] Payments can be made
- [ ] Journal entries are created in database
- [ ] Loan accounting fields are saved to database

---

## Quick Find All Temporary Code

Search for `"TEMPORARY:"` in your IDE to find all commented sections that need to be uncommented.

---

**Once all steps are complete, your loan accounting system will be fully functional!** 🎉







