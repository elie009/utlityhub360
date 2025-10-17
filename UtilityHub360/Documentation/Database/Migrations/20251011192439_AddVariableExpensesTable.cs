using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddVariableExpensesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create VariableExpenses table
            migrationBuilder.CreateTable(
                name: "VariableExpenses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Merchant = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariableExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariableExpenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for VariableExpenses
            migrationBuilder.CreateIndex(
                name: "IX_VariableExpenses_UserId",
                table: "VariableExpenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VariableExpenses_Category",
                table: "VariableExpenses",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_VariableExpenses_ExpenseDate",
                table: "VariableExpenses",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_VariableExpenses_UserId_ExpenseDate_Category",
                table: "VariableExpenses",
                columns: new[] { "UserId", "ExpenseDate", "Category" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 11, 19, 24, 39, 350, DateTimeKind.Utc).AddTicks(4722), new DateTime(2025, 10, 11, 19, 24, 39, 350, DateTimeKind.Utc).AddTicks(4722) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop VariableExpenses table
            migrationBuilder.DropTable(
                name: "VariableExpenses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 11, 16, 43, 5, 987, DateTimeKind.Utc).AddTicks(8301), new DateTime(2025, 10, 11, 16, 43, 5, 987, DateTimeKind.Utc).AddTicks(8301) });
        }
    }
}
