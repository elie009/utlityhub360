-- Complete Migration Script: Ensure ALL tables have Ln prefix
-- This script will migrate any remaining tables that don't have the Ln prefix
USE [DBUTILS]
GO

PRINT 'Complete Migration: Ensuring all tables have Ln prefix'
PRINT '===================================================='
PRINT ''

-- ==============================================
-- MIGRATE BORROWERS TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U')
BEGIN
    PRINT 'Migrating: Borrowers → LnBorrowers'
    
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Borrowers')
        ALTER TABLE [dbo].[Loans] DROP CONSTRAINT [FK_Loans_Borrowers]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Borrowers')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Borrowers]
    
    -- Rename table
    EXEC sp_rename 'dbo.Borrowers', 'LnBorrowers'
    PRINT '  ✓ Borrowers renamed to LnBorrowers'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
        ALTER TABLE [dbo].[Loans] ADD CONSTRAINT [FK_Loans_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
        ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_Notifications_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE
    PRINT 'Borrowers: Already migrated or not found'

-- ==============================================
-- MIGRATE LOANS TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
BEGIN
    PRINT 'Migrating: Loans → LnLoans'
    
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RepaymentSchedules_Loans')
        ALTER TABLE [dbo].[RepaymentSchedules] DROP CONSTRAINT [FK_RepaymentSchedules_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_Loans')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_Loans')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Loans')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Loans]
    
    -- Rename table
    EXEC sp_rename 'dbo.Loans', 'LnLoans'
    PRINT '  ✓ Loans renamed to LnLoans'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[RepaymentSchedules] ADD CONSTRAINT [FK_RepaymentSchedules_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
        ALTER TABLE [dbo].[Payments] ADD CONSTRAINT [FK_Payments_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
        ALTER TABLE [dbo].[Penalties] ADD CONSTRAINT [FK_Penalties_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
        ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_Notifications_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE
    PRINT 'Loans: Already migrated or not found'

-- ==============================================
-- MIGRATE REPAYMENT SCHEDULES TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
BEGIN
    PRINT 'Migrating: RepaymentSchedules → LnRepaymentSchedules'
    
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_RepaymentSchedules')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_RepaymentSchedules]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_RepaymentSchedules')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.RepaymentSchedules', 'LnRepaymentSchedules'
    PRINT '  ✓ RepaymentSchedules renamed to LnRepaymentSchedules'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
        ALTER TABLE [dbo].[Penalties] ADD CONSTRAINT [FK_Penalties_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
        ALTER TABLE [dbo].[Payments] ADD CONSTRAINT [FK_Payments_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE
    PRINT 'RepaymentSchedules: Already migrated or not found'

-- ==============================================
-- MIGRATE PAYMENTS TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U')
BEGIN
    PRINT 'Migrating: Payments → LnPayments'
    
    -- Rename table (foreign keys already handled above)
    EXEC sp_rename 'dbo.Payments', 'LnPayments'
    PRINT '  ✓ Payments renamed to LnPayments'
END
ELSE
    PRINT 'Payments: Already migrated or not found'

-- ==============================================
-- MIGRATE PENALTIES TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U')
BEGIN
    PRINT 'Migrating: Penalties → LnPenalties'
    
    -- Rename table (foreign keys already handled above)
    EXEC sp_rename 'dbo.Penalties', 'LnPenalties'
    PRINT '  ✓ Penalties renamed to LnPenalties'
END
ELSE
    PRINT 'Penalties: Already migrated or not found'

-- ==============================================
-- MIGRATE NOTIFICATIONS TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U')
BEGIN
    PRINT 'Migrating: Notifications → LnNotifications'
    
    -- Rename table (foreign keys already handled above)
    EXEC sp_rename 'dbo.Notifications', 'LnNotifications'
    PRINT '  ✓ Notifications renamed to LnNotifications'
END
ELSE
    PRINT 'Notifications: Already migrated or not found'

-- ==============================================
-- FINAL VERIFICATION
-- ==============================================
PRINT ''
PRINT 'Final verification:'
PRINT '=================='

-- Check if all Ln tables exist
DECLARE @LnBorrowersExists BIT = 0
DECLARE @LnLoansExists BIT = 0
DECLARE @LnPaymentsExists BIT = 0
DECLARE @LnRepaymentSchedulesExists BIT = 0
DECLARE @LnPenaltiesExists BIT = 0
DECLARE @LnNotificationsExists BIT = 0

IF EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U') SET @LnBorrowersExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U') SET @LnLoansExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U') SET @LnPaymentsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U') SET @LnRepaymentSchedulesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U') SET @LnPenaltiesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U') SET @LnNotificationsExists = 1

PRINT 'LnBorrowers: ' + CASE WHEN @LnBorrowersExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnLoans: ' + CASE WHEN @LnLoansExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnPayments: ' + CASE WHEN @LnPaymentsExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnRepaymentSchedules: ' + CASE WHEN @LnRepaymentSchedulesExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnPenalties: ' + CASE WHEN @LnPenaltiesExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnNotifications: ' + CASE WHEN @LnNotificationsExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END

-- Count foreign key constraints
DECLARE @FKCount INT
SELECT @FKCount = COUNT(*) 
FROM sys.foreign_keys 
WHERE referenced_object_id IN (
    OBJECT_ID('dbo.LnBorrowers'),
    OBJECT_ID('dbo.LnLoans'),
    OBJECT_ID('dbo.LnRepaymentSchedules')
)

PRINT ''
PRINT 'Foreign key constraints: ' + CAST(@FKCount AS VARCHAR(10))

-- Show all tables
PRINT ''
PRINT 'All tables in database:'
PRINT '======================'
SELECT name as 'Table Name' FROM sys.tables ORDER BY name

PRINT ''
PRINT 'Complete migration finished!'
GO
