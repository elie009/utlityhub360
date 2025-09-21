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
        public DbSet<Loan> Loans { get; set; }
        public DbSet<RepaymentSchedule> RepaymentSchedules { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            ConfigureUserEntity(modelBuilder);
            ConfigureLoanEntity(modelBuilder);
            ConfigureRepaymentScheduleEntity(modelBuilder);
            ConfigurePaymentEntity(modelBuilder);
            ConfigureTransactionEntity(modelBuilder);
            ConfigureNotificationEntity(modelBuilder);
            ConfigureLoanApplicationEntity(modelBuilder);
        }

        private void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(u => u.Phone)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(u => u.Role)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(u => u.IsActive)
                    .IsRequired();
                entity.Property(u => u.CreatedAt)
                    .IsRequired();
                entity.Property(u => u.UpdatedAt)
                    .IsRequired();
            });
        }

        private void ConfigureLoanEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.HasOne(l => l.User)
                    .WithMany(u => u.Loans)
                    .HasForeignKey(l => l.UserId);
                entity.Property(l => l.Principal)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(l => l.InterestRate)
                    .IsRequired()
                    .HasPrecision(5, 2);
                entity.Property(l => l.Term)
                    .IsRequired();
                entity.Property(l => l.Purpose)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(l => l.Status)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(l => l.MonthlyPayment)
                    .HasPrecision(18, 2);
                entity.Property(l => l.TotalAmount)
                    .HasPrecision(18, 2);
                entity.Property(l => l.RemainingBalance)
                    .HasPrecision(18, 2);
                entity.Property(l => l.AppliedAt)
                    .IsRequired();
                entity.Property(l => l.AdditionalInfo)
                    .HasMaxLength(1000);
            });
        }

        private void ConfigureRepaymentScheduleEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepaymentSchedule>(entity =>
            {
                entity.HasKey(rs => rs.Id);
                entity.HasOne(rs => rs.Loan)
                    .WithMany(l => l.RepaymentSchedules)
                    .HasForeignKey(rs => rs.LoanId);
                entity.Property(rs => rs.InstallmentNumber)
                    .IsRequired();
                entity.Property(rs => rs.DueDate)
                    .IsRequired();
                entity.Property(rs => rs.PrincipalAmount)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(rs => rs.InterestAmount)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(rs => rs.TotalAmount)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(rs => rs.Status)
                    .IsRequired()
                    .HasConversion<string>();
            });
        }

        private void ConfigurePaymentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasOne(p => p.Loan)
                    .WithMany(l => l.Payments)
                    .HasForeignKey(p => p.LoanId);
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Payments)
                    .HasForeignKey(p => p.UserId);
                entity.Property(p => p.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(p => p.Method)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(p => p.Reference)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(p => p.Status)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(p => p.ProcessedAt)
                    .IsRequired();
                entity.Property(p => p.CreatedAt)
                    .IsRequired();
            });
        }

        private void ConfigureTransactionEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasOne(t => t.Loan)
                    .WithMany(l => l.Transactions)
                    .HasForeignKey(t => t.LoanId);
                entity.Property(t => t.Type)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(t => t.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(t => t.Description)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(t => t.Reference)
                    .HasMaxLength(100);
                entity.Property(t => t.CreatedAt)
                    .IsRequired();
            });
        }

        private void ConfigureNotificationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId);
                entity.Property(n => n.Type)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(n => n.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(n => n.Message)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(n => n.IsRead)
                    .IsRequired();
                entity.Property(n => n.CreatedAt)
                    .IsRequired();
            });
        }
        
        private void ConfigureLoanApplicationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.HasKey(la => la.Id);
                entity.HasOne(la => la.User)
                    .WithMany(u => u.LoanApplications)
                    .HasForeignKey(la => la.UserId);
                entity.Property(la => la.Principal)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(la => la.Purpose)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(la => la.Term)
                    .IsRequired();
                entity.Property(la => la.MonthlyIncome)
                    .IsRequired()
                    .HasPrecision(18, 2);
                entity.Property(la => la.EmploymentStatus)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(la => la.AdditionalInfo)
                    .HasMaxLength(1000);
                entity.Property(la => la.Status)
                    .IsRequired()
                    .HasConversion<string>();
                entity.Property(la => la.AppliedAt)
                    .IsRequired();
                entity.Property(la => la.ReviewedBy)
                    .HasMaxLength(100);
                entity.Property(la => la.RejectionReason)
                    .HasMaxLength(500);
            });
        }
    }
}

