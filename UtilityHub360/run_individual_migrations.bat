@echo off
echo Running Individual Table Migrations
echo ===================================
echo.

echo Step 1: Migrating Borrowers...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i migrate_borrowers.sql
echo.

echo Step 2: Migrating RepaymentSchedules...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i migrate_schedules.sql
echo.

echo Step 3: Migrating Payments...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i migrate_payments.sql
echo.

echo Step 4: Migrating Penalties...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i migrate_penalties.sql
echo.

echo Step 5: Migrating Notifications...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i migrate_notifications.sql
echo.

echo All migrations completed!
echo.
echo Checking final status...
sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i quick_table_check.sql

pause
