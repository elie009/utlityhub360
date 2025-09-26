using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddIncomeSourcesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessIncomeAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DividendIncomeAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DividendIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InterestIncomeAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InterestIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InvestmentIncomeAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "InvestmentIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OtherIncomeAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OtherIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PassiveIncomeAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PassiveIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "RentalIncomeAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "RentalIncomeFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SalaryAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SalaryCurrency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SalaryFrequency",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SideHustleAmount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SideHustleFrequency",
                table: "UserProfiles");

            migrationBuilder.CreateTable(
                name: "IncomeSources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserProfileId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeSources_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncomeSources_Users_UserId",
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
                values: new object[] { new DateTime(2025, 9, 26, 14, 51, 58, 587, DateTimeKind.Utc).AddTicks(370), new DateTime(2025, 9, 26, 14, 51, 58, 587, DateTimeKind.Utc).AddTicks(370) });

            migrationBuilder.CreateIndex(
                name: "IX_IncomeSources_Category",
                table: "IncomeSources",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeSources_Frequency",
                table: "IncomeSources",
                column: "Frequency");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeSources_IsActive",
                table: "IncomeSources",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeSources_UserId",
                table: "IncomeSources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeSources_UserId_Name",
                table: "IncomeSources",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomeSources_UserProfileId",
                table: "IncomeSources",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncomeSources");

            migrationBuilder.AddColumn<decimal>(
                name: "BusinessIncomeAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DividendIncomeAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DividendIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestIncomeAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterestIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InvestmentIncomeAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestmentIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherIncomeAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PassiveIncomeAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassiveIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalIncomeAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RentalIncomeFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryCurrency",
                table: "UserProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryFrequency",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SideHustleAmount",
                table: "UserProfiles",
                type: "decimal(18,2)",
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
    }
}
