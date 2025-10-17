using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultPasswordForAllUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set default password "Demo123!" for all users
            // Using BCrypt hash of "Demo123!" - this is the hashed version
            migrationBuilder.Sql("UPDATE [Users] SET [PasswordHash] = '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi' WHERE [PasswordHash] = '' OR [PasswordHash] IS NULL");
            
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 18, 37, 12, 32, DateTimeKind.Utc).AddTicks(3737), new DateTime(2025, 10, 16, 18, 37, 12, 32, DateTimeKind.Utc).AddTicks(3740) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 18, 29, 39, 382, DateTimeKind.Utc).AddTicks(6979), new DateTime(2025, 10, 16, 18, 29, 39, 382, DateTimeKind.Utc).AddTicks(6982) });
        }
    }
}
