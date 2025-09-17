@echo off
echo ========================================
echo SQL Server Instance Detection
echo ========================================
echo.

echo Checking for SQL Server instances...
echo.

REM Check SQL Server services
echo SQL Server Services:
echo --------------------
sc query state= all | findstr /i "sql" | findstr "SERVICE_NAME"
echo.

REM Check SQL Server instances using sqlcmd
echo Available SQL Server instances:
echo --------------------------------
sqlcmd -L 2>nul
echo.

REM Check specific common instances
echo Testing common connection strings:
echo ----------------------------------

set "instances=localhost localhost\SQLEXPRESS .\SQLEXPRESS . localhost\SQL2019 localhost\SQL2022 localhost\MSSQLSERVER"

for %%i in (%instances%) do (
    echo Testing: %%i
    sqlcmd -S "%%i" -E -Q "SELECT @@SERVERNAME as ServerName, @@VERSION as Version" 2>nul
    if !errorlevel! equ 0 (
        echo   ✓ SUCCESS: %%i is accessible
    ) else (
        echo   ✗ FAILED: %%i is not accessible
    )
    echo.
)

echo.
echo If you see any successful connections above, use that instance name
echo in your migration scripts by replacing 'localhost' with the working instance.
echo.
pause
