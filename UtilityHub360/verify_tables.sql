-- =============================================
-- VERIFY LOAN MANAGEMENT SYSTEM TABLES
-- =============================================

USE [DBUTILS]
GO

PRINT '============================================='
PRINT 'CHECKING LOAN MANAGEMENT SYSTEM TABLES'
PRINT '============================================='
PRINT ''

-- Check if tables exist
DECLARE @tableCount INT = 0
DECLARE @expectedTables INT = 6

-- Check Borrowers table
IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U')
BEGIN
    PRINT '✅ Borrowers table - EXISTS'
    SET @tableCount = @tableCount + 1
END
ELSE
    PRINT '❌ Borrowers table - MISSING'

-- Check Loans table
IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
BEGIN
    PRINT '✅ Loans table - EXISTS'
    SET @tableCount = @tableCount + 1
END
ELSE
    PRINT '❌ Loans table - MISSING'

-- Check RepaymentSchedules table
IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
BEGIN
    PRINT '✅ RepaymentSchedules table - EXISTS'
    SET @tableCount = @tableCount + 1
END
ELSE
    PRINT '❌ RepaymentSchedules table - MISSING'

-- Check Payments table
IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
BEGIN
    PRINT '✅ Payments table - EXISTS'
    SET @tableCount = @tableCount + 1
END
ELSE
    PRINT '❌ Payments table - MISSING'

-- Check Penalties table
IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
BEGIN
    PRINT '✅ Penalties table - EXISTS'
    SET @tableCount = @tableCount + 1
END
ELSE
    PRINT '❌ Penalties table - MISSING'

-- Check Notifications table
IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
BEGIN
    PRINT '✅ Notifications table - EXISTS'
    SET @tableCount = @tableCount + 1
END
ELSE
    PRINT '❌ Notifications table - MISSING'

PRINT ''
PRINT '============================================='
PRINT 'SUMMARY'
PRINT '============================================='
PRINT 'Tables found: ' + CAST(@tableCount AS VARCHAR(10)) + ' out of ' + CAST(@expectedTables AS VARCHAR(10))

IF @tableCount = @expectedTables
BEGIN
    PRINT '🎉 SUCCESS: All Loan Management System tables created!'
END
ELSE
BEGIN
    PRINT '⚠️  WARNING: Some tables are missing. Please run the migration script.'
END

PRINT '============================================='
GO

-- Show table details
PRINT ''
PRINT 'TABLE DETAILS:'
PRINT '============================================='

-- Show Borrowers table structure
IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U')
BEGIN
    PRINT ''
    PRINT 'BORROWERS TABLE COLUMNS:'
    SELECT 
        COLUMN_NAME as 'Column',
        DATA_TYPE as 'Type',
        IS_NULLABLE as 'Nullable',
        COLUMN_DEFAULT as 'Default'
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Borrowers'
    ORDER BY ORDINAL_POSITION
END

-- Show Loans table structure
IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
BEGIN
    PRINT ''
    PRINT 'LOANS TABLE COLUMNS:'
    SELECT 
        COLUMN_NAME as 'Column',
        DATA_TYPE as 'Type',
        IS_NULLABLE as 'Nullable',
        COLUMN_DEFAULT as 'Default'
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Loans'
    ORDER BY ORDINAL_POSITION
END

-- Show foreign key relationships
PRINT ''
PRINT 'FOREIGN KEY RELATIONSHIPS:'
PRINT '============================================='
SELECT 
    fk.name AS 'Foreign Key',
    tp.name AS 'Parent Table',
    cp.name AS 'Parent Column',
    tr.name AS 'Referenced Table',
    cr.name AS 'Referenced Column'
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('Borrowers', 'Loans', 'RepaymentSchedules', 'Payments', 'Penalties', 'Notifications')
ORDER BY tp.name, fk.name

PRINT ''
PRINT 'Verification completed!'
GO
