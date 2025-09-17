-- Comprehensive Migration Script: Handle Loans to LnLoans table rename
-- This script handles both scenarios: existing database with Loans table and new setup
-- Note: This script should be run with: sqlcmd -S 174.138.185.18 -U sa01 -P "iSTc0#T3tw~noz2r" -d DBUTILS -i comprehensive_loans_migration.sql
USE [DBUTILS]
GO

PRINT 'Starting comprehensive migration for Loans to LnLoans...'
PRINT '================================================'

-- Check current state
DECLARE @LoansExists BIT = 0
DECLARE @LnLoansExists BIT = 0

IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
    SET @LoansExists = 1

IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
    SET @LnLoansExists = 1

PRINT 'Current state:'
PRINT '  Loans table exists: ' + CASE WHEN @LoansExists = 1 THEN 'YES' ELSE 'NO' END
PRINT '  LnLoans table exists: ' + CASE WHEN @LnLoansExists = 1 THEN 'YES' ELSE 'NO' END
PRINT ''

-- Scenario 1: Loans table exists, LnLoans does not - Rename Loans to LnLoans
IF @LoansExists = 1 AND @LnLoansExists = 0
BEGIN
    PRINT 'SCENARIO 1: Renaming existing Loans table to LnLoans...'
    
    -- Step 1: Drop foreign key constraints
    PRINT '  Dropping foreign key constraints...'
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RepaymentSchedules_Loans')
    BEGIN
        ALTER TABLE [dbo].[RepaymentSchedules] DROP CONSTRAINT [FK_RepaymentSchedules_Loans]
        PRINT '    Dropped FK_RepaymentSchedules_Loans'
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_Loans')
    BEGIN
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_Loans]
        PRINT '    Dropped FK_Payments_Loans'
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_Loans')
    BEGIN
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_Loans]
        PRINT '    Dropped FK_Penalties_Loans'
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Loans')
    BEGIN
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Loans]
        PRINT '    Dropped FK_Notifications_Loans'
    END
    
    -- Step 2: Rename the table
    PRINT '  Renaming table from Loans to LnLoans...'
    EXEC sp_rename 'dbo.Loans', 'LnLoans'
    PRINT '    Table renamed successfully'
    
    -- Step 3: Recreate foreign key constraints
    PRINT '  Recreating foreign key constraints...'
    
    ALTER TABLE [dbo].[RepaymentSchedules] 
    ADD CONSTRAINT [FK_RepaymentSchedules_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    PRINT '    Created FK_RepaymentSchedules_LnLoans'
    
    ALTER TABLE [dbo].[Payments] 
    ADD CONSTRAINT [FK_Payments_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    PRINT '    Created FK_Payments_LnLoans'
    
    ALTER TABLE [dbo].[Penalties] 
    ADD CONSTRAINT [FK_Penalties_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    PRINT '    Created FK_Penalties_LnLoans'
    
    ALTER TABLE [dbo].[Notifications] 
    ADD CONSTRAINT [FK_Notifications_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    PRINT '    Created FK_Notifications_LnLoans'
    
    PRINT '  SCENARIO 1 COMPLETED: Loans table successfully renamed to LnLoans'
END

-- Scenario 2: LnLoans table already exists - No action needed
ELSE IF @LnLoansExists = 1
BEGIN
    PRINT 'SCENARIO 2: LnLoans table already exists - No migration needed'
    PRINT '  The database is already in the correct state.'
END

-- Scenario 3: Neither table exists - Create LnLoans table
ELSE IF @LoansExists = 0 AND @LnLoansExists = 0
BEGIN
    PRINT 'SCENARIO 3: Neither Loans nor LnLoans table exists - Creating LnLoans table...'
    
    -- Create LnLoans table (same structure as original Loans table)
    CREATE TABLE [dbo].[LnLoans] (
        [LoanId] [int] IDENTITY(1,1) NOT NULL,
        [BorrowerId] [int] NOT NULL,
        [LoanType] [nvarchar](50) NULL,
        [PrincipalAmount] [decimal](18,2) NOT NULL,
        [InterestRate] [decimal](5,2) NOT NULL,
        [TermMonths] [int] NOT NULL,
        [RepaymentFrequency] [nvarchar](20) NULL,
        [AmortizationType] [nvarchar](20) NULL,
        [StartDate] [datetime] NOT NULL,
        [EndDate] [datetime] NULL,
        [Status] [nvarchar](20) NULL,
        [CreatedAt] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_LnLoans] PRIMARY KEY ([LoanId])
    )
    PRINT '    LnLoans table created'
    
    -- Add foreign key to Borrowers table
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U')
    BEGIN
        ALTER TABLE [dbo].[LnLoans] 
        ADD CONSTRAINT [FK_LnLoans_Borrowers] 
        FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[Borrowers] ([BorrowerId])
        PRINT '    Created FK_LnLoans_Borrowers'
    END
    
    PRINT '  SCENARIO 3 COMPLETED: LnLoans table created'
END

-- Scenario 4: Both tables exist - This is an error state
ELSE
BEGIN
    PRINT 'SCENARIO 4: ERROR - Both Loans and LnLoans tables exist!'
    PRINT '  This is an unexpected state. Manual intervention required.'
    PRINT '  Please check your database and resolve this conflict.'
END

-- Final verification
PRINT ''
PRINT 'Final verification:'
PRINT '=================='

IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
    PRINT '✓ LnLoans table exists'
ELSE
    PRINT '✗ LnLoans table does not exist'

-- Check foreign key constraints
DECLARE @FKCount INT
SELECT @FKCount = COUNT(*) 
FROM sys.foreign_keys 
WHERE referenced_object_id = OBJECT_ID('dbo.LnLoans')

PRINT '✓ Foreign key constraints referencing LnLoans: ' + CAST(@FKCount AS VARCHAR(10))

-- Check if Loans table still exists (should not after successful migration)
IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
    PRINT '⚠ WARNING: Loans table still exists (this may be expected in some scenarios)'
ELSE
    PRINT '✓ Loans table no longer exists (migration successful)'

PRINT ''
PRINT 'Migration process completed!'
GO
