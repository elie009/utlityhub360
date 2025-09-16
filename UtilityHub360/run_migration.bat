@echo off
echo =============================================
echo CREATING LOAN MANAGEMENT SYSTEM TABLES
echo =============================================
echo.

echo Running migration script...
echo.

sqlcmd -S 174.138.185.18 -d DBUTILS -U sa01 -P "iSTc0#T3tw~noz2r" -i simple_migration.sql

echo.
echo Migration completed!
echo.
echo Now run check_tables.bat to verify the tables were created.
echo.
pause
