using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityHub360.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIdToBankTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "VariableExpenses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VariableExpenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "UserProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "SavingsTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SavingsTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "SavingsAccounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterestCompoundingFrequency",
                table: "SavingsAccounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "SavingsAccounts",
                type: "decimal(5,4)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInterestCalculationDate",
                table: "SavingsAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextInterestCalculationDate",
                table: "SavingsAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "SavingsAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "EffectiveInterestRate",
                table: "Loans",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoanType",
                table: "Loans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefinancedFromLoanId",
                table: "Loans",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefinancedToLoanId",
                table: "Loans",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefinancingDate",
                table: "Loans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyIncome",
                table: "LoanApplications",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "EmploymentStatus",
                table: "LoanApplications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "IncomeSources",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "IncomeSources",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "Bills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Bills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BillId",
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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BankTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LoanId",
                table: "BankTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
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

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "BankAccounts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BankAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "BankAccounts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BankAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AllocationTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationTemplates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ComplianceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BankStatements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StatementName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StatementStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatementEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImportFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImportSource = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalTransactions = table.Column<int>(type: "int", nullable: false),
                    MatchedTransactions = table.Column<int>(type: "int", nullable: false),
                    UnmatchedTransactions = table.Column<int>(type: "int", nullable: false),
                    IsReconciled = table.Column<bool>(type: "bit", nullable: false),
                    ReconciledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReconciledBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatements_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankStatements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CardName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CardBrand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Last4Digits = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CardholderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryMonth = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ExpiryYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Cvv = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClosedMonths",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    ClosedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosedMonths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosedMonths_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosedMonths_Users_ClosedBy",
                        column: x => x.ClosedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MonthlyBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    YearlyBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ParentCategoryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsTaxDeductible = table.Column<bool>(type: "bit", nullable: false),
                    TaxCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_ExpenseCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Investments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvestmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrokerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfInvestment = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitialInvestment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealizedGainLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RealizedGainLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalReturnPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Investments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NotificationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipient = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationHistories_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NotificationHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InAppEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ScheduleTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduleDays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuietHoursEnabled = table.Column<bool>(type: "bit", nullable: false),
                    QuietHoursStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuietHoursEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxNotificationsPerDay = table.Column<int>(type: "int", nullable: true),
                    MinMinutesBetweenNotifications = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Receivables",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BorrowerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BorrowerContact = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Principal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MonthlyPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receivables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receivables_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YearlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxBankAccounts = table.Column<int>(type: "int", nullable: true),
                    MaxTransactionsPerMonth = table.Column<int>(type: "int", nullable: true),
                    MaxBillsPerMonth = table.Column<int>(type: "int", nullable: true),
                    MaxLoans = table.Column<int>(type: "int", nullable: true),
                    MaxSavingsGoals = table.Column<int>(type: "int", nullable: true),
                    MaxReceiptOcrPerMonth = table.Column<int>(type: "int", nullable: true),
                    MaxAiQueriesPerMonth = table.Column<int>(type: "int", nullable: true),
                    MaxApiCallsPerMonth = table.Column<int>(type: "int", nullable: true),
                    MaxUsers = table.Column<int>(type: "int", nullable: true),
                    TransactionHistoryMonths = table.Column<int>(type: "int", nullable: true),
                    HasAiAssistant = table.Column<bool>(type: "bit", nullable: false),
                    HasBankFeedIntegration = table.Column<bool>(type: "bit", nullable: false),
                    HasReceiptOcr = table.Column<bool>(type: "bit", nullable: false),
                    HasAdvancedReports = table.Column<bool>(type: "bit", nullable: false),
                    HasPrioritySupport = table.Column<bool>(type: "bit", nullable: false),
                    HasApiAccess = table.Column<bool>(type: "bit", nullable: false),
                    HasInvestmentTracking = table.Column<bool>(type: "bit", nullable: false),
                    HasTaxOptimization = table.Column<bool>(type: "bit", nullable: false),
                    HasMultiUserSupport = table.Column<bool>(type: "bit", nullable: false),
                    HasWhiteLabelOptions = table.Column<bool>(type: "bit", nullable: false),
                    HasCustomIntegrations = table.Column<bool>(type: "bit", nullable: false),
                    HasDedicatedSupport = table.Column<bool>(type: "bit", nullable: false),
                    HasAccountManager = table.Column<bool>(type: "bit", nullable: false),
                    HasCustomReporting = table.Column<bool>(type: "bit", nullable: false),
                    HasAdvancedSecurity = table.Column<bool>(type: "bit", nullable: false),
                    HasComplianceReports = table.Column<bool>(type: "bit", nullable: false),
                    HasFinancialHealthScore = table.Column<bool>(type: "bit", nullable: false),
                    HasBillForecasting = table.Column<bool>(type: "bit", nullable: false),
                    HasDebtOptimizer = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    StripeMonthlyPriceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StripeYearlyPriceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AssignedTo",
                        column: x => x.AssignedTo,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Users_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsSystemCategory = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ConditionField = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConditionOperator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConditionValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConditionCaseSensitive = table.Column<bool>(type: "bit", nullable: false),
                    AutoCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AutoTags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AutoApprove = table.Column<bool>(type: "bit", nullable: false),
                    AutoDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionRules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UtilityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UtilityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Consumption = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CostPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousReading = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CurrentReading = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReadingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PropertyAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MeterNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AutoGenerateNext = table.Column<bool>(type: "bit", nullable: false),
                    IsAutoGenerated = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentUtilityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhiteLabelSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CustomDomain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteLabelSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteLabelSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllocationPlans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TemplateId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationPlans_AllocationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "AllocationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AllocationPlans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllocationTemplateCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TemplateId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationTemplateCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationTemplateCategories_AllocationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "AllocationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankStatementItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BankStatementId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Merchant = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BalanceAfterTransaction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsMatched = table.Column<bool>(type: "bit", nullable: false),
                    MatchedTransactionId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    MatchedTransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MatchedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MatchedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatementItems_BankStatements_BankStatementId",
                        column: x => x.BankStatementId,
                        principalTable: "BankStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankStatementUploads",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessedBankStatementId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatementUploads_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankStatementUploads_BankStatements_ProcessedBankStatementId",
                        column: x => x.ProcessedBankStatementId,
                        principalTable: "BankStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BankStatementUploads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reconciliations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BankStatementId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReconciliationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReconciliationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatementBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTransactions = table.Column<int>(type: "int", nullable: false),
                    MatchedTransactions = table.Column<int>(type: "int", nullable: false),
                    UnmatchedTransactions = table.Column<int>(type: "int", nullable: false),
                    PendingTransactions = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reconciliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reconciliations_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reconciliations_BankStatements_BankStatementId",
                        column: x => x.BankStatementId,
                        principalTable: "BankStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reconciliations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseBudgets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AlertThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseBudgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseBudgets_ExpenseCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseBudgets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentPositions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvestmentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AverageCostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnrealizedGainLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GainLossPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DividendsReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InterestReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastPriceUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentPositions_Investments_InvestmentId",
                        column: x => x.InvestmentId,
                        principalTable: "Investments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BillId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SavingsAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReceivableId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EntryType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntries_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntries_Receivables_ReceivableId",
                        column: x => x.ReceivableId,
                        principalTable: "Receivables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntries_SavingsAccounts_SavingsAccountId",
                        column: x => x.SavingsAccountId,
                        principalTable: "SavingsAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceivablePayments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceivableId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivablePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivablePayments_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivablePayments_Receivables_ReceivableId",
                        column: x => x.ReceivableId,
                        principalTable: "Receivables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceivablePayments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubscriptionPlanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingCycle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaymentMethodId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TransactionsThisMonth = table.Column<int>(type: "int", nullable: false),
                    BillsThisMonth = table.Column<int>(type: "int", nullable: false),
                    ReceiptOcrThisMonth = table.Column<int>(type: "int", nullable: false),
                    AiQueriesThisMonth = table.Column<int>(type: "int", nullable: false),
                    ApiCallsThisMonth = table.Column<int>(type: "int", nullable: false),
                    LastUsageResetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamInvitations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InvitedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcceptedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamInvitations_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamInvitations_Users_AcceptedByUserId",
                        column: x => x.AcceptedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamInvitations_Users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketAttachments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TicketId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAttachments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketAttachments_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TicketComments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TicketId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketComments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketStatusHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TicketId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistories_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistories_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AllocationCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationCategories_AllocationPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "AllocationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Merchant = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BalanceAfterTransaction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingTransactions_BankStatementUploads_UploadId",
                        column: x => x.UploadId,
                        principalTable: "BankStatementUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationMatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReconciliationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SystemTransactionId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SystemTransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatementItemId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    MatchType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MatchStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatchNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AmountDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatchedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationMatches_BankStatementItems_StatementItemId",
                        column: x => x.StatementItemId,
                        principalTable: "BankStatementItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReconciliationMatches_Reconciliations_ReconciliationId",
                        column: x => x.ReconciliationId,
                        principalTable: "Reconciliations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvestmentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PositionId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PricePerShare = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentTransactions_InvestmentPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "InvestmentPositions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvestmentTransactions_Investments_InvestmentId",
                        column: x => x.InvestmentId,
                        principalTable: "Investments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JournalEntryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntrySide = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllocationHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PlanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Variance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VariancePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationHistories_AllocationCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AllocationCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AllocationHistories_AllocationPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "AllocationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllocationHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllocationRecommendations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PlanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecommendationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SuggestedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuggestedPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsApplied = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationRecommendations_AllocationCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AllocationCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AllocationRecommendations_AllocationPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "AllocationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllocationRecommendations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseApprovals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpenseId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalLevel = table.Column<int>(type: "int", nullable: false),
                    NextApproverId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseApprovals_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExpenseApprovals_Users_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseReceipts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExpenseId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtractedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExtractedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtractedMerchant = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExtractedItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OcrText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsOcrProcessed = table.Column<bool>(type: "bit", nullable: false),
                    OcrProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseReceipts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Merchant = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsTaxDeductible = table.Column<bool>(type: "bit", nullable: false),
                    IsReimbursable = table.Column<bool>(type: "bit", nullable: false),
                    ReimbursementRequestId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Mileage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MileageRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PerDiemAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NumberOfDays = table.Column<int>(type: "int", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasReceipt = table.Column<bool>(type: "bit", nullable: false),
                    ReceiptId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BudgetId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurringFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentExpenseId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseBudgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "ExpenseBudgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseReceipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "ExpenseReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "Country", "CreatedAt", "UpdatedAt" },
                values: new object[] { null, new DateTime(2025, 12, 26, 21, 10, 10, 797, DateTimeKind.Utc).AddTicks(5538), new DateTime(2025, 12, 26, 21, 10, 10, 797, DateTimeKind.Utc).AddTicks(5538) });

            migrationBuilder.CreateIndex(
                name: "IX_VariableExpenses_IsDeleted",
                table: "VariableExpenses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsTransactions_IsDeleted",
                table: "SavingsTransactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IsDeleted",
                table: "Payments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_RefinancedFromLoanId",
                table: "Loans",
                column: "RefinancedFromLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_RefinancedToLoanId",
                table: "Loans",
                column: "RefinancedToLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeSources_IsDeleted",
                table: "IncomeSources",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_IsDeleted",
                table: "Bills",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_BillId",
                table: "BankTransactions",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_IsDeleted",
                table: "BankTransactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_LoanId",
                table: "BankTransactions",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_PaymentId",
                table: "BankTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_SavingsAccountId",
                table: "BankTransactions",
                column: "SavingsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_TransactionPurpose",
                table: "BankTransactions",
                column: "TransactionPurpose");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_IsDeleted",
                table: "BankAccounts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationCategories_PlanId",
                table: "AllocationCategories",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationHistories_CategoryId",
                table: "AllocationHistories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationHistories_PeriodDate",
                table: "AllocationHistories",
                column: "PeriodDate");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationHistories_PlanId",
                table: "AllocationHistories",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationHistories_UserId",
                table: "AllocationHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationHistories_UserId_PlanId_PeriodDate",
                table: "AllocationHistories",
                columns: new[] { "UserId", "PlanId", "PeriodDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AllocationPlans_IsActive",
                table: "AllocationPlans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationPlans_TemplateId",
                table: "AllocationPlans",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationPlans_UserId",
                table: "AllocationPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationPlans_UserId_IsActive",
                table: "AllocationPlans",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRecommendations_CategoryId",
                table: "AllocationRecommendations",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRecommendations_IsApplied",
                table: "AllocationRecommendations",
                column: "IsApplied");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRecommendations_IsRead",
                table: "AllocationRecommendations",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRecommendations_PlanId",
                table: "AllocationRecommendations",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRecommendations_Priority",
                table: "AllocationRecommendations",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRecommendations_UserId",
                table: "AllocationRecommendations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationTemplateCategories_TemplateId",
                table: "AllocationTemplateCategories",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationTemplates_IsActive",
                table: "AllocationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationTemplates_IsSystemTemplate",
                table: "AllocationTemplates",
                column: "IsSystemTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationTemplates_UserId",
                table: "AllocationTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ComplianceType",
                table: "AuditLogs",
                column: "ComplianceType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_LogType",
                table: "AuditLogs",
                column: "LogType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_LogType_Severity",
                table: "AuditLogs",
                columns: new[] { "LogType", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementItems_BankStatementId",
                table: "BankStatementItems",
                column: "BankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementItems_IsMatched",
                table: "BankStatementItems",
                column: "IsMatched");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementItems_MatchedTransactionId",
                table: "BankStatementItems",
                column: "MatchedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementItems_TransactionDate",
                table: "BankStatementItems",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_BankAccountId",
                table: "BankStatements",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_BankAccountId_UserId",
                table: "BankStatements",
                columns: new[] { "BankAccountId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_IsReconciled",
                table: "BankStatements",
                column: "IsReconciled");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_StatementEndDate",
                table: "BankStatements",
                column: "StatementEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_StatementStartDate",
                table: "BankStatements",
                column: "StatementStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_UserId",
                table: "BankStatements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementUploads_BankAccountId",
                table: "BankStatementUploads",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementUploads_BankAccountId_UserId",
                table: "BankStatementUploads",
                columns: new[] { "BankAccountId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementUploads_CreatedAt",
                table: "BankStatementUploads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementUploads_ProcessedBankStatementId",
                table: "BankStatementUploads",
                column: "ProcessedBankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementUploads_Status",
                table: "BankStatementUploads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementUploads_UserId",
                table: "BankStatementUploads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_BankAccountId",
                table: "Cards",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IsActive",
                table: "Cards",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IsDeleted",
                table: "Cards",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IsPrimary",
                table: "Cards",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId",
                table: "Cards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedMonths_BankAccountId",
                table: "ClosedMonths",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedMonths_BankAccountId_Year_Month",
                table: "ClosedMonths",
                columns: new[] { "BankAccountId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClosedMonths_ClosedAt",
                table: "ClosedMonths",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedMonths_ClosedBy",
                table: "ClosedMonths",
                column: "ClosedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedMonths_Year_Month",
                table: "ClosedMonths",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseApprovals_ApprovedBy",
                table: "ExpenseApprovals",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseApprovals_ExpenseId",
                table: "ExpenseApprovals",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseApprovals_RequestedAt",
                table: "ExpenseApprovals",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseApprovals_RequestedBy",
                table: "ExpenseApprovals",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseApprovals_Status",
                table: "ExpenseApprovals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBudgets_CategoryId",
                table: "ExpenseBudgets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBudgets_EndDate",
                table: "ExpenseBudgets",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBudgets_IsActive",
                table: "ExpenseBudgets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBudgets_StartDate",
                table: "ExpenseBudgets",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBudgets_UserId",
                table: "ExpenseBudgets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBudgets_UserId_CategoryId_StartDate_EndDate",
                table: "ExpenseBudgets",
                columns: new[] { "UserId", "CategoryId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_IsActive",
                table: "ExpenseCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_ParentCategoryId",
                table: "ExpenseCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_UserId",
                table: "ExpenseCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_UserId_Name",
                table: "ExpenseCategories",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReceipts_ExpenseId",
                table: "ExpenseReceipts",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReceipts_IsOcrProcessed",
                table: "ExpenseReceipts",
                column: "IsOcrProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReceipts_UserId",
                table: "ExpenseReceipts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ApprovalStatus",
                table: "Expenses",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BudgetId",
                table: "Expenses",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseDate",
                table: "Expenses",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ReceiptId",
                table: "Expenses",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId_CategoryId_ExpenseDate",
                table: "Expenses",
                columns: new[] { "UserId", "CategoryId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId_ExpenseDate",
                table: "Expenses",
                columns: new[] { "UserId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPositions_InvestmentId",
                table: "InvestmentPositions",
                column: "InvestmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Investments_UserId",
                table: "Investments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTransactions_InvestmentId",
                table: "InvestmentTransactions",
                column: "InvestmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTransactions_PositionId",
                table: "InvestmentTransactions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_BillId",
                table: "JournalEntries",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_LoanId",
                table: "JournalEntries",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReceivableId",
                table: "JournalEntries",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_SavingsAccountId",
                table: "JournalEntries",
                column: "SavingsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_UserId",
                table: "JournalEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_Channel",
                table: "NotificationHistories",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_NotificationId",
                table: "NotificationHistories",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_SentAt",
                table: "NotificationHistories",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_Status",
                table: "NotificationHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_UserId",
                table: "NotificationHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_UserId_CreatedAt",
                table: "NotificationHistories",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_NotificationType",
                table: "NotificationPreferences",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_NotificationType",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "NotificationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Channel",
                table: "NotificationTemplates",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsActive",
                table: "NotificationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Name",
                table: "NotificationTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_NotificationType",
                table: "NotificationTemplates",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivablePayments_BankAccountId",
                table: "ReceivablePayments",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivablePayments_PaymentDate",
                table: "ReceivablePayments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivablePayments_ReceivableId",
                table: "ReceivablePayments",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivablePayments_Reference",
                table: "ReceivablePayments",
                column: "Reference");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivablePayments_UserId",
                table: "ReceivablePayments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_BorrowerName",
                table: "Receivables",
                column: "BorrowerName");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_IsDeleted",
                table: "Receivables",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_LentAt",
                table: "Receivables",
                column: "LentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_Status",
                table: "Receivables",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Receivables_UserId",
                table: "Receivables",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationMatches_MatchStatus",
                table: "ReconciliationMatches",
                column: "MatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationMatches_ReconciliationId",
                table: "ReconciliationMatches",
                column: "ReconciliationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationMatches_StatementItemId",
                table: "ReconciliationMatches",
                column: "StatementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationMatches_SystemTransactionId",
                table: "ReconciliationMatches",
                column: "SystemTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_BankAccountId",
                table: "Reconciliations",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_BankStatementId",
                table: "Reconciliations",
                column: "BankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_ReconciliationDate",
                table: "Reconciliations",
                column: "ReconciliationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_Status",
                table: "Reconciliations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_UserId",
                table: "Reconciliations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingTransactions_TransactionDate",
                table: "StagingTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_StagingTransactions_UploadId",
                table: "StagingTransactions",
                column: "UploadId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_DisplayOrder",
                table: "SubscriptionPlans",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_IsActive",
                table: "SubscriptionPlans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_AcceptedByUserId",
                table: "TeamInvitations",
                column: "AcceptedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_Email",
                table: "TeamInvitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_ExpiresAt",
                table: "TeamInvitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_InvitedByUserId",
                table: "TeamInvitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_Status",
                table: "TeamInvitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_TeamId",
                table: "TeamInvitations",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_Token",
                table: "TeamInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_IsActive",
                table: "TeamMembers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TeamId",
                table: "TeamMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TeamId_UserId",
                table: "TeamMembers",
                columns: new[] { "TeamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserId",
                table: "TeamMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_IsActive",
                table: "Teams",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_OwnerId",
                table: "Teams",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_TicketId",
                table: "TicketAttachments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_UploadedBy",
                table: "TicketAttachments",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId",
                table: "TicketComments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_UserId",
                table: "TicketComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedTo",
                table: "Tickets",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ResolvedBy",
                table: "Tickets",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistories_ChangedBy",
                table: "TicketStatusHistories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistories_TicketId",
                table: "TicketStatusHistories",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_IsActive",
                table: "TransactionCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_IsSystemCategory",
                table: "TransactionCategories",
                column: "IsSystemCategory");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_Type",
                table: "TransactionCategories",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_UserId",
                table: "TransactionCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategories_UserId_Name",
                table: "TransactionCategories",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRules_UserId",
                table: "TransactionRules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_BillingCycle",
                table: "UserSubscriptions",
                column: "BillingCycle");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_NextBillingDate",
                table: "UserSubscriptions",
                column: "NextBillingDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_Status",
                table: "UserSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_SubscriptionPlanId",
                table: "UserSubscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_Status",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_BillingDate",
                table: "Utilities",
                column: "BillingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_DueDate",
                table: "Utilities",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_Provider",
                table: "Utilities",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_Status",
                table: "Utilities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_UserId",
                table: "Utilities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_UserId_UtilityType_Provider",
                table: "Utilities",
                columns: new[] { "UserId", "UtilityType", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_UtilityType",
                table: "Utilities",
                column: "UtilityType");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Category",
                table: "Vendors",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_IsActive",
                table: "Vendors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Name",
                table: "Vendors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_UserId",
                table: "Vendors",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WhiteLabelSettings_IsActive",
                table: "WhiteLabelSettings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WhiteLabelSettings_UserId",
                table: "WhiteLabelSettings",
                column: "UserId",
                unique: true);

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
                name: "FK_BankTransactions_Payments_PaymentId",
                table: "BankTransactions",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_SavingsAccounts_SavingsAccountId",
                table: "BankTransactions",
                column: "SavingsAccountId",
                principalTable: "SavingsAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Loans_RefinancedFromLoanId",
                table: "Loans",
                column: "RefinancedFromLoanId",
                principalTable: "Loans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Loans_RefinancedToLoanId",
                table: "Loans",
                column: "RefinancedToLoanId",
                principalTable: "Loans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseApprovals_Expenses_ExpenseId",
                table: "ExpenseApprovals",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseReceipts_Expenses_ExpenseId",
                table: "ExpenseReceipts",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Bills_BillId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Loans_LoanId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Payments_PaymentId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_SavingsAccounts_SavingsAccountId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Loans_RefinancedFromLoanId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Loans_RefinancedToLoanId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseReceipts_Expenses_ExpenseId",
                table: "ExpenseReceipts");

            migrationBuilder.DropTable(
                name: "AllocationHistories");

            migrationBuilder.DropTable(
                name: "AllocationRecommendations");

            migrationBuilder.DropTable(
                name: "AllocationTemplateCategories");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "ClosedMonths");

            migrationBuilder.DropTable(
                name: "ExpenseApprovals");

            migrationBuilder.DropTable(
                name: "InvestmentTransactions");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "NotificationHistories");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "ReceivablePayments");

            migrationBuilder.DropTable(
                name: "ReconciliationMatches");

            migrationBuilder.DropTable(
                name: "StagingTransactions");

            migrationBuilder.DropTable(
                name: "TeamInvitations");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "TicketAttachments");

            migrationBuilder.DropTable(
                name: "TicketComments");

            migrationBuilder.DropTable(
                name: "TicketStatusHistories");

            migrationBuilder.DropTable(
                name: "TransactionCategories");

            migrationBuilder.DropTable(
                name: "TransactionRules");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "Utilities");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "WhiteLabelSettings");

            migrationBuilder.DropTable(
                name: "AllocationCategories");

            migrationBuilder.DropTable(
                name: "InvestmentPositions");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "BankStatementItems");

            migrationBuilder.DropTable(
                name: "Reconciliations");

            migrationBuilder.DropTable(
                name: "BankStatementUploads");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "AllocationPlans");

            migrationBuilder.DropTable(
                name: "Investments");

            migrationBuilder.DropTable(
                name: "Receivables");

            migrationBuilder.DropTable(
                name: "BankStatements");

            migrationBuilder.DropTable(
                name: "AllocationTemplates");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "ExpenseBudgets");

            migrationBuilder.DropTable(
                name: "ExpenseReceipts");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_VariableExpenses_IsDeleted",
                table: "VariableExpenses");

            migrationBuilder.DropIndex(
                name: "IX_SavingsTransactions_IsDeleted",
                table: "SavingsTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_IsDeleted",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Type",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Loans_RefinancedFromLoanId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_RefinancedToLoanId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_IncomeSources_IsDeleted",
                table: "IncomeSources");

            migrationBuilder.DropIndex(
                name: "IX_Bills_IsDeleted",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_BillId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_IsDeleted",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_LoanId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_PaymentId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_SavingsAccountId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_TransactionPurpose",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_IsDeleted",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "VariableExpenses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VariableExpenses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "VariableExpenses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VariableExpenses");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "SavingsTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SavingsTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SavingsTransactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SavingsTransactions");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "InterestCompoundingFrequency",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "LastInterestCalculationDate",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "NextInterestCalculationDate",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EffectiveInterestRate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LoanType",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "RefinancedFromLoanId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "RefinancedToLoanId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "RefinancingDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "IncomeSources");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "IncomeSources");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "IncomeSources");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "IncomeSources");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "BillId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "LoanId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "SavingsAccountId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionPurpose",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BankAccounts");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyIncome",
                table: "LoanApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmploymentStatus",
                table: "LoanApplications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 28, 16, 1, 45, 213, DateTimeKind.Utc).AddTicks(3151), new DateTime(2025, 10, 28, 16, 1, 45, 213, DateTimeKind.Utc).AddTicks(3152) });
        }
    }
}
