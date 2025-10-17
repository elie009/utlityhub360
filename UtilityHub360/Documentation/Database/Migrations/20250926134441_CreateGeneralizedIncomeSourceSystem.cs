using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class CreateGeneralizedIncomeSourceSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MonthlySideHustleIncome",
                table: "UserProfiles",
                newName: "SideHustleAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlySalary",
                table: "UserProfiles",
                newName: "SalaryAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyRentalIncome",
                table: "UserProfiles",
                newName: "RentalIncomeAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyPassiveIncome",
                table: "UserProfiles",
                newName: "PassiveIncomeAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyOtherIncome",
                table: "UserProfiles",
                newName: "OtherIncomeAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyInvestmentIncome",
                table: "UserProfiles",
                newName: "InvestmentIncomeAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyInterestIncome",
                table: "UserProfiles",
                newName: "InterestIncomeAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyDividendIncome",
                table: "UserProfiles",
                newName: "DividendIncomeAmount");

            migrationBuilder.RenameColumn(
                name: "MonthlyBusinessIncome",
                table: "UserProfiles",
                newName: "BusinessIncomeAmount");

            migrationBuilder.AddColumn<string>(
                name: "BusinessIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DividendIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterestIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestmentIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RentalIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SideHustleFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 13, 44, 40, 608, DateTimeKind.Utc).AddTicks(8951), new DateTime(2025, 9, 26, 13, 44, 40, 608, DateTimeKind.Utc).AddTicks(8951) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DividendIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InterestIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InvestmentIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OtherIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "RentalIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SideHustleFrequency",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "SideHustleAmount",
                table: "UserProfiles",
                newName: "MonthlySideHustleIncome");

            migrationBuilder.RenameColumn(
                name: "SalaryAmount",
                table: "UserProfiles",
                newName: "MonthlySalary");

            migrationBuilder.RenameColumn(
                name: "RentalIncomeAmount",
                table: "UserProfiles",
                newName: "MonthlyRentalIncome");

            migrationBuilder.RenameColumn(
                name: "PassiveIncomeAmount",
                table: "UserProfiles",
                newName: "MonthlyPassiveIncome");

            migrationBuilder.RenameColumn(
                name: "OtherIncomeAmount",
                table: "UserProfiles",
                newName: "MonthlyOtherIncome");

            migrationBuilder.RenameColumn(
                name: "InvestmentIncomeAmount",
                table: "UserProfiles",
                newName: "MonthlyInvestmentIncome");

            migrationBuilder.RenameColumn(
                name: "InterestIncomeAmount",
                table: "UserProfiles",
                newName: "MonthlyInterestIncome");

            migrationBuilder.RenameColumn(
                name: "DividendIncomeAmount",
                table: "UserProfiles",
                newName: "MonthlyDividendIncome");

            migrationBuilder.RenameColumn(
                name: "BusinessIncomeAmount",
                table: "UserProfiles",
                newName: "MonthlyBusinessIncome");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 13, 29, 32, 135, DateTimeKind.Utc).AddTicks(9030), new DateTime(2025, 9, 26, 13, 29, 32, 135, DateTimeKind.Utc).AddTicks(9030) });
        }
    }
}
