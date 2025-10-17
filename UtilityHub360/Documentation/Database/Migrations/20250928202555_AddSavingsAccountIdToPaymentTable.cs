using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingsAccountIdToPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SavingsAccountId",
                table: "Payments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 28, 20, 25, 55, 33, DateTimeKind.Utc).AddTicks(2873), new DateTime(2025, 9, 28, 20, 25, 55, 33, DateTimeKind.Utc).AddTicks(2874) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SavingsAccountId_Reference",
                table: "Payments",
                columns: new[] { "SavingsAccountId", "Reference" },
                unique: true,
                filter: "[SavingsAccountId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_SavingsAccounts_SavingsAccountId",
                table: "Payments",
                column: "SavingsAccountId",
                principalTable: "SavingsAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_SavingsAccounts_SavingsAccountId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SavingsAccountId_Reference",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SavingsAccountId",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 28, 18, 52, 54, 766, DateTimeKind.Utc).AddTicks(5772), new DateTime(2025, 9, 28, 18, 52, 54, 766, DateTimeKind.Utc).AddTicks(5774) });
        }
    }
}
