-- Quick Migration: Add Bill Payment Scheduling and Approval Workflow Columns
-- Run this script in SQL Server Management Studio or Azure Data Studio
-- Replace [YourDatabaseName] with your actual database name

-- ============================================
-- Add Scheduled Payment Columns
-- ============================================

-- IsScheduledPayment
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'IsScheduledPayment')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [IsScheduledPayment] [bit] NOT NULL DEFAULT 0;
END
GO

-- ScheduledPaymentBankAccountId
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentBankAccountId')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [ScheduledPaymentBankAccountId] [nvarchar](450) NULL;
    CREATE INDEX [IX_Bills_ScheduledPaymentBankAccountId] ON [dbo].[Bills] ([ScheduledPaymentBankAccountId]);
END
GO

-- ScheduledPaymentDaysBeforeDue
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentDaysBeforeDue')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [ScheduledPaymentDaysBeforeDue] [int] NULL;
END
GO

-- LastScheduledPaymentAttempt
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'LastScheduledPaymentAttempt')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [LastScheduledPaymentAttempt] [datetime2](7) NULL;
END
GO

-- ScheduledPaymentFailureReason
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentFailureReason')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [ScheduledPaymentFailureReason] [nvarchar](500) NULL;
END
GO

-- ============================================
-- Add Approval Workflow Columns
-- ============================================

-- ApprovalStatus (handle NOT NULL with default)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovalStatus')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [ApprovalStatus] [nvarchar](20) NULL;
    UPDATE [dbo].[Bills] SET [ApprovalStatus] = 'APPROVED' WHERE [ApprovalStatus] IS NULL;
    ALTER TABLE [dbo].[Bills] ALTER COLUMN [ApprovalStatus] [nvarchar](20) NOT NULL;
    ALTER TABLE [dbo].[Bills] ADD CONSTRAINT [DF_Bills_ApprovalStatus] DEFAULT 'APPROVED' FOR [ApprovalStatus];
    CREATE INDEX [IX_Bills_ApprovalStatus] ON [dbo].[Bills] ([ApprovalStatus]);
END
GO

-- ApprovedBy
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovedBy')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [ApprovedBy] [nvarchar](450) NULL;
    CREATE INDEX [IX_Bills_ApprovedBy] ON [dbo].[Bills] ([ApprovedBy]);
END
GO

-- ApprovedAt
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovedAt')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [ApprovedAt] [datetime2](7) NULL;
END
GO

-- ApprovalNotes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovalNotes')
BEGIN
    ALTER TABLE [dbo].[Bills] ADD [ApprovalNotes] [nvarchar](500) NULL;
END
GO

PRINT 'Migration completed! All columns have been added to Bills table.';

