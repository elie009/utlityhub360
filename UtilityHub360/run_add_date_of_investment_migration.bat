@echo off
REM Batch script to add DateOfInvestment column to Investments table
powershell.exe -ExecutionPolicy Bypass -File "%~dp0run_add_date_of_investment_migration.ps1"
pause

