using System.Data.Entity;
using System.Data.Entity.Migrations;
using UtilityHub360.Models;

namespace UtilityHub360.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<UtilityHubDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(UtilityHubDbContext context)
        {
            // This method will be called after migrating to the latest version.
            // You can use the DbSet<T>.AddOrUpdate() helper extension method 
            // to avoid creating duplicate seed data.
        }
    }
}
