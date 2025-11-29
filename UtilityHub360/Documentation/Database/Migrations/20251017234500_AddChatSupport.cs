using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 23, 44, 58, 305, DateTimeKind.Utc).AddTicks(7004), new DateTime(2025, 10, 17, 23, 44, 58, 305, DateTimeKind.Utc).AddTicks(7005) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 21, 24, 12, 699, DateTimeKind.Utc).AddTicks(2682), new DateTime(2025, 10, 17, 21, 24, 12, 699, DateTimeKind.Utc).AddTicks(2683) });
        }
    }
}
