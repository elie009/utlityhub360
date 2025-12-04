@echo off
echo ==========================================
echo Subscription Tables Migration
echo ==========================================
echo.

cd /d "%~dp0"
set SCRIPT_PATH=Documentation\Database\Scripts\create_subscription_tables.sql

if not exist "%SCRIPT_PATH%" (
    echo ERROR: SQL script not found at: %SCRIPT_PATH%
    pause
    exit /b 1
)

echo Running migration script...
echo Server: 174.138.185.18
echo Database: DBUTILS
echo.

sqlcmd -S 174.138.185.18 -d DBUTILS -U sa01 -P "iSTc0#T3tw~noz2r" -i "%SCRIPT_PATH%" -b

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ==========================================
    echo Migration completed successfully!
    echo ==========================================
    echo.
    echo Tables created:
    echo   - SubscriptionPlans
    echo   - UserSubscriptions
    echo.
    echo Default plans seeded:
    echo   - Starter (Free)
    echo   - Professional (Premium)
    echo   - Enterprise (Premium Plus)
) else (
    echo.
    echo Migration failed. Error code: %ERRORLEVEL%
    echo.
    echo Please run the SQL script manually in SSMS:
    echo   %SCRIPT_PATH%
)

echo.
pause

