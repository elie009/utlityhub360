@echo off
echo Running Table Names Migration
echo =============================
echo This will rename all loan management tables to use Ln prefix
echo.

REM Check if SQL Server is running
echo Checking SQL Server connection...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -Q "SELECT @@VERSION" >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Cannot connect to remote SQL Server at 174.138.185.18
    echo.
    echo Please check:
    echo 1. Network connectivity to 174.138.185.18
    echo 2. SQL Server is running on the remote server
    echo 3. Firewall allows connections on port 1433
    echo 4. Credentials are correct (sa01)
    echo.
    pause
    exit /b 1
)

echo.
echo Running table names migration script...
echo This will rename:
echo   - Borrowers → LnBorrowers
echo   - Loans → LnLoans
echo   - Payments → LnPayments
echo   - RepaymentSchedules → LnRepaymentSchedules
echo   - Penalties → LnPenalties
echo   - Notifications → LnNotifications
echo.

REM Run the migration
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i update_table_names_migration.sql

if %errorlevel% equ 0 (
    echo.
    echo ================================================
    echo Migration completed successfully!
    echo ================================================
    echo.
    echo All loan management tables have been renamed to use the Ln prefix.
    echo Your Entity Framework models are now in sync with the database.
) else (
    echo.
    echo ================================================
    echo ERROR: Migration failed!
    echo ================================================
    echo.
    echo Please check the error messages above and resolve any issues.
)

echo.
echo Press any key to continue...
pause >nul
