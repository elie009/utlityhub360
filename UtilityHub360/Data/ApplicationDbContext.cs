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
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }

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
            });

            // RepaymentSchedule configuration
            modelBuilder.Entity<RepaymentSchedule>(entity =>
            {
                entity.HasOne(d => d.Loan)
                    .WithMany(p => p.RepaymentSchedules)
                    .HasForeignKey(d => d.LoanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(d => d.Loan)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.LoanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => new { e.LoanId, e.Reference }).IsUnique();
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(d => d.Loan)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.LoanId)
                    .OnDelete(DeleteBehavior.Cascade);
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
