using Microsoft.EntityFrameworkCore;
using UtilityHub360.Entities;

namespace UtilityHub360.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<RepaymentSchedule> RepaymentSchedules { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationHistory> NotificationHistories { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Receivable> Receivables { get; set; }
        public DbSet<ReceivablePayment> ReceivablePayments { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<SavingsTransaction> SavingsTransactions { get; set; }
        
        // Investment Tables
        public DbSet<Investment> Investments { get; set; }
        public DbSet<InvestmentPosition> InvestmentPositions { get; set; }
        public DbSet<InvestmentTransaction> InvestmentTransactions { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<IncomeSource> IncomeSources { get; set; }
        public DbSet<VariableExpense> VariableExpenses { get; set; }
        public DbSet<UserOnboarding> UserOnboardings { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        
        // Bill Analytics Tables
        public DbSet<BudgetSetting> BudgetSettings { get; set; }
        public DbSet<BillAnalyticsCache> BillAnalyticsCaches { get; set; }
        public DbSet<BillAlert> BillAlerts { get; set; }
        
        // Chat Support Tables
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        
        // Accounting Tables
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        
        // Reconciliation Tables
        public DbSet<BankStatement> BankStatements { get; set; }
        public DbSet<BankStatementItem> BankStatementItems { get; set; }
        public DbSet<Reconciliation> Reconciliations { get; set; }
        public DbSet<ReconciliationMatch> ReconciliationMatches { get; set; }
        
        // Transaction Categories
        public DbSet<TransactionCategory> TransactionCategories { get; set; }
        
        // Transaction Rules
        public DbSet<TransactionRule> TransactionRules { get; set; }
        
        // Utilities
        public DbSet<Utility> Utilities { get; set; }
        
        // Expense Management Tables
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<ExpenseBudget> ExpenseBudgets { get; set; }
        public DbSet<ExpenseReceipt> ExpenseReceipts { get; set; }
        public DbSet<ExpenseApproval> ExpenseApprovals { get; set; }
        
        // Allocation Planning Tables
        public DbSet<AllocationTemplate> AllocationTemplates { get; set; }
        public DbSet<AllocationTemplateCategory> AllocationTemplateCategories { get; set; }
        public DbSet<AllocationPlan> AllocationPlans { get; set; }
        public DbSet<AllocationCategory> AllocationCategories { get; set; }
        public DbSet<AllocationHistory> AllocationHistories { get; set; }
        public DbSet<AllocationRecommendation> AllocationRecommendations { get; set; }
        
        // Audit Logging Tables
        public DbSet<AuditLog> AuditLogs { get; set; }
        
        // Ticket Management Tables
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketStatusHistory> TicketStatusHistories { get; set; }
        
        // Month Closing Tables
        public DbSet<ClosedMonth> ClosedMonths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();
            });

            // Loan configuration
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Loans)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Temporary: Ignore accounting properties until database migration is applied
                // TODO: Remove these Ignore() calls after running APPLY_ALL_LOAN_ACCOUNTING_CHANGES.sql
                entity.Ignore(e => e.InterestComputationMethod);
                entity.Ignore(e => e.TotalInterest);
                entity.Ignore(e => e.DownPayment);
                entity.Ignore(e => e.ProcessingFee);
                entity.Ignore(e => e.ActualFinancedAmount);
                entity.Ignore(e => e.PaymentFrequency);
                entity.Ignore(e => e.StartDate);
            });

            // RepaymentSchedule configuration
            modelBuilder.Entity<RepaymentSchedule>(entity =>
            {
                entity.HasOne(d => d.Loan)
                    .WithMany(p => p.RepaymentSchedules)
                    .HasForeignKey(d => d.LoanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment configuration (now includes bank transactions and bill payments)
            modelBuilder.Entity<Payment>(entity =>
            {
                // TEMPORARY: Ignore soft delete columns until EF migration is created
                // The columns may not exist in the database yet
                // TODO: Create EF migration to add these columns, then remove these Ignore() calls
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
                entity.HasOne(d => d.Loan)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.LoanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Bill)
                    .WithMany()
                    .HasForeignKey(d => d.BillId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.SavingsAccount)
                    .WithMany()
                    .HasForeignKey(d => d.SavingsAccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.BankAccount)
                    .WithMany()
                    .HasForeignKey(d => d.BankAccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Unique constraint for LoanId and Reference
                entity.HasIndex(e => new { e.LoanId, e.Reference })
                    .HasFilter("[LoanId] IS NOT NULL")
                    .IsUnique();

                // Unique constraint for BillId and Reference
                entity.HasIndex(e => new { e.BillId, e.Reference })
                    .HasFilter("[BillId] IS NOT NULL")
                    .IsUnique();

                // Unique constraint for SavingsAccountId and Reference
                entity.HasIndex(e => new { e.SavingsAccountId, e.Reference })
                    .HasFilter("[SavingsAccountId] IS NOT NULL")
                    .IsUnique();

                entity.HasIndex(e => e.ExternalTransactionId);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.IsBankTransaction);
                entity.HasIndex(e => e.BankAccountId);
                entity.HasIndex(e => e.IsDeleted);
            });


            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // TEMPORARY: Comment out Template relationship since TemplateId is ignored
                // TODO: Uncomment after creating EF migration for notification enhancements
                // entity.HasOne(d => d.Template)
                //     .WithMany()
                //     .HasForeignKey(d => d.TemplateId)
                //     .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => new { e.UserId, e.IsRead });
                
                // TEMPORARY: Ignore enhanced columns until EF migration is created
                // The columns exist in the database, but EF model snapshot doesn't include them
                // TODO: Create EF migration to add these columns, then remove these Ignore() calls
                entity.Ignore(e => e.Channel);
                entity.Ignore(e => e.Priority);
                entity.Ignore(e => e.ScheduledFor);
                entity.Ignore(e => e.TemplateId);
                entity.Ignore(e => e.TemplateVariables);
                entity.Ignore(e => e.Status);
                entity.Ignore(e => e.Template); // Also ignore navigation property
            });

            // NotificationPreference configuration
            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.NotificationType);
                entity.HasIndex(e => new { e.UserId, e.NotificationType }).IsUnique();
            });

            // NotificationTemplate configuration
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasIndex(e => e.NotificationType);
                entity.HasIndex(e => e.Channel);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Name);
            });

            // NotificationHistory configuration
            modelBuilder.Entity<NotificationHistory>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Notification)
                    .WithMany()
                    .HasForeignKey(d => d.NotificationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.NotificationId);
                entity.HasIndex(e => e.Channel);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            });

            // LoanApplication configuration
            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.LoanApplications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Bill configuration
            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Soft delete properties - columns now exist in database
                entity.HasIndex(e => e.IsDeleted);

                // Temporary: Ignore new properties until database migration is applied
                // TODO: Remove these Ignore() calls after running add_bill_columns_SIMPLE.sql
                entity.Ignore(e => e.IsScheduledPayment);
                entity.Ignore(e => e.ScheduledPaymentBankAccountId);
                entity.Ignore(e => e.ScheduledPaymentDaysBeforeDue);
                entity.Ignore(e => e.LastScheduledPaymentAttempt);
                entity.Ignore(e => e.ScheduledPaymentFailureReason);
                entity.Ignore(e => e.ApprovalStatus);
                entity.Ignore(e => e.ApprovedBy);
                entity.Ignore(e => e.ApprovedAt);
                entity.Ignore(e => e.ApprovalNotes);
            });

            // BankAccount configuration
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.AccountName }).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.AccountNumber }).IsUnique();

                // Soft delete properties - columns now exist in database
                entity.HasIndex(e => e.IsDeleted);
            });

            // BankTransaction configuration
            modelBuilder.Entity<BankTransaction>(entity =>
            {
                entity.HasOne(d => d.BankAccount)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.BankAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Bill)
                    .WithMany()
                    .HasForeignKey(d => d.BillId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Loan)
                    .WithMany()
                    .HasForeignKey(d => d.LoanId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.SavingsAccount)
                    .WithMany()
                    .HasForeignKey(d => d.SavingsAccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.ExternalTransactionId);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.IsDeleted);
                entity.HasIndex(e => e.BillId);
                entity.HasIndex(e => e.LoanId);
                entity.HasIndex(e => e.SavingsAccountId);
                entity.HasIndex(e => e.TransactionPurpose);
            });

            // ClosedMonth configuration
            modelBuilder.Entity<ClosedMonth>(entity =>
            {
                entity.HasOne(d => d.BankAccount)
                    .WithMany()
                    .HasForeignKey(d => d.BankAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.ClosedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                // Unique constraint: One closed month per BankAccount/Year/Month combination
                entity.HasIndex(e => new { e.BankAccountId, e.Year, e.Month }).IsUnique();
                
                entity.HasIndex(e => e.BankAccountId);
                entity.HasIndex(e => new { e.Year, e.Month });
                entity.HasIndex(e => e.ClosedAt);
            });

            // Card configuration
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasOne(d => d.BankAccount)
                    .WithMany(p => p.Cards)
                    .HasForeignKey(d => d.BankAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.BankAccountId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsPrimary);

                // Soft delete properties - columns now exist in database
                entity.HasIndex(e => e.IsDeleted);
            });

            // SavingsAccount configuration
            modelBuilder.Entity<SavingsAccount>(entity =>
            {
                // TEMPORARY: Ignore soft delete columns until EF migration is created
                // The columns may not exist in the database yet
                // TODO: Create EF migration to add these columns, then remove these Ignore() calls
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.AccountName }).IsUnique();

                // NOTE: Make sure add_startdate_to_savings_accounts.sql migration has been run!
                // If you get "Invalid column name 'StartDate'" error, run the migration first.

                // Soft delete properties - columns don't exist in database yet, so they're ignored above
            });

            // SavingsTransaction configuration
            modelBuilder.Entity<SavingsTransaction>(entity =>
            {
                entity.HasOne(d => d.SavingsAccount)
                    .WithMany(p => p.SavingsTransactions)
                    .HasForeignKey(d => d.SavingsAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.SourceBankAccount)
                    .WithMany()
                    .HasForeignKey(d => d.SourceBankAccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.TransactionDate);

                // Soft delete properties - columns now exist in database
                entity.HasIndex(e => e.IsDeleted);
            });

            // Receivable configuration
            modelBuilder.Entity<Receivable>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.BorrowerName);
                entity.HasIndex(e => e.LentAt);

                // Soft delete properties - columns now exist in database
                entity.HasIndex(e => e.IsDeleted);

                // Temporary: Ignore properties that may not exist in database yet
                // TODO: Remove these Ignore() calls after running database migration to add these columns
                // If database doesn't have these columns, uncomment these lines:
                entity.Ignore(e => e.PaymentFrequency);
                entity.Ignore(e => e.StartDate);
            });

            // ReceivablePayment configuration
            modelBuilder.Entity<ReceivablePayment>(entity =>
            {
                entity.HasOne(d => d.Receivable)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.ReceivableId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.BankAccount)
                    .WithMany()
                    .HasForeignKey(d => d.BankAccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.ReceivableId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PaymentDate);
                entity.HasIndex(e => e.Reference);
            });

            // UserProfile configuration
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Industry);
                entity.HasIndex(e => e.EmploymentType);
            });

            // IncomeSource configuration
            modelBuilder.Entity<IncomeSource>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Frequency);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();

                // Soft delete properties - columns now exist in database
                entity.HasIndex(e => e.IsDeleted);
            });

            // VariableExpense configuration
            modelBuilder.Entity<VariableExpense>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.ExpenseDate);
                entity.HasIndex(e => new { e.UserId, e.ExpenseDate, e.Category });

                // Soft delete properties - columns now exist in database
                entity.HasIndex(e => e.IsDeleted);
            });

            // BudgetSetting configuration
            modelBuilder.Entity<BudgetSetting>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Provider, e.BillType }).IsUnique();
            });

            // BillAnalyticsCache configuration
            modelBuilder.Entity<BillAnalyticsCache>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Provider, e.BillType, e.CalculationMonth }).IsUnique();
            });

            // BillAlert configuration
            modelBuilder.Entity<BillAlert>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Bill)
                    .WithMany()
                    .HasForeignKey(d => d.BillId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.AlertType);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
            });

            // UserOnboarding configuration
            modelBuilder.Entity<UserOnboarding>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.OnboardingCompleted);
                entity.HasIndex(e => e.CurrentStep);
            });

            // PasswordReset configuration
            modelBuilder.Entity<PasswordReset>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => e.IsUsed);
            });

            // ChatConversation configuration
            modelBuilder.Entity<ChatConversation>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.ChatConversations)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LastMessageAt);
                entity.HasIndex(e => e.StartedAt);
            });

            // ChatMessage configuration
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasOne(d => d.Conversation)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.ConversationId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.Timestamp);
            });

            // BankStatement configuration
            modelBuilder.Entity<BankStatement>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.BankAccount)
                    .WithMany()
                    .HasForeignKey(d => d.BankAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.BankAccountId);
                entity.HasIndex(e => new { e.BankAccountId, e.UserId }); // Composite index for common query pattern
                entity.HasIndex(e => e.StatementStartDate);
                entity.HasIndex(e => e.StatementEndDate);
                entity.HasIndex(e => e.IsReconciled);
            });

            // BankStatementItem configuration
            modelBuilder.Entity<BankStatementItem>(entity =>
            {
                entity.HasOne(d => d.BankStatement)
                    .WithMany(p => p.StatementItems)
                    .HasForeignKey(d => d.BankStatementId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.BankStatementId);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.IsMatched);
                entity.HasIndex(e => e.MatchedTransactionId);
            });

            // Reconciliation configuration
            modelBuilder.Entity<Reconciliation>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.BankAccount)
                    .WithMany()
                    .HasForeignKey(d => d.BankAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.BankStatement)
                    .WithMany(p => p.Reconciliations)
                    .HasForeignKey(d => d.BankStatementId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.BankAccountId);
                entity.HasIndex(e => e.ReconciliationDate);
                entity.HasIndex(e => e.Status);
            });

            // ReconciliationMatch configuration
            modelBuilder.Entity<ReconciliationMatch>(entity =>
            {
                entity.HasOne(d => d.Reconciliation)
                    .WithMany(p => p.Matches)
                    .HasForeignKey(d => d.ReconciliationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.StatementItem)
                    .WithMany()
                    .HasForeignKey(d => d.StatementItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.ReconciliationId);
                entity.HasIndex(e => e.SystemTransactionId);
                entity.HasIndex(e => e.StatementItemId);
                entity.HasIndex(e => e.MatchStatus);
            });

            // TransactionCategory configuration
            modelBuilder.Entity<TransactionCategory>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsSystemCategory);
                entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
            });

            // Vendor configuration
            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
            });

            // Utility configuration
            modelBuilder.Entity<Utility>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.UtilityType);
                entity.HasIndex(e => e.Provider);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.BillingDate);
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => new { e.UserId, e.UtilityType, e.Provider });
            });

            // Expense configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Category)
                    .WithMany(c => c.Expenses)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Receipt)
                    .WithMany()
                    .HasForeignKey(d => d.ReceiptId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Budget)
                    .WithMany(b => b.Expenses)
                    .HasForeignKey(d => d.BudgetId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.ExpenseDate);
                entity.HasIndex(e => e.ApprovalStatus);
                entity.HasIndex(e => e.BudgetId);
                entity.HasIndex(e => new { e.UserId, e.ExpenseDate });
                entity.HasIndex(e => new { e.UserId, e.CategoryId, e.ExpenseDate });
            });

            // ExpenseCategory configuration
            modelBuilder.Entity<ExpenseCategory>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ParentCategory)
                    .WithMany()
                    .HasForeignKey(d => d.ParentCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ParentCategoryId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
            });

            // ExpenseBudget configuration
            modelBuilder.Entity<ExpenseBudget>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Category)
                    .WithMany(c => c.Budgets)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.UserId, e.CategoryId, e.StartDate, e.EndDate });
            });

            // ExpenseReceipt configuration
            modelBuilder.Entity<ExpenseReceipt>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Expense)
                    .WithMany()
                    .HasForeignKey(d => d.ExpenseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpenseId);
                entity.HasIndex(e => e.IsOcrProcessed);
            });

            // ExpenseApproval configuration
            modelBuilder.Entity<ExpenseApproval>(entity =>
            {
                entity.HasOne(d => d.Expense)
                    .WithMany()
                    .HasForeignKey(d => d.ExpenseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.RequestedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ApprovedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.ExpenseId);
                entity.HasIndex(e => e.RequestedBy);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequestedAt);
            });

            // AllocationTemplate configuration
            modelBuilder.Entity<AllocationTemplate>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsSystemTemplate);
                entity.HasIndex(e => e.IsActive);
            });

            // AllocationTemplateCategory configuration
            modelBuilder.Entity<AllocationTemplateCategory>(entity =>
            {
                entity.HasOne(d => d.Template)
                    .WithMany(t => t.Categories)
                    .HasForeignKey(d => d.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TemplateId);
            });

            // AllocationPlan configuration
            modelBuilder.Entity<AllocationPlan>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Template)
                    .WithMany()
                    .HasForeignKey(d => d.TemplateId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.UserId, e.IsActive });
            });

            // AllocationCategory configuration
            modelBuilder.Entity<AllocationCategory>(entity =>
            {
                entity.HasOne(d => d.Plan)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(d => d.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PlanId);
            });

            // AllocationHistory configuration
            modelBuilder.Entity<AllocationHistory>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Plan)
                    .WithMany()
                    .HasForeignKey(d => d.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Category)
                    .WithMany()
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PlanId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.PeriodDate);
                entity.HasIndex(e => new { e.UserId, e.PlanId, e.PeriodDate });
            });

            // AllocationRecommendation configuration
            modelBuilder.Entity<AllocationRecommendation>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Plan)
                    .WithMany()
                    .HasForeignKey(d => d.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Category)
                    .WithMany()
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PlanId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.IsApplied);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.Priority);
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull); // Don't cascade delete to preserve audit trail

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.LogType);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ComplianceType);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
                entity.HasIndex(e => new { e.LogType, e.Severity });
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = "admin-001",
                    Name = "System Administrator",
                    Email = "admin@utilityhub360.com",
                    Phone = "+1234567890",
                    Role = "ADMIN",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
