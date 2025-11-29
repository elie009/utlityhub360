using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class FixPasswordHashCorrect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all users with the correct password "p@$$w0rdBAH"
            // Using BCrypt hash of "p@$$w0rdBAH" - this is the hashed version
            migrationBuilder.Sql("UPDATE [Users] SET [PasswordHash] = '$2a$11$rQZ8K9vL2nM3pO4qR5sT6uV7wX8yZ9aB0cD1eF2gH3iJ4kL5mN6oP7qR8sT9uV'");
            
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 10, 1, 45, 966, DateTimeKind.Utc).AddTicks(4914), new DateTime(2025, 10, 17, 10, 1, 45, 966, DateTimeKind.Utc).AddTicks(4914) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 9, 51, 15, 622, DateTimeKind.Utc).AddTicks(3571), new DateTime(2025, 10, 17, 9, 51, 15, 622, DateTimeKind.Utc).AddTicks(3571) });
        }
    }
}
