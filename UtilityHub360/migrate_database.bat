@echo off
echo =============================================
echo LOAN MANAGEMENT SYSTEM DATABASE MIGRATION
echo =============================================
echo.

echo Option 1: Run SQL Script directly
echo Option 2: Use Entity Framework Migrations
echo.

set /p choice="Choose option (1 or 2): "

if "%choice%"=="1" goto sql_script
if "%choice%"=="2" goto ef_migrations
goto invalid_choice

:sql_script
echo.
echo Running SQL Script to create tables...
echo.
sqlcmd -S 174.138.185.18 -d DBUTILS -U sa01 -P "iSTc0#T3tw~noz2r" -i create_loan_tables.sql
echo.
echo SQL Script execution completed!
goto end

:ef_migrations
echo.
echo Entity Framework Migrations approach:
echo.
echo 1. Open Visual Studio
echo 2. Go to Tools -> NuGet Package Manager -> Package Manager Console
echo 3. Run: Enable-Migrations -ContextTypeName UtilityHub360.Models.UtilityHubDbContext
echo 4. Run: Add-Migration InitialCreate
echo 5. Run: Update-Database
echo.
echo Please follow these steps in Visual Studio Package Manager Console
goto end

:invalid_choice
echo Invalid choice. Please run the script again and choose 1 or 2.
goto end

:end
echo.
echo Migration process completed!
pause
