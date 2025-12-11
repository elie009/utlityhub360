@echo off
echo =========================================
echo Add Country Column to Users Table
echo =========================================
echo.
echo This script will add the Country column to the Users table
echo to fix the "Invalid column name 'Country'" error.
echo.
pause

powershell.exe -ExecutionPolicy Bypass -File "%~dp0run_add_country_migration.ps1"

pause

