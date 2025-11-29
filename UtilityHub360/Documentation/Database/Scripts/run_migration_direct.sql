-- =============================================
-- DIRECT SQL MIGRATION - Copy and paste this into your SQL tool
-- Database: DBUTILS
-- =============================================

-- Create BankStatements table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankStatements]') AND type in (N'U'))
BEGIN
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

    ALTER TABLE [dbo].[BankStatements]
        ADD CONSTRAINT [FK_BankStatements_Users_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[BankStatements]
        ADD CONSTRAINT [FK_BankStatements_BankAccounts_BankAccountId] 
        FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id]) ON DELETE CASCADE;
    
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

    ALTER TABLE [dbo].[BankStatementItems]
        ADD CONSTRAINT [FK_BankStatementItems_BankStatements_BankStatementId] 
        FOREIGN KEY ([BankStatementId]) REFERENCES [dbo].[BankStatements] ([Id]) ON DELETE CASCADE;
    
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

    ALTER TABLE [dbo].[Reconciliations]
        ADD CONSTRAINT [FK_Reconciliations_Users_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[Reconciliations]
        ADD CONSTRAINT [FK_Reconciliations_BankAccounts_BankAccountId] 
        FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[Reconciliations]
        ADD CONSTRAINT [FK_Reconciliations_BankStatements_BankStatementId] 
        FOREIGN KEY ([BankStatementId]) REFERENCES [dbo].[BankStatements] ([Id]) ON DELETE SET NULL;
    
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

    ALTER TABLE [dbo].[ReconciliationMatches]
        ADD CONSTRAINT [FK_ReconciliationMatches_Reconciliations_ReconciliationId] 
        FOREIGN KEY ([ReconciliationId]) REFERENCES [dbo].[Reconciliations] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ReconciliationMatches]
        ADD CONSTRAINT [FK_ReconciliationMatches_BankStatementItems_StatementItemId] 
        FOREIGN KEY ([StatementItemId]) REFERENCES [dbo].[BankStatementItems] ([Id]) ON DELETE SET NULL;
    
    PRINT 'ReconciliationMatches table created successfully!';
END
ELSE
BEGIN
    PRINT 'ReconciliationMatches table already exists.';
END
GO

PRINT '=========================================';
PRINT 'All reconciliation tables created successfully!';
PRINT '=========================================';

