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
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 21, 19, 2, 541, DateTimeKind.Utc).AddTicks(7716), new DateTime(2025, 10, 17, 21, 19, 2, 541, DateTimeKind.Utc).AddTicks(7716) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 14, 12, 57, 640, DateTimeKind.Utc).AddTicks(6788), new DateTime(2025, 10, 17, 14, 12, 57, 640, DateTimeKind.Utc).AddTicks(6788) });
        }
    }
}
