@echo off
echo ========================================
echo Alternative Table Creation Methods
echo ========================================
echo.

echo This script will try different methods to create the loan tables.
echo.

REM Method 1: Try localhost
echo Method 1: Trying localhost...
sqlcmd -S localhost -E -i "create_loan_tables_with_prefix.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables created using localhost
    goto :verify_tables
)

REM Method 2: Try localhost\SQLEXPRESS
echo Method 2: Trying localhost\SQLEXPRESS...
sqlcmd -S localhost\SQLEXPRESS -E -i "create_loan_tables_with_prefix.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables created using localhost\SQLEXPRESS
    goto :verify_tables
)

REM Method 3: Try .\SQLEXPRESS
echo Method 3: Trying .\SQLEXPRESS...
sqlcmd -S .\SQLEXPRESS -E -i "create_loan_tables_with_prefix.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables created using .\SQLEXPRESS
    goto :verify_tables
)

REM Method 4: Try (local)
echo Method 4: Trying (local)...
sqlcmd -S (local) -E -i "create_loan_tables_with_prefix.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables created using (local)
    goto :verify_tables
)

echo.
echo ========================================
echo ALL METHODS FAILED
echo ========================================
echo.
echo None of the connection methods worked.
echo.
echo Please:
echo 1. Run fix_sql_connection.bat to diagnose the issue
echo 2. Start SQL Server manually from Services.msc
echo 3. Check if SQL Server is installed
echo 4. Verify your connection string in Web.config
echo.
goto :end

:verify_tables
echo.
echo ========================================
echo Verifying Tables
echo ========================================
echo.

REM Try to verify tables with different connection strings
echo Checking table existence...

REM Try localhost first
sqlcmd -S localhost -E -i "verify_ln_tables.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables verified using localhost
    goto :success
)

REM Try localhost\SQLEXPRESS
sqlcmd -S localhost\SQLEXPRESS -E -i "verify_ln_tables.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables verified using localhost\SQLEXPRESS
    goto :success
)

REM Try .\SQLEXPRESS
sqlcmd -S .\SQLEXPRESS -E -i "verify_ln_tables.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables verified using .\SQLEXPRESS
    goto :success
)

REM Try (local)
sqlcmd -S (local) -E -i "verify_ln_tables.sql" 2>nul
if %errorlevel% equ 0 (
    echo SUCCESS: Tables verified using (local)
    goto :success
)

echo Tables created but verification failed.

:success
echo.
echo ========================================
echo SUCCESS!
echo ========================================
echo.
echo Loan Management System tables created successfully!
echo Tables: LnBorrowers, LnLoans, LnRepaymentSchedules, LnPayments, LnPenalties, LnNotifications
echo.

:end
echo.
echo Press any key to continue...
pause >nul

