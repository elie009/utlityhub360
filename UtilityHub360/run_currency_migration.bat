@echo off
REM Batch script to add PreferredCurrency column using sqlcmd
REM Make sure sqlcmd is installed and in your PATH

echo Adding PreferredCurrency column to UserProfiles table...
echo.

REM Read connection details (you may need to adjust these based on your connection string)
set SERVER=174.138.185.18
set DATABASE=DBUTILS
set USER=sa01
set PASSWORD=iSTc0#T3tw~noz2r

echo Executing SQL script...
echo.

sqlcmd -S %SERVER% -d %DATABASE% -U %USER% -P %PASSWORD% -i add_preferred_currency_column.sql -W

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS: PreferredCurrency column has been added!
) else (
    echo.
    echo ERROR: Failed to execute SQL script
    echo.
    echo Please run the SQL script manually:
    echo 1. Open SQL Server Management Studio or Azure Data Studio
    echo 2. Connect to your database
    echo 3. Open the file: add_preferred_currency_column.sql
    echo 4. Execute the script
)

pause


