-- Comprehensive Migration Script: Rename All Tables to Ln Prefix
-- This script renames all loan management tables to match the model configurations
-- Note: This script should be run with: sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i comprehensive_all_tables_migration.sql
USE [DBUTILS]
GO

PRINT 'Starting comprehensive migration for all loan management tables...'
PRINT '================================================================'
PRINT ''

-- Check current state of all tables
DECLARE @BorrowersExists BIT = 0
DECLARE @LnBorrowersExists BIT = 0
DECLARE @LoansExists BIT = 0
DECLARE @LnLoansExists BIT = 0
DECLARE @PaymentsExists BIT = 0
DECLARE @LnPaymentsExists BIT = 0
DECLARE @RepaymentSchedulesExists BIT = 0
DECLARE @LnRepaymentSchedulesExists BIT = 0
DECLARE @PenaltiesExists BIT = 0
DECLARE @LnPenaltiesExists BIT = 0
DECLARE @NotificationsExists BIT = 0
DECLARE @LnNotificationsExists BIT = 0

-- Check table existence
IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U') SET @BorrowersExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U') SET @LnBorrowersExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U') SET @LoansExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U') SET @LnLoansExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U') SET @PaymentsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U') SET @LnPaymentsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U') SET @RepaymentSchedulesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U') SET @LnRepaymentSchedulesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U') SET @PenaltiesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U') SET @LnPenaltiesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U') SET @NotificationsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U') SET @LnNotificationsExists = 1

PRINT 'Current table state:'
PRINT '  Borrowers exists: ' + CASE WHEN @BorrowersExists = 1 THEN 'YES' ELSE 'NO' END + ' | LnBorrowers exists: ' + CASE WHEN @LnBorrowersExists = 1 THEN 'YES' ELSE 'NO' END
PRINT '  Loans exists: ' + CASE WHEN @LoansExists = 1 THEN 'YES' ELSE 'NO' END + ' | LnLoans exists: ' + CASE WHEN @LnLoansExists = 1 THEN 'YES' ELSE 'NO' END
PRINT '  Payments exists: ' + CASE WHEN @PaymentsExists = 1 THEN 'YES' ELSE 'NO' END + ' | LnPayments exists: ' + CASE WHEN @LnPaymentsExists = 1 THEN 'YES' ELSE 'NO' END
PRINT '  RepaymentSchedules exists: ' + CASE WHEN @RepaymentSchedulesExists = 1 THEN 'YES' ELSE 'NO' END + ' | LnRepaymentSchedules exists: ' + CASE WHEN @LnRepaymentSchedulesExists = 1 THEN 'YES' ELSE 'NO' END
PRINT '  Penalties exists: ' + CASE WHEN @PenaltiesExists = 1 THEN 'YES' ELSE 'NO' END + ' | LnPenalties exists: ' + CASE WHEN @LnPenaltiesExists = 1 THEN 'YES' ELSE 'NO' END
PRINT '  Notifications exists: ' + CASE WHEN @NotificationsExists = 1 THEN 'YES' ELSE 'NO' END + ' | LnNotifications exists: ' + CASE WHEN @LnNotificationsExists = 1 THEN 'YES' ELSE 'NO' END
PRINT ''

