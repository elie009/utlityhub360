@echo off
echo ========================================
echo SQL Server Connection Troubleshooter
echo ========================================
echo.

echo Checking for SQL Server services...
echo.

REM Check common SQL Server service names
set "services=MSSQLSERVER MSSQL$SQLEXPRESS MSSQL$MSSQLSERVER MSSQL$SQL2019 MSSQL$SQL2022"

for %%s in (%services%) do (
    echo Checking service: %%s
    sc query "%%s" >nul 2>&1
    if !errorlevel! equ 0 (
        echo   Found service: %%s
        echo   Starting service...
        net start "%%s" >nul 2>&1
        if !errorlevel! equ 0 (
            echo   SUCCESS: %%s started!
            set "found_service=%%s"
            goto :test_connection
        ) else (
            echo   Service %%s exists but failed to start
        )
    ) else (
        echo   Service %%s not found
    )
)

echo.
echo No SQL Server services found or started successfully.
echo.
echo Please check:
echo 1. SQL Server is installed on this machine
echo 2. You are running as Administrator
echo 3. SQL Server services are not disabled
echo.
echo You can also start SQL Server manually from:
echo - Services.msc (search for "SQL Server")
echo - SQL Server Configuration Manager
echo - SQL Server Management Studio
echo.
pause
exit /b 1

:test_connection
echo.
echo Testing database connections...
echo.

REM Test different connection strings
set "connections=localhost localhost\SQLEXPRESS .\SQLEXPRESS . localhost\SQL2019 localhost\SQL2022"

for %%c in (%connections%) do (
    echo Testing connection: %%c
    sqlcmd -S "%%c" -E -Q "SELECT 'Connection successful!' as Status" >nul 2>&1
    if !errorlevel! equ 0 (
        echo   SUCCESS: Connected to %%c
        set "working_connection=%%c"
        goto :update_scripts
    ) else (
        echo   Failed to connect to %%c
    )
)

echo.
echo No working connection found. Please check SQL Server configuration.
pause
exit /b 1

:update_scripts
echo.
echo Updating migration scripts with working connection: !working_connection!
echo.

REM Update the comprehensive migration script
powershell -Command "(Get-Content 'comprehensive_loans_migration.sql') -replace 'sqlcmd -S localhost -E', 'sqlcmd -S !working_connection! -E' | Set-Content 'comprehensive_loans_migration.sql'"

REM Update the verification script
powershell -Command "(Get-Content 'verify_loans_migration.sql') -replace 'sqlcmd -S localhost -E', 'sqlcmd -S !working_connection! -E' | Set-Content 'verify_loans_migration.sql'"

echo Scripts updated with connection: !working_connection!
echo.
echo You can now run the migration with:
echo   run_comprehensive_migration.bat
echo.
pause