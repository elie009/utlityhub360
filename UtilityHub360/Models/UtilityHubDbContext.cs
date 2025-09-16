using System.Data.Entity;

namespace UtilityHub360.Models
{
    public class UtilityHubDbContext : DbContext
    {
        public UtilityHubDbContext() : base("AppConn03")
        {
        }

        public DbSet<User> Users { get; set; }
        
        // Loan Management System entities
        public DbSet<Borrower> Borrowers { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<RepaymentSchedule> RepaymentSchedules { get; set; }
        public DbSet<LoanPayment> Payments { get; set; }
        public DbSet<LoanPenalty> Penalties { get; set; }
        public DbSet<LoanNotification> Notifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedDate)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .IsRequired();

            // Configure Loan Management entities
            ConfigureBorrowerEntity(modelBuilder);
            ConfigureLoanEntity(modelBuilder);
            ConfigureRepaymentScheduleEntity(modelBuilder);
            ConfigurePaymentEntity(modelBuilder);
            ConfigurePenaltyEntity(modelBuilder);
            ConfigureNotificationEntity(modelBuilder);
        }

        private void ConfigureBorrowerEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Borrower>()
                .HasKey(b => b.BorrowerId);

            modelBuilder.Entity<Borrower>()
                .Property(b => b.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Borrower>()
                .Property(b => b.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Borrower>()
                .Property(b => b.Email)
                .HasMaxLength(200);

            modelBuilder.Entity<Borrower>()
                .Property(b => b.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<Borrower>()
                .Property(b => b.Address)
                .HasMaxLength(255);

            modelBuilder.Entity<Borrower>()
                .Property(b => b.GovernmentId)
                .HasMaxLength(50);

            modelBuilder.Entity<Borrower>()
                .Property(b => b.Status)
                .HasMaxLength(20);
        }

        private void ConfigureLoanEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Loan>()
                .HasKey(l => l.LoanId);

            modelBuilder.Entity<Loan>()
                .HasRequired(l => l.Borrower)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BorrowerId);

            modelBuilder.Entity<Loan>()
                .Property(l => l.PrincipalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Loan>()
                .Property(l => l.InterestRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Loan>()
                .Property(l => l.LoanType)
                .HasMaxLength(50);

            modelBuilder.Entity<Loan>()
                .Property(l => l.RepaymentFrequency)
                .HasMaxLength(20);

            modelBuilder.Entity<Loan>()
                .Property(l => l.AmortizationType)
                .HasMaxLength(20);

            modelBuilder.Entity<Loan>()
                .Property(l => l.Status)
                .HasMaxLength(20);
        }

        private void ConfigureRepaymentScheduleEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepaymentSchedule>()
                .HasKey(rs => rs.ScheduleId);

            modelBuilder.Entity<RepaymentSchedule>()
                .HasRequired(rs => rs.Loan)
                .WithMany(l => l.RepaymentSchedules)
                .HasForeignKey(rs => rs.LoanId);

            modelBuilder.Entity<RepaymentSchedule>()
                .Property(rs => rs.AmountDue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RepaymentSchedule>()
                .Property(rs => rs.PrincipalPortion)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RepaymentSchedule>()
                .Property(rs => rs.InterestPortion)
                .HasPrecision(18, 2);
        }

        private void ConfigurePaymentEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanPayment>()
                .HasKey(p => p.PaymentId);

            modelBuilder.Entity<LoanPayment>()
                .HasRequired(p => p.Loan)
                .WithMany(l => l.Payments)
                .HasForeignKey(p => p.LoanId);

            modelBuilder.Entity<LoanPayment>()
                .HasOptional(p => p.RepaymentSchedule)
                .WithMany(rs => rs.Payments)
                .HasForeignKey(p => p.ScheduleId);

            modelBuilder.Entity<LoanPayment>()
                .Property(p => p.AmountPaid)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoanPayment>()
                .Property(p => p.PaymentMethod)
                .HasMaxLength(50);

            modelBuilder.Entity<LoanPayment>()
                .Property(p => p.Notes)
                .HasMaxLength(255);
        }

        private void ConfigurePenaltyEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanPenalty>()
                .HasKey(p => p.PenaltyId);

            modelBuilder.Entity<LoanPenalty>()
                .HasRequired(p => p.Loan)
                .WithMany(l => l.Penalties)
                .HasForeignKey(p => p.LoanId);

            modelBuilder.Entity<LoanPenalty>()
                .HasRequired(p => p.RepaymentSchedule)
                .WithMany(rs => rs.Penalties)
                .HasForeignKey(p => p.ScheduleId);

            modelBuilder.Entity<LoanPenalty>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }

        private void ConfigureNotificationEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanNotification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<LoanNotification>()
                .HasRequired(n => n.Borrower)
                .WithMany(b => b.Notifications)
                .HasForeignKey(n => n.BorrowerId);

            modelBuilder.Entity<LoanNotification>()
                .HasOptional(n => n.Loan)
                .WithMany()
                .HasForeignKey(n => n.LoanId);

            modelBuilder.Entity<LoanNotification>()
                .Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<LoanNotification>()
                .Property(n => n.NotificationType)
                .HasMaxLength(20);
        }
    }
}
