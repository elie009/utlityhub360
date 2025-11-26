-- Migration: Add Bill Payment Scheduling, Approval Workflow, and Vendor Management
-- Description: Adds scheduled payment, approval workflow, and vendor management features
-- Date: November 2024

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

-- Add ScheduledPaymentBankAccountId column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ScheduledPaymentBankAccountId')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ScheduledPaymentBankAccountId] [nvarchar](450) NULL;
    
    CREATE INDEX [IX_Bills_ScheduledPaymentBankAccountId] ON [dbo].[Bills] ([ScheduledPaymentBankAccountId]);
    
    -- Add foreign key if BankAccounts table exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[Bills]
        ADD CONSTRAINT [FK_Bills_BankAccounts_ScheduledPaymentBankAccountId]
        FOREIGN KEY ([ScheduledPaymentBankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id])
        ON DELETE SET NULL;
    END
    
    PRINT '✓ Added ScheduledPaymentBankAccountId column';
END
ELSE
BEGIN
    PRINT '⚠ ScheduledPaymentBankAccountId column already exists';
END

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

-- ============================================
-- STEP 2: Add Approval Workflow Columns to Bills Table
-- ============================================
PRINT '';
PRINT 'STEP 2: Adding approval workflow columns to Bills table...';
PRINT '';

-- Add ApprovalStatus column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovalStatus')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ApprovalStatus] [nvarchar](20) NOT NULL DEFAULT 'APPROVED';
    
    CREATE INDEX [IX_Bills_ApprovalStatus] ON [dbo].[Bills] ([ApprovalStatus]);
    
    PRINT '✓ Added ApprovalStatus column';
END
ELSE
BEGIN
    PRINT '⚠ ApprovalStatus column already exists';
END

-- Add ApprovedBy column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND name = 'ApprovedBy')
BEGIN
    ALTER TABLE [dbo].[Bills]
    ADD [ApprovedBy] [nvarchar](450) NULL;
    
    CREATE INDEX [IX_Bills_ApprovedBy] ON [dbo].[Bills] ([ApprovedBy]);
    
    PRINT '✓ Added ApprovedBy column';
END
ELSE
BEGIN
    PRINT '⚠ ApprovedBy column already exists';
END

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

-- ============================================
-- STEP 3: Create Vendors Table
-- ============================================
PRINT '';
PRINT 'STEP 3: Creating Vendors table...';
PRINT '';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vendors]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Vendors] (
        [Id] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [ContactPerson] [nvarchar](100) NULL,
        [Email] [nvarchar](100) NULL,
        [Phone] [nvarchar](20) NULL,
        [Address] [nvarchar](500) NULL,
        [Website] [nvarchar](100) NULL,
        [Category] [nvarchar](50) NULL,
        [AccountNumber] [nvarchar](100) NULL,
        [Notes] [nvarchar](500) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
    );
    
    -- Create indexes
    CREATE INDEX [IX_Vendors_UserId] ON [dbo].[Vendors] ([UserId]);
    CREATE INDEX [IX_Vendors_Name] ON [dbo].[Vendors] ([Name]);
    CREATE INDEX [IX_Vendors_Category] ON [dbo].[Vendors] ([Category]);
    CREATE INDEX [IX_Vendors_IsActive] ON [dbo].[Vendors] ([IsActive]);
    
    -- Foreign key to Users
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[Vendors]
        ADD CONSTRAINT [FK_Vendors_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE CASCADE;
    END
    
    PRINT '✓ Created Vendors table with indexes and foreign keys';
END
ELSE
BEGIN
    PRINT '⚠ Vendors table already exists';
END

GO

PRINT '';
PRINT 'Migration completed successfully!';
PRINT 'All bill scheduling, approval workflow, and vendor management features are now available.';
GO

