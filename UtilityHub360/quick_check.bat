@echo off
echo Quick Table Check
echo =================
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
echo Running quick table check...
echo.

REM Run the check
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i quick_table_check.sql

echo.
echo Quick check completed!
pause
