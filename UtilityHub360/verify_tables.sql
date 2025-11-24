-- Quick verification script - Run this first to check if tables exist
SELECT 
    TABLE_NAME,
    CASE 
        WHEN TABLE_NAME IN ('BankStatements', 'BankStatementItems', 'Reconciliations', 'ReconciliationMatches') 
        THEN 'EXISTS' 
        ELSE 'MISSING' 
    END AS Status
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('BankStatements', 'BankStatementItems', 'Reconciliations', 'ReconciliationMatches')
ORDER BY TABLE_NAME;

-- If no rows are returned, the tables don't exist and you need to run the migration script

