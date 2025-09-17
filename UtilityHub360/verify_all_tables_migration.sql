-- Verification Script: Check All Tables Migration Status
USE [DBUTILS]
GO

PRINT 'Verification Report: All Tables Migration Status'
PRINT '==============================================='
PRINT ''

-- Check table existence
DECLARE @LnBorrowersExists BIT = 0
DECLARE @LnLoansExists BIT = 0
DECLARE @LnPaymentsExists BIT = 0
DECLARE @LnRepaymentSchedulesExists BIT = 0
DECLARE @LnPenaltiesExists BIT = 0
DECLARE @LnNotificationsExists BIT = 0

-- Check old table names
DECLARE @BorrowersExists BIT = 0
DECLARE @LoansExists BIT = 0
DECLARE @PaymentsExists BIT = 0
DECLARE @RepaymentSchedulesExists BIT = 0
DECLARE @PenaltiesExists BIT = 0
DECLARE @NotificationsExists BIT = 0

-- Check new table existence
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U') SET @LnBorrowersExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U') SET @LnLoansExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U') SET @LnPaymentsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U') SET @LnRepaymentSchedulesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U') SET @LnPenaltiesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U') SET @LnNotificationsExists = 1

-- Check old table existence
IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U') SET @BorrowersExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U') SET @LoansExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U') SET @PaymentsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U') SET @RepaymentSchedulesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U') SET @PenaltiesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U') SET @NotificationsExists = 1

PRINT 'Table Migration Status:'
PRINT '======================'
PRINT 'Borrowers → LnBorrowers: ' + 
    CASE 
        WHEN @LnBorrowersExists = 1 AND @BorrowersExists = 0 THEN '✓ MIGRATED'
        WHEN @LnBorrowersExists = 1 AND @BorrowersExists = 1 THEN '⚠ BOTH EXIST'
        WHEN @LnBorrowersExists = 0 AND @BorrowersExists = 1 THEN '✗ NOT MIGRATED'
        ELSE '? UNKNOWN'
    END

PRINT 'Loans → LnLoans: ' + 
    CASE 
        WHEN @LnLoansExists = 1 AND @LoansExists = 0 THEN '✓ MIGRATED'
        WHEN @LnLoansExists = 1 AND @LoansExists = 1 THEN '⚠ BOTH EXIST'
        WHEN @LnLoansExists = 0 AND @LoansExists = 1 THEN '✗ NOT MIGRATED'
        ELSE '? UNKNOWN'
    END

PRINT 'Payments → LnPayments: ' + 
    CASE 
        WHEN @LnPaymentsExists = 1 AND @PaymentsExists = 0 THEN '✓ MIGRATED'
        WHEN @LnPaymentsExists = 1 AND @PaymentsExists = 1 THEN '⚠ BOTH EXIST'
        WHEN @LnPaymentsExists = 0 AND @PaymentsExists = 1 THEN '✗ NOT MIGRATED'
        ELSE '? UNKNOWN'
    END

PRINT 'RepaymentSchedules → LnRepaymentSchedules: ' + 
    CASE 
        WHEN @LnRepaymentSchedulesExists = 1 AND @RepaymentSchedulesExists = 0 THEN '✓ MIGRATED'
        WHEN @LnRepaymentSchedulesExists = 1 AND @RepaymentSchedulesExists = 1 THEN '⚠ BOTH EXIST'
        WHEN @LnRepaymentSchedulesExists = 0 AND @RepaymentSchedulesExists = 1 THEN '✗ NOT MIGRATED'
        ELSE '? UNKNOWN'
    END

PRINT 'Penalties → LnPenalties: ' + 
    CASE 
        WHEN @LnPenaltiesExists = 1 AND @PenaltiesExists = 0 THEN '✓ MIGRATED'
        WHEN @LnPenaltiesExists = 1 AND @PenaltiesExists = 1 THEN '⚠ BOTH EXIST'
        WHEN @LnPenaltiesExists = 0 AND @PenaltiesExists = 1 THEN '✗ NOT MIGRATED'
        ELSE '? UNKNOWN'
    END

PRINT 'Notifications → LnNotifications: ' + 
    CASE 
        WHEN @LnNotificationsExists = 1 AND @NotificationsExists = 0 THEN '✓ MIGRATED'
        WHEN @LnNotificationsExists = 1 AND @NotificationsExists = 1 THEN '⚠ BOTH EXIST'
        WHEN @LnNotificationsExists = 0 AND @NotificationsExists = 1 THEN '✗ NOT MIGRATED'
        ELSE '? UNKNOWN'
    END

PRINT ''

-- Show all tables in the database
PRINT 'All Tables in Database:'
PRINT '======================'
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

-- Check foreign key constraints
PRINT 'Foreign Key Constraints:'
PRINT '======================='
SELECT 
    fk.name as 'Constraint Name',
    OBJECT_NAME(fk.parent_object_id) as 'Referencing Table',
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as 'Referencing Column',
    OBJECT_NAME(fk.referenced_object_id) as 'Referenced Table',
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as 'Referenced Column'
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.referenced_object_id) LIKE 'Ln%'
   OR OBJECT_NAME(fk.parent_object_id) LIKE 'Ln%'
ORDER BY OBJECT_NAME(fk.parent_object_id), fk.name

-- Count foreign keys
DECLARE @FKCount INT
SELECT @FKCount = COUNT(*) 
FROM sys.foreign_keys 
WHERE OBJECT_NAME(referenced_object_id) LIKE 'Ln%'

PRINT ''
PRINT 'Total foreign key constraints involving Ln tables: ' + CAST(@FKCount AS VARCHAR(10))

-- Migration summary
PRINT ''
PRINT 'Migration Summary:'
PRINT '=================='

DECLARE @MigratedCount INT = 0
DECLARE @TotalCount INT = 6

IF @LnBorrowersExists = 1 SET @MigratedCount = @MigratedCount + 1
IF @LnLoansExists = 1 SET @MigratedCount = @MigratedCount + 1
IF @LnPaymentsExists = 1 SET @MigratedCount = @MigratedCount + 1
IF @LnRepaymentSchedulesExists = 1 SET @MigratedCount = @MigratedCount + 1
IF @LnPenaltiesExists = 1 SET @MigratedCount = @MigratedCount + 1
IF @LnNotificationsExists = 1 SET @MigratedCount = @MigratedCount + 1

PRINT 'Tables with Ln prefix: ' + CAST(@MigratedCount AS VARCHAR(10)) + ' / ' + CAST(@TotalCount AS VARCHAR(10))

IF @MigratedCount = @TotalCount
    PRINT '✓ ALL TABLES SUCCESSFULLY MIGRATED!'
ELSE IF @MigratedCount > 0
    PRINT '⚠ PARTIAL MIGRATION - Some tables still need migration'
ELSE
    PRINT '✗ NO TABLES MIGRATED - Migration may have failed'

PRINT ''
PRINT 'Verification completed!'
GO
