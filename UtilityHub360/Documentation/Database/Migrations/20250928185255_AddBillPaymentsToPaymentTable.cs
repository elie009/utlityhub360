using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddBillPaymentsToPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillId",
                table: "Payments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 28, 18, 52, 54, 766, DateTimeKind.Utc).AddTicks(5772), new DateTime(2025, 9, 28, 18, 52, 54, 766, DateTimeKind.Utc).AddTicks(5774) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BillId_Reference",
                table: "Payments",
                columns: new[] { "BillId", "Reference" },
                unique: true,
                filter: "[BillId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bills_BillId",
                table: "Payments",
                column: "BillId",
                principalTable: "Bills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bills_BillId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BillId_Reference",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BillId",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 27, 9, 21, 16, 795, DateTimeKind.Utc).AddTicks(5212), new DateTime(2025, 9, 27, 9, 21, 16, 795, DateTimeKind.Utc).AddTicks(5213) });
        }
    }
}
