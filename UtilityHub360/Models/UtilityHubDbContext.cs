using System.Data.Entity;

namespace UtilityHub360.Models
{
    public class UtilityHubDbContext : DbContext
    {
        public UtilityHubDbContext() : base("AppConn03")
        {
        }

        public DbSet<User> Users { get; set; }

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
        }
    }
}
