@echo off
echo Running Loans to LnLoans migration...
echo.

REM Check if SQL Server is running
echo Checking SQL Server connection...
sqlcmd -S localhost -E -Q "SELECT @@VERSION" >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Cannot connect to SQL Server. Please ensure SQL Server is running.
    echo.
    echo Starting SQL Server...
    start_sql_server.bat
    timeout /t 5 /nobreak >nul
)

REM Run the migration
echo Running migration script...
sqlcmd -S localhost -E -i migrate_loans_to_lnloans.sql

if %errorlevel% equ 0 (
    echo.
    echo Migration completed successfully!
    echo The Loans table has been renamed to LnLoans.
) else (
    echo.
    echo ERROR: Migration failed. Please check the error messages above.
)

echo.
pause
