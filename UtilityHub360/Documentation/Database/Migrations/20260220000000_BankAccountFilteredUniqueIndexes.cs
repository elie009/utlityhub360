using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <summary>
    /// Replaces BankAccounts unique indexes with filtered unique indexes so that
    /// duplicate account name/number is only enforced among non-deleted accounts.
    /// This allows reusing an account name when the existing account is soft-deleted (IsDeleted = 1).
    /// </summary>
    public partial class BankAccountFilteredUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_UserId_AccountName",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_UserId_AccountNumber",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_UserId_AccountName",
                table: "BankAccounts",
                columns: new[] { "UserId", "AccountName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_UserId_AccountNumber",
                table: "BankAccounts",
                columns: new[] { "UserId", "AccountNumber" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [AccountNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_UserId_AccountName",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_UserId_AccountNumber",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_UserId_AccountName",
                table: "BankAccounts",
                columns: new[] { "UserId", "AccountName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_UserId_AccountNumber",
                table: "BankAccounts",
                columns: new[] { "UserId", "AccountNumber" },
                unique: true,
                filter: "[AccountNumber] IS NOT NULL");
        }
    }
}
