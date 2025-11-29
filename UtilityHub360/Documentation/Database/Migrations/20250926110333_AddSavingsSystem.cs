using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavingsAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SavingsType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Goal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingsTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SavingsAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SourceBankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurringFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsTransactions_BankAccounts_SourceBankAccountId",
                        column: x => x.SourceBankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SavingsTransactions_SavingsAccounts_SavingsAccountId",
                        column: x => x.SavingsAccountId,
                        principalTable: "SavingsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 11, 3, 31, 849, DateTimeKind.Utc).AddTicks(7617), new DateTime(2025, 9, 26, 11, 3, 31, 849, DateTimeKind.Utc).AddTicks(7618) });

            migrationBuilder.CreateIndex(
                name: "IX_SavingsAccounts_UserId_AccountName",
                table: "SavingsAccounts",
                columns: new[] { "UserId", "AccountName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingsTransactions_SavingsAccountId",
                table: "SavingsTransactions",
                column: "SavingsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsTransactions_SourceBankAccountId",
                table: "SavingsTransactions",
                column: "SourceBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsTransactions_TransactionDate",
                table: "SavingsTransactions",
                column: "TransactionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavingsTransactions");

            migrationBuilder.DropTable(
                name: "SavingsAccounts");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 25, 16, 36, 38, 539, DateTimeKind.Utc).AddTicks(2646), new DateTime(2025, 9, 25, 16, 36, 38, 539, DateTimeKind.Utc).AddTicks(2647) });
        }
    }
}
