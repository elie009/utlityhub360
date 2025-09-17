-- Check Current Tables in Database
USE [DBUTILS]
GO

PRINT 'Current Tables in Database:'
PRINT '==========================='
PRINT ''

-- Show all tables
SELECT 
    name as 'Table Name',
    CASE 
        WHEN name LIKE 'Ln%' THEN '✓ Ln Prefixed'
        ELSE '⚠ No Ln Prefix'
    END as 'Status'
FROM sys.tables 
ORDER BY name

PRINT ''
PRINT 'Loan Management Related Tables:'
PRINT '=============================='

-- Show loan management tables specifically
SELECT 
    name as 'Table Name',
    CASE 
        WHEN name LIKE 'Ln%' THEN '✓ Ln Prefixed'
        ELSE '⚠ No Ln Prefix'
    END as 'Status'
FROM sys.tables 
WHERE name IN ('Borrowers', 'LnBorrowers', 'Loans', 'LnLoans', 'Payments', 'LnPayments', 
               'RepaymentSchedules', 'LnRepaymentSchedules', 'Penalties', 'LnPenalties', 
               'Notifications', 'LnNotifications')
ORDER BY name

PRINT ''
PRINT 'Foreign Key Constraints:'
PRINT '======================='

-- Show foreign key constraints
SELECT 
    fk.name as 'Constraint Name',
    OBJECT_NAME(fk.parent_object_id) as 'Referencing Table',
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as 'Referencing Column',
    OBJECT_NAME(fk.referenced_object_id) as 'Referenced Table',
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as 'Referenced Column'
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.referenced_object_id) LIKE '%Borrower%' 
   OR OBJECT_NAME(fk.referenced_object_id) LIKE '%Loan%'
   OR OBJECT_NAME(fk.parent_object_id) LIKE '%Borrower%'
   OR OBJECT_NAME(fk.parent_object_id) LIKE '%Loan%'
ORDER BY OBJECT_NAME(fk.parent_object_id), fk.name

GO
