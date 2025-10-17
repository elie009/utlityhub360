using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 12, 28, 21, 767, DateTimeKind.Utc).AddTicks(7956), new DateTime(2025, 9, 26, 12, 28, 21, 767, DateTimeKind.Utc).AddTicks(7956) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 11, 3, 31, 849, DateTimeKind.Utc).AddTicks(7617), new DateTime(2025, 9, 26, 11, 3, 31, 849, DateTimeKind.Utc).AddTicks(7618) });
        }
    }
}
