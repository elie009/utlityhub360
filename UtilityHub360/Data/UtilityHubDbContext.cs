using Microsoft.EntityFrameworkCore;
using UtilityHub360.Models;

namespace UtilityHub360.Data
{
    public class UtilityHubDbContext : DbContext
    {
        public UtilityHubDbContext(DbContextOptions<UtilityHubDbContext> options) : base(options)
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(u => u.CreatedDate)
                    .IsRequired();
                entity.Property(u => u.IsActive)
                    .IsRequired();
            });

            // Configure Loan Management entities
            ConfigureBorrowerEntity(modelBuilder);
            ConfigureLoanEntity(modelBuilder);
            ConfigureRepaymentScheduleEntity(modelBuilder);
            ConfigurePaymentEntity(modelBuilder);
            ConfigurePenaltyEntity(modelBuilder);
            ConfigureNotificationEntity(modelBuilder);
        }

        private void ConfigureBorrowerEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Borrower>(entity =>
            {
                entity.HasKey(b => b.BorrowerId);
                entity.Property(b => b.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(b => b.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(b => b.Email)
                    .HasMaxLength(200);
                entity.Property(b => b.Phone)
                    .HasMaxLength(20);
                entity.Property(b => b.Address)
                    .HasMaxLength(255);
                entity.Property(b => b.GovernmentId)
                    .HasMaxLength(50);
                entity.Property(b => b.Status)
                    .HasMaxLength(20);
            });
        }

        private void ConfigureLoanEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasKey(l => l.LoanId);
                entity.HasOne(l => l.Borrower)
                    .WithMany(b => b.Loans)
                    .HasForeignKey(l => l.BorrowerId);
                entity.Property(l => l.PrincipalAmount)
                    .HasPrecision(18, 2);
                entity.Property(l => l.InterestRate)
                    .HasPrecision(5, 2);
                entity.Property(l => l.LoanType)
                    .HasMaxLength(50);
                entity.Property(l => l.RepaymentFrequency)
                    .HasMaxLength(20);
                entity.Property(l => l.AmortizationType)
                    .HasMaxLength(20);
                entity.Property(l => l.Status)
                    .HasMaxLength(20);
            });
        }

        private void ConfigureRepaymentScheduleEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepaymentSchedule>(entity =>
            {
                entity.HasKey(rs => rs.ScheduleId);
                entity.HasOne(rs => rs.Loan)
                    .WithMany(l => l.RepaymentSchedules)
                    .HasForeignKey(rs => rs.LoanId);
                entity.Property(rs => rs.AmountDue)
                    .HasPrecision(18, 2);
                entity.Property(rs => rs.PrincipalPortion)
                    .HasPrecision(18, 2);
                entity.Property(rs => rs.InterestPortion)
                    .HasPrecision(18, 2);
            });
        }

        private void ConfigurePaymentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanPayment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);
                entity.HasOne(p => p.Loan)
                    .WithMany(l => l.Payments)
                    .HasForeignKey(p => p.LoanId);
                entity.HasOne(p => p.RepaymentSchedule)
                    .WithMany(rs => rs.Payments)
                    .HasForeignKey(p => p.ScheduleId)
                    .IsRequired(false);
                entity.Property(p => p.AmountPaid)
                    .HasPrecision(18, 2);
                entity.Property(p => p.PaymentMethod)
                    .HasMaxLength(50);
                entity.Property(p => p.Notes)
                    .HasMaxLength(255);
            });
        }

        private void ConfigurePenaltyEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanPenalty>(entity =>
            {
                entity.HasKey(p => p.PenaltyId);
                entity.HasOne(p => p.Loan)
                    .WithMany(l => l.Penalties)
                    .HasForeignKey(p => p.LoanId);
                entity.HasOne(p => p.RepaymentSchedule)
                    .WithMany(rs => rs.Penalties)
                    .HasForeignKey(p => p.ScheduleId);
                entity.Property(p => p.Amount)
                    .HasPrecision(18, 2);
            });
        }

        private void ConfigureNotificationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanNotification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);
                entity.HasOne(n => n.Borrower)
                    .WithMany(b => b.Notifications)
                    .HasForeignKey(n => n.BorrowerId);
                entity.HasOne(n => n.Loan)
                    .WithMany()
                    .HasForeignKey(n => n.LoanId)
                    .IsRequired(false);
                entity.Property(n => n.Message)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(n => n.NotificationType)
                    .HasMaxLength(20);
            });
        }
    }
}

