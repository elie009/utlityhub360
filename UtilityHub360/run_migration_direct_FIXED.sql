-- =============================================
-- FIXED SQL MIGRATION - Checks for existing tables first
-- Database: DBUTILS
-- =============================================

-- First, check what tables exist
PRINT 'Checking for existing tables...';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('Users', 'BankAccounts') ORDER BY TABLE_NAME;
GO

-- Create BankStatements table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
BEGIN
    PRINT 'Creating BankStatements table...';
    CREATE TABLE [dbo].[BankStatements] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [BankAccountId] NVARCHAR(450) NOT NULL,
        [StatementName] NVARCHAR(255) NOT NULL,
        [StatementStartDate] DATETIME2 NOT NULL,
        [StatementEndDate] DATETIME2 NOT NULL,
        [OpeningBalance] DECIMAL(18,2) NOT NULL,
        [ClosingBalance] DECIMAL(18,2) NOT NULL,
        [ImportFormat] NVARCHAR(50) NULL,
        [ImportSource] NVARCHAR(500) NULL,
        [TotalTransactions] INT NOT NULL,
        [MatchedTransactions] INT NOT NULL DEFAULT 0,
        [UnmatchedTransactions] INT NOT NULL DEFAULT 0,
        [IsReconciled] BIT NOT NULL DEFAULT 0,
        [ReconciledAt] DATETIME2 NULL,
        [ReconciledBy] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_BankStatements] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_BankStatements_UserId] ON [dbo].[BankStatements] ([UserId]);
    CREATE INDEX [IX_BankStatements_BankAccountId] ON [dbo].[BankStatements] ([BankAccountId]);
    CREATE INDEX [IX_BankStatements_StatementStartDate] ON [dbo].[BankStatements] ([StatementStartDate]);
    CREATE INDEX [IX_BankStatements_StatementEndDate] ON [dbo].[BankStatements] ([StatementEndDate]);
    CREATE INDEX [IX_BankStatements_IsReconciled] ON [dbo].[BankStatements] ([IsReconciled]);

    -- Only add foreign keys if referenced tables exist
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[BankStatements]
            ADD CONSTRAINT [FK_BankStatements_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key FK_BankStatements_Users_UserId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Users table not found. Foreign key FK_BankStatements_Users_UserId skipped.';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[BankStatements]
            ADD CONSTRAINT [FK_BankStatements_BankAccounts_BankAccountId] 
            FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key FK_BankStatements_BankAccounts_BankAccountId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: BankAccounts table not found. Foreign key FK_BankStatements_BankAccounts_BankAccountId skipped.';
    END
    
    PRINT 'BankStatements table created successfully!';
END
ELSE
BEGIN
    PRINT 'BankStatements table already exists.';
END
GO

-- Create BankStatementItems table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatementItems]') AND type in (N'U'))
BEGIN
    PRINT 'Creating BankStatementItems table...';
    CREATE TABLE [dbo].[BankStatementItems] (
        [Id] NVARCHAR(450) NOT NULL,
        [BankStatementId] NVARCHAR(450) NOT NULL,
        [TransactionDate] DATETIME2 NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [TransactionType] NVARCHAR(10) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [ReferenceNumber] NVARCHAR(255) NULL,
        [Merchant] NVARCHAR(255) NULL,
        [Category] NVARCHAR(255) NULL,
        [BalanceAfterTransaction] DECIMAL(18,2) NOT NULL,
        [IsMatched] BIT NOT NULL DEFAULT 0,
        [MatchedTransactionId] NVARCHAR(450) NULL,
        [MatchedTransactionType] NVARCHAR(50) NULL,
        [MatchedAt] DATETIME2 NULL,
        [MatchedBy] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_BankStatementItems] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_BankStatementItems_BankStatementId] ON [dbo].[BankStatementItems] ([BankStatementId]);
    CREATE INDEX [IX_BankStatementItems_TransactionDate] ON [dbo].[BankStatementItems] ([TransactionDate]);
    CREATE INDEX [IX_BankStatementItems_IsMatched] ON [dbo].[BankStatementItems] ([IsMatched]);
    CREATE INDEX [IX_BankStatementItems_MatchedTransactionId] ON [dbo].[BankStatementItems] ([MatchedTransactionId]);

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[BankStatementItems]
            ADD CONSTRAINT [FK_BankStatementItems_BankStatements_BankStatementId] 
            FOREIGN KEY ([BankStatementId]) REFERENCES [dbo].[BankStatements] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key FK_BankStatementItems_BankStatements_BankStatementId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: BankStatements table not found. Foreign key skipped.';
    END
    
    PRINT 'BankStatementItems table created successfully!';
END
ELSE
BEGIN
    PRINT 'BankStatementItems table already exists.';
END
GO

