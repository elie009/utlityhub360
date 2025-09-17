-- Migration Script: Rename Loans table to LnLoans
-- This script renames the existing Loans table to LnLoans to match the model configuration
USE [DBUTILS]
GO

-- Check if the Loans table exists before proceeding
IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
BEGIN
    PRINT 'Starting migration: Renaming Loans table to LnLoans...'
    
    -- Step 1: Drop foreign key constraints that reference the Loans table
    PRINT 'Dropping foreign key constraints...'
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RepaymentSchedules_Loans')
        ALTER TABLE [dbo].[RepaymentSchedules] DROP CONSTRAINT [FK_RepaymentSchedules_Loans]
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_Loans')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_Loans]
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_Loans')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_Loans]
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Loans')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Loans]
    
    -- Step 2: Rename the table from Loans to LnLoans
    PRINT 'Renaming table from Loans to LnLoans...'
    EXEC sp_rename 'dbo.Loans', 'LnLoans'
    
    -- Step 3: Recreate foreign key constraints with the new table name
    PRINT 'Recreating foreign key constraints...'
    
    -- RepaymentSchedules -> LnLoans
    ALTER TABLE [dbo].[RepaymentSchedules] 
    ADD CONSTRAINT [FK_RepaymentSchedules_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    
    -- Payments -> LnLoans
    ALTER TABLE [dbo].[Payments] 
    ADD CONSTRAINT [FK_Payments_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    
    -- Penalties -> LnLoans
    ALTER TABLE [dbo].[Penalties] 
    ADD CONSTRAINT [FK_Penalties_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    
    -- Notifications -> LnLoans
    ALTER TABLE [dbo].[Notifications] 
    ADD CONSTRAINT [FK_Notifications_LnLoans] 
    FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    
    PRINT 'Migration completed successfully! Loans table renamed to LnLoans'
END
ELSE
BEGIN
    PRINT 'Loans table does not exist. No migration needed.'
    
    -- Check if LnLoans table already exists
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        PRINT 'LnLoans table already exists.'
    ELSE
        PRINT 'Neither Loans nor LnLoans table exists. Consider running the initial table creation script.'
END

-- Verify the migration
PRINT 'Verifying migration...'
IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
    PRINT 'SUCCESS: LnLoans table exists'
ELSE
    PRINT 'ERROR: LnLoans table does not exist'

-- Check foreign key constraints
DECLARE @FKCount INT
SELECT @FKCount = COUNT(*) 
FROM sys.foreign_keys 
WHERE referenced_object_id = OBJECT_ID('dbo.LnLoans')

PRINT 'Foreign key constraints referencing LnLoans: ' + CAST(@FKCount AS VARCHAR(10))

GO
