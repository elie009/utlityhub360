using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddParentBillIdToBills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentBillId",
                table: "Bills",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 11, 16, 43, 5, 987, DateTimeKind.Utc).AddTicks(8301), new DateTime(2025, 10, 11, 16, 43, 5, 987, DateTimeKind.Utc).AddTicks(8301) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentBillId",
                table: "Bills");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 11, 0, 15, 55, 971, DateTimeKind.Utc).AddTicks(6829), new DateTime(2025, 10, 11, 0, 15, 55, 971, DateTimeKind.Utc).AddTicks(6829) });
        }
    }
}
