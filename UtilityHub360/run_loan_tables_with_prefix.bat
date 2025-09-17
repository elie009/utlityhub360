@echo off
echo ========================================
echo Creating Loan Management System Tables
echo with Ln prefix (LnBorrowers, LnLoans, etc.)
echo ========================================
echo.

REM Check if sqlcmd is available
sqlcmd -? >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: sqlcmd is not available. Please install SQL Server Command Line Utilities.
    echo Download from: https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility
    pause
    exit /b 1
)

echo Running SQL script to create tables...
echo.

REM Run the SQL script
sqlcmd -S localhost -E -i "create_loan_tables_with_prefix.sql"

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo SUCCESS: Tables created successfully!
    echo ========================================
    echo.
    echo Tables created:
    echo - LnBorrowers
    echo - LnLoans  
    echo - LnRepaymentSchedules
    echo - LnPayments
    echo - LnPenalties
    echo - LnNotifications
    echo.
) else (
    echo.
    echo ========================================
    echo ERROR: Failed to create tables
    echo ========================================
    echo.
    echo Please check:
    echo 1. SQL Server is running
    echo 2. You have permission to create tables
    echo 3. Database connection is correct
    echo.
)

echo.
echo Press any key to continue...
pause >nul

