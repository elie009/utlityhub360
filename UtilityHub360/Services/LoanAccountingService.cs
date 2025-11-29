using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.Entities;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for handling loan accounting and journal entries
    /// </summary>
    public class LoanAccountingService
    {
        private readonly ApplicationDbContext _context;

        public LoanAccountingService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates journal entry for loan disbursement
        /// Debit: Cash, Credit: Loan Payable
        /// </summary>
        public async Task<JournalEntry> CreateLoanDisbursementEntryAsync(
            string loanId,
            string userId,
            decimal loanAmount,
            string? reference = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                LoanId = loanId,
                EntryType = "LOAN_DISBURSEMENT",
                EntryDate = DateTime.UtcNow,
                Description = $"Loan disbursement for loan {loanId}",
                Reference = reference ?? $"DISB-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = loanAmount,
                TotalCredit = loanAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Cash
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Cash",
                AccountType = "ASSET",
                EntrySide = "DEBIT",
                Amount = loanAmount,
                Description = $"Loan disbursement received",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Loan Payable
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Loan Payable",
                AccountType = "LIABILITY",
                EntrySide = "CREDIT",
                Amount = loanAmount,
                Description = $"Loan liability for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for loan payment
        /// Debit: Loan Payable (principal), Debit: Interest Expense (interest), Credit: Cash (total payment)
        /// </summary>
        public async Task<JournalEntry> CreateLoanPaymentEntryAsync(
            string loanId,
            string userId,
            decimal principalAmount,
            decimal interestAmount,
            decimal totalPayment,
            string? reference = null,
            string? description = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                LoanId = loanId,
                EntryType = "LOAN_PAYMENT",
                EntryDate = DateTime.UtcNow,
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

            // Credit Cash
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Cash",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = totalPayment,
                Description = $"Payment made for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for processing fee
        /// Debit: Loan Processing Expense, Credit: Cash
        /// </summary>
        public async Task<JournalEntry> CreateProcessingFeeEntryAsync(
            string loanId,
            string userId,
            decimal processingFee,
            string? reference = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                LoanId = loanId,
                EntryType = "PROCESSING_FEE",
                EntryDate = DateTime.UtcNow,
                Description = $"Processing fee for loan {loanId}",
                Reference = reference ?? $"FEE-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = processingFee,
                TotalCredit = processingFee,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Loan Processing Expense
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Loan Processing Expense",
                AccountType = "EXPENSE",
                EntrySide = "DEBIT",
                Amount = processingFee,
                Description = $"Processing fee for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Cash
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Cash",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = processingFee,
                Description = $"Processing fee paid for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            return journalEntry;
        }

        /// <summary>
        /// Creates journal entry for down payment
        /// Debit: Loan Payable, Credit: Cash
        /// </summary>
        public async Task<JournalEntry> CreateDownPaymentEntryAsync(
            string loanId,
            string userId,
            decimal downPayment,
            string? reference = null)
        {
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                LoanId = loanId,
                EntryType = "DOWN_PAYMENT",
                EntryDate = DateTime.UtcNow,
                Description = $"Down payment for loan {loanId}",
                Reference = reference ?? $"DOWN-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalDebit = downPayment,
                TotalCredit = downPayment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Debit Loan Payable (reduces the liability)
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Loan Payable",
                AccountType = "LIABILITY",
                EntrySide = "DEBIT",
                Amount = downPayment,
                Description = $"Down payment reduces loan liability for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            // Credit Cash
            journalEntry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountName = "Cash",
                AccountType = "ASSET",
                EntrySide = "CREDIT",
                Amount = downPayment,
                Description = $"Down payment made for loan {loanId}",
                CreatedAt = DateTime.UtcNow
            });

            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            return journalEntry;
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
    }
}







