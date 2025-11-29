-- Migration script to add BillId and SavingsAccountId columns to JournalEntry table
-- This enables journal entries for bills and savings accounts

-- Check if JournalEntry table exists, if not create it first
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND type in (N'U'))
BEGIN
    -- Create JournalEntry table if it doesn't exist
    CREATE TABLE [dbo].[JournalEntries] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [LoanId] NVARCHAR(450) NULL,
        [EntryType] NVARCHAR(50) NOT NULL,
        [EntryDate] DATETIME2 NOT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [Reference] NVARCHAR(100) NULL,
        [TotalDebit] DECIMAL(18,2) NOT NULL,
        [TotalCredit] DECIMAL(18,2) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id])
    );
    
    -- Create indexes
    CREATE INDEX [IX_JournalEntries_UserId] ON [dbo].[JournalEntries] ([UserId]);
    CREATE INDEX [IX_JournalEntries_LoanId] ON [dbo].[JournalEntries] ([LoanId]);
    CREATE INDEX [IX_JournalEntries_EntryType] ON [dbo].[JournalEntries] ([EntryType]);
    CREATE INDEX [IX_JournalEntries_EntryDate] ON [dbo].[JournalEntries] ([EntryDate]);
END

-- Add BillId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND name = 'BillId')
BEGIN
    ALTER TABLE [dbo].[JournalEntries]
    ADD [BillId] NVARCHAR(450) NULL;
    
    CREATE INDEX [IX_JournalEntries_BillId] ON [dbo].[JournalEntries] ([BillId]);
    
    -- Add foreign key constraint if Bills table exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bills]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[JournalEntries]
        ADD CONSTRAINT [FK_JournalEntries_Bills_BillId]
        FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills] ([Id])
        ON DELETE SET NULL;
    END
END

-- Add SavingsAccountId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND name = 'SavingsAccountId')
BEGIN
    ALTER TABLE [dbo].[JournalEntries]
    ADD [SavingsAccountId] NVARCHAR(450) NULL;
    
    CREATE INDEX [IX_JournalEntries_SavingsAccountId] ON [dbo].[JournalEntries] ([SavingsAccountId]);
    
    -- Add foreign key constraint if SavingsAccounts table exists
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavingsAccounts]') AND type in (N'U'))
    BEGIN
        ALTER TABLE [dbo].[JournalEntries]
        ADD CONSTRAINT [FK_JournalEntries_SavingsAccounts_SavingsAccountId]
        FOREIGN KEY ([SavingsAccountId]) REFERENCES [dbo].[SavingsAccounts] ([Id])
        ON DELETE SET NULL;
    END
END

-- Check if JournalEntryLines table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntryLines]') AND type in (N'U'))
BEGIN
    -- Create JournalEntryLine table if it doesn't exist
    CREATE TABLE [dbo].[JournalEntryLines] (
        [Id] NVARCHAR(450) NOT NULL,
        [JournalEntryId] NVARCHAR(450) NOT NULL,
        [AccountName] NVARCHAR(100) NOT NULL,
        [AccountType] NVARCHAR(50) NOT NULL,
        [EntrySide] NVARCHAR(10) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JournalEntryLines_JournalEntries_JournalEntryId]
        FOREIGN KEY ([JournalEntryId]) REFERENCES [dbo].[JournalEntries] ([Id])
        ON DELETE CASCADE
    );
    
    -- Create indexes
    CREATE INDEX [IX_JournalEntryLines_JournalEntryId] ON [dbo].[JournalEntryLines] ([JournalEntryId]);
    CREATE INDEX [IX_JournalEntryLines_AccountName] ON [dbo].[JournalEntryLines] ([AccountName]);
    CREATE INDEX [IX_JournalEntryLines_AccountType] ON [dbo].[JournalEntryLines] ([AccountType]);
END

PRINT 'Migration completed successfully. JournalEntry table now supports Bills and SavingsAccounts.';





