-- Migration: Add Bill Payment Scheduling and Approval Workflow Columns
-- This script checks if the table exists and handles different scenarios
-- Run this in SQL Server Management Studio connected to DBUTILS database

USE DBUTILS;
GO

-- First, check if Bills table exists
DECLARE @TableExists BIT = 0;
DECLARE @TableName NVARCHAR(128) = NULL;
DECLARE @SchemaName NVARCHAR(128) = NULL;

-- Check for Bills table in dbo schema
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SET @TableExists = 1;
    SET @TableName = 'Bills';
    SET @SchemaName = 'dbo';
    PRINT '✓ Found Bills table in dbo schema';
END
-- Check for Bills table in any schema
ELSE IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills')
BEGIN
    SET @TableExists = 1;
    SET @TableName = 'Bills';
    SELECT @SchemaName = SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = 'Bills';
    PRINT '✓ Found Bills table in ' + @SchemaName + ' schema';
END
ELSE
BEGIN
    PRINT '❌ ERROR: Bills table not found!';
    PRINT 'Available tables with "Bill" in name:';
    SELECT SCHEMA_NAME(schema_id) AS SchemaName, name AS TableName 
    FROM sys.tables 
    WHERE name LIKE '%Bill%'
    ORDER BY SchemaName, TableName;
    RETURN; -- Exit if table doesn't exist
END
GO

-- Now add columns using dynamic SQL to handle schema
DECLARE @SQL NVARCHAR(MAX);
DECLARE @TableFullName NVARCHAR(256) = '[dbo].[Bills]'; -- Default to dbo

-- Verify table exists before proceeding
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT '';
    PRINT '============================================';
    PRINT 'Adding columns to Bills table...';
    PRINT '============================================';
    PRINT '';

    -- ============================================
    -- Add Scheduled Payment Columns
    -- ============================================

    -- IsScheduledPayment
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'IsScheduledPayment')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [IsScheduledPayment] [bit] NOT NULL DEFAULT 0;
        PRINT '✓ Added IsScheduledPayment column';
    END
    ELSE
        PRINT '⚠ IsScheduledPayment already exists';

    -- ScheduledPaymentBankAccountId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentBankAccountId')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [ScheduledPaymentBankAccountId] [nvarchar](450) NULL;
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ScheduledPaymentBankAccountId' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
        BEGIN
            CREATE INDEX [IX_Bills_ScheduledPaymentBankAccountId] ON [dbo].[Bills] ([ScheduledPaymentBankAccountId]);
        END
        PRINT '✓ Added ScheduledPaymentBankAccountId column and index';
    END
    ELSE
        PRINT '⚠ ScheduledPaymentBankAccountId already exists';

    -- ScheduledPaymentDaysBeforeDue
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentDaysBeforeDue')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [ScheduledPaymentDaysBeforeDue] [int] NULL;
        PRINT '✓ Added ScheduledPaymentDaysBeforeDue column';
    END
    ELSE
        PRINT '⚠ ScheduledPaymentDaysBeforeDue already exists';

    -- LastScheduledPaymentAttempt
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'LastScheduledPaymentAttempt')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [LastScheduledPaymentAttempt] [datetime2](7) NULL;
        PRINT '✓ Added LastScheduledPaymentAttempt column';
    END
    ELSE
        PRINT '⚠ LastScheduledPaymentAttempt already exists';

    -- ScheduledPaymentFailureReason
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentFailureReason')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [ScheduledPaymentFailureReason] [nvarchar](500) NULL;
        PRINT '✓ Added ScheduledPaymentFailureReason column';
    END
    ELSE
        PRINT '⚠ ScheduledPaymentFailureReason already exists';

    -- ============================================
    -- Add Approval Workflow Columns
    -- ============================================

    -- ApprovalStatus
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovalStatus')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [ApprovalStatus] [nvarchar](20) NULL;
        UPDATE [dbo].[Bills] SET [ApprovalStatus] = 'APPROVED' WHERE [ApprovalStatus] IS NULL;
        ALTER TABLE [dbo].[Bills] ALTER COLUMN [ApprovalStatus] [nvarchar](20) NOT NULL;
        IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Bills_ApprovalStatus')
        BEGIN
            ALTER TABLE [dbo].[Bills] ADD CONSTRAINT [DF_Bills_ApprovalStatus] DEFAULT 'APPROVED' FOR [ApprovalStatus];
        END
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ApprovalStatus' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
        BEGIN
            CREATE INDEX [IX_Bills_ApprovalStatus] ON [dbo].[Bills] ([ApprovalStatus]);
        END
        PRINT '✓ Added ApprovalStatus column with default and index';
    END
    ELSE
        PRINT '⚠ ApprovalStatus already exists';

    -- ApprovedBy
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovedBy')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [ApprovedBy] [nvarchar](450) NULL;
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bills_ApprovedBy' AND object_id = OBJECT_ID(N'[dbo].[Bills]'))
        BEGIN
            CREATE INDEX [IX_Bills_ApprovedBy] ON [dbo].[Bills] ([ApprovedBy]);
        END
        PRINT '✓ Added ApprovedBy column and index';
    END
    ELSE
        PRINT '⚠ ApprovedBy already exists';

    -- ApprovedAt
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovedAt')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [ApprovedAt] [datetime2](7) NULL;
        PRINT '✓ Added ApprovedAt column';
    END
    ELSE
        PRINT '⚠ ApprovedAt already exists';

    -- ApprovalNotes
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovalNotes')
    BEGIN
        ALTER TABLE [dbo].[Bills] ADD [ApprovalNotes] [nvarchar](500) NULL;
        PRINT '✓ Added ApprovalNotes column';
    END
    ELSE
        PRINT '⚠ ApprovalNotes already exists';

    PRINT '';
    PRINT '============================================';
    PRINT 'Migration completed successfully!';
    PRINT 'All columns have been added to Bills table.';
    PRINT '============================================';
END
ELSE
BEGIN
    PRINT '❌ ERROR: Bills table does not exist in dbo schema!';
    PRINT 'Please run add_bill_columns_CHECK_TABLE.sql first to find the correct table name.';
END
GO

