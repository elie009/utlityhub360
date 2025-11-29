using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add soft delete columns to Bills table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Bills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Bills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Bills",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "Bills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Add soft delete columns to Payments table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Payments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Add soft delete columns to BankTransactions table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BankTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BankTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "BankTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "BankTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Add soft delete columns to IncomeSources table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "IncomeSources",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "IncomeSources",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "IncomeSources",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "IncomeSources",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Add soft delete columns to VariableExpenses table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VariableExpenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "VariableExpenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "VariableExpenses",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "VariableExpenses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Add soft delete columns to SavingsTransactions table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SavingsTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SavingsTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SavingsTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "SavingsTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 28, 16, 1, 45, 213, DateTimeKind.Utc).AddTicks(3151), new DateTime(2025, 10, 28, 16, 1, 45, 213, DateTimeKind.Utc).AddTicks(3152) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove soft delete columns from Bills table
            migrationBuilder.DropColumn(name: "IsDeleted", table: "Bills");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "Bills");
            migrationBuilder.DropColumn(name: "DeletedBy", table: "Bills");
            migrationBuilder.DropColumn(name: "DeleteReason", table: "Bills");

            // Remove soft delete columns from Payments table
            migrationBuilder.DropColumn(name: "IsDeleted", table: "Payments");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "Payments");
            migrationBuilder.DropColumn(name: "DeletedBy", table: "Payments");
            migrationBuilder.DropColumn(name: "DeleteReason", table: "Payments");

            // Remove soft delete columns from BankTransactions table
            migrationBuilder.DropColumn(name: "IsDeleted", table: "BankTransactions");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "BankTransactions");
            migrationBuilder.DropColumn(name: "DeletedBy", table: "BankTransactions");
            migrationBuilder.DropColumn(name: "DeleteReason", table: "BankTransactions");

            // Remove soft delete columns from IncomeSources table
            migrationBuilder.DropColumn(name: "IsDeleted", table: "IncomeSources");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "IncomeSources");
            migrationBuilder.DropColumn(name: "DeletedBy", table: "IncomeSources");
            migrationBuilder.DropColumn(name: "DeleteReason", table: "IncomeSources");

            // Remove soft delete columns from VariableExpenses table
            migrationBuilder.DropColumn(name: "IsDeleted", table: "VariableExpenses");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "VariableExpenses");
            migrationBuilder.DropColumn(name: "DeletedBy", table: "VariableExpenses");
            migrationBuilder.DropColumn(name: "DeleteReason", table: "VariableExpenses");

            // Remove soft delete columns from SavingsTransactions table
            migrationBuilder.DropColumn(name: "IsDeleted", table: "SavingsTransactions");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "SavingsTransactions");
            migrationBuilder.DropColumn(name: "DeletedBy", table: "SavingsTransactions");
            migrationBuilder.DropColumn(name: "DeleteReason", table: "SavingsTransactions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 25, 11, 44, 42, 624, DateTimeKind.Utc).AddTicks(3447), new DateTime(2025, 10, 25, 11, 44, 42, 624, DateTimeKind.Utc).AddTicks(3447) });
        }
    }
}
