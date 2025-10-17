using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class FixPasswordHashFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all users with the correct password "p@$$w0rdBAH"
            // Using BCrypt hash of "p@$$w0rdBAH" - this is the hashed version
            migrationBuilder.Sql("UPDATE [Users] SET [PasswordHash] = '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi'");
            
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 10, 6, 52, 925, DateTimeKind.Utc).AddTicks(2468), new DateTime(2025, 10, 17, 10, 6, 52, 925, DateTimeKind.Utc).AddTicks(2468) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 10, 1, 45, 966, DateTimeKind.Utc).AddTicks(4914), new DateTime(2025, 10, 17, 10, 1, 45, 966, DateTimeKind.Utc).AddTicks(4914) });
        }
    }
}
