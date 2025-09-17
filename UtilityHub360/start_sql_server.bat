@echo off
echo ========================================
echo Starting SQL Server Services
echo ========================================
echo.

echo Starting SQL Server (MSSQLSERVER)...
net start MSSQLSERVER
if %errorlevel% equ 0 (
    echo SQL Server started successfully!
) else (
    echo Failed to start SQL Server. Trying alternative...
    echo.
    echo Starting SQL Server (SQLEXPRESS)...
    net start MSSQL$SQLEXPRESS
    if %errorlevel% equ 0 (
        echo SQL Server Express started successfully!
    ) else (
        echo Failed to start SQL Server Express.
        echo.
        echo Please check:
        echo 1. SQL Server is installed
        echo 2. You have administrator privileges
        echo 3. SQL Server service is not disabled
        echo.
        echo You can also start it manually from:
        echo - Services.msc (search for "SQL Server")
        echo - SQL Server Configuration Manager
    )
)

echo.
echo Testing connection...
sqlcmd -S localhost -E -Q "SELECT 'Connection successful!' as Status" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: SQL Server is running and accessible!
) else (
    echo Connection test failed. Trying alternative instances...
    sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT 'Connection successful!' as Status" 2>nul
    if %errorlevel% equ 0 (
        echo SUCCESS: SQL Server Express is running and accessible!
    ) else (
        echo Connection test failed. Please check SQL Server configuration.
    )
)

echo.
echo Press any key to continue...
pause >nul

