@echo off
echo ========================================
echo SQL Server Diagnostic Tool
echo ========================================
echo.

echo Step 1: Checking SQL Server Services
echo ====================================
sc query state= all | findstr /i "sql" | findstr "SERVICE_NAME"
echo.

echo Step 2: Testing Common Connection Strings
echo ========================================

REM Test localhost
echo Testing localhost...
sqlcmd -S localhost -E -Q "SELECT 'localhost works' as Result" 2>nul
if %errorlevel% equ 0 (
    echo ✓ localhost - SUCCESS
    set "working_instance=localhost"
) else (
    echo ✗ localhost - FAILED
)

REM Test localhost\SQLEXPRESS
echo Testing localhost\SQLEXPRESS...
sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT 'SQLEXPRESS works' as Result" 2>nul
if %errorlevel% equ 0 (
    echo ✓ localhost\SQLEXPRESS - SUCCESS
    set "working_instance=localhost\SQLEXPRESS"
) else (
    echo ✗ localhost\SQLEXPRESS - FAILED
)

REM Test .\SQLEXPRESS
echo Testing .\SQLEXPRESS...
sqlcmd -S .\SQLEXPRESS -E -Q "SELECT '.\SQLEXPRESS works' as Result" 2>nul
if %errorlevel% equ 0 (
    echo ✓ .\SQLEXPRESS - SUCCESS
    set "working_instance=.\SQLEXPRESS"
) else (
    echo ✗ .\SQLEXPRESS - FAILED
)

REM Test just .
echo Testing . (default instance)...
sqlcmd -S . -E -Q "SELECT 'default instance works' as Result" 2>nul
if %errorlevel% equ 0 (
    echo ✓ . (default instance) - SUCCESS
    set "working_instance=."
) else (
    echo ✗ . (default instance) - FAILED
)

echo.
echo Step 3: Results
echo ===============

if defined working_instance (
    echo SUCCESS: Found working SQL Server instance: %working_instance%
    echo.
    echo You can use this instance in your migration scripts.
    echo Replace 'localhost' with '%working_instance%' in the sqlcmd commands.
    echo.
    echo Example:
    echo   sqlcmd -S %working_instance% -E -i comprehensive_loans_migration.sql
) else (
    echo ERROR: No working SQL Server instance found.
    echo.
    echo Please check:
    echo 1. SQL Server is installed
    echo 2. SQL Server service is running
    echo 3. You have proper permissions
    echo 4. Windows Authentication is enabled
    echo.
    echo You can start SQL Server from:
    echo - Services.msc (search for "SQL Server")
    echo - SQL Server Configuration Manager
    echo - Command: net start MSSQLSERVER
)

echo.
pause
