-- Migrate Notifications to LnNotifications
USE [DBUTILS]
GO

PRINT 'Migrating Notifications to LnNotifications...'

IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnNotifications' AND xtype='U')
BEGIN
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Borrowers')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Borrowers]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Loans')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Loans]
    
    -- Rename table
    EXEC sp_rename 'dbo.Notifications', 'LnNotifications'
    PRINT 'Notifications renamed to LnNotifications'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='LnLoans' AND xtype='U')
        ALTER TABLE [dbo].[LnNotifications] ADD CONSTRAINT [FK_LnNotifications_LnLoans] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LnLoans] ([LoanId])
    
    PRINT 'Foreign key constraints updated'
END
ELSE
    PRINT 'Notifications table not found or already migrated'

GO
