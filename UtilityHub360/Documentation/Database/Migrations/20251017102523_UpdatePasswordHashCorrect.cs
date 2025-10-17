using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordHashCorrect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all users with the correct password "p@$$w0rdBAH"
            // Using a known working BCrypt hash for "p@$$w0rdBAH"
            migrationBuilder.Sql("UPDATE [Users] SET [PasswordHash] = '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdQvOQ5aOqTkKZWj6HyAGSWyRzGxy'");
            
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 10, 25, 23, 121, DateTimeKind.Utc).AddTicks(3819), new DateTime(2025, 10, 17, 10, 25, 23, 121, DateTimeKind.Utc).AddTicks(3820) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 10, 10, 36, 581, DateTimeKind.Utc).AddTicks(8935), new DateTime(2025, 10, 17, 10, 10, 36, 581, DateTimeKind.Utc).AddTicks(8936) });
        }
    }
}
