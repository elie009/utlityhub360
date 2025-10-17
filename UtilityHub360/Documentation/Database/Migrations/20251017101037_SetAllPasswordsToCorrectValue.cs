using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class SetAllPasswordsToCorrectValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set all users' password to "p@$$w0rdBAH"
            // Using BCrypt hash of "p@$$w0rdBAH" - this is the hashed version
            migrationBuilder.Sql("UPDATE [Users] SET [PasswordHash] = '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi'");
            
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 10, 10, 36, 581, DateTimeKind.Utc).AddTicks(8935), new DateTime(2025, 10, 17, 10, 10, 36, 581, DateTimeKind.Utc).AddTicks(8936) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 10, 6, 52, 925, DateTimeKind.Utc).AddTicks(2468), new DateTime(2025, 10, 17, 10, 6, 52, 925, DateTimeKind.Utc).AddTicks(2468) });
        }
    }
}
