-- ============================================
-- Receivables Management System - Database Migration
-- ============================================
-- This script creates the Receivables and ReceivablePayments tables
-- Run this script to add receivables functionality to the database
-- 
-- IMPORTANT: Make sure you're connected to the correct database before running this script
-- If your database name is different, comment out or modify the USE statement below
-- ============================================

-- Uncomment and modify the database name if needed:
-- USE [YourDatabaseName]
-- GO

-- ============================================
-- 1. Receivables Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Receivables]') AND type in (N'U'))
BEGIN
    -- Check if Users table exists before creating foreign key
    DECLARE @UsersTableExists BIT = 0;
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        SET @UsersTableExists = 1;
    END

    IF @UsersTableExists = 1
    BEGIN
        -- Create table with foreign key constraint
        CREATE TABLE [dbo].[Receivables] (
            [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
            [UserId] NVARCHAR(450) NOT NULL,
            [BorrowerName] NVARCHAR(255) NOT NULL,
            [BorrowerContact] NVARCHAR(500) NULL,
            [Principal] DECIMAL(18,2) NOT NULL,
            [InterestRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
            [Term] INT NOT NULL,
            [Purpose] NVARCHAR(500) NOT NULL DEFAULT '',
            [Status] NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
            [MonthlyPayment] DECIMAL(18,2) NOT NULL,
            [TotalAmount] DECIMAL(18,2) NOT NULL,
            [RemainingBalance] DECIMAL(18,2) NOT NULL,
            [TotalPaid] DECIMAL(18,2) NOT NULL DEFAULT 0,
            [LentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            [CompletedAt] DATETIME2 NULL,
            [AdditionalInfo] NVARCHAR(1000) NULL,
            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT [FK_Receivables_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
        );
    END
    ELSE
    BEGIN
        -- Create table without foreign key constraint (Users table doesn't exist)
        PRINT 'WARNING: Users table not found. Creating Receivables table without foreign key constraint.';
        CREATE TABLE [dbo].[Receivables] (
            [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
            [UserId] NVARCHAR(450) NOT NULL,
            [BorrowerName] NVARCHAR(255) NOT NULL,
            [BorrowerContact] NVARCHAR(500) NULL,
            [Principal] DECIMAL(18,2) NOT NULL,
            [InterestRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
            [Term] INT NOT NULL,
            [Purpose] NVARCHAR(500) NOT NULL DEFAULT '',
            [Status] NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
            [MonthlyPayment] DECIMAL(18,2) NOT NULL,
            [TotalAmount] DECIMAL(18,2) NOT NULL,
            [RemainingBalance] DECIMAL(18,2) NOT NULL,
            [TotalPaid] DECIMAL(18,2) NOT NULL DEFAULT 0,
            [LentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            [CompletedAt] DATETIME2 NULL,
            [AdditionalInfo] NVARCHAR(1000) NULL,
            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        );
        PRINT 'NOTE: You can add the foreign key constraint later using:';
        PRINT 'ALTER TABLE [dbo].[Receivables] ADD CONSTRAINT [FK_Receivables_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;';
    END

    CREATE INDEX [IX_Receivables_UserId] ON [dbo].[Receivables] ([UserId]);
    CREATE INDEX [IX_Receivables_Status] ON [dbo].[Receivables] ([Status]);
    CREATE INDEX [IX_Receivables_BorrowerName] ON [dbo].[Receivables] ([BorrowerName]);
    CREATE INDEX [IX_Receivables_LentAt] ON [dbo].[Receivables] ([LentAt]);
    
    PRINT 'Created Receivables table';
END
ELSE
BEGIN
    PRINT 'Receivables table already exists';
END
GO

-- ============================================
-- 2. ReceivablePayments Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReceivablePayments]') AND type in (N'U'))
BEGIN
    -- Check if required tables exist
    DECLARE @ReceivablesTableExists BIT = 0;
    DECLARE @UsersTableExists2 BIT = 0;
    DECLARE @BankAccountsTableExists BIT = 0;
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Receivables]') AND type in (N'U'))
        SET @ReceivablesTableExists = 1;
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
        SET @UsersTableExists2 = 1;
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND type in (N'U'))
        SET @BankAccountsTableExists = 1;

    IF @ReceivablesTableExists = 0
    BEGIN
        PRINT 'ERROR: Receivables table does not exist. Please create it first.';
    END
    ELSE
    BEGIN
        -- Create table with conditional foreign keys
        DECLARE @Sql NVARCHAR(MAX) = N'
        CREATE TABLE [dbo].[ReceivablePayments] (
            [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
            [ReceivableId] NVARCHAR(450) NOT NULL,
            [UserId] NVARCHAR(450) NOT NULL,
            [BankAccountId] NVARCHAR(450) NULL,
            [Amount] DECIMAL(18,2) NOT NULL,
            [Method] NVARCHAR(20) NOT NULL,
            [Reference] NVARCHAR(50) NOT NULL,
            [Status] NVARCHAR(20) NOT NULL DEFAULT ''COMPLETED'',
            [Description] NVARCHAR(500) NULL,
            [Notes] NVARCHAR(500) NULL,
            [PaymentDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()';

        -- Add foreign key to Receivables (required)
        SET @Sql = @Sql + N',
            CONSTRAINT [FK_ReceivablePayments_Receivables] FOREIGN KEY ([ReceivableId]) REFERENCES [dbo].[Receivables] ([Id]) ON DELETE CASCADE';

        -- Add foreign key to Users if it exists
        IF @UsersTableExists2 = 1
        BEGIN
            SET @Sql = @Sql + N',
            CONSTRAINT [FK_ReceivablePayments_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE';
        END
        ELSE
        BEGIN
            PRINT 'WARNING: Users table not found. Creating ReceivablePayments without Users foreign key.';
        END

        -- Add foreign key to BankAccounts if it exists
        IF @BankAccountsTableExists = 1
        BEGIN
            SET @Sql = @Sql + N',
            CONSTRAINT [FK_ReceivablePayments_BankAccounts] FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id]) ON DELETE NO ACTION';
        END
        ELSE
        BEGIN
            PRINT 'WARNING: BankAccounts table not found. Creating ReceivablePayments without BankAccounts foreign key.';
        END

        SET @Sql = @Sql + N'
        );';
        
        EXEC sp_executesql @Sql;

        CREATE INDEX [IX_ReceivablePayments_ReceivableId] ON [dbo].[ReceivablePayments] ([ReceivableId]);
        CREATE INDEX [IX_ReceivablePayments_UserId] ON [dbo].[ReceivablePayments] ([UserId]);
        CREATE INDEX [IX_ReceivablePayments_PaymentDate] ON [dbo].[ReceivablePayments] ([PaymentDate]);
        CREATE INDEX [IX_ReceivablePayments_Reference] ON [dbo].[ReceivablePayments] ([Reference]);
        
        PRINT 'Created ReceivablePayments table';
    END
END
ELSE
BEGIN
    PRINT 'ReceivablePayments table already exists';
END
GO

PRINT 'Receivables tables migration completed successfully!';
GO

