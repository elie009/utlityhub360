-- Migrate RepaymentSchedules to LnRepaymentSchedules
USE [DBUTILS]
GO

PRINT 'Migrating RepaymentSchedules to LnRepaymentSchedules...'

IF EXISTS (SELECT * FROM sysobjects WHERE name='RepaymentSchedules' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
BEGIN
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RepaymentSchedules_Loans')
        ALTER TABLE [dbo].[RepaymentSchedules] DROP CONSTRAINT [FK_RepaymentSchedules_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_RepaymentSchedules')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.RepaymentSchedules', 'LnRepaymentSchedules'
    PRINT 'RepaymentSchedules renamed to LnRepaymentSchedules'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnRepaymentSchedules] ADD CONSTRAINT [FK_LnRepaymentSchedules_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
        ALTER TABLE [dbo].[Penalties] ADD CONSTRAINT [FK_Penalties_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    
    PRINT 'Foreign key constraints updated'
END
ELSE
    PRINT 'RepaymentSchedules table not found or already migrated'

GO
