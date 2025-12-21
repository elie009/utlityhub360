-- ============================================
-- Create BankStatementUploads and StagingTransactions Tables
-- ============================================
-- This script creates the missing tables for bank statement upload functionality
-- Run this script if you encounter: "Invalid object name 'BankStatementUploads'"
-- 
-- IMPORTANT: Make sure you're connected to the correct database before running this script
-- ============================================

-- Check if BankStatementUploads table exists, create if not
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatementUploads]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BankStatementUploads] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [BankAccountId] nvarchar(450) NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [OriginalFileName] nvarchar(255) NOT NULL,
        [FileType] nvarchar(50) NOT NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT 'PENDING',
        [ErrorMessage] nvarchar(1000) NULL,
        [ProcessedBankStatementId] nvarchar(450) NULL,
        [RetryCount] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2(7) NOT NULL,
        [ProcessedAt] datetime2(7) NULL,
        [UpdatedAt] datetime2(7) NOT NULL,
        CONSTRAINT [PK_BankStatementUploads] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    -- Create indexes (removed composite index due to key length limit)
    CREATE INDEX [IX_BankStatementUploads_UserId] ON [dbo].[BankStatementUploads] ([UserId]);
    CREATE INDEX [IX_BankStatementUploads_BankAccountId] ON [dbo].[BankStatementUploads] ([BankAccountId]);
    -- Note: Composite index removed - nvarchar(450) * 2 = 1800 bytes exceeds 1700 byte limit
    -- Individual indexes above provide good query performance
    CREATE INDEX [IX_BankStatementUploads_Status] ON [dbo].[BankStatementUploads] ([Status]);
    CREATE INDEX [IX_BankStatementUploads_CreatedAt] ON [dbo].[BankStatementUploads] ([CreatedAt]);
    
    -- Create foreign keys (only if referenced tables exist)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[BankStatementUploads]
            ADD CONSTRAINT [FK_BankStatementUploads_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key to Users table created.';
    END
    ELSE
    BEGIN
        PRINT 'Warning: Users table not found. Foreign key to Users skipped.';
    END
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[BankStatementUploads]
            ADD CONSTRAINT [FK_BankStatementUploads_BankAccounts_BankAccountId] 
            FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key to BankAccounts table created.';
    END
    ELSE
    BEGIN
        PRINT 'Warning: BankAccounts table not found. Foreign key to BankAccounts skipped.';
    END
    
    -- Foreign key to BankStatements (nullable)
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[BankStatementUploads]
            ADD CONSTRAINT [FK_BankStatementUploads_BankStatements_ProcessedBankStatementId] 
            FOREIGN KEY ([ProcessedBankStatementId]) REFERENCES [dbo].[BankStatements] ([Id]) ON DELETE SET NULL;
        PRINT 'Foreign key to BankStatements table created.';
    END
    
    PRINT 'BankStatementUploads table created successfully.';
END
ELSE
BEGIN
    PRINT 'BankStatementUploads table already exists.';
    
    -- Try to add foreign keys if they don't exist
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
        AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BankStatementUploads_Users_UserId')
    BEGIN
        ALTER TABLE [dbo].[BankStatementUploads]
            ADD CONSTRAINT [FK_BankStatementUploads_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key to Users table added.';
    END
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND type in (N'U'))
        AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BankStatementUploads_BankAccounts_BankAccountId')
    BEGIN
        ALTER TABLE [dbo].[BankStatementUploads]
            ADD CONSTRAINT [FK_BankStatementUploads_BankAccounts_BankAccountId] 
            FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key to BankAccounts table added.';
    END
    
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
        AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BankStatementUploads_BankStatements_ProcessedBankStatementId')
    BEGIN
        ALTER TABLE [dbo].[BankStatementUploads]
            ADD CONSTRAINT [FK_BankStatementUploads_BankStatements_ProcessedBankStatementId] 
            FOREIGN KEY ([ProcessedBankStatementId]) REFERENCES [dbo].[BankStatements] ([Id]) ON DELETE SET NULL;
        PRINT 'Foreign key to BankStatements table added.';
    END
END
GO

-- Check if StagingTransactions table exists, create if not
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StagingTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StagingTransactions] (
        [Id] nvarchar(450) NOT NULL,
        [UploadId] nvarchar(450) NOT NULL,
        [TransactionDate] datetime2(7) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [TransactionType] nvarchar(10) NOT NULL,
        [Description] nvarchar(500) NULL,
        [ReferenceNumber] nvarchar(255) NULL,
        [Merchant] nvarchar(255) NULL,
        [Category] nvarchar(255) NULL,
        [BalanceAfterTransaction] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2(7) NOT NULL,
        CONSTRAINT [PK_StagingTransactions] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    -- Create indexes
    CREATE INDEX [IX_StagingTransactions_UploadId] ON [dbo].[StagingTransactions] ([UploadId]);
    CREATE INDEX [IX_StagingTransactions_TransactionDate] ON [dbo].[StagingTransactions] ([TransactionDate]);
    
    -- Create foreign key to BankStatementUploads
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatementUploads]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[StagingTransactions]
            ADD CONSTRAINT [FK_StagingTransactions_BankStatementUploads_UploadId] 
            FOREIGN KEY ([UploadId]) REFERENCES [dbo].[BankStatementUploads] ([Id]) ON DELETE CASCADE;
    END
    
    PRINT 'StagingTransactions table created successfully.';
END
ELSE
BEGIN
    PRINT 'StagingTransactions table already exists.';
END
GO

PRINT 'Script completed. Please verify the tables were created.';
GO

