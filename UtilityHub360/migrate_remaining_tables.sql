-- Migration Script: Migrate Remaining Tables (Notifications, Payments, Penalties)
-- This script specifically targets the tables that may not have been migrated yet
USE [DBUTILS]
GO

PRINT 'Migrating Remaining Tables...'
PRINT '============================'
PRINT ''

-- ==============================================
-- MIGRATE NOTIFICATIONS TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
BEGIN
    PRINT 'Migrating: Notifications → LnNotifications'
    
    -- Drop foreign key constraints that reference Notifications
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Borrowers')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Borrowers]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Loans')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Loans]
    
    -- Rename table
    EXEC sp_rename 'dbo.Notifications', 'LnNotifications'
    PRINT '  ✓ Notifications renamed to LnNotifications'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_Borrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[Borrowers] ([BorrowerId])
    
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U')
    PRINT 'Notifications: LnNotifications already exists - No migration needed'
ELSE
    PRINT 'Notifications: Table not found - No migration needed'

-- ==============================================
-- MIGRATE PAYMENTS TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
BEGIN
    PRINT 'Migrating: Payments → LnPayments'
    
    -- Drop foreign key constraints that reference Payments
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_Loans')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_RepaymentSchedules')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.Payments', 'LnPayments'
    PRINT '  ✓ Payments renamed to LnPayments'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_RepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[RepaymentSchedules] ([ScheduleId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U')
    PRINT 'Payments: LnPayments already exists - No migration needed'
ELSE
    PRINT 'Payments: Table not found - No migration needed'

-- ==============================================
-- MIGRATE PENALTIES TABLE
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
BEGIN
    PRINT 'Migrating: Penalties → LnPenalties'
    
    -- Drop foreign key constraints that reference Penalties
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_Loans')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_RepaymentSchedules')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.Penalties', 'LnPenalties'
    PRINT '  ✓ Penalties renamed to LnPenalties'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_RepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[RepaymentSchedules] ([ScheduleId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U')
    PRINT 'Penalties: LnPenalties already exists - No migration needed'
ELSE
    PRINT 'Penalties: Table not found - No migration needed'

-- ==============================================
-- MIGRATE REPAYMENT SCHEDULES TABLE (if not done)
-- ==============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
BEGIN
    PRINT 'Migrating: RepaymentSchedules → LnRepaymentSchedules'
    
    -- Drop foreign key constraints that reference RepaymentSchedules
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RepaymentSchedules_Loans')
        ALTER TABLE [dbo].[RepaymentSchedules] DROP CONSTRAINT [FK_RepaymentSchedules_Loans]
    
    -- Rename table
    EXEC sp_rename 'dbo.RepaymentSchedules', 'LnRepaymentSchedules'
    PRINT '  ✓ RepaymentSchedules renamed to LnRepaymentSchedules'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnRepaymentSchedules] ADD CONSTRAINT [FK_LnRepaymentSchedules_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
        ALTER TABLE [dbo].[LnRepaymentSchedules] ADD CONSTRAINT [FK_LnRepaymentSchedules_Loans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[Loans] ([LoanId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
    PRINT 'RepaymentSchedules: LnRepaymentSchedules already exists - No migration needed'
ELSE
    PRINT 'RepaymentSchedules: Table not found - No migration needed'

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

PRINT ''
PRINT 'Remaining tables migration completed!'
GO
