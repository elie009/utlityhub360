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
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<SavingsTransaction> SavingsTransactions { get; set; }
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

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running apply_soft_delete_migration.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
            });


            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
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

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running apply_soft_delete_migration.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
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

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running add_bankaccount_softdelete.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
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

                entity.HasIndex(e => e.ExternalTransactionId);
                entity.HasIndex(e => e.TransactionDate);

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running apply_soft_delete_migration.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
            });

            // SavingsAccount configuration
            modelBuilder.Entity<SavingsAccount>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.AccountName }).IsUnique();

                // NOTE: Make sure add_startdate_to_savings_accounts.sql migration has been run!
                // If you get "Invalid column name 'StartDate'" error, run the migration first.

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running apply_soft_delete_migration.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
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

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running apply_soft_delete_migration.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
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

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running apply_soft_delete_migration.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
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

                // Temporary: Ignore soft delete properties until migration is applied
                // TODO: Remove these Ignore() calls after running apply_soft_delete_migration.sql
                entity.Ignore(e => e.IsDeleted);
                entity.Ignore(e => e.DeletedAt);
                entity.Ignore(e => e.DeletedBy);
                entity.Ignore(e => e.DeleteReason);
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
