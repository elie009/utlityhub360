-- Migrate Payments to LnPayments
USE [DBUTILS]
GO

PRINT 'Migrating Payments to LnPayments...'

IF EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnPayments' AND xtype='U')
BEGIN
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_Loans')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_Loans]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Payments_RepaymentSchedules')
        ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_RepaymentSchedules]
    
    -- Rename table
    EXEC sp_rename 'dbo.Payments', 'LnPayments'
    PRINT 'Payments renamed to LnPayments'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnRepaymentSchedules' AND xtype='U')
        ALTER TABLE [dbo].[LnPayments] ADD CONSTRAINT [FK_LnPayments_LnRepaymentSchedules] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[LnRepaymentSchedules] ([ScheduleId])
    
    PRINT 'Foreign key constraints updated'
END
ELSE
    PRINT 'Payments table not found or already migrated'

GO
