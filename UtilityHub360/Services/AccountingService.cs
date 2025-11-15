using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.Entities;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for handling accounting and journal entries for all transaction types
    /// Ensures double-entry bookkeeping compliance
    /// </summary>
    public class AccountingService
    {
        private readonly ApplicationDbContext _context;

        public AccountingService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates journal entry for loan disbursement
        /// Debit: Bank Account (Asset), Credit: Loan Payable (Liability)
        /// </summary>
        public async Task<JournalEntry> CreateLoanDisbursementEntryAsync(
            string loanId,
            string userId,
            decimal loanAmount,
            string? bankAccountName = null,
            string? reference = null,
            DateTime? entryDate = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                LoanId = loanId,
                EntryType = "LOAN_DISBURSEMENT",
                EntryDate = entryDate ?? DateTime.UtcNow,
                Description = $"Loan disbursement for loan {loanId}",
                Reference = reference ?? $"DISB-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = loanAmount,
                TotalCredit = loanAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Bank Account (Asset)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = string.IsNullOrEmpty(bankAccountName) ? "Bank Account" : $"Bank Account - {bankAccountName}",
                AccountType = "ASSET",
                EntrySide = "DEBIT",
                Amount = loanAmount,
                Description = $"Loan disbursement received",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Loan Payable (Liability)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Loan Payable",
                AccountType = "LIABILITY",
                EntrySide = "CREDIT",
                Amount = loanAmount,
                Description = $"Loan liability for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            ValidateJournalEntry(journalEntry);
            _context.JournalEntries.Add(journalEntry);
            // Note: Don't call SaveChangesAsync here - let the caller handle it in the transaction
            // await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for loan payment
        /// Debit: Loan Payable (principal), Debit: Interest Expense (interest), Credit: Bank Account (total payment)
        /// </summary>
        public async Task<JournalEntry> CreateLoanPaymentEntryAsync(
            string loanId,
            string userId,
            decimal principalAmount,
            decimal interestAmount,
            decimal totalPayment,
            string? bankAccountName = null,
            string? reference = null,
            string? description = null,
            DateTime? entryDate = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                LoanId = loanId,
                EntryType = "LOAN_PAYMENT",
                EntryDate = entryDate ?? DateTime.UtcNow,
                Description = description ?? $"Loan payment for loan {loanId}",
                Reference = reference ?? $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = totalPayment,
                TotalCredit = totalPayment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Loan Payable (principal portion)
            if (principalAmount > 0)
            {
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountName = "Loan Payable",
                    AccountType = "LIABILITY",
                    EntrySide = "DEBIT",
                    Amount = principalAmount,
                    Description = $"Principal payment for loan {loanId}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Debit Interest Expense (interest portion)
            if (interestAmount > 0)
            {
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountName = "Interest Expense",
                    AccountType = "EXPENSE",
                    EntrySide = "DEBIT",
                    Amount = interestAmount,
                    Description = $"Interest payment for loan {loanId}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Credit Bank Account
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = string.IsNullOrEmpty(bankAccountName) ? "Bank Account" : $"Bank Account - {bankAccountName}",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = totalPayment,
                Description = $"Payment made for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            ValidateJournalEntry(journalEntry);
            _context.JournalEntries.Add(journalEntry);
            // Note: Don't call SaveChangesAsync here - let the caller handle it in the transaction
            // await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for bill payment
        /// Debit: Expense Account (Expense), Credit: Bank Account (Asset)
        /// </summary>
        public async Task<JournalEntry> CreateBillPaymentEntryAsync(
            string billId,
            string userId,
            decimal amount,
            string billName,
            string billType,
            string? bankAccountName = null,
            string? reference = null,
            string? description = null,
            DateTime? entryDate = null)
        {
            var expenseAccountName = GetExpenseAccountName(billType);

            var journalEntry = new JournalEntry
            {
                UserId = userId,
                BillId = billId,
                EntryType = "BILL_PAYMENT",
                EntryDate = entryDate ?? DateTime.UtcNow,
                Description = description ?? $"Payment for {billName}",
                Reference = reference ?? $"BILL-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = amount,
                TotalCredit = amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Expense Account
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = expenseAccountName,
                AccountType = "EXPENSE",
                EntrySide = "DEBIT",
                Amount = amount,
                Description = $"Payment for {billName}",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Bank Account
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = string.IsNullOrEmpty(bankAccountName) ? "Bank Account" : $"Bank Account - {bankAccountName}",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = amount,
                Description = $"Payment for {billName}",
                CreatedAt = DateTime.UtcNow
            });

            ValidateJournalEntry(journalEntry);
            _context.JournalEntries.Add(journalEntry);
            // Note: Don't call SaveChangesAsync here - let the caller handle it in the transaction
            // await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for savings deposit
        /// Debit: Savings Account (Asset), Credit: Bank Account (Asset)
        /// </summary>
        public async Task<JournalEntry> CreateSavingsDepositEntryAsync(
            string savingsAccountId,
            string userId,
            decimal amount,
            string savingsAccountName,
            string? bankAccountName = null,
            string? reference = null,
            string? description = null,
            DateTime? entryDate = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                SavingsAccountId = savingsAccountId,
                EntryType = "SAVINGS_DEPOSIT",
                EntryDate = entryDate ?? DateTime.UtcNow,
                Description = description ?? $"Deposit to {savingsAccountName}",
                Reference = reference ?? $"SAV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = amount,
                TotalCredit = amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Savings Account (Asset)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = $"Savings Account - {savingsAccountName}",
                AccountType = "ASSET",
                EntrySide = "DEBIT",
                Amount = amount,
                Description = $"Deposit to {savingsAccountName}",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Bank Account (Asset)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = string.IsNullOrEmpty(bankAccountName) ? "Bank Account" : $"Bank Account - {bankAccountName}",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = amount,
                Description = $"Transfer to savings: {savingsAccountName}",
                CreatedAt = DateTime.UtcNow
            });

            ValidateJournalEntry(journalEntry);
            _context.JournalEntries.Add(journalEntry);
            // Note: Don't call SaveChangesAsync here - let the caller handle it in the transaction
            // await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for savings withdrawal
        /// Debit: Bank Account (Asset), Credit: Savings Account (Asset)
        /// </summary>
        public async Task<JournalEntry> CreateSavingsWithdrawalEntryAsync(
            string savingsAccountId,
            string userId,
            decimal amount,
            string savingsAccountName,
            string? bankAccountName = null,
            string? reference = null,
            string? description = null,
            DateTime? entryDate = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                SavingsAccountId = savingsAccountId,
                EntryType = "SAVINGS_WITHDRAWAL",
                EntryDate = entryDate ?? DateTime.UtcNow,
                Description = description ?? $"Withdrawal from {savingsAccountName}",
                Reference = reference ?? $"SAV-WD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = amount,
                TotalCredit = amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Bank Account (Asset)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = string.IsNullOrEmpty(bankAccountName) ? "Bank Account" : $"Bank Account - {bankAccountName}",
                AccountType = "ASSET",
                EntrySide = "DEBIT",
                Amount = amount,
                Description = $"Withdrawal from {savingsAccountName}",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Savings Account (Asset)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = $"Savings Account - {savingsAccountName}",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = amount,
                Description = $"Withdrawal from {savingsAccountName}",
                CreatedAt = DateTime.UtcNow
            });

            ValidateJournalEntry(journalEntry);
            _context.JournalEntries.Add(journalEntry);
            // Note: Don't call SaveChangesAsync here - let the caller handle it in the transaction
            // await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for regular expense
        /// Debit: Expense Account (Expense), Credit: Bank Account (Asset)
        /// </summary>
        public async Task<JournalEntry> CreateExpenseEntryAsync(
            string userId,
            decimal amount,
            string category,
            string? bankAccountName = null,
            string? reference = null,
            string? description = null,
            DateTime? entryDate = null)
        {
            var expenseAccountName = GetExpenseAccountName(category);

            var journalEntry = new JournalEntry
            {
                UserId = userId,
                EntryType = "EXPENSE",
                EntryDate = entryDate ?? DateTime.UtcNow,
                Description = description ?? $"Expense: {category}",
                Reference = reference ?? $"EXP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = amount,
                TotalCredit = amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Expense Account
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = expenseAccountName,
                AccountType = "EXPENSE",
                EntrySide = "DEBIT",
                Amount = amount,
                Description = description ?? $"Expense: {category}",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Bank Account
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = string.IsNullOrEmpty(bankAccountName) ? "Bank Account" : $"Bank Account - {bankAccountName}",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = amount,
                Description = description ?? $"Expense: {category}",
                CreatedAt = DateTime.UtcNow
            });

            ValidateJournalEntry(journalEntry);
            _context.JournalEntries.Add(journalEntry);
            // Note: Don't call SaveChangesAsync here - let the caller handle it in the transaction
            // await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for bank transfer
        /// Debit: Destination Bank Account (Asset), Credit: Source Bank Account (Asset)
        /// </summary>
        public async Task<JournalEntry> CreateBankTransferEntryAsync(
            string userId,
            decimal amount,
            string sourceAccountName,
            string destinationAccountName,
            string? reference = null,
            string? description = null,
            DateTime? entryDate = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                EntryType = "BANK_TRANSFER",
                EntryDate = entryDate ?? DateTime.UtcNow,
                Description = description ?? $"Transfer from {sourceAccountName} to {destinationAccountName}",
                Reference = reference ?? $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = amount,
                TotalCredit = amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Destination Bank Account (Asset)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = $"Bank Account - {destinationAccountName}",
                AccountType = "ASSET",
                EntrySide = "DEBIT",
                Amount = amount,
                Description = $"Transfer from {sourceAccountName}",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Source Bank Account (Asset)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = $"Bank Account - {sourceAccountName}",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = amount,
                Description = $"Transfer to {destinationAccountName}",
                CreatedAt = DateTime.UtcNow
            });

            ValidateJournalEntry(journalEntry);
            _context.JournalEntries.Add(journalEntry);
            // Note: Don't call SaveChangesAsync here - let the caller handle it in the transaction
            // await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Validates that journal entry follows double-entry bookkeeping rules
        /// </summary>
        private void ValidateJournalEntry(JournalEntry journalEntry)
        {
            var totalDebit = journalEntry.JournalEntryLines
                .Where(l => l.EntrySide == "DEBIT")
                .Sum(l => l.Amount);

            var totalCredit = journalEntry.JournalEntryLines
                .Where(l => l.EntrySide == "CREDIT")
                .Sum(l => l.Amount);

            if (totalDebit != totalCredit)
            {
                throw new InvalidOperationException(
                    $"Journal entry is not balanced. Total Debit: {totalDebit}, Total Credit: {totalCredit}");
            }

            if (journalEntry.TotalDebit != totalDebit || journalEntry.TotalCredit != totalCredit)
            {
                throw new InvalidOperationException(
                    $"Journal entry totals don't match line items. Entry Total Debit: {journalEntry.TotalDebit}, " +
                    $"Line Total Debit: {totalDebit}, Entry Total Credit: {journalEntry.TotalCredit}, " +
                    $"Line Total Credit: {totalCredit}");
            }

            if (journalEntry.JournalEntryLines.Count < 2)
            {
                throw new InvalidOperationException(
                    "Journal entry must have at least 2 lines (one debit and one credit)");
            }
        }

        /// <summary>
        /// Maps bill type or category to expense account name
        /// </summary>
        private string GetExpenseAccountName(string billTypeOrCategory)
        {
            var type = billTypeOrCategory.ToLower();

            return type switch
            {
                var t when t.Contains("utility") || t.Contains("electric") || t.Contains("water") || t.Contains("gas") => "Utilities Expense",
                var t when t.Contains("rent") => "Rent Expense",
                var t when t.Contains("insurance") => "Insurance Expense",
                var t when t.Contains("subscription") => "Subscription Expense",
                var t when t.Contains("phone") || t.Contains("telecom") => "Telecommunications Expense",
                var t when t.Contains("internet") => "Internet Expense",
                var t when t.Contains("food") || t.Contains("groceries") => "Food Expense",
                var t when t.Contains("transport") || t.Contains("gasoline") => "Transportation Expense",
                var t when t.Contains("entertainment") => "Entertainment Expense",
                var t when t.Contains("medical") || t.Contains("health") => "Medical Expense",
                var t when t.Contains("education") => "Education Expense",
                _ => "General Expense"
            };
        }

        /// <summary>
        /// Gets all journal entries for a loan
        /// </summary>
        public async Task<List<JournalEntry>> GetLoanJournalEntriesAsync(string loanId)
        {
            return await _context.JournalEntries
                .Include(je => je.JournalEntryLines)
                .Where(je => je.LoanId == loanId)
                .OrderBy(je => je.EntryDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all journal entries for a bill
        /// </summary>
        public async Task<List<JournalEntry>> GetBillJournalEntriesAsync(string billId)
        {
            return await _context.JournalEntries
                .Include(je => je.JournalEntryLines)
                .Where(je => je.BillId == billId)
                .OrderBy(je => je.EntryDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all journal entries for a savings account
        /// </summary>
        public async Task<List<JournalEntry>> GetSavingsJournalEntriesAsync(string savingsAccountId)
        {
            return await _context.JournalEntries
                .Include(je => je.JournalEntryLines)
                .Where(je => je.SavingsAccountId == savingsAccountId)
                .OrderBy(je => je.EntryDate)
                .ToListAsync();
        }
    }
}

