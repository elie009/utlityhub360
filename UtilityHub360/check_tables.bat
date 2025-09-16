@echo off
echo =============================================
echo CHECKING LOAN MANAGEMENT SYSTEM TABLES
echo =============================================
echo.

echo Running verification script...
echo.

sqlcmd -S 174.138.185.18 -d DBUTILS -U sa01 -P "iSTc0#T3tw~noz2r" -i verify_tables.sql

echo.
echo Verification completed!
echo.
pause
