-- =============================================
-- DIAGNOSTIC SCRIPT - Run this to check everything
-- =============================================

-- 1. Check which database you're connected to
SELECT DB_NAME() AS CurrentDatabase;
GO

-- 2. Check if reconciliation tables exist
SELECT 
    TABLE_NAME,
    CASE 
        WHEN TABLE_NAME IN ('BankStatements', 'BankStatementItems', 'Reconciliations', 'ReconciliationMatches') 
        THEN '✅ EXISTS' 
        ELSE '❌ MISSING' 
    END AS Status
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('BankStatements', 'BankStatementItems', 'Reconciliations', 'ReconciliationMatches')
ORDER BY TABLE_NAME;
GO

-- 3. If no rows returned above, the tables DON'T EXIST
--    You need to run: run_migration_direct.sql

-- 4. Check all tables in the database (to see what exists)
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

-- 5. Check if Users and BankAccounts tables exist (required for foreign keys)
SELECT 
    CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users') 
         THEN '✅ Users table exists' 
         ELSE '❌ Users table MISSING' 
    END AS UsersTableStatus,
    CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BankAccounts') 
         THEN '✅ BankAccounts table exists' 
         ELSE '❌ BankAccounts table MISSING' 
    END AS BankAccountsTableStatus;
GO

