-- Verification Script: Check Loans to LnLoans Migration Status
USE [DBUTILS]
GO

PRINT 'Verification Report: Loans to LnLoans Migration'
PRINT '==============================================='
PRINT ''

-- Check table existence
DECLARE @LoansExists BIT = 0
DECLARE @LnLoansExists BIT = 0

IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
    SET @LoansExists = 1

IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
    SET @LnLoansExists = 1

PRINT 'Table Status:'
PRINT '  Loans table exists: ' + CASE WHEN @LoansExists = 1 THEN 'YES' ELSE 'NO' END
PRINT '  LnLoans table exists: ' + CASE WHEN @LnLoansExists = 1 THEN 'YES' ELSE 'NO' END
PRINT ''

-- Check LnLoans table structure
IF @LnLoansExists = 1
BEGIN
    PRINT 'LnLoans Table Structure:'
    PRINT '------------------------'
    
    SELECT 
        COLUMN_NAME as 'Column Name',
        DATA_TYPE as 'Data Type',
        CHARACTER_MAXIMUM_LENGTH as 'Max Length',
        IS_NULLABLE as 'Nullable',
        COLUMN_DEFAULT as 'Default Value'
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'LnLoans'
    ORDER BY ORDINAL_POSITION
    
    PRINT ''
    
    -- Check primary key
    PRINT 'Primary Key:'
    SELECT 
        kcu.COLUMN_NAME as 'Primary Key Column'
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
        ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
    WHERE tc.TABLE_NAME = 'LnLoans' 
        AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
    
    PRINT ''
END

-- Check foreign key constraints
PRINT 'Foreign Key Constraints:'
PRINT '------------------------'
PRINT 'Constraints referencing LnLoans:'

SELECT 
    fk.name as 'Constraint Name',
    OBJECT_NAME(fk.parent_object_id) as 'Referencing Table',
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as 'Referencing Column',
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as 'Referenced Column'
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fk.referenced_object_id = OBJECT_ID('dbo.LnLoans')
ORDER BY OBJECT_NAME(fk.parent_object_id), fk.name

-- Count foreign keys
DECLARE @FKCount INT
SELECT @FKCount = COUNT(*) 
FROM sys.foreign_keys 
WHERE referenced_object_id = OBJECT_ID('dbo.LnLoans')

PRINT ''
PRINT 'Total foreign key constraints: ' + CAST(@FKCount AS VARCHAR(10))

-- Check for any remaining references to old Loans table
PRINT ''
PRINT 'Checking for remaining Loans table references:'
PRINT '---------------------------------------------'

SELECT 
    fk.name as 'Constraint Name',
    OBJECT_NAME(fk.parent_object_id) as 'Referencing Table'
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id = OBJECT_ID('dbo.Loans')

DECLARE @OldFKCount INT
SELECT @OldFKCount = COUNT(*) 
FROM sys.foreign_keys 
WHERE referenced_object_id = OBJECT_ID('dbo.Loans')

IF @OldFKCount > 0
    PRINT 'WARNING: Found ' + CAST(@OldFKCount AS VARCHAR(10)) + ' foreign key constraints still referencing the old Loans table!'
ELSE
    PRINT '✓ No foreign key constraints found referencing the old Loans table'

-- Migration status summary
PRINT ''
PRINT 'Migration Status Summary:'
PRINT '========================'

IF @LnLoansExists = 1 AND @LoansExists = 0
    PRINT '✓ MIGRATION SUCCESSFUL: LnLoans table exists, Loans table removed'
ELSE IF @LnLoansExists = 1 AND @LoansExists = 1
    PRINT '⚠ PARTIAL MIGRATION: Both tables exist (manual cleanup may be needed)'
ELSE IF @LnLoansExists = 0 AND @LoansExists = 1
    PRINT '✗ MIGRATION INCOMPLETE: Loans table exists, LnLoans table missing'
ELSE
    PRINT '? UNKNOWN STATE: Neither table exists'

PRINT ''
PRINT 'Verification completed!'
GO