-- ==============================================
-- MIGRATE BORROWERS TABLE
-- ==============================================
IF @BorrowersExists = 1 AND @LnBorrowersExists = 0
BEGIN
    PRINT 'MIGRATING: Borrowers → LnBorrowers'
    
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Borrowers')
        ALTER TABLE [dbo].[Loans] DROP CONSTRAINT [FK_Loans_Borrowers]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LnLoans_Borrowers')
        ALTER TABLE [dbo].[LnLoans] DROP CONSTRAINT [FK_LnLoans_Borrowers]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Borrowers')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Borrowers]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LnNotifications_Borrowers')
        ALTER TABLE [dbo].[LnNotifications] DROP CONSTRAINT [FK_LnNotifications_Borrowers]
    
    -- Rename table
    EXEC sp_rename 'dbo.Borrowers', 'LnBorrowers'
    PRINT '  ✓ Borrowers renamed to LnBorrowers'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnLoans] ADD CONSTRAINT [FK_LnLoans_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
        ALTER TABLE [dbo].[Loans] ADD CONSTRAINT [FK_Loans_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
        ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_Notifications_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF @LnBorrowersExists = 1
    PRINT 'Borrowers: LnBorrowers already exists - No migration needed'
ELSE
    PRINT 'Borrowers: Neither table exists - No migration needed'

-- ==============================================
-- MIGRATE LOANS TABLE
-- ==============================================
IF @LoansExists = 1 AND @LnLoansExists = 0
BEGIN
    PRINT 'MIGRATING: Loans → LnLoans'
    
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
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnRepaymentSchedules] ADD CONSTRAINT [FK_LnRepaymentSchedules_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[RepaymentSchedules] ADD CONSTRAINT [FK_RepaymentSchedules_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
        ALTER TABLE [dbo].[Payments] ADD CONSTRAINT [FK_Payments_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
        ALTER TABLE [dbo].[Penalties] ADD CONSTRAINT [FK_Penalties_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
        ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_Notifications_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF @LnLoansExists = 1
    PRINT 'Loans: LnLoans already exists - No migration needed'
ELSE
    PRINT 'Loans: Neither table exists - No migration needed'

-- ==============================================
-- MIGRATE PAYMENTS TABLE
-- ==============================================
IF @PaymentsExists = 1 AND @LnPaymentsExists = 0
BEGIN
    PRINT 'MIGRATING: Payments → LnPayments'
    
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_RepaymentSchedules')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.Payments', 'LnPayments'
    PRINT '  ✓ Payments renamed to LnPayments'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_RepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[RepaymentSchedules] ([ScheduleId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF @LnPaymentsExists = 1
    PRINT 'Payments: LnPayments already exists - No migration needed'
ELSE
    PRINT 'Payments: Neither table exists - No migration needed'

-- ==============================================
-- MIGRATE REPAYMENT SCHEDULES TABLE
-- ==============================================
IF @RepaymentSchedulesExists = 1 AND @LnRepaymentSchedulesExists = 0
BEGIN
    PRINT 'MIGRATING: RepaymentSchedules → LnRepaymentSchedules'
    
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_RepaymentSchedules')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.RepaymentSchedules', 'LnRepaymentSchedules'
    PRINT '  ✓ RepaymentSchedules renamed to LnRepaymentSchedules'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
        ALTER TABLE [dbo].[Penalties] ADD CONSTRAINT [FK_Penalties_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    
    PRINT '  ✓ Foreign key constraints updated'
END
ELSE IF @LnRepaymentSchedulesExists = 1
    PRINT 'RepaymentSchedules: LnRepaymentSchedules already exists - No migration needed'
ELSE
    PRINT 'RepaymentSchedules: Neither table exists - No migration needed'

-- ==============================================
-- MIGRATE PENALTIES TABLE
-- ==============================================
IF @PenaltiesExists = 1 AND @LnPenaltiesExists = 0
BEGIN
    PRINT 'MIGRATING: Penalties → LnPenalties'
    
    -- Rename table (no foreign keys to drop for this table)
    EXEC sp_rename 'dbo.Penalties', 'LnPenalties'
    PRINT '  ✓ Penalties renamed to LnPenalties'
END
ELSE IF @LnPenaltiesExists = 1
    PRINT 'Penalties: LnPenalties already exists - No migration needed'
ELSE
    PRINT 'Penalties: Neither table exists - No migration needed'

-- ==============================================
-- MIGRATE NOTIFICATIONS TABLE
-- ==============================================
IF @NotificationsExists = 1 AND @LnNotificationsExists = 0
BEGIN
    PRINT 'MIGRATING: Notifications → LnNotifications'
    
    -- Rename table (foreign keys already handled above)
    EXEC sp_rename 'dbo.Notifications', 'LnNotifications'
    PRINT '  ✓ Notifications renamed to LnNotifications'
END
ELSE IF @LnNotificationsExists = 1
    PRINT 'Notifications: LnNotifications already exists - No migration needed'
ELSE
    PRINT 'Notifications: Neither table exists - No migration needed'

-- ==============================================
-- FINAL VERIFICATION
-- ==============================================
PRINT ''
PRINT 'Final verification:'
PRINT '=================='

-- Check final state
DECLARE @FinalLnBorrowersExists BIT = 0
DECLARE @FinalLnLoansExists BIT = 0
DECLARE @FinalLnPaymentsExists BIT = 0
DECLARE @FinalLnRepaymentSchedulesExists BIT = 0
DECLARE @FinalLnPenaltiesExists BIT = 0
DECLARE @FinalLnNotificationsExists BIT = 0

IF EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U') SET @FinalLnBorrowersExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U') SET @FinalLnLoansExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U') SET @FinalLnPaymentsExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U') SET @FinalLnRepaymentSchedulesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U') SET @FinalLnPenaltiesExists = 1
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U') SET @FinalLnNotificationsExists = 1

PRINT 'LnBorrowers: ' + CASE WHEN @FinalLnBorrowersExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnLoans: ' + CASE WHEN @FinalLnLoansExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnPayments: ' + CASE WHEN @FinalLnPaymentsExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnRepaymentSchedules: ' + CASE WHEN @FinalLnRepaymentSchedulesExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnPenalties: ' + CASE WHEN @FinalLnPenaltiesExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END
PRINT 'LnNotifications: ' + CASE WHEN @FinalLnNotificationsExists = 1 THEN '✓ EXISTS' ELSE '✗ MISSING' END

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
PRINT 'Migration completed successfully!'
PRINT 'All loan management tables now have the Ln prefix to match your Entity Framework models.'
GO
