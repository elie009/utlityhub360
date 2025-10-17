using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class MergeBankTransactionsToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_LoanId_Reference",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "LoanId",
                table: "Payments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfterTransaction",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountId",
                table: "Payments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Payments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalTransactionId",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBankTransaction",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Merchant",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurringFrequency",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 27, 8, 32, 23, 939, DateTimeKind.Utc).AddTicks(939), new DateTime(2025, 9, 27, 8, 32, 23, 939, DateTimeKind.Utc).AddTicks(939) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BankAccountId",
                table: "Payments",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalTransactionId",
                table: "Payments",
                column: "ExternalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IsBankTransaction",
                table: "Payments",
                column: "IsBankTransaction");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LoanId_Reference",
                table: "Payments",
                columns: new[] { "LoanId", "Reference" },
                unique: true,
                filter: "[LoanId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionDate",
                table: "Payments",
                column: "TransactionDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            // Migrate data from BankTransactions to Payments
            migrationBuilder.Sql(@"
                INSERT INTO Payments (
                    Id, LoanId, BankAccountId, UserId, Amount, Method, Reference, Status, 
                    ProcessedAt, IsBankTransaction, TransactionType, Description, Category, 
                    ExternalTransactionId, Notes, Merchant, Location, IsRecurring, 
                    RecurringFrequency, Currency, BalanceAfterTransaction, TransactionDate, 
                    CreatedAt, UpdatedAt
                )
                SELECT 
                    Id, 
                    NULL as LoanId,
                    BankAccountId,
                    UserId,
                    Amount,
                    CASE 
                        WHEN TransactionType = 'CREDIT' THEN 'BANK_TRANSFER'
                        WHEN TransactionType = 'DEBIT' THEN 'BANK_TRANSFER'
                        ELSE 'BANK_TRANSFER'
                    END as Method,
                    ISNULL(ReferenceNumber, 'BANK_TXN_' + Id) as Reference,
                    'COMPLETED' as Status,
                    TransactionDate as ProcessedAt,
                    1 as IsBankTransaction,
                    TransactionType,
                    Description,
                    Category,
                    ExternalTransactionId,
                    Notes,
                    Merchant,
                    Location,
                    IsRecurring,
                    RecurringFrequency,
                    Currency,
                    BalanceAfterTransaction,
                    TransactionDate,
                    CreatedAt,
                    UpdatedAt
                FROM BankTransactions
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove migrated bank transaction data from Payments
            migrationBuilder.Sql(@"
                DELETE FROM Payments WHERE IsBankTransaction = 1
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BankAccountId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ExternalTransactionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_IsBankTransaction",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_LoanId_Reference",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TransactionDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BalanceAfterTransaction",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExternalTransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsBankTransaction",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Merchant",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RecurringFrequency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "LoanId",
                table: "Payments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 14, 51, 58, 587, DateTimeKind.Utc).AddTicks(370), new DateTime(2025, 9, 26, 14, 51, 58, 587, DateTimeKind.Utc).AddTicks(370) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LoanId_Reference",
                table: "Payments",
                columns: new[] { "LoanId", "Reference" },
                unique: true);
        }
    }
}
