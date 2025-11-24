-- Migration: Add Bill Payment Scheduling and Approval Workflow Columns
-- Description: Adds scheduled payment and approval workflow columns to Bills table
-- Date: November 2024
-- This script is safe to run multiple times (idempotent)

USE [YourDatabaseName]; -- Replace with your actual database name
GO

-- ============================================
-- STEP 1: Add Scheduled Payment Columns to Bills Table
-- ============================================
PRINT '';
PRINT 'STEP 1: Adding scheduled payment columns to Bills table...';
PRINT '';

-- Add IsScheduledPayment column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'IsScheduledPayment')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [IsScheduledPayment] [bit] NOT NULL DEFAULT 0;
    PRINT '✓ Added IsScheduledPayment column';
END
ELSE
BEGIN
    PRINT '⚠ IsScheduledPayment column already exists';
END
GO

-- Add ScheduledPaymentBankAccountId column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentBankAccountId')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ScheduledPaymentBankAccountId] [nvarchar](450) NULL;
    PRINT '✓ Added ScheduledPaymentBankAccountId column';
    
    -- Create index
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ScheduledPaymentBankAccountId' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
    BEGIN
        CREATE INDEX [IX_Bills_ScheduledPaymentBankAccountId] ON [dbo].[Bills] ([ScheduledPaymentBankAccountId]);
        PRINT '✓ Created index IX_Bills_ScheduledPaymentBankAccountId';
    END
    
    -- Add foreign key if BankAccounts table exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND type in (N'U'))
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Bills_BankAccounts_ScheduledPaymentBankAccountId')
        BEGIN
            ALTER TABLE [dbo].[Bills]
            ADD CONSTRAINT [FK_Bills_BankAccounts_ScheduledPaymentBankAccountId]
            FOREIGN KEY ([ScheduledPaymentBankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id])
            ON DELETE SET NULL;
            PRINT '✓ Added foreign key FK_Bills_BankAccounts_ScheduledPaymentBankAccountId';
        END
    END
END
ELSE
BEGIN
    PRINT '⚠ ScheduledPaymentBankAccountId column already exists';
END
GO

-- Add ScheduledPaymentDaysBeforeDue column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentDaysBeforeDue')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ScheduledPaymentDaysBeforeDue] [int] NULL;
    PRINT '✓ Added ScheduledPaymentDaysBeforeDue column';
END
ELSE
BEGIN
    PRINT '⚠ ScheduledPaymentDaysBeforeDue column already exists';
END
GO

-- Add LastScheduledPaymentAttempt column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'LastScheduledPaymentAttempt')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [LastScheduledPaymentAttempt] [datetime2](7) NULL;
    PRINT '✓ Added LastScheduledPaymentAttempt column';
END
ELSE
BEGIN
    PRINT '⚠ LastScheduledPaymentAttempt column already exists';
END
GO

-- Add ScheduledPaymentFailureReason column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentFailureReason')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ScheduledPaymentFailureReason] [nvarchar](500) NULL;
    PRINT '✓ Added ScheduledPaymentFailureReason column';
END
ELSE
BEGIN
    PRINT '⚠ ScheduledPaymentFailureReason column already exists';
END
GO

-- ============================================
-- STEP 2: Add Approval Workflow Columns to Bills Table
-- ============================================
PRINT '';
PRINT 'STEP 2: Adding approval workflow columns to Bills table...';
PRINT '';

-- Add ApprovalStatus column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovalStatus')
BEGIN
    -- First add as nullable
    ALTER TABLE [dbo].[Bills]
    ADD [ApprovalStatus] [nvarchar](20) NULL;
    
    -- Set default value for existing rows
    UPDATE [dbo].[Bills]
    SET [ApprovalStatus] = 'APPROVED'
    WHERE [ApprovalStatus] IS NULL;
    
    -- Now make it NOT NULL
    ALTER TABLE [dbo].[Bills]
    ALTER COLUMN [ApprovalStatus] [nvarchar](20) NOT NULL;
    
    -- Add default constraint
    ALTER TABLE [dbo].[Bills]
    ADD CONSTRAINT [DF_Bills_ApprovalStatus] DEFAULT 'APPROVED' FOR [ApprovalStatus];
    
    -- Create index
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ApprovalStatus' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
    BEGIN
        CREATE INDEX [IX_Bills_ApprovalStatus] ON [dbo].[Bills] ([ApprovalStatus]);
        PRINT '✓ Created index IX_Bills_ApprovalStatus';
    END
    
    PRINT '✓ Added ApprovalStatus column';
END
ELSE
BEGIN
    PRINT '⚠ ApprovalStatus column already exists';
END
GO

-- Add ApprovedBy column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovedBy')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ApprovedBy] [nvarchar](450) NULL;
    
    -- Create index
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ApprovedBy' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
    BEGIN
        CREATE INDEX [IX_Bills_ApprovedBy] ON [dbo].[Bills] ([ApprovedBy]);
        PRINT '✓ Created index IX_Bills_ApprovedBy';
    END
    
    PRINT '✓ Added ApprovedBy column';
END
ELSE
BEGIN
    PRINT '⚠ ApprovedBy column already exists';
END
GO

-- Add ApprovedAt column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovedAt')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ApprovedAt] [datetime2](7) NULL;
    PRINT '✓ Added ApprovedAt column';
END
ELSE
BEGIN
    PRINT '⚠ ApprovedAt column already exists';
END
GO

-- Add ApprovalNotes column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovalNotes')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ApprovalNotes] [nvarchar](500) NULL;
    PRINT '✓ Added ApprovalNotes column';
END
ELSE
BEGIN
    PRINT '⚠ ApprovalNotes column already exists';
END
GO

PRINT '';
PRINT '============================================';
PRINT 'Migration completed successfully!';
PRINT 'All bill scheduling and approval workflow columns have been added.';
PRINT '============================================';
GO