-- Create Reconciliations table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reconciliations]') AND type in (N'U'))
BEGIN
    PRINT 'Creating Reconciliations table...';
    CREATE TABLE [dbo].[Reconciliations] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [BankAccountId] NVARCHAR(450) NOT NULL,
        [BankStatementId] NVARCHAR(450) NULL,
        [ReconciliationName] NVARCHAR(255) NOT NULL,
        [ReconciliationDate] DATETIME2 NOT NULL,
        [BookBalance] DECIMAL(18,2) NOT NULL,
        [StatementBalance] DECIMAL(18,2) NOT NULL,
        [Difference] DECIMAL(18,2) NOT NULL,
        [TotalTransactions] INT NOT NULL,
        [MatchedTransactions] INT NOT NULL DEFAULT 0,
        [UnmatchedTransactions] INT NOT NULL DEFAULT 0,
        [PendingTransactions] INT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
        [Notes] NVARCHAR(1000) NULL,
        [CompletedAt] DATETIME2 NULL,
        [CompletedBy] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_Reconciliations] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_Reconciliations_UserId] ON [dbo].[Reconciliations] ([UserId]);
    CREATE INDEX [IX_Reconciliations_BankAccountId] ON [dbo].[Reconciliations] ([BankAccountId]);
    CREATE INDEX [IX_Reconciliations_ReconciliationDate] ON [dbo].[Reconciliations] ([ReconciliationDate]);
    CREATE INDEX [IX_Reconciliations_Status] ON [dbo].[Reconciliations] ([Status]);

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[Reconciliations]
            ADD CONSTRAINT [FK_Reconciliations_Users_UserId] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key FK_Reconciliations_Users_UserId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Users table not found. Foreign key FK_Reconciliations_Users_UserId skipped.';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankAccounts]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[Reconciliations]
            ADD CONSTRAINT [FK_Reconciliations_BankAccounts_BankAccountId] 
            FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key FK_Reconciliations_BankAccounts_BankAccountId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: BankAccounts table not found. Foreign key FK_Reconciliations_BankAccounts_BankAccountId skipped.';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[Reconciliations]
            ADD CONSTRAINT [FK_Reconciliations_BankStatements_BankStatementId] 
            FOREIGN KEY ([BankStatementId]) REFERENCES [dbo].[BankStatements] ([Id]) ON DELETE SET NULL;
        PRINT 'Foreign key FK_Reconciliations_BankStatements_BankStatementId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: BankStatements table not found. Foreign key FK_Reconciliations_BankStatements_BankStatementId skipped.';
    END
    
    PRINT 'Reconciliations table created successfully!';
END
ELSE
BEGIN
    PRINT 'Reconciliations table already exists.';
END
GO

-- Create ReconciliationMatches table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReconciliationMatches]') AND type in (N'U'))
BEGIN
    PRINT 'Creating ReconciliationMatches table...';
    CREATE TABLE [dbo].[ReconciliationMatches] (
        [Id] NVARCHAR(450) NOT NULL,
        [ReconciliationId] NVARCHAR(450) NOT NULL,
        [SystemTransactionId] NVARCHAR(450) NOT NULL,
        [SystemTransactionType] NVARCHAR(50) NOT NULL,
        [StatementItemId] NVARCHAR(450) NULL,
        [MatchType] NVARCHAR(50) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [TransactionDate] DATETIME2 NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [MatchStatus] NVARCHAR(50) NOT NULL DEFAULT 'MATCHED',
        [MatchNotes] NVARCHAR(1000) NULL,
        [AmountDifference] DECIMAL(18,2) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        [MatchedBy] NVARCHAR(450) NULL,
        CONSTRAINT [PK_ReconciliationMatches] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_ReconciliationMatches_ReconciliationId] ON [dbo].[ReconciliationMatches] ([ReconciliationId]);
    CREATE INDEX [IX_ReconciliationMatches_SystemTransactionId] ON [dbo].[ReconciliationMatches] ([SystemTransactionId]);
    CREATE INDEX [IX_ReconciliationMatches_StatementItemId] ON [dbo].[ReconciliationMatches] ([StatementItemId]);
    CREATE INDEX [IX_ReconciliationMatches_MatchStatus] ON [dbo].[ReconciliationMatches] ([MatchStatus]);

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reconciliations]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[ReconciliationMatches]
            ADD CONSTRAINT [FK_ReconciliationMatches_Reconciliations_ReconciliationId] 
            FOREIGN KEY ([ReconciliationId]) REFERENCES [dbo].[Reconciliations] ([Id]) ON DELETE CASCADE;
        PRINT 'Foreign key FK_ReconciliationMatches_Reconciliations_ReconciliationId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Reconciliations table not found. Foreign key skipped.';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatementItems]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[ReconciliationMatches]
            ADD CONSTRAINT [FK_ReconciliationMatches_BankStatementItems_StatementItemId] 
            FOREIGN KEY ([StatementItemId]) REFERENCES [dbo].[BankStatementItems] ([Id]) ON DELETE SET NULL;
        PRINT 'Foreign key FK_ReconciliationMatches_BankStatementItems_StatementItemId created.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: BankStatementItems table not found. Foreign key skipped.';
    END
    
    PRINT 'ReconciliationMatches table created successfully!';
END
ELSE
BEGIN
    PRINT 'ReconciliationMatches table already exists.';
END
GO

PRINT '=========================================';
PRINT 'Migration completed!';
PRINT '=========================================';

-- Verify tables were created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('BankStatements', 'BankStatementItems', 'Reconciliations', 'ReconciliationMatches')
ORDER BY TABLE_NAME;
GO

