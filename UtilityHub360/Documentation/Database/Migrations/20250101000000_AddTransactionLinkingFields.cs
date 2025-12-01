using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionLinkingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add linking fields to BankTransactions table
            migrationBuilder.AddColumn<string>(
                name: "BillId",
                table: "BankTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoanId",
                table: "BankTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SavingsAccountId",
                table: "BankTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionPurpose",
                table: "BankTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Create indexes for the new foreign key columns
            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_BillId",
                table: "BankTransactions",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_LoanId",
                table: "BankTransactions",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_SavingsAccountId",
                table: "BankTransactions",
                column: "SavingsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_TransactionPurpose",
                table: "BankTransactions",
                column: "TransactionPurpose");

            // Add foreign key constraints
            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Bills_BillId",
                table: "BankTransactions",
                column: "BillId",
                principalTable: "Bills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Loans_LoanId",
                table: "BankTransactions",
                column: "LoanId",
                principalTable: "Loans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_SavingsAccounts_SavingsAccountId",
                table: "BankTransactions",
                column: "SavingsAccountId",
                principalTable: "SavingsAccounts",
                principalColumn: "Id");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { DateTime.UtcNow, DateTime.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Bills_BillId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Loans_LoanId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_SavingsAccounts_SavingsAccountId",
                table: "BankTransactions");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_BillId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_LoanId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_SavingsAccountId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_TransactionPurpose",
                table: "BankTransactions");

            // Drop columns
            migrationBuilder.DropColumn(
                name: "BillId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "LoanId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "SavingsAccountId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionPurpose",
                table: "BankTransactions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { DateTime.UtcNow, DateTime.UtcNow });
        }
    }
}

