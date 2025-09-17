-- Migrate Borrowers to LnBorrowers
USE [DBUTILS]
GO

PRINT 'Migrating Borrowers to LnBorrowers...'

IF EXISTS (SELECT * FROM sysobjects WHERE name='Borrowers' AND xtype='U')
   AND NOT EXISTS (SELECT * FROM sysobjects WHERE name='LnBorrowers' AND xtype='U')
BEGIN
    -- Drop foreign key constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Borrowers')
        ALTER TABLE [dbo].[Loans] DROP CONSTRAINT [FK_Loans_Borrowers]
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Borrowers')
        ALTER TABLE [dbo].[Notifications] DROP CONSTRAINT [FK_Notifications_Borrowers]
    
    -- Rename table
    EXEC sp_rename 'dbo.Borrowers', 'LnBorrowers'
    PRINT 'Borrowers renamed to LnBorrowers'
    
    -- Recreate foreign key constraints
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Loans' AND xtype='U')
        ALTER TABLE [dbo].[Loans] ADD CONSTRAINT [FK_Loans_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
        ALTER TABLE [dbo].[Notifications] ADD CONSTRAINT [FK_Notifications_LnBorrowers] FOREIGN KEY ([BorrowerId]) REFERENCES [dbo].[LnBorrowers] ([BorrowerId])
    
    PRINT 'Foreign key constraints updated'
END
ELSE
    PRINT 'Borrowers table not found or already migrated'

GO
