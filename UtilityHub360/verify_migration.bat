@echo off
echo Verifying Loans to LnLoans Migration Status
echo ===========================================
echo.

REM Check if SQL Server is running
echo Checking SQL Server connection...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -Q "SELECT @@VERSION" >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Cannot connect to remote SQL Server at 174.138.185.18
    echo Please check network connectivity and credentials.
    pause
    exit /b 1
)

echo.
echo Running verification script...
echo.

REM Run the verification
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i verify_loans_migration.sql

echo.
echo Verification completed!
pause
