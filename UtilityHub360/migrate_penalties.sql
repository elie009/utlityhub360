-- Migrate Penalties to LnPenalties
USE [DBUTILS]
GO

PRINT 'Migrating Penalties to LnPenalties...'

IF EXISTS (SELECT * FROM sysobjects WHERE name='Penalties' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnPenalties' AND xtype='U')
BEGIN
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_Loans')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Penalties_RepaymentSchedules')
        ALTER TABLE [dbo].[Penalties] DROP CONSTRAINT [FK_Penalties_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.Penalties', 'LnPenalties'
    PRINT 'Penalties renamed to LnPenalties'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPenalties] ADD CONSTRAINT [FK_LnPenalties_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    
    PRINT 'Foreign key constraints updated'
END
ELSE
    PRINT 'Penalties table not found or already migrated'

GO
