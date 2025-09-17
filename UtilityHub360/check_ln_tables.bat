@echo off
echo ========================================
echo Verifying Loan Management System Tables
echo with Ln prefix
echo ========================================
echo.

REM Check if sqlcmd is available
sqlcmd -? >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: sqlcmd is not available. Please install SQL Server Command Line Utilities.
    pause
    exit /b 1
)

echo Checking table existence...
echo.

REM Run the verification script
sqlcmd -S localhost -E -i "verify_ln_tables.sql"

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo Verification completed!
    echo ========================================
    echo.
) else (
    echo.
    echo ========================================
    echo ERROR: Failed to verify tables
    echo ========================================
    echo.
)

echo.
echo Press any key to continue...
pause >nul

