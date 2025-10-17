using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileIncomeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalaryCurrency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MonthlyPassiveIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyInvestmentIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyRentalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyDividendIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyInterestIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyBusinessIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlySideHustleIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyOtherIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalaryFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassiveIncomeFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MonthlyTaxDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlySavingsGoal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyInvestmentGoal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyEmergencyFundGoal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 13, 29, 32, 135, DateTimeKind.Utc).AddTicks(9030), new DateTime(2025, 9, 26, 13, 29, 32, 135, DateTimeKind.Utc).AddTicks(9030) });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_EmploymentType",
                table: "UserProfiles",
                column: "EmploymentType");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Industry",
                table: "UserProfiles",
                column: "Industry");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 12, 28, 21, 767, DateTimeKind.Utc).AddTicks(7956), new DateTime(2025, 9, 26, 12, 28, 21, 767, DateTimeKind.Utc).AddTicks(7956) });
        }
    }
}
