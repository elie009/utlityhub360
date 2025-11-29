using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredCurrencyToUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 25, 11, 37, 57, 524, DateTimeKind.Utc).AddTicks(3323), new DateTime(2025, 10, 25, 11, 37, 57, 524, DateTimeKind.Utc).AddTicks(3323) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 18, 8, 19, 53, 825, DateTimeKind.Utc).AddTicks(5023), new DateTime(2025, 10, 18, 8, 19, 53, 825, DateTimeKind.Utc).AddTicks(5024) });
        }
    }
}
