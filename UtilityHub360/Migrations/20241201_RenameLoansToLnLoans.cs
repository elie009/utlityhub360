using System.Data.Entity.Migrations;

namespace UtilityHub360.Migrations
{
    public partial class RenameLoansToLnLoans : DbMigration
    {
        public override void Up()
        {
            // Rename the table from Loans to LnLoans
            RenameTable(name: "dbo.Loans", newName: "LnLoans");
            
            // Update foreign key constraint names to reference the new table name
            RenameIndex(table: "dbo.RepaymentSchedules", name: "IX_LoanId", newName: "IX_LoanId");
            RenameIndex(table: "dbo.Payments", name: "IX_LoanId", newName: "IX_LoanId");
            RenameIndex(table: "dbo.Penalties", name: "IX_LoanId", newName: "IX_LoanId");
            RenameIndex(table: "dbo.Notifications", name: "IX_LoanId", newName: "IX_LoanId");
        }
        
        public override void Down()
        {
            // Revert the table name back to Loans
            RenameTable(name: "dbo.LnLoans", newName: "Loans");
            
            // Revert foreign key constraint names
            RenameIndex(table: "dbo.RepaymentSchedules", name: "IX_LoanId", newName: "IX_LoanId");
            RenameIndex(table: "dbo.Payments", name: "IX_LoanId", newName: "IX_LoanId");
            RenameIndex(table: "dbo.Penalties", name: "IX_LoanId", newName: "IX_LoanId");
            RenameIndex(table: "dbo.Notifications", name: "IX_LoanId", newName: "IX_LoanId");
        }
    }
}
