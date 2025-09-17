-- Verify Loan Management System Tables with Ln prefix
-- This script checks if all loan-related tables exist with the correct Ln prefix

SELECT 
    'LnBorrowers' as TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'LnBorrowers') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END as Status
UNION ALL
SELECT 
    'LnLoans' as TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'LnLoans') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END as Status
UNION ALL
SELECT 
    'LnRepaymentSchedules' as TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'LnRepaymentSchedules') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END as Status
UNION ALL
SELECT 
    'LnPayments' as TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'LnPayments') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END as Status
UNION ALL
SELECT 
    'LnPenalties' as TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'LnPenalties') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END as Status
UNION ALL
SELECT 
    'LnNotifications' as TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'LnNotifications') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END as Status;

-- Show all tables with Ln prefix
SELECT 
    name as TableName,
    create_date as CreatedDate
FROM sys.tables 
WHERE name LIKE 'Ln%'
ORDER BY name;

